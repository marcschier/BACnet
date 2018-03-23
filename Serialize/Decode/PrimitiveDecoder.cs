using System.Text;

namespace System.IO.BACnet.Serialize.Decode
{
    public class PrimitiveDecoder
    {
        public static PrimitiveDecoder Standard = new PrimitiveDecoder(NumericDecoder.Standard);

        private readonly NumericDecoder _numericDecoder;

        public PrimitiveDecoder(NumericDecoder numericDecoder)
        {
            _numericDecoder = numericDecoder;
        }

        public virtual Result<Tag> DecodeTag(byte[] buffer, int offset)
        {
            var length = 1;
            var @class = (buffer[offset] & 1 << 3) == 0 ? Class.APPLICATION : Class.CONTEXTSPECIFIC;

            var tagNumber = (byte) (buffer[offset] >> 4);
            var value = (uint) (buffer[offset] & 0b111);

            if (tagNumber == 0b1111)
                tagNumber = buffer[offset + length++];

            if (value == 0b101)
                value = buffer[offset + length++];

            switch (value)
            {
                case 0b110 when @class == Class.CONTEXTSPECIFIC:
                    @class = Class.OPENING;
                    break;

                case 0b111 when @class == Class.CONTEXTSPECIFIC:
                    @class = Class.CLOSING;
                    break;

                case 254:
                {
                    var result = _numericDecoder.DecodeNumber<ushort>(buffer, offset + length, sizeof(ushort), sizeof(ushort));
                    length += result.Length;
                    value = result.Value;
                }
                    break;

                case 255:
                {
                    var result = _numericDecoder.DecodeNumber<uint>(buffer, offset + length, sizeof(uint), sizeof(uint));
                    length += result.Length;
                    value = result.Value;
                }
                    break;
            }

            var tag = new Tag(@class, tagNumber, value);
            return Result.Create(tag, length);
        }

        public virtual Result<Tag> DecodeTag(Context context, Class expectedClass, byte? expectedTag = null,
            bool required = true, bool peek = false)
        {
            var tagResult = DecodeTag(context.Buffer, context.Offset);

            if (tagResult.Value.Class != expectedClass || expectedTag.HasValue && tagResult.Value.Number != expectedTag)
            {
                if (required)
                    throw new ExpectedTagNotFoundException(expectedClass, expectedTag, tagResult.Value);

                return null;
            }

            return peek ? tagResult : context.ProcessResult(tagResult);
        }

        public virtual Result<object> DecodeNull(Context context, byte? expectedTag = null, bool required = true)
        {
            var tag = DecodeTag(context, 0, expectedTag, required)?.Value;

            // ReSharper disable once PossibleNullReferenceException
            if (required && tag.LengthOrValueOrType != 0)
                throw new DecodingException("Expected NULL");

            return Result.Create<object>(null, 1);
        }

        private Result<Tag> DecodeTag(Context context, byte applicationTag, byte? expectedTag = null,
            bool required = true)
            => DecodeTag(
                context, expectedTag == null ? Class.APPLICATION : Class.CONTEXTSPECIFIC,
                expectedTag ?? applicationTag, required);

        [Decodes(BacnetPropertyIds.PROP_OUT_OF_SERVICE)]
        [Decodes(BacnetPropertyIds.PROP_DAYLIGHT_SAVINGS_STATUS)]
        [Decodes(BacnetPropertyIds.PROP_MAINTENANCE_REQUIRED, BacnetObjectTypes.OBJECT_LIFE_SAFETY_ZONE)]
        public virtual Result<bool> DecodeBoolean(Context context, byte? expectedTag = null,
            bool required = true)
        {
            var tagResult = DecodeTag(context, 1, expectedTag, required);
            if (tagResult == null)
                return null;

            switch (tagResult.Value.Class)
            {
                case Class.APPLICATION:
                    return context.ProcessResult(Result.Create(tagResult.Value.LengthOrValueOrType == 1, tagResult.Length), 0);

                case Class.CONTEXTSPECIFIC:
                    var value = context.Buffer[context.Offset];
                    bool? retVal;
                    switch (value)
                    {
                        case 0:
                            retVal = false;
                            break;
                        case 1:
                            retVal = true;
                            break;
                        default:
                            throw new DecodingException($"Unexpected value: '{value}'");
                    }

                    return context.ProcessResult(Result.Create(retVal.Value, tagResult.Length + 1), 1);

                default:
                    throw new DecodingException(
                        $"Unexpected {nameof(tagResult.Value.Class)} '{tagResult.Value.Class}'");
            }
        }

