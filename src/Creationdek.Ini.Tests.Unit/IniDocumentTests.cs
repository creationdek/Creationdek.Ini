using Creationdek.Ini;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Creationdek.IniConfig.Net.Tests.Unit
{
    public class IniDocumentTests
    {

        private string GenerateIniDocumentActualResults(int numberOfSections = 0)
        {
            var loadAll = numberOfSections == 0;
            var loadFirst = numberOfSections == 1;
            var loadSecond = numberOfSections == 2;
            var loadThird = numberOfSections == 3;
            var loadRemainder = numberOfSections == -1;

            var sb = new StringBuilder();

            sb.AppendLine("Creationdek.IniConfig.Net generated test file.".AsHeader());
            sb.AppendLine("Could be deleted if found.".AsHeader());

            if (loadAll || loadFirst)
            {
                sb.AppendLine("First Section.".AsComment());
                sb.AppendLine("Person".AsSection());
                sb.AppendLine("First Property".AsComment());
                sb.AppendLine("FirstName=Jon");
                sb.AppendLine("Second Property".AsComment());
                sb.AppendLine("LastName=Doe");
                sb.AppendLine("Third Property - Disabled".AsComment());
                sb.AppendLine("Age=30".AsDisabled());
            }

            if (loadAll || loadSecond || loadRemainder)
            {
                sb.AppendLine("Section 2.".AsComment());
                sb.AppendLine("Animal".AsSection());
                sb.AppendLine("Property 1".AsComment());
                sb.AppendLine("Kind=Cat");
                sb.AppendLine("Property 2".AsComment());
                sb.AppendLine("Color=Ash-Grey");
                sb.AppendLine("Property 3".AsComment());
                sb.AppendLine("Cry=Meeeoow!");
            }

            if (loadAll || loadThird || loadRemainder)
            {
                sb.AppendLine("Miss Section - Disabled.".AsComment());
                sb.AppendLine("Robot".AsSection().AsDisabled());
                sb.AppendLine("Property 1".AsComment());
                sb.AppendLine("BodyType=Iron".AsDisabled());
                sb.AppendLine("Property 2".AsComment());
                sb.AppendLine("Color=Green".AsDisabled());
                sb.AppendLine("Property 3".AsComment());
                sb.AppendLine("Model=3000-246".AsDisabled());
            }

            if (loadAll || loadRemainder)
            {
                sb.AppendLine($"Generated on: {DateTime.Now.Date}.".AsFooter());
            }

            return sb.ToString().Trim();
        }

        private string GenerateIniDocument(int numberOfSections = 0)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Creationdek.IniConfig.Net generated test file.".AsHeader());
            sb.AppendLine("Could be deleted if found.".AsHeader());

            if (numberOfSections < 1 || numberOfSections == 1)
            {
                sb.AppendLine("First Section.".AsComment());
                sb.AppendLine("Person".AsSection());
                sb.AppendLine("First Property".AsComment());
                sb.AppendLine("FirstName=Jon");
                sb.AppendLine("Second Property".AsComment());
                sb.AppendLine("LastName=Doe");
                sb.AppendLine("Third Property - Disabled".AsComment());
                sb.AppendLine("Age=30".AsDisabled());
            }

            if (numberOfSections < 1 || numberOfSections == 2)
            {
                sb.AppendLine("Section 2.".AsComment());
                sb.AppendLine("Animal".AsSection());
                sb.AppendLine("Property 1".AsComment());
                sb.AppendLine("Kind=Cat");
                sb.AppendLine("Property 2".AsComment());
                sb.AppendLine("Color=Ash-Grey");
                sb.AppendLine("Property 3".AsComment());
                sb.AppendLine("Cry=Meeeoow!");
            }

            if (numberOfSections < 1 || numberOfSections == 3)
            {
                sb.AppendLine("Miss Section - Disabled.".AsComment());
                sb.AppendLine("Robot".AsSection().AsDisabled());
                sb.AppendLine("Property 1".AsComment());
                sb.AppendLine("BodyType=Iron");
                sb.AppendLine("Property 2".AsComment());
                sb.AppendLine("Color=Green");
                sb.AppendLine("Property 3".AsComment());
                sb.AppendLine("Model=3000-246");
            }

            sb.AppendLine($"Generated on: {DateTime.Now.Date}.".AsFooter());

            return sb.ToString().Trim();
        }

        private IniDocument GenerateIniObject(string doc)
        {
            return IniDocument.Builder().Parse(doc).Build();
        }

        //private async Task<bool> CreateIniFileAsync(int numberOfSections = 0)
        //{
        //    var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "tmp.unit-test-file_7.ini");

        //    var doc = GenerateIniObject(GenerateIniDocument(numberOfSections));
        //    await doc.WriteAsync(file);
        //    return File.Exists(file);
        //}

        private async Task<(bool success, string path)> CreateIniFileAsync(int numberOfSections = 0)
        {
            var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $"tmp.unit-test-file_{numberOfSections}.ini");

            var doc = GenerateIniObject(GenerateIniDocument(numberOfSections));
            await doc.WriteAsync(file);
            return (File.Exists(file), file);
        }

        [Fact]
        public void IniDocumentBuilder_FluentApi_ShouldCreateIniDocument()
        {
            var actual = IniDocument
                .Builder(
                    new[] { "header line 1", "header line 2" },
                    new[] { "footer line 1", "footer line 2" },
                    true,
                    Section.Builder("person", properties: Property.Builder("name").Build()).Build(),
                    Section.Builder("animal", properties: Property.Builder("kind").Build()).Build())
                .Build();

            var expected = new StringBuilder()
                .AppendLine("header line 1".AsHeader())
                .AppendLine("header line 2".AsHeader())
                .AppendLine("person".AsSection())
                .AppendLine("name=")
                .AppendLine("animal".AsSection())
                .AppendLine("kind=")
                .AppendLine("footer line 1".AsFooter())
                .AppendLine("footer line 2".AsFooter())
                .ToString().Trim();

            Assert.Equal(expected, actual.ToString());
        }

        [Fact]
        public void IniDocumentBuilderDotBuild_ShouldCreateAnEmptyDocument()
        {
            var doc = IniDocument.Builder().Build();
            Assert.True(doc.IsEmpty);
        }

        [Fact]
        public void IsEmpty_ShouldReturnTrue()
        {
            var doc = IniDocument.Builder().Build();
            Assert.True(doc.IsEmpty);
            Assert.Equal(0, doc.SectionCount);
            Assert.Equal("", doc.ToString());
        }

        [Fact]
        public void FileName_ShouldSetAFileToSaveTo()
        {
            var doc = IniDocument.Builder().SetFile("c:\\tmp\\file.ini").Build();
            Assert.True(!string.IsNullOrWhiteSpace(doc.FilePath));
        }

        [Fact]
        public void IsEnabled_ShouldSetTheDocumentStatusToTheGivenValue()
        {
            var doc = IniDocument.Builder().Build();
            Assert.True(doc.IsEnabled);
            doc = IniDocument.Builder(doc).IsEnabled(false).Build();
            Assert.False(doc.IsEnabled);
        }

        [Fact]
        public void WithHeader_ShouldReplaceExistingHeader()
        {
            var doc = IniDocument
                .Builder()
                .Build();
            Assert.True(doc.Header.IsEmpty);
            Assert.Equal(CommentType.Header, doc.Header.Type);

            doc = IniDocument
                .Builder(doc)
                .WithHeader(Comment
                    .Builder()
                    .AppendLine("CommentLine1")
                    .Build())
                .Build();
            Assert.False(doc.Header.IsEmpty);

            var doc1 = IniDocument
                .Builder(doc)
                .WithHeader(null)
                .Build();
            Assert.True(doc1.Header.IsEmpty);

            var doc2 = IniDocument
                .Builder(doc)
                .WithHeader(Comment.Builder().Build())
                .Build();
            Assert.True(doc2.Header.IsEmpty);
            Assert.Equal(CommentType.Header, doc2.Header.Type);
        }

        [Fact]
        public void WithFooter_ShouldReplaceExistingFooter()
        {
            var doc = IniDocument
                .Builder()
                .Build();
            Assert.True(doc.Footer.IsEmpty);
            Assert.Equal(CommentType.Footer, doc.Footer.Type);

            doc = IniDocument
                .Builder(doc)
                .WithFooter(Comment
                    .Builder()
                    .AppendLine("CommentLine1")
                    .Build())
                .Build();
            Assert.False(doc.Footer.IsEmpty);

            var doc1 = IniDocument
                .Builder(doc)
                .WithFooter(null)
                .Build();
            Assert.True(doc1.Footer.IsEmpty);

            var doc2 = IniDocument
                .Builder(doc)
                .WithFooter(Comment.Builder().Build())
                .Build();
            Assert.True(doc2.Footer.IsEmpty);
            Assert.Equal(CommentType.Footer, doc2.Footer.Type);
        }

        [Fact]
        public void Merge_ShouldAddTheUniqueSectionsAndPropertiesFromTheOtherDocument()
        {
            var doc = IniDocument
                .Builder()
                .AppendSection(Section
                    .Builder()
                    .WithName("Things")
                    .AppendProperty(Property
                        .Builder()
                        .WithKey("foo")
                        .Build())
                    .Build())
                .Build();
            Assert.False(doc.IsEmpty);

            var sec1 = Section
                .Builder()
                .WithName("Things")
                .AppendProperty(Property
                    .Builder()
                    .WithKey("foo")
                    .Build())
                .AppendProperty(Property
                    .Builder()
                    .WithKey("fizz")
                    .WithValue("buzz")
                    .Build())
                .Build();
            Assert.False(sec1.IsEmpty);

            var sec2 = Section
                .Builder()
                .WithName("Person")
                .AppendProperty(Property
                    .Builder()
                    .WithKey("firstname")
                    .WithValue("jon")
                    .Build())
                .Build();
            Assert.False(sec2.IsEmpty);

            var doc2 = IniDocument
                .Builder()
                .AppendSection(sec1)
                .AppendSection(sec2)
                .Build();
            Assert.Equal(2, doc2.SectionCount);

            doc = IniDocument
                .Builder(doc)
                .Merge(doc2)
                .Merge(null)
                .Build();
            Assert.Equal(2, doc.SectionCount);
            Assert.Equal(2, doc.Sections()[0].PropertyCount);
            Assert.Equal(1, doc.Sections()[1].PropertyCount);
        }

        [Fact]
        public void Sections_ShouldReturnResultBasedOnStatusFilter()
        {
            var doc = IniDocument
                .Builder()
                .WithHeader(Comment.Builder().AppendLine("header comment").Build())
                .WithFooter(Comment.Builder().AppendLine("footer comment").Build())
                .AppendSection(Section
                    .Builder()
                    .WithComment(Comment
                        .Builder()
                        .AppendLine("section comment")
                        .Build())
                    .WithName("section")
                    .AppendProperty(Property
                        .Builder()
                        .WithComment(Comment
                            .Builder()
                            .AppendLine("property comment")
                            .Build())
                        .WithKey("foo")
                        .Build())
                    .AppendProperty(Property
                        .Builder()
                        .IsEnable(false)
                        .WithKey("foo")
                        .Build())
                    .Build())
                .AppendSection(Section
                    .Builder()
                    .IsEnable(false)
                    .WithName("Things")
                    .AppendProperty(Property
                        .Builder()
                        .WithKey("foo")
                        .Build())
                    .AppendProperty(Property
                        .Builder()
                        .WithKey("fizz")
                        .WithValue("buzz")
                        .Build())
                    .Build())
                .Build();
            Assert.False(doc.IsEmpty);
            Assert.Equal(2, doc.Sections().Count);
            Assert.Equal(2, doc.Sections(Status.All).Count);
            Assert.Equal(2, doc.Sections((Status)999).Count);
            Assert.Equal(1, doc.Sections(Status.Enabled).Count);
            Assert.Equal(1, doc.Sections(Status.Disabled).Count);
        }

        [Fact]
        public void Parse_ShouldReturnAnIniDocumentIfTheGivenTextIsValidAndAnEmptyIniDocumentOtherwise()
        {
            var text = new StringBuilder()
                .AppendLine("header".AsHeader())
                .AppendLine("section 1 comment".AsComment())
                .AppendLine("section1".AsSection())
                .AppendLine("property 1 comment".AsComment())
                .AppendLine("key1=val1")
                .AppendLine("property 2 comment".AsComment())
                .AppendLine("key2=val2")
                .AppendLine("section 2 comment".AsComment())
                .AppendLine("section2".AsSection())
                .AppendLine("property 01 comment".AsComment())
                .AppendLine("key01=val01")
                .AppendLine("property 02 comment".AsComment())
                .AppendLine("key02=val02")
                .AppendLine("footer".AsFooter())
                .ToString().Trim();

            var doc = IniDocument
                .Builder()
                .Parse(text)
                .Build();
            Assert.False(doc.IsEmpty);
            Assert.Equal(text, doc.ToString());

            doc = IniDocument
                .Builder()
                .Parse(null)
                .Build();

            Assert.True(doc.IsEmpty);
            Assert.Equal("", doc.ToString());
        }

        [Fact]
        public void AppendProperty_ShouldAddAPropertyToAValidSectionIfThePropertyIsUnique()
        {
            var property = Property
                .Builder()
                .WithComment(Comment
                    .Builder()
                    .AppendLine("property comment")
                    .Build())
                .WithKey("foo")
                .WithValue("bar")
                .Build();
            Assert.False(property.IsEmpty);

            var doc = IniDocument
                .Builder()
                .AppendSection(Section
                    .Builder()
                    .WithName("section")
                    .Build())
                .Build();
            Assert.True(doc.Sections()[0].IsEmpty);
            Assert.True(doc.IsEmpty);
            Assert.Single(doc.Sections());
            Assert.Equal("section", doc.Sections()[0].Name);

            var clone = IniDocument
                .Builder(doc)
                .AppendProperty("section", property)
                .Build();
            Assert.Equal(clone.Sections()[0].Name, doc.Sections()[0].Name);
            Assert.Single(clone.Sections());
            Assert.Single(clone.Sections()[0].Properties());
            Assert.False(clone.Sections()[0].IsEmpty);
            Assert.False(clone.IsEmpty);

            var clone2 = IniDocument
                .Builder(doc)
                .AppendProperty("section2", property)
                .AppendProperty("section2", property)
                .AppendProperty("section2", null)
                .AppendProperty(" ", property)
                .AppendProperty("", property)
                .AppendProperty(null, property)
                .AppendProperty(null, null)
                .AppendProperty("", null)
                .AppendProperty(" ", null)
                .AppendProperty("test", null)
                .Build();
            Assert.Equal(clone2.Sections()[0].Name, doc.Sections()[0].Name);
            Assert.Equal(2, clone2.Sections().Count);
            Assert.Empty(clone2.Sections()[0].Properties());
            Assert.True(clone2.Sections()[0].IsEmpty);
            Assert.Single(clone2.Sections()[1].Properties());
            Assert.False(clone2.Sections()[1].IsEmpty);
            Assert.False(clone2.IsEmpty);
        }

        [Fact]
        public void RemoveSection_ShouldRemoveSectionIfFound()
        {
            var doc = IniDocument
                .Builder()
                .WithHeader(Comment.Builder().AppendLine("header comment").Build())
                .WithFooter(Comment.Builder().AppendLine("footer comment").Build())
                .AppendSection(Section
                    .Builder()
                    .WithComment(Comment
                        .Builder()
                        .AppendLine("section comment")
                        .Build())
                    .WithName("section")
                    .AppendProperty(Property
                        .Builder()
                        .WithComment(Comment
                            .Builder()
                            .AppendLine("property comment")
                            .Build())
                        .WithKey("foo")
                        .Build())
                    .AppendProperty(Property
                        .Builder()
                        .IsEnable(false)
                        .WithKey("foo")
                        .Build())
                    .Build())
                .AppendSection(Section
                    .Builder()
                    .IsEnable(false)
                    .WithName("Things")
                    .AppendProperty(Property
                        .Builder()
                        .WithKey("foo")
                        .Build())
                    .AppendProperty(Property
                        .Builder()
                        .WithKey("fizz")
                        .WithValue("buzz")
                        .Build())
                    .Build())
                .Build();

            var actual = IniDocument
                .Builder(doc)
                .RemoveSection("Things")
                .RemoveSection(" ")
                .RemoveSection("")
                .RemoveSection(null)
                .Build();

            Assert.Equal(1, actual.SectionCount);
            Assert.Equal("section", actual.Sections()[0].Name);
        }

        [Fact]
        public void RemoveSectionAt_ShouldRemoveSectionIfFound()
        {
            var doc = IniDocument
                .Builder()
                .WithHeader(Comment.Builder().AppendLine("header comment").Build())
                .WithFooter(Comment.Builder().AppendLine("footer comment").Build())
                .AppendSection(Section
                    .Builder()
                    .WithComment(Comment
                        .Builder()
                        .AppendLine("section comment")
                        .Build())
                    .WithName("section")
                    .AppendProperty(Property
                        .Builder()
                        .WithComment(Comment
                            .Builder()
                            .AppendLine("property comment")
                            .Build())
                        .WithKey("foo")
                        .Build())
                    .AppendProperty(Property
                        .Builder()
                        .IsEnable(false)
                        .WithKey("foo")
                        .Build())
                    .Build())
                .AppendSection(Section
                    .Builder()
                    .IsEnable(false)
                    .WithName("Things")
                    .AppendProperty(Property
                        .Builder()
                        .WithKey("foo")
                        .Build())
                    .AppendProperty(Property
                        .Builder()
                        .WithKey("fizz")
                        .WithValue("buzz")
                        .Build())
                    .Build())
                .Build();

            var actual = IniDocument
                .Builder(doc)
                .RemoveSectionAt(1)
                .RemoveSectionAt(-4)
                .RemoveSectionAt(999)
                .Build();

            Assert.Equal(1, actual.SectionCount);
            Assert.Equal("section", actual.Sections()[0].Name);
        }

        [Fact]
        public void RemoveProperty_ShouldRemoveTheGivenPropertyFromTheGivenSectionIfFound()
        {
            var doc = IniDocument
                .Builder()
                .WithHeader(Comment.Builder().AppendLine("header comment").Build())
                .WithFooter(Comment.Builder().AppendLine("footer comment").Build())
                .AppendSection(Section
                    .Builder()
                    .WithComment(Comment
                        .Builder()
                        .AppendLine("section comment")
                        .Build())
                    .WithName("section")
                    .AppendProperty(Property
                        .Builder()
                        .WithComment(Comment
                            .Builder()
                            .AppendLine("property comment")
                            .Build())
                        .WithKey("foo")
                        .WithValue("bar")
                        .Build())
                    .AppendProperty(Property
                        .Builder()
                        .IsEnable(false)
                        .WithKey("foo")
                        .WithValue("bared")
                        .Build())
                    .Build())
                .AppendSection(Section
                    .Builder()
                    .IsEnable(false)
                    .WithName("Things")
                    .AppendProperty(Property
                        .Builder()
                        .WithKey("foo")
                        .Build())
                    .AppendProperty(Property
                        .Builder()
                        .WithKey("fizz")
                        .WithValue("buzz")
                        .Build())
                    .Build())
                .Build();
            Assert.Equal(2, doc.Sections()[0].PropertyCount);
            Assert.Equal(2, doc.Sections()[1].PropertyCount);

            var actual = IniDocument
                .Builder(doc)
                .RemoveProperty(null, null, null)
                .RemoveProperty("section", null, null)
                .RemoveProperty(null, "foo", null)
                .RemoveProperty(null, null, "bar")
                .RemoveProperty("section", "foo", "bared")
                .RemoveProperty("Things", "foo", null)
                .Build();
            Assert.Equal(1, actual.Sections()[0].PropertyCount);
            Assert.Equal(1, actual.Sections()[1].PropertyCount);
            Assert.Collection(actual.Sections()[0].Properties(),
                i =>
                {
                    Assert.Equal("foo", i.Key);
                    Assert.Equal("bar", i.Value);
                });
            Assert.Collection(actual.Sections()[1].Properties(),
                i =>
                {
                    Assert.Equal("fizz", i.Key);
                    Assert.Equal("buzz", i.Value);
                });
        }

        [Fact]
        public void RemovePropertyAt_ShouldRemoveTheGivenPropertyFromTheGivenSectionIfFound()
        {
            var doc = IniDocument
                .Builder()
                .WithHeader(Comment.Builder().AppendLine("header comment").Build())
                .WithFooter(Comment.Builder().AppendLine("footer comment").Build())
                .AppendSection(Section
                    .Builder()
                    .WithComment(Comment
                        .Builder()
                        .AppendLine("section comment")
                        .Build())
                    .WithName("section")
                    .AppendProperty(Property
                        .Builder()
                        .WithComment(Comment
                            .Builder()
                            .AppendLine("property comment")
                            .Build())
                        .WithKey("foo")
                        .WithValue("bar")
                        .Build())
                    .AppendProperty(Property
                        .Builder()
                        .IsEnable(false)
                        .WithKey("foo")
                        .WithValue("bared")
                        .Build())
                    .Build())
                .AppendSection(Section
                    .Builder()
                    .IsEnable(false)
                    .WithName("Things")
                    .AppendProperty(Property
                        .Builder()
                        .WithKey("foo")
                        .Build())
                    .AppendProperty(Property
                        .Builder()
                        .WithKey("fizz")
                        .WithValue("buzz")
                        .Build())
                    .Build())
                .Build();
            Assert.Equal(2, doc.Sections()[0].PropertyCount);
            Assert.Equal(2, doc.Sections()[1].PropertyCount);

            var actual = IniDocument
                .Builder(doc)
                .RemovePropertyAt(null, -1)
                .RemovePropertyAt("section", -999)
                .RemovePropertyAt(null, 999)
                .RemovePropertyAt("Things", 5)
                .RemovePropertyAt("section", 1)
                .RemovePropertyAt("Things", 0)
                .Build();
            Assert.Equal(1, actual.Sections()[0].PropertyCount);
            Assert.Equal(1, actual.Sections()[1].PropertyCount);
            Assert.Collection(actual.Sections()[0].Properties(),
                i =>
                {
                    Assert.Equal("foo", i.Key);
                    Assert.Equal("bar", i.Value);
                });
            Assert.Collection(actual.Sections()[1].Properties(),
                i =>
                {
                    Assert.Equal("fizz", i.Key);
                    Assert.Equal("buzz", i.Value);
                });
        }

        [Fact]
        public void IsEnabled_ShouldRepresentStringBasedOnIsEnabled()
        {
            var ini = IniDocument
                .Builder()
                .AppendSection(Section
                .Builder()
                .WithName("Person")
                .AppendProperty(Property
                    .Builder()
                    .WithKey("name")
                    .WithValue("joy")
                    .Build())
                .AppendProperty(Property
                    .Builder()
                    .IsEnable(false)
                    .WithKey("age")
                    .WithValue("17")
                    .Build())
                .Build())
                .AppendSection(Section
                .Builder()
                    .IsEnable(false)
                .WithName("Pet")
                .AppendProperty(Property
                    .Builder()
                    .WithKey("Kind")
                    .WithValue("Cat")
                    .Build())
                .Build())
                .Build();

            var expected = new StringBuilder()
                .AppendLine("Person".AsSection())
                .AppendLine("name=joy")
                .AppendLine("age=17".AsDisabled())
                .AppendLine("Pet".AsSection().AsDisabled())
                .AppendLine("Kind=Cat".AsDisabled())
                .ToString().Trim();

            Assert.True(ini.IsEnabled);
            Assert.Equal(expected, ini.ToString());

            var dIni = IniDocument
                .Builder(ini)
                .IsEnabled(false)
                .Build();

            var dExpected = new StringBuilder()
                .AppendLine("Person".AsSection().AsDisabled())
                .AppendLine("name=joy".AsDisabled())
                .AppendLine("age=17".AsDisabled())
                .AppendLine("Pet".AsSection().AsDisabled())
                .AppendLine("Kind=Cat".AsDisabled())
                .ToString().Trim();

            Assert.False(dIni.IsEnabled);
            Assert.Equal(dExpected, dIni.ToString());
        }

        [Fact]
        public async Task LoadIniAsync_ShouldCreateObjectFromValidIniFileAsync()
        {
            var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "tmp.unit-test-file.ini");
            await file.CreateFileAsync(GenerateIniObject(GenerateIniDocument()).ToString());
            Assert.True(File.Exists(file));

            var doc = IniDocument
                .Builder()
                .SetFile(file)
                .LoadIniAsync()
                .GetAwaiter()
                .GetResult()
                .Build();
            Assert.Equal(GenerateIniDocumentActualResults(), doc.ToString());


            File.Delete(file);
            Assert.False(File.Exists(file));
        }

        [Fact]
        public async Task LoadIniAsync_GivenNumberOfSections_ShouldOnlyLoadThatNumberOfSectionsAsync()
        {
            var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "tmp.unit-test-file_2.ini");
            await file.CreateFileAsync(GenerateIniObject(GenerateIniDocument()).ToString());
            Assert.True(File.Exists(file));

            var doc = IniDocument
                .Builder()
                .SetFile(file)
                .LoadIniAsync(1)
                .GetAwaiter()
                .GetResult()
                .Build();
            Assert.Equal(GenerateIniDocumentActualResults(1), doc.ToString());


            File.Delete(file);
            Assert.False(File.Exists(file));

            var emptyDoc = IniDocument
                            .Builder()
                            .SetFile("")
                            .LoadIniAsync()
                            .GetAwaiter()
                            .GetResult()
                            .Build();
            Assert.Equal("", emptyDoc.ToString());
            emptyDoc = IniDocument
                            .Builder()
                            .SetFile(null)
                            .LoadIniAsync()
                            .GetAwaiter()
                            .GetResult()
                            .Build();
            Assert.Equal("", emptyDoc.ToString());
            emptyDoc = IniDocument
                            .Builder()
                            .LoadIniAsync()
                            .GetAwaiter()
                            .GetResult()
                            .Build();
            Assert.Equal("", emptyDoc.ToString());
        }

        [Fact]
        public async Task LoadNextAsync_GivenNumberOfSections_ShouldOnlyLoadThatNextNumberOfSectionsAsync()
        {
            var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "tmp.unit-test-file_2.ini");
            await file.CreateFileAsync(GenerateIniObject(GenerateIniDocument()).ToString());
            Assert.True(File.Exists(file));

            var doc = IniDocument
                .Builder()
                .SetFile(file)
                .LoadIniAsync(1)
                .GetAwaiter()
                .GetResult()
                .Build();
            Assert.Equal(GenerateIniDocumentActualResults(1), doc.ToString());

            var next = IniDocument
                .Builder(doc)
                .LoadNextAsync(1)
                .GetAwaiter()
                .GetResult()
                .Build();
            Assert.Equal(GenerateIniDocumentActualResults(2), next.ToString());


            File.Delete(file);
            Assert.False(File.Exists(file));

            var emptyDoc = IniDocument
                            .Builder()
                            .SetFile("")
                            .LoadNextAsync()
                            .GetAwaiter()
                            .GetResult()
                            .Build();
            Assert.Equal("", emptyDoc.ToString());
            emptyDoc = IniDocument
                            .Builder()
                            .SetFile(null)
                            .LoadNextAsync()
                            .GetAwaiter()
                            .GetResult()
                            .Build();
            Assert.Equal("", emptyDoc.ToString());
            emptyDoc = IniDocument
                            .Builder()
                            .LoadNextAsync()
                            .GetAwaiter()
                            .GetResult()
                            .Build();
            Assert.Equal("", emptyDoc.ToString());
        }

        [Fact]
        public async Task LoadNextAsync_Given0MeaningAllTheRest_ShouldLoadTheRemainingSectionsAsync()
        {
            var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "tmp.unit-test-file_2.ini");
            await file.CreateFileAsync(GenerateIniObject(GenerateIniDocument()).ToString());
            Assert.True(File.Exists(file));

            var doc = IniDocument
                .Builder()
                .SetFile(file)
                .LoadIniAsync(1)
                .GetAwaiter()
                .GetResult()
                .Build();
            Assert.Equal(GenerateIniDocumentActualResults(1), doc.ToString());

            var next = IniDocument
                .Builder(doc)
                .LoadNextAsync()
                .GetAwaiter()
                .GetResult()
                .Build();
            Assert.Equal(GenerateIniDocumentActualResults(-1), next.ToString());


            File.Delete(file);
            Assert.False(File.Exists(file));

            var emptyDoc = IniDocument
                            .Builder()
                            .SetFile("")
                            .LoadNextAsync()
                            .GetAwaiter()
                            .GetResult()
                            .Build();
            Assert.Equal("", emptyDoc.ToString());
            emptyDoc = IniDocument
                            .Builder()
                            .SetFile(null)
                            .LoadNextAsync()
                            .GetAwaiter()
                            .GetResult()
                            .Build();
            Assert.Equal("", emptyDoc.ToString());
            emptyDoc = IniDocument
                            .Builder()
                            .LoadNextAsync()
                            .GetAwaiter()
                            .GetResult()
                            .Build();
            Assert.Equal("", emptyDoc.ToString());
        }

        [Fact]
        public async Task WriteAsync_ShouldWriteDocumentToDiskAsync()
        {
            var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "tmp.unit-test-file_3.ini");
            //Assert.False(File.Exists(file));

            var doc = GenerateIniObject(GenerateIniDocument());
            await doc.WriteAsync(file);
            Assert.True(File.Exists(file));

            File.Delete(file);
            Assert.False(File.Exists(file));
        }

        [Fact]
        public async Task WriteAsync_GivenPathThatDontExists_ShouldWriteToDiskTheSectionOrPropertyAsync()
        {
            var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "tmp.unit-test-file_4.ini");

            await IniDocument.WriteAsync(file, "Person", "name", "jon");
            Assert.True(File.Exists(file));
            Assert.True(new FileInfo(file).Length > 0);

            File.Delete(file);
            Assert.False(File.Exists(file));
        }

        [Fact]
        public async Task WriteAsync_GivenGivenExistingFileAndNotExistingSection_ShouldShouldAddSectionAndPropertyAsync()
        {
            var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "tmp.unit-test-file_5.ini");

            var doc = GenerateIniObject(GenerateIniDocument(1));
            await doc.WriteAsync(file);
            Assert.True(File.Exists(file));
            Assert.True(new FileInfo(file).Length > 0);

            await IniDocument.WriteAsync(file, "Insect", "Type", "Fly");
            Assert.True(File.Exists(file));

            File.Delete(file);
            Assert.False(File.Exists(file));
        }

        [Fact]
        public async Task WriteAsync_GivenGivenExistingFileAndExistingSectionAndNotExistingProperty_ShouldShouldAddPropertyAsync()
        {
            var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "tmp.unit-test-file_6.ini");

            var doc = GenerateIniObject(GenerateIniDocument(1));
            await doc.WriteAsync(file);
            Assert.True(File.Exists(file));
            Assert.True(new FileInfo(file).Length > 0);

            await IniDocument.WriteAsync(file, "Person", "Sex", "Male");
            Assert.True(File.Exists(file));

            File.Delete(file);
            Assert.False(File.Exists(file));
        }

        [Fact]
        public async Task WriteAsync_GivenGivenExistingFileAndExistingSectionAndExistingPropertyWithDifferentValueUpdateProperty_ShouldUpdateExistingPropertyAsync()
        {
            var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "tmp.unit-test-file_7.ini");

            var doc = GenerateIniObject(GenerateIniDocument(1));
            await doc.WriteAsync(file);
            //Assert.True(File.Exists(file));
            //Assert.True(new FileInfo(file).Length > 0);

            await IniDocument.WriteAsync(file, "Person", "Age", "90", true);
            //Assert.True(File.Exists(file));

            var sec = await IniDocument.ReadSectionAsync(file, "Person");

            Assert.Collection(sec.Properties(),
                s => Assert.Equal("FirstName", s.Key),
                s => Assert.Equal("LastName", s.Key),
                s => Assert.Equal("Age", s.Key));

            File.Delete(file);
            Assert.False(File.Exists(file));
        }

        [Fact]
        public async Task WriteAsync_GivenGivenExistingFileAndExistingSectionAndExistingPropertyWithDifferentValueDontUpdateProperty_ShouldAddPropertyAsync()
        {
            var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "tmp.unit-test-file_7.ini");

            var doc = GenerateIniObject(GenerateIniDocument(1));
            await doc.WriteAsync(file);
            //Assert.True(File.Exists(file));
            //Assert.True(new FileInfo(file).Length > 0);

            await IniDocument.WriteAsync(file, "Person", "Age", "90");
            //Assert.True(File.Exists(file));

            var sec = await IniDocument.ReadSectionAsync(file, "Person");

            Assert.Collection(sec.Properties(),
                s => Assert.Equal("FirstName", s.Key),
                s => Assert.Equal("LastName", s.Key),
                s => Assert.Equal("Age", s.Key),
                s => Assert.Equal("Age", s.Key));

            File.Delete(file);
            Assert.False(File.Exists(file));
        }

        [Fact]
        public async Task WriteAsync_ShouldWriteFileToNonExistingFolder()
        {
            var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "test", "tmp.unit-test-file_0.ini");

            var doc = GenerateIniObject(GenerateIniDocument(1));
            await doc.WriteAsync(file);

            await IniDocument.WriteAsync(file, "Alien", "kind", "log jaw");

            var sec = await IniDocument.ReadSectionAsync(file, "Person");

            Assert.Collection(sec.Properties(),
                s => Assert.Equal("FirstName", s.Key),
                s => Assert.Equal("LastName", s.Key),
                s => Assert.Equal("Age", s.Key));

            sec = await IniDocument.ReadSectionAsync(file, "Alien");

            Assert.Collection(sec.Properties(),
                s => Assert.Equal("kind", s.Key));
            Assert.Collection(sec.Properties(),
                s => Assert.Equal("log jaw", s.Value));

            File.Delete(file);
            Assert.False(File.Exists(file));
            Directory.Delete(Path.GetDirectoryName(file));
            Assert.False(Directory.Exists(Path.GetDirectoryName(file)));
        }

        [Fact]
        public async Task ReadSection_ShouldReadSectionIfItExistsAsync()
        {
            var createFile = await CreateIniFileAsync();
            Assert.True(createFile.success);
            Assert.True(createFile.path.IsValidFile());

            var section = await IniDocument.ReadSectionAsync(createFile.path, "Animal");
            Assert.Equal("Animal", section.Name);

            File.Delete(createFile.path);
            Assert.False(File.Exists(createFile.path));
        }

        [Fact]
        public async Task ReadAsync_ShouldReadPropertyValueIfItExistsAsync()
        {
            var createFile = await CreateIniFileAsync();
            Assert.True(createFile.success);
            Assert.True(createFile.path.IsValidFile());

            var value = await IniDocument.ReadAsync(createFile.path, "Animal", "Color");
            Assert.Equal("Ash-Grey", value);

            File.Delete(createFile.path);
            Assert.False(File.Exists(createFile.path));
        }
    }
}