using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.OptionalAuth.Controllers
{
   [ApiController, Route("api")]
   public class ApiController : ControllerBase
   {
      [HttpGet]
      public string Get() => "success";
   }
}