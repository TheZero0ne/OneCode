﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CSharp;
using OneCode;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DAL {
    class DataAcessor {
        private static DataAcessor instance;
        public VariableCollection varCollection { get; set; }

        private DataAcessor() {
            varCollection = new VariableCollection();
        }

        public static DataAcessor getInstance() {
            if (instance == null)
                instance = new DataAcessor();

            return instance;
        }

        public void FindVariablesInDoc(EnvDTE.TextDocument haystackDoc) {
            VariableCollection helperCollection = new VariableCollection();
            var objEditPt = haystackDoc.StartPoint.CreateEditPoint();
            var tree = CSharpSyntaxTree.ParseText(objEditPt.GetText(haystackDoc.EndPoint));
            var Mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var compilation = CSharpCompilation.Create("MyCompilation", syntaxTrees: new[] { tree }, references: new[] { Mscorlib });
            var model = compilation.GetSemanticModel(tree);
            var variables = tree.GetRoot().DescendantNodes().Where(v => v is FieldDeclarationSyntax || v is LocalDeclarationStatementSyntax || v is PropertyDeclarationSyntax || v is ParameterSyntax);

            //TODO: übergeordneten Typen finden -> Objekt in Variable(Klasse) speichern
            var ws = new AdhocWorkspace();
            Solution s = ws.CurrentSolution;


            foreach (var v in variables) {
                var para = v as ParameterSyntax;
                var prop = v as PropertyDeclarationSyntax;
                var local = v as LocalDeclarationStatementSyntax;
                var field = v as FieldDeclarationSyntax;

                if (para != null) {
                    var symbol = model.GetDeclaredSymbol(para);
                    string visibleType = para.Type.ToString();
                    string name = symbol.Name;
                    string kind = symbol.Kind.ToString();

                    //SymbolFinder.FindImplementationsAsync(symbol);

                    varCollection.Add(new Variable(visibleType, name, kind, para.SpanStart));
                } else if (prop != null) {
                    var symbol = model.GetDeclaredSymbol(prop);
                    string visibleType = prop.Type.ToString();
                    string name = symbol.Name;
                    string kind = symbol.Kind.ToString();

                    varCollection.Add(new Variable(visibleType, name, kind, prop.SpanStart));
                } else if (local != null) {
                    string visibleType = local.Declaration.Type.ToString();

                    foreach (var var in local.Declaration.Variables) {
                        var symbol = model.GetDeclaredSymbol(var);
                        string name = symbol.Name;
                        string kind = symbol.Kind.ToString();

                        varCollection.Add(new Variable(visibleType, name, kind, local.SpanStart));
                    }
                } else if (field != null) {
                    string visibleType = field.Declaration.Type.ToString();

                    foreach (var var in field.Declaration.Variables) {
                        var symbol = model.GetDeclaredSymbol(var);
                        string name = symbol.Name;
                        string kind = symbol.Kind.ToString();

                        varCollection.Add(new Variable(visibleType, name, kind, field.SpanStart));
                    }
                }
            }
        }
    }
}