        [Decodes(BacnetPropertyIds.PROP_NOTIFICATION_CLASS)]
        [Decodes(BacnetPropertyIds.PROP_VENDOR_IDENTIFIER)]
        [Decodes(BacnetPropertyIds.PROP_TIME_DELAY)]
        [Decodes(BacnetPropertyIds.PROP_PROTOCOL_VERSION)]
        [Decodes(BacnetPropertyIds.PROP_PROTOCOL_REVISION)]
        [Decodes(BacnetPropertyIds.PROP_MAX_APDU_LENGTH_ACCEPTED)]
        [Decodes(BacnetPropertyIds.PROP_APDU_SEGMENT_TIMEOUT)]
        [Decodes(BacnetPropertyIds.PROP_APDU_TIMEOUT)]
        [Decodes(BacnetPropertyIds.PROP_NUMBER_OF_APDU_RETRIES)]
        [Decodes(BacnetPropertyIds.PROP_DATABASE_REVISION)]
        [Decodes(BacnetPropertyIds.PROP_MAX_SEGMENTS_ACCEPTED)]
        [Decodes(BacnetPropertyIds.PROP_NUMBER_OF_STATES)]
        [Decodes(BacnetPropertyIds.PROP_PRESENT_VALUE, BacnetObjectTypes.OBJECT_MULTI_STATE_VALUE)]
        [Decodes(BacnetPropertyIds.PROP_PRESENT_VALUE, BacnetObjectTypes.OBJECT_POSITIVE_INTEGER_VALUE)]
        [DecodesArrayItem(BacnetPropertyIds.PROP_PRIORITY, 3)]
        public virtual Result<uint> DecodeUInt(Context context, byte? expectedTag = null,
            bool required = true)
        {
            var tagResult = DecodeTag(context, 2, expectedTag, required);
            if (tagResult == null)
                return null;

            var uintResult = _numericDecoder.DecodeNumber<uint>(
                context.Buffer, context.Offset, (int) tagResult.Value.LengthOrValueOrType, sizeof(uint));

            return context.ProcessResult(uintResult, tagResult);
        }

        [Decodes(BacnetPropertyIds.PROP_UTC_OFFSET)]
        public virtual Result<int> DecodeInt(Context context, byte? expectedTag = null,
            bool required = true)
        {
            var tagResult = DecodeTag(context, 3, expectedTag, required);
            if (tagResult == null)
                return null;

            var intResult = _numericDecoder.DecodeNumber<int>(
                context.Buffer, context.Offset, (int) tagResult.Value.LengthOrValueOrType,
                sizeof(int));

            return context.ProcessResult(intResult, tagResult);
        }

        [Decodes(BacnetPropertyIds.PROP_PRESENT_VALUE, BacnetObjectTypes.OBJECT_ANALOG_INPUT)]
        [Decodes(BacnetPropertyIds.PROP_TRACKING_VALUE, BacnetObjectTypes.OBJECT_ANALOG_INPUT)]
        public virtual Result<float> DecodeReal(Context context, byte? expectedTag = null,
            bool required = true)
        {
            var tagResult = DecodeTag(context, 4, expectedTag, required);
            if (tagResult == null)
                return null;

            var floatResult = _numericDecoder.DecodeNumber<float>(
                context.Buffer, context.Offset, (int) tagResult.Value.LengthOrValueOrType,
                sizeof(float));

            return context.ProcessResult(floatResult, tagResult);
        }

        public virtual Result<double> DecodeDouble(Context context, byte? expectedTag = null,
            bool required = true)
        {
            var tagResult = DecodeTag(context, 5, expectedTag, required);
            if (tagResult == null)
                return null;

            var doubleResult = _numericDecoder.DecodeNumber<double>(
                context.Buffer, context.Offset, (int) tagResult.Value.LengthOrValueOrType,
                sizeof(double));

            return context.ProcessResult(doubleResult, tagResult);
        }

        public virtual Result<byte[]> DecodeOctetString(Context context, byte? expectedTag = null,
            bool required = true)
        {
            var tagResult = DecodeTag(context, 6, expectedTag, required);
            if (tagResult == null)
                return null;

            var octets = new byte[tagResult.Value.LengthOrValueOrType];
            Array.Copy(context.Buffer, context.Offset, octets, 0, octets.Length);

            return context.ProcessResult(
                Result.Create(octets, (int) (tagResult.Length + tagResult.Value.LengthOrValueOrType)),
                (int)tagResult.Value.LengthOrValueOrType);
        }

