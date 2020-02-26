using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Rubberduck.Inspections.Abstract;
using Rubberduck.Parsing;
using Rubberduck.Parsing.Grammar;
using Rubberduck.Parsing.Inspections.Abstract;
using Rubberduck.Parsing.VBA;
using Rubberduck.Resources;

namespace Rubberduck.Inspections.Concrete
{
    /// <summary>
    /// Flags parameters declared across multiple physical lines of code.
    /// </summary>
    /// <why>
    /// When splitting a long list of parameters across multiple lines, care should be taken to avoid splitting a parameter declaration in two.
    /// </why>
    /// <example hasResults="true">
    /// <![CDATA[
    /// Public Sub DoSomething(ByVal foo As Long, ByVal _ 
    ///                              bar As Long)
    ///     ' ...
    /// End Sub
    /// ]]>
    /// </example>
    /// <example hasResults="false">
    /// <![CDATA[
    /// Public Sub DoSomething(ByVal foo As Long, _ 
    ///                        ByVal bar As Long)
    ///     ' ...
    /// End Sub
    /// ]]>
    /// </example>
    public sealed class MultilineParameterInspection : ParseTreeInspectionBase
    {
        public MultilineParameterInspection(IDeclarationFinderProvider declarationFinderProvider)
            : base(declarationFinderProvider)
        {
            Listener = new ParameterListener();
        }
        
        public override IInspectionListener Listener { get; }
        protected override string ResultDescription(QualifiedContext<ParserRuleContext> context)
        {
            var parameterText = ((VBAParser.ArgContext) context.Context).unrestrictedIdentifier().GetText();
            return string.Format(context.Context.GetSelection().LineCount > 3
                    ? RubberduckUI.EasterEgg_Continuator
                    : Resources.Inspections.InspectionResults.MultilineParameterInspection,
                parameterText);
        }

        public class ParameterListener : InspectionListenerBase
        {
            public override void ExitArg([NotNull] VBAParser.ArgContext context)
            {
                if (context.Start.Line != context.Stop.Line)
                {
                    SaveContext(context);
                }
            }
        }
    }
}
