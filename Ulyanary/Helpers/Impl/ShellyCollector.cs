using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ulyanary.Config;
using Ulyanary.DTO;
using System.Text.Json.Nodes;

namespace Ulyanary.Helpers.Impl
{
    class ShellyCollector
    {

        private readonly ConfigData _config;
        private readonly FalconConsumer _falconConsumer;
        private int _lastHour;
        private bool _polling = false;
        private readonly HttpClient _httpClient;

        public ShellyCollector(ConfigData config)
        {
            _config = config;
            _falconConsumer = new FalconConsumer(config);
            _lastHour = DateTime.Now.Hour;
            _httpClient = new HttpClient()
            {
                Timeout = new TimeSpan(0, 0, 30)
            };
            _ = StartCollectors();
        }

        private async Task StartCollectors()
        {
            _polling = true;
            while (_polling)
            {
                if(CalculateExactHour())
                {
                    await Shelly3EMCollector();
                    await Shelly1PMCollector();
                }
                Thread.Sleep(TimeSpan.FromSeconds(30));
            }
            _polling = false;
        }
        private bool CalculateExactHour()
        {
            if (_lastHour < DateTime.Now.Hour || (_lastHour == 23 && DateTime.Now.Hour == 0))
            {
                _lastHour = DateTime.Now.Hour;
                return true;
            }
            return false;
        }

        private async Task Shelly1PMCollector()
        {
            foreach (Device device in _config.ShellyDevices.Where(m => m.Model.Contains("1PM")))
            {
                var response = await QueryShellyDevice($"http://{device.IP}/status/");
                var parseResult = double.TryParse(response["meters"][0]["total"].ToString(), out double total);
                if (parseResult)
                {
                    if (total == 0 || total < device.CounterValue || device.CounterValue == 0)
                    {
                        Console.WriteLine($"Something funky with {device.Name}");
                        device.CounterValue = total;
                        device.InitialPoll = false;
                        continue;
                    }

                    double consumed = ConvertWminTokWh(total, device.CounterValue);
                    if (!device.InitialPoll)
                    {
                        InvokeFalcon(new SensorData
                        {
                            SensorName = device.Name,
                            UsedPower = consumed
                        });
                    }
                    device.InitialPoll = false;
                    device.CounterValue = total;
                }
            }
        }
        private async Task Shelly3EMCollector()
        {
            foreach (Device device in _config.ShellyDevices.Where(m => m.Model.Contains("3EM")))
            {
                var response = await QueryShellyDevice($"http://{device.IP}/status/");
                double total = 0;
                bool parseResult = false;
                for (int i = 0; i <= 2; i++)
                {
                    var testi = response["emeters"][i];
                    var testiB = response["emeters"][i]["total_returned"];
                    parseResult = double.TryParse(response["emeters"][i]["total_returned"].ToString(), out double tempTotal);
                    total += tempTotal;
                }
                if (parseResult)
                {
                    if (total == 0 || total < device.CounterValue || device.CounterValue == 0)
                    {
                        Console.WriteLine($"Something funky with {device.Name}");
                        device.CounterValue = total;
                        device.InitialPoll = false;
                        continue;
                    }
                    double consumed = CalculateWHTokWh(total, device.CounterValue);
                    InvokeFalcon(new SensorData
                    {
                        SensorName = device.Name,
                        UsedPower = consumed
                    });

                    device.CounterValue = total;
                }
            }
        }
        private async Task<JsonObject> QueryShellyDevice(string url)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonNode.Parse(responseContent).AsObject();
        }
        private void InvokeFalcon(SensorData sensorData)
        {
            _ = Task.Run(async () => await _falconConsumer.SendSensorData(sensorData));
        }

        private static double ConvertWminTokWh(double total, double previous)
        {
            var consumed = total - previous;
            return consumed * 0.000016666;
        }
        private static double CalculateWHTokWh(double total, double previous )
        {
            var consumed = total - previous;
            return consumed / 1000;
        }
    }
}
