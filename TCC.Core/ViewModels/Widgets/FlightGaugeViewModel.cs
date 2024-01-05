﻿using System;
using TCC.Data;
using TCC.Settings.WindowSettings;
using TCC.Utils;
using TeraPacketParser.Analysis;
using TeraPacketParser.Messages;

namespace TCC.ViewModels.Widgets;

[TccModule]
public class FlightGaugeViewModel : TccWindowViewModel
{
    public event Action<double>? EnergyChanged;

    public FlightStackType Type => FlyingGuardianDataProvider.StackType;
    public double FlightGaugeRotation => ((FlightWindowSettings)Settings!).Rotation;
    public bool FlipFlightGauge => ((FlightWindowSettings)Settings!).Flip;
    public bool FlyingMissionInProgress => FlyingGuardianDataProvider.IsInProgress;

    public FlightGaugeViewModel(FlightWindowSettings settings) : base(settings)
    {
        FlyingGuardianDataProvider.StackTypeChanged += () => InvokePropertyChanged(nameof(Type));
        FlyingGuardianDataProvider.IsInProgressChanged += () => InvokePropertyChanged(nameof(FlyingMissionInProgress));
        settings.RotationChanged += () => InvokePropertyChanged(nameof(FlightGaugeRotation));
        settings.FlipChanged += () => InvokePropertyChanged(nameof(FlipFlightGauge));
    }

    protected override void InstallHooks()
    {
        PacketAnalyzer.Processor.Hook<S_PLAYER_CHANGE_FLIGHT_ENERGY>(OnPlayerChangeFlightEnergy);
    }

    protected override void RemoveHooks()
    {
        PacketAnalyzer.Processor.Unhook<S_PLAYER_CHANGE_FLIGHT_ENERGY>(OnPlayerChangeFlightEnergy);
    }

    void OnPlayerChangeFlightEnergy(S_PLAYER_CHANGE_FLIGHT_ENERGY m)
    {
        EnergyChanged?.Invoke(m.Energy);
    }
}