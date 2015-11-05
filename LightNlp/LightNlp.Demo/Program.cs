using BulStem;
using de.bwaldvogel.liblinear;
using LibSvmHelper;
using LibSvmHelper.Helpers;
using LightNlp.Core;
using LightNlp.Core.Helpers;
using LightNlp.Core.Modules;
using LightNlp.Tools.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace LightNlp.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            //Define feature extraction modules
            #region 1 - Define all possible feature extraction modules
            List<FeatureExtractionModule> modules = new List<FeatureExtractionModule>();
            //Extract words from text and add them as annotations for later use
            modules.Add(new ActionFeatureExtractionModule("annotate_words", (text, features, annotations) =>
            {
                FeatureExtractionNlpHelpers.TokenizeAndAppendAnnotations(text, annotations);
                var wordAnnotations = annotations.Where(a => a.Type == "Word").ToList();
                foreach (var wordAnn in wordAnnotations)
                {
                    var loweredWordAnnotation = new Annotation() { Type = "Word_Lowered", Text = wordAnn.Text.ToLower(), FromIndex = wordAnn.FromIndex, ToIndex = wordAnn.ToIndex };
                    annotations.Add(loweredWordAnnotation);
                }
            }));

            //Generate bag of words features
            modules.Add(new ActionFeatureExtractionModule("plain_bow", (text, features, annotations) =>
            {
                var wordAnnotations = annotations.Where(a => a.Type == "Word_Lowered").ToList();
                foreach (var wordAnn in wordAnnotations)
                {
                    FeaturesDictionaryHelpers.IncreaseFeatureFrequency(features, "plain_bow_" + wordAnn.Text.ToLower(), 1.0);
                }
            }));

            //Generate word prefixes from all lowered word tokens
            int[] charPrefixLengths = new int[] { 2, 3, 4 };
            foreach (var charPrefLength in charPrefixLengths)
            {
                modules.Add(new ActionFeatureExtractionModule("npref_" + charPrefLength, (text, features, annotations) =>
                {
                    var wordAnnotations = annotations.Where(a => a.Type == "Word_Lowered").ToList();
                    foreach (var wordAnn in wordAnnotations)
                    {
                        FeatureExtractionNlpHelpers.ExtractPrefixFeatureFromSingleTokenAndUpdateItemFeatures(features, wordAnn.Text.ToLower(), charPrefLength);
                    }
                }));
            }

            //Generate word suffixes from all lowered word tokens
            int[] charSuffixLengths = new int[] { 2, 3, 4 };
            foreach (var charSuffLength in charSuffixLengths)
            {
                modules.Add(new ActionFeatureExtractionModule("nsuff_" + charSuffLength, (text, features, annotations) =>
                {
                    var wordAnnotations = annotations.Where(a => a.Type == "Word_Lowered").ToList();
                    foreach (var wordAnn in wordAnnotations)
                    {
                        FeatureExtractionNlpHelpers.ExtractSuffixFeatureFromSingleTokenAndUpdateItemFeatures(features, wordAnn.Text.ToLower(), charSuffLength);
                    }
                }));
            }

            //Generate word character ngrams
            int[] charNGramLengths = new int[] { 2, 3, 4 };
            foreach (var charNGramLength in charNGramLengths)
            {
                modules.Add(new ActionFeatureExtractionModule("chngram_" + charNGramLength, (text, features, annotations) =>
                {
                    var wordAnnotations = annotations.Where(a => a.Type == "Word_Lowered").ToList();
                    foreach (var wordAnn in wordAnnotations)
                    {
                        FeatureExtractionNlpHelpers.ExtractCharNgramFeaturesFromSingleTokenAndUpdateItemFeatures(features, wordAnn.Text.ToLower(), charNGramLength);
                    }
                }));
            }

            //Generate Stemmed word features, using Bulstem(P.Nakov)
            bool useStemmer = true;
            Stemmer stemmer = useStemmer ? new Stemmer(StemmingLevel.Medium) : null;
            modules.Add(new ActionFeatureExtractionModule("plain_word_stems", (text, features, annotations) =>
            {
                var wordAnnotations = annotations.Where(a => a.Type == "Word_Lowered").ToList();
                foreach (var wordAnn in wordAnnotations)
                {
                    FeatureExtractionNlpHelpers.ExtractStemFeatureFromSingleTokenAndUpdateItemFeatures(stemmer, features, wordAnn.Text.ToLower());
                }
            }));

            //Generate Word Ngram features
            int[] wordNgramLengths = new int[] { 2, 3, 4 };
            foreach (var ngramLength in wordNgramLengths)
            {
                modules.Add(new ActionFeatureExtractionModule(string.Format("word{0}gram", ngramLength), (text, features, annotations) =>
                {
                    var wordTokens = annotations.Where(a => a.Type == "Word_Lowered").Select(a => a.Text).ToList();
                    FeatureExtractionNlpHelpers.ExtractWordNGramFeaturesFromTextTokensAndUpdateItemFeatures(features, wordTokens, ngramLength);
                }));
            }

            modules.Add(new ActionFeatureExtractionModule("count_punct", (text, features, annotations) =>
            {
                FeatureExtractionNlpHelpers.ExtractTextPunctuationFeaturesAndUpdateItemFeatures(text, features);
            }));

            modules.Add(new ActionFeatureExtractionModule("emoticons_dnevnikbg", (text, features, annotations) =>
            {
                FeatureExtractionNlpHelpers.ExtractDnevnikEmoticonsFeaturesAndUpdateItemFeatures(text, features);
            }));

            //custom module
            //modules.Add(new ActionFeatureExtractionModule("", (text, features, annotations) => {

            //}));

            //Print possible module options
            foreach (var module in modules)
            {
                Debug.Write(module.Name + ",");
            }
            #endregion

            //configure which modules to use
            #region 2 - Configure which module configurations
            //string settingsConfig = "annotate_words,plain_bow,npref_2,npref_3,npref_4,nsuff_2,nsuff_3,nsuff_4,chngram_2,chngram_3,chngram_4,plain_word_stems,word2gram,word3gram, word4gram,count_punct,emoticons_dnevnikbg";
            string settingsConfig = "annotate_words,plain_bow,npref_2,npref_3,npref_4,nsuff_2,nsuff_3,nsuff_4,chngram_2,chngram_3,chngram_4,plain_word_stems,word2gram,word3gram, word4gram";
            //string settingsConfig = "annotate_words,plain_word_stems, npref_4, nsuff_3";

            Console.WriteLine("Module configurations:");

            var moduleNamesToConfigure = settingsConfig.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var moduleName in moduleNamesToConfigure)
            {
                Console.WriteLine(moduleName);
            }
            Console.WriteLine();

            var modulesToRegister = modules.Where(m => moduleNamesToConfigure.Contains(m.Name)).ToList();

            FeatureExtractionPipeline pipeline = new FeatureExtractionPipeline();

            foreach (var module in modulesToRegister)
            {
                pipeline.RegisterModule(module);
            }
            #endregion

            //Process input file
            string inputFile = "troll-comments.txt";
            if (args.Length > 0)
            {
                inputFile = args[0];
            }
            
            Console.WriteLine("Input file:{0}", inputFile);
            char fieldSeparator = '\t';

            #region 3 - Build features dictionary - process documents and extract all possible features
            //build features dictionary
            var featureStatisticsDictBuilder = new FeatureStatisticsDictionaryBuilder();
            
            Console.WriteLine("Building a features dictionary...");
            var timeStart = DateTime.Now;
            int itemsCnt = 0;
            Dictionary<string, int> classLabels = new Dictionary<string, int>();
            int maxClassLabelIndex = 0;

            using (var filereader = System.IO.File.OpenText(inputFile))
            {
                while (!filereader.EndOfStream)
                {
                    string line = filereader.ReadLine();
                    var fields = line.Split(new char[] { fieldSeparator });

                    //class label and doc contents
                    string classLabel = fields[0];
                    string docContent = fields[1];

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

            Console.WriteLine("Extracted {0} features from {1} documents", featureStatisticsDictBuilder.FeatureInfoStatistics.Count, itemsCnt);
            Console.WriteLine("Done - {0}", (DateTime.Now - timeStart));

            int minFeaturesFrequency = 5;
            RecomputeFeatureIndexes(featureStatisticsDictBuilder, minFeaturesFrequency);
            Console.WriteLine("Selected {0} features with min freq {1} ", featureStatisticsDictBuilder.FeatureInfoStatistics.Count, minFeaturesFrequency);
            
            //save fetures for later use
            string featuresDictFile = inputFile + ".feature";
            if (System.IO.File.Exists(featuresDictFile))
            {
                System.IO.File.Delete(featuresDictFile);
            }

            featureStatisticsDictBuilder.SaveToFile(featuresDictFile);
            Console.WriteLine("Features saved to file {0}", featuresDictFile);
            #endregion
            
            //4- Load features from file
            featureStatisticsDictBuilder.LoadFromFile(featuresDictFile);

            #region 5 - Build items with features from text documents and features dictionary
            //Build libsvm file from text insput file and features dictionary
            string libSvmFileName = inputFile + ".libsvm";

            var sparseItemsWithIndexFeatures = new List<SparseItemInt>();
            timeStart = DateTime.Now;
            Console.WriteLine("Exporting to libsvm file format...");
            using (System.IO.TextWriter textWriter = new System.IO.StreamWriter(libSvmFileName, false))
            {
                LibSvmFileBuilder libSvmFileBuilder = new LibSvmFileBuilder(textWriter);
                using (var filereader = System.IO.File.OpenText(inputFile))
                {
                    while (!filereader.EndOfStream)
                    {
                        string line = filereader.ReadLine();
                        var fields = line.Split(new char[] { fieldSeparator });

                        //class label and doc contents
                        string classLabel = fields[0];
                        string docContent = fields[1];

                        Dictionary<string, double> docFeatures = new Dictionary<string, double>();
                        pipeline.ProcessDocument(docContent, docFeatures);

                        int classLabelIndex = classLabels[classLabel];

                        //Append extracted features

                        //A - Extracted indexed features
                        var itemIndexedFeatures = LibSvmFileBuilder.GetIndexedFeaturesFromStringFeatures(docFeatures, featureStatisticsDictBuilder.FeatureInfoStatistics, minFeaturesFrequency, true, ScaleRange.MinusOneToOne);
                        libSvmFileBuilder.AppendItem(classLabelIndex, itemIndexedFeatures);
                        sparseItemsWithIndexFeatures.Add(new SparseItemInt() { Label = classLabelIndex, Features = itemIndexedFeatures });

                        //B - Or extract indexed features and append
                        //libSvmFileBuilder.PreprocessStringFeaturesAndAppendItem(classLabelIndex, docFeatures, featureStatisticsDictBuilder.FeatureInfoStatistics, minFeaturesFrequency);
                    }
                }
            }

            Console.WriteLine("Done - {0}", (DateTime.Now - timeStart));
            Console.WriteLine("Libsvm file saved to {0}", libSvmFileName);
            Console.WriteLine();
            #endregion


            #region 6 - Train and test classifier
            //LIBLINEAR - TRAIN AND EVAL CLASSIFIER
            //Build problem X and Y

            //Split data on train and test dataset
            int trainRate = 4;
            int testRate = 1;

            FeatureNode[][] problemXTrain = null;
            double[] problemYTrain = null;

            int allItemsCnt = sparseItemsWithIndexFeatures.Count;
            int trainCnt = (int)((double)allItemsCnt * trainRate / (double)(trainRate + testRate));
            int testCnt = allItemsCnt - trainCnt;

            var trainItems = sparseItemsWithIndexFeatures.Take(trainCnt).ToList();
            var testItems = sparseItemsWithIndexFeatures.Skip(trainCnt).Take(testCnt).ToList();

            SetLibLinearProblemXandYFromSparseItems(trainItems, out problemXTrain, out problemYTrain);

            //Create liblinear problem
            var problem = new Problem();
            problem.l = problemXTrain.Length;
            problem.n = featureStatisticsDictBuilder.FeatureInfoStatistics.Count;
            //problem.x = new FeatureNode[][]{
            //    new FeatureNode[]{new FeatureNode(1,0.0), new FeatureNode(2,1.0)},
            //    new FeatureNode[]{new FeatureNode(1,2.0), new FeatureNode(2,0.0)}
            //};
            //problem.y = new double[] { 1, 2 };

            problem.x = problemXTrain;
            problem.y = problemYTrain;

            var solver = SolverType.L1R_LR;//L2R_LR
            var c = 1.0;
            var eps = 0.01;

            Console.WriteLine("Training a classifier with {0} items...", sparseItemsWithIndexFeatures.Count);
            timeStart = DateTime.Now;
            var parameter = new Parameter(solver, c, eps);
            var model = Linear.train(problem, parameter);

            Console.WriteLine("Done - {0}", (DateTime.Now - timeStart));
            string modelFileName = inputFile + ".train.model";
            var modelFile = new java.io.File(modelFileName);
            model.save(modelFile);
            Console.WriteLine("Model saved to {0}", modelFileName);


            var modelLoaded = Model.load(modelFile);

            //evaluation
            FeatureNode[][] problemXEval = null;
            double[] problemYEval = null;
            SetLibLinearProblemXandYFromSparseItems(testItems, out problemXEval, out problemYEval);

            List<double> predictedY = new List<double>();
            foreach (var item in problemXEval)
            {
                double prediction = Linear.predict(model, item);
                predictedY.Add(prediction);
            }

            int[][] matrix = BuildConfusionMatrix(problemYEval, predictedY, classLabels.Count);

            Console.WriteLine("Class labels:");
            foreach (var label in classLabels)
            {
                Console.WriteLine(string.Format("{1} - {0}", label.Key, label.Value));
            }
            Console.WriteLine();
            PrintMatrix(matrix, true);

            for (int i = 0; i < matrix.Length; i++)
            {
                int truePositivesCnt = matrix[i][i];
                int falsePositievesCnt = matrix[i].Sum() - matrix[i][i];
                int falseNegativesCnt = matrix.Select(m => m[i]).Sum() - matrix[i][i];

                double precision;
                double recall;
                double fScore;

                CalculatePRF(truePositivesCnt, falsePositievesCnt, falseNegativesCnt, out precision, out recall, out fScore);
                Console.WriteLine(string.Format("[{0} - {4}] P={1:0.00}, R={2:0.00}, F={3:0.00} ", i, precision, recall, fScore, orderedClassLabels.ToList().ToDictionary(kv => kv.Value, kv => kv.Key)[i]));
            }

            int truePositivesCntOverall = 0;
            int testedCnt = 0;
            for (int i = 0; i < matrix.Length; i++)
            {
                truePositivesCntOverall += matrix[i][i];
                testedCnt += matrix[i].Sum();
            }

            double accuracyOverall = (double)truePositivesCntOverall / (double)testedCnt;
            Console.WriteLine(string.Format("[{0}] Accuracy={1:0.00}", "Overall", accuracyOverall));


            //CROSSVALIDATION
            //int crossValidationFold = 5;
            //Console.WriteLine("Training with {0} items and 5 fold crossvalidation...", sparseItemsWithIndexFeatures.Count);
            //timeStart = DateTime.Now;
            //double[] target = new double[problem.l];
            //Linear.crossValidation(problem, parameter, crossValidationFold, target);
            //Console.WriteLine("Done - {0}", (DateTime.Now - timeStart));

            //WriteResult(target);
            #endregion

            Console.ReadKey();

            //var instancesToTest = new Feature[] { new FeatureNode(1, 0.0), new FeatureNode(2, 1.0) };
            //var prediction = Linear.predict(model, instancesToTest);

            //string docToProcess = "Оставете правителството да си работи на спокойствие!";
            //SparseItemString docItem = new SparseItemString()
            //{
            //    Label = 1,
            //};

            //pipeline.ProcessDocument(docToProcess, docItem.Features);

            //foreach (var feature in docItem.Features)
            //{
            //    Debug.WriteLine(string.Format("{0} - {1}", feature.Key, feature.Value));
            //}
        }


        private static void CalculatePRF(int truePositives, int falsePositive, int falseNegatives, out double precision, out double recall, out double fScore)
        {
            precision = (double)truePositives / ((double)truePositives + (double)falsePositive);
            recall = (double)truePositives / ((double)truePositives + (double)falseNegatives);
            fScore = 2 * (double)precision * (double)recall / ((double)precision + (double)recall);
        }

        private static void PrintMatrix(int[][] matrix, bool printClassLabels)
        {
            Console.Write(string.Format("{0,5}", string.Empty));
            for (int j = 0; j < matrix[0].Length; j++)
            {
                Console.Write(string.Format("{0,5}", j));
            }

            Console.WriteLine();
            for (int i = 0; i < matrix.Length; i++)
            {
                Console.Write(string.Format("{0,5}", i));
                for (int j = 0; j < matrix[i].Length; j++)
                {
                    Console.Write(string.Format("{0,5}", matrix[i][j]));
                }
                Console.WriteLine();
            }
        }

        private static int[][] BuildConfusionMatrix(double[] problemYEval, List<double> predictedY, int numberOfLabels)
        {
            List<int[]> list = new List<int[]>();
            for (int i = 0; i < numberOfLabels; i++)
            {
                list.Add(new int[numberOfLabels]);
            }

            int[][] matrix = list.ToArray();

            for (int i = 0; i < problemYEval.Length; i++)
            {
                var expectedClassLabel = (int)problemYEval[i];
                var predictedClassLabel = (int)predictedY[i];

                matrix[expectedClassLabel][predictedClassLabel] = matrix[expectedClassLabel][predictedClassLabel] + 1;
            }
            return matrix;
        }


        private static void RecomputeFeatureIndexes(FeatureStatisticsDictionaryBuilder featureStatisticsDictBuilder, int minFeaturesFrequency)
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
                FeatureNode[] itemFeatureNodes = ConvertToSortedFeatureNodeArray(item.Features);
                problemXList.Add(itemFeatureNodes);

                problemYList.Add(item.Label);
            }

            problemX = problemXList.ToArray();
            problemY = problemYList.ToArray();
        }

        private static FeatureNode[] ConvertToSortedFeatureNodeArray(Dictionary<int, double> featuresDictionary)
        {
            var featuresDictSorted = featuresDictionary.Select(kv => kv).OrderBy(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value);
            List<FeatureNode> featureNodes = new List<FeatureNode>();
            foreach (var item in featuresDictSorted)
            {
                featureNodes.Add(new FeatureNode(item.Key, item.Value));
            }

            var featureNodesArray = featureNodes.ToArray();
            return featureNodesArray;
        }
    }
}
