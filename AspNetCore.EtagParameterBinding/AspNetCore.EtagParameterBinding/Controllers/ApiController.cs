using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.EtagParameterBinding.Controllers
{
   [Route("api")]
   [ApiController]
   public class ApiController : ControllerBase
   {
      [HttpGet]
      public object Get([Required]string ifMatch, string ifNoneMatch)
      {
         Response.SetETagHeader(ifNoneMatch);
         return new { ifMatch, ifNoneMatch };
      }

      [HttpPost]
      public void Post([Required][FromHeader(Name = "xyz")]string foobar) { }
   }
}
