using GameData;
using LevelGeneration;

namespace ExtraChainedPuzzleFeatures.JsonStructure;

public class ExtraChainedPuzzleSettings
{
    public class BioscanFeature
    {
        public uint BioscanIndex { get; set; }

        public bool CorruptedBioscan { get; set; } = false;

        public bool SkipBioscan { get; set; } = false;

        public bool CompleteWithTerminalCommand { get; set; } = false;

        public LG_LayerType TerminalLayerType { get; set; } = LG_LayerType.MainLayer;

        public eLocalZoneIndex TerminalZoneIndex { get; set; } = 0;
        
        public List<WardenObjectiveEventData> EventsOnPlayersLeft { get; set; } = new();
    }
    
    public class TimedBioscan
    {
        public uint BioscanInstanceIndex { get; set; }
        
        public int TimeLeft { get; set; } = 60;

        public bool ResetOnTimeExpired { get; set; } = false;

        public bool StopAlarmOnTimeExpired { get; set; } = true;
        
        public List<WardenObjectiveEventData> EventsOnTimeExpired { get; set; } = new();
    }
}