using System.Text;

namespace Obss.AAA.Infrastructure.Services.Diameter;

internal enum DiameterCommandCode : uint
{
    CapabilitiesExchange = 257,
    ReAuth = 258,
    Accounting = 271,
    CreditControl = 272,
    DeviceWatchdog = 280,
    DisconnectPeer = 282,
}

internal enum DiameterApplicationId : uint
{
    DiameterCommon = 0,
    CreditControl = 4,
}

internal enum DiameterResultCode : uint
{
    Success = 2001,
    LimitedSuccess = 2002,
    CommandUnsupported = 3001,
    UnableToDeliver = 3002,
    RealmNotServed = 3003,
    TooBusy = 3004,
    ApplicationUnsupported = 3007,
    CreditLimitReached = 4012,
    UserUnknown = 5030,
    RatingFailed = 5031,
}

internal enum AvpCode : uint
{
    SessionId = 263,
    OriginHost = 264,
    OriginRealm = 265,
    DestinationHost = 293,
    DestinationRealm = 283,
    AuthApplicationId = 258,
    VendorSpecificApplicationId = 260,
    ResultCode = 268,
    ProductName = 269,
    OriginStateId = 278,
    ErrorMessage = 281,
    FailedAvp = 279,
    RedirectHost = 292,
    RedirectPort = 383,
    SubscriptionId = 443,
    SubscriptionIdType = 450,
    SubscriptionIdData = 444,
    CCRequestType = 416,
    CCRequestNumber = 415,
    ServiceContextId = 461,
    ServiceIdentifier = 439,
    RatingGroup = 432,
    RequestedServiceUnit = 437,
    GrantedServiceUnit = 431,
    UsedServiceUnit = 446,
    MultipleServiceIndicator = 455,
    MultipleServiceCreditControl = 456,
    CCMoney = 413,
    CurrencyCode = 404,
    CostUnit = 413,
    UnitValue = 445,
    ValueDigits = 447,
    Exponent = 448,
    FinalUnitIndication = 430,
    FinalUnitAction = 449,
    CheckBalanceResult = 422,
    CreditControlFailureHandling = 427,
    DirectDebitingFailureHandling = 428,
    ValidityTime = 448,
    ResultCodeExtension = 463,
    CalledStationId = 30,
    CallingStationId = 31,
    FramedIpAddress = 8,
    NasIdentifier = 32,
}

[Flags]
internal enum DiameterFlag : byte
{
    None = 0,
    Request = 0x80,
    Proxiable = 0x40,
    Error = 0x20,
    Retransmitted = 0x10,
}

internal sealed class DiameterHeader
{
    public byte Version { get; set; } = 1;
    public int MessageLength { get; set; }
    public DiameterFlag Flags { get; set; }
    public DiameterCommandCode CommandCode { get; set; }
    public DiameterApplicationId ApplicationId { get; set; }
    public uint HopByHopId { get; set; }
    public uint EndToEndId { get; set; }

    public int Encode(Span<byte> buffer)
    {
        buffer[0] = Version;
        buffer[1] = (byte)Flags;
        buffer[2] = (byte)(MessageLength >> 16);
        buffer[3] = (byte)(MessageLength >> 8);
        buffer[4] = (byte)MessageLength;
        var cc = (uint)CommandCode;
        buffer[5] = (byte)(cc >> 16);
        buffer[6] = (byte)(cc >> 8);
        buffer[7] = (byte)cc;
        var appId = (uint)ApplicationId;
        buffer[8] = (byte)(appId >> 24);
        buffer[9] = (byte)(appId >> 16);
        buffer[10] = (byte)(appId >> 8);
        buffer[11] = (byte)appId;
        buffer[12] = (byte)(HopByHopId >> 24);
        buffer[13] = (byte)(HopByHopId >> 16);
        buffer[14] = (byte)(HopByHopId >> 8);
        buffer[15] = (byte)HopByHopId;
        buffer[16] = (byte)(EndToEndId >> 24);
        buffer[17] = (byte)(EndToEndId >> 16);
        buffer[18] = (byte)(EndToEndId >> 8);
        buffer[19] = (byte)EndToEndId;
        return 20;
    }

