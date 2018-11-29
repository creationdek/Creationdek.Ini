using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Creationdek.Ini
{
    /// <summary>
    /// Represents an ini configuration file.
    /// </summary>
    public sealed class IniDocument
    {
        private const string InvalidFilePathMessage = "Please enter a valid file path.";
        private const int DefaultBufferSize = 4096; // StreamReader and FileStream default buffer size.
        private const FileOptions DefaultOptions = FileOptions.Asynchronous | FileOptions.SequentialScan; // async read file from beginning to end
        private readonly string _newLineX2 = Environment.NewLine + Environment.NewLine;
        private readonly List<Section> _sections;

        /// <summary>
        /// If <see cref="IniDocument"/> section count is 0 or all sections are empty then the <see cref="IniDocument"/> is empty.
        /// </summary>
        public bool IsEmpty => _sections.Count < 1 || _sections.TrueForAll(s => s.IsEmpty);

        /// <summary>
        /// Returns the number of sections in the <see cref="IniDocument"/>.
        /// </summary>
        public int SectionCount => _sections.Count;

        /// <summary>
        /// The topmost special header text of an ini configuration file.
        /// </summary>
        public Comment Header { get; }

        /// <summary>
        /// The bottommost special footer text of an ini configuration file.
        /// </summary>
        public Comment Footer { get; }

        /// <summary>
        /// Specially comments out all sections and properties in the ini configuration file. Used for learning and manual editing of the ini configuration file.
        /// </summary>
        public bool IsEnabled { get; }

        /// <summary>
        /// The path to the ini configuration file to write to or read from.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// The index stopped at when partially loaded a file. Used when partially loading large files and you don't want the whole file in memory.
        /// </summary>
        internal long LoadEndedAtIndex { get; }

        internal IniDocument(Comment header, Comment footer, bool isEnabled, string filePath, long loadEndedAtIndex,
            List<Section> sections)
        {
            Header = header;
            Footer = footer;
            IsEnabled = isEnabled;
            FilePath = filePath;
            LoadEndedAtIndex = loadEndedAtIndex;
            _sections = sections;
        }

        /// <summary>
        /// Returns a collection of <see cref="Section"/> based on the given <see cref="Status"/>.
        /// </summary>
        /// <param name="status">The <see cref="Status"/> the <see cref="Section"/> should have.</param>
        /// <returns>Returns sections with the given <see cref="Status"/> otherwise returns all sections.</returns>
        public IReadOnlyList<Section> Sections(Status status = Status.All)
        {
            switch (status)
            {
                case Status.Enabled:
                    return _sections.FindAll(s => s.IsEnabled);
                case Status.Disabled:
                    return _sections.FindAll(s => !s.IsEnabled);
                case Status.All:
                    return _sections;
            }

            return _sections;
        }

        /// <summary>
        /// Use to create an <see cref="IniDocument"/>
        /// <para> Pass in an existing <see cref="IniDocument"/> for manipulation or to create a deep clone.</para>
        /// </summary>
        /// <param name="document">The <see cref="IniDocument"/> for cloning or manipulation.</param>
        /// <returns>Returns a new <see cref="IniDocumentBuilder"/>.</returns>
        public static IniDocumentBuilder Builder(IniDocument document = null)
        {
            return new IniDocumentBuilder(document);
        }

        public override string ToString()
        {
            return ToString(Filters.None);
        }

        /// <summary>
        /// Returns a string representation of the <see cref="IniDocument"/> formatted according to the given filters.
        /// </summary>
        /// <param name="filters">Shape the output of ths <see cref="IniDocument"/></param>
        public string ToString(Filters filters)
        {
            if (IsEmpty || filters.HasFlag(Filters.TrimDisabled) && !IsEnabled)
                return string.Empty;

            var sb = new StringBuilder();

            var header = GetHeader();
            if (!string.IsNullOrWhiteSpace(header))
                sb.AppendLine(header);

            GetSections();

            var footer = GetFooter();
            if (!string.IsNullOrWhiteSpace(footer))
                sb.AppendLine(footer);

            return sb.ToString().Trim();

            string GetHeader() => filters.HasFlag(Filters.TrimHeader) || Header.IsEmpty
                ? string.Empty
                : Header + (filters.HasFlag(Filters.Formatted) ? _newLineX2 : "");

            string GetFooter() => filters.HasFlag(Filters.TrimFooter) || Footer.IsEmpty
                ? string.Empty
                : (filters.HasFlag(Filters.Formatted) ? _newLineX2 : "") + Footer;

            void GetSections()
            {
                var s = new StringBuilder();
                for (var i = 0; i < _sections.Count; i++)
                {
                    if (filters.HasFlag(Filters.Formatted) && s.ToString() != "")
                        s.AppendLine();

                    if (!IsEnabled)
                    {
                        var tmp = Section
                            .Builder(_sections[i])
                            .IsEnable(IsEnabled)
                            .Build()
                            .ToString(filters);

                        if (!string.IsNullOrWhiteSpace(tmp))
                            s.AppendLine(tmp);
                    }
                    else
                    {
                        var sec = _sections[i].ToString(filters);
                        if (!string.IsNullOrWhiteSpace(sec))
                            s.AppendLine(sec);
                    }
                }

                var result = s.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(result))
                    sb.AppendLine(result);
            }
        }

        /// <summary>
        /// Writes the <see cref="IniDocument"/> object to disk.
        /// </summary>
        /// <param name="filePath">Path to write to. Optional if the LoadAsync method was used in <see cref="IniDocumentBuilder"/>.</param>
        /// <param name="filters">Shape the output that will be written.</param>
        public async Task WriteAsync(string filePath = "", Filters filters = Filters.None)
        {
            var path = filePath.IsValidPath()
                ? filePath
                : FilePath.IsValidPath()
                    ? FilePath
                    : "";

            if (path == "")
                throw new FileNotFoundException(InvalidFilePathMessage);

            using (var writer = File.CreateText(path))
                await writer.WriteAsync(ToString(filters));
        }

        /// <summary>
        /// Writes the <see cref="IniDocument"/> object to disk.
        /// </summary>
        /// <param name="file">The full path to the file to write to.</param>
        /// <param name="sectionName">The section to create/update.</param>
        /// <param name="key">The property key to create/update</param>
        /// <param name="value">The property value to set.</param>
        /// <param name="updateProperty">If a property with the given key exists, should its value be updated or should a new property with the same key be created.</param>
        public static async Task WriteAsync(string file, string sectionName, string key, string value, bool updateProperty = false)
        {
            if (!file.IsValidPath())
            {
                throw new ArgumentException(InvalidFilePathMessage);
            }

            if (!file.IsValidFile())
            {
                await Builder()
                    .SetFile(file)
                    .AppendSection(Section
                        .Builder()
                        .WithName(sectionName)
                        .AppendProperty(Property
                            .Builder()
                            .WithKey(key)
                            .WithValue(value)
                            .Build())
                        .Build())
                    .Build()
                    .WriteAsync();
                return;
            }

            var tmpFile = file + ".tmp";
            var cache = new List<string>(DefaultBufferSize);
            var footer = new StringBuilder();

            var isSectionFound = false;

            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.None, DefaultBufferSize, DefaultOptions))
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (cache.Capacity >= DefaultBufferSize)
                    {
                        await BackupAndResetCacheAsync();
                    }

                    if (line.IsHeader())
                    {
                        cache.Add(line);
                        continue;
                    }

                    if (line.IsFooter())
                    {
                        footer.AppendLine(line);
                        continue;
                    }

                    if (line.IsComment().isTrue)
                    {
                        cache.Add(line);
                        continue;
                    }

                    var sec = line.IsSection();
                    if (sec.isTrue && sec.sectionName.Equals(sectionName))
                    {
                        isSectionFound = true;
                        cache.Add(line);
                        continue;
                    }

                    var prop = line.IsProperty();
                    if (isSectionFound && prop.isTrue && prop.key.Equals(key))
                    {
                        if (updateProperty)
                        {
                            cache.Add($"{key}={value}");
                        }
                        else
                        {
                            cache.Add(line);
                            cache.Add($"{key}={value}");
                        }
                        continue;
                    }

                    cache.Add(line);
                }
            }

            if (!isSectionFound)
            {
                cache.Add(sectionName.AsSection());
                cache.Add($"{key}={value}");
            }

            if (footer.Length > 0)
            {
                cache.Add(footer.ToString().Trim());
            }

            if (cache.Count > 0)
            {
                await BackupAndResetCacheAsync();
            }

            File.Delete(file);
            File.Move(tmpFile, file);

            async Task BackupAndResetCacheAsync()
            {
                using (var fs = new FileStream(tmpFile, FileMode.Append, FileAccess.Write, FileShare.None, DefaultBufferSize, DefaultOptions))
                using (var sw = new StreamWriter(fs))
                {
                    for (int i = 0; i < cache.Count; i++)
                    {
                        await sw.WriteLineAsync(cache[i]);
                    }
                }
                cache.Clear();
                cache.Capacity = DefaultBufferSize;
            }
        }

        /// <summary>
        /// Read the value of a <see cref="Property"/>.
        /// </summary>
        /// <param name="file">The full path of the file to read.</param>
        /// <param name="sectionName">The name of the <see cref="Section"/> to lookup.</param>
        /// <param name="key">The <see cref="Property"/> key to lookup.</param>
        /// <returns>The value or the given <see cref="Property"/> key.</returns>
        public static async Task<string> ReadAsync(string file, string sectionName, string key)
        {
            if (!file.IsValidFile())
                return string.Empty;

            var isMatch = false;
            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, DefaultOptions))
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    var (_, _, sName) = line.IsSection();
                    if (sName == sectionName)
                    {
                        isMatch = true;
                        continue;
                    }

                    var (_, _, pKey, value) = line.IsProperty();
                    if (isMatch && pKey.Equals(key))
                        return value;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Read a <see cref="Section"/> from an ini file.
        /// </summary>
        /// <param name="file">The full path of the file to read from.</param>
        /// <param name="name">The name of the <see cref="Section"/> to read.</param>
        /// <returns>Returns the <see cref="Section"/> if its found.</returns>
        public static async Task<Section> ReadSectionAsync(string file, string name)
        {
            if (!file.IsValidFile())
                return null;

            var section = Section.Builder().Build();
            var comment = Comment.Builder().Build();

            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, DefaultOptions))
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (line.IsComment().isTrue)
                    {
                        comment = Comment.Builder().AppendLine(line).Build();
                        continue;
                    }

                    var sec = line.IsSection();
                    if (sec.isTrue)
                    {
                        if (section.Name.Equals(name))
                            return section;

                        if (sec.sectionName.Equals(name))
                        {
                            section = Section
                                .Builder()
                                .WithName(sec.sectionName)
                                .WithComment(comment)
                                .IsEnable(sec.isEnable)
                                .Build();
                        }

                        comment = Comment.Builder().Build();
                        continue;
                    }

                    var (isTrue, isEnabled, key, value) = line.IsProperty();
                    if (isTrue && section.Name.Equals(name))
                    {
                        section = Section
                            .Builder(section)
                            .AppendProperty(Property
                                .Builder()
                                .WithKey(key)
                                .WithValue(value)
                                .WithComment(comment)
                                .IsEnable(isEnabled)
                                .Build())
                            .Build();
                    }
                    comment = Comment.Builder().Build();
                }
            }

            return section.IsEmpty ? null : section;
        }
    }
}