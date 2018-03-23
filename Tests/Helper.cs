using System.Collections;
using System.IO.BACnet.Serialize.Decode;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace System.IO.BACnet.Tests
{
    public static class Helper
    {
        public static T[] A<T>(params T[] values)
            => values;

        public static readonly BacnetAddress DummyAddress = new BacnetAddress(BacnetAddressTypes.None, 0, new byte[] { 42 });

        public static (BacnetClient Client1, BacnetClient Client2) CreateConnectedClients()
        {
            var transport1 = new InMemoryTransport();
            var client1 = new BacnetClient(transport1);
            var transport2 = new InMemoryTransport();
            var client2 = new BacnetClient(transport2);

            transport1.BytesSent += transport2.ReceiveBytes;
            transport2.BytesSent += transport1.ReceiveBytes;

            client1.Start();
            client2.Start();

            return (client1, client2);
        }

        public static void AssertPropertiesAndFieldsAreEqual(object expected, object actual)
        {
            if (expected == null)
            {
                Assert.That(actual, Is.Null, "expected == null, checking actual");
                return;
            }

            Assert.That(actual, Is.Not.Null, "checking actual");

            if (expected is IList expectedList && actual is IList actualList)
            {
                for (var i = 0; i < expectedList.Count; i++)
                {
                    AssertPropertiesAndFieldsAreEqual(expectedList[i], actualList[i]);
                }

                return;
            }

            var t = expected.GetType();

            if (t.IsValueType)
            {
                Assert.AreEqual(expected, actual);
                return;
            }

            foreach (var pi in t.GetProperties())
            {
                if (pi.PropertyType.IsValueType || pi.PropertyType == typeof(string))
                    Assert.AreEqual(pi.GetValue(expected, null), pi.GetValue(actual, null), "Property: " + pi.Name);
                else
                    AssertPropertiesAndFieldsAreEqual(pi.GetValue(expected, null), pi.GetValue(actual, null));
            }
            foreach (var fi in t.GetFields())
            {
                var expectedValue = fi.GetValue(expected);
                var actualValue = fi.GetValue(actual);

                if (expectedValue is IList)
                    AssertPropertiesAndFieldsAreEqual(expectedValue, actualValue);
                else
                    Assert.AreEqual(expectedValue, actualValue, "Field: " + fi.Name);
            }
        }

        public static void RunConstructDecoderTest<T>(Func<(T Object, byte[] EncodedBytes)> testDataProvider,
            DecodingFunctionDelegate<T> decodingFunction)
        {
            // arrange
            var data = testDataProvider();
            var context = new Context(data.EncodedBytes, 0);

            // act
            var result = decodingFunction(context);

            //assert
            Assert.That(context.Offset, Is.EqualTo(data.EncodedBytes.Length));
            Assert.That(result.Length, Is.EqualTo(data.EncodedBytes.Length));
            AssertPropertiesAndFieldsAreEqual(data.Object, result.Value);
        }

        public static void RunDecoderTest<T>(Func<(T Object, byte[] EncodedBytes)> testDataProvider,
            Func<Context, Result<T>> decodingFunction)
        {
            // arrange
            var data = testDataProvider();
            var context = new Context(data.EncodedBytes, 0);

            // act
            var result = decodingFunction(context);

            //assert
            Assert.That(context.Offset, Is.EqualTo(data.EncodedBytes.Length));
            Assert.That(result.Length, Is.EqualTo(data.EncodedBytes.Length));
            AssertPropertiesAndFieldsAreEqual(data.Object, result.Value);
        }

        public static string Doc2Code(string input, string variableName = "expectedBytes")
        {
            var hexCodes = input.Split('\r', '\n')
                .Select(line => Regex.Match(line, @"(^|=\s+)X'(?<hex>[^']+)'"))
                .Where(m => m.Success)
                .SelectMany(m => m.Groups["hex"].Value).ToArray();

            var pairs = Enumerable.Range(0, hexCodes.Length)
                .GroupBy(x => x / 2)
                .Select(x => "0x" + new string(x.Select(y => hexCodes[y]).ToArray()))
                .ToArray();

            return $"var {variableName} = new byte[] {{{string.Join(", ", pairs)}}};";
        }
    }
}
