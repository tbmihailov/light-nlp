using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
//using System.Data.Entity;
using LightNlpWebApiSelfHost.Models;
using de.bwaldvogel.liblinear;
using LibSvmHelper;
using LibSvmHelper.Helpers;
using LightNlp.Core.Modules;
using LightNlp.Tools.Helpers;
using System.Configuration;
using LightNlp.Core.Helpers;
using BulStem;
using System.Diagnostics;
using LightNlp.Core;
using LightNlp.Tools.IO;
using LightNlp.Helpers;

namespace LightNlpWebApiSelfHost
{
    class Program
    {
        static void Main(string[] args)
        {
            string command = "service";
            if (args.Length > 0)
            {
                command = args[0];
            }

            if (command == "service")
            {
                // Specify the URI to use for the local host:
                string baseUri = ConfigurationManager.AppSettings["WEBSERVICE_ADDRESS"] ?? "http://localhost:8080";

                Console.WriteLine("Starting LightNlp web Server...");
                WebApp.Start<Startup>(baseUri);
                Console.WriteLine("Server running at {0} - press Enter to quit. ", baseUri);
                Console.WriteLine("Demo at: "+baseUri+"/api/Services/Categorize?jsonContent={%22id%22:1,%22text%22:%22some%20example%20text%20here%22}");
                Console.ReadLine();
            }
            else if (command == "train")
            {
                string modulesConfig, inputRawFile, classLabelsOutputFileName, featuresDictOutputFile, modelOutputFileName, libSvmOutputFileName;
                SolverType liblinearSolver;
                double liblinearC, liblinearEps;
                int minFeaturesFrequency;
                bool normalize;
                ScaleRange scaleRange;

                ModelConfiguration.SetParams(out modulesConfig, out inputRawFile, out classLabelsOutputFileName, out featuresDictOutputFile, out modelOutputFileName, out libSvmOutputFileName, out liblinearSolver, out liblinearC, out liblinearEps, out minFeaturesFrequency, out normalize, out scaleRange);

                Train(modulesConfig,
                    inputRawFile,
                    liblinearSolver,
                    liblinearC,
                    liblinearEps,
                    classLabelsOutputFileName,
                    featuresDictOutputFile,
                    modelOutputFileName,
                    libSvmOutputFileName,
                    minFeaturesFrequency,
                    normalize,
                    scaleRange);
            }
        }

        

