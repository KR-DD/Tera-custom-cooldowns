﻿using System.Collections.Generic;
using TeraDataLite;

namespace TeraPacketParser.Messages;

public class S_SHOW_PARTY_MATCH_INFO : ParsedMessage
{
    public static List<ListingData> Listings { get; } = new();
    public bool IsLast => Pages == Page;
    public short Pages { get; set; }
    public short Page { get; set; }

    public S_SHOW_PARTY_MATCH_INFO(TeraMessageReader reader) : base(reader)
    {
        var count = reader.ReadUInt16();
        var offset = reader.ReadUInt16();
        Page = reader.ReadInt16();
        Pages = reader.ReadInt16();

        if (Page == 0) Listings.Clear();
        if (count == 0)
        {
            Listings.Clear();
            //IsLast = true;
            return;
        }

        reader.BaseStream.Position = offset - 4;
        for (var i = 0; i < count; i++)
        {
            var l = new ListingData();
            reader.Skip(2); // var curr = reader.ReadUInt16();
            var next = reader.ReadUInt16();
            var msgOffset = reader.ReadUInt16();
            var leaderNameOffset = reader.ReadUInt16();
            var leaderId = reader.ReadUInt32();
            var serverId = 0U;
            if (reader.Factory.ReleaseVersion / 100 >= 108) serverId = reader.ReadUInt32(); 
            var isRaid = reader.ReadBoolean();
            var playerCount = reader.ReadInt16();

            reader.BaseStream.Position = msgOffset - 4;
            var msg = reader.ReadTeraString();
            try
            {
                reader.BaseStream.Position = leaderNameOffset - 4;
            }
            catch
            {
                if (next != 0) reader.BaseStream.Position = next - 4;
                continue;
            }
            var leaderName = reader.ReadTeraString();
            l.LeaderName = leaderName;
            l.LeaderId = leaderId;
            l.LeaderServerId = serverId;
            l.IsRaid = isRaid;
            l.Message = msg;
            l.PlayerCount = playerCount;
            {
                Listings.Add(l);
            }
            if (next != 0) reader.BaseStream.Position = next - 4;

        }
    }
}