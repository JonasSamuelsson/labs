using System;
using System.Collections.Generic;

namespace AspNetCore.ApiVersioning
{
   public class SemVerApiVersionValidationStrategy : IApiVersionValidationStrategy
   {
      public bool Validate(string apiVersion, bool optional, IReadOnlyList<string> validApiVersions, out string matchedVersion,
         out string error)
      {
         error = null;

         if (Validate(apiVersion, optional, validApiVersions, out matchedVersion))
            return true;

         error = validApiVersions.Count == 1
            ? $"invalid api version, valid version is <= {validApiVersions[0]}"
            : $"invalid api version, valid versions are <= {string.Join(", ", validApiVersions)}";

         return false;
      }

      private static bool Validate(string apiVersion, bool optional, IReadOnlyList<string> validApiVersions,
         out string matchedVersion)
      {
         matchedVersion = null;

         if (apiVersion == null)
            return optional;

         if (!Version.TryParse(apiVersion, out var version))
            return false;

         // ReSharper disable once ForCanBeConvertedToForeach
         for (var i = 0; i < validApiVersions.Count; i++)
         {
            var validApiVersion = validApiVersions[i];

            if (!Version.TryParse(validApiVersion, out var validVersion))
               throw new FormatException($"invalid version format '{validVersion}'");

            if (!version.Matches(validVersion))
               continue;

            matchedVersion = validApiVersion;
            return true;
         }

         return false;
      }
   }
}