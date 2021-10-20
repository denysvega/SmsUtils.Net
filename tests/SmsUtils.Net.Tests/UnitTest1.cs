using System;
using System.Diagnostics;
using NUnit.Framework;
using SmsUtil.Sms;
using SmsUtils.Net.Domain;

namespace SmsUtils.Net.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void CharactersFromGsm7BitCharsetTakeUpTwoSpaces()
        {
            const string message = "\f^{}\\[~]|€";

            Assert.IsTrue(
                SmsUtils.EscapeAny7BitExtendedCharsetInContent(message).Length == message.Length * 2,
                "GSM7Bit extended-charset characters are escaped and take twice as much space"
            );
        }

        [Test]
        public void EncodingIsSwitchedForMessagesWithCharactersNotInGsm7Bit()
        {
            const string message = "Hello țar";
            Assert.IsTrue(
                SmsSplitter.SplitSms(message).Encoding == Encoding.GSM_UNICODE,
                "Encoding is changed to Unicode"
            );
        }

        [Test]
        public void MessageFitsInsideSinglePartSmsWithGsm7BitEncoding()
        {
            const string message =
                "11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111" +
                "11111111111111111111111111111111111111111111111111111111111111111";

            Debug.Assert(message.Length == 160);

            SmsParts smsParts = SmsSplitter.SplitSms(message);

            Assert.IsTrue(
                smsParts.Encoding == Encoding.GSM_7BIT,
                "Encoding is Gsm7Bit"
            );

            Assert.IsTrue(
                smsParts.Parts.Length == 1 && smsParts.Parts[0].Length == 160,
                "Fits inside a single part sms of 160"
            );
        }

        [Test]
        public void MessageOverSinglePartSmsWithGsm7BitEncodingIsSplitIntoTwoPartsEachOfMaxMultipartSizeOf153()
        {
            const string message =
                "11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111" +
                "111111111111111111111111111111111111111111111111111111111111111112";

            Debug.Assert(message.Length == 161); //
            SmsParts smsParts = SmsUtils.SplitSms(message);

            Assert.IsTrue(
                smsParts.Encoding == Encoding.GSM_7BIT,
                "Encoding is Gsm7Bit"
            );

            Assert.IsTrue(
                smsParts.Parts.Length == 2,
                "Simple message fits inside a two part sms"
            );

            Assert.IsTrue(
                smsParts.Parts[0].Length == 153,
                "First part size = 153"
            );
            Assert.IsTrue(
                smsParts.Parts[1].Length == 8,
                "Second part size = (161 - 153) = 8"
            );
        }

        [Test]
        public void MessageIsSplitInto3PartsBecauseOfEndingWithTheGsm7BitEscapeCharacter()
        {
            const string message =
                "11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111100000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000091€";
            Debug.Assert(message.Length == 306); //Theoretically would fit inside 2part * 153

            var smsParts = SmsUtils.SplitSms(message);
            Assert.IsTrue(
                smsParts.Encoding == Encoding.GSM_7BIT,
                "Encoding is Gsm7Bit"
            );

            Assert.IsTrue(
                smsParts.Parts.Length == 3,
                "It takes 3 parts"
            );

            Assert.IsTrue(
                smsParts.Parts[1].Length == 152,
                "Second part is smaller by one(since Euro character escape cannot sit alone)"
            );

            Parts numParts = SmsUtils.GetNumberOfParts(message);
            Assert.IsTrue(
                numParts.Encoding == Encoding.GSM_7BIT,
                "Encoding is Gsm7Bit - numParts"
            );
            Assert.IsTrue(
                numParts.NumberOfParts == 3,
                "It takes 3 parts - numParts"
            );
        }

        [Test]
        public void MessageThatNeedsUnicodeCanHaveMaxLenghtOf70()
        {
            String message = "Д111111111111111111111111111111111111111111111111111111111111111111111";
            Debug.Assert(message.Length == 70);

            SmsParts smsParts = SmsUtils.SplitSms(message);
            Assert.IsTrue(
                smsParts.Encoding == Encoding.GSM_UNICODE,
                "Encoding is Unicode"
            );

            Assert.IsTrue(
                smsParts.Parts.Length == 1 && smsParts.Parts[0].Length == 70,
                "Fits inside a single part sms but with Unicode max Length of 70"
            );

            Parts numParts = SmsUtils.GetNumberOfParts(message);
            Assert.IsTrue(
                numParts.Encoding == Encoding.GSM_UNICODE,
                "Encoding is Unicode - numParts"
            );

            Assert.IsTrue(
                numParts.NumberOfParts == 1,
                "Fits inside a single part sms"
            );
        }

        [Test]
        public void MessageThatNeedsUnicodeCanHaveMaxLengthOf67IfSplitMultipart()
        {
            String message = "Д1111111111111111111111111111111111111111111111111111111111111111111112";
            Debug.Assert(message.Length == 71);

            SmsParts smsParts = SmsUtils.SplitSms(message);
            Assert.IsTrue(
                smsParts.Encoding == Encoding.GSM_UNICODE,
                "Encoding is Unicode"
            );

            Assert.IsTrue(
                smsParts.Parts.Length == 2,
                "Fits inside a 2 part sms"
            );

            Assert.IsTrue(
                smsParts.Parts[0].Length == 67,
                "First part size = 67"
            );

            Assert.IsTrue(
                smsParts.Parts[1].Length == 4,
                "Second part size = (71 - 67)"
            );

            Parts numParts = SmsUtils.GetNumberOfParts(message);

            Assert.IsTrue(
                numParts.Encoding == Encoding.GSM_UNICODE,
                "Encoding is Unicode - numParts"
            );

            Assert.IsTrue(
                numParts.NumberOfParts == 2,
                "It takes 2 parts - numParts"
            );
        }

        [Test]
        public void MessageThatNeedsUnicodeDontRequireEscapeCharacterForCharactersInExtendedGsm7BitCharset()
        {
            const string
                message = "€" + "Д"; //€ is in the extended GSM0338Charset however since the needed change to Unicode
            //required by the Д character, it's no longer required to escape the €

            SmsParts smsParts = SmsUtils.SplitSms(message);
            Assert.IsTrue(
                smsParts.Encoding == Encoding.GSM_UNICODE,
                "Encoding is Unicode"
            );

            Assert.IsTrue(
                smsParts.Parts[0].Length == 2,
                "Message takes up 2 spaces"
            );
        }

        [Test]
        public void SmsPartDetectionBypassOptimization()
        {
            string message =
                "11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111€000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000009€222222222222222222222222222222222333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333€44444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444€567";

            Parts numParts = SmsUtils.GetNumberOfParts(message);
            Assert.IsTrue(
                numParts.NumberOfParts == 5,
                "Message takes up 5 spaces"
            );

            message =
                "11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111€000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000009€222222222222222222222222222222222333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333333€44444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444444€";
            numParts = SmsUtils.GetNumberOfParts(message);

            Assert.IsTrue(
                numParts.NumberOfParts == 4,
                "Message takes up 5 spaces"
            );
        }
    }
}