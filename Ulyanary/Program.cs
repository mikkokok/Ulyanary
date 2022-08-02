using System;
using Ulyanary.Helpers.Impl;

namespace Ulyanary
{
    class Program
    {
        static void Main(string[] args)
        {
            _ = AppLoader.Instance;
            AppLoader.LoadConfig();
            Console.WriteLine("Config loaded");
            _ = new OumanCollector(AppLoader.LoadedConfig);
            Console.WriteLine("Ouman polling started");
            _ = new ShellyCollector(AppLoader.LoadedConfig);
            Console.WriteLine("Shelly polling started");
        }
    }
}
