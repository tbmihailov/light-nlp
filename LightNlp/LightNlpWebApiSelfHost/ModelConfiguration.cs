using de.bwaldvogel.liblinear;
using LibSvmHelper.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightNlpWebApiSelfHost
{
    public static class ModelConfiguration
    {
        public static void SetParams(out string modulesConfig, out string inputRawFile, out string classLabelsOutputFileName, out string featuresDictOutputFile, out string modelOutputFileName, out string libSvmOutputFileName, out SolverType liblinearSolver, out double liblinearC, out double liblinearEps, out int minFeaturesFrequency, out bool normalize, out ScaleRange scaleRange)
        {
            modulesConfig = ConfigurationManager.AppSettings["PIPELINE_MODULES_CONFIG"] ?? "annotate_words,plain_bow,nsuff_3,chngram_3,word2gram,doc_end";
            inputRawFile = ConfigurationManager.AppSettings["TRAIN_RAW_FILE"] ?? "data\troll-comments.txt";
            classLabelsOutputFileName = ConfigurationManager.AppSettings["MODEL_CLASSLABELS_FILE"] != null ? ConfigurationManager.AppSettings["MODEL_CLASSLABELS_FILE"] : inputRawFile + ".classlabels";
            featuresDictOutputFile = ConfigurationManager.AppSettings["MODEL_FEATURES_FILE"] != null ? ConfigurationManager.AppSettings["MODEL_FEATURES_FILE"] : inputRawFile + ".features";
            modelOutputFileName = ConfigurationManager.AppSettings["MODEL_MODEL_FILE"] != null ? ConfigurationManager.AppSettings["MODEL_MODEL_FILE"] : inputRawFile + ".model";
            libSvmOutputFileName = inputRawFile + ".libsvm";
            liblinearSolver = SolverType.L1R_LR;
            liblinearC = ConfigurationManager.AppSettings["LIBLINEAR_C"] != null ? double.Parse(ConfigurationManager.AppSettings["LIBLINEAR_C"]) : 1.0;
            liblinearEps = ConfigurationManager.AppSettings["LIBLINEAR_EPS"] != null ? double.Parse(ConfigurationManager.AppSettings["LIBLINEAR_EPS"]) : 0.01;
            minFeaturesFrequency = 5;
            normalize = false;
            scaleRange = ScaleRange.ZeroToOne;
        }
    }
}
