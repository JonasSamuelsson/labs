using System.Collections.Generic;

namespace AspNetCore.ApiVersioning
{
   public interface IApiVersionValidationStrategy
   {
      bool Validate(string apiVersion, bool optional, IReadOnlyList<string> validApiVersions, out string matchedVersion, out string error);
   }
}