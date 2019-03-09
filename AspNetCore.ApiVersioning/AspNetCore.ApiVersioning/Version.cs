using System.Text.RegularExpressions;

namespace AspNetCore.ApiVersioning
{
   internal class Version
   {
      private const string Pattern = @"^(?<major>\d+)(\.(?<minor>\d+)(-(?<preRelease>.+))?)?$";

      public string Major { get; set; }
      public long Minor { get; set; }
      public string PreRelease { get; set; }

      public bool IsPreRelease => PreRelease != null;

      public bool Matches(Version version)
      {
         if (Major != version.Major)
            return false;

         if (IsPreRelease || version.IsPreRelease)
            return Minor == version.Minor && PreRelease == version.PreRelease;

         return Minor <= version.Minor;
      }

      public static bool TryParse(string s, out Version version)
      {
         var match = Regex.Match(s, Pattern, RegexOptions.Compiled);

         if (!match.Success)
         {
            version = null;
            return false;
         }

         var minor = match.Groups["minor"];
         var preRelease = match.Groups["preRelease"];

         version = new Version
         {
            Major = match.Groups["major"].Value,
            Minor = minor.Success ? long.Parse(minor.Value) : 0,
            PreRelease = preRelease.Success ? preRelease.Value : null
         };

         return true;
      }
   }
}