using System.Collections.Generic;
using System.Linq;
using Rubberduck.Inspections.Abstract;
using Rubberduck.Inspections.Results;
using Rubberduck.Parsing.Inspections.Abstract;
using Rubberduck.Resources.Inspections;
using Rubberduck.Parsing.Symbols;
using Rubberduck.Parsing.VBA;

namespace Rubberduck.Inspections.Concrete
{
    /// <summary>
    /// Warns about host-evaluated square-bracketed expressions.
    /// </summary>
    /// <why>
    /// Host-evaluated expressions should be implementable using the host application's object model.
    /// If the expression yields an object, member calls against that object are late-bound.
    /// </why>
    /// <example>
    /// This inspection would flag the following call to the GetFoo function:
    /// <code>
    /// Public Sub DoSomething()
    ///     [A1].Value = 42
    /// End Sub
    /// </code>
    /// The following code would not trip the inspection:
    /// <code>
    /// Public Sub DoSomething()
    ///     ActiveSheet.Range("A1").Value = 42
    /// End Sub
    /// </code>
    /// </example>
    public sealed class HostSpecificExpressionInspection : InspectionBase
    {
        public HostSpecificExpressionInspection(RubberduckParserState state)
            : base(state) { }

        protected override IEnumerable<IInspectionResult> DoGetInspectionResults()
        {
            return Declarations.Where(item => item.DeclarationType == DeclarationType.BracketedExpression)
                .Select(item => new DeclarationInspectionResult(this, string.Format(InspectionResults.HostSpecificExpressionInspection, item.IdentifierName), item));
        }
    }
}