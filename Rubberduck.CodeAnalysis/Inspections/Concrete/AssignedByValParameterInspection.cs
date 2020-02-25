using System.Linq;
using Rubberduck.Inspections.Abstract;
using Rubberduck.Resources.Inspections;
using Rubberduck.Parsing.Symbols;
using Rubberduck.Parsing.VBA;
using Rubberduck.Parsing.VBA.DeclarationCaching;

namespace Rubberduck.Inspections.Concrete
{
    /// <summary>
    /// Warns about parameters passed by value being assigned a new value in the body of a procedure.
    /// </summary>
    /// <why>
    /// Debugging is easier if the procedure's initial state is preserved and accessible anywhere within its scope.
    /// Mutating the inputs destroys the initial state, and makes the intent ambiguous: if the calling code is meant
    /// to be able to access the modified values, then the parameter should be passed ByRef; the ByVal modifier might be a bug.
    /// </why>
    /// <example hasResults="true">
    /// <![CDATA[
    /// Public Sub DoSomething(ByVal foo As Long)
    ///     foo = foo + 1 ' is the caller supposed to see the updated value?
    ///     Debug.Print foo
    /// End Sub
    /// ]]>
    /// </example>
    /// <example hasResults="false">
    /// <![CDATA[
    /// Public Sub DoSomething(ByVal foo As Long)
    ///     Dim bar As Long
    ///     bar = foo
    ///     bar = bar + 1 ' clearly a local copy of the original value.
    ///     Debug.Print bar
    /// End Sub
    /// ]]>
    /// </example>
    public sealed class AssignedByValParameterInspection : DeclarationInspectionBase
    {
        public AssignedByValParameterInspection(RubberduckParserState state)
            : base(state, DeclarationType.Parameter)
        {}

        protected override bool IsResultDeclaration(Declaration declaration, DeclarationFinder finder)
        {
            return declaration is ParameterDeclaration parameter 
                   && !parameter.IsByRef 
                   && parameter.References
                       .Any(reference => reference.IsAssignment);
        }

        protected override string ResultDescription(Declaration declaration)
        {
            return string.Format(InspectionResults.AssignedByValParameterInspection, declaration.IdentifierName);
        }
    }
}
