using SmsUtil.Sms;

namespace SmsUtils.Net.Domain
{
    public sealed class Parts
    {
        public Parts(Encoding encoding, int numberOfParts)
        {
            Encoding = encoding;
            NumberOfParts = numberOfParts;
        }

        public Encoding Encoding { get; }
        public int NumberOfParts { get; }
    }
}
