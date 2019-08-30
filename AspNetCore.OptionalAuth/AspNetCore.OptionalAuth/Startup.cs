using AspNetCore.OptionalAuth.Authentication.Foobar;
using AspNetCore.OptionalAuth.Authentication.Optional;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.OptionalAuth
{
   public class Startup
   {
      // This method gets called by the runtime. Use this method to add services to the container.
      // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
      public void ConfigureServices(IServiceCollection services)
      {
         services.AddAuthentication(options => options.DefaultScheme = FoobarAuthenticationDefaults.AuthenticationScheme)
            .AddFoobar()
            .AddOptional()
            ;

         services.AddMvc(config =>
         {
            var policy = new AuthorizationPolicyBuilder(FoobarAuthenticationDefaults.AuthenticationScheme).RequireAuthenticatedUser().Build();
            config.Filters.Add(new AuthorizeFilter(policy));
         }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
      }

      // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
      public void Configure(IApplicationBuilder app, IHostingEnvironment env)
      {
         if (env.IsDevelopment())
         {
            app.UseDeveloperExceptionPage();
         }

         app.UseAuthentication();

         app.UseMvc();
      }
   }
}
