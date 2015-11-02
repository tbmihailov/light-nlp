using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LightNlp.Tools.Helpers
{
    public class LexiconReaderHelper
    {
        public static List<string> LoadWordsFromFile(string fileName)
        {
            List<string> words = new List<string>();
            string line;
            using (TextReader textReader = new StreamReader(fileName))
            {
                while ((line = textReader.ReadLine()) != null)
                {
                    words.Add(line.Trim());
                }
            }

            return words;
        }

        public static void SaveDictionaryToFile(Dictionary<string, int> dict, string fileName)
        {
            using (TextWriter textWriter = new StreamWriter(fileName))
            {
                foreach (var item in dict)
                {
                    textWriter.WriteLine("{0}\t{1}", item.Key, item.Value);
                }
            }
        }

        public static Dictionary<string, int> LoadDictionaryFromFile(string fileName)
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            string line;
            using (TextReader textReader = new StreamReader(fileName))
            {
                while ((line = textReader.ReadLine()) != null)
                {
                    var items = line.Split('\t');
                    dict.Add(items[0], int.Parse(items[1]));
                }
            }

            return dict;
        }
    }
}
