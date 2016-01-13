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

namespace LightNlpWebApiSelfHost.Controllers
{
    public class ServicesController : ApiController
    {
        ApplicationDbContext dbContext = new ApplicationDbContext();

        public IEnumerable<PostData> Get()
        {
            return null;
        }


        public async Task<PostData> Get(int id)
        {
            return null;
        }


        [HttpPost]
        public async Task<IHttpActionResult> Post(PostData postData)
        {
            return Ok();
        }


        public async Task<IHttpActionResult> Put(PostData postData)
        {
            return Ok();
        }


        public async Task<IHttpActionResult> Delete(int id)
        {
            return Ok();
        }
    }
}
