using System.Text;
using Microsoft.CodeAnalysis;

namespace Arch.Unity.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public class SourceGenerator : IIncrementalGenerator
{
    private const int MaxParameters = 24;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(context.CompilationProvider, static (context, compilation) =>
        {
            if (compilation.AssemblyName != "Arch.Unity")
            {
                return;
            }

            context.AddSource("NativeQuery.g.cs", GenerateNativeQuery());
            context.AddSource("WorldExtensions.g.cs", GenerateWorldExtensions());
            context.AddSource("NativeQueryBuilder.g.cs", GenerateNativeQueryBuilder());
        });
    }

    private static string GenerateWorldExtensions()
    {
        var builder = new StringBuilder();

        builder.AppendLine(
            """
            using System.Runtime.CompilerServices;
            using Arch.Core;
            using Unity.Collections;

            namespace Arch.Unity.Queries
            {
                public static partial class WorldExtensions
                {
            """);

        for (var i = 0; i < MaxParameters; i++)
        {
            builder.Append($"       public static void Query<");
            for (var j = 0; j <= i; j++)
            {
                builder.Append($"T{j}");
                if (j < i) builder.Append(", ");
            }

            builder.Append($">(this World world, ForEach<");
            for (var j = 0; j <= i; j++)
            {
                builder.Append($"T{j}");
                if (j < i) builder.Append(", ");
            }

            builder.AppendLine("> forEach) =>");

            builder.AppendLine("            world.CreateNativeQueryBuilder(Allocator.Temp)");

            builder.Append($"               .WithAll<");
            for (var j = 0; j <= i; j++)
            {
                builder.Append($"T{j}");
                if (j < i) builder.Append(", ");
            }

            builder.AppendLine(">()");
            builder.AppendLine("               .Build()");
            builder.AppendLine("               .Query(forEach);");
        }

        builder.AppendLine(
            """
                }
            }
            """);
        return builder.ToString();
    }

    private static string GenerateNativeQuery()
    {
        var builder = new StringBuilder();
        builder.AppendLine(
            """
            using System.Runtime.CompilerServices;
            using Arch.Core;
            using Arch.Core.Utils;
            using Unity.Collections;
            using UnityEngine.Profiling;

            namespace Arch.Unity.Queries
            {
                public partial struct NativeQuery
                {
            """);

        for (var i = 0; i < MaxParameters; i++)
        {
            builder.Append($"        public void Query<");
            for (var j = 0; j <= i; j++)
            {
                builder.Append($"T{j}");
                if (j < i) builder.Append(", ");
            }

            builder.Append(">(ForEach<");

            for (var j = 0; j <= i; j++)
            {
                builder.Append($"T{j}");
                if (j < i) builder.Append(", ");
            }

            builder.AppendLine("> forEach)");

            builder.Append(
                """
                        {
                            var secondQuery = World.CreateNativeQueryBuilder().WithAll<
                """);

            for (var j = 0; j <= i; j++)
            {
                builder.Append($"T{j}");
                if (j < i) builder.Append(", ");
            }
            
            builder.AppendLine(">().Build();");
            builder.AppendLine("            var query = And(secondQuery);");

            builder.AppendLine(
                """
                            foreach (var archetype in query)
                            {
                                foreach (var chunk in archetype)
                                {
                """);

            for (var j = 0; j <= i; j++)
            {
                builder.AppendLine($"                   ref var t{j}FirstElement = ref chunk.GetFirst<T{j}>();");
            }

            builder.AppendLine(
                """
                                    foreach (var entityIndex in chunk)
                                    {
                """);

            for (var j = 0; j <= i; j++)
            {
                builder.AppendLine(
                    $"                       ref var t{j}Component = ref Unsafe.Add(ref t{j}FirstElement, entityIndex);");
            }

            builder.Append("                       forEach(");

            for (var j = 0; j <= i; j++)
            {
                builder.Append($"ref t{j}Component");
                if (j < i) builder.Append(", ");
            }

            builder.AppendLine(");");

            builder.AppendLine(
                """
                                    }
                                }
                            }
                        }
                """);
        }

        builder.AppendLine(
            """
                }
            }
            """);
        return builder.ToString();
    }

    private static string GenerateNativeQueryBuilder()
    {
        var builder = new StringBuilder();
        builder.AppendLine(
            """
            using System.Runtime.CompilerServices;
            using Arch.Core;
            using Arch.Core.Utils;
            using Unity.Collections;
            using UnityEngine.Profiling;

            namespace Arch.Unity.Queries
            {
                public partial struct NativeQueryBuilder
                {
            """);

        for (var i = 0; i < MaxParameters; i++)
        {
            builder.Append($"        public NativeQueryBuilder WithAll<");
            for (var j = 0; j <= i; j++)
            {
                builder.Append($"T{j}");
                if (j < i) builder.Append(", ");
            }

            builder.AppendLine(">()");
            builder.AppendLine("        {");
            for (var j = 0; j <= i; j++)
            {
                builder.AppendLine($"            _all.Add(Component<T{j}>.ComponentType);");
            }

            builder.AppendLine("            return this;");
            builder.AppendLine("        }");
        }

        builder.AppendLine(
            """
                }
            }
            """);
        return builder.ToString();
    }
}