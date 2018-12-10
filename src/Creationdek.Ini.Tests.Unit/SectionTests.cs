using Creationdek.Ini;
using System;
using System.Text;
using Xunit;

namespace Creationdek.IniConfig.Net.Tests.Unit
{
    public class SectionTests
    {
        [Fact]
        public void SectionBuilder_FluentApi_ShouldCreateSectionWithMultiLineComment()
        {
            var actual = Section
                .Builder("person", new[] { "comment line 1", "comment line 2", "comment line 3" }, true, Property.Builder("name").Build(), Property.Builder("age").Build(), Property.Builder("sex").Build())
                .Build();

            var expected = new StringBuilder()
                    .AppendLine("comment line 1".AsComment())
                    .AppendLine("comment line 2".AsComment())
                    .AppendLine("comment line 3".AsComment())
                    .AppendLine("person".AsSection())
                    .AppendLine("name=")
                    .AppendLine("age=")
                    .AppendLine("sex=")
                    .ToString().Trim();

            Assert.Equal(expected, actual.ToString());
        }

        [Fact]
        public void IsEmpty_GivenEmptySection_ShouldReturnTrueIfNameIsDefault()
        {
            var section = Section
                .Builder()
                .Build();

            Assert.True(section.IsEmpty);
            Assert.Equal("", section.ToString());
        }

        [Fact]
        public void IsEmpty_ShouldReturnTrueIfPropertyCountIsZero()
        {
            var section = Section
                .Builder()
                .WithName("test")
                .Build();
            Assert.Equal(0, section.PropertyCount);
            Assert.True(section.IsEmpty);
            Assert.Equal("", section.ToString());
        }

        [Fact]
        public void SectionBuilder_GivenAValidSection_ShouldCreateACloneOfIt()
        {
            var section = Section
                .Builder()
                .Build();

            var clone = Section
                .Builder(section)
                .Build();

            Assert.Equal(clone.ToString(), section.ToString());
            Assert.NotSame(clone.Properties(), section.Properties());
            Assert.NotSame(clone, section);
        }


        [Fact]
        public void IsEnable_ShouldSetTheStateOfTheSection()
        {
            var section = Section
                .Builder()
                .Build();
            Assert.True(section.IsEnabled);

            section = Section
                .Builder(section)
                .IsEnable(false)
                .Build();
            Assert.False(section.IsEnabled);
        }

