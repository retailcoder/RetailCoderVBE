﻿using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Rubberduck.CodeAnalysis.Inspections;
using Rubberduck.CodeAnalysis.Inspections.Concrete;
using Rubberduck.Parsing.VBA;
using Rubberduck.VBEditor.SafeComWrappers;
using Rubberduck.VBEditor.SafeComWrappers.Abstract;
using RubberduckTests.Mocks;

namespace RubberduckTests.Inspections
{
    [TestFixture]
    public class SheetAccessedUsingStringInspectionTests : InspectionTestsBase
    {
        [Test]
        [Category("Inspections")]
        public void SheetAccessedUsingString_ReturnsResult_AccessingUsingWorkbookModule()
        {
            const string inputCode =
                @"Public Sub Foo()
    ThisWorkbook.Worksheets(""Sheet1"").Range(""A1"") = ""Foo""
    ThisWorkbook.Sheets(""Sheet1"").Range(""A1"") = ""Foo""
End Sub";

            Assert.AreEqual(2, ArrangeParserAndGetResults(inputCode).Count());
        }

        [Test]
        [Category("Inspections")]
        public void SheetAccessedUsingString_DoesNotReturnResult_NoDocumentWithSheetName()
        {
            const string inputCode =
                @"Public Sub Foo()
    ThisWorkbook.Worksheets(""Sheet1"").Range(""A1"") = ""Foo""
    ThisWorkbook.Sheets(""Sheet1"").Range(""A1"") = ""Foo""
End Sub";

            Assert.AreEqual(0, ArrangeParserAndGetResults(inputCode, "NotSheet1").Count());
        }

        [Test]
        [Category("Inspections")]
        public void SheetAccessedUsingString_ReturnsResult_CodeNameAndSheetNameDifferent()
        {
            const string inputCode =
                @"Public Sub Foo()
    ThisWorkbook.Worksheets(""NotSheet1"").Range(""A1"") = ""Foo""
    ThisWorkbook.Sheets(""NotSheet1"").Range(""A1"") = ""Foo""
End Sub";

            Assert.AreEqual(2, ArrangeParserAndGetResults(inputCode, "NotSheet1").Count());
        }

