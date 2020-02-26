﻿using Antlr4.Runtime.Misc;
using Rubberduck.Inspections.Abstract;
using Rubberduck.Parsing.Common;
using Rubberduck.Parsing.Grammar;
using Rubberduck.Parsing.Inspections.Abstract;
using Rubberduck.Resources.Inspections;
using Rubberduck.Parsing.VBA;
using Antlr4.Runtime;
using Rubberduck.Resources.Experimentals;
using Rubberduck.Parsing;

namespace Rubberduck.Inspections.Concrete
{
    /// <summary>
    /// Identifies empty 'For Each...Next' blocks that can be safely removed.
    /// </summary>
    /// <why>
    /// Dead code should be removed. A loop without a body is usually redundant.
    /// </why>
    /// <example hasResults="true">
    /// <![CDATA[
    /// Public Sub DoSomething()
    ///     Dim sheet As Worksheet
    ///     For Each sheet In ThisWorkbook.Worksheets
    ///         ' no executable statement...
    ///     Next
    /// End Sub
    /// ]]>
    /// </example>
    /// <example hasResults="false">
    /// <![CDATA[
    /// Public Sub DoSomething()
    ///     Dim sheet As Worksheet
    ///     For Each sheet In ThisWorkbook.Worksheets
    ///         Debug.Print sheet.Name
    ///     Next
    /// End Sub
    /// ]]>
    /// </example>
    [Experimental(nameof(ExperimentalNames.EmptyBlockInspections))]
    internal class EmptyForEachBlockInspection : ParseTreeInspectionBase
    {
        public EmptyForEachBlockInspection(IDeclarationFinderProvider declarationFinderProvider)
            : base(declarationFinderProvider)
        {}

        protected override string ResultDescription(QualifiedContext<ParserRuleContext> context)
        {
            return InspectionResults.EmptyForEachBlockInspection;
        }

        public override IInspectionListener Listener { get; } =
            new EmptyForEachBlockListener();

        public class EmptyForEachBlockListener : EmptyBlockInspectionListenerBase
        {
            public override void EnterForEachStmt([NotNull] VBAParser.ForEachStmtContext context)
            {
                InspectBlockForExecutableStatements(context.unterminatedBlock(), context);
            }
        }
    }
}