        [Decodes(BacnetPropertyIds.PROP_DEVICE_TYPE)]
        [Decodes(BacnetPropertyIds.PROP_PROFILE_NAME)]
        [Decodes(BacnetPropertyIds.PROP_DESCRIPTION)]
        [Decodes(BacnetPropertyIds.PROP_OBJECT_NAME)]
        [Decodes(BacnetPropertyIds.PROP_VENDOR_NAME)]
        [Decodes(BacnetPropertyIds.PROP_MODEL_NAME)]
        [Decodes(BacnetPropertyIds.PROP_FIRMWARE_REVISION)]
        [Decodes(BacnetPropertyIds.PROP_APPLICATION_SOFTWARE_VERSION)]
        [Decodes(BacnetPropertyIds.PROP_LOCATION)]
        [DecodesSequenceItem(BacnetPropertyIds.PROP_EVENT_MESSAGE_TEXTS)]
        [DecodesSequenceItem(BacnetPropertyIds.PROP_STATE_TEXT)]
        public virtual Result<string> DecodeCharacterString(Context context, byte? expectedTag = null,
            bool required = true)
        {
            var tagResult = DecodeTag(context, 7, expectedTag, required);
            if (tagResult == null)
                return null;

            var offset = context.Offset;
            var characterSet = context.Buffer[offset];
            var processedBytes = 1;

            Encoding textEncoding;

            switch (characterSet)
            {
                // 'normal' encoding, backward compatible ANSI_X34 (for decoding only)
                case 0:
                    textEncoding = Encoding.UTF8;
                    break;

                // IBM/Microsoft DBCS
                case 1:
                    var codepageBytes = new byte[]
                        {context.Buffer[offset + processedBytes + 1], context.Buffer[offset + processedBytes], 0x00, 0x00};
                    var codepage = BitConverter.ToInt32(codepageBytes, 0);
                    processedBytes += 2;
                    textEncoding = Encoding.GetEncoding(codepage);
                    break;

                // UCS2 is backward compatible UTF16 (for decoding only)
                // http://hackipedia.org/Character%20sets/Unicode,%20UTF%20and%20UCS%20encodings/UCS-2.htm
                // https://en.wikipedia.org/wiki/Byte_order_mark
                case 4:
                    if (context.Buffer[offset + processedBytes] == 0xFF && context.Buffer[offset + processedBytes + 1] == 0xFE
                    ) // Byte Order Mark
                        textEncoding = Encoding.Unicode; // little endian encoding
                    else
                        textEncoding =
                            Encoding.BigEndianUnicode; // big endian encoding if BOM is not set, or 0xFE-0xFF
                    break;

                default:
                    throw new DecodingException($"Unsupported character-set: '{characterSet}'");
            }

            var str = textEncoding.GetString(
                context.Buffer, offset + processedBytes,
                (int) (tagResult.Value.LengthOrValueOrType - processedBytes));

            return context.ProcessResult(
                Result.Create(str, (int) (tagResult.Length + tagResult.Value.LengthOrValueOrType)),
                (int)tagResult.Value.LengthOrValueOrType);
        }

        [Decodes(BacnetPropertyIds.PROP_ACK_REQUIRED)]
        [Decodes(BacnetPropertyIds.PROP_ACKED_TRANSITIONS)]
        [Decodes(BacnetPropertyIds.PROP_STATUS_FLAGS)]
        [Decodes(BacnetPropertyIds.PROP_EVENT_ENABLE)]
        [Decodes(BacnetPropertyIds.PROP_PROTOCOL_SERVICES_SUPPORTED)]
        [Decodes(BacnetPropertyIds.PROP_PROTOCOL_OBJECT_TYPES_SUPPORTED)]
        public virtual Result<BitString> DecodeBitString(Context context, byte? expectedTag = null,
            bool required = true)
        {
            var tagResult = DecodeTag(context, 8, expectedTag, required);
            if (tagResult == null)
                return null;

            var length = (int) (tagResult.Length + tagResult.Value.LengthOrValueOrType);

            if (tagResult.Value.LengthOrValueOrType < 2)
                return context.ProcessResult(
                    Result.Create(new BitString(), length), (int) tagResult.Value.LengthOrValueOrType);

            var offset = context.Offset;
            var unusedBits = context.Buffer[offset++];

            var data = new byte[tagResult.Value.LengthOrValueOrType - 1];
            Array.Copy(context.Buffer, offset, data, 0, data.Length);

            var bitString = new BitString(data, unusedBits);

            return context.ProcessResult(Result.Create(bitString, length), (int) tagResult.Value.LengthOrValueOrType);
        }

