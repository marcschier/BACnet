using System.Diagnostics.CodeAnalysis;
using System.IO.BACnet.Serialize.Decode;
using System.IO.BACnet.Tests.TestData;
using System.Linq;
using NUnit.Framework;

namespace System.IO.BACnet.Tests.Serialize.Decode
{
    [TestFixture]
    public class DecoderTests
    {
        [Test]
        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        public void should_decode_datetime_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] {0xA4, 0x5B, 0x01, 0x18, 0x05, 0xB4, 0x11, 0x23, 0x2D, 0x11}; // 20.2.16
            var context = new Context(input, 0);

            // act
            var result = ConstructDecoder.Standard.DecodeDateTime(context);

            //assert
            Assert.That(context.Offset, Is.EqualTo(input.Length));
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value.NativeDateTime.Value,
                Is.EqualTo(new DateTime(1991, 01, 24, 17, 35, 45).AddMilliseconds(170)));
        }

        [Test]
        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        public void should_decode_context_datetime_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] {0x0E, 0xA4, 0x5B, 0x01, 0x18, 0x05, 0xB4, 0x11, 0x23, 0x2D, 0x11, 0x0F}; // 20.2.16
            var context = new Context(input, 0);

            // act
            var result = ConstructDecoder.Standard.DecodeDateTime(context, 0);

            //assert
            Assert.That(context.Offset, Is.EqualTo(input.Length));
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value.NativeDateTime.Value,
                Is.EqualTo(new DateTime(1991, 01, 24, 17, 35, 45).AddMilliseconds(170)));
        }

        [Test]
        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        public void should_decode_context_datetime_47_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] {0xFE, 0x2F, 0xA4, 0x5B, 0x01, 0x18, 0x05, 0xB4, 0x11, 0x23, 0x2D, 0x11, 0xFF, 0x2F}; // 20.2.16
            var context = new Context(input, 0);

            // act
            var result = ConstructDecoder.Standard.DecodeDateTime(context, 47);

            //assert
            Assert.That(context.Offset, Is.EqualTo(input.Length));
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value.NativeDateTime.Value,
                Is.EqualTo(new DateTime(1991, 01, 24, 17, 35, 45).AddMilliseconds(170)));
        }


        [Test]
        public void should_decode_sequence_of_integer_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] {0x21, 0x01, 0x21, 0x02, 0x21, 0x04}; // 20.2.17
            var context = new Context(input, 0);

            // act
            var result =
                ConstructDecoder.Standard.DecodeSequenceOf(context, PrimitiveDecoder.Standard.DecodeUInt);

            //assert
            Assert.That(context.Offset, Is.EqualTo(input.Length));
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value, Is.EquivalentTo(new[] {1, 2, 4}));
        }

        [Test]
        public void should_decode_tagged_sequence_of_integer_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] {0x1E, 0x21, 0x01, 0x21, 0x02, 0x21, 0x04, 0x1F}; // 20.2.17
            var context = new Context(input, 0);

            // act
            var result =
                ConstructDecoder.Standard.DecodeSequenceOf(context, PrimitiveDecoder.Standard.DecodeUInt, 1);

            //assert
            Assert.That(context.Offset, Is.EqualTo(input.Length));
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value, Is.EquivalentTo(new[] { 1, 2, 4 }));
        }

        [Test]
        public void should_decode_tagged_sequence_of_constructeddata_according_to_ashrae_example()
        {
            // arrange
            var input = new byte[] // 20.2.17
            {
                0xA4, 0x5B, 0x01, 0x18, 0x04, 0xB4, 0x11, 0x00, 0x00, 0x00, 0xA4, 0x5B, 0x01, 0x18, 0x04, 0xB4, 0x12,
                0x2D, 0x00, 0x00
            };
            var context = new Context(input, 0);

            // act
            var result =
                ConstructDecoder.Standard.DecodeSequenceOf(context, ConstructDecoder.Standard.DecodeDateTime);

            //assert
            Assert.That(context.Offset, Is.EqualTo(input.Length));
            Assert.That(result.Length, Is.EqualTo(input.Length));
            Assert.That(result.Value.Select(v => v.NativeDateTime), Is.EquivalentTo(new[]
            {
                new DateTime(1991, 01, 24, 17, 00, 00), 
                new DateTime(1991, 01, 24, 18, 45, 00)
            }));
        }

        [Test]
        public void should_decode_destination_according_to_field_observation()
            => Helper.RunConstructDecoderTest(Field.BACNetDestination, ConstructDecoder.Standard.DecodeDestination);

        [Test]
        public void should_decode_addressbinding_according_to_field_observation()
            => Helper.RunConstructDecoderTest(Field.BACNetAddressBinding, ConstructDecoder.Standard.DecodeAddressBinding);

        [Test]
        public void should_decode_deviceobjectreference_according_to_field_observation()
            => Helper.RunConstructDecoderTest(Field.BACNetDeviceObjectReference, ConstructDecoder.Standard.DecodeDeviceObjectReference);
    }
}
