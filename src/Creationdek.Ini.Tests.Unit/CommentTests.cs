using System.Text;
using Creationdek.Ini;
using Xunit;

namespace Creationdek.IniConfig.Net.Tests.Unit
{
    public class CommentTests
    {
        [Fact]
        public void ShouldCreateEmptyComment()
        {
            var comment = Comment
                .Builder()
                .Build();

            Assert.True(comment.IsEmpty);
            Assert.Equal("", comment.ToString());
        }

        [Fact]
        public void ShouldChangeTheTypeOfComment()
        {
            var comment = Comment
                .Builder()
                .AppendLine("comment")
                .Build();
            Assert.Equal(CommentType.Comment, comment.Type);
            Assert.Equal("comment".AsComment(), comment.ToString());

            comment = Comment
                .Builder(comment)
                .AsType(CommentType.Comment)
                .Build();
            Assert.Equal(CommentType.Comment, comment.Type);
            Assert.Equal("comment".AsComment(), comment.ToString());

            comment = Comment
                .Builder(comment)
                .AsType(CommentType.Header)
                .Build();
            Assert.Equal(CommentType.Header, comment.Type);
            Assert.Equal("comment".AsHeader(), comment.ToString());

            comment = Comment
                .Builder(comment)
                .AsType(CommentType.Footer)
                .Build();
            Assert.Equal(CommentType.Footer, comment.Type);
            Assert.Equal("comment".AsFooter(), comment.ToString());
        }

        [Fact]
        public void ShouldReturn1LineComment()
        {
            var comment = Comment
                .Builder()
                .AppendLine("Comment line 0")
                .Build();

            var expected = new StringBuilder()
                .AppendLine("Comment line 0".AsComment())
                .ToString().Trim();

            Assert.Equal(1, comment.LineCount);
            Assert.Equal(expected, comment.ToString());
        }

        [Fact]
        public void ShouldRemoveLineAtGivenIndex()
        {
            var comment = Comment
                .Builder()
                .AppendLine("Comment line 0")
                .AppendLine("Comment line 1")
                .AppendLine("Comment line 2")
                .Build();
            var expected = new StringBuilder()
                .AppendLine("Comment line 0".AsComment())
                .AppendLine("Comment line 1".AsComment())
                .AppendLine("Comment line 2".AsComment())
                .ToString().Trim();
            Assert.Equal(3, comment.LineCount);
            Assert.Equal(expected, comment.ToString());

            comment = Comment
                .Builder(comment)
                .RemoveLineAt(0)
                .Build();
            expected = new StringBuilder()
                .AppendLine("Comment line 1".AsComment())
                .AppendLine("Comment line 2".AsComment())
                .ToString().Trim();
            Assert.Equal(2, comment.LineCount);
            Assert.Equal(expected, comment.ToString());
        }

        [Fact]
        public void ShouldMergeTheLinesFromTheGivenCommentToExistingOne()
        {
            var comment = Comment
                .Builder()
                .AppendLine("Comment line 0")
                .AppendLine("Comment line 1")
                .Build();

            var comment2 = Comment
                .Builder()
                .AppendLine("Comment line 8")
                .AppendLine("Comment line 9")
                .Build();

            var merge = Comment
                .Builder(comment)
                .Merge(comment2)
                .Build();

            var expected = new StringBuilder()
                .AppendLine("Comment line 0".AsComment())
                .AppendLine("Comment line 1".AsComment())
                .AppendLine("Comment line 8".AsComment())
                .AppendLine("Comment line 9".AsComment())
                .ToString().Trim();

            Assert.Equal(4, merge.LineCount);
            Assert.Equal(expected, merge.ToString());
        }

        [Fact]
        public void ShouldReturnCommentWithAllValidLinesFromTheGivenText()
        {
            var text = new StringBuilder()
                .AppendLine("Comment line 0")
                .AppendLine()
                .AppendLine("#;#Comment line 1")
                .AppendLine(" Comment line 2;;")
                .ToString().Trim();

            var comment = Comment
                .Builder()
                .Parse(text)
                .Build();

            var expected = new StringBuilder()
                .AppendLine("Comment line 0".AsComment())
                .AppendLine("Comment line 1".AsComment())
                .AppendLine("Comment line 2".AsComment())
                .ToString().Trim();

            Assert.Equal(3, comment.LineCount);
            Assert.Equal(expected, comment.ToString());
        }
    }
}