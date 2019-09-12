using System;
using System.IO;
using System.Text;
using McMaster.Extensions.CommandLineUtils;

namespace GitInit
{
   [Command("commit-msg")]
   public class CommitMsgHook
   {
      [Argument(0)]
      public string Directory { get; set; }

      [Option]
      public string Template { get; set; }

      [Option(CommandOptionType.NoValue)]
      public bool Force { get; set; }

      public int OnExecute()
      {
         if (!System.IO.Directory.Exists(Directory))
         {
            Console.WriteLine($"Directory '{Directory}' not found.");
            return 1;
         }

         var content = !string.IsNullOrWhiteSpace(Template)
            ? File.ReadAllText(Template)
            : GetDefault();

         var gitDirectories = System.IO.Directory.GetDirectories(Directory, ".git", SearchOption.AllDirectories);

         foreach (var gitDirectory in gitDirectories)
         {
            foreach (var hooksDirectory in System.IO.Directory.GetDirectories(gitDirectory, "hooks", SearchOption.TopDirectoryOnly))
            {
               var path = Path.Combine(hooksDirectory, "commit-msg");

               if (!Force && File.Exists(path))
                  continue;

               File.WriteAllText(path, content);
            }
         }

         return 0;
      }

      private static string GetDefault()
      {
         var builder = new StringBuilder();

         builder.AppendLine("#!/bin/sh");
         builder.AppendLine();
         builder.AppendLine("pattern=\"(#[0-9]|#no-wi)\"");
         builder.AppendLine("message=\"Commit message must contain work item reference '#123' or '#no-wi'.\"");
         builder.AppendLine();
         builder.AppendLine("if ! grep -iqE \"$pattern\" \"$1\"; then");
         builder.AppendLine("    echo \"$message\" >&2");
         builder.AppendLine("    exit 1");
         builder.AppendLine("fi");

         return builder.ToString();
      }
   }
}