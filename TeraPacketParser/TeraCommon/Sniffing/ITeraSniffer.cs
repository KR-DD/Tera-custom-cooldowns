﻿// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using TeraPacketParser.TeraCommon.Game;

namespace TeraPacketParser.TeraCommon.Sniffing;

public interface ITeraSniffer
{
    bool Enabled { get; set; }
    bool Connected { get; set; }

    event Action<Message> MessageReceived;
    event Action<Server> NewConnection;
    event Action EndConnection;
}