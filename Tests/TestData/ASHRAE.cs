using System.Collections.Generic;
using System.IO.BACnet.EventNotification;
using System.IO.BACnet.EventNotification.EventValues;
using System.IO.BACnet.Serialize;
using System.IO.BACnet.Serialize.Decode;
using System.Linq;
using System.Text;
using static System.IO.BACnet.Tests.Helper;

namespace System.IO.BACnet.Tests.TestData
{
    public static class ASHRAE
    {
        public static (uint SubscriberProcessIdentifier, uint InitiatingDeviceIdentifier, BacnetObjectId
            MonitoredObjectIdentifier, uint TimeRemaining, BacnetPropertyValue[] Values)
            F_1_2()
        {
            var data = new[]
            {
                new BacnetPropertyValue
                {
                    property = new BacnetPropertyReference((uint) BacnetPropertyIds.PROP_PRESENT_VALUE),
                    value = new List<BacnetValue> {new BacnetValue(65.0f)}
                },
                new BacnetPropertyValue
                {
                    property = new BacnetPropertyReference((uint) BacnetPropertyIds.PROP_STATUS_FLAGS),
                    value = new List<BacnetValue> {new BacnetValue(BacnetBitString.Parse("0000"))}
                }
            };

            return (18, 4, new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_INPUT, 10), 0, data);
        }

        public static (uint SubscriberProcessIdentifier, uint InitiatingDeviceIdentifier, BacnetObjectId
            MonitoredObjectIdentifier, uint TimeRemaining, BacnetPropertyValue[] Values)
            F_1_3() => F_1_2();

