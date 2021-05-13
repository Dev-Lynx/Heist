using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Heist.Extensions
{
    public static class StringExtensions
    {
        #region Binding
        public static string BindTo<T>(this string body, T model, string markers = "{{}}") where T : class
        {
            int mid = markers.Length / 2;
            string left = markers.Substring(0, mid);
            string right = markers.Substring(mid);

            Regex regex = new Regex($@"{left}([a-zA-Z]+[0-9]*){right}");

            var matches = regex.Matches(body).Cast<Match>()
                .OrderByDescending(i => i.Index);

            foreach (Match match in matches)
            {
                var fullMatch = match.Groups[0];

                var propName = match.Groups[1].Value;

                object value = string.Empty;

                try
                {
                    // use reflection to get property
                    // Note: if you need to use fields use GetField
                    var prop = typeof(T).GetProperty(propName);
                    if (prop == null) continue;

                    value = prop.GetValue(model);

                    if (value == null) value = string.Empty;
                }
                catch (Exception ex)
                {
                    //Core.Log.Debug($"A binding error occured while binding type of ({typeof(T)}) to {body}\n{ex}");
                    return body;
                }

                string change = value.ToString();
                // remove substring with pattern
                // use remove instead of replace, since 
                // you may have several the same string
                // and insert what required
                if (!string.IsNullOrWhiteSpace(change))
                    body = body.Remove(fullMatch.Index, fullMatch.Length)
                        .Insert(fullMatch.Index, change);
            }

            return body;
        }

        public static Stream ToStream(this string s)
        {
            return s.ToStream(Encoding.UTF8);
        }

        public static Stream ToStream(this string s, Encoding encoding)
        {
            return new MemoryStream(encoding.GetBytes(s ?? ""));
        }

        #endregion

        public static bool IsEmailAddress(this string value) => new EmailAddressAttribute().IsValid(value);

        #region Url Refining
        /// <summary>
        /// Creates a URL And SEO friendly slug
        /// </summary>
        /// <param name="text">Text to slugify</param>
        /// <param name="maxLength">Max length of slug</param>
        /// <returns>URL and SEO friendly string</returns>
        /// <remarks>
        /// Cloned from (https://www.johanbostrom.se/blog/how-to-create-a-url-and-seo-friendly-string-in-csharp-text-to-slug-generator)
        /// thanks to JOHAN BOSTRÖM for making this available.
        /// Javascript version (https://gist.github.com/hagemann/382adfc57adbd5af078dc93feef01fe1)
        /// </remarks>
        public static string UrlFriendly(this string text, int maxLength = 0)
        {
            // Return empty value if text is null
            if (text == null) return "";
            var normalizedString = text
                // Make lowercase
                .ToLowerInvariant()
                // Normalize the text
                .Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();
            var stringLength = normalizedString.Length;
            var prevdash = false;
            var trueLength = 0;
            char c;
            for (int i = 0; i < stringLength; i++)
            {
                c = normalizedString[i];
                switch (CharUnicodeInfo.GetUnicodeCategory(c))
                {
                    // Check if the character is a letter or a digit if the character is a
                    // international character remap it to an ascii valid character
                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.UppercaseLetter:
                    case UnicodeCategory.DecimalDigitNumber:
                        if (c < 128)
                            stringBuilder.Append(c);
                        else
                            stringBuilder.Append(RemapInternationalCharToAscii(c));
                        prevdash = false;
                        trueLength = stringBuilder.Length;
                        break;
                    // Check if the character is to be replaced by a hyphen but only if the last character wasn't
                    case UnicodeCategory.SpaceSeparator:
                    case UnicodeCategory.ConnectorPunctuation:
                    case UnicodeCategory.DashPunctuation:
                    case UnicodeCategory.OtherPunctuation:
                    case UnicodeCategory.MathSymbol:
                        if (!prevdash)
                        {
                            stringBuilder.Append('-');
                            prevdash = true;
                            trueLength = stringBuilder.Length;
                        }
                        break;
                }
                // If we are at max length, stop parsing
                if (maxLength > 0 && trueLength >= maxLength)
                    break;
            }
            // Trim excess hyphens
            var result = stringBuilder.ToString().Trim('-');
            // Remove any excess character to meet maxlength criteria
            return maxLength <= 0 || result.Length <= maxLength ? result : result.Substring(0, maxLength);
        }

        /// <summary>
        /// Remaps international characters to ascii compatible ones
        /// based of: https://meta.stackexchange.com/questions/7435/non-us-ascii-characters-dropped-from-full-profile-url/7696#7696
        /// </summary>
        /// <param name="c">Charcter to remap</param>
        /// <returns>Remapped character</returns>
        public static string RemapInternationalCharToAscii(char c)
        {
            string s = c.ToString().ToLowerInvariant();
            if ("àåáâäãåą".Contains(s))
            {
                return "a";
            }
            else if ("èéêëę".Contains(s))
            {
                return "e";
            }
            else if ("ìíîïı".Contains(s))
            {
                return "i";
            }
            else if ("òóôõöøőð".Contains(s))
            {
                return "o";
            }
            else if ("ùúûüŭů".Contains(s))
            {
                return "u";
            }
            else if ("çćčĉ".Contains(s))
            {
                return "c";
            }
            else if ("żźž".Contains(s))
            {
                return "z";
            }
            else if ("śşšŝ".Contains(s))
            {
                return "s";
            }
            else if ("ñń".Contains(s))
            {
                return "n";
            }
            else if ("ýÿ".Contains(s))
            {
                return "y";
            }
            else if ("ğĝ".Contains(s))
            {
                return "g";
            }
            else if (c == 'ř')
            {
                return "r";
            }
            else if (c == 'ł')
            {
                return "l";
            }
            else if (c == 'đ')
            {
                return "d";
            }
            else if (c == 'ß')
            {
                return "ss";
            }
            else if (c == 'þ')
            {
                return "th";
            }
            else if (c == 'ĥ')
            {
                return "h";
            }
            else if (c == 'ĵ')
            {
                return "j";
            }
            else
            {
                return "";
            }
        }
        #endregion

        public static bool AnyIsNullOrEmpty(params string[] strings)
        {
            bool empty = false;

            for (int i = 0; i < strings.Length && !empty; i++)
                empty = !string.IsNullOrEmpty(strings[i]);

            return empty;
        }

        public static string Clean(this string source, params string[] strings)
            => strings.Aggregate(new StringBuilder(source), (current, replacement)
                => current.Replace(replacement, "")).ToString();

        public static string ShortGUID() =>
            Convert.ToBase64String(
            Guid.NewGuid().ToByteArray())
                .Replace("/", "_")
                .Replace("+", "-")
                .TrimEnd('=');

        /// <summary>
        /// Convert a normal string to base64
        /// </summary>
        /// <param name="text">Original String</param>
        /// <returns></returns>
        /// <remarks>
        /// Original Source: https://stackoverflow.com/a/60738564/8058709
        /// </remarks>
        public static string EncodeToBase64(this string text)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(text))
                .TrimEnd('=').Replace('+', '-')
                .Replace('/', '_');
        }



        /// <summary>
        /// Convert a base64 string to a normal one
        /// </summary>
        /// <param name="payload">Base64 string</param>
        /// <returns>A normal string</returns>
        /// <remarks>
        /// Original Source: https://stackoverflow.com/a/60738564/8058709
        /// </remarks>
        public static string DecodeFromBase64(this string payload)
        {
            payload = payload.Replace('_', '/').Replace('-', '+');
            switch (payload.Length % 4)
            {
                case 2:
                    payload += "==";
                    break;
                case 3:
                    payload += "=";
                    break;
            }
            return Encoding.UTF8.GetString(Convert.FromBase64String(payload));
        }

        /// <summary>
        /// Creates a url friendly base64 string from byte array
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        public static string ToBase64(this byte[] payload)
            => Convert.ToBase64String(payload)
                .TrimEnd('=').Replace('+', '-')
                .Replace('/', '_');

        /// <summary>
        /// Creates a byte array from a base64 string
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        public static byte[] FromBase64(this string payload)
        {
            payload = payload.Replace('_', '/').Replace('-', '+');
            switch (payload.Length % 4)
            {
                case 2:
                    payload += "==";
                    break;
                case 3:
                    payload += "=";
                    break;
            }

            return Convert.FromBase64String(payload);
        }
    }
}
