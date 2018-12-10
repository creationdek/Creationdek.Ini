using System;
using System.Linq;

namespace Creationdek.Ini
{
    /// <summary>
    /// Create, Clone or Edit a Comment.
    /// </summary>
    public sealed class PropertyBuilder
    {
        private const string ExceptionMessage = "A property's key cannot be null or whitespace";
        private bool _isEnabled = true;
        private string _key = Lib.DefaultKeyName;
        private string _value = "";
        private Comment _comment = Comment.Builder().Build();

        internal PropertyBuilder(Property property)
        {
            if (property != null)
            {
                _isEnabled = property.IsEnabled;
                _key = property.Key;
                _value = property.Value;
                _comment = Comment.Builder(property.Comment).Build();
            }
        }

        internal PropertyBuilder(string key, string value = "", bool isEnabled = true, params string[] commentLine)
        {
            WithKey(key);
            WithValue(value);
            IsEnable(isEnabled);
            _comment = Comment.Builder(commentLine).Build();
        }

        /// <summary>
        /// Creates the Property object.
        /// </summary>
        /// <returns>Returns a new Property with the given data.</returns>
        public Property Build()
        {
            return new Property(_key, _value, _comment, _isEnabled);
        }

        /// <summary>
        /// Enable or Disable a Property.
        /// </summary>
        /// <param name="enable">Toggle true or false.</param>
        /// <returns>Returns the PropertyBuilder instance.</returns>
        public PropertyBuilder IsEnable(bool enable)
        {
            _isEnabled = enable;
            return this;
        }

        /// <summary>
        /// Sets the Key of the Property.
        /// </summary>
        /// <param name="key">The key to set.</param>
        /// <returns>Returns the PropertyBuilder instance.</returns>
        public PropertyBuilder WithKey(string key)
        {
            _key = string.IsNullOrWhiteSpace(key)
                ? throw new ArgumentException(ExceptionMessage, nameof(key))
                : key;
            return this;
        }

        /// <summary>
        /// Sets the Value of the Property.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <returns>Returns the PropertyBuilder instance.</returns>
        public PropertyBuilder WithValue(string value)
        {
            _value = value ?? "";
            return this;
        }

        /// <summary>
        /// Sets a Comment for the Property.
        /// </summary>
        /// <param name="comment">The Comment you want for the Property.</param>
        /// <returns>If the given Comment is empty or null, nothing happens.</returns>
        public PropertyBuilder WithComment(Comment comment)
        {
            _comment = comment ?? Comment.Builder().AsType(CommentType.Comment).Build();

            return this;
        }

        /// <summary>
        /// Creates a Property object out of a valid string.
        /// </summary>
        /// <param name="text">The text to Parse into an object.</param>
        /// <returns>Returns the PropertyBuilder instance.</returns>
        public PropertyBuilder Parse(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return this;

            var lines = text.SplitToLines().Select(l => l.Trim()).Where(l => l != "").ToList();
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].CommentIsValidAndUpdated(ref _comment))
                {
                    continue;
                }

                if (lines[i].PropertyIsValidAndUpdated(out _key, out _value, out _isEnabled))
                {
                    break;
                }
            }
            return this;
        }
    }
}