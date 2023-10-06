
namespace Ulyanary.Helpers.Impl
{
    internal static class CalcHelpers
    {
        public static double ConvertWminTokWh(double total, double previous)
        {
            var consumed = total - previous;
            return consumed * 0.000016666;
        }
        public static double CalculateWHTokWh(double total, double previous)
        {
            var consumed = total - previous;
            return consumed / 1000;
        }

        public static double CalculateHourlyYield(double total, double previous)
        {
            return total - previous;
        }
    }
}
