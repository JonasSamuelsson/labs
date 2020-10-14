using _20201005_AzureDevOpsWorkItemVisualizer.Model;
using Handyman.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _20201005_AzureDevOpsWorkItemVisualizer
{
   public class GraphGenerator
   {
      public string GenerateGraph(Data data)
      {
         var builder = new StringBuilder();

         builder.AppendLine("digraph g {");

         builder.AppendLine("  rankdir = LR;");

         foreach (var item in data.Items.OrderBy(x => x.Id))
         {
            var attributes = new Dictionary<string, string>();

            var segments = new[] { (object)item.Id, item.Type, item.State, item.Tags.Join(", ") };
            var metadata = string.Join(" / ", segments.Select(x => x.ToString()).Where(x => !string.IsNullOrWhiteSpace(x)));
            attributes["label"] = $"<<table border=\"0\"><tr><td>{metadata}</td></tr><tr><td>{SanitizeLabel(item.Name)}</td></tr></table>>";
            attributes["style"] = "filled";
            if (!item.IsDone) attributes["fillcolor"] = item.Type == WorkItemType.PBI ? "palegreen" : "lightskyblue";
            builder.AppendLine($"  {item.Id} [{string.Join(" ", attributes.Select(x => $"{x.Key}={x.Value}"))}]");
         }

         foreach (var link in data.Links.OrderBy(x => x.Id))
         {
            var attributes = new Dictionary<string, string>();
            attributes["label"] = link.Type.ToString();
            builder.AppendLine($"  {link.From} -> {link.To} [{string.Join(" ", attributes.Select(x => $"{x.Key}={x.Value}"))}]");
         }

         builder.AppendLine("}");

         return builder.ToString();
      }

      private static string SanitizeLabel(string s) => s.Replace("\"", "").Replace("&", "and");
   }
}