    public static DiameterHeader Decode(ReadOnlySpan<byte> data)
    {
        var flags = (DiameterFlag)data[1];
        var msgLen = (data[2] << 16) | (data[3] << 8) | data[4];
        var cc = (uint)((data[5] << 16) | (data[6] << 8) | data[7]);
        var appId = (uint)((data[8] << 24) | (data[9] << 16) | (data[10] << 8) | data[11]);

        return new DiameterHeader
        {
            Version = data[0],
            MessageLength = msgLen,
            Flags = flags,
            CommandCode = (DiameterCommandCode)cc,
            ApplicationId = (DiameterApplicationId)appId,
            HopByHopId = (uint)((data[12] << 24) | (data[13] << 16) | (data[14] << 8) | data[15]),
            EndToEndId = (uint)((data[16] << 24) | (data[17] << 16) | (data[18] << 8) | data[19]),
        };
    }
}

internal sealed class DiameterAvp
{
    public AvpCode Code { get; }
    public bool VendorSpecific { get; }
    public bool Mandatory { get; }
    public bool Protected { get; }
    public uint? VendorId { get; }
    public byte[] Data { get; }

    public DiameterAvp(AvpCode code, byte[] data, bool mandatory = true, uint? vendorId = null)
    {
        Code = code;
        Data = data;
        Mandatory = mandatory;
        VendorId = vendorId;
        VendorSpecific = vendorId.HasValue;
        Protected = false;
    }

    public int RawLength => 8 + (VendorSpecific ? 4 : 0) + Data.Length;

    public int EncodedLength
    {
        get
        {
            var len = RawLength;
            var padding = len % 4;
            if (padding > 0) len += 4 - padding;
            return len;
        }
    }

    public int Encode(Span<byte> buffer)
    {
        var offset = 0;
        var code = (uint)Code;
        buffer[offset++] = (byte)(code >> 24);
        buffer[offset++] = (byte)(code >> 16);
        buffer[offset++] = (byte)(code >> 8);
        buffer[offset++] = (byte)code;

        var avpLen = EncodedLength;
        var flags = 0;
        if (VendorSpecific) flags |= 0x80;
        if (Mandatory) flags |= 0x40;
        if (Protected) flags |= 0x20;
        buffer[offset++] = (byte)flags;
        buffer[offset++] = (byte)(avpLen >> 16);
        buffer[offset++] = (byte)(avpLen >> 8);
        buffer[offset++] = (byte)avpLen;

        if (VendorSpecific && VendorId.HasValue)
        {
            var vid = VendorId.Value;
            buffer[offset++] = (byte)(vid >> 24);
            buffer[offset++] = (byte)(vid >> 16);
            buffer[offset++] = (byte)(vid >> 8);
            buffer[offset++] = (byte)vid;
        }

        Buffer.BlockCopy(Data, 0, buffer.ToArray(), offset, Data.Length);
        offset += Data.Length;

        var padded = EncodedLength - RawLength;
        for (var i = 0; i < padded; i++)
            buffer[offset++] = 0;

        return offset;
    }

    public static DiameterAvp Decode(ReadOnlySpan<byte> data, int offset, out int consumed)
    {
        var code = (AvpCode)((uint)(data[offset] << 24) | (uint)(data[offset + 1] << 16) | (uint)(data[offset + 2] << 8) | data[offset + 3]);
        var flags = data[offset + 4];
        var avpLen = (data[offset + 5] << 16) | (data[offset + 6] << 8) | data[offset + 7];
        var vendorSpecific = (flags & 0x80) != 0;
        var mandatory = (flags & 0x40) != 0;

        uint? vendorId = null;
        var dataOffset = offset + 8;
        if (vendorSpecific)
        {
            vendorId = (uint)((data[offset + 8] << 24) | (data[offset + 9] << 16) | (data[offset + 10] << 8) | data[offset + 11]);
            dataOffset = offset + 12;
        }

        var payloadLen = avpLen - (vendorSpecific ? 12 : 8);
        var payload = new byte[payloadLen];
        if (payloadLen > 0)
            data.Slice(dataOffset, payloadLen).CopyTo(payload);

        consumed = offset + ((avpLen + 3) / 4 * 4);
        if (consumed - offset < avpLen)
            consumed = offset + avpLen;

        return new DiameterAvp(code, payload, mandatory, vendorId);
    }
}

