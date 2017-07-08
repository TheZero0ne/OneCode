using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneCode.DAL
{
    class MyCSharpSyntaxRewriter : CSharpSyntaxRewriter
    {
        private readonly SemanticModel MySemanticModel;
        private VariableCollection varCol;
        private MyRewriterModus modus;
        private string docName;

        public MyCSharpSyntaxRewriter(SemanticModel sm, VariableCollection col, MyRewriterModus modus, string docName)
        {
            this.MySemanticModel = sm;
            this.varCol = col;
            this.modus = modus;
            this.docName = docName;

            if (modus == MyRewriterModus.WRITE && (varCol == null || varCol.Count < 1))
            {
                throw new InvalidOperationException("The Rewriter can only alter attributes by the given VariableCollection. It can not be empty on WRITE Mode");
            }
        }

        public override SyntaxNode VisitVariableDeclarator(VariableDeclaratorSyntax node)
        {
            if (modus == MyRewriterModus.SEARCH)
            {
                string typeString = "";
                if (node.Initializer == null)
                {
                    typeString = "nicht initialisiert";
                } else
                {
                    TypeInfo initializerInfo = MySemanticModel.GetTypeInfo(node.Initializer.Value);
                    typeString = initializerInfo.Type.Name.ToString();
                }

                varCol.Add(new Variable(typeString, node.Identifier.Text, node.Kind().ToString(), node.SpanStart, docName));
            }
            else
            {
                var enumerable = varCol.Where(x => x.SpanStart == node.SpanStart);
                if (enumerable.Count() > 0)
                {
                    Variable v = enumerable.First();
                    var newIdentifier = SyntaxFactory.Identifier(v.Translation.GetContentWithPrefix() + node.Identifier.TrailingTrivia.ToString());

                    return node.WithIdentifier(newIdentifier);
                }
            }
            return base.VisitVariableDeclarator(node);
        }

        public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
        {
            if (modus == MyRewriterModus.WRITE)
            {
                var enumerable = varCol.Where(x => x.Name.GetContentWithPrefix() == node.Identifier.Text);
                if (enumerable.Count() > 0)
                {
                    Variable v = enumerable.First();
                    var newIdentifier = SyntaxFactory.Identifier(node.Identifier.LeadingTrivia.ToString() + v.Translation.GetContentWithPrefix() + node.Identifier.TrailingTrivia.ToString());

                    return node.WithIdentifier(newIdentifier);
                }
            }
            return base.VisitIdentifierName(node);
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            if (modus == MyRewriterModus.SEARCH)
            {
                varCol.Add(new Variable(node.Type.ToString(), node.Identifier.Text, node.Kind().ToString(), node.SpanStart, docName));
            }
            else
            {
                var enumerable = varCol.Where(x => x.SpanStart == node.SpanStart);
                if (enumerable.Count() > 0)
                {
                    Variable v = enumerable.First();
                    var newIdentifier = SyntaxFactory.Identifier(v.Translation.GetContentWithPrefix() + node.Identifier.TrailingTrivia.ToString());
                    return node.WithIdentifier(newIdentifier);
                }
            }

            return base.VisitPropertyDeclaration(node);
        }

        public override SyntaxNode VisitParameter(ParameterSyntax node)
        {
            if (modus == MyRewriterModus.SEARCH)
            {
                varCol.Add(new Variable(node.Type.ToString(), node.Identifier.Text, node.Kind().ToString(), node.SpanStart, docName));
            }
            else
            {
                var enumerable = varCol.Where(x => x.SpanStart == node.SpanStart);
                if (enumerable.Count() > 0)
                {
                    Variable v = enumerable.First();
                    var newIdentifier = SyntaxFactory.Identifier(v.Translation.GetContentWithPrefix() + node.Identifier.TrailingTrivia.ToString());
                    return node.WithIdentifier(newIdentifier);
                }
            }
            return base.VisitParameter(node);
        }
    }

    public enum MyRewriterModus
    {
        SEARCH,
        WRITE
    }
}
