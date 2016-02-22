using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net.Http;
using LightNlpWebApiSelfHost.Models;

// Add these usings:
using System.Data.Entity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web.Http.Results;
using System.Configuration;
using de.bwaldvogel.liblinear;
using LightNlp.Core.Modules;
using LightNlp.Core;
using LibSvmHelper.Helpers;
using LightNlp.Tools.Helpers;
using LibSvmHelper;

namespace LightNlpWebApiSelfHost.Controllers
{
    public class ServicesController : ApiController
    {
        ApplicationDbContext dbContext = new ApplicationDbContext();


        [HttpGet]
        public JsonResult<Dictionary<string, string>> Categorize(string jsonContent)
        {
            Dictionary<string, string> docValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonContent);
            string text = docValues.ContainsKey("Text") ? docValues["Text"] : docValues["text"];

            //MODEL CONFIGURATION
            string modulesConfig, inputRawFile, classLabelsOutputFileName, featuresDictOutputFile, modelOutputFileName, libSvmOutputFileName;
            SolverType liblinearSolver;
            double liblinearC, liblinearEps;
            int minFeaturesFrequency;
            bool normalize;
            ScaleRange scaleRange;

            ModelConfiguration.SetParams(out modulesConfig, out inputRawFile, out classLabelsOutputFileName, out featuresDictOutputFile, out modelOutputFileName, out libSvmOutputFileName, out liblinearSolver, out liblinearC, out liblinearEps, out minFeaturesFrequency, out normalize, out scaleRange);

            //LOAD PIPELINE
            List<FeatureExtractionModule> modules = PipelineConfiguration.GetExtractionModules();
            FeatureExtractionPipeline pipeline = PipelineConfiguration.BuildPipeline(modulesConfig, modules);

            //load features
            FeatureStatisticsDictionaryBuilder featureStatisticsDictBuilder = new FeatureStatisticsDictionaryBuilder();
            featureStatisticsDictBuilder.LoadFromFile(featuresDictOutputFile);

            //load labels
            var classLabels = LexiconReaderHelper.LoadDictionaryFromFile(classLabelsOutputFileName);

            //load model
            var modelFileLoad = new java.io.File(modelOutputFileName);
            var modelLoaded = Model.load(modelFileLoad);


            SparseItemInt sparseItem = ProcessingHelpers.ProcessTextAndGetSparseItem(pipeline, featureStatisticsDictBuilder, minFeaturesFrequency, normalize, scaleRange, text, 0);
            var itemFeatureNodes = ProcessingHelpers.ConvertToSortedFeatureNodeArray(sparseItem.Features);

            //predict
            double prediction = Linear.predict(modelLoaded, itemFeatureNodes);
            int label = (int)prediction;
            string labelName = "unknown";
            if (classLabels.ContainsValue(label))
            {
                labelName = classLabels.ToList().Where(k => k.Value == label).Select(k => k.Key).FirstOrDefault();
            }

            docValues["LabelName"] = labelName;
            docValues["Label"] = (prediction).ToString();
            docValues["Confidence"] = (0.00).ToString();

            return Json(docValues);
        }

        [HttpPost]
        public JsonResult<Dictionary<string, string>> CategorizeTextDoc(Dictionary<string, string> docValues)
        {
            //Load model configuration
            string modulesConfig, inputRawFile, classLabelsOutputFileName, featuresDictOutputFile, modelOutputFileName, libSvmOutputFileName;
            SolverType liblinearSolver;
            double liblinearC, liblinearEps;
            int minFeaturesFrequency;
            bool normalize;
            ScaleRange scaleRange;
            ModelConfiguration.SetParams(out modulesConfig, out inputRawFile, out classLabelsOutputFileName, out featuresDictOutputFile, out modelOutputFileName, out libSvmOutputFileName, out liblinearSolver, out liblinearC, out liblinearEps, out minFeaturesFrequency, out normalize, out scaleRange);

            //Load pipeline
            List<FeatureExtractionModule> modules = PipelineConfiguration.GetExtractionModules();
            FeatureExtractionPipeline pipeline = PipelineConfiguration.BuildPipeline(modulesConfig, modules);

            //Load features
            FeatureStatisticsDictionaryBuilder featureStatisticsDictBuilder = new FeatureStatisticsDictionaryBuilder();
            featureStatisticsDictBuilder.LoadFromFile(featuresDictOutputFile);

            //Load labels
            var classLabels = LexiconReaderHelper.LoadDictionaryFromFile(classLabelsOutputFileName);

            //Load model
            var modelFileLoad = new java.io.File(modelOutputFileName);
            var modelLoaded = Model.load(modelFileLoad);

            //Categorize single item
            string text = docValues.ContainsKey("Text") ? docValues["Text"] : docValues["text"];
            double prediction, confidence;
            int label;
            string labelName;
            CategorizeText(text, minFeaturesFrequency, normalize, scaleRange, pipeline, featureStatisticsDictBuilder, classLabels, modelLoaded, out prediction, out label, out labelName, out confidence);

            docValues["LabelName"] = labelName;
            docValues["Label"] = (prediction).ToString();//label might also be used for categorization
            docValues["Confidence"] = (confidence).ToString();

            return Json(docValues);
        }

