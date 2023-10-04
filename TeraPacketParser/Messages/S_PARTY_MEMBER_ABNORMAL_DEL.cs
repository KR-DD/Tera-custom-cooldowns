﻿


namespace TeraPacketParser.Messages;

public class S_PARTY_MEMBER_ABNORMAL_DEL : ParsedMessage
{
    public uint ServerId { get; private set; }
    public uint PlayerId { get; private set; }
    public uint Id { get; private set; }

    public S_PARTY_MEMBER_ABNORMAL_DEL(TeraMessageReader reader) : base(reader)
    {
        ServerId = reader.ReadUInt32();
        PlayerId = reader.ReadUInt32();
        Id = reader.ReadUInt32();
    }
}