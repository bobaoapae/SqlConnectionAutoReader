using System;
using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SqlConnectionAutoReader;

[Generator]
public class IncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var nodes = context.SyntaxProvider.CreateSyntaxProvider(PreFilter, Parse)
            .Where(static s => s != null);

        var combined = context.CompilationProvider.Combine(nodes.Collect());
        context.RegisterSourceOutput(combined, Generate);
    }

    private static void Generate(SourceProductionContext arg1, (Compilation Left, ImmutableArray<SourceState> Right) arg2)
    {
        var count = 0;
        var (compilation, sourceStates) = arg2;
        var writer = new IndentedTextWriter(new StringWriter(), "   ");
        writer.WriteLine("using System;");
        writer.WriteLine("using System.Collections.Generic;");
        writer.WriteLine("using Microsoft.Data.SqlClient;");
        writer.WriteLine();
        writer.WriteLine("namespace SqlConnectionAutoReader");
        writer.WriteLine("{");
        writer.Indent++;
        writer.WriteLine("[System.CodeDom.Compiler.GeneratedCode(\"SqlConnectionAutoReader.Generator\", \"1.0\")]");
        writer.WriteLine("file static class SqlConnectionExtensions_Interceptors_Generated");
        writer.WriteLine("{");
        writer.Indent++;
        foreach (var sourceState in sourceStates)
        {
            var isList = sourceState.MethodSymbol.Name.Contains("All");
            var returnType = sourceState.ReturnType;
            writer.Write("[System.Runtime.CompilerServices.InterceptsLocation(");
            writer.Write("@\"");
            writer.Write(sourceState.FullPath);
            writer.Write("\", ");
            writer.Write(sourceState.Location.GetLineSpan().StartLinePosition.Line + 1);
            writer.Write(", ");
            writer.Write(sourceState.Location.GetLineSpan().StartLinePosition.Character + 1);
            writer.WriteLine(")]");
            if (!isList)
            {
                writer.Write("public static async System.Threading.Tasks.Task<StoredProcedureResult<");
                writer.Write(returnType);
                writer.Write(">> ExecuteProcedureAsync_");
                writer.Write(returnType.Name);
            }
            else
            {
                writer.Write("public static async System.Threading.Tasks.Task<StoredProcedureMultipleResult<");
                writer.Write(returnType);
                writer.Write(">> ExecuteProcedureAllAsync_");
                writer.Write(returnType.Name);
            }

            writer.Write(count++);

            writer.Write("(");
            writer.Write("this Microsoft.Data.SqlClient.SqlConnection connection");
            writer.Write(", ");
            writer.Write("string procedureName");
            writer.Write(", ");
            writer.Write("System.Action<Microsoft.Data.SqlClient.SqlParameterCollection> parametersAction = null");
            writer.WriteLine(")");
            writer.WriteLine("{");
            writer.Indent++;
            writer.WriteLine("await using var command = new Microsoft.Data.SqlClient.SqlCommand(procedureName, connection);");
            writer.WriteLine("command.CommandType = System.Data.CommandType.StoredProcedure;");
            writer.WriteLine();
            writer.WriteLine("parametersAction?.Invoke(command.Parameters);");
            writer.WriteLine();
            writer.WriteLine("await connection.OpenAsync();");
            writer.WriteLine();
            writer.WriteLine("await using var reader = await command.ExecuteReaderAsync(System.Data.CommandBehavior.Default);");
            writer.WriteLine("if (!await reader.ReadAsync())");
            writer.Indent++;
            writer.WriteLine("throw new System.Exception(\"Can't read procedure header result\");");
            writer.Indent--;
            writer.WriteLine();
            writer.Write("var spResult = new ");
            writer.Write(isList ? "StoredProcedureMultipleResult" : "StoredProcedureResult");
            writer.Write("<");
            writer.Write(returnType);
            writer.WriteLine(">");
            writer.WriteLine("{");
            writer.Indent++;
            writer.Write("ResultType = (SpResultType)(int)reader[0]");
            writer.WriteLine(",");
            writer.Write("Code = (int)reader[1]");
            writer.WriteLine(",");
            writer.Write("Result = ");
            writer.Write("new ");
            if (isList)
            {
                writer.Write("System.Collections.Generic.List<");
                writer.Write(returnType);
                writer.Write(">()");
            }
            else
            {
                writer.Write(returnType);
                writer.Write("()");
            }

            writer.Indent--;
            writer.WriteLine();
            writer.WriteLine("};");
            writer.WriteLine();
            writer.WriteLine("if(spResult.ResultType == SpResultType.SP_ERROR)");
            writer.Indent++;
            writer.WriteLine("return spResult;");
            writer.Indent--;
            writer.WriteLine("if(!await reader.NextResultAsync())");
            writer.Indent++;
            writer.WriteLine("throw new System.Exception(\"Can't read procedure result set\");");
            writer.Indent--;
            writer.WriteLine();
            if (!isList)
            {
                writer.WriteLine("if (!await reader.ReadAsync())");
                writer.Indent++;
                writer.WriteLine("throw new System.Exception(\"Can't read procedure result\");");
                writer.Indent--;
                writer.WriteLine();
                foreach (var property in returnType.GetMembers().OfType<IPropertySymbol>())
                {
                    writer.Write("spResult.Result.");
                    writer.Write(property.Name);
                    writer.Write(" = (");
                    writer.Write(property.Type);
                    writer.Write(")reader[nameof(spResult.Result.");
                    writer.Write(property.Name);
                    writer.WriteLine(")];");
                }
                //populate from base class as well if base class is not object
                if (returnType.BaseType!.Name != "Object")
                {
                    foreach (var property in returnType.BaseType.GetMembers().OfType<IPropertySymbol>())
                    {
                        writer.Write("spResult.Result.");
                        writer.Write(property.Name);
                        writer.Write(" = (");
                        writer.Write(property.Type);
                        writer.Write(")reader[nameof(spResult.Result.");
                        writer.Write(property.Name);
                        writer.WriteLine(")];");
                    }
                }
            }
            else
            {
                writer.WriteLine("while (await reader.ReadAsync())");
                writer.WriteLine("{");
                writer.Indent++;
                writer.Write("var resultItem = new ");
                writer.Write(returnType);
                writer.WriteLine("();");
                foreach (var property in returnType.GetMembers().OfType<IPropertySymbol>())
                {
                    writer.Write("resultItem.");
                    writer.Write(property.Name);
                    writer.Write(" = (");
                    writer.Write(property.Type);
                    writer.Write(")reader[nameof(resultItem.");
                    writer.Write(property.Name);
                    writer.WriteLine(")];");
                }
                //populate from base class as well if base class is not object
                if (returnType.BaseType!.Name != "Object")
                {
                    foreach (var property in returnType.BaseType.GetMembers().OfType<IPropertySymbol>())
                    {
                        writer.Write("resultItem.");
                        writer.Write(property.Name);
                        writer.Write(" = (");
                        writer.Write(property.Type);
                        writer.Write(")reader[nameof(resultItem.");
                        writer.Write(property.Name);
                        writer.WriteLine(")];");
                    }
                }

                writer.WriteLine("spResult.Result.Add(resultItem);");
                writer.Indent--;
                writer.WriteLine("}");
            }

            writer.WriteLine();
            writer.WriteLine("return spResult;");
            writer.Indent--;
            writer.WriteLine("}");
            writer.WriteLine();
        }

        writer.Indent--;
        writer.WriteLine("}");
        writer.WriteLine();
        writer.Indent--;
        writer.WriteLine("}");
        writer.WriteLine();
        writer.Flush();
        arg1.AddSource("SqlConnectionExtensions_Interceptors_Generated.g.cs", writer.InnerWriter.ToString());
    }

    private static bool IsCandidate(string methodName) =>
        methodName.StartsWith("ExecuteProcedure");

    private static bool PreFilter(SyntaxNode node, CancellationToken cancellationToken)
    {
        if (node is InvocationExpressionSyntax ie && ie.ChildNodes().FirstOrDefault() is MemberAccessExpressionSyntax ma)
        {
            return IsCandidate(ma.Name.ToString());
        }

        return false;
    }

    private SourceState Parse(GeneratorSyntaxContext ctx, CancellationToken cancellationToken)
    {
        try
        {
            var methodSymbol = ctx.SemanticModel.GetSymbolInfo(ctx.Node).Symbol as IMethodSymbol;
            if (methodSymbol == null)
                return null;
            if (methodSymbol.Arity < 1)
                return null;
            var classSymbol = methodSymbol.ContainingType;
            if (classSymbol == null)
                return null;
            if (classSymbol.ToDisplayString() != "SqlConnectionAutoReader.SqlConnectionExtensions")
                return null;
            var location = GetMemberLocation(ctx.Node);
            if (methodSymbol!.ReturnType is not INamedTypeSymbol returnType)
                return null;
            returnType = returnType.TypeArguments[0] as INamedTypeSymbol;
            if (returnType == null)
                return null;
            returnType = returnType.TypeArguments[0] as INamedTypeSymbol;
            if (returnType == null)
                return null;
            var fullPath = location.SourceTree!.FilePath;
            return new SourceState(location, returnType, methodSymbol, fullPath);
        }
        catch (Exception ex)
        {
            Debug.Fail(ex.Message);
            return null;
        }
    }

    private class SourceState(Location location, INamedTypeSymbol returnType, IMethodSymbol methodSymbol, string fullPath)
    {
        public Location Location => location;
        public INamedTypeSymbol ReturnType => returnType;
        public IMethodSymbol MethodSymbol => methodSymbol;
        public string FullPath => fullPath;
    }

    public static Location GetMemberLocation(SyntaxNode call)
        => GetMemberSyntax(call).GetLocation();

    public static SyntaxNode GetMemberSyntax(SyntaxNode syntax)
    {
        foreach (var outer in syntax.ChildNodesAndTokens())
        {
            var outerNode = outer.AsNode();
            if (outerNode is MemberAccessExpressionSyntax)
            {
                SyntaxNode identifier = null;
                foreach (var inner in outerNode.ChildNodesAndTokens())
                {
                    var innerNode = inner.AsNode();
                    if (innerNode is SimpleNameSyntax)
                        identifier = innerNode;
                }

                return identifier ?? outerNode;
            }
        }

        return syntax;
    }
}