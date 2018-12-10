using System.Text;

namespace Creationdek.Ini
{
    /// <summary>
    /// Represents a <see cref="Property"/> of an ini configuration file.
    /// </summary>
    public sealed class Property
    {
        /// <summary>
        /// Represents an ini config Property Key.
        /// </summary>
        public string Key { get; }
        /// <summary>
        /// Represents an ini config Property Value.
        /// </summary>
        public string Value { get; }
        /// <summary>
        /// Represents an ini config Property Comment.
        /// </summary>
        public Comment Comment { get; }
        /// <summary>
        /// Allows the deactivation of a Property in an ini file. (it basically comments out the property)
        /// </summary>
        public bool IsEnabled { get; }
        /// <summary>
        /// Represents a non usable Property. (basically for use in place of initializing a property variable to null)
        /// </summary>
        public bool IsEmpty => Key.Equals(Lib.DefaultKeyName);

        internal Property(string key, string value, Comment comment, bool isEnabled)
        {
            Key = key;
            Value = value;
            Comment = comment;
            IsEnabled = isEnabled;
        }

        /// <summary>
        /// Instantiates the PropertyBuilder to start the process of building a Property.
        /// </summary>
        /// <param name="property">The source <see cref="Property"/> to modify (eg. clone or update).</param>
        /// <returns></returns>
        public static PropertyBuilder Builder(Property property = null)
        {
            return new PropertyBuilder(property);
        }

        public static PropertyBuilder Builder(string key, string value = "", bool isEnabled = true, params string[] commentLine)
        {
            return new PropertyBuilder(key, value, isEnabled, commentLine);
        }

        /// <summary>
        /// Returns a string representation of the property.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(Filters.None);
        }

        /// <summary>
        /// Returns a string representation of the property formatted according to the given filters.
        /// </summary>
        /// <param name="filters">Determines how you want to display the output.</param>
        /// <returns></returns>
        public string ToString(Filters filters)
        {
            if (IsEmpty || filters.HasFlag(Filters.TrimDisabled) && !IsEnabled)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            if (!(filters.HasFlag(Filters.TrimComment) || Comment.IsEmpty))
            {
                sb.AppendLine(Comment.ToString());
            }

            sb.AppendLine(IsEnabled ? $"{Key}={Value}" : $"{Key}={Value}".AsDisabled());

            return sb.ToString().Trim();
        }
    }
}