        public static StateTransition<OutOfRange> F_1_4()
        {
            return new StateTransition<OutOfRange>(new OutOfRange()
            {
                ExceedingValue = 80.1f,
                StatusFlags = BacnetBitString.Parse("1000"),
                Deadband = 1.0f,
                ExceededLimit = 80.0f
            })
            {
                ProcessIdentifier = 1,
                InitiatingObjectIdentifier = new BacnetObjectId(BacnetObjectTypes.OBJECT_DEVICE, 4),
                EventObjectIdentifier = new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_INPUT, 2),
                TimeStamp = new BacnetGenericTime(default(DateTime), BacnetTimestampTags.TIME_STAMP_SEQUENCE, 16),
                NotificationClass = 4,
                Priority = 100,
                NotifyType = BacnetNotifyTypes.NOTIFY_ALARM,
                AckRequired = true,
                FromState = BacnetEventStates.EVENT_STATE_NORMAL,
                ToState = BacnetEventStates.EVENT_STATE_HIGH_LIMIT
            };
        }

        public static BacnetAlarmSummaryData[]
            F_1_6()
        {
            return new[]
            {
                new BacnetAlarmSummaryData(new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_INPUT, 2),
                    BacnetEventStates.EVENT_STATE_HIGH_LIMIT, BacnetBitString.Parse("011")),
                new BacnetAlarmSummaryData(new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_INPUT, 3),
                    BacnetEventStates.EVENT_STATE_LOW_LIMIT, BacnetBitString.Parse("111"))
            };
        }

        public static (BacnetLogRecord Record1, BacnetLogRecord Record2, BacnetObjectId ObjectId, BacnetPropertyIds
            PropertyId, BacnetBitString Flags, uint ItemCount, BacnetReadRangeRequestTypes RequestType, uint
            FirstSequence
            ) F_3_8_Ack()
        {
            var record1 = new BacnetLogRecord(BacnetTrendLogValueType.TL_TYPE_REAL, 18.0,
                new DateTime(1998, 3, 23, 19, 54, 27), 0);
            var record2 = new BacnetLogRecord(BacnetTrendLogValueType.TL_TYPE_REAL, 18.1,
                new DateTime(1998, 3, 23, 19, 56, 27), 0);

            return (record1, record2, new BacnetObjectId(BacnetObjectTypes.OBJECT_TRENDLOG, 1),
                BacnetPropertyIds.PROP_LOG_BUFFER, BacnetBitString.Parse("110"), 2,
                BacnetReadRangeRequestTypes.RR_BY_SEQUENCE, 79201);
        }

        public static (BacnetGetEventInformationData[] Data, bool MoreEvents) F_1_8()
        {
            return (new[]
            {
                new BacnetGetEventInformationData()
                {
                    objectIdentifier = new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_INPUT, 2),
                    eventState = BacnetEventStates.EVENT_STATE_HIGH_LIMIT,
                    acknowledgedTransitions = BacnetBitString.Parse("011"),
                    eventTimeStamps = new[]
                    {
                        new BacnetGenericTime(new DateTime(1, 1, 1, 15, 35, 00).AddMilliseconds(200),
                            BacnetTimestampTags.TIME_STAMP_TIME),
                        new BacnetGenericTime(default(DateTime), BacnetTimestampTags.TIME_STAMP_TIME),
                        new BacnetGenericTime(default(DateTime), BacnetTimestampTags.TIME_STAMP_TIME),
                    },
                    notifyType = BacnetNotifyTypes.NOTIFY_ALARM,
                    eventEnable = BacnetBitString.Parse("111"),
                    eventPriorities = new uint[] {15, 15, 20}
                },
                new BacnetGetEventInformationData()
                {
                    objectIdentifier = new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_INPUT, 3),
                    eventState = BacnetEventStates.EVENT_STATE_NORMAL,
                    acknowledgedTransitions = BacnetBitString.Parse("110"),
                    eventTimeStamps = new[]
                    {
                        new BacnetGenericTime(new DateTime(1, 1, 1, 15, 40, 00), BacnetTimestampTags.TIME_STAMP_TIME),
                        new BacnetGenericTime(default(DateTime), BacnetTimestampTags.TIME_STAMP_TIME),
                        new BacnetGenericTime(new DateTime(1, 1, 1, 15, 45, 30).AddMilliseconds(300),
                            BacnetTimestampTags.TIME_STAMP_TIME),
                    },
                    notifyType = BacnetNotifyTypes.NOTIFY_ALARM,
                    eventEnable = BacnetBitString.Parse("111"),
                    eventPriorities = new uint[] {15, 15, 20}
                }
            }, false);
        }

        public static (EventInformation Object, byte[] EncodedBytes) F_1_8_PayloadOnly()
        {
            return (new EventInformation(new[]
                {
                    new EventSummary(
                        new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_INPUT, 2),
                        BacnetEventStates.EVENT_STATE_HIGH_LIMIT,
                        BitString.Parse("011"),
                        new TimeStamp[]
                        {
                            new Time(15, 35, 00, 20),
                            Time.Any,
                            Time.Any,
                        },
                        BacnetNotifyTypes.NOTIFY_ALARM,
                        BitString.Parse("111"),
                        new uint[] {15, 15, 20}
                    ),
                    new EventSummary
                    (
                        new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_INPUT, 3),
                        BacnetEventStates.EVENT_STATE_NORMAL,
                        BitString.Parse("110"),
                        new TimeStamp[]
                        {
                            new Time(15, 40, 0, 0),
                            Time.Any,
                            new Time(15, 45, 30, 30),
                        },
                        BacnetNotifyTypes.NOTIFY_ALARM,
                        BitString.Parse("111"),
                        new uint[] {15, 15, 20}
                    )
                }, false),

                // example taken from ANNEX F - Examples of APDU Encoding - F.1.8
                new byte[]
                {
                    0x0E, 0x0C, 0x00, 0x00, 0x00, 0x02, 0x19, 0x03, 0x2A, 0x05, 0x60, 0x3E, 0x0C,
                    0x0F, 0x23, 0x00, 0x14, 0x0C, 0xFF, 0xFF, 0xFF, 0xFF, 0x0C, 0xFF, 0xFF, 0xFF, 0xFF, 0x3F, 0x49,
                    0x00, 0x5A, 0x05, 0xE0, 0x6E, 0x21, 0x0F, 0x21, 0x0F, 0x21, 0x14, 0x6F, 0x0C, 0x00, 0x00, 0x00,
                    0x03, 0x19, 0x00, 0x2A, 0x05, 0xC0, 0x3E, 0x0C, 0x0F, 0x28, 0x00, 0x00, 0x0C, 0xFF, 0xFF, 0xFF,
                    0xFF, 0x0C, 0x0F, 0x2D, 0x1E, 0x1E, 0x3F, 0x49, 0x00, 0x5A, 0x05, 0xE0, 0x6E, 0x21, 0x0F, 0x21,
                    0x0F, 0x21, 0x14, 0x6F, 0x0F, 0x19, 0x00
                });
        }

        public static (uint SubscriberProcessIdentifier, BacnetObjectId MonitoredObjectIdentifier, bool
            CancellationRequest, bool IssueConfirmedNotifications, uint Lifetime)
            F_1_10()
        {
            return (18, new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_INPUT, 10),
                false, true, 0);
        }

        public static (uint SubscriberProcessIdentifier, BacnetObjectId MonitoredObjectIdentifier, bool
            CancellationRequest, bool IssueConfirmedNotifications, uint Lifetime, BacnetPropertyReference
            MonitoredProperty, bool CovIncrementPresent, float CovIncrement)
            F_1_11()
        {
            return (18, new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_INPUT, 10), false, true, 60,
                new BacnetPropertyReference((uint) BacnetPropertyIds.PROP_PRESENT_VALUE), true, 1.0f);
        }

        public static (bool IsStream, bool EndOfFile, int Position, uint BlockCount, byte[][] Blocks, int[] Counts)
            F_2_1()
        {
            var data = new[]
            {
                Encoding.ASCII.GetBytes("Chiller01 On-Time=4.3 Hours")
            };

            return (true, false, 0, 1, data, data.Select(arr => arr.Length).ToArray());
        }

        public static (BacnetObjectTypes ObjectType, ICollection<BacnetPropertyValue> ValueList)
            F_3_3()
        {
            var data = new List<BacnetPropertyValue>
            {
                new BacnetPropertyValue
                {
                    property = new BacnetPropertyReference(BacnetPropertyIds.PROP_OBJECT_NAME),
                    value = new List<BacnetValue> {new BacnetValue("Trend 1")}
                },
                new BacnetPropertyValue
                {
                    property = new BacnetPropertyReference(BacnetPropertyIds.PROP_FILE_ACCESS_METHOD),
                    value = new List<BacnetValue> {new BacnetValue(BacnetFileAccessMethod.RECORD_ACCESS)}
                }
            };

            return (BacnetObjectTypes.OBJECT_FILE, data);
        }

        public static BacnetObjectId F_3_4()
            => new BacnetObjectId(BacnetObjectTypes.OBJECT_GROUP, 6);

        public static (BacnetObjectId ObjectId, BacnetPropertyIds PropertyId, uint ArrayIndex)
            F_3_5()
            => (new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_INPUT, 5), BacnetPropertyIds.PROP_PRESENT_VALUE,
                ASN1.BACNET_ARRAY_ALL);

        public static (BacnetObjectId ObjectId, BacnetPropertyIds PropertyId, IEnumerable<BacnetValue> ValueList, uint
            ArrayIndex)
            F_3_5_Ack()
            => (new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_INPUT, 5), BacnetPropertyIds.PROP_PRESENT_VALUE,
                new List<BacnetValue>
                {
                    new BacnetValue(72.3f)
                }, ASN1.BACNET_ARRAY_ALL);

        public static (BacnetObjectId ObjectId, IList<BacnetPropertyReference> Properties)
            F_3_7()
            => (new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_INPUT, 16), new List<BacnetPropertyReference>
            {
                new BacnetPropertyReference(BacnetPropertyIds.PROP_PRESENT_VALUE),
                new BacnetPropertyReference(BacnetPropertyIds.PROP_RELIABILITY)
            });

        public static IList<BacnetReadAccessResult> F_3_7_Ack()
            => new List<BacnetReadAccessResult>
            {
                new BacnetReadAccessResult(new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_INPUT, 16),
                    new List<BacnetPropertyValue>
                    {
                        new BacnetPropertyValue
                        {
                            property = new BacnetPropertyReference(BacnetPropertyIds.PROP_PRESENT_VALUE),
                            value = new List<BacnetValue> {new BacnetValue(72.3f)},
                        },
                        new BacnetPropertyValue
                        {
                            property = new BacnetPropertyReference(BacnetPropertyIds.PROP_RELIABILITY),
                            value = new List<BacnetValue>
                            {
                                new BacnetValue(BacnetReliability.RELIABILITY_NO_FAULT_DETECTED)
                            },
                        }
                    })
            };

        public static (List<BacnetReadAccessSpecification> Input, byte[] ExpectedBytes)
            F_3_7_Multiple()
            => (new List<BacnetReadAccessSpecification>
                {
                    new BacnetReadAccessSpecification(new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_INPUT, 33),
                        new List<BacnetPropertyReference>
                        {
                            new BacnetPropertyReference(BacnetPropertyIds.PROP_PRESENT_VALUE)
                        }),
                    new BacnetReadAccessSpecification(new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_INPUT, 50),
                        new List<BacnetPropertyReference>
                        {
                            new BacnetPropertyReference(BacnetPropertyIds.PROP_PRESENT_VALUE)
                        }),
                    new BacnetReadAccessSpecification(new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_INPUT, 35),
                        new List<BacnetPropertyReference>
                        {
                            new BacnetPropertyReference(BacnetPropertyIds.PROP_PRESENT_VALUE)
                        })
                }, // example taken from ANNEX F - Examples of APDU Encoding - F.3.7
                new byte[]
                {
                    0x00, 0x04, 0x02, 0x0E, 0x0C, 0x00, 0x00, 0x00, 0x21, 0x1E, 0x09, 0x55, 0x1F, 0x0C, 0x00, 0x00,
                    0x00, 0x32, 0x1E, 0x09, 0x55, 0x1F, 0x0C, 0x00, 0x00, 0x00, 0x23, 0x1E, 0x09, 0x55, 0x1F
                });

        public static (IList<BacnetReadAccessResult> Input, byte[] ExpectedBytes) F_3_7_Multiple_Ack()
            => (new List<BacnetReadAccessResult>
                {
                    new BacnetReadAccessResult(new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_INPUT, 33),
                        new List<BacnetPropertyValue>
                        {
                            new BacnetPropertyValue
                            {
                                property = new BacnetPropertyReference(BacnetPropertyIds.PROP_PRESENT_VALUE),
                                value = new List<BacnetValue> {new BacnetValue(42.3f)},
                            }
                        }),

                    new BacnetReadAccessResult(new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_INPUT, 50),
                        new List<BacnetPropertyValue>
                        {
                            new BacnetPropertyValue
                            {
                                property = new BacnetPropertyReference(BacnetPropertyIds.PROP_PRESENT_VALUE),
                                value = new List<BacnetValue>
                                {
                                    new BacnetValue(new BacnetError(BacnetErrorClasses.ERROR_CLASS_OBJECT,
                                        BacnetErrorCodes.ERROR_CODE_UNKNOWN_OBJECT))
                                },
                            }
                        }),

                    new BacnetReadAccessResult(new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_INPUT, 35),
                        new List<BacnetPropertyValue>
                        {
                            new BacnetPropertyValue
                            {
                                property = new BacnetPropertyReference(BacnetPropertyIds.PROP_PRESENT_VALUE),
                                value = new List<BacnetValue> {new BacnetValue(435.7f)},
                            }
                        })
                },
                // example taken from ANNEX F - Examples of APDU Encoding - F.3.7
                new byte[]
                {
                    0x30, 0x02, 0x0E, 0x0C, 0x00, 0x00, 0x00, 0x21, 0x1E, 0x29, 0x55, 0x4E, 0x44, 0x42, 0x29, 0x33,
                    0x33, 0x4F, 0x1F, 0x0C, 0x00, 0x00, 0x00, 0x32, 0x1E, 0x29, 0x55, 0x5E, 0x91, 0x01, 0x91, 0x1F,
                    0x5F, 0x1F, 0x0C, 0x00, 0x00, 0x00, 0x23, 0x1E, 0x29, 0x55, 0x4E, 0x44, 0x43, 0xD9, 0xD9, 0x9A,
                    0x4F, 0x1F
                });

        public static (BacnetObjectId ObjectId, BacnetPropertyIds PropertyId, BacnetReadRangeRequestTypes RequestType,
            uint Position, DateTime Time, int Count, uint ArrayIndex)
            F_3_8()
            => (new BacnetObjectId(BacnetObjectTypes.OBJECT_TRENDLOG, 1),
                BacnetPropertyIds.PROP_LOG_BUFFER, BacnetReadRangeRequestTypes.RR_BY_TIME, 0,
                new DateTime(1998, 3, 23, 19, 52, 34), 4, ASN1.BACNET_ARRAY_ALL);

        public static (BacnetObjectId ObjectId, BacnetPropertyIds PropertyId, IEnumerable<BacnetValue> ValueList, uint
            ArrayIndex, uint Priority)
            F_3_9()
            => (new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_VALUE, 1), BacnetPropertyIds.PROP_PRESENT_VALUE,
                new List<BacnetValue> {new BacnetValue(180f)}, ASN1.BACNET_ARRAY_ALL, 0);

        public static BacnetWriteAccessSpecification[] F_3_10()
            => new[]
            {
                new BacnetWriteAccessSpecification(
                    new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_VALUE, 5),
                    A(new BacnetWriteAccessSpecification.Property(BacnetPropertyIds.PROP_PRESENT_VALUE,
                        new BacnetValue(67f)))),

                new BacnetWriteAccessSpecification(
                    new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_VALUE, 6),
                    A(new BacnetWriteAccessSpecification.Property(BacnetPropertyIds.PROP_PRESENT_VALUE,
                        new BacnetValue(67f)))),

                new BacnetWriteAccessSpecification(
                    new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_VALUE, 7),
                    A(new BacnetWriteAccessSpecification.Property(BacnetPropertyIds.PROP_PRESENT_VALUE,
                        new BacnetValue(72f)))),
            };

        public static (uint TimeDuration, EnableDisable EnableDisable, string Password)
            F_4_1()
            => (5, EnableDisable.DISABLE, "#egbdf!");

        public static (BacnetReinitializedStates State, string Password)
            F_4_4()
            => (BacnetReinitializedStates.BACNET_REINIT_WARMSTART, "AbCdEfGh");

        public static DateTime F_4_7()
            => new DateTime(1992, 11, 17, 22, 45, 30).AddMilliseconds(700);

        public static string F_4_8_Name()
            => "OATemp";

        public static BacnetObjectId F_4_8_Id()
            => new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_INPUT, 3);

        public static (int LowLimit, int HighLimit) F_4_9()
            => (3, 3);

        public static string GenerateCode()
        {
            return Doc2Code(@"
X'00' PDU Type=0 (BACnet-Confirmed-Request-PDU, SEG=0, MOR=0, SA=0)
X'04' Maximum APDU Size Accepted=1024 octets
X'02' Invoke ID=2
X'0E' Service Choice=14 (ReadPropertyMultiple-Request)
X'0C' SD Context Tag 0 (Object Identifier, L=4)
X'00000021' Analog Input, Instance Number=33
X'1E' PD Opening Tag 1 (List Of Property References)
X'09' SD Context Tag 0 (Property Identifier, L=1)
X'55' 85 (PRESENT_VALUE)
X'1F' PD Closing Tag 1 (List Of Property References)
X'0C' SD Context Tag 0 (Object Identifier, L=4)
X'00000032' Analog Input, Instance Number=50
X'1E' PD Opening Tag 1 (List Of Property References)
X'09' SD Context Tag 0 (Property Identifier, L=1)
X'55' 85 (PRESENT_VALUE)
X'1F' PD Closing Tag 1 (List Of Property References)
X'0C' SD Context Tag 0 (Object Identifier, L=4)
X'00000023' Analog Input, Instance Number=35
X'1E' PD Opening Tag 1 (List Of Property References)
X'09' SD Context Tag 0 (Property Identifier, L=1)
X'55' 85 (PRESENT_VALUE)
X'1F' PD Closing Tag 1 (List Of Property References)
");
        }
    }
}
