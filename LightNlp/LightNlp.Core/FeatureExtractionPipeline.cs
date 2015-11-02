using LibSvmHelper;
using LightNlp.Core.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightNlp.Core
{
    public class FeatureExtractionPipeline
    {
        public FeatureExtractionPipeline()
        {
            Modules = new HashSet<FeatureExtractionModule>();
        }
        private HashSet<Modules.FeatureExtractionModule> Modules { get; set; }

        public void RegisterModule(Modules.FeatureExtractionModule module)
        {
            if (module == null)
            {
                throw new ArgumentNullException("module");
            }

            Modules.Add(module);
        }

        public void UnregisterModule(FeatureExtractionModule module)
        {
            if (module == null)
            {
                throw new ArgumentNullException("module");
            }


            if (!Modules.Contains(module))
            {
                throw new ArgumentException("Module not found in this pipeline");
            }

            Modules.Remove(module);
        }

        public void ProcessDocument(string textContent, Dictionary<string, double> features)
        {
            var annotations = new List<Annotation>();
            foreach (var module in Modules)
            {
                module.ExtractTextFeatures(textContent, features, annotations);
            }
        }
    }
}
