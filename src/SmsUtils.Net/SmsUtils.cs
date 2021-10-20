using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using SmsUtils.Net.Charset;
using SmsUtils.Net.Domain;
using Encoding = SmsUtil.Sms.Encoding;

namespace SmsUtils.Net
{
    public class SmsUtils
    {
        /// <summary>
        /// Determines the necessary Gsm encoding to be used based on the characters in the message
        /// </summary>
        /// <param name="message">The message content</param>
        /// <returns>Encoding that needs to be used</returns>
        public static Encoding GetGsmEncoding(string message)
        {
            if (!GSM0338Charset.ContainsOnlyCharsetCharacters(message, true))
                return Encoding.GSM_UNICODE;

            return Encoding.GSM_7BIT;
        }

        /// <summary>
        /// Determines the necessary encoding based upon the characters in the message and the number of
        /// parts the sms needs to be split into
        /// </summary>
        /// <param name="content">The message</param>
        /// <returns>Parts holding the number of parts and the encoding</returns>
        public static Parts GetNumberOfParts(string content)
        {
            var encoding = GetGsmEncoding(content);

            if (encoding == Encoding.GSM_7BIT)
            {
                return new Parts(Encoding.GSM_7BIT, GetNumberOfPartsFor7BitEncoding(content));
            }

            if (content.Length <= Encoding.GSM_UNICODE.MaxLengthSinglePart)
            {
                return new Parts(Encoding.GSM_UNICODE, 1);
            }

            return new Parts(
                Encoding.GSM_UNICODE,
                (int)Math.Ceiling(content.Length / (float)Encoding.GSM_UNICODE.MaxLengthMultiPart)
            );
        }
        private static int GetNumberOfPartsFor7BitEncoding(string content)
        {
            var content7Bit = EscapeAny7BitExtendedCharsetInContent(content);

            var messageLength = content7Bit.Length;

            if (content7Bit.Length <= Encoding.GSM_7BIT.MaxLengthSinglePart)
            {
                return 1;
            }

            //number of parts if we don't consider that a message part cannot just end with GSM0338Charset.ESCAPE_CHAR
            var parts = (int)Math.Ceiling(messageLength / (float)Encoding.GSM_7BIT.MaxLengthMultiPart);

            //we do some quick "optimization" checking
            //if we have enough left characters in a message part, check if it would fill a worst case scenario
            //where each part was ending with the escape character
            var lastPartChars = messageLength % Encoding.GSM_7BIT.MaxLengthMultiPart;
            var freeChars = 0;

            if (lastPartChars > 0)
                freeChars = Encoding.GSM_7BIT.MaxLengthMultiPart - lastPartChars;

            // There are characters left, don't care about escape character at the end of multi parts
            if (parts <= freeChars)
            {
                //optimization
                return parts;
            }

            // Otherwise "manually" split the message
            return SplitGsm7BitEncodedMessage(content7Bit).Length;
        }

        private static string[] SplitGsm7BitEncodedMessage(string content)
        {
            var parts = new List<string>();
            var contentString = new StringBuilder(content);

            var maxLengthMultipart = Encoding.GSM_7BIT.MaxLengthMultiPart;

            while (contentString.Length > 0)
            {
                if (contentString.Length >= maxLengthMultipart)
                {
                    var endPosition = maxLengthMultipart;
                    if (SmsSplitter.IsMultipartSmsLastCharGsm7BitEscapeChar(contentString.ToString()))
                    {
                        endPosition = endPosition - 1;
                    }
                    parts.Add(contentString.ToString(0, endPosition));
                    contentString = contentString.Remove(0, endPosition);
                }
                else
                {
                    parts.Add(contentString.ToString());
                    break;
                }
            }

            return parts.ToArray();
        }

        public static string EscapeAny7BitExtendedCharsetInContent(string message)
        {
            var content7Bit = new StringBuilder();

            foreach (var ch in message)
            {
                // Add escape characters for extended charset
                if (GSM0338Charset.IsExtendedCharsetCharacter(ch))
                {
                    content7Bit.Append(GSM0338Charset.ESCAPE_CHAR);
                }
                else
                {
                    if (!GSM0338Charset.IsBaseCharsetCharacter(ch))
                    {
                        //also not in the base charset
                        throw new InvalidEnumArgumentException("Message contains '" + ch + "' which is not in GSM0338Charset");
                    }
                }

                content7Bit.Append(ch);
            }

            return content7Bit.ToString();
        }
       
        /// <summary>
        /// Split the SMS into as many parts as necessary according to the determined encoding
        /// </summary>
        /// <param name="message">The message</param>
        /// <returns></returns>
        public static SmsParts SplitSms(string message) => 
            SmsSplitter.SplitSms(message);
    }
}