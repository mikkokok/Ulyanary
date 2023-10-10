using System;
using System.Threading;
using System.Threading.Tasks;
using Ulyanary.Config;
using Ulyanary.Helpers.Impl;

namespace Ulyanary
{
    class Program
    {
        static void Main(string[] args)
        {
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            var configLoader = new ConfigLoader();
            configLoader.LoadConfig();
            Console.WriteLine("Config loaded");
            var oumanCollector = new OumanCollector(configLoader.LoadedConfig);
            oumanCollector.StartPolling();
            Console.WriteLine("Ouman polling started");
            var shellyCollectorTask = new ShellyCollector(configLoader.LoadedConfig).StartCollectors(token);
            Console.WriteLine("Shelly polling started");
            var froniusCollectorTask = new FroniusCollector(configLoader.LoadedConfig).StartCollectors(token);
            Console.WriteLine("Fronius polling started");
            Task.WaitAll(shellyCollectorTask, froniusCollectorTask);
            Console.WriteLine("We are ready");
        }
    }
}
