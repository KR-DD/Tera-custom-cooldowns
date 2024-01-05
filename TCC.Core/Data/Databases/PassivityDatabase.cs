﻿using System.Collections.Generic;
using TCC.Data.Skills;
using TeraDataLite;

namespace TCC.Data.Databases;

public static class PassivityDatabase
{
    //TODO: maybe move this to a TeraData tsv file (or merge in hotdot.tsv)
    public static Dictionary<uint, uint> Passivities { get; } = new()
    {
        {6001,60}, {6002,60}, {6003,60}, {6004,60}, // dragon
        {6012,60}, {6013,60}, // phoenix
        {6017,60}, {6018,60}, // drake
        {60029,120}, {60030,120}, { 60031,120}, { 60032,120},  //concentration
        {15162,60}, // insigna of the punisher
        {6040, 60}, {6041,60},

        // bracing force
        {13000, 180},{13001, 180},{13002, 180},
        {13003, 180},{13004, 180},{13005, 180},
        {13006, 180},{13007, 180},{13008, 180},
        {13009, 180},{13010, 180},{13011, 180},
        {13012, 180},{13013, 180},{13014, 180},
        {13015, 180},{13016, 180},{13017, 180},
        {13018, 180},{13019, 180},{13020, 180},
        {13021, 180},{13022, 180},{13023, 180},
        {13024, 180},{13025, 180},{13026, 180},
        {13027, 180},{13028, 180},{13029, 180},
        {13030, 180},{13031, 180},{13032, 180},
        {13033, 180},{13034, 180},{13035, 180},
        {13036, 180},{13037, 180},

        // magic amp
        { 13040, 180 }, { 13041, 180 }, { 13042, 180 },
        { 13043, 180 }, { 13044, 180 }, { 13045, 180 },
        { 13046, 180 }, { 13047, 180 }, { 13048, 180 },
        { 13049, 180 }, { 13050, 180 }, { 13051, 180 },
        { 13052, 180 }, { 13053, 180 }, { 13054, 180 },
        { 13055, 180 }, { 13056, 180 }, { 13057, 180 },
        { 13058, 180 }, { 13059, 180 }, { 13060, 180 },
        { 13061, 180 }, { 13062, 180 }, { 13063, 180 },
        { 13064, 180 }, { 13065, 180 }, { 13066, 180 },
        { 13067, 180 }, { 13068, 180 }, { 13069, 180 },
        { 13070, 180 }, { 13071, 180 }, { 13072, 180 },
        { 13073, 180 }, { 13074, 180 }, { 13075, 180 },
        { 13076, 180 }, { 13077, 180 },

        // phys amp
        { 13080, 180 }, { 13081, 180 }, { 13082, 180 },
        { 13083, 180 }, { 13084, 180 }, { 13085, 180 },
        { 13086, 180 }, { 13087, 180 }, { 13088, 180 },
        { 13089, 180 }, { 13090, 180 }, { 13091, 180 },
        { 13092, 180 }, { 13093, 180 }, { 13094, 180 },
        { 13095, 180 }, { 13096, 180 }, { 13097, 180 },
        { 13098, 180 }, { 13099, 180 }, { 13100, 180 },
        { 13101, 180 }, { 13102, 180 }, { 13103, 180 },
        { 13104, 180 }, { 13105, 180 }, { 13106, 180 },
        { 13107, 180 }, { 13108, 180 }, { 13109, 180 },
        { 13110, 180 }, { 13111, 180 }, { 13112, 180 },
        { 13113, 180 }, { 13114, 180 }, { 13115, 180 },
        { 13116, 180 }, { 13117, 180 }

    };
    public static bool TryGetPassivitySkill(uint id, out Skill sk)
    {
        sk = new Skill(0, Class.None, string.Empty, string.Empty);

        if (!Game.DB!.AbnormalityDatabase.Abnormalities.TryGetValue(id, out var ab)) return false;

        sk = new Skill(id, Class.Common, ab.Name, "") { IconName = ab.IconName };
        return true;
    }
}