        public static void Train(string modulesConfig, string inputRawFile, SolverType liblinearSolver, double liblinearC, double liblinearEps,
                string classLabelsOutputFileName, string featuresDictOutputFile, string modelOutputFileName, string libSvmOutputFileName, 
                int minFeaturesFrequency,bool normalize, ScaleRange scaleRange)
        {
            //Define feature extraction modules
            #region 1 - Define all possible feature extraction modules
            List<FeatureExtractionModule> modules = PipelineConfiguration.GetExtractionModules();

            //Print possible module options
            foreach (var module in modules)
            {
                Debug.Write(module.Name + ",");
            }
            #endregion

            //configure which modules to use
            #region 2 - Configure which module configurations
            //string modulesConfig = "annotate_words,plain_bow,npref_2,npref_3,npref_4,nsuff_2,nsuff_3,nsuff_4,chngram_2,chngram_3,chngram_4,plain_word_stems,word2gram,word3gram, word4gram,count_punct,emoticons_dnevnikbg,doc_start,doc_end";

            //string settingsConfig = "annotate_words,plain_word_stems, npref_4, nsuff_3";

            FeatureExtractionPipeline pipeline = PipelineConfiguration.BuildPipeline(modulesConfig, modules);
            #endregion

            Console.WriteLine("Input file:{0}", inputRawFile);
            //char fieldSeparator = '\t';

            #region 3 - Build features dictionary - process documents and extract all possible features
            //build features dictionary
            var featureStatisticsDictBuilder = new FeatureStatisticsDictionaryBuilder();

            Console.WriteLine("Building a features dictionary...");
            var timeStart = DateTime.Now;
            int itemsCnt = 0;
            Dictionary<string, int> classLabels = new Dictionary<string, int>();
            int maxClassLabelIndex = 0;

            using (var filereader = new LabeledTextDocumentFileReader(inputRawFile))
            {
                while (!filereader.EndOfSource())
                {
                    var doc = filereader.ReadDocument();

                    //class label and doc contents
                    string classLabel = doc.ClassLabel;
                    string docContent = doc.DocContent;

                    //build class labels dictionary
                    if (!classLabels.ContainsKey(classLabel))
                    {
                        classLabels[classLabel] = maxClassLabelIndex;
                        maxClassLabelIndex++;
                    }

                    Dictionary<string, double> docFeatures = new Dictionary<string, double>();
                    pipeline.ProcessDocument(docContent, docFeatures);
                    featureStatisticsDictBuilder.UpdateInfoStatisticsFromSingleDoc(docFeatures);
                    itemsCnt++;

                    if (itemsCnt % 500 == 0)
                    {
                        Console.WriteLine("{0} processed so far", itemsCnt);
                    }
                }
            }

            //order classes by name - until now they are ordered by first occurence in dataset

            var ordered = classLabels.OrderBy(cv => cv.Key);
            var orderedClassLabels = new Dictionary<string, int>();
            int classIndexCounter = 0;
            foreach (var item in ordered)
            {
                orderedClassLabels.Add(item.Key, classIndexCounter);
                classIndexCounter++;
            }
            classLabels = orderedClassLabels;
            LexiconReaderHelper.SaveDictionaryToFile(classLabels, classLabelsOutputFileName);
            Console.WriteLine("Class labels saved to file {0}", classLabelsOutputFileName);

            Console.WriteLine("Extracted {0} features from {1} documents", featureStatisticsDictBuilder.FeatureInfoStatistics.Count, itemsCnt);
            Console.WriteLine("Done - {0}", (DateTime.Now - timeStart));

            
            RecomputeFeatureIndexes(featureStatisticsDictBuilder, minFeaturesFrequency);
            Console.WriteLine("Selected {0} features with min freq {1} ", featureStatisticsDictBuilder.FeatureInfoStatistics.Count, minFeaturesFrequency);

            //save fetures for later use
            if (System.IO.File.Exists(featuresDictOutputFile))
            {
                System.IO.File.Delete(featuresDictOutputFile);
            }

            featureStatisticsDictBuilder.SaveToFile(featuresDictOutputFile);
            Console.WriteLine("Features saved to file {0}", featuresDictOutputFile);
            #endregion

            //4- Load features from file
            featureStatisticsDictBuilder.LoadFromFile(featuresDictOutputFile);
            classLabels = LexiconReaderHelper.LoadDictionaryFromFile(classLabelsOutputFileName);

            #region 5 - Build items with features from text documents and features dictionary
            //Build libsvm file from text insput file and features dictionary

            var sparseItemsWithIndexFeatures = new List<SparseItemInt>();
            timeStart = DateTime.Now;
            Console.WriteLine("Exporting to libsvm file format...");

            
            using (System.IO.TextWriter textWriter = new System.IO.StreamWriter(libSvmOutputFileName, false))
            {
                LibSvmFileBuilder libSvmFileBuilder = new LibSvmFileBuilder(textWriter);
                using (var filereader = new LabeledTextDocumentFileReader(inputRawFile))
                {
                    while (!filereader.EndOfSource())
                    {
                        var doc = filereader.ReadDocument();

                        //class label and doc contents
                        string classLabel = doc.ClassLabel;
                        string docContent = doc.DocContent;
                        int classLabelIndex = classLabels[classLabel];

                        SparseItemInt sparseItem = ProcessingHelpers.ProcessTextAndGetSparseItem(pipeline, featureStatisticsDictBuilder, minFeaturesFrequency, normalize, scaleRange, docContent, classLabelIndex);

                        libSvmFileBuilder.AppendItem(sparseItem.Label, sparseItem.Features);
                        sparseItemsWithIndexFeatures.Add(sparseItem);

                        //B - Or extract indexed features and append
                        //libSvmFileBuilder.PreprocessStringFeaturesAndAppendItem(classLabelIndex, docFeatures, featureStatisticsDictBuilder.FeatureInfoStatistics, minFeaturesFrequency);
                    }
                }
            }

            Console.WriteLine("Done - {0}", (DateTime.Now - timeStart));
            Console.WriteLine("Libsvm file saved to {0}", libSvmOutputFileName);
            Console.WriteLine();
            #endregion


            #region 6 - Train and test classifier
            //LIBLINEAR - TRAIN AND EVAL CLASSIFIER
            //Build problem X and Y



            //Split data on train and test dataset
            int trainRate = 4;
            int testRate = 1;


            int allItemsCnt = sparseItemsWithIndexFeatures.Count;
            int trainCnt = (int)((double)allItemsCnt * trainRate / (double)(trainRate + testRate));
            int testCnt = allItemsCnt - trainCnt;

            var trainItems = sparseItemsWithIndexFeatures.Take(trainCnt).ToList();
            var testItems = sparseItemsWithIndexFeatures.Skip(trainCnt).Take(testCnt).ToList();

            string trainDataModelFileName = inputRawFile + ".train.model";

            FeatureNode[][] problemXTrain = null;
            double[] problemYTrain = null;

            SetLibLinearProblemXandYFromSparseItems(trainItems, out problemXTrain, out problemYTrain);

            TrainLibLinearProblemAndSveModel(liblinearSolver, liblinearC, liblinearEps, featureStatisticsDictBuilder, trainDataModelFileName, problemXTrain, problemYTrain);

            var modelFileLoad = new java.io.File(trainDataModelFileName);
            var modelLoaded = Model.load(modelFileLoad);

            //evaluation
            List<FeatureNode[]> problemXEvalList = new List<FeatureNode[]>();
            List<double> problemYEvalList = new List<double>();
            //SetLibLinearProblemXandYFromSparseItems(testItems, out problemXEval, out problemYEval);

            //EVALUATE
            List<double> predictedY = new List<double>();
            foreach (var item in testItems)
            {
                var itemFeatureNodes = ProcessingHelpers.ConvertToSortedFeatureNodeArray(item.Features);

                //fill eval list
                problemXEvalList.Add(itemFeatureNodes);
                problemYEvalList.Add((double)item.Label);

                //predict
                double prediction = Linear.predict(modelLoaded, itemFeatureNodes);
                predictedY.Add(prediction);
            }

            int[][] matrix = ResultCalculationMetricsHelpers.BuildConfusionMatrix(problemYEvalList.ToArray(), predictedY, classLabels.Count);

            Console.WriteLine("Class labels:");
            foreach (var label in classLabels)
            {
                Console.WriteLine(string.Format("{1} - {0}", label.Key, label.Value));
            }
            Console.WriteLine();
            ResultCalculationMetricsHelpers.PrintMatrix(matrix, true);

            for (int i = 0; i < matrix.Length; i++)
            {
                int truePositivesCnt = matrix[i][i];
                int falsePositievesCnt = matrix[i].Sum() - matrix[i][i];
                int falseNegativesCnt = matrix.Select(m => m[i]).Sum() - matrix[i][i];

                double precision;
                double recall;
                double fScore;

                ResultCalculationMetricsHelpers.CalculatePRF(truePositivesCnt, falsePositievesCnt, falseNegativesCnt, out precision, out recall, out fScore);
                Console.WriteLine(string.Format("[{0} - {4}] P={1:0.0000}, R={2:0.0000}, F={3:0.0000} ", i, precision, recall, fScore, orderedClassLabels.ToList().ToDictionary(kv => kv.Value, kv => kv.Key)[i]));
            }

            int truePositivesCntOverall = 0;
            int testedCnt = 0;
            for (int i = 0; i < matrix.Length; i++)
            {
                truePositivesCntOverall += matrix[i][i];
                testedCnt += matrix[i].Sum();
            }

            double accuracyOverall = (double)truePositivesCntOverall / (double)testedCnt;
            Console.WriteLine(string.Format("[{0}] Accuracy={1:0.0000}", "Overall", accuracyOverall));

            //----TRAIN MODEL IWTH ALL DATA
            Console.WriteLine("------------------------------------------------");
            Console.WriteLine("Train model on all data");



            FeatureNode[][] problemXAll = null;
            double[] problemYAll = null;

            var allItems = sparseItemsWithIndexFeatures;
            SetLibLinearProblemXandYFromSparseItems(allItems, out problemXAll, out problemYAll);

            TrainLibLinearProblemAndSveModel(liblinearSolver, liblinearC, liblinearEps, featureStatisticsDictBuilder, modelOutputFileName, problemXAll, problemYAll);

            //CROSSVALIDATION
            //int crossValidationFold = 5;
            //Console.WriteLine("Training with {0} items and 5 fold crossvalidation...", sparseItemsWithIndexFeatures.Count);
            //timeStart = DateTime.Now;
            //double[] target = new double[problem.l];
            //Linear.crossValidation(problem, parameter, crossValidationFold, target);
            //Console.WriteLine("Done - {0}", (DateTime.Now - timeStart));

            //WriteResult(target);
            #endregion

            //Console.ReadKey();

            //var instancesToTest = new Feature[] { new FeatureNode(1, 0.0), new FeatureNode(2, 1.0) };
            //var prediction = Linear.predict(model, instancesToTest);
        }

        



