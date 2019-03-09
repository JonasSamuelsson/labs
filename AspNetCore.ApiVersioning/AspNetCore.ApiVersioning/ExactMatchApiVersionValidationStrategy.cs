using System.Collections.Generic;

namespace AspNetCore.ApiVersioning
{
   public class ExactMatchApiVersionValidationStrategy : IApiVersionValidationStrategy
   {
      public bool Validate(string apiVersion, bool optional, IReadOnlyList<string> validApiVersions, out string matchedVersion,
         out string error)
      {
         error = null;

         if (Validate(apiVersion, optional, validApiVersions, out matchedVersion))
            return true;

         error = validApiVersions.Count == 1
            ? $"invalid api version, valid version is {validApiVersions[0]}"
            : $"invalid api version, valid versions are {string.Join(", ", validApiVersions)}";

         return false;
      }

      private static bool Validate(string apiVersion, bool optional, IReadOnlyList<string> validApiVersions,
         out string matchedVersion)
      {
         matchedVersion = null;

         if (apiVersion == null)
            return optional;

         // ReSharper disable once ForCanBeConvertedToForeach
         // ReSharper disable once LoopCanBeConvertedToQuery
         for (var i = 0; i < validApiVersions.Count; i++)
         {
            var validApiVersion = validApiVersions[i];

            if (apiVersion != validApiVersion)
               continue;

            matchedVersion = validApiVersion;
            return true;
         }

         return false;
      }
   }
}