using LibSvmHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace LightNlp.Core.Modules
{
    public interface IFeatureExtractionModule
    {
        void ExtractTextFeatures(string textContent, Dictionary<string, double> item, List<Annotation> annotations);
    }
}
