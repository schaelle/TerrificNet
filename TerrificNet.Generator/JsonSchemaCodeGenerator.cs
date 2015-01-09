﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Schema;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;

namespace TerrificNet.Generator
{
    public class JsonSchemaCodeGenerator : IJsonSchemaCodeGenerator
    {
        private readonly INamingRule _namingRule;

        public JsonSchemaCodeGenerator() : this(new NamingRule())
        {
        }

        public JsonSchemaCodeGenerator(INamingRule namingRule)
        {
            _namingRule = namingRule;
        }

        public string Generate(JsonSchema schema)
        {
            var root = GetSyntax(schema);
            return root.NormalizeWhitespace().ToFullString();
        }

        private CompilationUnitSyntax GetSyntax(JsonSchema schema)
        {
            var typeContext = new Dictionary<string, MemberDeclarationSyntax>();

            RoslynExtension.GenerateClass(schema, typeContext, string.Empty, _namingRule);

            var root = Syntax.CompilationUnit()
                .WithMembers(Syntax.NamespaceDeclaration(Syntax.ParseName(schema.Title)).WithMembers(Syntax.List(typeContext.Values.ToArray())));
            return root;
        }

        public Type Compile(JsonSchema schema)
        {
            var schemas = new[] { schema };
            using (var stream = new MemoryStream())
            {
                var result = CompileToInternal(schemas, stream);
                if (result.Success)
                {
                    var assembly = Assembly.Load(stream.ToReadOnlyArray().ToArray());
                    return assembly.GetTypes().First();
                }
            }

            return null;
        }

        public void WriteTo(IEnumerable<JsonSchema> schemas, Stream stream)
        {
            var syntaxTree = SyntaxTree.Create(
                Syntax.CompilationUnit().WithMembers(Syntax.List(schemas.SelectMany(s => GetSyntax(s).Members))).NormalizeWhitespace());

            var text = syntaxTree.GetText();
            var streamWriter = new StreamWriter(stream);
            
            text.Write(streamWriter);
            streamWriter.Flush();            
        }

        public void CompileTo(IEnumerable<JsonSchema> schemas, Stream stream)
        {
            CompileToInternal(schemas, stream);
        }

        private EmitResult CompileToInternal(IEnumerable<JsonSchema> schemas, Stream stream)
        {
            var syntaxTrees = schemas.Select(s => SyntaxTree.Create(GetSyntax(s)));

            var compilation = Compilation.Create("test", new CompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(MetadataReference.CreateAssemblyReference("mscorlib"))
                .AddReferences(MetadataReference.CreateAssemblyReference("System"))
                .AddReferences(MetadataReference.CreateAssemblyReference("System.Core"))
                .AddSyntaxTrees(syntaxTrees);

            var result = compilation.Emit(stream);
            return result;
        }
    }

    static class RoslynExtension
    {
        public static SyntaxList<MemberDeclarationSyntax> AddProperties(this SyntaxList<MemberDeclarationSyntax> memberList, JsonSchema schema, Dictionary<string, MemberDeclarationSyntax> typeContext, INamingRule namingRule)
        {
            var result = memberList;
            if (schema.Properties != null)
            {
                foreach (var property in schema.Properties)
                {
                    var propertyName = namingRule.GetPropertyName(property.Key);

                    result =
                        result.Add(Syntax.PropertyDeclaration(
                            GetPropertyType(property.Value, typeContext, propertyName, namingRule).WithTrailingTrivia(Syntax.Space),
                            propertyName)
                            .WithModifiers(new SyntaxTokenList().Add(Syntax.Token(SyntaxKind.PublicKeyword)))
                            .WithAccessorList(Syntax.AccessorList(Syntax.List(
                                Syntax.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                    .WithSemicolonToken(Syntax.Token(SyntaxKind.SemicolonToken)),
                                Syntax.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                    .WithSemicolonToken(Syntax.Token(SyntaxKind.SemicolonToken))
                                ))));
                }
            }

            return result;
        }

        private static TypeSyntax GetPropertyType(JsonSchema value, Dictionary<string, MemberDeclarationSyntax> typeContext, string propertyName, INamingRule namingRule)
        {
            switch (value.Type)
            {
                case JsonSchemaType.String:
                    return Syntax.ParseTypeName("string");
                case JsonSchemaType.Integer:
                    return Syntax.ParseTypeName("int");
                case JsonSchemaType.Float:
                    return Syntax.ParseTypeName("double");
                case JsonSchemaType.Boolean:
                    return Syntax.ParseTypeName("bool");
                case JsonSchemaType.Array:
                    var valueType = value.Items.FirstOrDefault();
                    var name = namingRule.GetClassNameFromArrayItem(value, propertyName);
                    var genericType = GetPropertyType(valueType, typeContext, name, namingRule);
                    return Syntax.QualifiedName(GetQualifiedName("System", "Collections", "Generic"), Syntax.GenericName(Syntax.Identifier("IList"), Syntax.TypeArgumentList(Syntax.SeparatedList(genericType))));
                case JsonSchemaType.Object:
                    var className = GenerateClass(value, typeContext, propertyName, namingRule);
                    return Syntax.IdentifierName(className);
                default:
                    return Syntax.ParseTypeName("object");
            }
        }

        // TODO: Remove hack
        private static QualifiedNameSyntax GetQualifiedName(params string[] parts)
        {
            var syntax = Syntax.QualifiedName(Syntax.IdentifierName(parts[0]), Syntax.IdentifierName(parts[1]));
            return Syntax.QualifiedName(syntax, Syntax.IdentifierName(parts[2]));
        }

        public static string GenerateClass(JsonSchema schema, Dictionary<string, MemberDeclarationSyntax> typeContext, string propertyName, INamingRule namingRule)
        {
            if (schema.Type == JsonSchemaType.Object)
            {
                var className = namingRule.GetClassName(schema, propertyName);

                if (typeContext.ContainsKey(className))
                    return className;

                if (string.IsNullOrEmpty(className))
                    throw new Exception("Title not set");

                var classDeclaration = Syntax.ClassDeclaration(
                    Syntax.Identifier(className))
                    .WithModifiers(new SyntaxTokenList().Add(Syntax.Token(SyntaxKind.PublicKeyword)))
                    .WithKeyword(
                        Syntax.Token(SyntaxKind.ClassKeyword, Syntax.TriviaList(Syntax.Space)))
                    .WithOpenBraceToken(
                        Syntax.Token(SyntaxKind.OpenBraceToken))
                    .WithMembers(new SyntaxList<MemberDeclarationSyntax>().AddProperties(schema, typeContext, namingRule))
                    .WithCloseBraceToken(
                        Syntax.Token(SyntaxKind.CloseBraceToken));

                typeContext.Add(className, classDeclaration);
                return className;
            }
            return null;
        }
    }
}
