using McMaster.Extensions.CommandLineUtils;

namespace GitInit
{
   [Subcommand(typeof(CommitMsgHook))]
   public class Program
   {
      public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

      public void OnExecute(CommandLineApplication app) => app.ShowHelp();
   }
}
