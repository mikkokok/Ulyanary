using System;

namespace Ulyanary
{
    class Program
    {
        static void Main(string[] args)
        {
            var apploader = AppLoader.Instance;
            AppLoader.LoadConfig();
            Console.WriteLine("Config loaded");
        }
    }
}
