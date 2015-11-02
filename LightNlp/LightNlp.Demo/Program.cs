using BulStem;
using LibSvmHelper;
using LibSvmHelper.Helpers;
using LightNlp.Core;
using LightNlp.Core.Helpers;
using LightNlp.Core.Modules;
using LightNlp.Tools.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace LightNlp.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            //Define feature extraction modules

            List<FeatureExtractionModule> modules = new List<FeatureExtractionModule>();
            //Extract words from text and add them as annotations for later use
            modules.Add(new ActionFeatureExtractionModule("annotate_words", (text, features, annotations) => {
                FeatureExtractionNlpHelpers.TokenizeAndAppendAnnotations(text, annotations);
                var wordAnnotations = annotations.Where(a => a.Type == "Word").ToList();
                foreach (var wordAnn in wordAnnotations)
                {
                    var loweredWordAnnotation = new Annotation() { Type = "Word_Lowered", Text = wordAnn.Text.ToLower(), FromIndex = wordAnn.FromIndex, ToIndex = wordAnn.ToIndex };
                    annotations.Add(loweredWordAnnotation);
                }
            }));

            //Generate bag of words features
            modules.Add(new ActionFeatureExtractionModule("plain_bow", (text, features, annotations) => {
                var wordAnnotations = annotations.Where(a => a.Type == "Word_Lowered").ToList();
                foreach (var wordAnn in wordAnnotations)
                {
                    FeaturesDictionaryHelpers.IncreaseFeatureFrequency(features, wordAnn.Text.ToLower(), 1.0);
                }
            }));

            //Generate word prefixes from all lowered word tokens
            int[] charPrefixLengths = new int[] { 2, 3, 4 };
            foreach (var charPrefLength in charPrefixLengths)
            {
                modules.Add(new ActionFeatureExtractionModule("npref_" + charPrefLength, (text, features, annotations) => {
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
                modules.Add(new ActionFeatureExtractionModule("nsuff_"+charSuffLength, (text, features, annotations) => {
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
                modules.Add(new ActionFeatureExtractionModule("chngram_" + charNGramLength, (text, features, annotations) => {
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
            modules.Add(new ActionFeatureExtractionModule("plain_word_stems", (text, features, annotations) => {
                var wordAnnotations = annotations.Where(a => a.Type == "Word_Lowered").ToList();
                foreach (var wordAnn in wordAnnotations)
                {
                    FeatureExtractionNlpHelpers.ExtractStemFeatureFromSingleTokenAndUpdateItemFeatures(stemmer,features, wordAnn.Text.ToLower());
                }
            }));

            //Generate Word Ngram features
            int[] wordNgramLengths = new int[] { 2, 3, 4 };
            foreach (var ngramLength in wordNgramLengths)
            {
                modules.Add(new ActionFeatureExtractionModule(string.Format("word{0}gram", ngramLength), (text, features, annotations) => {
                    var wordTokens = annotations.Where(a => a.Type == "Word_Lowered").Select(a => a.Text).ToList();
                    FeatureExtractionNlpHelpers.ExtractWordNGramFeaturesFromTextTokensAndUpdateItemFeatures(features, wordTokens, ngramLength);
                }));
            }

            modules.Add(new ActionFeatureExtractionModule("count_punct", (text, features, annotations) => {
                FeatureExtractionNlpHelpers.ExtractTextPunctuationFeaturesAndUpdateItemFeatures(text, features);
            }));

            modules.Add(new ActionFeatureExtractionModule("emoticons_dnevnikbg", (text, features, annotations) => {
                FeatureExtractionNlpHelpers.ExtractDnevnikEmoticonsFeaturesAndUpdateItemFeatures(text, features);
            }));

            //custom module
            //modules.Add(new ActionFeatureExtractionModule("", (text, features, annotations) => {

            //}));

            //Print possible module options
            foreach (var module in modules)
            {
                Debug.Write(module.Name+",");
            }

            //configure which modules to use
            string settingsConfig = "annotate_words,plain_bow,npref_2,npref_3,npref_4,nsuff_2,nsuff_3,nsuff_4,chngram_2,chngram_3,chngram_4,plain_word_stems,word2gram,word3gram,word4gram,count_punct,emoticons_dnevnikbg";
            var moduleNamesToConfigure = settingsConfig.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var modulesToRegister = modules.Where(m => moduleNamesToConfigure.Contains(m.Name)).ToList();

            FeatureExtractionPipeline pipeline = new FeatureExtractionPipeline();

            foreach (var module in modulesToRegister)
            {
                pipeline.RegisterModule(module);                
            }


            string inputFile = "troll-comments.txt";
            char separator = '\t';

            var featureStatisticsDictBuilder = new FeatureStatisticsDictionaryBuilder();
            //build features dictionary
            using (var filereader = File.OpenText(inputFile))
            {
                while (!filereader.EndOfStream)
                {
                    string line = filereader.ReadLine();
                    var fields = line.Split(new char[] { separator });

                    //class label and doc contents
                    string classLabel = fields[0];
                    string docContent = fields[1];

                    Dictionary<string, double> docFeatures = new Dictionary<string, double>();
                    pipeline.ProcessDocument(docContent, docFeatures);
                    featureStatisticsDictBuilder.UpdateInfoStatisticsFromSingleDoc(docFeatures);
                }
            }

            //save fetures for later use
            string featuresDictFile = inputFile + ".feature";
            featureStatisticsDictBuilder.SaveToFile(featuresDictFile);

            //load features from file
            featureStatisticsDictBuilder.LoadFromFile(featuresDictFile);
            
            
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
    }
}
