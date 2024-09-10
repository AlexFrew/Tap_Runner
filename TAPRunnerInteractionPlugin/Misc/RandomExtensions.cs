using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAPRunnerInteractionPlugin.Misc
{
    public static class RandomExtensions
    {
        private static readonly Random random = new Random();

        public static double NextDouble(double minValue, double maxValue)
        {
            // Ensure the range is valid
            if (minValue > maxValue)
            {
                throw new ArgumentException("minValue must be less than or equal to maxValue");
            }

            // Generate a random double between 0.0 and 1.0, then scale to the desired range
            return minValue + (random.NextDouble() * (maxValue - minValue));
        }
    }
}