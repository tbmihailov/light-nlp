using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LibSvmHelper;
using LightNlp.Tools.Helpers;
using LightNlp.Core.Modules;

namespace LightNlp.Tools.Modules
{
    public class CustomPunctuationCountFeatureExtractionModule : FeatureExtractionModule
    {
        public CustomPunctuationCountFeatureExtractionModule(string moduleName):base(moduleName)
        {

        }        

        public override void ExtractTextFeatures(string textContent, Dictionary<string, double> item, List<Annotation> annotations)
        {
            FeatureExtractionNlpHelpers.ExtractTextPunctuationFeaturesAndUpdateItemFeatures(textContent, item);
        }
    }
}
