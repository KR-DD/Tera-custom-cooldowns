﻿using TeraDataLite;

namespace TeraPacketParser.Messages;

public class S_LOGIN : ParsedMessage
{
    short NameOffset { get; }
    public ulong EntityId { get; }
    public uint ServerId { get; }
    public uint PlayerId { get; }
    public short Level { get; }
    uint Model { get; }
    // ReSharper disable UnusedAutoPropertyAccessor.Local
    ulong Appearance { get; }
    long RestCurr { get; }
    long RestMax { get; }
    int Weap { get; }
    int Chest { get; }
    int Gloves { get; }
    int Boots { get; }
    int InnerWear { get; }
    int Head { get; }
    int Face { get; }
    int Title { get; }
    int WeapMod { get; }
    int ChestMod { get; }
    int GlovesMod { get; }
    int BootsMod { get; }
    int WeapEnch { get; }
    int HairAdorn { get; }
    int Mask { get; }
    int Back { get; }
    int WeapSkin { get; }

    int Costume { get; }
    // ReSharper restore UnusedAutoPropertyAccessor.Local
    public string Name { get; }


    public Class CharacterClass
    {
        get
        {
            var classId = (int)(Model - 10101) % 100;
            return (Class)classId;
        }
    }

    public S_LOGIN(TeraMessageReader reader) : base(reader)
    {
        reader.Skip(4);
        NameOffset = reader.ReadInt16();
        reader.Skip(8);
        Model = reader.ReadUInt32();
        EntityId = reader.ReadUInt64();
        ServerId = reader.ReadUInt32();
        PlayerId = reader.ReadUInt32();
        reader.Skip(17);
        Appearance = reader.ReadUInt64();
        reader.Skip(2);
        Level = reader.ReadInt16();
        reader.Skip(58);
        if (reader.Factory.ReleaseVersion >= 8600)
        {

            RestCurr = reader.ReadInt64();
            RestMax = reader.ReadInt64();
        }
        else
        {
            RestCurr = reader.ReadInt32();
            RestMax = reader.ReadInt32();
        }

        reader.Skip(8 + 12); // dunno why + 12
        Weap = reader.ReadInt32();
        Chest = reader.ReadInt32();
        Gloves = reader.ReadInt32();
        Boots = reader.ReadInt32();
        InnerWear = reader.ReadInt32();
        Head = reader.ReadInt32();
        Face = reader.ReadInt32();
        reader.Skip(17);
        Title = reader.ReadInt32();
        WeapMod = reader.ReadInt32();
        ChestMod = reader.ReadInt32();
        GlovesMod = reader.ReadInt32();
        BootsMod = reader.ReadInt32();
        //unk19=reader.ReadInt32();
        //unk20=reader.ReadInt32();
        //unk21=reader.ReadInt32();
        //unk22=reader.ReadInt32();
        //unk23=reader.ReadInt32();
        //unk24=reader.ReadInt32();
        //unk25=reader.ReadInt32();
        //unk26=reader.ReadInt32();
        reader.Skip(32);
        WeapEnch = reader.ReadInt32();
        //unk27= reader.ReadInt32();
        //unk28= reader.ReadByte(); 
        //unk29= reader.ReadByte(); 
        reader.Skip(6);
        HairAdorn = reader.ReadInt32();
        Mask = reader.ReadInt32();
        Back = reader.ReadInt32();
        WeapSkin = reader.ReadInt32();
        Costume = reader.ReadInt32();
        //unk30=reader.ReadInt32(); 
        //unk31=reader.ReadInt32(); 
        //unk32=reader.ReadInt32(); 
        //unk33=reader.ReadInt32(); 
        //unk35=reader.ReadInt32(); 
        //unk36=reader.ReadInt32();
        //unk37 = reader.ReadInt16();
        reader.BaseStream.Position = NameOffset - 4;
        Name = reader.ReadTeraString();
        /*
                    details = new byte[detailsCount];
                    details2 = new byte[details2Count];
                    for (var i = 0; i < detailsCount; i++)
                    {
                        details[i] = reader.ReadByte();
                    }
                    for (var i = 0; i < details2Count; i++)
                    {
                        details2[i] = reader.ReadByte();
                    }
        */
        //Console.WriteLine();
        //Console.Write("unk1: {0} - ", unk1);
        //Console.Write("unk2: {0} - ", unk2);
        //Console.Write("unk3: {0} - ", unk3);
        //Console.Write("unk4: {0} - ", unk4);
        //Console.Write("unk5: {0} - ", unk5);
        //Console.Write("unk6: {0} - ", unk6);
        //Console.Write("unk7: {0} - ", unk7);
        //Console.Write("unk8: {0} - ", unk8);
        //Console.Write("unk9: {0} - ", unk9);
        //Console.Write("unk10: {0} - ", unk10);
        //Console.Write("unk11: {0} - ", unk11);
        //Console.Write("unk12: {0} - ", unk12);
        //Console.Write("unk13: {0} - ", unk13);
        //Console.Write("unk14: {0} - ", unk14);
        //Console.Write("unk15: {0} - ", unk15);
        //Console.Write("unk16: {0} - ", unk16);
        //Console.Write("unk17: {0} - ", unk17);
        //Console.Write("unk19: {0} - ", unk19);
        //Console.Write("unk20: {0} - ", unk20);
        //Console.Write("unk21: {0} - ", unk21);
        //Console.Write("unk22: {0} - ", unk22);
        //Console.Write("unk23: {0} - ", unk23);
        //Console.Write("unk24: {0} - ", unk24);
        //Console.Write("unk25: {0} - ", unk25);
        //Console.Write("unk26: {0} - ", unk26);
        //Console.Write("unk27: {0} - ", unk27);
        //Console.Write("unk28: {0} - ", unk28);
        //Console.Write("unk29: {0} - ", unk29);
        //Console.Write("unk30: {0} - ", unk30);
        //Console.Write("unk31: {0} - ", unk31);
        //Console.Write("unk32: {0} - ", unk32);
        //Console.Write("unk33: {0} - ", unk33);
        //Console.Write("unk34: {0} - ", unk34);
        //Console.Write("unk35: {0} - ", unk35);
        //Console.Write("unk36: {0} - ", unk36);
        //Console.Write("unk37: {0} - ", unk37);

    }
}