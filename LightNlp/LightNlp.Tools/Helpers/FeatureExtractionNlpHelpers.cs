using BulStem;
using LibSvmHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LightNlp.Core.Helpers;
using LightNlp.Core.Modules;

namespace LightNlp.Tools.Helpers
{
    public class FeatureExtractionNlpHelpers
    {
        public static void ExtractDnevnikEmoticonsFeaturesAndUpdateItemFeatures(string commentText, Dictionary<string, double> item)
        {
            var matches = Regex.Matches(commentText, @"\[emo-[^\]]*\]");
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    string emoticon = match.Value.Replace("[", "").Replace("]", "").Replace("-", "_");
                    item.SetFeatureValue("emo_" + emoticon, 1);
                }

            }
        }

        public static void TokenizeAndAppendAnnotations(string text, List<Annotation> annotations)
        {
            string type = "Word";

            Regex regex = new Regex(@"([#\w]+)");
            var matches = regex.Matches(text);
            foreach (Match match in matches)
            {
                if (match.Groups.Count > 1)
                {
                    for (int i = 1; i <= match.Groups.Count; i++)
                    {
                        var annotation = new Annotation();
                        var group = match.Groups[i];

                        if (group.Length == 0)
                        {
                            continue;
                        }
                        annotation.Text = group.Value;
                        annotation.Type = type;
                        annotation.FromIndex = group.Index;
                        annotation.ToIndex = group.Index + group.Length - 1;
                        annotations.Add(annotation);
                    }
                }
            }
        }

        public static void TokenizeBySplittingAndAppendAnnotations(string text, List<Annotation> annotations)
        {
            string type = "Word";

            //Regex regex = new Regex(@"([#\w]+)");
            var matches = Tokenize(text);
            foreach (var match in matches)
            {

                var annotation = new Annotation();
                
                annotation.Text = match;
                annotation.Type = type;
                annotation.FromIndex = 0;
                annotation.ToIndex = 0;
                annotations.Add(annotation);

            }
        }

        public static void ExtractWord3gramFeaturesFromTextTokensAndUpdateItemFeatures(Dictionary<string, double> item, List<string> commentTokens)
        {
            string prefix = "word3gram";
            for (int i = 0; i < commentTokens.Count - 3; i++)
            {
                string ngramToken = string.Format("{0}_{1}_{2}_{3}", prefix, commentTokens[i], commentTokens[i + 1], commentTokens[i + 2]);
                item.IncreaseFeatureFrequency(ngramToken, 1);
            }
        }

        public static void ExtractWord2gramFeaturesFromTextTokensAndUpdateItemFeatures(Dictionary<string, double> item, List<string> commentTokens)
        {
            string prefix = "word2gram";
            for (int i = 0; i < commentTokens.Count - 2; i++)
            {
                string ngramToken = string.Format("{0}_{1}_{2}", prefix, commentTokens[i], commentTokens[i + 1]);
                item.IncreaseFeatureFrequency(ngramToken, 1);
            }
        }

        public static void ExtractWordNGramFeaturesFromTextTokensAndUpdateItemFeatures(Dictionary<string, double> item, List<string> commentTokens, int ngramLength)
        {
            string prefix = string.Format("word{0}gram", ngramLength);
            if (commentTokens.Count < ngramLength)
            {
                return;
            }

            for (int i = 0; i < commentTokens.Count - ngramLength; i++)
            {
                StringBuilder sbNgramToken = new StringBuilder();
                sbNgramToken.AppendFormat("{0}_{1}", prefix, commentTokens[i]);
                for (int j = 1; j < ngramLength; j++)
                {
                    sbNgramToken.AppendFormat("_{0}", commentTokens[i + j]);
                }
                item.IncreaseFeatureFrequency(sbNgramToken.ToString(), 1);
            }
        }

        public static void ExtractCharNgramFeaturesFromSingleTokenAndUpdateItemFeatures(Dictionary<string, double> item, string tokenKey, int ngramLength)
        {
            List<string> ngramValues = GetCharNgramsFromWord(tokenKey, ngramLength);
            if (ngramValues != null)
            {
                foreach (var ngramVal in ngramValues)
                {
                    if (!string.IsNullOrWhiteSpace(ngramVal))
                    {
                        item.IncreaseFeatureFrequency(string.Format("ngram{0}_{1}", ngramLength, ngramVal), 1);
                    }
                }
            }
        }

        public static void ExtractPrefixFeatureFromSingleTokenAndUpdateItemFeatures(Dictionary<string, double> item, string tokenKey, int ngramLength)
        {
            string ngramVal = GetWordPrefix(tokenKey, ngramLength);
            if (!string.IsNullOrWhiteSpace(ngramVal))
            {
                item.IncreaseFeatureFrequency(string.Format("npref{0}_{1}", ngramLength, ngramVal), 1);
            }
        }

        public static void ExtractSuffixFeatureFromSingleTokenAndUpdateItemFeatures(Dictionary<string, double> item, string tokenKey, int ngramLength)
        {
            string ngramVal = GetWordSuffix(tokenKey, ngramLength);
            if (!string.IsNullOrWhiteSpace(ngramVal))
            {
                item.IncreaseFeatureFrequency(string.Format("nsuff{0}_{1}", ngramLength, ngramVal), 1);
            }
        }

        public static void ExtractBagOfWordFeatureFromSingleTokenAndUpdateItemFeatures(Dictionary<string, double> item, string tokenKey)
        {
            item.IncreaseFeatureFrequency(string.Format("bow_{0}", tokenKey), 1);
        }

        public static void ExtractTextPunctuationFeaturesAndUpdateItemFeatures(string commentText, Dictionary<string, double> item)
        {
            SetFeatureToRegExMatchesCount("!{1}", "punct_singl_exclam", commentText, item);
            SetFeatureToRegExMatchesCount("!{2,}", "punct_multi_exclam", commentText, item);

            SetFeatureToRegExMatchesCount("\\?{1}", "punct_singl_question", commentText, item);
            SetFeatureToRegExMatchesCount("\\?{2,}", "punct_multi_question", commentText, item);

            SetFeatureToRegExMatchesCount("\\.", "punct_singl_dot", commentText, item);
            SetFeatureToRegExMatchesCount("\\.{2,}", "punct_multi_dot", commentText, item);

            SetFeatureToRegExMatchesCount("\\w", "stat_words_count", commentText, item);

            SetFeatureToRegExMatchesCount(@"\[emo-[^\]]*\]", "stat_emoticons_all", commentText, item);

            SetFeatureToRegExMatchesCount("[А-Я]{3,}", "stat_allcaps_words_count", commentText, item);
        }

        public static string ExtractStemFeatureFromSingleTokenAndUpdateItemFeatures(Stemmer stemmer, Dictionary<string, double> item, string tokenKey)
        {
            tokenKey = stemmer.Stem(tokenKey);
            item.IncreaseFeatureFrequency("stem_" + tokenKey, 1);
            return tokenKey;
        }

        public static List<string> GetCharNgramsFromWord(string wordToken, int charsCnt)
        {
            if (string.IsNullOrEmpty(wordToken)
                  || charsCnt < 1)
            {
                return null;
            }

            if (wordToken.Length <= charsCnt)
            {
                return null;
            }

            var ngramsList = new List<string>();
            for (int i = 0; i < wordToken.Length - charsCnt + 1; i++)
            {
                ngramsList.Add(wordToken.Substring(i, charsCnt));
            }

            return ngramsList;
        }

        public static string GetWordPrefix(string wordToken, int charsCnt)
        {
            if (string.IsNullOrEmpty(wordToken)
                   || charsCnt < 1)
            {
                return string.Empty;
            }

            if (wordToken.Length <= charsCnt)
            {
                return string.Empty;
            }

            return wordToken.Substring(0, charsCnt);
        }

        public static string GetWordSuffix(string wordToken, int charsCnt)
        {
            if (string.IsNullOrEmpty(wordToken)
                   || charsCnt < 1)
            {
                return string.Empty;
            }

            if (wordToken.Length <= charsCnt)
            {
                return string.Empty;
            }

            return wordToken.Substring(wordToken.Length - charsCnt, charsCnt);
        }

        public static List<string> Tokenize(string commentText)
        {
            List<string> commentTokens = commentText.ToLower().Split(new char[] { ',', ' ', ';', ':', '\t', '\r', '\n', '(', ')', '?', '.', '!' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            return commentTokens;
        }

        public static void SetFeatureToRegExMatchesCount(string pattern, string featureKey, string commentText, Dictionary<string, double> item)
        {
            Regex regEx = new Regex(pattern);
            var matches = regEx.IsMatch(commentText) ? regEx.Matches(commentText) : null;
            if (matches == null)
            {
                item.SetFeatureValue(featureKey, 0);
                return;
            }

            int numberOfMatches = matches.Count;
            item.SetFeatureValue(featureKey, numberOfMatches * 1.0);
        }

    }
}