        private static void TrainLibLinearProblemAndSveModel(SolverType liblinearSolver, double liblinearC, double liblinearEps, FeatureStatisticsDictionaryBuilder featureStatisticsDictBuilder, string trainDataModelFileName, FeatureNode[][] problemXTrain, double[] problemYTrain)
        {
            //Create liblinear problem
            var problem = new Problem();
            problem.l = problemXTrain.Length;
            problem.n = featureStatisticsDictBuilder.FeatureInfoStatistics.Count;
            problem.x = problemXTrain;
            problem.y = problemYTrain;

            Console.WriteLine("Training a classifier with {0} items...", problemXTrain.Length);
            DateTime timeStart = DateTime.Now;
            var parameter = new Parameter(liblinearSolver, liblinearC, liblinearEps);
            Model model = Linear.train(problem, parameter);
            Console.WriteLine("Done - {0}", (DateTime.Now - timeStart));

            var modelFile = new java.io.File(trainDataModelFileName);
            model.save(modelFile);
            Console.WriteLine("Train data model saved to {0}", trainDataModelFileName);
        }



        public static void RecomputeFeatureIndexes(FeatureStatisticsDictionaryBuilder featureStatisticsDictBuilder, int minFeaturesFrequency)
        {
            int featureIndex = 0;
            Dictionary<string, FeatureInfo> featureInfos = new Dictionary<string, FeatureInfo>();
            foreach (var item in featureStatisticsDictBuilder.FeatureInfoStatistics)
            {
                if (item.Value.DocsFrequency < minFeaturesFrequency)
                {
                    continue;
                }
                featureIndex++;
                featureInfos.Add(item.Key, new FeatureInfo() { Index = featureIndex, DocsFrequency = item.Value.DocsFrequency, FeatureName = item.Value.FeatureName, MinValue = item.Value.MinValue, MaxValue = item.Value.MaxValue });
            };

            featureStatisticsDictBuilder.FeatureInfoStatistics.Clear();
            foreach (var fi in featureInfos)
            {
                featureStatisticsDictBuilder.FeatureInfoStatistics.Add(fi.Key, fi.Value);
            }
        }

        private static void WriteResult(double[] target)
        {
            foreach (var val in target)
            {
                Console.Write("{0}\t", val);
            }

            Console.WriteLine();
        }

        private static void SetLibLinearProblemXandYFromSparseItems(List<SparseItemInt> items, out FeatureNode[][] problemX, out double[] problemY)
        {
            List<FeatureNode[]> problemXList = new List<FeatureNode[]>();
            List<double> problemYList = new List<double>();

            foreach (var item in items)
            {
                FeatureNode[] itemFeatureNodes = ProcessingHelpers.ConvertToSortedFeatureNodeArray(item.Features);
                problemXList.Add(itemFeatureNodes);

                problemYList.Add(item.Label);
            }

            problemX = problemXList.ToArray();
            problemY = problemYList.ToArray();
        }


    }
}

