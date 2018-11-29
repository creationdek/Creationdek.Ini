using System.Collections.Generic;
using System.Linq;

namespace Creationdek.Ini
{
    /// <summary>
    /// Use to create, clone or edit a Comment.
    /// </summary>
    public sealed class CommentBuilder
    {
        private readonly List<string> _lines = new List<string>();
        private CommentType _type;

        internal CommentBuilder(Comment comment)
        {
            if (comment != null)
            {
                _lines = new List<string>(comment.Lines);
                _type = comment.Type;
            }
        }

        /// <summary>
        /// Creates the Comment.
        /// </summary>
        /// <returns>Returns a Comment with all the modifications made.</returns>
        public Comment Build()
        {
            return new Comment(_type, _lines);
        }

        /// <summary>
        /// Determines whether this will be a normal or specialized Header or Footer ini comment.
        /// <para>- Header and Footer types are only to be used in Ini, not in Section or Property.</para>
        /// </summary>
        /// <param name="type">Choose between Comment or specialized (Header or Footer) comment representations.</param>
        /// <returns>The resulting Comment is updated to represent the Type.</returns>
        public CommentBuilder AsType(CommentType type)
        {
            _type = type;
            return this;
        }

        /// <summary>
        /// Appends a line to the bottom of the Comment.
        /// </summary>
        /// <param name="line">The line to append to the Comment.</param>
        /// <returns>If the line is valid it is added to the list, otherwise the function just returns.</returns>
        public CommentBuilder AppendLine(string line)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                var clean = line.CleanAffix();
                if (!(string.IsNullOrWhiteSpace(clean) || _lines.Contains(clean)))
                    _lines.Add(clean);
            }

            return this;
        }

        /// <summary>
        /// Removes the line at the given index from the Comment.
        /// </summary>
        /// <param name="index">The line to remove from the Comment.</param>
        /// <returns>If index is valid the line is removed, otherwise the function just returns.</returns>
        public CommentBuilder RemoveLineAt(int index)
        {
            if (index.IsWithinRange(0, _lines.Count))
            {
                _lines.RemoveAt(index);
            }
            return this;
        }

        /// <summary>
        /// Merges the unique lines from the given Comment to the existing one.
        /// </summary>
        /// <param name="other">The comment to merge to the existing one.</param>
        /// <returns>If the other comment is valid, the unique comments are merged, otherwise the function just returns.</returns>
        public CommentBuilder Merge(Comment other)
        {
            if (other != null && !other.IsEmpty)
            {
                for (int i = 0; i < other.LineCount; i++)
                {
                    if (!_lines.Contains(other.Lines[i]))
                    {
                        _lines.Add(other.Lines[i]);
                    }
                }
            }
            return this;
        }

        /// <summary>
        /// Creates an object out of the given text.
        /// </summary>
        /// <param name="text">The text to parse.</param>
        /// <returns>If the text is valid a new object is created, otherwise the function just returns.</returns>
        public CommentBuilder Parse(string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                var lines = text.SplitToLines().ToArray();
                for (int i = 0; i < lines.Length; i++)
                {
                    AppendLine(lines[i]);
                }
            }
            return this;
        }
    }
}