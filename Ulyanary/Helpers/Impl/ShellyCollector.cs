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
        private bool retry = true;

        public ShellyCollector(ConfigData config)
        {
            _config = config;
            _falconConsumer = new FalconConsumer(config);
            _lastHour = DateTime.Now.Hour;
            _httpClient = new HttpClient()
            {
                Timeout = new TimeSpan(0, 0, 30)
            };
        }

        public async Task StartCollectors(CancellationToken token)
        {
            await Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    if (CalculateExactHour())
                    {
                        await Shelly3EMCollector();
                        await Shelly1PMCollector();
                        await ShellyPro3EMCollector();
                        await ShellyPlus1PMCollector();
                    }
                    await Task.Delay(30 * 1000);

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

        private async Task Shelly1PMCollector()
        {
            foreach (Device device in _config.ShellyDevices.Where(m => m.Model.Equals("Shelly1PM")))
            {
                try
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

                        double consumed = CalcHelpers.ConvertWminTokWh(total, device.CounterValue);
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
                catch (Exception ex)
                {
                    device.InitialPoll = true;
                    Console.WriteLine($"Shelly device {device.Name} failed, errormessage {ex.Message}, continuing");
                }

            }
        }
        private async Task Shelly3EMCollector()
        {
            foreach (Device device in _config.ShellyDevices.Where(m => m.Model.Equals("Shelly3EM")))
            {
                try
                {
                    var response = await QueryShellyDevice($"http://{device.IP}/status/");
                    double total = 0;
                    bool parseResult = false;
                    for (int i = 0; i <= 2; i++)
                    {
                        parseResult = double.TryParse(response["emeters"][i]["total_returned"].ToString(), out double tempTotal);
                        total += tempTotal;
                    }
                    if (parseResult)
                    {
                        if (total == 0 || total < device.CounterValue || device.CounterValue == 0)
                        {
                            Console.WriteLine($"Something funky with {device.Name}, {device.IP}, total: {total}, CounterValue {device.CounterValue} ");
                            device.CounterValue = total;
                            device.InitialPoll = false;
                            continue;
                        }
                        double consumed = CalcHelpers.CalculateWHTokWh(total, device.CounterValue);
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
                catch (Exception ex)
                {
                    device.InitialPoll = true;
                    Console.WriteLine($"Shelly device {device.Name} failed, errormessage {ex.Message}, continuing");
                }
            }
        }
        private async Task ShellyPro3EMCollector()
        {
            foreach (Device device in _config.ShellyDevices.Where(m => m.Model.Equals("ShellyPro3EM")))
            {
                try
                {
                    var response = await QueryShellyDevice($"http://{device.IP}/rpc/EMdata.GetStatus?id=0");
                    var parseResult = double.TryParse(response["total_act_ret"].ToString(), out double total);
                    if (parseResult)
                    {
                        if (total == 0 || total < device.CounterValue || device.CounterValue == 0)
                        {
                            Console.WriteLine($"Something funky with {device.Name}");
                            device.CounterValue = total;
                            device.InitialPoll = false;
                            continue;
                        }

                        double consumed = CalcHelpers.CalculateWHTokWh(total, device.CounterValue);
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
                catch (Exception ex)
                {
                    device.InitialPoll = true;
                    Console.WriteLine($"Shelly device {device.Name} failed, errormessage {ex.Message}, continuing");
                }

            }
        }

        private async Task ShellyPlus1PMCollector()
        {
            foreach (Device device in _config.ShellyDevices.Where(m => m.Model.Equals("ShellyPlus1PM")))
            {
                try
                {
                    var response = await QueryShellyDevice($"http://{device.IP}/rpc/Switch.GetStatus?id=0");
                    var parseResult = double.TryParse(response["aenergy"]["total"].ToString(), out double total);
                    if (parseResult)
                    {
                        if (total == 0 || total < device.CounterValue || device.CounterValue == 0)
                        {
                            Console.WriteLine($"Something funky with {device.Name}");
                            device.CounterValue = total;
                            device.InitialPoll = false;
                            continue;
                        }

                        double consumed = CalcHelpers.ConvertWminTokWh(total, device.CounterValue);
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
                catch (Exception ex)
                {
                    device.InitialPoll = true;
                    Console.WriteLine($"Shelly device {device.Name} failed, errormessage {ex.Message}, continuing");
                }

            }
        }
        private async Task<JsonObject> QueryShellyDevice(string url)
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
                Console.WriteLine($"Polling Shelly device failed from url {url}, errormessage {ex.Message}, failing");
                throw;
            }
        }
        private void InvokeFalcon(SensorData sensorData)
        {
            Task.Run(async () => await _falconConsumer.SendSensorData(sensorData));
        }
    }
}
