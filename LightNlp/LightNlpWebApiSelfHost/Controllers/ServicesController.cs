using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net.Http;
using LightNlpWebApiSelfHost.Models;

// Add these usings:
using System.Data.Entity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web.Http.Results;

namespace LightNlpWebApiSelfHost.Controllers
{
    public class ServicesController : ApiController
    {
        ApplicationDbContext dbContext = new ApplicationDbContext();


        [HttpGet]
        public JsonResult<Dictionary<string, string>> Categorize(string jsonContent)
        {
            Dictionary<string, string> docValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonContent);
            string text = docValues.ContainsKey("Text")?docValues["Text"]: docValues["text"];

            docValues["LabelName"] = "troll";
            docValues["Label"] = (1.0).ToString();
            docValues["Confidence"] = (0.9).ToString();

            return Json(docValues);

            //return new ClassificationResult()
            //{
            //    Text  = text,
            //    LabelName = "troll",
            //    Label = 1.0,
            //    Confidence = 0.0,
            //};
        }

        //public IEnumerable<PostData> Get()
        //{
        //    return null;
        //}
        //public IEnumerable<PostData> Get()
        //{
        //    return null;
        //}


        //public async Task<PostData> Get(int id)
        //{
        //    return null;
        //}


        //[HttpPost]
        //public async Task<IHttpActionResult> Post(PostData postData)
        //{
        //    return Ok();
        //}


        //public async Task<IHttpActionResult> Put(PostData postData)
        //{
        //    return Ok();
        //}


        //public async Task<IHttpActionResult> Delete(int id)
        //{
        //    return Ok();
        //}
    }
}
