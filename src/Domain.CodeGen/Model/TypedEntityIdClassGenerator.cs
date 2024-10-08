﻿// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Domain;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

[Generator]
#pragma warning disable RS1036 // Specify analyzer banned API enforcement setting
public class TypedEntityIdClassGenerator : ISourceGenerator
#pragma warning restore RS1036 // Specify analyzer banned API enforcement setting
{
    public void Execute(GeneratorExecutionContext context)
    {
        var compilation = context.Compilation;

        var classesWithAttribute = compilation.SyntaxTrees
            .SelectMany(st => st.GetRoot().DescendantNodes())
            .OfType<ClassDeclarationSyntax>()
            .Where(cds => cds.AttributeLists
                .SelectMany(al => al.Attributes)
                .Any(a => a.Name.ToString().StartsWith("TypedEntityId")))
            .ToList();

        foreach (var classDeclaration in classesWithAttribute)
        {
            var semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);

            if (classSymbol != null)
            {
                var attribute = classSymbol.GetAttributes().FirstOrDefault(ad => ad.AttributeClass.Name.StartsWith("TypedEntityId"));
                if (attribute != null)
                {
                    if (ImplementsIEntity(classSymbol))
                    {
                        var underlyingType = attribute.AttributeClass.TypeArguments.First();
                        var generatedCode = GenerateIdClassCode(classSymbol, underlyingType);
                        context.AddSource($"{classSymbol.Name}Id.g.cs", SourceText.From(generatedCode, Encoding.UTF8));
                    }
                    else
                    {
                        var diagnostic = Diagnostic.Create(
                            new DiagnosticDescriptor(
                                "TIG001",
                                "Invalid use of TypedEntityIdAttribute",
                                "TypedEntityIdAttribute can only be applied to classes implementing IEntity (directly or indirectly)",
                                "Usage",
                                DiagnosticSeverity.Error,
                                isEnabledByDefault: true),
                            classDeclaration.GetLocation());
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        // No initialization required
    }

    private static bool ImplementsIEntity(INamedTypeSymbol classSymbol)
    {
        return classSymbol.AllInterfaces.Any(i => i.Name == "IEntity");
    }

    private static string GenerateIdClassCode(INamedTypeSymbol entityType, ITypeSymbol underlyingType)
    {
        var className = $"{entityType.Name}Id";
        var namespaceName = entityType.ContainingNamespace.ToDisplayString();
        var typeName = underlyingType.Name;

        var createMethod = GetCreateMethod(underlyingType, className);
        var parseMethod = GetParseMethod(underlyingType, className);
        var converterClass = GenerateJsonConverterClass(className, underlyingType);

        return $@"
// <auto-generated />
namespace {namespaceName}
{{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using BridgingIT.DevKit.Domain.Model;

    [DebuggerDisplay(""{{Value}}"")]
    [JsonConverter(typeof({className}JsonConverter))]
    public class {className} : EntityId<{typeName}>
    {{
        private {className}()
        {{
        }}

        private {className}({typeName} value)
        {{
            this.Value = value;
        }}

        public override {typeName} Value {{ get; protected set; }}

        public bool IsEmpty => this.Value{GetIsEmptyCheck(underlyingType)};

        public static implicit operator {typeName}({className} id) => id?.Value ?? default;
        public static implicit operator string({className} id) => id?.Value.ToString();
        public static implicit operator {className}({typeName} id) => Create(id);
        public static implicit operator {className}(string id) => Create(id);

        {createMethod}

        public static {className} Create({typeName} id)
        {{
            return new {className}(id);
        }}

        {parseMethod}

        protected override IEnumerable<object> GetAtomicValues()
        {{
            yield return this.Value;
        }}
    }}

    {converterClass}
}}";
    }

    private static string GetCreateMethod(ITypeSymbol underlyingType, string className)
    {
        if (underlyingType.ToString() == "System.Guid")
        {
            return $@"public static {className} Create()
        {{
            return new {className}(Guid.NewGuid());
        }}";
        }
        else if (underlyingType.SpecialType == SpecialType.System_Int32 || underlyingType.SpecialType == SpecialType.System_Int64)
        {
            return $@"// Note: Implement a strategy for generating new IDs for integer/long
        public static {className} Create()
        {{
            //Implement a strategy for generating new integer IDs
            throw new NotImplementedException();
        }}";
        }
        else
        {
            return $@"// Note: Implement a strategy for generating new IDs for this type
        public static {className} Create()
        {{
            //Implement a strategy for generating new IDs
            throw new NotImplementedException();
        }}";
        }
    }

    private static string GetParseMethod(ITypeSymbol underlyingType, string className)
    {
        if (underlyingType.ToString() == "System.Guid")
        {
            return $@"public static {className} Create(string id)
        {{
            if (string.IsNullOrEmpty(id))
            {{
                throw new ArgumentException(""Id cannot be null or empty."", nameof(id));
            }}

            return new {className}(Guid.Parse(id));
        }}";
        }
        else if (underlyingType.SpecialType == SpecialType.System_Int32)
        {
            return $@"public static {className} Create(string id)
        {{
            if (string.IsNullOrEmpty(id))
            {{
                throw new ArgumentException(""Id cannot be null or empty."", nameof(id));
            }}

            return new {className}(int.Parse(id));
        }}";
        }
        else if (underlyingType.SpecialType == SpecialType.System_Int64)
        {
            return $@"public static {className} Create(string id)
        {{
            if (string.IsNullOrEmpty(id))
            {{
                throw new ArgumentException(""Id cannot be null or empty."", nameof(id));
            }}

            return new {className}(long.Parse(id));
        }}";
        }
        else if (underlyingType.SpecialType == SpecialType.System_String)
        {
            return $@"public static {className} Create(string id)
        {{
            if (string.IsNullOrEmpty(id))
            {{
                throw new ArgumentException(""Id cannot be null or empty."", nameof(id));
            }}

            return new {className}(id);
        }}";
        }
        else
        {
            return $@"// Note: Implement a custom parsing strategy for this type
        public static {className} Create(string id)
        {{
            throw new NotImplementedException(""Implement a custom parsing strategy for this type"");
        }}";
        }
    }

    private static string GetIsEmptyCheck(ITypeSymbol underlyingType)
    {
        if (underlyingType.ToString() == "System.Guid")
        {
            return " == Guid.Empty";
        }
        else if (underlyingType.SpecialType == SpecialType.System_Int32 || underlyingType.SpecialType == SpecialType.System_Int64)
        {
            return " == 0";
        }
        else if (underlyingType.SpecialType == SpecialType.System_String)
        {
            return " == null || this.Value == string.Empty";
        }
        else
        {
            return "; // Implement custom empty check for this type";
        }
    }

    private static string GenerateJsonConverterClass(string className, ITypeSymbol underlyingType)
    {
        var typeName = underlyingType.Name;
        var readMethod = GetJsonConverterReadMethod(underlyingType, className);
        var writeMethod = GetJsonConverterWriteMethod(underlyingType);

        return $@"
    public class {className}JsonConverter : JsonConverter<{className}>
    {{
        public override {className} Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {{
            {readMethod}
        }}

        public override void Write(Utf8JsonWriter writer, {className} value, JsonSerializerOptions options)
        {{
            {writeMethod}
        }}
    }}";
    }

    private static string GetJsonConverterReadMethod(ITypeSymbol underlyingType, string className)
    {
        if (underlyingType.ToString() == "System.Guid")
        {
            return $@"if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException(""Expected string value for {className}"");

            var guidString = reader.GetString();
            return {className}.Create(guidString);";
        }
        else if (underlyingType.SpecialType == SpecialType.System_Int32)
        {
            return $@"if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType != JsonTokenType.Number)
                throw new JsonException(""Expected number value for {className}"");

            var intValue = reader.GetInt32();
            return {className}.Create(intValue);";
        }
        else if (underlyingType.SpecialType == SpecialType.System_Int64)
        {
            return $@"if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType != JsonTokenType.Number)
                throw new JsonException(""Expected number value for {className}"");

            var longValue = reader.GetInt64();
            return {className}.Create(longValue);";
        }
        else if (underlyingType.SpecialType == SpecialType.System_String)
        {
            return $@"if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException(""Expected string value for {className}"");

            var stringValue = reader.GetString();
            return {className}.Create(stringValue);";
        }
        else
        {
            return $@"throw new JsonException(""Unsupported type for {className}"");";
        }
    }

    private static string GetJsonConverterWriteMethod(ITypeSymbol underlyingType)
    {
        if (underlyingType.ToString() == "System.Guid")
        {
            return @"if (value == null)
                writer.WriteNullValue();
            else
                writer.WriteStringValue(value.Value.ToString());";
        }
        else if (underlyingType.SpecialType == SpecialType.System_Int32)
        {
            return @"if (value == null)
                writer.WriteNullValue();
            else
                writer.WriteNumberValue(value.Value);";
        }
        else if (underlyingType.SpecialType == SpecialType.System_Int64)
        {
            return @"if (value == null)
                writer.WriteNullValue();
            else
                writer.WriteNumberValue(value.Value);";
        }
        else if (underlyingType.SpecialType == SpecialType.System_String)
        {
            return @"if (value == null)
                writer.WriteNullValue();
            else
                writer.WriteStringValue(value.Value);";
        }
        else
        {
            return @"throw new JsonException(""Unsupported type for writing"");";
        }
    }
}