        #region OBJECT_LIFE_SAFETY_ZONE
        [Decodes(BacnetPropertyIds.PROP_OPERATION_EXPECTED, BacnetObjectTypes.OBJECT_LIFE_SAFETY_ZONE, typeof(BacnetLifeSafetyOperations))]
        [Decodes(BacnetPropertyIds.PROP_MODE, BacnetObjectTypes.OBJECT_LIFE_SAFETY_ZONE, typeof(BacnetLifeSafetyModes))]
        [Decodes(BacnetPropertyIds.PROP_PRESENT_VALUE, BacnetObjectTypes.OBJECT_LIFE_SAFETY_ZONE, typeof(BacnetLifeSafetyStates))]
        [Decodes(BacnetPropertyIds.PROP_TRACKING_VALUE, BacnetObjectTypes.OBJECT_LIFE_SAFETY_ZONE, typeof(BacnetLifeSafetyStates))]
        [DecodesSequenceItem(BacnetPropertyIds.PROP_LIFE_SAFETY_ALARM_VALUES, BacnetObjectTypes.OBJECT_LIFE_SAFETY_ZONE, typeof(BacnetLifeSafetyStates))]
        [DecodesSequenceItem(BacnetPropertyIds.PROP_ALARM_VALUES, BacnetObjectTypes.OBJECT_LIFE_SAFETY_ZONE, typeof(BacnetLifeSafetyStates))]
        [DecodesSequenceItem(BacnetPropertyIds.PROP_FAULT_VALUES, BacnetObjectTypes.OBJECT_LIFE_SAFETY_ZONE, typeof(BacnetLifeSafetyStates))]
        [DecodesSequenceItem(BacnetPropertyIds.PROP_ACCEPTED_MODES, BacnetObjectTypes.OBJECT_LIFE_SAFETY_ZONE, typeof(BacnetLifeSafetyModes))]
        #endregion
        #region OBJECT_LIFE_SAFETY_POINT
        [Decodes(BacnetPropertyIds.PROP_OPERATION_EXPECTED, BacnetObjectTypes.OBJECT_LIFE_SAFETY_POINT, typeof(BacnetLifeSafetyOperations))]
        [Decodes(BacnetPropertyIds.PROP_MODE, BacnetObjectTypes.OBJECT_LIFE_SAFETY_POINT, typeof(BacnetLifeSafetyModes))]
        [Decodes(BacnetPropertyIds.PROP_PRESENT_VALUE, BacnetObjectTypes.OBJECT_LIFE_SAFETY_POINT, typeof(BacnetLifeSafetyStates))]
        [Decodes(BacnetPropertyIds.PROP_TRACKING_VALUE, BacnetObjectTypes.OBJECT_LIFE_SAFETY_POINT, typeof(BacnetLifeSafetyStates))]
        [Decodes(BacnetPropertyIds.PROP_MAINTENANCE_REQUIRED, BacnetObjectTypes.OBJECT_LIFE_SAFETY_POINT, typeof(BacnetMaintenance))]
        [DecodesSequenceItem(BacnetPropertyIds.PROP_LIFE_SAFETY_ALARM_VALUES, BacnetObjectTypes.OBJECT_LIFE_SAFETY_POINT, typeof(BacnetLifeSafetyStates))]
        [DecodesSequenceItem(BacnetPropertyIds.PROP_ALARM_VALUES, BacnetObjectTypes.OBJECT_LIFE_SAFETY_POINT, typeof(BacnetLifeSafetyStates))]
        [DecodesSequenceItem(BacnetPropertyIds.PROP_FAULT_VALUES, BacnetObjectTypes.OBJECT_LIFE_SAFETY_POINT, typeof(BacnetLifeSafetyStates))]
        [DecodesSequenceItem(BacnetPropertyIds.PROP_ACCEPTED_MODES, BacnetObjectTypes.OBJECT_LIFE_SAFETY_POINT, typeof(BacnetLifeSafetyModes))]
        #endregion
        [Decodes(BacnetPropertyIds.PROP_NODE_TYPE, typeof(BacnetNodeTypes))]
        [Decodes(BacnetPropertyIds.PROP_FILE_ACCESS_METHOD, typeof(BacnetFileAccessMethod))]
        [Decodes(BacnetPropertyIds.PROP_RELIABILITY, typeof(BacnetReliability))]
        [Decodes(BacnetPropertyIds.PROP_OBJECT_TYPE, typeof(BacnetObjectTypes))]
        [Decodes(BacnetPropertyIds.PROP_EVENT_STATE, typeof(BacnetEventStates))]
        [Decodes(BacnetPropertyIds.PROP_SILENCED, typeof(BacnetSilencedState))]
        [Decodes(BacnetPropertyIds.PROP_NOTIFY_TYPE, typeof(BacnetNotifyTypes))]
        [Decodes(BacnetPropertyIds.PROP_SYSTEM_STATUS, typeof(BacnetDeviceStatus))]
        [Decodes(BacnetPropertyIds.PROP_SEGMENTATION_SUPPORTED, typeof(BacnetSegmentations))]
        [Decodes(BacnetPropertyIds.PROP_LAST_RESTART_REASON, typeof(BacnetRestartReason))]
        [Decodes(BacnetPropertyIds.PROP_UNITS, typeof(BacnetEngineeringUnits))]
        public virtual Result<T> DecodeEnumerated<T>(Context context, byte? expectedTag = null,
            bool required = true) where T : struct, IComparable
        {
            var tagResult = DecodeTag(context, 9, expectedTag, required);
            if (tagResult == null)
                return null;

            var uintResult = _numericDecoder.DecodeNumber<uint>(
                context.Buffer, context.Offset, (int) tagResult.Value.LengthOrValueOrType,
                sizeof(uint));

            return context.ProcessResult(
                Result.Create(
                    (T) (dynamic) uintResult.Value, // dynamic is faster than Enum.ToObject()
                    (int) (tagResult.Length + tagResult.Value.LengthOrValueOrType)),
                (int) tagResult.Value.LengthOrValueOrType);
        }

