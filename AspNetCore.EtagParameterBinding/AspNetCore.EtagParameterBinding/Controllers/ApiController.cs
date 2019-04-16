using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.EtagParameterBinding.Controllers
{
   [Route("api")]
   [ApiController]
   public class ApiController : ControllerBase
   {
      [HttpGet]
      public object Get(string etag)
      {
         return etag;
      }
   }
}
