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

         foreach (var item in data.WorkItems.OrderBy(x => x.Id))
         {
            var attributes = new Dictionary<string, string>();

            var segments = new[] { (object)$"{item.Type} {item.Id}", item.State, item.Tags.Join(", ") };
            var metadata = string.Join(" / ", segments.Select(x => x.ToString()).Where(x => !string.IsNullOrWhiteSpace(x)));
            var name = SanitizeLabel(item.Name);

            attributes["label"] = $"<<table border=\"0\"><tr><td>{metadata}</td></tr><tr><td>{name}</td></tr></table>>";
            attributes["shape"] = "box";
            attributes["style"] = item.IsOrigin ? "\"bold,filled,rounded\"" : "\"filled,rounded\"";
            attributes["fillcolor"] = item.IsFinished ? "transparent" : item.Type == WorkItemType.PBI ? "palegreen" : "lightskyblue";
            attributes["fontcolor"] = item.IsOrigin ? "black" : "gray25";
            attributes["fontsize"] = item.IsOrigin ? "16" : "14";
            builder.AppendLine($"  {item.Id} [{string.Join(" ", attributes.Select(x => $"{x.Key}={x.Value}"))}]");
         }

         foreach (var link in data.Links.OrderBy(x => x.Id))
         {
            var attributes = new Dictionary<string, string>();
            attributes["label"] = link.Type.ToString();
            builder.AppendLine($"  {link.FromWorkItemId} -> {link.ToWorkItemId} [{string.Join(" ", attributes.Select(x => $"{x.Key}={x.Value}"))}]");
         }

         builder.AppendLine("}");

         return builder.ToString();
      }

      private static string SanitizeLabel(string s) => s.Replace("\"", "").Replace("&", "and");
   }
}