using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace AspNetCore.Routing.EndpointSelection
{
   public class Startup
   {
      public Startup(IConfiguration configuration)
      {
         Configuration = configuration;
      }

      public IConfiguration Configuration { get; }

      // This method gets called by the runtime. Use this method to add services to the container.
      public void ConfigureServices(IServiceCollection services)
      {
         services.AddSingleton<IActionDescriptorProvider, CustomActionDescriptorProvider>();
         services.AddSingleton<MatcherPolicy, CustomEndpointSelector>();
         services.AddControllers(options => options.ModelBinderProviders.Insert(0, new MyModelBinderProvider()));
      }

      // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
      public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
      {
         if (env.IsDevelopment())
         {
            app.UseDeveloperExceptionPage();
         }

         app.UseHttpsRedirection();

         app.UseRouting();

         app.UseAuthorization();

         app.UseEndpoints(endpoints =>
         {
            endpoints.MapControllers();
         });
      }
   }

   public class CustomActionDescriptorProvider : IActionDescriptorProvider
   {
      public void OnProvidersExecuted(ActionDescriptorProviderContext context)
      {
         
      }

      public void OnProvidersExecuting(ActionDescriptorProviderContext context)
      {
         foreach (var actionDescriptor in context.Results)
         {
            var va = actionDescriptor.EndpointMetadata.OfType<VersionAttribute>().SingleOrDefault();
         }
      }

      public int Order { get; }
   }

   public class MyModelBinderProvider : IModelBinderProvider
   {
      public IModelBinder GetBinder(ModelBinderProviderContext context)
      {
         return null;

         if (context.Metadata.ModelType != typeof(string))
            return null;

         return new ModelBinder(context.Metadata.Name);
      }
   }

   public class ModelBinder : IModelBinder
   {
      private readonly string _name;

      public ModelBinder(string name)
      {
         _name = name;
      }

      public Task BindModelAsync(ModelBindingContext bindingContext)
      {
         var headers = bindingContext.HttpContext.Request.Headers;

         if (headers.TryGetValue(_name, out var values))
         {
            var value = values.ToString();

            if (value.StartsWith("W/"))
            {
               bindingContext.Result = ModelBindingResult.Success(value);
            }
            else
            {
               bindingContext.ModelState.AddModelError($"{_name} header", "Invalid ETag format.");
               bindingContext.Result = ModelBindingResult.Failed();
            }
         }

         return Task.CompletedTask;
      }
   }
}
