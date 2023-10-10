using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Ulyanary.Config;
using Ulyanary.DTO;

namespace Ulyanary.Helpers.Impl
{
    internal class FroniusCollector
    {
        private readonly ConfigData _config;
        private readonly FalconConsumer _falconConsumer;
        private int _lastHour;
        private DateTime _todaysDate;
        private readonly HttpClient _httpClient;
        private bool retry = true;

        public FroniusCollector(ConfigData config)
        {
            _config = config;
            _falconConsumer = new FalconConsumer(config);
            _lastHour = DateTime.Now.Hour;
            _todaysDate = DateTime.Today.Date;
            _httpClient = new HttpClient()
            {
                Timeout = new TimeSpan(0, 0, 30)
            };
        }

        public async Task StartCollectors(CancellationToken token)
        {
            await Task.Run(async () =>
            {
                {
                    while (!token.IsCancellationRequested)
                    {
                        if (CalculateExactHour())
                        {
                            await FroniusSymoCollector();
                        }
                        await Task.Delay(TimeSpan.FromSeconds(30));
                    }
                }
            }, token);

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

        private async Task FroniusSymoCollector()
        {
            foreach (Device device in _config.FroniusDevices.Where(m => m.Model.Contains("FroniusSymo")))
            {
                if (HasDayChanged())
                {
                    device.CounterValue = 0;
                }
                try
                {
                    var response = await QueryFroniusDevice($"http://{device.IP}/solar_api/v1/GetPowerFlowRealtimeData.fcgi");
                    var parseResult = double.TryParse(response["Body"]["Data"]["Site"]["E_Day"].ToString(), out double total);
                    if (parseResult)
                    {
                        double yield = CalcHelpers.CalculateHourlyYield(total, device.CounterValue);
                        if (yield > 0)
                        {
                            InvokeFalcon(new SensorData
                            {
                                SensorName = device.Name,
                                PowerYield = yield,
                                Time = DateTime.Now.AddHours(-1).ToString("yyyy-MM-dd HH:mm:ss").Replace(".", ":")

                            });
                        }
                        device.CounterValue = total;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fronius device {device.Name} failed, errormessage {ex.Message}, continuing");
                }
            }
        }


        private async Task<JsonObject> QueryFroniusDevice(string url)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonNode.Parse(responseContent).AsObject();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Polling Fronius device failed from url {url}, errormessage {ex.Message}, retrying");
                if (retry) {
                    retry = false;
                    await Task.Delay(TimeSpan.FromMinutes(5));
                    await QueryFroniusDevice(url);
                    retry = true;
                }

                Console.WriteLine($"Polling Fronius device failed from url {url}, errormessage {ex.Message}, failing");
            }
            throw new Exception($"Polling Fronius device failed from url {url}, failing");

        }
        private void InvokeFalcon(SensorData sensorData)
        {
            _ = Task.Run(async () => await _falconConsumer.SendSensorData(sensorData));
        }

        private bool HasDayChanged()
        {
            if (_todaysDate != DateTime.Today.Date)
            {
                _todaysDate = DateTime.Today.Date;
                return true;
            }
            return false;
        }
    }
}
