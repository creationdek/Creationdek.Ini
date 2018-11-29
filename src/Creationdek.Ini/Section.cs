using System.Collections.Generic;
using System.Text;

namespace Creationdek.Ini
{
    /// <summary>
    /// Represents a <see cref="Section"/> or an ini configuration file.
    /// </summary>
    public sealed class Section
    {
        private readonly List<Property> _properties;
        /// <summary>
        /// Represents a default non usable property. To be use instead of initializing a property to null.
        /// </summary>
        public bool IsEmpty => Name.Equals(Lib.DefaultKeyName) || PropertyCount < 1 || _properties.TrueForAll(p => p.IsEmpty);
        /// <summary>
        /// Returns the number of properties in the section.
        /// </summary>
        public int PropertyCount => _properties.Count;
        /// <summary>
        /// Represents the <see cref="Section"/>'s name.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Represents the <see cref="Section"/>'s comment.
        /// </summary>
        public Comment Comment { get; }
        /// <summary>
        /// Represents a disabled <see cref="Section"/>. To be used for manually enabling of sections or properties in the config file by the user.
        /// </summary>
        public bool IsEnabled { get; }

        internal Section(string name, Comment comment, bool isEnabled, List<Property> properties)
        {
            Name = name;
            Comment = comment;
            IsEnabled = isEnabled;
            _properties = properties;
        }

        /// <summary>
        /// Instantiate the <see cref="Section"/> Builder to create the object. 
        /// </summary>
        /// <param name="section">The source <see cref="Section"/> to modify (eg. clone or update).</param>
        /// <returns></returns>
        public static SectionBuilder Builder(Section section = null)
        {
            return new SectionBuilder(section);
        }

        /// <summary>
        /// Returns a collection of properties based on the given status filter (All, Enabled or Disabled properties).
        /// </summary>
        /// <param name="status">Return properties by status.</param>
        /// <returns></returns>
        public IReadOnlyList<Property> Properties(Status status = Status.All)
        {
            switch (status)
            {
                case Status.Enabled:
                    return _properties.FindAll(p => p.IsEnabled);
                case Status.Disabled:
                    return _properties.FindAll(p => !p.IsEnabled);
                case Status.All:
                    return _properties;
            }
            return _properties;
        }

        public Property Property(string key, string value = null)
        {
            var i = _properties.IndexOfProperty(key, value);
            return i.IsPositiveNumber() ? _properties[i] : Ini.Property.Builder().Build();
        }

        public Property PropertyAt(int index)
        {
            return index.IsWithinRange(0, _properties.Count)
                ? _properties[index]
                : Ini.Property.Builder().Build();
        }

        public bool ContainsProperty(string key, string value = null)
        {
            return _properties.PropertyExists(key, value).exists;
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
            if (IsEmpty)
            {
                return string.Empty;
            }

            if (filters.HasFlag(Filters.TrimDisabled) && !IsEnabled)
                return string.Empty;

            var sb = new StringBuilder();
            if (!(filters.HasFlag(Filters.TrimComment) || Comment.IsEmpty))
                sb.AppendLine(Comment.ToString());

            sb.AppendLine(IsEnabled
                ? $"[{Name}]"
                : $"[{Name}]".AsDisabled());

            for (int i = 0; i < _properties.Count; i++)
            {

                if (!IsEnabled)
                {
                    var s = Ini.Property
                        .Builder(_properties[i])
                        .IsEnable(IsEnabled)
                        .Build()
                        .ToString(filters);

                    if (!string.IsNullOrWhiteSpace(s))
                        sb.AppendLine(s);
                }
                else
                {
                    var s = _properties[i].ToString(filters);
                    if (!string.IsNullOrWhiteSpace(s))
                        sb.AppendLine(s);
                }
            }

            return sb.ToString().Trim();
        }
    }
}