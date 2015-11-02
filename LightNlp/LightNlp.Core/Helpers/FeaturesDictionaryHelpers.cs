using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightNlp.Core.Helpers
{
    public static class FeaturesDictionaryHelpers
    {
        public static void IncreaseFeatureFrequency(this Dictionary<string, double> features, string feature, double value)
        {
            if (features.ContainsKey(feature))
            {
                features[feature] += value;
            }
            else
            {
                features.Add(feature, value);
            }
        }

        public static void SetFeatureValue(this Dictionary<string, double> features, string feature, double value)
        {
            if (features.ContainsKey(feature))
            {
                features[feature] = value;
            }
            else
            {
                features.Add(feature, value);
            }
        }
    }
}