internal sealed class DiameterMessage
{
    public DiameterHeader Header { get; set; } = new();
    public List<DiameterAvp> Avps { get; set; } = [];

    public byte[] Encode()
    {
        var totalLen = 20 + Avps.Sum(a => a.EncodedLength);
        Header.MessageLength = totalLen;
        var buffer = new byte[totalLen];

        Header.Encode(buffer);
        var offset = 20;

        foreach (var avp in Avps)
        {
            var avpBuffer = new byte[avp.EncodedLength];
            avp.Encode(avpBuffer);
            Buffer.BlockCopy(avpBuffer, 0, buffer, offset, avpBuffer.Length);
            offset += avpBuffer.Length;
        }

        return buffer;
    }

    public static DiameterMessage Decode(byte[] data)
    {
        var header = DiameterHeader.Decode(data);
        var msg = new DiameterMessage { Header = header };

        var offset = 20;
        while (offset < data.Length)
        {
            var avp = DiameterAvp.Decode(data, offset, out var consumed);
            msg.Avps.Add(avp);
            offset = consumed;
        }

        return msg;
    }

    public uint? GetUint32Avp(AvpCode code)
    {
        var avp = Avps.FirstOrDefault(a => a.Code == code);
        if (avp?.Data.Length == 4)
        {
            return (uint)((avp.Data[0] << 24) | (avp.Data[1] << 16) | (avp.Data[2] << 8) | avp.Data[3]);
        }
        return null;
    }

    public static DiameterMessage CreateCreditControlRequest(
        uint ccRequestType,
        uint ccRequestNumber,
        string sessionId,
        string originHost,
        string originRealm,
        string destinationRealm,
        string? destinationHost,
        string? serviceContextId,
        uint hopByHopId,
        uint endToEndId)
    {
        var msg = new DiameterMessage
        {
            Header = new DiameterHeader
            {
                Version = 1,
                Flags = DiameterFlag.Request | DiameterFlag.Proxiable,
                CommandCode = DiameterCommandCode.CreditControl,
                ApplicationId = DiameterApplicationId.CreditControl,
                HopByHopId = hopByHopId,
                EndToEndId = endToEndId,
            }
        };

        msg.Avps.Add(new DiameterAvp(AvpCode.SessionId, Encoding.UTF8.GetBytes(sessionId)));
        msg.Avps.Add(new DiameterAvp(AvpCode.OriginHost, Encoding.UTF8.GetBytes(originHost)));
        msg.Avps.Add(new DiameterAvp(AvpCode.OriginRealm, Encoding.UTF8.GetBytes(originRealm)));
        msg.Avps.Add(new DiameterAvp(AvpCode.DestinationRealm, Encoding.UTF8.GetBytes(destinationRealm)));

        if (!string.IsNullOrEmpty(destinationHost))
            msg.Avps.Add(new DiameterAvp(AvpCode.DestinationHost, Encoding.UTF8.GetBytes(destinationHost)));

        msg.Avps.Add(new DiameterAvp(AvpCode.CCRequestType, PackUint32(ccRequestType)));
        msg.Avps.Add(new DiameterAvp(AvpCode.CCRequestNumber, PackUint32(ccRequestNumber)));

        if (!string.IsNullOrEmpty(serviceContextId))
            msg.Avps.Add(new DiameterAvp(AvpCode.ServiceContextId, Encoding.UTF8.GetBytes(serviceContextId)));

        msg.Avps.Add(new DiameterAvp(AvpCode.AuthApplicationId, PackUint32((uint)DiameterApplicationId.CreditControl)));

        return msg;
    }

    internal static byte[] PackUint32(uint value)
    {
        return
        [
            (byte)(value >> 24),
            (byte)(value >> 16),
            (byte)(value >> 8),
            (byte)value,
        ];
    }
}
