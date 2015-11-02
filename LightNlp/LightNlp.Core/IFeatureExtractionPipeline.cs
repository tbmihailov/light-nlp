using LightNlp.Core.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightNlp.Core
{
    interface IFeatureExtractionPipeline 
    {
        void RegisterModule(FeatureExtractionModule module);

        void UnregisterModule(FeatureExtractionModule module);

        void ProcessDocument(string textContent, Dictionary<string, double> item);
    }
}
