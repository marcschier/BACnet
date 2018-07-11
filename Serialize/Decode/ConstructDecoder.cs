using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System.IO.BACnet.Serialize.Decode
{
    using HandlerLookupKey = Tuple<BacnetPropertyIds, BacnetObjectTypes?>;

    public class ConstructDecoder
    {
        public static ConstructDecoder Standard = new ConstructDecoder(PrimitiveDecoder.Standard);

        protected readonly PrimitiveDecoder PrimitiveDecoder;

        private readonly Dictionary<HandlerLookupKey, DecodingFunctionDelegate> _decodingFunctions;

        public ConstructDecoder(PrimitiveDecoder primitiveDecoder)
        {
            PrimitiveDecoder = primitiveDecoder;

            var itemDecodingFunctions = GetItemDecodingFunctions();            
            var sequenceOfDecodingFunctions = GetSequenceOfDecodingFunctions();
            var arrayDecodingFunctions = GetArrayDecodingFunctions();

            _decodingFunctions = itemDecodingFunctions
                .Concat(sequenceOfDecodingFunctions)
                .Concat(arrayDecodingFunctions)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        #region Reflection
        private class ReflectionInfo<T> where T: DecodesAttribute
        {
            public object Object { get; set; }
            public MethodInfo MethodInfo { get; set; }
            public T Attribute { get; set; }
        }

        private Dictionary<HandlerLookupKey, DecodingFunctionDelegate> GetDecodingFunctions<T>(
            Func<ReflectionInfo<T>, DecodingFunctionDelegate> delegateFactory) where T : DecodesAttribute
        {
            return new object[] {PrimitiveDecoder, this}
                .SelectMany(
                    o => o.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance),
                    (o, mi) => new
                    {
                        Object = o,
                        MethodInfo = mi,
                        Attributes = mi.GetCustomAttributes<T>().Where(a => a.GetType() == typeof(T))
                    })
                .Where(e => e.Attributes != null)
                .SelectMany(
                    e => e.Attributes,
                    (e, attr) => new ReflectionInfo<T> {Object = e.Object, MethodInfo = e.MethodInfo, Attribute = attr})
                .ToDictionary(e => new HandlerLookupKey(e.Attribute.Property, e.Attribute.ObjectType), delegateFactory);
        }

        private Dictionary<HandlerLookupKey, DecodingFunctionDelegate> GetSequenceOfDecodingFunctions()
        {
            var sequenceDecoder = GetType().GetMethod(nameof(DecodeSequenceOf)) ?? throw new ArgumentNullException();

            return GetDecodingFunctions<DecodesSequenceItemAttribute>(
                e =>
                {
                    var itemMethodInfo = e.MethodInfo.IsGenericMethod
                        ? e.MethodInfo.MakeGenericMethod(e.Attribute.EnumType)
                        : e.MethodInfo;

                    var returnType = itemMethodInfo.ReturnType.GenericTypeArguments;

                    var sequenceFunc = sequenceDecoder.MakeGenericMethod(returnType);
                    var itemFunc = Delegate.CreateDelegate(
                        typeof(DecodingFunctionDelegate<>).MakeGenericType(returnType), e.Object, itemMethodInfo);

                    return ((context, tag, required) => (ResultBase) sequenceFunc.Invoke(
                        this, new object[] {context, itemFunc, tag}));
                });
        }

        private Dictionary<HandlerLookupKey, DecodingFunctionDelegate> GetArrayDecodingFunctions()
        {
            var arrayDecoder = GetType().GetMethod(nameof(DecodeArray)) ?? throw new ArgumentNullException();

            return GetDecodingFunctions<DecodesArrayItemAttribute>(
                e =>
                {
                    var returnType = e.MethodInfo.ReturnType.GenericTypeArguments;
                    var arrayFunc = arrayDecoder.MakeGenericMethod(returnType);
                    var itemFunc = Delegate.CreateDelegate(
                        typeof(DecodingFunctionDelegate<>).MakeGenericType(returnType), e.Object, e.MethodInfo);

                    return ((context, tag, required) => (ResultBase) arrayFunc.Invoke(
                        this, new object[] {context, itemFunc, tag, e.Attribute.ExpectedCount}));
                });
        }

        private Dictionary<HandlerLookupKey, DecodingFunctionDelegate> GetItemDecodingFunctions()
            => GetDecodingFunctions<DecodesAttribute>(
                e => (DecodingFunctionDelegate) Delegate.CreateDelegate(
                    typeof(DecodingFunctionDelegate), e.Object,
                    e.MethodInfo.IsGenericMethod
                        ? e.MethodInfo.MakeGenericMethod(e.Attribute.EnumType)
                        : e.MethodInfo));
        #endregion

        /// <summary>
        /// Wraps calls to decode a sequence (20.2.16)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="method"></param>
        /// <param name="expectedContextTag">if specified, we expect opening / closing tags</param>
        /// <param name="required">if <see langword="true"/>, throws if the expected context-tag was not found.
        ///     if <see langword="false"/>, returns <see langword="null"/> if the expected context-tag was not found.
        /// </param>
        /// <returns></returns>
        public Result<T> DecodeSequence<T>(Context context, Func<T> method, byte? expectedContextTag = null,
            bool required = true)
        {
            var startOffset = context.Offset;

            if (expectedContextTag.HasValue)
            {
                var openingTag = PrimitiveDecoder.DecodeTag(
                    context, Class.OPENING, expectedContextTag.Value, required);

                if (openingTag == null)
                    return null;

                if (!required)
                {
                    var peekedTag = PrimitiveDecoder.DecodeTag(
                        context, Class.CLOSING, expectedContextTag.Value, false, true);

                    if (peekedTag != null)
                    {
                        context.ProcessResult(peekedTag);
                        return null;
                    }
                }
            }

            var result = method.Invoke();

            if (expectedContextTag.HasValue)
            {
                PrimitiveDecoder.DecodeTag(
                    context, Class.CLOSING, expectedContextTag.Value);
                // required = true because if .HasValue && !required and we reached this point, there MUST be a closing tag
            }

            return context.ProcessResult(Result.Create(result, context.Offset - startOffset), 0);
        }

        public virtual Result<IList<T>> DecodeSequenceOf<T>(Context context, DecodingFunctionDelegate<T> method, byte? contextTag = null)
        {
            return DecodeSequence(context, () =>
            {
                IList<T> results = new List<T>();

                bool IsEndOfSequence()
                {
                    return context.Offset >= context.Buffer.Length
                           || PrimitiveDecoder.DecodeTag(context, Class.CLOSING, contextTag, false, true) != null;
                }

                while (!IsEndOfSequence())
                {
                    results.Add(method.Invoke(context, required: false).Value);
                }

                return results;
            }, contextTag, false) ?? new Result<IList<T>>(new List<T>(), 0);
        }

        public Result<IList<T>> DecodeArray<T>(Context context, DecodingFunctionDelegate<T> method, byte? contextTag = null, int? expectedCount = null)
        {
            var result = DecodeSequenceOf(context, method, contextTag);

            if(expectedCount.HasValue && result.Value.Count != expectedCount.Value)
                throw new DecodingException($"Expected {expectedCount.Value} items, but got {result.Value.Count}");

            return result;
        }

        public virtual Result<DateTime> DecodeDateTime(Context context, byte? contextTag = null,
            bool required = true)
        {
            return DecodeSequence(
                context, () =>
                {
                    var date = PrimitiveDecoder.DecodeDate(context);
                    var time = PrimitiveDecoder.DecodeTime(context);
                    return new DateTime(date.Value, time.Value);
                }, contextTag, required);
        }

        [Decodes(BacnetPropertyIds.PROP_TIME_OF_DEVICE_RESTART)]
        [Decodes(BacnetPropertyIds.PROP_LAST_RESTORE_TIME)]
        [DecodesArrayItem(BacnetPropertyIds.PROP_EVENT_TIME_STAMPS, 3)]
        public virtual Result<TimeStamp> DecodeTimeStamp(Context context, byte? contextTag = null,
            bool required = true)
        {
            return DecodeSequence(
                context, () =>
                {
                    var tagResult = PrimitiveDecoder.DecodeTag(context.Buffer, context.Offset);
                    ResultBase result;

                    switch (tagResult.Value.Number)
                    {
                        case 0:
                            result = PrimitiveDecoder.DecodeTime(context, tagResult.Value.Number);
                            break;

                        case 1: // sequencenumber
                            throw new NotImplementedException(); // TODO: support for sequencenumber

                        case 2:
                            result = DecodeDateTime(context, tagResult.Value.Number);
                            break;

                        default:
                            throw new DecodingException(
                                $"Invalid CHOICE for {nameof(TimeStamp)}: '{tagResult.Value.Number}'");
                    }

                    return (TimeStamp) result.ValueAsObject;
                }, contextTag, required);
        }

        public virtual Result<BacnetPropertyReference> DecodePropertyReference(Context context,
            byte? contextTag = null, bool required = true)
        {
            return DecodeSequence(
                context, () =>
                {
                    var propId = PrimitiveDecoder.DecodeEnumerated<BacnetPropertyIds>(context, 0);
                    var arrayIndex = PrimitiveDecoder.DecodeUInt(context, 1, false);
                    return new BacnetPropertyReference(propId.Value, arrayIndex?.Value ?? ASN1.BACNET_ARRAY_ALL);
                }, contextTag, required);
        }

        public virtual Result<BacnetReadAccessSpecification> DecodeReadAccessSpecification(Context context,
            byte? contextTag = null, bool required = true)
        {
            return DecodeSequence(context, () =>
            {
                var oid = PrimitiveDecoder.DecodeObjectIdentifier(context, 0);
                var propRefs = DecodeSequenceOf(context, DecodePropertyReference, 1);

                return new BacnetReadAccessSpecification(oid.Value, propRefs.Value);
            }, contextTag, required);
        }

        public virtual Result<BacnetError> DecodeError(Context context,
            byte? contextTag = null, bool required = true)
        {
            return DecodeSequence(
                context, () =>
                {
                    var errorClass = PrimitiveDecoder.DecodeEnumerated<BacnetErrorClasses>(context);
                    var errorCode = PrimitiveDecoder.DecodeEnumerated<BacnetErrorCodes>(context);

                    return new BacnetError(errorClass.Value, errorCode.Value);
                }, contextTag, required);
        }

        [Decodes(BacnetPropertyIds.PROP_MEMBER_OF)]
        [DecodesSequenceItem(BacnetPropertyIds.PROP_SUBORDINATE_LIST)]
        [DecodesSequenceItem(BacnetPropertyIds.PROP_ZONE_MEMBERS)]
        public virtual Result<DeviceObjectReference> DecodeDeviceObjectReference(Context context,
            byte? contextTag = null, bool required = true)
        {
            var deviceId = PrimitiveDecoder.DecodeObjectIdentifier(context, 0, false);
            var objectId = PrimitiveDecoder.DecodeObjectIdentifier(context, 1);

            return Result.Create(new DeviceObjectReference(objectId.Value, deviceId?.Value), deviceId?.Length ?? 0 + objectId.Length);
        }

        public virtual Result<BacnetReadAccessResult> DecodeReadAccessResult(Context context,
            byte? contextTag = null, bool required = true)
        {
            return DecodeSequence(context, () =>
            {
                var oid = PrimitiveDecoder.DecodeObjectIdentifier(context, 0);
                var results = DecodeSequenceOf(context, (ctx, ctxTag, req) => DecodeListOfResults(ctx, oid.Value), 1);

                return new BacnetReadAccessResult(oid.Value, results.Value);
            }, contextTag, required);
        }

        public virtual Result<BacnetAddress> DecodeAddress(Context context,
            byte? contextTag = null, bool required = true)
        {
            return DecodeSequence(context, () =>
            {
                var net = PrimitiveDecoder.DecodeUInt(context);
                var mac = PrimitiveDecoder.DecodeOctetString(context);

                return new BacnetAddress(BacnetAddressTypes.None, (ushort)net.Value, mac.Value);
            }, contextTag, required);
        }

        [Decodes(BacnetPropertyIds.PROP_DEVICE_ADDRESS_BINDING)]
        public virtual Result<AddressBinding> DecodeAddressBinding(Context context,
            byte? contextTag = null, bool required = true)
        {
            return DecodeSequence(context, () =>
            {
                var oid = PrimitiveDecoder.DecodeObjectIdentifier(context);
                var address = DecodeAddress(context);

                return new AddressBinding(oid.Value, address.Value);
            }, contextTag, required);
        }

        [DecodesSequenceItem(BacnetPropertyIds.PROP_RECIPIENT_LIST)]
        public virtual Result<Destination> DecodeDestination(Context context,
            byte? contextTag = null, bool required = true)
            => DecodeSequence(
                context, ()
                    => new Destination(
                        PrimitiveDecoder.DecodeBitString(context).Value, PrimitiveDecoder.DecodeTime(context).Value,
                        PrimitiveDecoder.DecodeTime(context).Value,
                        DecodeRecipient(context).Value,
                        PrimitiveDecoder.DecodeUInt(context).Value,
                        PrimitiveDecoder.DecodeBoolean(context).Value,
                        PrimitiveDecoder.DecodeBitString(context).Value), contextTag, required);


        public virtual Result<Recipient> DecodeRecipient(Context context, byte? contextTag = null, bool required = true)
        {
            return DecodeSequence(
                context, () =>
                {
                    var peekedTag = PrimitiveDecoder.DecodeTag(
                        context, Class.CONTEXTSPECIFIC, required: true, peek: true);

                    switch (peekedTag.Value.Number)
                    {
                        case 0:
                            return new Recipient(PrimitiveDecoder.DecodeObjectIdentifier(context, 0).Value);

                        case 1:
                            return new Recipient(address: DecodeAddress(context, 1).Value);
                    }

                    throw new DecodingException($"Unexpected CHOICE: '{peekedTag.Value.Number}'");
                }, contextTag, required);
        }

        [DecodesSequenceItem(BacnetPropertyIds.PROP_LOG_BUFFER)]
        public virtual Result<BacnetLogRecord> DecodeLogRecord(Context context, byte? contextTag = null, bool required = true)
        {
            return DecodeSequence(
                context, () =>
                {
                    var timeStamp = DecodeDateTime(context, 0).Value;

                    PrimitiveDecoder.DecodeTag(context, Class.OPENING, 1);

                    object logDatum;

                    var choiceTag = PrimitiveDecoder.DecodeTag(context, Class.CONTEXTSPECIFIC, peek:true).Value;

                    switch (choiceTag.Number)
                    {
                        case 0:
                            logDatum = PrimitiveDecoder.DecodeBitString(context, choiceTag.Number).Value;
                            break;

                        case 1:
                            logDatum = PrimitiveDecoder.DecodeBoolean(context, choiceTag.Number).Value;
                            break;

                        case 2:
                            logDatum = PrimitiveDecoder.DecodeReal(context, choiceTag.Number).Value;
                            break;

                        case 3: // enumerated, but enum-type is not known --> uint
                        case 4: // unsigned
                            logDatum = PrimitiveDecoder.DecodeUInt(context, choiceTag.Number).Value;
                            break;

                        case 5:
                            logDatum = PrimitiveDecoder.DecodeInt(context, choiceTag.Number).Value;
                            break;

                        case 6:
                            logDatum = PrimitiveDecoder.DecodeBitString(context, choiceTag.Number).Value;
                            break;

                        case 7:
                            logDatum = PrimitiveDecoder.DecodeNull(context, choiceTag.Number).Value;
                            break;

                        case 8:
                            logDatum = DecodeError(context, choiceTag.Number).Value;
                            break;

                        case 9: // time-change
                            logDatum = PrimitiveDecoder.DecodeReal(context, choiceTag.Number).Value;
                            break;

                        case 10:
                            var tag = PrimitiveDecoder.DecodeTag(context, Class.CONTEXTSPECIFIC, choiceTag.Number).Value;
                            logDatum = context.GetRawData(tag.LengthOrValueOrType);
                            break;

                        default:
                            throw new DecodingException($"Unexpected CHOICE: '{choiceTag.Number}'");
                    }

                    PrimitiveDecoder.DecodeTag(context, Class.CLOSING, 1);

                    var statusFlags = PrimitiveDecoder.DecodeEnumerated<BacnetStatusFlags>(context, 2, required: false)
                                          ?.Value ?? BacnetStatusFlags.NONE;

                    return new BacnetLogRecord(
                        (BacnetTrendLogValueType) choiceTag.Number, logDatum, timeStamp.NativeDateTime ?? default,
                        statusFlags);
                }, contextTag, required);
        }

        // TODO: return-type should not be BacnetPropertyValue!
        private Result<BacnetPropertyValue> DecodeListOfResults(Context context, BacnetObjectId objectId)
        {
            return DecodeSequence(
                context, () =>
                {
                    var propId = PrimitiveDecoder.DecodeEnumerated<BacnetPropertyIds>(context, 2);
                    var arrayIndex = PrimitiveDecoder.DecodeUInt(context, 3, false);

                    var readResult = DecodeSequence(
                        context, () => DecodeAny(context, objectId, propId.Value).ValueAsObject, 4, false);

                    if (readResult != null)
                    {
                        return new BacnetPropertyValue
                        {
                            property = new BacnetPropertyReference(
                                propId.Value, arrayIndex?.Value ?? ASN1.BACNET_ARRAY_ALL),

                            value = readResult.ValueAsObject is IList list
                                ? list.Cast<object>().Select(v => new BacnetValue(v)).ToList()
                                : new List<BacnetValue> {new BacnetValue(readResult.ValueAsObject)}

                        };
                    }

                    var errorResult = DecodeError(context, 5, false);

                    return new BacnetPropertyValue()
                    {
                        property = new BacnetPropertyReference(
                            propId.Value, arrayIndex?.Value ?? ASN1.BACNET_ARRAY_ALL),

                        value = new List<BacnetValue> {new BacnetValue(errorResult?.Value)}
                    };
                });
        }

        public virtual ResultBase DecodeAny(Context context, BacnetObjectId objectId, BacnetPropertyIds propertyId,
            byte? contextTag = null, bool required = true) => DecodeSequence(
            context, () =>
            {
                if (_decodingFunctions.TryGetValue(new HandlerLookupKey(propertyId, objectId.Type), out var func))
                    return func.Invoke(context).ValueAsObject;

                if (_decodingFunctions.TryGetValue(new HandlerLookupKey(propertyId, null), out func))
                    return func.Invoke(context).ValueAsObject;

                throw new NotImplementedException(
                    $"Property '{propertyId}' (for object '{objectId}') is not implemented");
            }, contextTag, required);

        public virtual Result<EventSummary> DecodeEventSummary(Context context, byte? contextTag = null,
            bool required = true)
            => DecodeSequence(
                context, ()
                    => new EventSummary(
                        PrimitiveDecoder.DecodeObjectIdentifier(context, 0).Value,
                        PrimitiveDecoder.DecodeEnumerated<BacnetEventStates>(context, 1).Value,
                        PrimitiveDecoder.DecodeBitString(context, 2).Value,
                        DecodeArray(context, DecodeTimeStamp, 3, 3).Value.ToArray(),
                        PrimitiveDecoder.DecodeEnumerated<BacnetNotifyTypes>(context, 4).Value,
                        PrimitiveDecoder.DecodeBitString(context, 5).Value,
                        DecodeArray(context, PrimitiveDecoder.DecodeUInt, 6, 3).Value.ToArray()
                    ), contextTag, required);
    }
}
