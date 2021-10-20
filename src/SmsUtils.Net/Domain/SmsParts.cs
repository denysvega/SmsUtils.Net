using SmsUtil.Sms;

namespace SmsUtils.Net.Domain
{
    public sealed class SmsParts
    {
        public SmsParts(Encoding encoding, string[] parts)
        {
            Encoding = encoding;
            Parts = parts;
        }

        public Encoding Encoding { get; }
        public string[] Parts { get; }
    }
}