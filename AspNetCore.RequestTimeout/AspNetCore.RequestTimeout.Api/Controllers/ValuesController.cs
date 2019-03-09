using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.RequestTimeout.Api.Controllers
{
   [Route("api/[controller]")]
   [ApiController]
   public class ValuesController : ControllerBase
   {
      public static List<string> Strings = new List<string>();

      [HttpGet]
      public ActionResult<IEnumerable<string>> Get()
      {
         return Strings;
      }

      [HttpPost("{value}"), Timeout(100)]
      public async Task Post(string value, CancellationToken cancellationToken, int delay = 0)
      {
         await Task.Delay(delay, cancellationToken);

         if (!cancellationToken.IsCancellationRequested)
            Strings.Add(value);
      }

      [HttpDelete]
      public void Delete()
      {
         Strings.Clear();
      }
   }
}
