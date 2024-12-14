using System.Collections.Immutable;
using System.Composition;
using System.Reflection.Metadata;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Document = Microsoft.CodeAnalysis.Document;
using Formatter = Microsoft.CodeAnalysis.Formatting.Formatter;

namespace StringAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(StringToUpperCaseCodeFixProvider)), Shared]
    public class StringToUpperCaseCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(StringToUpperCaseAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async System.Threading.Tasks.Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics[0];
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var literal = root.FindNode(diagnosticSpan) as LiteralExpressionSyntax;
            if (literal == null) return;

            context.RegisterCodeFix(
                Microsoft.CodeAnalysis.CodeActions.CodeAction.Create(
                    title: "Convert to uppercase",
                    createChangedDocument: c => ConvertToUppercaseAsync(context.Document, literal, c),
                    equivalenceKey: "ConvertToUppercase"),
                diagnostic);
        }

        private async System.Threading.Tasks.Task<Document> ConvertToUppercaseAsync(Document document,
            LiteralExpressionSyntax literal, System.Threading.CancellationToken cancellationToken)
        {
            var newLiteral = literal.WithToken(SyntaxFactory.Literal(literal.Token.ValueText.ToUpper()))
                .WithAdditionalAnnotations(Formatter.Annotation);

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = root.ReplaceNode(literal, newLiteral);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}