﻿using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.ApiVersioning.WebApp.Controllers
{
   [Route("api/[controller]")]
   [ApiController]
   public class ValuesController : ControllerBase
   {
      [HttpGet("1"), ApiVersion(new[] { "1.1" }, Optional = true, ValidationStrategy = typeof(SemVerApiVersionValidationStrategy))]
      public string Get1(string apiVersion)
      {
         return apiVersion;
      }
   }
}
