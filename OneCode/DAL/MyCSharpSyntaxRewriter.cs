using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace OneCode.DAL {

    /// <summary>
    /// Own Implementation of the CSharpSyntaxRewriter. 
    /// 
    /// We use the overridden Visit-methods to gather the node infos we want to translate.
    /// 
    /// The MyRewriterModus defines wether the Rewriter is used to search or to write.
    /// 
    /// Calling in the MyRewriterModus.Write Modus without a variableCollection throws an InvalidOperationException
    /// 
    /// </summary>
    class MyCSharpSyntaxRewriter : CSharpSyntaxRewriter {
        private readonly SemanticModel MySemanticModel;
        private VariableCollection varCol;
        private MyRewriterModus modus;
        private string docName;

        public MyCSharpSyntaxRewriter(SemanticModel sm, VariableCollection col, MyRewriterModus modus, string docName) {
            this.MySemanticModel = sm;
            this.varCol = col;
            this.modus = modus;
            this.docName = docName;

            if (modus == MyRewriterModus.WRITE && (varCol == null || varCol.Count < 1)) {
                throw new InvalidOperationException("The Rewriter can only alter attributes by the given VariableCollection. It can not be empty on WRITE Mode");
            }
        }

        public override SyntaxNode VisitVariableDeclarator(VariableDeclaratorSyntax node) {
            if (modus == MyRewriterModus.SEARCH) {
                // the system can only define the type of a variable after it is initialized
                string typeString = "";
                if (node.Initializer == null) {
                    typeString = "nicht initialisiert";
                } else {
                    TypeInfo initializerInfo = MySemanticModel.GetTypeInfo(node.Initializer.Value);
                    typeString = initializerInfo.Type.Name.ToString();
                }

                varCol.Add(new Variable(typeString, node.Identifier.Text, node.Kind().ToString(), node.SpanStart, docName));
            } else {
                var enumerable = varCol.Where(x => x.SpanStart == node.SpanStart);
                if (enumerable.Count() > 0) {
                    Variable v = enumerable.First();
                    var newIdentifier = SyntaxFactory.Identifier(v.Translation.GetContentWithPrefix() + node.Identifier.TrailingTrivia.ToString());

                    return node.WithIdentifier(newIdentifier);
                }
            }
            return base.VisitVariableDeclarator(node);
        }

        /// <summary>
        ///     We use the VisitIdentifierName method to alter other occurences of nodes within the text
        ///     therefor it is only called in MyRewriterModus.WRITE mode.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node) {
            if (modus == MyRewriterModus.WRITE) {
                var enumerable = varCol.Where(x => x.Name.GetContentWithPrefix() == node.Identifier.Text);
                if (enumerable.Count() > 0) {
                    Variable v = enumerable.First();
                    var newIdentifier = SyntaxFactory.Identifier(node.Identifier.LeadingTrivia.ToString() + v.Translation.GetContentWithPrefix() + node.Identifier.TrailingTrivia.ToString());

                    return node.WithIdentifier(newIdentifier);
                }
            }
            return base.VisitIdentifierName(node);
        }

        public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node) {
            if (modus == MyRewriterModus.SEARCH) {
                varCol.Add(new Variable(node.Type.ToString(), node.Identifier.Text, node.Kind().ToString(), node.SpanStart, docName));
            } else {
                var enumerable = varCol.Where(x => x.SpanStart == node.SpanStart);
                if (enumerable.Count() > 0) {
                    Variable v = enumerable.First();
                    var newIdentifier = SyntaxFactory.Identifier(v.Translation.GetContentWithPrefix() + node.Identifier.TrailingTrivia.ToString());
                    return node.WithIdentifier(newIdentifier);
                }
            }

            return base.VisitPropertyDeclaration(node);
        }

        public override SyntaxNode VisitParameter(ParameterSyntax node) {
            if (modus == MyRewriterModus.SEARCH) {
                varCol.Add(new Variable(node.Type.ToString(), node.Identifier.Text, node.Kind().ToString(), node.SpanStart, docName));
            } else {
                var enumerable = varCol.Where(x => x.SpanStart == node.SpanStart);
                if (enumerable.Count() > 0) {
                    Variable v = enumerable.First();
                    var newIdentifier = SyntaxFactory.Identifier(v.Translation.GetContentWithPrefix() + node.Identifier.TrailingTrivia.ToString());
                    return node.WithIdentifier(newIdentifier);
                }
            }
            return base.VisitParameter(node);
        }
    }

    /// <summary>
    /// The modus for MyCSharpSyntaxRewriter, that defines if the Rewriter is used to gather information or to write in the files
    /// </summary>
    public enum MyRewriterModus {
        SEARCH,
        WRITE
    }
}
