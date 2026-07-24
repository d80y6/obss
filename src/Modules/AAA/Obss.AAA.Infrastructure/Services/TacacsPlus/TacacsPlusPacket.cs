using System.Security.Cryptography;
using System.Text;

namespace Obss.AAA.Infrastructure.Services.TacacsPlus;

internal enum TacacsPacketType : byte
{
    Authentication = 0x01,
    Authorization = 0x02,
    Accounting = 0x03,
}

internal enum TacacsAuthAction : byte
{
    Login = 0x01,
    Enable = 0x02,
    PapAscii = 0x03,
    Chap = 0x05,
    ArAp = 0x06,
}

internal enum TacacsAuthType : byte
{
    Ascii = 0x01,
    Pap = 0x02,
    Chap = 0x03,
    ArAp = 0x04,
}

internal enum TacacsAuthStatus : byte
{
    Pass = 0x01,
    Fail = 0x02,
    GetData = 0x03,
    GetUser = 0x04,
    GetPassword = 0x05,
    Restart = 0x06,
    Error = 0x07,
    Follow = 0x21,
}

internal enum TacacsAuthorStatus : byte
{
    PassAdd = 0x01,
    PassReply = 0x02,
    PassReplyFollow = 0x03,
    Fail = 0x10,
    Error = 0x11,
    Follow = 0x21,
}

internal enum TacacsAcctStatus : byte
{
    Success = 0x01,
    Error = 0x02,
    Follow = 0x21,
}

[Flags]
internal enum TacacsFlag : byte
{
    None = 0,
    SingleConnect = 0x01,
    NoEncrypt = 0x02,
    UnencryptedFlag = 0x04,
}

internal sealed class TacacsHeader
{
    public byte Version { get; set; } = 0xC0;
    public TacacsPacketType PacketType { get; set; }
    public byte SeqNo { get; set; }
    public TacacsFlag Flags { get; set; }
    public uint SessionId { get; set; }
    public int Length { get; set; }

    public byte[] Encode()
    {
        var buf = new byte[12];
        buf[0] = Version;
        buf[1] = (byte)PacketType;
        buf[2] = SeqNo;
        buf[3] = (byte)Flags;
        buf[4] = (byte)(SessionId >> 24);
        buf[5] = (byte)(SessionId >> 16);
        buf[6] = (byte)(SessionId >> 8);
        buf[7] = (byte)SessionId;
        buf[8] = (byte)(Length >> 16);
        buf[9] = (byte)(Length >> 8);
        buf[10] = (byte)Length;
        buf[11] = 0;
        return buf;
    }

    public static TacacsHeader Decode(ReadOnlySpan<byte> data)
    {
        return new TacacsHeader
        {
            Version = data[0],
            PacketType = (TacacsPacketType)data[1],
            SeqNo = data[2],
            Flags = (TacacsFlag)data[3],
            SessionId = (uint)((data[4] << 24) | (data[5] << 16) | (data[6] << 8) | data[7]),
            Length = (data[8] << 16) | (data[9] << 8) | data[10],
        };
    }
}

internal sealed class TacacsAuthContinue
{
    public byte UserMsgLen { get; set; }
    public byte UserDataLen { get; set; }
    public byte Flags { get; set; }
    public byte[] UserMessage { get; set; } = [];
    public byte[] UserData { get; set; } = [];

    public int Length => 3 + UserMsgLen + UserDataLen;

    public byte[] Encode()
    {
        var buf = new byte[Length];
        buf[0] = UserMsgLen;
        buf[1] = UserDataLen;
        buf[2] = Flags;
        if (UserMsgLen > 0) UserMessage.CopyTo(buf, 3);
        if (UserDataLen > 0) UserData.CopyTo(buf, 3 + UserMsgLen);
        return buf;
    }
}

internal sealed class TacacsAuthReply
{
    public byte Version { get; set; } = 0xC0;
    public TacacsAuthStatus Status { get; set; }
    public byte ArgCnt { get; set; }
    public byte ServerMsgLen { get; set; }
    public byte DataLen { get; set; }
    public string? ServerMsg { get; set; }
    public string? Data { get; set; }

