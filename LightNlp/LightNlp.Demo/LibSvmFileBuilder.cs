using LibSvmHelper.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LightNlp.Demo
{
    /// <summary>
    /// Libsvm input file builder
    /// </summary>
    public class LibSvmFileBuilder
    {
        TextWriter _textWriter;

        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="textWriter">Text writer to append data to</param>
        public LibSvmFileBuilder(TextWriter textWriter)
        {
            if (textWriter == null)
            {
                throw new ArgumentNullException("stream");
            }

            _textWriter = textWriter;
        }

        /// <summary>
        /// Preprocess and append item string features to a libsvm
        /// </summary>
        /// <param name="classLabel">Class label of the item</param>
        /// <param name="docFeatures">String based features</param>
        /// <param name="featuresInforDictionary">Features info dictionary</param>
        /// <param name="minFeatureFreq">Minimum frequence of features to include</param>
        /// <param name="normalize">Should features be normalized</param>
        /// <param name="scaleRange">Scale range [-1 to 1] or [0 to 1]</param>
        public void PreprocessStringFeaturesAndAppendItem(int classLabel, Dictionary<string, double> docFeatures, Dictionary<string, LibSvmHelper.Helpers.FeatureInfo> featuresInforDictionary, int minFeatureFreq, bool normalize = true, ScaleRange scaleRange = ScaleRange.MinusOneToOne)
        {
            Dictionary<int, double> itemFeatures = GetIndexedFeaturesFromStringFeatures(docFeatures, featuresInforDictionary, minFeatureFreq, normalize, scaleRange);

            //write to output
            itemFeatures = AppendItem(classLabel, itemFeatures);
        }

        /// <summary>
        /// Appends item indexed features to libsvm file
        /// </summary>
        /// <param name="classLabel">Class label</param>
        /// <param name="itemFeatures">Features</param>
        /// <returns></returns>
        public Dictionary<int, double> AppendItem(int classLabel, Dictionary<int, double> itemFeatures)
        {
            var writer = _textWriter;
            writer.Write(classLabel);
            itemFeatures = itemFeatures.OrderBy(kv => kv.Key).ToDictionary(a => a.Key, a => a.Value);
            foreach (var itemFeature in itemFeatures)
            {
                writer.Write(string.Format(" {0}:{1:0.000000}", itemFeature.Key, itemFeature.Value));
            }
            writer.WriteLine("");
            return itemFeatures;
        }

        /// <summary>
        /// Converts string features to indexed features. Liblinea/libsvm accepted indexed features only (no strng names)
        /// </summary>
        /// <param name="docFeatures">String based features</param>
        /// <param name="featuresInforDictionary">Features info dictionary</param>
        /// <param name="minFeatureFreq">Minimum frequence of features to include</param>
        /// <param name="normalize">Should features be normalized</param>
        /// <param name="scaleRange">Scale range [-1 to 1] or [0 to 1]</param>
        /// <returns></returns>
        public static Dictionary<int, double> GetIndexedFeaturesFromStringFeatures(Dictionary<string, double> docFeatures, Dictionary<string, LibSvmHelper.Helpers.FeatureInfo> featuresInforDictionary, int minFeatureFreq, bool normalize, ScaleRange scaleRange)
        {
            Dictionary<int, double> itemFeatures = new Dictionary<int, double>();
            foreach (var feature in docFeatures)
            {
                if (!featuresInforDictionary.ContainsKey(feature.Key))
                {
                    continue;
                }

                if (featuresInforDictionary[feature.Key].DocsFrequency < minFeatureFreq)
                {
                    continue;
                }

                int featureIndex = featuresInforDictionary[feature.Key].Index;
                double featureValue = feature.Value;
                if (normalize)
                {
                    double minMaxDiff = featuresInforDictionary[feature.Key].MaxValue - featuresInforDictionary[feature.Key].MinValue;
                    double currValueToMin = Math.Max(featureValue, featuresInforDictionary[feature.Key].MinValue) - featuresInforDictionary[feature.Key].MinValue;

                    switch (scaleRange)
                    {
                        case ScaleRange.MinusOneToOne:
                            featureValue = 2 * ((currValueToMin + 0.001) / (minMaxDiff + 0.001)) - 1;
                            break;
                        case ScaleRange.ZeroToOne:
                            featureValue = (currValueToMin + 0.001) / (minMaxDiff + 0.001);
                            break;
                        default:
                            break;
                    }

                }

                if (Math.Abs(featureValue) > 0.001)
                {
                    itemFeatures.Add(featureIndex, featureValue);
                }

            }
            return itemFeatures;
        }


    }
}
