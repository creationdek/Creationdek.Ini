using System.Text;
using Creationdek.Ini;
using Xunit;

namespace Creationdek.IniConfig.Net.Tests.Unit
{
    public class PropertyTests
    {
        [Fact]
        public void BuilderBuilder_ShouldCreateAnEmptyProperty()
        {
            var property = Property
                .Builder()
                .Build();

            Assert.True(property.IsEmpty);
            Assert.Equal("", property.ToString());
        }

        [Fact]
        public void IsEnable_ShouldEnableProperty()
        {
            var property = Property
                .Builder()
                .IsEnable(false)
                .Build();

            Assert.False(property.IsEnabled);

            property = Property
                .Builder(property)
                .IsEnable(true)
                .Build();

            Assert.True(property.IsEnabled);
        }

        [Fact]
        public void WithKey_ShouldUpdatePropertyKey()
        {
            var property = Property
                .Builder()
                .WithKey("foo")
                .Build();
            Assert.Equal("foo", property.Key);
        }

        [Fact]
        public void WithValue_ShouldUpdatePropertyValue()
        {
            var property = Property
                .Builder()
                .WithValue("foo")
                .Build();
            Assert.Equal("foo", property.Value);
        }

        [Fact]
        public void WithComment_ShouldReplaceTheCommentWithTheGivenOne()
        {
            var property = Property
                .Builder()
                .WithKey("foo")
                .WithComment(Comment.Builder().AppendLine("line1").Build())
                .Build();
            Assert.Equal("line1".AsComment(), property.Comment.ToString());

            property = Property
                .Builder(property)
                .WithComment(Comment.Builder().AppendLine("updated line1").AppendLine("line2").Build())
                .Build();

            var expected = new StringBuilder()
                .AppendLine("updated line1".AsComment())
                .AppendLine("line2".AsComment())
                .ToString().Trim();

            Assert.Equal(expected, property.Comment.ToString());

            var emptyPropComment = Property
                .Builder(property)
                .WithComment(null)
                .Build();

            Assert.Equal("", emptyPropComment.Comment.ToString());
            Assert.True(emptyPropComment.Comment.IsEmpty);

            var emptyPropComment2 = Property
                .Builder(property)
                .WithComment(Comment.Builder().Build())
                .Build();

            Assert.Equal("", emptyPropComment2.Comment.ToString());
            Assert.True(emptyPropComment2.Comment.IsEmpty);
        }

        [Fact]
        public void Parse_ShouldReturnAPropertyIfTheTextIsValid()
        {
            var sb = new StringBuilder()
                .AppendLine("property comment")
                .AppendLine("foo=bar")
                .AppendLine("fizz=buzz")
                .ToString().Trim();

            var property = Property
                .Builder()
                .Parse(sb)
                .Build();

            var expected = new StringBuilder()
                .AppendLine("property comment".AsComment())
                .AppendLine("foo=bar")
                .ToString().Trim();

            Assert.Equal("foo", property.Key);
            Assert.Equal(expected, property.ToString());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Parse_GivenInvalidData_ShouldJustReturn(string data)
        {
            var property = Property
                .Builder()
                .Parse(data)
                .Build();

            Assert.True(property.IsEmpty);
        }

        [Fact]
        public void ToString_ShouldReturnResultAccordingToTheFiltersUsed()
        {
            var property = Property
                .Builder()
                .WithKey("foo")
                .WithValue("bar")
                .WithComment(Comment
                    .Builder()
                    .AppendLine("prop comment")
                    .Build())
                .IsEnable(false)
                .Build();

            var expected = new StringBuilder()
                .AppendLine("prop comment".AsComment())
                .AppendLine("foo=bar".AsDisabled())
                .ToString().Trim();

            Assert.Equal(expected, property.ToString());
            Assert.Equal(expected, property.ToString(Filters.None));
            Assert.Equal("foo=bar".AsDisabled(), property.ToString(Filters.TrimComment));
            Assert.Equal("", property.ToString(Filters.TrimDisabled));
        }
    }
}