using de.bwaldvogel.liblinear;
using LibSvmHelper;
using LibSvmHelper.Helpers;
using LightNlp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightNlpWebApiSelfHost
{
    public class ProcessingHelpers
    {
        public static FeatureNode[] ConvertToSortedFeatureNodeArray(Dictionary<int, double> itemFeatures)
        {
            var featuresDictSorted = itemFeatures.Select(kv => kv).OrderBy(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value);
            List<FeatureNode> featureNodes = new List<FeatureNode>();
            foreach (var item in featuresDictSorted)
            {
                featureNodes.Add(new FeatureNode(item.Key, item.Value));
            }

            var featureNodesArray = featureNodes.ToArray();
            return featureNodesArray;
        }

        public static SparseItemInt ProcessTextAndGetSparseItem(FeatureExtractionPipeline pipeline, FeatureStatisticsDictionaryBuilder featureStatisticsDictBuilder, int minFeaturesFrequency, bool normalize, ScaleRange scaleRange, string docContent, int classLabelIndex)
        {
            Dictionary<string, double> docFeatures = new Dictionary<string, double>();
            pipeline.ProcessDocument(docContent, docFeatures);
            //Append extracted features

            //A - Extracted indexed features
            var itemIndexedFeatures = LibSvmFileBuilder.GetIndexedFeaturesFromStringFeatures(docFeatures, featureStatisticsDictBuilder.FeatureInfoStatistics, minFeaturesFrequency, normalize, scaleRange);
            var sparseItem = new SparseItemInt() { Label = classLabelIndex, Features = itemIndexedFeatures };
            return sparseItem;
        }
    }
}
