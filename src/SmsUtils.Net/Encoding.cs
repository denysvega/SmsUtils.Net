// ReSharper disable All
namespace SmsUtil.Sms
{
    public sealed class Encoding
    {
        /// <summary>
        /// Encoding that is used for messages that have all the characters in the GSM0338Charset
        /// </summary>
        public static readonly Encoding GSM_7BIT = new Encoding(160, 153);

        /// <summary>
        /// Encoding that is used for messages that have characters outside of the Gsm7BitCharset
        /// </summary>
        public static readonly Encoding GSM_UNICODE = new Encoding(70, 67);

        public Encoding(int maxLengthSinglePart, int maxLengthMultiPart)
        {
            MaxLengthSinglePart = maxLengthSinglePart;
            MaxLengthMultiPart = maxLengthMultiPart;
        }

        public int MaxLengthSinglePart { get; }

        /// <summary>
        /// For SMS messages that are split into multiple parts, some bytes need to be used as a header to
        /// establish a sequence for reassembly the parts when they arrive at the destination
        /// </summary>
        public int MaxLengthMultiPart { get; }


        public override string ToString()
        {
            return $"Multi: {MaxLengthMultiPart} / Single: {MaxLengthSinglePart}";
        }
    }
}