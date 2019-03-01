using System.Diagnostics.CodeAnalysis;
using System.IO.BACnet.Serialize.Decode;
using System.Linq;
using NUnit.Framework;
using static System.IO.BACnet.Tests.Helper;

namespace System.IO.BACnet.Tests.Serialize.Decode
{
    [TestFixture]
    public class PrimitiveDecoderTests
    {
        [Test]
        public void should_decode_null_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] {0x00}; // 20.2.2
            var context = new Context(input, 0);

            // act
            var result = PrimitiveDecoder.Standard.DecodeNull(context);

            //assert
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value, Is.Null);
        }

        public void should_decode_bool_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] {0x10}; // 20.2.3
            var context = new Context(input, 0);

            // act
            var result = PrimitiveDecoder.Standard.DecodeBoolean(context);

            //assert
            Assert.That(context.Offset, Is.EqualTo(input.Length));
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value, Is.True);
        }

        [Test]
        public void should_decode_context_bool_according_to_ashrae_example_20_2_3()
        {
            // arrange
            var input = new byte[] {0x29, 0x01}; // 20.2.3
            var context = new Context(input, 0);

            // act
            var result = PrimitiveDecoder.Standard.DecodeBoolean(context, 2);

            //assert
            Assert.That(context.Offset, Is.EqualTo(input.Length));
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value, Is.True);
        }

        [Test]
        public void should_decode_uint_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] {0x21, 0x48}; // 20.2.4
            var context = new Context(input, 0);

            // act
            var result = PrimitiveDecoder.Standard.DecodeUInt(context);

            //assert
            Assert.That(context.Offset, Is.EqualTo(input.Length));
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value, Is.EqualTo(72));
        }

        [Test]
        public void should_decode_int_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] {0x31, 0x48}; // 20.2.5
            var context = new Context(input, 0);

            // act
            var result = PrimitiveDecoder.Standard.DecodeInt(context);

            //assert
            Assert.That(context.Offset, Is.EqualTo(input.Length));
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value, Is.EqualTo(72));
        }

        [Test]
        public void should_decode_real_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] {0x44, 0x42, 0x90, 0x00, 0x00}; // 20.2.6
            var context = new Context(input, 0);

            // act
            var result = PrimitiveDecoder.Standard.DecodeReal(context);

            //assert
            Assert.That(context.Offset, Is.EqualTo(input.Length));
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value, Is.EqualTo(72.0f));
        }

        [Test]
        public void should_decode_double_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] {0x55, 0x08, 0x40, 0x52, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00}; // 20.2.7
            var context = new Context(input, 0);

            // act
            var result = PrimitiveDecoder.Standard.DecodeDouble(context);

            //assert
            Assert.That(context.Offset, Is.EqualTo(input.Length));
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value, Is.EqualTo(72.0f));
        }

        [Test]
        public void should_decode_octetstring_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] {0x63, 0x12, 0x34, 0xFF}; // 20.2.8
            var context = new Context(input, 0);

            // act
            var result = PrimitiveDecoder.Standard.DecodeOctetString(context);

            //assert
            Assert.That(context.Offset, Is.EqualTo(input.Length));
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value, Is.EquivalentTo(input.Skip(1)));
        }

        [Test]
        public void should_decode_characterstring_utf8_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] // 20.2.9
            {
                0x75, 0x19, 0x00, 0x54, 0x68, 0x69, 0x73, 0x20, 0x69, 0x73, 0x20, 0x61, 0x20, 0x42, 0x41, 0x43, 0x6E,
                0x65, 0x74, 0x20, 0x73, 0x74, 0x72, 0x69, 0x6E, 0x67, 0x21
            };
            var context = new Context(input, 0);

            // act
            var result = PrimitiveDecoder.Standard.DecodeCharacterString(context);

            //assert
            Assert.That(context.Offset, Is.EqualTo(input.Length));
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value, Is.EqualTo("This is a BACnet string!"));
        }

        [Test]
        public void should_decode_characterstring_dbcs_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] // 20.2.9
            {
                0x75, 0x1B, 0x01, 0x03, 0x52, 0x54, 0x68, 0x69, 0x73, 0x20, 0x69, 0x73, 0x20, 0x61, 0x20, 0x42, 0x41,
                0x43, 0x6E, 0x65, 0x74, 0x20, 0x73, 0x74, 0x72, 0x69, 0x6E, 0x67, 0x21
            };
            var context = new Context(input, 0);

            // act
            var result = PrimitiveDecoder.Standard.DecodeCharacterString(context);

            //assert
            Assert.That(context.Offset, Is.EqualTo(input.Length));
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value, Is.EqualTo("This is a BACnet string!"));
        }

        [Test]
        public void should_decode_characterstring_ucs2_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] // 20.2.9
            {
                0x75, 0x31, 0x04, 0x00, 0x54, 0x00, 0x68, 0x00, 0x69, 0x00, 0x73, 0x00, 0x20, 0x00, 0x69, 0x00, 0x73,
                0x00, 0x20, 0x00, 0x61, 0x00, 0x20, 0x00, 0x42, 0x00, 0x41, 0x00, 0x43, 0x00, 0x6E, 0x00, 0x65, 0x00,
                0x74, 0x00, 0x20, 0x00, 0x73, 0x00, 0x74, 0x00, 0x72, 0x00, 0x69, 0x00, 0x6E, 0x00, 0x67, 0x00, 0x21
            };
            var context = new Context(input, 0);

            // act
            var result = PrimitiveDecoder.Standard.DecodeCharacterString(context);

            //assert
            Assert.That(context.Offset, Is.EqualTo(input.Length));
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value, Is.EqualTo("This is a BACnet string!"));
        }

        [Test]
        public void should_decode_bitstring_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] {0x82, 0x03, 0xA8}; // 20.2.10
            var context = new Context(input, 0);

            // act
            var result = PrimitiveDecoder.Standard.DecodeBitString(context);

            //assert
            Assert.That(context.Offset, Is.EqualTo(input.Length));
            Assert.That(result.Length, Is.EqualTo(input.Length));
            AssertPropertiesAndFieldsAreEqual(BitString.Parse("10101"), result.Value);
            Assert.That(result.Value.ToString(), Is.EqualTo("10101"));
        }

        [Test]
        public void should_decode_enumerated_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] { 0x91, 0x00 }; // 20.2.11
            var context = new Context(input, 0);

            // act
            var result = PrimitiveDecoder.Standard.DecodeEnumerated<BacnetObjectTypes>(context);

            //assert
            Assert.That(context.Offset, Is.EqualTo(input.Length));
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value, Is.EqualTo(BacnetObjectTypes.OBJECT_ANALOG_INPUT));
        }

        [Test]
        public void should_decode_date_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] {0xA4, 0x5B, 0x01, 0x18, 0x04}; // 20.2.12
            var context = new Context(input, 0);

            // act
            var result = PrimitiveDecoder.Standard.DecodeDate(context);

            //assert
            Assert.That(context.Offset, Is.EqualTo(input.Length));
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value.DateTime, Is.EqualTo(new DateTime(1991, 01, 24)));
        }

        [Test]
        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        public void should_decode_date_pattern_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] { 0xA4, 0x5B, 0xFF, 0x18, 0xFF }; // 20.2.12
            var context = new Context(input, 0);

            // act
            var result = PrimitiveDecoder.Standard.DecodeDate(context);

            //assert
            Assert.That(context.Offset, Is.EqualTo(input.Length));
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value.DateTime, Is.Null);
            Assert.That(result.Value.IsPattern, Is.True);
            Assert.That(result.Value.Year.Value, Is.EqualTo(1991));
            Assert.That(result.Value.Month, Is.Null);
            Assert.That(result.Value.Day.Value, Is.EqualTo(24));
            Assert.That(result.Value.DayOfWeek, Is.Null);
        }

        [Test]
        public void should_decode_time_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] { 0xB4, 0x11, 0x23, 0x2D, 0x11 }; // 20.2.13
            var context = new Context(input, 0);

            // act
            var result = PrimitiveDecoder.Standard.DecodeTime(context);

            //assert
            Assert.That(context.Offset, Is.EqualTo(input.Length));
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value.TimeSpan, Is.EqualTo(new TimeSpan(0, 17, 35, 45, 170)));
        }

        [Test]
        public void should_decode_objectidentifier_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] { 0xC4, 0x00, 0xC0, 0x00, 0x0F }; // 20.2.14
            var context = new Context(input, 0);

            // act
            var result = PrimitiveDecoder.Standard.DecodeObjectIdentifier(context);

            //assert
            Assert.That(context.Offset, Is.EqualTo(input.Length));
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value.Type, Is.EqualTo(BacnetObjectTypes.OBJECT_BINARY_INPUT));
            Assert.That(result.Value.Instance, Is.EqualTo(15));
        }

        [Test]
        public void should_decode_context_null_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] { 0x38 }; // 20.2.15

            // act
            var result = PrimitiveDecoder.Standard.DecodeTag(input, 0);

            //assert
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value.Class, Is.EqualTo(Class.CONTEXTSPECIFIC));
            Assert.That(result.Value.Number, Is.EqualTo((byte)3));
            Assert.That(result.Value.LengthOrValueOrType, Is.EqualTo(0));
        }

        [Test]
        public void should_decode_context_bool_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] { 0x69, 0x00 }; // 20.2.15
            var context = new Context(input, 0);

            // act
            var result = PrimitiveDecoder.Standard.DecodeBoolean(context, 6);

            //assert
            Assert.That(context.Offset, Is.EqualTo(input.Length));
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value, Is.False);
        }

        [Test]
        public void should_decode_context_bool_27_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] { 0xF9, 0x1B, 0x00 }; // 20.2.15
            var context = new Context(input, 0);

            // act
            var result = PrimitiveDecoder.Standard.DecodeBoolean(context, 27);

            //assert
            Assert.That(context.Offset, Is.EqualTo(input.Length));
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value, Is.False);
        }

        [Test]
        public void should_decode_context_uint_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] { 0x0A, 0x01, 0x00 }; // 20.2.15
            var context = new Context(input, 0);

            // act
            var result = PrimitiveDecoder.Standard.DecodeUInt(context, 0);

            //assert
            Assert.That(context.Offset, Is.EqualTo(input.Length));
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value, Is.EqualTo(256));
        }

        [Test]
        public void should_decode_context_int_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] {0x59, 0xB8}; // 20.2.15
            var context = new Context(input, 0);

            // act
            var result = PrimitiveDecoder.Standard.DecodeInt(context, 5);

            //assert
            Assert.That(context.Offset, Is.EqualTo(input.Length));
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value, Is.EqualTo(-72));
        }

        [Test]
        public void should_decode_context_int_33_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] { 0xF9, 0x21, 0xB8 }; // 20.2.15
            var context = new Context(input, 0);

            // act
            var result = PrimitiveDecoder.Standard.DecodeInt(context, 33);

            //assert
            Assert.That(context.Offset, Is.EqualTo(input.Length));
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value, Is.EqualTo(-72));
        }

        [Test]
        public void should_decode_context_real_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] { 0x0C, 0xC2, 0x05, 0x33, 0x33 }; // 20.2.15
            var context = new Context(input, 0);

            // act
            var result = PrimitiveDecoder.Standard.DecodeReal(context, 0);

            //assert
            Assert.That(context.Offset, Is.EqualTo(input.Length));
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value, Is.EqualTo(-33.3f));
        }

        [Test]
        public void should_decode_context_double_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] { 0x1D, 0x08, 0xC0, 0x40, 0xA6, 0x66, 0x66, 0x66, 0x66, 0x66 }; // 20.2.15
            var context = new Context(input, 0);

            // act
            var result = PrimitiveDecoder.Standard.DecodeDouble(context, 1);

            //assert
            Assert.That(context.Offset, Is.EqualTo(input.Length));
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value, Is.EqualTo(-33.3d));
        }

        [Test]
        public void should_decode_context_double_85_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] { 0xFD, 0x55, 0x08, 0xC0, 0x40, 0xA6, 0x66, 0x66, 0x66, 0x66, 0x66 }; // 20.2.15
            var context = new Context(input, 0);

            // act
            var result = PrimitiveDecoder.Standard.DecodeDouble(context, 85);

            //assert
            Assert.That(context.Offset, Is.EqualTo(input.Length));
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value, Is.EqualTo(-33.3d));
        }

        [Test]
        public void should_decode_context_octetstring_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] { 0x1A, 0x43, 0x21 }; // 20.2.15
            var context = new Context(input, 0);

            // act
            var result = PrimitiveDecoder.Standard.DecodeOctetString(context, 1);

            //assert
            Assert.That(context.Offset, Is.EqualTo(input.Length));
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value, Is.EquivalentTo(input.Skip(1)));
        }

        [Test]
        public void should_decode_context_characterstring_utf8_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] // 20.2.15
            {
                0x5D, 0x19, 0x00, 0x54, 0x68, 0x69, 0x73, 0x20, 0x69, 0x73, 0x20, 0x61, 0x20, 0x42, 0x41, 0x43, 0x6E,
                0x65, 0x74, 0x20, 0x73, 0x74, 0x72, 0x69, 0x6E, 0x67, 0x21
            };
            var context = new Context(input, 0);

            // act
            var result = PrimitiveDecoder.Standard.DecodeCharacterString(context, 5);

            //assert
            Assert.That(context.Offset, Is.EqualTo(input.Length));
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value, Is.EqualTo("This is a BACnet string!"));
        }

        [Test]
        public void should_decode_context_characterstring_utf8_127_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] // 20.2.15
            {
                0xFD, 0x7F, 0x19, 0x00, 0x54, 0x68, 0x69, 0x73, 0x20, 0x69, 0x73, 0x20, 0x61, 0x20, 0x42, 0x41, 0x43,
                0x6E, 0x65, 0x74, 0x20, 0x73, 0x74, 0x72, 0x69, 0x6E, 0x67, 0x21
            };
            var context = new Context(input, 0);

            // act
            var result = PrimitiveDecoder.Standard.DecodeCharacterString(context, 127);

            //assert
            Assert.That(context.Offset, Is.EqualTo(input.Length));
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value, Is.EqualTo("This is a BACnet string!"));
        }

        [Test]
        public void should_decode_characterstring_utf8_nonansi_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] {0x75, 0x0A, 0x00, 0x46, 0x72, 0x61, 0x6E, 0xC3, 0xA7, 0x61, 0x69, 0x73}; // 20.2.15
            var context = new Context(input, 0);

            // act
            var result = PrimitiveDecoder.Standard.DecodeCharacterString(context);

            //assert
            Assert.That(context.Offset, Is.EqualTo(input.Length));
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value, Is.EqualTo("Français"));
        }

        [Test]
        public void should_decode_context_bitstring_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] { 0x0A, 0x03, 0xA8 }; // 20.2.15
            var context = new Context(input, 0);

            // act
            var result = PrimitiveDecoder.Standard.DecodeBitString(context, 0);

            //assert
            Assert.That(context.Offset, Is.EqualTo(input.Length));
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value.ToString(), Is.EqualTo("10101"));
        }

        [Test]
        public void should_decode_context_enumerated_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] { 0x99, 0x00 }; // 20.2.15
            var context = new Context(input, 0);

            // act
            var result = PrimitiveDecoder.Standard.DecodeEnumerated<BacnetObjectTypes>(context, 9);

            //assert
            Assert.That(context.Offset, Is.EqualTo(input.Length));
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value, Is.EqualTo(BacnetObjectTypes.OBJECT_ANALOG_INPUT));
        }

        [Test]
        public void should_decode_context_date_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] { 0x9C, 0x5B, 0x01, 0x18, 0x04 }; // 20.2.15
            var context = new Context(input, 0);

            // act
            var result = PrimitiveDecoder.Standard.DecodeDate(context, 9);

            //assert
            Assert.That(context.Offset, Is.EqualTo(input.Length));
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value.DateTime, Is.EqualTo(new DateTime(1991, 01, 24)));
        }

        [Test]
        public void should_decode_context_time_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] { 0x4C, 0x11, 0x23, 0x2D, 0x11 }; // 20.2.15
            var context = new Context(input, 0);

            // act
            var result = PrimitiveDecoder.Standard.DecodeTime(context, 4);

            //assert
            Assert.That(context.Offset, Is.EqualTo(input.Length));
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value.TimeSpan, Is.EqualTo(new TimeSpan(0, 17, 35, 45, 170)));
        }

        [Test]
        public void should_decode_context_objectidentifier_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] { 0x4C, 0x00, 0xC0, 0x00, 0x0F }; // 20.2.15
            var context = new Context(input, 0);

            // act
            var result = PrimitiveDecoder.Standard.DecodeObjectIdentifier(context, 4);

            //assert
            Assert.That(context.Offset, Is.EqualTo(input.Length));
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value.Type, Is.EqualTo(BacnetObjectTypes.OBJECT_BINARY_INPUT));
            Assert.That(result.Value.Instance, Is.EqualTo(15));
        }

        [Test]
        public void GenerateCode()
        {
            Console.WriteLine(Doc2Code(@"
Encoded Tag = X'75'
Length Extension = X'0A'
Character Set = X'00' (ISO 10646: UTF-8)
Encoded Data = X'4672616EC3A7616973'
Example: Context-
", "input"));
        }
    }
}
