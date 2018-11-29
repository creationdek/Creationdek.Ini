using System;

namespace Creationdek.Ini
{
    /// <summary>
    /// Shapes the output.
    /// </summary>
    [Flags]
    public enum Filters
    {
        None = 0,
        TrimComment = 1 << 0,
        TrimHeader = 1 << 1,
        TrimFooter = 1 << 2,
        TrimDisabled = 1 << 3,
        Formatted = 1 << 4,
        TrimCommentDisabled = TrimComment | TrimDisabled,
        TrimCommentDisabledFormatted = TrimCommentDisabled | Formatted,
        TrimHeaderFooter = TrimHeader | TrimFooter,
        TrimHeaderFooterFormatted = TrimHeaderFooter | Formatted,
        TrimCommentHeaderFooter = TrimComment | TrimHeader | TrimFooter,
        TrimCommentHeaderFooterFormatted = TrimCommentHeaderFooter | Formatted,
        TrimCommentHeaderFooterDisabled = TrimComment | TrimHeader | TrimFooter | TrimDisabled,
        TrimCommentHeaderFooterDisabledFormatted = TrimCommentHeaderFooterDisabled | Formatted
    }
}