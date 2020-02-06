using System;

namespace AspNetCore.Routing.EndpointSelection
{
   public class VersionAttribute : Attribute
   {
      public VersionAttribute(string version)
      {
         Version = version;
      }

      public string Version { get; }
   }
}