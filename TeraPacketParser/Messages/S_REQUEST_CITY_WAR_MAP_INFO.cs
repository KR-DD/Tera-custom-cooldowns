﻿using System.Collections.Generic;
using TeraDataLite;

namespace TeraPacketParser.Messages;

public class S_REQUEST_CITY_WAR_MAP_INFO : ParsedMessage
{
    public readonly List<CityWarGuildData> Guilds;
    public S_REQUEST_CITY_WAR_MAP_INFO(TeraMessageReader reader) : base(reader)
    {
        Guilds = new List<CityWarGuildData>();
        try
        {
            var count = reader.ReadUInt16();
            var offset = reader.ReadUInt16();
            reader.Skip(4 + 4 + 4); //id, state, timeRemaining
            reader.BaseStream.Position = offset - 4;
            for (var i = 0; i < count; i++)
            {
                reader.Skip(2); // var current = reader.ReadUInt16();
                var next = reader.ReadUInt16();
                var self = reader.ReadInt32();
                var id = reader.ReadUInt32();
                var kills = reader.ReadInt32();
                var deaths = reader.ReadInt32();
                var towerHp = reader.ReadSingle();
                Guilds.Add(new CityWarGuildData(self, id, kills, deaths, towerHp));

                if (next != 0) reader.BaseStream.Position = next - 4;
            }
        }
        catch
        {
            // ignored
        }
    }
}