using System.Collections.Concurrent;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Obss.AAA.Infrastructure.Services.Radius;

internal enum RadiusCode : byte
{
    AccessRequest = 1,
    AccessAccept = 2,
    AccessReject = 3,
    AccountingRequest = 4,
    AccountingResponse = 5,
    AccessChallenge = 11,
    CoARequest = 43,
    CoAACK = 44,
    CoANAK = 45,
    DisconnectRequest = 40,
    DisconnectACK = 41,
    DisconnectNAK = 42
}

internal enum RadiusAttributeType : byte
{
    UserName = 1,
    UserPassword = 2,
    ChapPassword = 3,
    NasIpAddress = 4,
    NasPort = 5,
    ServiceType = 6,
    FramedProtocol = 7,
    FramedIpAddress = 8,
    FramedIpNetmask = 9,
    FramedRouting = 10,
    FilterId = 11,
    FramedMtu = 12,
    FramedCompression = 13,
    LoginIpHost = 14,
    LoginService = 15,
    LoginPort = 16,
    ReplyMessage = 18,
    CallbackNumber = 19,
    CallbackId = 20,
    FramedRoute = 22,
    FramedIpXNetwork = 23,
    State = 24,
    Class = 25,
    VendorSpecific = 26,
    SessionTimeout = 27,
    IdleTimeout = 28,
    TerminationAction = 29,
    CalledStationId = 30,
    CallingStationId = 31,
    NasIdentifier = 32,
    ProxyState = 33,
    LoginLATService = 34,
    LoginLATNode = 35,
    LoginLATGroup = 36,
    FramedAppleTalkLink = 37,
    FramedAppleTalkNetwork = 38,
    FramedAppleTalkZone = 39,
    AcctStatusType = 40,
    AcctDelayTime = 41,
    AcctInputOctets = 42,
    AcctOutputOctets = 43,
    AcctSessionId = 44,
    AcctAuthentic = 45,
    AcctSessionTime = 46,
    AcctInputPackets = 47,
    AcctOutputPackets = 48,
    AcctTerminateCause = 49,
    AcctMultiSessionId = 50,
    AcctLinkCount = 51,
    AcctInputGigawords = 52,
    AcctOutputGigawords = 53,
    EventTimestamp = 55,
    NasPortId = 61,
    FramedInterfaceId = 96,
    FramedIpv6Prefix = 97,
    LoginIpv6Host = 98,
    FramedIpv6Route = 99,
    FramedIpv6Pool = 100,
    ErrorCause = 101,
    AcctTunnelConnection = 68,
    TunnelType = 64,
    TunnelMediumType = 65,
    TunnelClientEndpoint = 66,
    TunnelServerEndpoint = 67,
    ChargeableUserIdentity = 89,
    EapMessage = 79,
    MessageAuthenticator = 80
}

internal sealed class RadiusAttribute
{
    public RadiusAttributeType Type { get; }
    public byte[] Value { get; set; }

    public RadiusAttribute(RadiusAttributeType type, byte[] value)
    {
        Type = type;
        Value = value;
    }
}

internal sealed class RadiusPacket
{
    private static readonly ConcurrentDictionary<string, byte[]> _requestAuthenticators = new();

    public RadiusCode Code { get; set; }
    public byte Identifier { get; set; }
    public byte[] Authenticator { get; set; } = new byte[16];
    public List<RadiusAttribute> Attributes { get; set; } = [];

    public ushort Length => (ushort)(20 + Attributes.Sum(a => 2 + a.Value.Length));

    public byte[] Encode(string secret)
    {
        var length = Length;
        var packet = new byte[length];
        packet[0] = (byte)Code;
        packet[1] = Identifier;
        packet[2] = (byte)(length >> 8);
        packet[3] = (byte)(length & 0xFF);

        Buffer.BlockCopy(Authenticator, 0, packet, 4, 16);

        var offset = 20;
        foreach (var attr in Attributes)
        {
            packet[offset] = (byte)attr.Type;
            packet[offset + 1] = (byte)(2 + attr.Value.Length);
            Buffer.BlockCopy(attr.Value, 0, packet, offset + 2, attr.Value.Length);
            offset += 2 + attr.Value.Length;
        }

        if (Code is RadiusCode.AccessRequest or RadiusCode.AccountingRequest or RadiusCode.CoARequest
            or RadiusCode.DisconnectRequest)
        {
            var md5 = MD5.HashData(packet);
            md5.AsSpan().CopyTo(Authenticator);
            Buffer.BlockCopy(md5, 0, packet, 4, 16);

            if (Code == RadiusCode.AccessRequest)
            {
                foreach (var attr in Attributes.Where(a => a.Type == RadiusAttributeType.UserPassword))
                {
                    var encrypted = EncryptUserPassword(attr.Value, secret, Authenticator);
                    attr.Value = encrypted;
                    Buffer.BlockCopy(encrypted, 0, packet, 20 + Attributes.TakeWhile(a => a != attr).Sum(a => 2 + a.Value.Length) + 2, encrypted.Length);
                }
            }

            if (HasMessageAuthenticator())
            {
                var msgAuthAttr = Attributes.First(a => a.Type == RadiusAttributeType.MessageAuthenticator);
                var emptyAuth = new byte[16];
                msgAuthAttr.Value = emptyAuth;

                var msgAuthOffset = 20;
                foreach (var attr in Attributes)
                {
                    if (attr == msgAuthAttr)
                        break;
                    msgAuthOffset += 2 + attr.Value.Length;
                }

                var preMsgPacket = new byte[packet.Length];
                Buffer.BlockCopy(packet, 0, preMsgPacket, 0, packet.Length);
                Buffer.BlockCopy(emptyAuth, 0, preMsgPacket, msgAuthOffset + 2, 16);

                var msgMd5 = MD5.HashData(preMsgPacket);
                msgAuthAttr.Value = msgMd5;
                Buffer.BlockCopy(msgMd5, 0, packet, msgAuthOffset + 2, 16);
            }

            var finalMd5 = MD5.HashData(packet);
            _requestAuthenticators[Identifier.ToString()] = finalMd5;
        }

        return packet;
    }

