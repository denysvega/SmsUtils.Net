using System.Collections.Generic;

// ReSharper disable InconsistentNaming
namespace SmsUtils.Net.Charset
{
    public class GSM0338Charset
    {
        public const char ESCAPE_CHAR = '\u001b';

        private static readonly HashSet<string> BASE_CHARSET =
            new HashSet<string>(
                new[]
                {
                    "@", "£", "$", "¥", "è", "é", "ù", "ì", "ò", "ç", "\n", "Ø", "ø", "\r", "Å", "å",
                    "Δ", "_", "Φ", "Γ", "Λ", "Ω", "Π", "Ψ", "Σ", "Θ", "Ξ", "\u001b", "Æ", "æ", "ß", "É",
                    " ", "!", "'", "#", "¤", "%", "&", "\"", "(", ")", "*", "+", ",", "-", ".", "/",
                    "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", ":", ";", "<", "=", ">", "?",
                    "¡", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O",
                    "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "Ä", "Ö", "Ñ", "Ü", "§",
                    "¿", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o",
                    "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "ä", "ö", "ñ", "ü", "à"
                }
            );

        private static readonly HashSet<string> EXTENDED_CHARSET =
            new HashSet<string>(
                new[]
                {
                    "\f", "^", "{", "}", "\\", "[", "~", "]", "|", "€"
                }
            );

        public static bool ContainsOnlyBaseCharsetCharacters(string content)
            => ContainsOnlyCharsetCharacters(content, false);

        public static bool IsBaseCharsetCharacter(char ch)
            => BASE_CHARSET.Contains(char.ToString(ch));

        public static bool IsExtendedCharsetCharacter(char ch)
            => EXTENDED_CHARSET.Contains(char.ToString(ch));

        public static bool ContainsOnlyCharsetCharacters(string message, bool includeExtendedCharset)
        {
            foreach (var ch in message)
            {
                if ((
                    BASE_CHARSET.Contains(char.ToString(ch))
                    ||
                    (includeExtendedCharset && IsExtendedCharsetCharacter(ch))
                ) == false)
                {
                    return false;
                }
            }

            return true;
        }
    }
}