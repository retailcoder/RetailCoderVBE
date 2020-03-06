﻿using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Rubberduck.Inspections.Abstract;
using Rubberduck.Parsing;
using Rubberduck.Parsing.Grammar;
using Rubberduck.Parsing.Inspections.Abstract;
using Rubberduck.Resources.Inspections;
using Rubberduck.Parsing.VBA;

namespace Rubberduck.Inspections.Concrete
{
    /// <summary>
    /// Identifies empty 'If' blocks.
    /// </summary>
    /// <why>
    /// Conditional expression is inverted; there would not be a need for an 'Else' block otherwise.
    /// </why>
    /// <example hasResults="true">
    /// <![CDATA[
    /// Public Sub DoSomething(ByVal foo As Boolean)
    ///     If foo Then
    ///     Else
    ///         ' ...
    ///     End If
    /// End Sub
    /// ]]>
    /// </example>
    /// <example hasResults="false">
    /// <![CDATA[
    /// Public Sub DoSomething(ByVal foo As Boolean)
    ///     If Not foo Then
    ///         ' ...
    ///     End If
    /// End Sub
    /// ]]>
    /// </example>
    internal sealed class EmptyIfBlockInspection : ParseTreeInspectionBase<ParserRuleContext>
    {
        public EmptyIfBlockInspection(IDeclarationFinderProvider declarationFinderProvider)
            : base(declarationFinderProvider)
        {
            ContextListener = new EmptyIfBlockListener();
        }

        protected override string ResultDescription(QualifiedContext<ParserRuleContext> context)
        {
            return InspectionResults.EmptyIfBlockInspection;
        }

        protected override IInspectionListener<ParserRuleContext> ContextListener { get; }

        private class EmptyIfBlockListener : EmptyBlockInspectionListenerBase<ParserRuleContext>
        {
            public override void EnterIfStmt([NotNull] VBAParser.IfStmtContext context)
            {
                InspectBlockForExecutableStatements(context.block(), context);
            }

            public override void EnterElseIfBlock([NotNull] VBAParser.ElseIfBlockContext context)
            {
                InspectBlockForExecutableStatements(context.block(), context);
            }

            public override void EnterSingleLineIfStmt([NotNull] VBAParser.SingleLineIfStmtContext context)
            {
                if (context.ifWithEmptyThen() != null)
                {
                    SaveContext(context.ifWithEmptyThen());
                }
            }
        }
    }
}
