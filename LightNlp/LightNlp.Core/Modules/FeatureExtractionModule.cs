using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightNlp.Core.Modules
{
    public abstract class FeatureExtractionModule : IFeatureExtractionModule
    {
        public FeatureExtractionModule(string moduleName)
        {
            if (string.IsNullOrWhiteSpace(moduleName))
            {
                throw new ArgumentNullException(moduleName);
            }
            this.Name = moduleName;
        }
        public string Name { get; private set; }
        public abstract void ExtractTextFeatures(string textContent, Dictionary<string, double> item, List<Annotation> annotations);
        
    }
}
