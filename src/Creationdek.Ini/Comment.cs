using System.Collections.Generic;
using System.Text;

namespace Creationdek.Ini
{
    /// <summary>
    /// Represents a <see cref="Comment"/> of an Ini configuration file.
    /// </summary>
    public sealed class Comment
    {
        public bool IsEmpty => Lines.Count < 1;
        public int LineCount => Lines.Count;
        public CommentType Type { get; }
        public readonly IReadOnlyList<string> Lines;

        internal Comment(CommentType type, IReadOnlyList<string> lines)
        {
            Type = type;
            Lines = lines;
        }

        /// <summary>
        /// Comment Builder used to create new, clone and or modify a <see cref="Comment"/>;
        /// </summary>
        /// <param name="comment">The source <see cref="Comment"/> to clone or modify.</param>
        public static CommentBuilder Builder(Comment comment = null)
        {
            return new CommentBuilder(comment);
        }

        /// <summary>
        /// Creates a new <see cref="CommentBuilder"/> with the given lines.;
        /// </summary>
        /// <param name="line">The lines for the resulting <see cref="Comment"/> given in order.</param>
        public static CommentBuilder Builder(params string[] line)
        {
            return new CommentBuilder(line);
        }

        /// <summary>
        /// Returns the string as a formatted ini <see cref="Comment"/>.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (IsEmpty)
                return string.Empty;

            var sb = new StringBuilder();
            for (var i = 0; i < LineCount; i++)
            {
                switch (Type)
                {
                    case CommentType.Comment:
                        sb.AppendLine(Lines[i].AsComment());
                        break;
                    case CommentType.Header:
                        sb.AppendLine(Lines[i].AsHeader());
                        break;
                    case CommentType.Footer:
                        sb.AppendLine(Lines[i].AsFooter());
                        break;
                }
            }

            return sb.ToString().Trim();
        }
    }
}