        [Test]
        [Category("Inspections")]
        public void SheetAccessedUsingString_ReturnsResult_SheetNameContainsDoubleQuotes()
        {
            const string inputCode =
                @"Public Sub Foo()
    ThisWorkbook.Worksheets(""She""""et1"").Range(""A1"") = ""Foo""
    ThisWorkbook.Sheets(""She""""et1"").Range(""A1"") = ""Foo""
End Sub";

            Assert.AreEqual(2, ArrangeParserAndGetResults(inputCode, "She\"et1").Count());
        }

        [Test]
        [Category("Inspections")]
        //Access via Application is an access on the ActiveWorkbook, not necessarily ThisWorkbook.
        public void SheetAccessedUsingString_ReturnsNoResult_AccessingUsingApplicationModule()
        {
            const string inputCode =
                @"Public Sub Foo()
    Application.Worksheets(""Sheet1"").Range(""A1"") = ""Foo""
    Application.Sheets(""Sheet1"").Range(""A1"") = ""Foo""
End Sub";

            Assert.AreEqual(0, ArrangeParserAndGetResults(inputCode).Count());
        }

        [Test]
        [Category("Inspections")]
        //Unqualified access is an access on the ActiveWorkbook, not necessarily ThisWorkbook.
        public void SheetAccessedUsingString_ReturnsNoResult_AccessingUsingGlobalModule()
        {
            const string inputCode =
                @"Public Sub Foo()
    Worksheets(""Sheet1"").Range(""A1"") = ""Foo""
    Sheets(""Sheet1"").Range(""A1"") = ""Foo""
End Sub";

            Assert.AreEqual(0, ArrangeParserAndGetResults(inputCode).Count());
        }

        [Test]
        [Ignore("Ref #4329")]
        [Category("Inspections")]
        public void SheetAccessedUsingString_DoesNotReturnResult_AccessingUsingActiveWorkbookProperty()
        {
            const string inputCode =
                @"Public Sub Foo()
    ActiveWorkbook.Worksheets(""Sheet1"").Range(""A1"") = ""Foo""
    ActiveWorkbook.Sheets(""Sheet1"").Range(""A1"") = ""Foo""
End Sub";
            Assert.AreEqual(0, ArrangeParserAndGetResults(inputCode).Count());
        }

        [Test]
        [Ignore("Ref #4329")]
        [Category("Inspections")]
        public void SheetAccessedUsingString_DoesNotReturnResult_AccessingUsingWorkbooksProperty()
        {
            const string inputCode =
                @"Public Sub Foo()
    Workbooks(""Foo"").Worksheets(""Sheet1"").Range(""A1"") = ""Foo""
    Workbooks(""Foo"").Sheets(""Sheet1"").Range(""A1"") = ""Foo""
End Sub";
            Assert.AreEqual(0, ArrangeParserAndGetResults(inputCode).Count());
        }

        [Test]
        [Ignore("Ref #4329")]
        [Category("Inspections")]
        public void SheetAccessedUsingString_DoesNotReturnResult_AccessingUsingWorkbookVariable()
        {
            const string inputCode =
                @"Public Sub Foo()
    Dim wkb As Excel.Workbook
    Set wkb = Workbooks(""Foo"")
    wkb.Worksheets(""Sheet1"").Range(""A1"") = ""Foo""
    wkb.Sheets(""Sheet1"").Range(""A1"") = ""Foo""
End Sub";
            Assert.AreEqual(0, ArrangeParserAndGetResults(inputCode).Count());
        }

        [Test]
        [Ignore("Ref #4329")]
        [Category("Inspections")]
        public void SheetAccessedUsingString_DoesNotReturnResult_AccessingUsingWorkbookProperty()
        {
            const string inputCode =
                @"
Public Property Get MyWorkbook() As Excel.Workbook
    Set MyWorkbook = Workbooks(""Foo"")
End Property

Public Sub Foo()
    MyWorkbook.Worksheets(""Sheet1"").Range(""A1"") = ""Foo""
    MyWorkbook.Sheets(""Sheet1"").Range(""A1"") = ""Foo""
End Sub";
            Assert.AreEqual(0, ArrangeParserAndGetResults(inputCode).Count());
        }

        [Test]
        [Ignore("Ref #4329")]
        [Category("Inspections")]
        public void SheetAccessedUsingString_DoesNotReturnResult_AccessingUsingWithBlockVariable()
        {
            const string inputCode =
                @"Public Sub Foo()
    With Workbooks(""Foo"")
        .Worksheets(""Sheet1"").Range(""A1"") = ""Foo""
        .Sheets(""Sheet1"").Range(""A1"") = ""Foo""
    End With
End Sub";
            Assert.AreEqual(0, ArrangeParserAndGetResults(inputCode).Count());
        }

        [Test]
        [Category("Inspections")]
        public void SheetAccessedUsingString_DoesNotReturnResult_NoSheetWithGivenNameExists()
        {
            const string inputCode =
                @"Public Sub Foo()
    ThisWorkbook.Worksheets(""BadName"").Range(""A1"") = ""Foo""
    ThisWorkbook.Sheets(""BadName"").Range(""A1"") = ""Foo""
End Sub";

            Assert.AreEqual(0, ArrangeParserAndGetResults(inputCode).Count());
        }

        [Test]
        [Category("Inspections")]
        public void SheetAccessedUsingString_DoesNotReturnResult_SheetWithGivenNameExistsInAnotherProject()
        {
            const string inputCode =
                @"Public Sub Foo()
    ThisWorkbook.Worksheets(""SheetFromOtherProject"").Range(""A1"") = ""Foo""
    ThisWorkbook.Sheets(""SheetFromOtherProject"").Range(""A1"") = ""Foo""
End Sub";

            // Referenced project is created inside helper method
            Assert.AreEqual(0, ArrangeParserAndGetResults(inputCode).Count());
        }

        [Test]
        [Category("Inspections")]
        public void SheetAccessedUsingString_DoesNotReturnResult_AccessingUsingVariable()
        {
            const string inputCode =
                @"Public Sub Foo()
    Dim s As String
    s = ""Sheet1""

    ThisWorkbook.Worksheets(s).Range(""A1"") = ""Foo""
    ThisWorkbook.Sheets(s).Range(""A1"") = ""Foo""
End Sub";

            Assert.AreEqual(0, ArrangeParserAndGetResults(inputCode).Count());
        }

        private IEnumerable<IInspectionResult> ArrangeParserAndGetResults(string inputCode, string sheetName = "Sheet1")
        {
            var builder = new MockVbeBuilder();

            var referencedProject = builder.ProjectBuilder("ReferencedProject", ProjectProtection.Unprotected)
                .AddComponent("SheetFromOtherProject", ComponentType.Document, "",
                    properties: new[]
                    {
                        CreateVBComponentPropertyMock("Name", "SheetFromOtherProject").Object,
                        CreateVBComponentPropertyMock("CodeName", "SheetFromOtherProject").Object
                    })
                .Build();

            var project = builder.ProjectBuilder("VBAProject", ProjectProtection.Unprotected)
                .AddComponent("Module1", ComponentType.StandardModule, inputCode)
                .AddComponent("Sheet1", ComponentType.Document, "",
                    properties: new[]
                    {
                        CreateVBComponentPropertyMock("Name", sheetName).Object,
                        CreateVBComponentPropertyMock("CodeName", "Sheet1").Object
                    })
                .AddReference("ReferencedProject", string.Empty, 0, 0)
                .AddReference(ReferenceLibrary.Excel)
                .Build();

            var vbe = builder.AddProject(referencedProject).AddProject(project).Build();

            return InspectionResults(vbe.Object);
        }

        // ReSharper disable once InconsistentNaming
        private static Mock<IProperty> CreateVBComponentPropertyMock(string propertyName, string propertyValue)
        {
            var propertyMock = new Mock<IProperty>();
            propertyMock.SetupGet(m => m.Name).Returns(propertyName);
            propertyMock.SetupGet(m => m.Value).Returns(propertyValue);

            return propertyMock;
        }

        protected override IInspection InspectionUnderTest(RubberduckParserState state)
        {
            return new SheetAccessedUsingStringInspection(state, state.ProjectsProvider);
        }
    }
}