        [Fact]
        public void WithName_ShouldSetSectionNameToGivenValidName()
        {
            var section = Section
                .Builder()
                .WithName("Hello Section")
                .Build();
            Assert.Equal("Hello Section", section.Name);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void WithName_ShouldThrowArgumentExceptionWhenGivenInvalidName(string data)
        {
            var exception = Assert.Throws<ArgumentException>(() => Section
                .Builder()
                .WithName(data)
                .Build());
            Assert.Equal("Section name cannot be null or whitespace.".AppendLine("Parameter name: name"), exception.Message);
        }

        [Fact]
        public void WithComment_ShouldReplaceCurrentCommentWithTheGivenComment()
        {
            var section = Section
                .Builder()
                .WithComment(Comment
                    .Builder()
                    .AppendLine("comment line 1")
                    .Build())
                .Build();

            Assert.Equal(1, section.Comment.LineCount);

            var emptyComment1 = Section
                .Builder(section)
                .WithComment(null)
                .Build();
            Assert.True(emptyComment1.Comment.IsEmpty);
            Assert.Equal("", emptyComment1.Comment.ToString());

            var emptyComment2 = Section
                .Builder(section)
                .WithComment(Comment.Builder().Build())
                .Build();
            Assert.True(emptyComment2.Comment.IsEmpty);
            Assert.Equal("", emptyComment2.Comment.ToString());
        }

        [Fact]
        public void AppendProperty_ShouldAddTheGivenPropertyIfValidToTheSection()
        {
            var section = Section
                .Builder()
                .WithName("Person")
                .AppendProperty(Property
                    .Builder()
                    .WithKey("foo")
                    .WithValue("bar")
                    .Build())
                .Build();
            Assert.Equal(1, section.PropertyCount);
            Assert.Equal("Person", section.Name);
            Assert.Equal("foo", section.Properties()[0].Key);

            var sec2 = Section
                .Builder(section)
                .AppendProperty(Property
                    .Builder()
                    .WithKey("foo")
                    .WithValue("bar")
                    .Build())
                .AppendProperty(null)
                .AppendProperty(Property
                    .Builder()
                    .Build())
                .AppendProperty(Property
                    .Builder()
                    .WithKey("fizz")
                    .WithValue("buzz")
                    .Build())
                .Build();
            Assert.Equal(3, sec2.PropertyCount);
            Assert.Equal("Person", sec2.Name);
            Assert.Equal(Lib.DefaultKeyName, sec2.Properties()[1].Key);
            Assert.Equal("foo", sec2.Properties()[0].Key);
            Assert.Equal("fizz", sec2.Properties()[2].Key);
        }

        [Fact]
        public void Merge_ShouldMergeUniquePropertiesFromTheGivenSection()
        {
            var sec1 = Section
                .Builder()
                .WithName("Person")
                .AppendProperty(Property
                    .Builder()
                    .WithKey("foo")
                    .WithValue("bar")
                    .Build())
                .AppendProperty(Property
                    .Builder()
                    .WithKey("fizz")
                    .WithValue("buzz")
                    .Build())
                .Build();

            var sec2 = Section
                .Builder()
                .WithName("Animal")
                .AppendProperty(Property
                    .Builder()
                    .WithKey("pet")
                    .WithValue("cat")
                    .Build())
                .Build();

            var section = Section
                .Builder()
                .WithName("Person")
                .AppendProperty(Property
                    .Builder()
                    .WithKey("foo")
                    .WithValue("bar")
                    .Build())
                .Merge(sec1)
                .Merge(sec2)
                .Build();

            Assert.Equal(3, section.PropertyCount);
        }

        [Fact]
        public void Parse_ShouldParseValidStringIntoSectionObject()
        {
            var sb = new StringBuilder()
                .AppendLine("sec comment".AsComment())
                .AppendLine("Person".AsSection())
                .AppendLine("prop comment")
                .AppendLine("name=jon")
                .AppendLine("pet=cat")
                .AppendLine("sec2 comment".AsComment())
                .AppendLine("Person2".AsSection())
                .AppendLine("pet=dog")
                .ToString().Trim();

            var section = Section
                .Builder()
                .Parse(sb)
                .Build();
            Assert.False(section.IsEmpty);
            Assert.Equal(2, section.PropertyCount);
        }

        [Fact]
        public void RemovePropertyAt_ShouldRemoveThePropertyAtTheIndexIfFound()
        {
            var sb = new StringBuilder()
                .AppendLine("sec comment".AsComment())
                .AppendLine("Person".AsSection())
                .AppendLine("prop comment")
                .AppendLine("name=jon")
                .AppendLine("pet=cat")
                .ToString().Trim();

            var section = Section
                .Builder()
                .Parse(sb)
                .RemovePropertyAt(0)
                .Build();

            Assert.Equal(1, section.PropertyCount);
            Assert.False(section.Property("pet").IsEmpty);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(3)]
        [InlineData(100)]
        public void RemovePropertyAt_ShouldDoNothingIfTheGivenNumberIsInvalid(int data)
        {
            var sb = new StringBuilder()
                .AppendLine("sec comment".AsComment())
                .AppendLine("Person".AsSection())
                .AppendLine("prop comment")
                .AppendLine("name=jon")
                .AppendLine("pet=cat")
                .ToString().Trim();

            var section = Section
                .Builder()
                .Parse(sb)
                .RemovePropertyAt(data)
                .Build();

            Assert.Equal(2, section.PropertyCount);
        }

        [Fact]
        public void RemoveProperty_ShouldRemoveThePropertyIfFound()
        {
            var sb = new StringBuilder()
                .AppendLine("sec comment".AsComment())
                .AppendLine("Person".AsSection())
                .AppendLine("prop comment")
                .AppendLine("name=jon")
                .AppendLine("pet=cat")
                .ToString().Trim();

            var section = Section
                .Builder()
                .Parse(sb)
                .RemoveProperty("name", "jon")
                .Build();

            Assert.Equal(1, section.PropertyCount);
            Assert.False(section.Property("pet").IsEmpty);
        }

        [Fact]
        public void SectionBuilderClone_ShouldCreateADeepCopyOfTheGivenSection()
        {
            var section = Section
                .Builder()
                .WithComment(Comment.Builder().AppendLine("original section").Build())
                .WithName("Person")
                .AppendProperty(Property
                    .Builder()
                    .WithKey("name")
                    .WithValue("joy")
                    .Build())
                .Build();

            section = Section
                .Builder(section)
                .AppendProperty(Property
                    .Builder()
                    .WithComment(Comment.Builder().AppendLine("person age").Build())
                    .WithKey("age")
                    .WithValue("17")
                    .Build())
                .Build();
            Assert.Equal(2, section.PropertyCount);
        }

        [Fact]
        public void IsEnabled_ShouldRepresentStringBasedOnIsEnabled()
        {
            var sec = Section
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
                .Build();

            var expected = new StringBuilder()
                .AppendLine("Person".AsSection())
                .AppendLine("name=joy")
                .AppendLine("age=17".AsDisabled())
                .ToString().Trim();

            Assert.True(sec.IsEnabled);
            Assert.Equal(expected, sec.ToString());

            var dSec = Section
                .Builder(sec)
                .IsEnable(false)
                .Build();

            var dExpected = new StringBuilder()
                .AppendLine("Person".AsSection().AsDisabled())
                .AppendLine("name=joy".AsDisabled())
                .AppendLine("age=17".AsDisabled())
                .ToString().Trim();

            Assert.False(dSec.IsEnabled);
            Assert.Equal(dExpected, dSec.ToString());
        }

        [Fact]
        public void ToString_ResultsShouldBeBasedOnFilter()
        {
            var section = Section
                .Builder()
                .WithComment(Comment.Builder().AppendLine("original section").Build())
                .WithName("Person")
                .AppendProperty(Property
                    .Builder()
                    .WithComment(Comment.Builder().AppendLine("person name").Build())
                    .WithKey("name")
                    .WithValue("joy")
                    .Build())
                .AppendProperty(Property
                    .Builder()
                    .IsEnable(false)
                    .WithComment(Comment.Builder().AppendLine("person age").Build())
                    .WithKey("age")
                    .WithValue("17")
                    .Build())
                .Build();

            var expected = new StringBuilder()
                .AppendLine("original section".AsComment())
                .AppendLine("Person".AsSection())
                .AppendLine("person name".AsComment())
                .AppendLine("name=joy")
                .AppendLine("person age".AsComment())
                .AppendLine("age=17".AsDisabled())
                .ToString().Trim();

            Assert.Equal(expected, section.ToString());
            Assert.Equal(expected, section.ToString(Filters.None));

            expected = new StringBuilder()
                .AppendLine("Person".AsSection())
                .AppendLine("name=joy")
                .AppendLine("age=17".AsDisabled())
                .ToString().Trim();

            Assert.Equal(expected, section.ToString(Filters.TrimComment));

            expected = new StringBuilder()
                .AppendLine("original section".AsComment())
                .AppendLine("Person".AsSection())
                .AppendLine("person name".AsComment())
                .AppendLine("name=joy")
                .ToString().Trim();

            Assert.Equal(expected, section.ToString(Filters.TrimDisabled));

            expected = new StringBuilder()
                .AppendLine("original section".AsComment())
                .AppendLine("Person".AsSection())
                .AppendLine("person name".AsComment())
                .AppendLine("name=joy")
                .ToString().Trim();

            Assert.Equal(expected, section.ToString(Filters.TrimDisabled));

            expected = new StringBuilder()
                .AppendLine("Person".AsSection())
                .AppendLine("name=joy")
                .ToString().Trim();

            Assert.Equal(expected, section.ToString(Filters.TrimCommentDisabled));

            section = Section
                .Builder(section)
                .IsEnable(false)
                .Build();
            Assert.Equal("", section.ToString(Filters.TrimDisabled));

            expected = new StringBuilder()
                .AppendLine("Person".AsSection().AsDisabled())
                .AppendLine("name=joy".AsDisabled())
                .AppendLine("age=17".AsDisabled())
                .ToString().Trim();
            Assert.Equal(expected, section.ToString(Filters.TrimComment));
        }

        [Fact]
        public void Properties_ResultsShouldBeBasedOnFilter()
        {
            var section = Section
                .Builder()
                .WithComment(Comment.Builder().AppendLine("original section").Build())
                .WithName("Person")
                .AppendProperty(Property
                    .Builder()
                    .WithComment(Comment.Builder().AppendLine("person name").Build())
                    .WithKey("name")
                    .WithValue("joy")
                    .Build())
                .AppendProperty(Property
                    .Builder()
                    .IsEnable(false)
                    .WithComment(Comment.Builder().AppendLine("person age").Build())
                    .WithKey("age")
                    .WithValue("17")
                    .Build())
                .AppendProperty(Property
                    .Builder()
                    .WithKey("sex")
                    .WithValue("male")
                    .Build())
                .Build();

            Assert.Equal(3, section.Properties().Count);
            Assert.Equal(3, section.Properties(Status.All).Count);
            Assert.Equal(3, section.Properties((Status)88).Count);
            Assert.Equal(2, section.Properties(Status.Enabled).Count);
            Assert.Equal(1, section.Properties(Status.Disabled).Count);
        }
    }
}