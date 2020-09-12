using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace _20200912_IsOfTypeBenchmarks
{
   [MemoryDiagnoser]
   public class Program
   {
      public static void Main() => BenchmarkRunner.Run<Program>();

      [ParamsSource(nameof(GetSystemUnderTest))]
      public WithoutCaching SystemUnderTest { get; set; }

      public IEnumerable<WithoutCaching> GetSystemUnderTest => new WithoutCaching[] { new WithoutCaching(), new WithCaching() };

      [Benchmark]
      [Arguments(typeof(IDontHaveAttribute))]
      [Arguments(typeof(IHaveAttribute))]
      public void Attribute(Type type)
      {
         SystemUnderTest.IsClassOrHasAttribute(type);
      }

      [Benchmark]
      [Arguments(typeof(IsClass))]
      [Arguments(typeof(IsNotClass))]
      public void Class(Type type) { }

      [Benchmark]
      [Arguments(typeof(IDontImplementInterface))]
      [Arguments(typeof(IImplementInterface))]
      public void Interface(Type type)
      {
         SystemUnderTest.IsClassOrInterface(type);
      }

      public interface IMarker { }

      public interface IDontImplementInterface : IMarker { }

      public interface IImplementInterface : IMarker { }

      [AttributeUsage(AttributeTargets.Interface)]
      public class MarkerAttribute : Attribute { }

      [Marker]
      public interface IHaveAttribute { }

      public interface IDontHaveAttribute { }

      public class Marker { }

      public class IsClass : Marker { }

      public class IsNotClass { }

      public class WithCaching : WithoutCaching
      {
         private readonly Dictionary<Type, bool> _cache = new Dictionary<Type, bool>();

         public override bool IsClassOrHasAttribute(Type type)
         {
            if (!_cache.TryGetValue(type, out var result))
            {
               result = _cache[type] = base.IsClassOrHasAttribute(type);
            }

            return result;
         }

         public override bool IsClassOrInterface(Type type)
         {
            if (!_cache.TryGetValue(type, out var result))
            {
               result = _cache[type] = base.IsClassOrInterface(type);
            }

            return result;
         }
      }

      public class WithoutCaching
      {
         public virtual bool IsClassOrHasAttribute(Type type)
         {
            return type.IsClass
               ? IsClass(type)
               : HasAttribute(type);
         }

         public virtual bool IsClassOrInterface(Type type)
         {
            return type.IsClass
               ? IsClass(type)
               : IsInterface(type);
         }

         private static bool IsClass(Type @class)
         {
            do
            {
               if (@class == typeof(Marker))
                  return true;

               @class = @class.BaseType;
            } while (@class != null);

            return false;
         }

         private static bool HasAttribute(Type @interface)
         {
            return @interface.GetCustomAttribute<MarkerAttribute>() != null;
         }

         private static bool IsInterface(Type @interface)
         {
            foreach (var i in @interface.GetInterfaces())
            {
               if (i == typeof(IMarker))
                  return true;
            }

            return false;
         }
      }
   }
}
