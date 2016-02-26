
using System;
using System.Collections.Generic;

namespace LightNlpWebApi.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            string authToken = "<token if authorization is enabled>";
            string serviceUrl = "http://localhost:8080";
            LightNlpWebApiRestClient client = new LightNlpWebApiRestClient(serviceUrl);

            //Single doc categorization
            Dictionary<string, string> textDoc = new Dictionary<string, string>()
            {
                { "Id", "1" },
                { "Text", "Има огромна дупка на улицата!!!" }
            };

            var result = client.CategorizeTextDoc(textDoc);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var categorizedDoc = result.Data;
                Console.WriteLine("{0} - {1}", categorizedDoc["Label"], categorizedDoc["LabelName"]);
            }

            //multi docs
            List<Dictionary<string, string>> textDocs = new List<Dictionary<string, string>>()
            { new Dictionary<string, string>()
            {
                { "Id", "1" },
                { "Text", "Има огромна дупка на улицата!!!" }
            },
            new Dictionary<string, string>()
            {
                { "Id", "2" },
                { "Text", "Счупена люлка на детска площадка!!!" }
            },
            };

            var resultList = client.CategorizeTextDocMulti(textDocs);
            if (resultList.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var categorizedDocs = resultList.Data;
                foreach (var catDoc in categorizedDocs)
                {
                    Console.WriteLine("[{2}]{0} - {1}", catDoc["Label"], catDoc["LabelName"], catDoc["Id"]);
                }

            }
        }
    }
}

