using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Text;

namespace TeamTaskManager.Helpers
{
    // https://jonathancrozier.com/blog/how-to-replace-accented-characters-with-plain-characters-using-c-sharp
    public static class StringExtensions
    {
        public static string RemoveDiacritics(this string text)
        {
            string normalizedString = text.Normalize(NormalizationForm.FormD);

            var stringBuilder = new StringBuilder(normalizedString.Length);

            foreach (char c in normalizedString)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString();
        }
    }
}