        [Decodes(BacnetPropertyIds.PROP_LOCAL_DATE)]
        public virtual Result<Date> DecodeDate(Context context, byte? expectedTag = null,
            bool required = true)
        {
            var tagResult = DecodeTag(context, 10, expectedTag, required);
            if (tagResult == null)
                return null;

            var offset = context.Offset;

            var dateResult = new Date(
                context.Buffer[offset], context.Buffer[offset + 1], context.Buffer[offset + 2],
                context.Buffer[offset + 3]);

            return context.ProcessResult(
                Result.Create(dateResult, (int) (tagResult.Length + tagResult.Value.LengthOrValueOrType)),
                (int) tagResult.Value.LengthOrValueOrType);
        }

        [Decodes(BacnetPropertyIds.PROP_LOCAL_TIME)]
        public virtual Result<Time> DecodeTime(Context context, byte? expectedTag = null,
            bool required = true)
        {
            var tagResult = DecodeTag(context, 11, expectedTag, required);
            if (tagResult == null)
                return null;

            var timeResult = new Time(
                context.Buffer[context.Offset], context.Buffer[context.Offset + 1], context.Buffer[context.Offset + 2],
                context.Buffer[context.Offset + 3]);

            return context.ProcessResult(
                Result.Create(timeResult, (int) (tagResult.Length + tagResult.Value.LengthOrValueOrType)),
                (int)tagResult.Value.LengthOrValueOrType);
        }

        [Decodes(BacnetPropertyIds.PROP_OBJECT_IDENTIFIER)]
        [DecodesArrayItem(BacnetPropertyIds.PROP_OBJECT_LIST)]
        [DecodesArrayItem(BacnetPropertyIds.PROP_STRUCTURED_OBJECT_LIST)]
        public virtual Result<BacnetObjectId> DecodeObjectIdentifier(Context context,
            byte? expectedTag = null, bool required = true)
        {
            var tagResult = DecodeTag(context, 12, expectedTag, required);
            if (tagResult == null)
                return null;

            var octets = new byte[tagResult.Value.LengthOrValueOrType];
            for (var i = 0; i < octets.Length; i++)
                octets[i] = context.Buffer[context.Offset + 3 - i];

            var rawData = BitConverter.ToUInt32(octets, 0);
            var oidResult = new BacnetObjectId((BacnetObjectTypes) (rawData >> 22), rawData & 0b1111111111111111111111);

            return context.ProcessResult(
                Result.Create(oidResult, (int) (tagResult.Length + tagResult.Value.LengthOrValueOrType)),
                (int)tagResult.Value.LengthOrValueOrType);
        }
    }
}