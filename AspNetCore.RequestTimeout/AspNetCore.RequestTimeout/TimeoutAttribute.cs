using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.RequestTimeout
{
   [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
   public class TimeoutAttribute : ActionFilterAttribute
   {
      private readonly int _milliseconds;

      public TimeoutAttribute(int milliseconds)
      {
         _milliseconds = milliseconds;
      }

      public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
      {
         using (var cts = new CancellationTokenSource())
         {
            var timeoutTask = Task.Delay(_milliseconds, cts.Token);
            var workTask = next();

            await Task.WhenAny(timeoutTask, workTask);

            if (workTask.IsCompleted)
            {
               cts.Cancel();
            }
            else
            {
               // todo cancel request processing
               context.Result = new StatusCodeResult(408);
            }
         }
      }
   }
}