        [HttpPost]
        public JsonResult<List<Dictionary<string, string>>> CategorizeTextDocMulti(List<Dictionary<string, string>> docValuesList)
        {
            //Load model configuration
            string modulesConfig, inputRawFile, classLabelsOutputFileName, featuresDictOutputFile, modelOutputFileName, libSvmOutputFileName;
            SolverType liblinearSolver;
            double liblinearC, liblinearEps;
            int minFeaturesFrequency;
            bool normalize;
            ScaleRange scaleRange;
            ModelConfiguration.SetParams(out modulesConfig, out inputRawFile, out classLabelsOutputFileName, out featuresDictOutputFile, out modelOutputFileName, out libSvmOutputFileName, out liblinearSolver, out liblinearC, out liblinearEps, out minFeaturesFrequency, out normalize, out scaleRange);

            //Load pipeline
            List<FeatureExtractionModule> modules = PipelineConfiguration.GetExtractionModules();
            FeatureExtractionPipeline pipeline = PipelineConfiguration.BuildPipeline(modulesConfig, modules);

            //Load features
            FeatureStatisticsDictionaryBuilder featureStatisticsDictBuilder = new FeatureStatisticsDictionaryBuilder();
            featureStatisticsDictBuilder.LoadFromFile(featuresDictOutputFile);

            //Load labels
            var classLabels = LexiconReaderHelper.LoadDictionaryFromFile(classLabelsOutputFileName);

            //Load model
            var modelFileLoad = new java.io.File(modelOutputFileName);
            var modelLoaded = Model.load(modelFileLoad);

            foreach (var docValues in docValuesList)
            {
                string text = string.Empty;
                //Categorize single item
                if (docValues.ContainsKey("Text")){
                    text = docValues["Text"];
                    docValues.Remove("Text");
                }
                else
                if (docValues.ContainsKey("text")){
                    text = docValues["text"];
                    docValues.Remove("text");
                }
                
                double prediction, confidence;
                int label;
                string labelName;
                CategorizeText(text, minFeaturesFrequency, normalize, scaleRange, pipeline, featureStatisticsDictBuilder, classLabels, modelLoaded, out prediction, out label, out labelName, out confidence);

                docValues["LabelName"] = labelName;
                docValues["Label"] = (prediction).ToString();//label might also be used for categorization
                docValues["Confidence"] = (confidence).ToString();
            }

            return Json(docValuesList);
        }

        private static void CategorizeText(string text, int minFeaturesFrequency, bool normalize, ScaleRange scaleRange, FeatureExtractionPipeline pipeline, FeatureStatisticsDictionaryBuilder featureStatisticsDictBuilder, Dictionary<string, int> classLabels, Model modelLoaded, out double prediction, out int label, out string labelName, out double confidence)
        {
            SparseItemInt sparseItem = ProcessingHelpers.ProcessTextAndGetSparseItem(pipeline, featureStatisticsDictBuilder, minFeaturesFrequency, normalize, scaleRange, text, 0);
            var itemFeatureNodes = ProcessingHelpers.ConvertToSortedFeatureNodeArray(sparseItem.Features);

            //predict
            prediction = Linear.predict(modelLoaded, itemFeatureNodes);
            label = (int)prediction;
            labelName = "unknown";
            var labelInt = label;
            if (classLabels.ContainsValue(labelInt))
            {
                labelName = classLabels.ToList().Where(k => k.Value == labelInt).Select(k => k.Key).FirstOrDefault();
            }
            confidence = 0.00;
            //not supported
        }

        //public IEnumerable<PostData> Get()
        //{
        //    return null;
        //}
        //public IEnumerable<PostData> Get()
        //{
        //    return null;
        //}


        //public async Task<PostData> Get(int id)
        //{
        //    return null;
        //}


        //[HttpPost]
        //public async Task<IHttpActionResult> Post(PostData postData)
        //{
        //    return Ok();
        //}


        //public async Task<IHttpActionResult> Put(PostData postData)
        //{
        //    return Ok();
        //}


        //public async Task<IHttpActionResult> Delete(int id)
        //{
        //    return Ok();
        //}
    }
}