    public static RadiusPacket Parse(byte[] data, string secret, byte[]? requestAuthenticator = null)
    {
        if (data.Length < 20)
            throw new ArgumentException("Packet too short");

        var packet = new RadiusPacket
        {
            Code = (RadiusCode)data[0],
            Identifier = data[1],
        };

        var length = (ushort)((data[2] << 8) | data[3]);
        if (data.Length < length)
            throw new ArgumentException("Packet truncated");

        Buffer.BlockCopy(data, 4, packet.Authenticator, 0, 16);

        var offset = 20;
        while (offset < length)
        {
            if (offset + 1 >= length)
                break;

            var attrType = (RadiusAttributeType)data[offset];
            var attrLength = data[offset + 1];

            if (attrLength < 2 || offset + attrLength > length)
                break;

            var value = new byte[attrLength - 2];
            if (value.Length > 0)
                Buffer.BlockCopy(data, offset + 2, value, 0, value.Length);

            packet.Attributes.Add(new RadiusAttribute(attrType, value));
            offset += attrLength;
        }

        if (packet.Code is RadiusCode.AccessAccept or RadiusCode.AccessReject
            or RadiusCode.AccountingResponse or RadiusCode.AccessChallenge
            or RadiusCode.CoAACK or RadiusCode.CoANAK
            or RadiusCode.DisconnectACK or RadiusCode.DisconnectNAK)
        {
            if (requestAuthenticator == null)
            {
                _requestAuthenticators.TryGetValue(packet.Identifier.ToString(), out requestAuthenticator);
            }

            if (requestAuthenticator != null)
            {
                var reconstructed = new byte[length];
                Buffer.BlockCopy(data, 0, reconstructed, 0, length);
                var origAuth = new byte[16];
                Buffer.BlockCopy(data, 4, origAuth, 0, 16);
                Buffer.BlockCopy(requestAuthenticator, 0, reconstructed, 4, 16);

                var expected = MD5.HashData(reconstructed);
                if (!expected.AsSpan().SequenceEqual(origAuth.AsSpan()))
                    throw new InvalidOperationException("Response authenticator mismatch");
            }
        }

        return packet;
    }

    public T? GetAttribute<T>(RadiusAttributeType type, Func<byte[], T> converter)
        where T : class
    {
        var attr = Attributes.FirstOrDefault(a => a.Type == type);
        return attr?.Value is not null ? converter(attr.Value) : null;
    }

    public string? GetStringAttribute(RadiusAttributeType type)
    {
        var attr = Attributes.FirstOrDefault(a => a.Type == type);
        return attr?.Value is not null ? Encoding.UTF8.GetString(attr.Value) : null;
    }

    public IPAddress? GetIpAttribute(RadiusAttributeType type)
    {
        var attr = Attributes.FirstOrDefault(a => a.Type == type);
        if (attr?.Value.Length == 4)
            return new IPAddress(attr.Value);
        return null;
    }

    public uint? GetUintAttribute(RadiusAttributeType type)
    {
        var attr = Attributes.FirstOrDefault(a => a.Type == type);
        if (attr?.Value.Length == 4)
            return (uint)((attr.Value[0] << 24) | (attr.Value[1] << 16) | (attr.Value[2] << 8) | attr.Value[3]);
        return null;
    }

    public bool HasMessageAuthenticator()
    {
        return Attributes.Any(a => a.Type == RadiusAttributeType.MessageAuthenticator);
    }

    private static byte[] EncryptUserPassword(byte[] password, string secret, byte[] authenticator)
    {
        var paddedLen = ((password.Length + 15) / 16) * 16;
        if (paddedLen == 0) paddedLen = 16;
        var padded = new byte[paddedLen];
        Buffer.BlockCopy(password, 0, padded, 0, password.Length);

        if (password.Length < paddedLen)
            padded[password.Length] = 0;

        var secretBytes = Encoding.UTF8.GetBytes(secret);
        var result = new byte[paddedLen];

        for (var i = 0; i < paddedLen; i += 16)
        {
            var hashInput = new byte[secretBytes.Length + 16];
            Buffer.BlockCopy(secretBytes, 0, hashInput, 0, secretBytes.Length);

            if (i == 0)
            {
                Buffer.BlockCopy(authenticator, 0, hashInput, secretBytes.Length, 16);
            }
            else
            {
                Buffer.BlockCopy(result, i - 16, hashInput, secretBytes.Length, 16);
            }

            var hash = MD5.HashData(hashInput);
            for (var j = 0; j < 16 && i + j < paddedLen; j++)
                result[i + j] = (byte)(padded[i + j] ^ hash[j]);
        }

        return result;
    }
}
