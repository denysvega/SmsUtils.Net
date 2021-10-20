using System;
using System.Collections.Generic;
using System.Text;
using SmsUtils.Net.Charset;
using SmsUtils.Net.Domain;
using Encoding = SmsUtil.Sms.Encoding;

namespace SmsUtils.Net
{
    public class SmsSplitter
    {
        public static SmsParts SplitSms(string content)
        {
            var encoding = SmsUtils.GetGsmEncoding(content);

            if (encoding == Encoding.GSM_7BIT)
            {
                var escapedContent = SmsUtils.EscapeAny7BitExtendedCharsetInContent(content);
                if (escapedContent.Length <= Encoding.GSM_7BIT.MaxLengthSinglePart)
                    return new SmsParts(Encoding.GSM_7BIT, new[] { escapedContent });

                return new SmsParts(Encoding.GSM_7BIT, SplitGsm7BitEncodedMessage(escapedContent));
            }

            if (content.Length <= Encoding.GSM_UNICODE.MaxLengthSinglePart)
                return new SmsParts(Encoding.GSM_UNICODE, new[] { content });

            return new SmsParts(Encoding.GSM_UNICODE, SplitUnicodeEncodedMessage(content));
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
                    if (IsMultipartSmsLastCharGsm7BitEscapeChar(contentString.ToString()))
                        endPosition = endPosition - 1;

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

        private static string[] SplitUnicodeEncodedMessage(string content)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            var parts = new List<string>();
            var contentString = new StringBuilder(content);

            var maxLengthMultipart = Encoding.GSM_UNICODE.MaxLengthMultiPart;

            while (contentString.Length > 0)
            {
                if (contentString.Length >= (maxLengthMultipart))
                {
                    parts.Add(contentString.ToString(0, maxLengthMultipart));
                    contentString = contentString.Remove(0, maxLengthMultipart);
                }
                else
                {
                    parts.Add(contentString.ToString());
                    break;
                }
            }

            return parts.ToArray();
        }

        public static bool IsMultipartSmsLastCharGsm7BitEscapeChar(string content)
        {
            return content[Encoding.GSM_7BIT.MaxLengthMultiPart - 1] == GSM0338Charset.ESCAPE_CHAR;
        }
    }
}