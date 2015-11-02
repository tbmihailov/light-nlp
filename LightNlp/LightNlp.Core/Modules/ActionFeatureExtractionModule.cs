using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibSvmHelper;
using LightNlp.Core.Modules;

namespace LightNlp.Core.Modules
{
    public class ActionFeatureExtractionModule : FeatureExtractionModule
    {
        public ActionFeatureExtractionModule(string moduleName, Action<String, Dictionary<string, double>, List<Annotation>> action): base(moduleName)
        {
            if(action == null)
            {
                throw new ArgumentNullException("action");
            }
            _action = action;
        }

        Action<String, Dictionary<string, double>, List<Annotation>> _action;
        public override void ExtractTextFeatures(string textContent, Dictionary<string, double> item, List<Annotation> annotations)
        {
            if (_action != null)
            {
                _action(textContent, item, annotations);
            }
        }
    }
}
