﻿namespace NebulaSiege.Utility.ExtensionMethods
{
    internal static class NsFloatExtensions
    {
        /// <summary>
        /// Clips a value to a min/max value.
        /// </summary>
        public static float Box(this float value, float minValue, float maxValue)
        {
            if (value > maxValue) return maxValue;
            else if (value < minValue) return minValue;
            else return value;
        }
    }
}