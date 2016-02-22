using BulStem;
using LightNlp.Core;
using LightNlp.Core.Helpers;
using LightNlp.Core.Modules;
using LightNlp.Tools.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightNlpWebApiSelfHost
{
    public class PipelineConfiguration
    {
        public static List<FeatureExtractionModule> GetExtractionModules()
        {
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

            modules.Add(new ActionFeatureExtractionModule("annotate_words_troll_sentence", (text, features, annotations) =>
            {
                List<Annotation> annotationsAll = new List<Annotation>();

                string txt = text.Replace(",", " , ").Replace(";", " ; ").Replace("?", " ? ");
                FeatureExtractionNlpHelpers.TokenizeAndAppendAnnotationsRegEx(txt, annotationsAll, @"([#\w,;?-]+)");

                var annotationsTroll = annotationsAll.Where(a => a.Text.ToLower().StartsWith("трол") || a.Text.ToLower().StartsWith("мурзи")).ToList();

                var annotationsTrollWindow = new List<Annotation>();

                int wordScope = 2;
                foreach (var ann in annotationsTroll)
                {
                    int trollIndex = annotationsAll.IndexOf(ann);
                    for (int i = Math.Max((trollIndex - wordScope), 0); i < Math.Min((trollIndex + wordScope), annotationsAll.Count); i++)
                    {
                        var annToAdd = annotationsAll[i];
                        if (!annotationsTrollWindow.Contains(annToAdd))
                        {
                            annotationsTrollWindow.Add(annToAdd);
                        }
                    }
                }

                foreach (var ann in annotationsTrollWindow)
                {
                    annotations.Add(ann);
                }

                Console.WriteLine(string.Join(" ", annotations.Select(a => a.Text)));
                var wordAnnotations = annotations.Where(a => a.Type == "Word").ToList();
                foreach (var wordAnn in wordAnnotations)
                {
                    var loweredWordAnnotation = new Annotation() { Type = "Word_Lowered", Text = wordAnn.Text.ToLower(), FromIndex = wordAnn.FromIndex, ToIndex = wordAnn.ToIndex };
                    annotations.Add(loweredWordAnnotation);
                }
            }));

            modules.Add(new ActionFeatureExtractionModule("annotate_words_troll_words", (text, features, annotations) =>
            {
                List<Annotation> annotationsAll = new List<Annotation>();

                string txt = text.Replace(",", " , ").Replace(";", " ; ").Replace("?", " ? ");
                FeatureExtractionNlpHelpers.TokenizeAndAppendAnnotationsRegEx(txt, annotationsAll, @"([#\w,;?-]+)");

                List<string> allowedWords = new List<string>()
                {
                    "аз","ти","той","тя","то","ние","вие","те",
                    "съм","си","е","сте","са","сме",
                    "ми","ни","ви","им","му",
                    "нас","вас","тях",
                    "ги", "го", "я"
                    
                    //"коя","кое","кои","кой",
                };
                annotationsAll = annotationsAll.Where(a => a.Text.ToLower().StartsWith("трол") || a.Text.ToLower().StartsWith("мурзи") || allowedWords.Contains(a.Text.ToLower())).ToList();

                foreach (var ann in annotationsAll)
                {
                    annotations.Add(ann);
                }

                Debug.WriteLine(string.Join(" ", annotations.Select(a => a.Text)));
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

            //Doc starts with
            modules.Add(new ActionFeatureExtractionModule("doc_start", (text, features, annotations) =>
            {
                if (text.Length < 3)
                {
                    return;
                }
                FeaturesDictionaryHelpers.IncreaseFeatureFrequency(features, "doc_starts_3_" + text.Substring(0, 3), 1.0);

                if (text.Length < 4)
                {
                    return;
                }
                FeaturesDictionaryHelpers.IncreaseFeatureFrequency(features, "doc_starts_4_" + text.Substring(0, 4), 1.0);

                if (text.Length < 5)
                {
                    return;
                }
                FeaturesDictionaryHelpers.IncreaseFeatureFrequency(features, "doc_starts_5_" + text.Substring(0, 5), 1.0);
            }));

            //Doc ends with
            modules.Add(new ActionFeatureExtractionModule("doc_end", (text, features, annotations) =>
            {
                if (text.Length < 3)
                {
                    return;
                }
                FeaturesDictionaryHelpers.IncreaseFeatureFrequency(features, "doc_ends_3_" + text.Substring(text.Length - 3, 3), 1.0);
                if (text.Length < 4)
                {
                    return;
                }
                FeaturesDictionaryHelpers.IncreaseFeatureFrequency(features, "doc_ends_4_" + text.Substring(text.Length - 4, 4), 1.0);
                if (text.Length < 5)
                {
                    return;
                }
                FeaturesDictionaryHelpers.IncreaseFeatureFrequency(features, "doc_ends_5_" + text.Substring(text.Length - 5, 5), 1.0);
            }));

            //custom module
            //modules.Add(new ActionFeatureExtractionModule("", (text, features, annotations) => {

            //}));
            return modules;
        }

        public static FeatureExtractionPipeline BuildPipeline(string modulesConfig, List<FeatureExtractionModule> modules)
        {
            Debug.WriteLine("Module configurations:");

            var moduleNamesToConfigure = modulesConfig.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var moduleName in moduleNamesToConfigure)
            {
                Console.WriteLine(moduleName);
            }
            Debug.WriteLine("");

            var modulesToRegister = modules.Where(m => moduleNamesToConfigure.Contains(m.Name)).ToList();

            FeatureExtractionPipeline pipeline = new FeatureExtractionPipeline();

            foreach (var module in modulesToRegister)
            {
                pipeline.RegisterModule(module);
            }

            return pipeline;
        }
    }
}
