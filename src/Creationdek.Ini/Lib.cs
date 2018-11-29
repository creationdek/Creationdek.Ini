using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Creationdek.Ini
{
    /// <summary>
    /// Holds Functions Shared by the whole Library.
    /// </summary>
    public static class Lib
    {
        public const string PreNoneAffix = "";
        public const string PreHeaderAffix = "###--";
        public const string PreFooterAffix = "##--";
        public const string PreDisableAffix = "#--";
        public const string PreCommentAffix = ";";
        public const string PreSectionAffix = "[";

        public const string PostNoneAffix = "";
        public const string PostHeaderAffix = "--###";
        public const string PostFooterAffix = "--##";
        public const string PostDisableAffix = "--#";
        public const string PostCommentAffix = "";
        public const string PostSectionAffix = "]";

        public const string DefaultKeyName = ";_;";


        public static IEnumerable<string> SplitToLines(this string text)
        {
            return Regex.Split(text, "\r\n|\r|\n");
        }

        public static bool IsCommentOld(this string text)
        {
            return !text.IsDisabled() && !text.IsFooter() && !text.IsHeader() && !text.StartsWith("[") &&
                   !text.EndsWith("]") && !text.Contains("=") && (text.StartsWith(";") || text.StartsWith("#"));
        }

        public static bool IsDisabled(this string text)
        {
            return text.StartsWith(PreDisableAffix) && text.EndsWith(PostDisableAffix);
        }

        public static bool IsFooter(this string text)
        {
            return text.StartsWith(PreFooterAffix) && text.EndsWith(PostFooterAffix);
        }

        public static bool IsHeader(this string text)
        {
            return text.StartsWith(PreHeaderAffix) && text.EndsWith(PostHeaderAffix);
        }

        public static (bool isTrue, string value) IsComment(this string text)
        {
            var m = Regex.Match(text,
                $"^(?!{PreHeaderAffix}|{PreFooterAffix}|{PreDisableAffix})(?:{PreCommentAffix}|#)?(?'comment'[^[=]+?)$");
            return (m.Success, m.Groups["comment"].Value);
        }

        public static string CleanAffix(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            // pattern: @"@"^(?>###--|##--|#--|#|;|\s|\[)*(?'text'.+?)(?>--###|--##|--#|#|;|]|\s)*$""
            var pattern =
                $@"^(?>{PreHeaderAffix}|{PreFooterAffix}|{PreDisableAffix}|{PreCommentAffix}|#|;|\s|\{PreSectionAffix})*(?'clean'.+?)(?>#|;|]|\s|{PostSectionAffix}|{PostHeaderAffix}|{PostFooterAffix}|{PostDisableAffix}|{PostCommentAffix})*$";
            var m = Regex.Match(text, pattern);
            return m.Groups["clean"].Value.Trim();
        }


        // TODO: recognize multiline property value
        public static (bool isTrue, bool isEnabled, string key, string value) IsProperty(this string text)
        {
            var m = Regex.Match(text,
                $@"^(?'preDisable'{PreDisableAffix})?(?'key'[^\[#; ]+?)=(?'value'.*?)(?'postDisable'{PostDisableAffix})?$");
            return (m.Success, !IsDisabled(m), m.Groups["key"].Value, m.Groups["value"].Value);
        }

        public static (bool isTrue, bool isEnable, string sectionName) IsSection(this string text)
        {
            var m = Regex.Match(text,
                $@"^(?'preDisable'{PreDisableAffix})?\[(?'sectionName'.+)\](?'postDisable'{PostDisableAffix})?$");
            return (m.Success, !IsDisabled(m), m.Groups["sectionName"].Value);
        }

        private static bool IsDisabled(Match m)
        {
            return m.Groups["preDisable"].Value == PreDisableAffix && m.Groups["postDisable"].Value == PostDisableAffix;
        }

        public static string AsDisabled(this string text)
        {
            return PreDisableAffix + text + PostDisableAffix;
        }

        public static string AsComment(this string text)
        {
            return PreCommentAffix + text + PostCommentAffix;
        }

        public static string AsHeader(this string text)
        {
            return PreHeaderAffix + text + PostHeaderAffix;
        }

        public static string AsFooter(this string text)
        {
            return PreFooterAffix + text + PostFooterAffix;
        }

        public static string AsSection(this string text)
        {
            return PreSectionAffix + text + PostSectionAffix;
        }

        public static string AddNoneAffix(this string text)
        {
            return PreNoneAffix + text + PostNoneAffix;
        }

        public static bool IsEven(this int value)
        {
            return value % 2 == 0;
        }

        public static bool IsOdd(this int value)
        {
            return value % 2 != 0;
        }

        public static bool IsWithinRange(this int value, int minimum, int maximum)
        {
            return minimum <= value && value <= maximum;
        }

        public static bool IsKnRange(this int value, int minimum, int maximum)
        {
            return Enumerable.Range(minimum, maximum).Contains(value);
        }

        /// <summary>
        /// Validates the given file.
        /// <para> Makes sure that the file exists and its not empty.</para>
        /// </summary>
        /// <param name="file">The file to validate.</param>
        /// <returns>True if the file is valid False if it is not.</returns>
        public static bool IsValidFile(this string file)
        {
            return !string.IsNullOrWhiteSpace(file) && File.Exists(file) && new FileInfo(file).Length > 0;
        }

        /// <summary>
        /// Validates the given path.
        /// <para>- Makes sure that it is not null or whitespace and does not have illegal characters</para>
        /// </summary>
        /// <param name="path">The path to the file including the file name.</param>
        /// <returns>True if the path is valid False if it is not.</returns>
        public static bool IsValidPath(this string path)
        {
            try
            {
                return !string.IsNullOrWhiteSpace(Path.GetFullPath(path));
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsWithin<T>(this T value, T minimum, T maximum) where T : IComparable<T>
        {
            return value.CompareTo(minimum) >= 0 && value.CompareTo(maximum) <= 0;
        }

        public static bool IsPositiveNumber(this int number)
        {
            return number > -1;
        }

        public static bool IsNegativeNumber(this int number)
        {
            return number < 0;
        }

        public static (bool exists, int index) SectionExists(this List<Section> sections, string name)
        {
            return (IsPositiveNumber(IndexOfSection(sections, name)), IndexOfSection(sections, name));
        }

        public static int IndexOfSection(this List<Section> sections, string name)
        {
            return sections.FindIndex(s => s.Name.Equals(name));
        }

        public static (bool exists, int index) PropertyExists(this List<Property> properties, string key, string value)
        {
            return (IsPositiveNumber(IndexOfProperty(properties, key, value)), IndexOfProperty(properties, key, value));
        }

        public static async Task CreateFileAsync(this string path, string text = "")
        {
            using (var fs = File.Create(path))
            {
                if (!string.IsNullOrWhiteSpace(text))
                {
                    using (var sw = new StreamWriter(fs))
                    {
                        await sw.WriteAsync(text);
                    }
                }
            }
        }

        public static int IndexOfProperty(this List<Property> properties, string key, string value)
        {
            return value == null
                ? properties.FindIndex(p => p.Key.Equals(key))
                : properties.FindIndex(p => p.Key.Equals(key) && p.Value.Equals(value));
        }

        public static string AppendLine(this string arg1, string arg2)
        {
            return $"{arg1}\r\n{arg2}";
        }

        public static bool CommentIsValidAndUpdated(
            this string line,
            ref Comment comment)
        {
            if (line.IsComment().isTrue)
            {
                comment = Comment
                    .Builder(comment)
                    .AppendLine(line)
                    .Build();
                return true;
            }

            return false;
        }

        public static bool PropertyIsValidAndUpdated(
            this string line,
            out string key,
            out string value,
            out bool isEnabled)
        {
            key = "";
            value = "";
            isEnabled = true;

            var p = line.IsProperty();
            if (p.isTrue)
            {
                key = p.key;
                value = p.value;
                isEnabled = p.isEnabled;
                return true;
            }

            return false;
        }

        public static bool SectionIsValidAndUpdated(
            this string line,
            ref string name,
            ref bool isEnabled)
        {
            var s = line.IsSection();
            if (s.isTrue)
            {
                name = s.sectionName;
                isEnabled = s.isEnable;
                return true;
            }

            return false;
        }
    }
}