    public static TacacsAuthReply Decode(byte[] body)
    {
        var reply = new TacacsAuthReply
        {
            Version = body[0],
            Status = (TacacsAuthStatus)body[1],
            ArgCnt = body[2],
            ServerMsgLen = body[3],
            DataLen = body[4],
        };

        var offset = 6;
        if (reply.ServerMsgLen > 0)
            reply.ServerMsg = Encoding.UTF8.GetString(body, offset, reply.ServerMsgLen);
        offset += reply.ServerMsgLen;
        if (reply.DataLen > 0)
            reply.Data = Encoding.UTF8.GetString(body, offset, reply.DataLen);

        return reply;
    }
}

internal sealed class TacacsAuthStart
{
    public TacacsAuthAction Action { get; set; } = TacacsAuthAction.Login;
    public byte PrivLvl { get; set; } = 1;
    public TacacsAuthType AuthType { get; set; } = TacacsAuthType.Ascii;
    public byte Service { get; set; } = 1;
    public byte UserLen { get; set; }
    public byte PortLen { get; set; }
    public byte RemAddrLen { get; set; }
    public byte DataLen { get; set; }
    public string? User { get; set; }
    public string? Port { get; set; }
    public string? RemAddr { get; set; }
    public byte[]? Data { get; set; }

    public int Length => 8 + UserLen + PortLen + RemAddrLen + DataLen;

    public byte[] Encode()
    {
        var buf = new byte[Length];
        buf[0] = (byte)Action;
        buf[1] = PrivLvl;
        buf[2] = (byte)AuthType;
        buf[3] = Service;
        buf[4] = UserLen;
        buf[5] = PortLen;
        buf[6] = RemAddrLen;
        buf[7] = DataLen;
        var offset = 8;
        if (UserLen > 0 && User is not null) Encoding.UTF8.GetBytes(User).CopyTo(buf, offset);
        offset += UserLen;
        if (PortLen > 0 && Port is not null) Encoding.UTF8.GetBytes(Port).CopyTo(buf, offset);
        offset += PortLen;
        if (RemAddrLen > 0 && RemAddr is not null) Encoding.UTF8.GetBytes(RemAddr).CopyTo(buf, offset);
        offset += RemAddrLen;
        if (DataLen > 0 && Data is not null) Data.CopyTo(buf, offset);
        return buf;
    }
}

internal static class TacacsCrypto
{
    public static byte[] Encrypt(byte[] body, string secret, TacacsHeader header)
    {
        if (header.Flags.HasFlag(TacacsFlag.NoEncrypt))
            return body;

        return XorBody(body, secret, header);
    }

    public static byte[] Decrypt(byte[] body, string secret, TacacsHeader header) =>
        Encrypt(body, secret, header);

    private static byte[] XorBody(byte[] body, string secret, TacacsHeader header)
    {
        var secretBytes = Encoding.UTF8.GetBytes(secret);
        var result = new byte[body.Length];
        var pad = new byte[body.Length];
        var offset = 0;

        while (offset < body.Length)
        {
            var hashInput = new byte[secretBytes.Length + 12 + 12];
            secretBytes.CopyTo(hashInput, 0);

            hashInput[secretBytes.Length] = (byte)(header.SessionId >> 24);
            hashInput[secretBytes.Length + 1] = (byte)(header.SessionId >> 16);
            hashInput[secretBytes.Length + 2] = (byte)(header.SessionId >> 8);
            hashInput[secretBytes.Length + 3] = (byte)header.SessionId;

            hashInput[secretBytes.Length + 4] = header.Version;
            hashInput[secretBytes.Length + 5] = (byte)header.PacketType;
            hashInput[secretBytes.Length + 6] = header.SeqNo;
            hashInput[secretBytes.Length + 7] = (byte)header.Flags;

            hashInput[secretBytes.Length + 8] = (byte)(offset >> 24);
            hashInput[secretBytes.Length + 9] = (byte)(offset >> 16);
            hashInput[secretBytes.Length + 10] = (byte)(offset >> 8);
            hashInput[secretBytes.Length + 11] = (byte)offset;

            var hash = MD5.HashData(hashInput);
            var copyLen = Math.Min(16, body.Length - offset);
            Array.Copy(hash, 0, pad, offset, copyLen);
            offset += 16;
        }

        for (var i = 0; i < body.Length; i++)
            result[i] = (byte)(body[i] ^ pad[i]);

        return result;
    }
}
