using System;
using System.Collections.Generic;
using System.Linq;

namespace Creationdek.Ini
{
    /// <summary>
    /// Create, Clone or Edit a Section.
    /// </summary>
    public sealed class SectionBuilder
    {
        private const string NameErrorMessage = "Section name cannot be null or whitespace.";
        private string _name = Lib.DefaultKeyName;
        private Comment _comment = Comment.Builder().Build();
        private bool _isEnabled = true;
        private readonly List<Property> _properties = new List<Property>();

        /// <summary>
        /// Essentially creates a clone of the given section if its valid. Can be used to modify an existing Section.
        /// </summary>
        /// <param name="section">The section to modify.</param>
        internal SectionBuilder(Section section)
        {
            if (section != null)
            {
                _name = section.Name;
                _isEnabled = section.IsEnabled;
                _comment = Comment.Builder(section.Comment).Build();

                for (int i = 0; i < section.Properties().Count; i++)
                {
                    _properties.Add(Property.Builder(section.Properties()[i]).Build());
                }
            }
        }

        /// <summary>
        /// Creates the <see cref="Section"/> object.
        /// </summary>
        /// <returns></returns>
        public Section Build()
        {
            return new Section(_name, _comment, _isEnabled, _properties);
        }

        /// <summary>
        /// Replaces the current <see cref="Section"/>'s IsEnabled with the given value.
        /// </summary>
        /// <param name="enable">Set IsEnabled to this.</param>
        /// <returns>Returns the SectionBuilder</returns>
        public SectionBuilder IsEnable(bool enable)
        {
            _isEnabled = enable;
            return this;
        }

        /// <summary>
        /// Replaces the current <see cref="Section"/>'s Name with the given value.
        /// </summary>
        /// <param name="name">Set Name to this.</param>
        /// <returns>Does nothing if the given value is null or whitespace.</returns>
        public SectionBuilder WithName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(NameErrorMessage, nameof(name));

            _name = name;
            return this;
        }

        /// <summary>
        /// Replaces the current <see cref="Section"/>'s <see cref="Comment"/> with the given value.
        /// </summary>
        /// <param name="comment">Set <see cref="Comment"/> to this.</param>
        /// <returns>Does nothing if the given value is null or empty.</returns>
        public SectionBuilder WithComment(Comment comment)
        {
            _comment = comment ?? Comment.Builder().AsType(CommentType.Comment).Build();

            return this;
        }

        /// <summary>
        /// Merges all unique properties from the given <see cref="Section"/>.
        /// </summary>
        /// <param name="other">Merges the unique Properties from this.</param>
        /// <returns>Does nothing if the given value is null or empty.</returns>
        public SectionBuilder Merge(Section other)
        {
            if (other != null)
            {
                _comment = Comment.Builder(_comment).Merge(other.Comment).Build();
                for (int i = 0; i < other.Properties().Count; i++)
                {
                    if (!_properties.PropertyExists(
                        other.Properties()[i].Key,
                        other.Properties()[i].Value).exists)
                    {
                        _properties.Add(other.Properties()[i]);
                    }
                }
            }

            return this;
        }

        /// <summary>
        /// Creates <see cref="Section"/> from the given text if it's valid.
        /// </summary>
        /// <param name="text">The text to parse.</param>
        /// <returns></returns>
        public SectionBuilder Parse(string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                var foundSection = false;
                var comment = Comment.Builder().Build();

                using (var en = text.SplitToLines().Select(l => l.Trim()).Where(l => l != "").GetEnumerator())
                {
                    while (en.MoveNext())
                    {
                        if (en.Current.CommentIsValidAndUpdated(ref comment))
                        {
                            continue;
                        }

                        if (en.Current.SectionIsValidAndUpdated(ref _name, ref _isEnabled))
                        {
                            if (foundSection)
                            {
                                break;
                            }

                            foundSection = true;
                            _comment = comment;
                            comment = Comment.Builder().Build();
                        }

                        if (en.Current.PropertyIsValidAndUpdated(out var key, out var value, out var isEnabled))
                        {
                            _properties.Add(Property
                                .Builder()
                                .WithKey(key)
                                .WithValue(value)
                                .WithComment(comment)
                                .IsEnable(isEnabled)
                                .Build());

                            comment = Comment.Builder().Build();
                        }
                    }
                }
            }

            return this;
        }

        /// <summary>
        /// Adds the given Property if it is valid and unique to the section.
        /// </summary>
        /// <param name="property">The Property to add.</param>
        /// <returns>Adds the Property if it is valid and unique.</returns>
        public SectionBuilder AppendProperty(Property property)
        {
            if (property != null && !_properties.PropertyExists(property.Key, property.Value).exists)
            {
                _properties.Add(property);
            }

            return this;
        }

        /// <summary>
        /// Removes the <see cref="Property"/> that matches the given key and value if found.
        /// </summary>
        /// <param name="key">The key to match.</param>
        /// <param name="value">The value to match</param>
        /// <returns>Removes the <see cref="Property"/> from the <see cref="Section"/> if found.</returns>
        public SectionBuilder RemoveProperty(string key, string value)
        {
            return RemovePropertyAt(_properties.IndexOfProperty(key, value));
        }

        public SectionBuilder RemovePropertyAt(int index)
        {
            if (index.IsWithinRange(0, _properties.Count))
            {
                _properties.RemoveAt(index);
            }

            return this;
        }
    }
}