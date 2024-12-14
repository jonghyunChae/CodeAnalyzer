using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace StringAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class StringToUpperCaseAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "STR001";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            "Lowercase string detected",
            "String '{0}' should be uppercase",
            "Naming",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            // Register an action for analyzing string literals
            context.RegisterSyntaxNodeAction(AnalyzeStringLiterals,
                Microsoft.CodeAnalysis.CSharp.SyntaxKind.StringLiteralExpression);
        }

        private static void AnalyzeStringLiterals(SyntaxNodeAnalysisContext context)
        {
            var literal = (Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax)context.Node;
            var valueText = literal.Token.ValueText;

            if (!string.IsNullOrEmpty(valueText) && valueText != valueText.ToUpper())
            {
                var diagnostic = Diagnostic.Create(Rule, literal.GetLocation(), valueText);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}