using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightNlpWebApi.Client
{
    public class LightNlpWebApiRestClient

    {
        public const string AUTH_TOKEN = "AUTH_TOKEN";

        string _AuthKey;

        string _apiUrl;

        public LightNlpWebApiRestClient(string apiUrl)
            : this(apiUrl, string.Empty)
        { }


        public LightNlpWebApiRestClient(string apiUrl, string agentAuthKey)
        {
            _apiUrl = apiUrl;
            _AuthKey = agentAuthKey;
        }

        private void AddCredentialsToRestRequest(RestRequest request)
        {
            if (!string.IsNullOrEmpty(_AuthKey))
            {
                request.AddHeader(AUTH_TOKEN, _AuthKey);
            }
        }

        #region CategorizeTextDoc
        public void CategorizeTextDocAsync(Dictionary<string, string> activity, Action<IRestResponse<Dictionary<string, string>>> callback)
        {
            RestClient client = new RestClient(_apiUrl);
            var request = PrepareCategorizeTextDocRequest(activity);

            client.ExecuteAsync<Dictionary<string, string>>(request, callback);
        }

        public IRestResponse<Dictionary<string, string>> CategorizeTextDoc(Dictionary<string, string> activity)
        {
            RestClient client = new RestClient(_apiUrl);
            var request = PrepareCategorizeTextDocRequest(activity);

            return client.Execute<Dictionary<string, string>>(request);
        }

        private RestRequest PrepareCategorizeTextDocRequest(Dictionary<string, string> activity)
        {
            var request = new RestRequest(new Uri("/api/Services/CategorizeTextDoc", UriKind.Relative), Method.POST);

            request.RequestFormat = DataFormat.Json;
            request.AddBody(activity);
            AddCredentialsToRestRequest(request);

            return request;
        }
        #endregion

        #region CategorizeTextDoc
        public void CategorizeTextDocMultiAsync(List<Dictionary<string, string>> activity, Action<IRestResponse<List<Dictionary<string, string>>>> callback)
        {
            RestClient client = new RestClient(_apiUrl);
            var request = PrepareCategorizeTextDocMultiRequest(activity);

            client.ExecuteAsync<List<Dictionary<string, string>>>(request, callback);
        }

        public IRestResponse<List<Dictionary<string, string>>> CategorizeTextDocMulti(List<Dictionary<string, string>> docs)
        {
            RestClient client = new RestClient(_apiUrl);
            var request = PrepareCategorizeTextDocMultiRequest(docs);

            return client.Execute<List<Dictionary<string, string>>>(request);
        }

        private RestRequest PrepareCategorizeTextDocMultiRequest(List<Dictionary<string, string>> activity)
        {
            var request = new RestRequest(new Uri("/api/Services/CategorizeTextDocMulti", UriKind.Relative), Method.POST);

            request.RequestFormat = DataFormat.Json;
            request.AddBody(activity);
            AddCredentialsToRestRequest(request);

            return request;
        }

        #endregion
    }
}
