using Microsoft.AspNetCore.Mvc;
using System;

namespace AspNetCore.Routing.EndpointSelection
{
   [ApiController, Route("api")]
   public class ApiController : ControllerBase
   {
      [HttpGet, Version("1")]
      public string Get1() => "1";

      [HttpGet, Version("2")]
      public string Get2() => "2";

      [HttpGet, Version("3'")]
      public string Get3() => "3";
   }
}
