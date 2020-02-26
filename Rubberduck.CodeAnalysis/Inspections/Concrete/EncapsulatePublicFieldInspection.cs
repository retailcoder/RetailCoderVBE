﻿using Rubberduck.Inspections.Abstract;
using Rubberduck.Resources.Inspections;
using Rubberduck.Parsing.Symbols;
using Rubberduck.Parsing.VBA;
using Rubberduck.Parsing.VBA.DeclarationCaching;

namespace Rubberduck.Inspections.Concrete
{
    /// <summary>
    /// Flags publicly exposed instance fields.
    /// </summary>
    /// <why>
    /// Instance fields are the implementation details of a object's internal state; exposing them directly breaks encapsulation. 
    /// Often, an object only needs to expose a 'Get' procedure to expose an internal instance field.
    /// </why>
    /// <example hasResults="true">
    /// <![CDATA[
    /// Public Foo As Long
    /// ]]>
    /// </example>
    /// <example hasResults="false">
    /// <![CDATA[
    /// Private internalFoo As Long
    /// 
    /// Public Property Get Foo() As Long
    ///     Foo = internalFoo
    /// End Property
    /// ]]>
    /// </example>
    public sealed class EncapsulatePublicFieldInspection : DeclarationInspectionBase
    {
        public EncapsulatePublicFieldInspection(IDeclarationFinderProvider declarationFinderProvider)
            : base(declarationFinderProvider, DeclarationType.Variable)
        {}

        protected override bool IsResultDeclaration(Declaration declaration, DeclarationFinder finder)
        {
            // we're creating a public field for every control on a form, needs to be ignored
            return declaration.DeclarationType != DeclarationType.Control
                   && (declaration.Accessibility == Accessibility.Public
                   || declaration.Accessibility == Accessibility.Global);
        }

        protected override string ResultDescription(Declaration declaration)
        {
            return string.Format(InspectionResults.EncapsulatePublicFieldInspection, declaration.IdentifierName);
        }
    }
}
