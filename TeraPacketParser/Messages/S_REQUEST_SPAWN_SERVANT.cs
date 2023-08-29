﻿namespace TeraPacketParser.Messages
{
    public class S_REQUEST_SPAWN_SERVANT : ParsedMessage
    {
        public ulong Owner { get; }
        public ulong EntityId { get; set; }
        public GameId GameId { get; set; }

        public uint FellowShip { get; set; }
        public uint ServantType { get; }
        public uint Id { get; set; }
        public uint SpawnType { get; }

        public S_REQUEST_SPAWN_SERVANT(TeraMessageReader reader) : base(reader)
        {
            // ReSharper disable UnusedVariable
            var giftedSkillsCount = reader.ReadUInt16();
            var giftedSkillsOffset = reader.ReadUInt16();
            var abilitiesCount = reader.ReadUInt16();
            var abilitiesOffset = reader.ReadUInt16();
            var nameOffset = reader.ReadUInt16();
            EntityId = reader.ReadUInt64();
            GameId = GameId.Parse(EntityId);
            var dbid = reader.ReadUInt64();
            var loc = reader.ReadVector3f();
            var h = reader.ReadUInt16();
            ServantType = reader.ReadUInt32();
            Id = reader.ReadUInt32();
            var linkedNpcTemplateId = reader.ReadUInt32();
            var linkedNpcZoneId = reader.ReadUInt16();
            var walkSpeed = reader.ReadUInt16();
            var runSpeed = reader.ReadUInt16();

            Owner = reader.ReadUInt64();
            var energy = reader.ReadUInt32();
            SpawnType = reader.ReadUInt32();
            var level = reader.ReadUInt32();
            FellowShip = reader.ReadUInt32();
            // ReSharper restore UnusedVariable
        }

    }
}
