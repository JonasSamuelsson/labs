using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AspNetCore.EtagParameterBinding.Controllers
{
   [Route("api")]
   [ApiController]
   public class ApiController : ControllerBase
   {
      [HttpGet]
      public object Get([Required] string etag)
      {
         return new { etag };
      }
   }
}
