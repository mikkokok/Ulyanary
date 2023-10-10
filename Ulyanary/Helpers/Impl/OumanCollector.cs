using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Ulyanary.Config;

namespace Ulyanary.Helpers.Impl
{
    internal class OumanCollector
    {
        private readonly string _polledUrl;
        private Timer _timer;
        private FalconConsumer _falconConsumer;
        private HttpClient _httpClient;

        public OumanCollector(ConfigData config)
        {
            _polledUrl = config.Ouman.url;
            _falconConsumer = new FalconConsumer(config);
            _httpClient = new HttpClient()
            {
                Timeout = new TimeSpan(0, 0, 30)
            };
        }

        public void StartPolling()
        {
            Console.WriteLine("Resuming polling");
            _timer = new Timer(async _ => await GetAsync(_polledUrl), null, TimeSpan.Zero, TimeSpan.FromMinutes(15));
        }
        public async Task StopPolling()
        {
            Console.WriteLine("Stopping polling");
            _timer.Dispose();
            Console.WriteLine("Wait 15 minutes before resuming polling");
            await Task.Delay(900000);
            StartPolling();
        }

        public async Task GetAsync(string url)
        {
            try
            {
                var result = await DoRequestAsync(url);
                Console.WriteLine($"Result from Ouman {result}");
                string[] split = result.Split('?')[1].Split(';');
                foreach (var splitted in split)
                {
                    InvokeFalcon(splitted);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await StopPolling();
            }
        }
        private async Task<string> DoRequestAsync(string url)
        {
            var response = await _httpClient.GetAsync($"{url}request?S_261_85;S_272_85;S_227_85;S_259_85");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
        private void InvokeFalcon(string setwithcode)
        {
            if (string.IsNullOrEmpty(setwithcode) || !setwithcode.Contains("=")) return;
            var code = setwithcode.Split('=')[0];
            var result = setwithcode.Split('=')[1];
            string translation;
            if (code.Equals("S_227_85"))
            {
                translation = "Ulkolämpötila";
                double.TryParse(result, out var doubleValue);
                _ = Task.Run(async () =>
                  {
                      await _falconConsumer.SendSensorData(new DTO.SensorData
                      {
                          SensorName = "OumanSensorOutside",
                          Temperature = doubleValue
                      });
                  });
            }
            else if (code.Equals("S_261_85"))
            {
                translation = "Mitattu huonelämpötila";
                _ = double.TryParse(result, out var doubleValue);
                _ = Task.Run(async () =>
                {
                    await _falconConsumer.SendSensorData(new DTO.SensorData
                    {
                        SensorName = "OumanSensorInside",
                        Temperature = doubleValue
                    });
                });
            }
            else if (code.Equals("S_272_85"))
            {
                translation = "Venttiilin asento";
                double.TryParse(result, out var doubleValue);
                _ = Task.Run(async () =>
                {
                    await _falconConsumer.SendSensorData(new DTO.SensorData
                    {
                        SensorName = "OumanValve",
                        ValvePosition = doubleValue
                    });
                });
            }
            else if (code.Equals("S_259_85"))
            {
                translation = "Mitattu menoveden lämpötila";
                double.TryParse(result, out var doubleValue);
                _ = Task.Run(async () =>
                {
                    await _falconConsumer.SendSensorData(new DTO.SensorData
                    {
                        SensorName = "RadiatorNetworkIn",
                        Temperature = doubleValue
                    });
                });

            }
            else
            {
                translation = code;
            }
        }
    }
}
