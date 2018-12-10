using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Creationdek.Ini
{
    /// <summary>
    /// Creates and manipulates <see cref="IniDocument"/> objects.
    /// </summary>
    public sealed class IniDocumentBuilder
    {
        private const int DefaultBufferSize = 4096; // StreamReader and FileStream default buffer size.
        private const FileOptions DefaultOptions = FileOptions.Asynchronous | FileOptions.SequentialScan; // async read file from beginning to end
        private Comment _header = Comment.Builder().AsType(CommentType.Header).Build();
        private Comment _footer = Comment.Builder().AsType(CommentType.Footer).Build();
        private readonly List<Section> _sections = new List<Section>();
        private bool _isEnabled = true;
        private string _filePath = string.Empty;
        private long _loadEndedAtIndex = -1;

        internal IniDocumentBuilder(IniDocument document)
        {
            if (document != null)
            {
                _header = Comment.Builder(document.Header).Build();
                _footer = Comment.Builder(document.Footer).Build();
                _isEnabled = document.IsEnabled;
                _filePath = document.FilePath;
                _loadEndedAtIndex = document.LoadEndedAtIndex;

                for (int i = 0; i < document.Sections().Count; i++)
                {
                    _sections.Add(Section.Builder(document.Sections()[i]).Build());
                }
            }
        }

        internal IniDocumentBuilder(string[] headerLines = null, string[] footerLines = null, bool isEnabled = true, params Section[] sections)
        {
            _header = Comment.Builder(headerLines).AsType(CommentType.Header).Build();
            _footer = Comment.Builder(footerLines).AsType(CommentType.Footer).Build();
            _isEnabled = isEnabled;

            for (int i = 0; i < sections.Length; i++)
            {
                AppendSection(sections[i]);
            }
        }

        /// <summary>
        /// Create the resulting <see cref="IniDocument"/>.
        /// </summary>
        public IniDocument Build()
        {
            return new IniDocument(_header, _footer, _isEnabled, _filePath, _loadEndedAtIndex, _sections);
        }

        /// <summary>
        /// Set the full path to the ini file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public IniDocumentBuilder SetFile(string filePath)
        {
            if (filePath.IsValidPath())
            {
                _filePath = filePath;
            }

            return this;
        }

        /// <summary>
        /// Set whether the <see cref="IniDocument"/> is enabled or disabled.
        /// </summary>
        /// <param name="enable">True enabled False disabled.</param>
        public IniDocumentBuilder IsEnabled(bool enable)
        {
            _isEnabled = enable;
            return this;
        }

        /// <summary>
        /// Set the header comment of the <see cref="IniDocument"/>.
        /// </summary>
        /// <param name="header">The header comment.</param>
        public IniDocumentBuilder WithHeader(Comment header)
        {
            _header = header != null
                ? Comment.Builder(header).AsType(CommentType.Header).Build()
                : Comment.Builder().AsType(CommentType.Header).Build();

            return this;
        }

        /// <summary>
        /// Set the footer comment of the <see cref="IniDocument"/>.
        /// </summary>
        /// <param name="footer">The footer comment.</param>
        public IniDocumentBuilder WithFooter(Comment footer)
        {
            _footer = footer != null
                ? Comment.Builder(footer).AsType(CommentType.Footer).Build()
                : Comment.Builder().AsType(CommentType.Footer).Build();

            return this;
        }

        /// <summary>
        /// Merge another <see cref="IniDocument"/> with the current one.
        /// </summary>
        /// <param name="other">The other <see cref="IniDocument"/>.</param>
        public IniDocumentBuilder Merge(IniDocument other)
        {
            if (other == null)
                return this;

            if (string.IsNullOrWhiteSpace(_filePath))
            {
                _filePath = other.FilePath;
            }

            if (_loadEndedAtIndex < 0)
            {
                _loadEndedAtIndex = other.LoadEndedAtIndex;
            }

            _header = Comment.Builder(_header).Merge(other.Header).Build();
            _footer = Comment.Builder(_footer).Merge(other.Footer).Build();

            for (var i = 0; i < other.Sections().Count; i++)
            {
                AppendSection(other.Sections()[i]);
            }

            return this;
        }

        /// <summary>
        /// Parse the given text into an <see cref="IniDocument"/>.
        /// </summary>
        /// <param name="text">The text to parse.</param>
        public IniDocumentBuilder Parse(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return this;
            }

            var comment = Comment.Builder().Build();
            var sName = string.Empty;
            var sIsEnabled = true;

            foreach (var line in text.SplitToLines().Select(l => l.Trim()).Where(l => l != ""))
            {
                if (line.IsHeader())
                {
                    _header = Comment.Builder(_header).AsType(CommentType.Header).AppendLine(line).Build();
                    continue;
                }

                if (line.IsFooter())
                {
                    _footer = Comment.Builder(_footer).AsType(CommentType.Footer).AppendLine(line).Build();
                    continue;
                }

                if (line.CommentIsValidAndUpdated(ref comment))
                {
                    continue;
                }

                if (line.SectionIsValidAndUpdated(ref sName, ref sIsEnabled))
                {
                    AppendSection(Section
                        .Builder()
                        .IsEnable(sIsEnabled)
                        .WithName(sName)
                        .WithComment(comment)
                        .Build());

                    comment = Comment.Builder().Build();
                    continue;
                }

                if (line.PropertyIsValidAndUpdated(out var key, out var value, out var isEnabled))
                {
                    AppendProperty(sName, Property
                        .Builder()
                        .WithKey(key)
                        .WithValue(value)
                        .WithComment(comment)
                        .IsEnable(isEnabled)
                        .Build());

                    comment = Comment.Builder().Build();
                }
            }

            return this;
        }

        /// <summary>
        /// Append the given <see cref="Section"/> to the <see cref="IniDocument"/>.
        /// </summary>
        /// <param name="section">The <see cref="Section"/> to append.</param>
        public IniDocumentBuilder AppendSection(Section section)
        {
            if (section != null)
            {
                var index = _sections.IndexOfSection(section.Name);
                if (index.IsPositiveNumber()) // section exists
                {
                    _sections[index] = Section
                        .Builder(_sections[index])
                        .Merge(section)
                        .Build();
                }
                else
                {
                    _sections.Add(section);
                }
            }

            return this;
        }

        /// <summary>
        /// Remove the <see cref="Section"/> with the given name from the <see cref="IniDocument"/>.
        /// </summary>
        /// <param name="name">The name to the section to remove.</param>
        public IniDocumentBuilder RemoveSection(string name)
        {
            var (exists, index) = _sections.SectionExists(name);
            if (exists)
            {
                _sections.RemoveAt(index);
            }
            return this;
        }

        /// <summary>
        /// Remove the <see cref="Section"/> at the specific index of the <see cref="IniDocument"/> sections collection.
        /// </summary>
        /// <param name="index">Remove the <see cref="Section"/> at this index.</param>
        public IniDocumentBuilder RemoveSectionAt(int index)
        {
            if (index.IsWithinRange(0, _sections.Count))
            {
                _sections.RemoveAt(index);
            }
            return this;
        }

        /// <summary>
        /// Append a <see cref="Property"/> to the <see cref="Section"/> with the given name.
        /// </summary>
        /// <param name="sectionName">The name of the <see cref="Section"/> to append the <see cref="Property"/> to.</param>
        /// <param name="property">The <see cref="Property"/> to append to the <see cref="Section"/>.</param>
        public IniDocumentBuilder AppendProperty(string sectionName, Property property)
        {
            if (string.IsNullOrWhiteSpace(sectionName) || property == null)
            {
                return this;
            }

            var index = _sections.IndexOfSection(sectionName);
            if (index.IsPositiveNumber())
            {
                _sections[index] = Section
                    .Builder(_sections[index])
                    .AppendProperty(property)
                    .Build();
            }
            else
            {
                _sections.Add(Section
                    .Builder()
                    .WithName(sectionName)
                    .AppendProperty(property)
                    .Build());
            }

            return this;
        }

        /// <summary>
        /// Remove the <see cref="Property"/> with the given key and value from the <see cref="Section"/> with the given name.
        /// </summary>
        /// <param name="sectionName">The name of the <see cref="Section"/> to remove the <see cref="Property"/> from.</param>
        /// <param name="key">The key of the <see cref="Property"/>.</param>
        /// <param name="value">The value of the <see cref="Property"/>.</param>
        public IniDocumentBuilder RemoveProperty(string sectionName, string key, string value)
        {
            var (exists, index) = _sections.SectionExists(sectionName);
            if (exists)
            {
                _sections[index] = Section
                    .Builder(_sections[index])
                    .RemoveProperty(key, value)
                    .Build();
            }

            return this;
        }

        /// <summary>
        /// Remove the property at the given index of the <see cref="Section"/> with the given name.
        /// </summary>
        /// <param name="sectionName">The name of the <see cref="Section"/> to remove the <see cref="Property"/> from.</param>
        /// <param name="index">The zero based index of the <see cref="Property"/> to remove.</param>
        public IniDocumentBuilder RemovePropertyAt(string sectionName, int index)
        {
            var (exists, i) = _sections.SectionExists(sectionName);
            if (exists)
            {
                _sections[i] = Section
                    .Builder(_sections[i])
                    .RemovePropertyAt(index)
                    .Build();
            }

            return this;
        }

        /// <summary>
        /// Load an ini configuration file.
        /// </summary>
        /// <param name="numberOfSections">The number of sections to load. 0 (default) being all.
        /// <para>Possible use case is if the file is very large and you don't wand to load the whole file into ram.</para></param>
        /// <param name="filePath">The path to the ini configuration file to load. Optional if you already used the <see cref="SetFile"/> function.</param>
        public async Task<IniDocumentBuilder> LoadIniAsync(int numberOfSections = 0, string filePath = "")
        {
            if (!(filePath.IsValidFile() || _filePath.IsValidFile()))
                return this;

            if (filePath.IsValidFile())
            {
                _filePath = filePath;
            }

            long currentIndex = -1;
            var comment = Comment.Builder().Build();
            var sName = string.Empty;
            var sIsEnabled = true;

            using (var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.None, DefaultBufferSize, DefaultOptions))
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    currentIndex++;

                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    if (GetHeader(line))
                        continue;

                    if (GetFooter(line))
                        continue;

                    if (GetComment(line, ref comment))
                        continue;

                    var sec = GetSection(numberOfSections, line, currentIndex, ref sName, ref sIsEnabled, ref comment);
                    if (sec.endOfLoad)
                        return this;
                    if (sec.continueLoop)
                        continue;

                    GetProperty(line, sName, ref comment);
                }
            }

            return this;
        }

        /// <summary>
        /// Load the nest given number of sections from the ini configuration file.
        /// <para> Used only when you have already used <see cref="LoadIniAsync"/> and want to load another set of sections from the same ini file.</para>
        /// </summary>
        /// <param name="numberOfSections">The number of sections to load. 0 (default) being all.</param>
        public async Task<IniDocumentBuilder> LoadNextAsync(int numberOfSections = 0)
        {
            if (!_filePath.IsValidFile() || _loadEndedAtIndex < 0)
                return this;

            _sections.Clear();

            long currentIndex = -1;
            var comment = Comment.Builder().Build();
            var sName = string.Empty;
            var sIsEnabled = true;

            using (var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.None, DefaultBufferSize, DefaultOptions))
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    currentIndex++;
                    if (currentIndex < _loadEndedAtIndex)
                        continue;

                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    if (GetHeader(line))
                        continue;

                    if (GetFooter(line))
                        continue;

                    if (GetComment(line, ref comment))
                        continue;

                    var sec = GetSection(numberOfSections, line, currentIndex, ref sName, ref sIsEnabled, ref comment);
                    if (sec.endOfLoad)
                        return this;
                    if (sec.continueLoop)
                        continue;

                    GetProperty(line, sName, ref comment);
                }
            }

            return this;
        }

        private void GetProperty(string line, string sName, ref Comment comment)
        {
            if (line.PropertyIsValidAndUpdated(out var key, out var value, out var isEnabled))
            {
                AppendProperty(sName, Property
                    .Builder()
                    .WithKey(key)
                    .WithValue(value)
                    .WithComment(comment)
                    .IsEnable(isEnabled)
                    .Build());

                comment = Comment.Builder().Build();
            }
        }

        private (bool endOfLoad, bool continueLoop, bool notSection) GetSection(
            int numberOfSections,
            string line,
            long currentIndex,
            ref string sName,
            ref bool sIsEnabled,
            ref Comment comment)
        {
            if (line.SectionIsValidAndUpdated(ref sName, ref sIsEnabled))
            {
                if (numberOfSections > 0 && _sections.Count == numberOfSections)
                {
                    _loadEndedAtIndex = currentIndex - comment.LineCount;
                    return (true, false, false);
                }

                AppendSection(Section
                    .Builder()
                    .IsEnable(sIsEnabled)
                    .WithName(sName)
                    .WithComment(comment)
                    .Build());

                comment = Comment.Builder().Build();
                return (false, true, false);
            }

            return (false, false, true);
            //else
            //{
            //    continue;
            //}

            //if (line.SectionIsValidAndUpdated(ref sName, ref sIsEnabled))
            //{
            //    if (numberOfSections > 0 && _sections.Count == numberOfSections)
            //    {
            //        _loadEndedAtIndex = currentIndex - comment.LineCount;
            //        return this;
            //    }

            //    AppendSection(Section
            //        .Builder()
            //        .IsEnable(sIsEnabled)
            //        .WithName(sName)
            //        .WithComment(comment)
            //        .Build());

            //    comment = Comment.Builder().Build();
            //    continue;
            //}

            //if (string.IsNullOrWhiteSpace(line))
            //{
            //    continue;
            //}

            //if (line.IsHeader())
            //{
            //    _header = Comment.Builder(_header).AsType(CommentType.Header).AppendLine(line).Build();
            //    continue;
            //}

            //if (line.IsFooter())
            //{
            //    _footer = Comment.Builder(_footer).AsType(CommentType.Footer).AppendLine(line).Build();
            //    continue;
            //}

            //if (line.CommentIsValidAndUpdated(ref comment))
            //{
            //    continue;
            //}

            //if (line.SectionIsValidAndUpdated(ref sName, ref sIsEnabled))
            //{
            //    if (numberOfSections > 0 && _sections.Count == numberOfSections)
            //    {
            //        _loadEndedAtIndex = currentIndex - comment.LineCount;
            //        return this;
            //    }

            //    AppendSection(Section
            //        .Builder()
            //        .IsEnable(sIsEnabled)
            //        .WithName(sName)
            //        .WithComment(comment)
            //        .Build());

            //    comment = Comment.Builder().Build();
            //    continue;
            //}

            //if (line.PropertyIsValidAndUpdated(out var key, out var value, out var isEnabled))
            //{
            //    AppendProperty(sName, Property
            //        .Builder()
            //        .WithKey(key)
            //        .WithValue(value)
            //        .WithComment(comment)
            //        .IsEnable(isEnabled)
            //        .Build());

            //    comment = Comment.Builder().Build();
            //}
        }

        private static bool GetComment(string line, ref Comment comment)
        {
            if (line.CommentIsValidAndUpdated(ref comment))
            {
                return true;
            }

            return false;
        }

        private bool GetFooter(string line)
        {
            if (line.IsFooter())
            {
                _footer = Comment.Builder(_footer).AsType(CommentType.Footer).AppendLine(line).Build();
                return true;
            }

            return false;
        }

        private bool GetHeader(string line)
        {
            if (line.IsHeader())
            {
                _header = Comment.Builder(_header).AsType(CommentType.Header).AppendLine(line).Build();
                return true;
            }

            return false;
        }
    }
}