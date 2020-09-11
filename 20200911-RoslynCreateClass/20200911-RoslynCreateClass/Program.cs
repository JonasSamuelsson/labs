using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace _20200911_RoslynCreateClass
{
   public class Program
   {
      public static void Main()
      {
         Process(typeof(IBar));
         Process(typeof(IFoo));
      }

      private static void Process(Type @interface)
      {
         Console.WriteLine(@interface.FullName);
         var @class = CreateClass(@interface);
         var instance = Activator.CreateInstance(@class);
         var json = JsonConvert.SerializeObject(instance, Formatting.Indented);
         Console.WriteLine(json);
         Console.WriteLine();
      }

      private static Type CreateClass(Type @interface)
      {
         var code = GetCode(@interface, "GeneratedCode", $"{@interface.Name}Implementation");
         var syntaxTree = CSharpSyntaxTree.ParseText(code);
         var references = GetMetadataReferences(@interface);
         var compilation = GetCompilation(syntaxTree, references);
         var assembly = GetAssembly(compilation);
         return assembly.GetTypes().Single();
      }

      private static string GetCode(Type @interface, string @namespace, string className)
      {
         var builder = new StringBuilder();

         builder.AppendLine($"namespace {@namespace}");
         builder.AppendLine($"{{");
         builder.AppendLine($"  public class {className} : {@interface.FullName}");
         builder.AppendLine($"  {{");

         foreach (var property in @interface.GetProperties())
         {
            builder.AppendLine($"    public {property.PropertyType.FullName} {property.Name} {{ get; set; }}");
         }

         builder.AppendLine($"  }}");
         builder.AppendLine($"}}");

         return builder.ToString();
      }

      private static IEnumerable<MetadataReference> GetMetadataReferences(Type @interface)
      {
         var assemblies = new HashSet<Assembly>();

         var dir = Path.GetDirectoryName(typeof(object).Assembly.Location);

         foreach (var dll in new[] { "mscorlib.dll", "System.dll", "System.Core.dll", "System.Runtime.dll" })
         {
            // these assemblies needs to be referenced manually, as per https://stackoverflow.com/a/47196516

            var path = Path.Combine(dir, dll);

            if (!File.Exists(path))
               continue;

            assemblies.Add(Assembly.LoadFile(path));
         }

         assemblies.Add(@interface.Assembly);

         foreach (var property in @interface.GetProperties())
         {
            assemblies.Add(property.PropertyType.Assembly);
         }

         return assemblies
            .Select(x => MetadataReference.CreateFromFile(x.Location))
            .ToList();
      }

      private static CSharpCompilation GetCompilation(SyntaxTree syntaxTree, IEnumerable<MetadataReference> references)
      {
         return CSharpCompilation.Create(Path.GetRandomFileName(),
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
      }

      private static Assembly GetAssembly(CSharpCompilation compilation)
      {
         using (var stream = new MemoryStream())
         {
            var result = compilation.Emit(stream);

            if (!result.Success)
            {
               throw new InvalidOperationException();
            }

            stream.Seek(0, SeekOrigin.Begin);
            return Assembly.Load(stream.ToArray());
         }
      }
   }

   public interface IFoo
   {
      bool Flag { get; set; }
      int Number { get; set; }
      string Text { get; set; }
   }

   public interface IBar
   {
      Guid Id { get; set; }
   }
}
