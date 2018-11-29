using Creationdek.Ini;
using Xunit;

namespace Creationdek.IniConfig.Net.Tests.Unit
{
    public class LibTests
    {
        [Theory]
        [InlineData(4)]
        [InlineData(2)]
        [InlineData(6)]
        [InlineData(198)]
        [InlineData(0)]
        public void IsEven_ShouldReturnTrue(int data)
        {
            Assert.True(data.IsEven());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(9)]
        [InlineData(-199)]
        [InlineData(-1)]
        public void IsOdd_ShouldReturnTrue(int data)
        {
            Assert.True(data.IsOdd());
        }

        [Theory]
        [InlineData(4)]
        [InlineData(2)]
        [InlineData(6)]
        [InlineData(198)]
        [InlineData(0)]
        public void IsPositiveNumber_ShouldReturnTrue(int data)
        {
            Assert.True(data.IsPositiveNumber());
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-3)]
        [InlineData(-9)]
        [InlineData(-199)]
        public void IsNegativeNumber_ShouldReturnTrue(int data)
        {
            Assert.True(data.IsNegativeNumber());
        }

        [Fact]
        public void ClearAffix_ShouldReturnACleanString()
        {
            var input = "text".AsComment();
            var actual = input.CleanAffix();
            var expected = "text";
            Assert.Equal(expected, actual);

            input = "text".AsDisabled();
            actual = input.CleanAffix();
            expected = "text";
            Assert.Equal(expected, actual);

            input = "text".AsSection();
            actual = input.CleanAffix();
            expected = "text";
            Assert.Equal(expected, actual);

            input = "text".AsHeader();
            actual = input.CleanAffix();
            expected = "text";
            Assert.Equal(expected, actual);

            input = "text".AsFooter();
            actual = input.CleanAffix();
            expected = "text";
            Assert.Equal(expected, actual);

            input = ";# #; text;;##  ; # ";
            actual = input.CleanAffix();
            expected = "text";
            Assert.Equal(expected, actual);

            input = null;
            actual = input.CleanAffix();
            Assert.Equal("", actual);

            input = "";
            actual = input.CleanAffix();
            Assert.Equal("", actual);
        }
    }
}