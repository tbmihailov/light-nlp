using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LightNlp.Tools.Helpers;
using System.Collections.Generic;
using LightNlp.Core.Modules;
using System.Diagnostics;

namespace LightNlp.Tests
{
    [TestClass]
    public class FeatureExtraction_Tests
    {
        [TestMethod]
        public void Test_Token_Annotations()
        {
            var annotations = new List<Annotation>();
            string text = "Оставете правителството да работи! Please, leave the government to do it's job!";
            FeatureExtractionNlpHelpers.TokenizeAndAppendAnnotations(text, annotations);
            foreach (var annotation in annotations)
            {
                //var ann = new Annotation() { Type = "", Text = "", FromIndex = 1, ToIndex = 2 };
                Debug.WriteLine(string.Format("new Annotation() {{ Type = \"{0}\", Text = \"{1}\", FromIndex = {2}, ToIndex = {3} }},", annotation.Type, annotation.Text, annotation.FromIndex, annotation.ToIndex));
            }

        }

        [TestMethod]
        public void Test_GetWordNgrams()
        {
            string word = "трансформърс";
            int ngramLen = 3;
            List<string> exprectedNgrams = new List<string>(){
                "тра",
                "ран",
                "анс",
                "нсф",
                "сфо",
                "фор",
                "орм",
                "рмъ",
                "мър",
                "ърс",
            };

            var actualNgrams = FeatureExtractionNlpHelpers.GetCharNgramsFromWord(word, ngramLen);

            for (int i = 0; i < exprectedNgrams.Count; i++)
            {
                Assert.AreEqual(exprectedNgrams[i], actualNgrams[i]);
            }

        }

        [TestMethod]
        public void GetWordPrefix()
        {
            string word = "трансформърс";
            int ngramLen = 3;
            string exprected = "тра";

            var actual = FeatureExtractionNlpHelpers.GetWordPrefix(word, ngramLen);

            Assert.AreEqual(exprected, actual);
        }

        [TestMethod]
        public void GetWordSuffix()
        {
            string word = "трансформърс";
            int ngramLen = 4;
            string exprected = "мърс";

            var actual = FeatureExtractionNlpHelpers.GetWordSuffix(word, ngramLen);

            Assert.AreEqual(exprected, actual);
        }
    }
}
