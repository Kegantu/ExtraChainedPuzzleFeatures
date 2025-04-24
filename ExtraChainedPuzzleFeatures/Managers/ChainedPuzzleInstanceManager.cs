using System.Text;
using ChainedPuzzles;
using GTFO.API;
using LevelGeneration;

namespace ExtraChainedPuzzleFeatures;

public class ChainedPuzzleInstanceManager
{
    public static uint MainLevelLayout => RundownManager.ActiveExpedition.LevelLayoutData;

    public static readonly ChainedPuzzleInstanceManager Current;
    
    private Dictionary<IntPtr, uint> BioscanInstanceIndex { get; } = new(); 
    
    private Dictionary<uint, ChainedPuzzleInstance> Index2BioscanInstance { get; } = new();

    private uint _bioscanInstanceIndex = 1u;
    
    public uint Register(ChainedPuzzleInstance __instance)
    { 
        if (__instance == null) return 0u;

        uint allotedIndex = _bioscanInstanceIndex;
        _bioscanInstanceIndex += 1;
        if (!BioscanInstanceIndex.ContainsKey(__instance.Pointer))
        {
            BioscanInstanceIndex.Add(__instance.Pointer, allotedIndex);
            //bioscanCoreIntPtr2Index.Add(__instance.Pointer, allotedIndex);
            Index2BioscanInstance.Add(allotedIndex, __instance);
            return allotedIndex;
        }
            
        return GetBioscanInstanceIndex(__instance);
    }
    
    public void OutputLevelPuzzleInfo()
    {
        List<ChainedPuzzleInstance> levelChainedPuzzleInstances = new();
        foreach (var cpInstance in ChainedPuzzleManager.Current.m_instances)
                levelChainedPuzzleInstances.Add(cpInstance);

        levelChainedPuzzleInstances.Sort((c1, c2) =>
        {
            LG_Zone z1 = c1.m_sourceArea.m_zone;
            LG_Zone z2 = c2.m_sourceArea.m_zone;
            if (z1.DimensionIndex != z2.DimensionIndex) return (uint)z1.DimensionIndex < (uint)z2.DimensionIndex ? -1 : 1;
            if (z1.Layer.m_type != z2.Layer.m_type) return (uint)z1.Layer.m_type < (uint)z2.Layer.m_type ? -1 : 1;

            return (uint)z1.LocalIndex < (uint)z2.LocalIndex ? -1 : 1;
        });

        StringBuilder chainedPuzzlesInfo = new();
        foreach (var chainedPuzzleInstance in levelChainedPuzzleInstances)
        {
            LG_Zone srcZone = chainedPuzzleInstance.m_sourceArea.m_zone;
            // alarm info. 
            chainedPuzzlesInfo.Append($"\nZone {srcZone.Alias}, {srcZone.m_layer.m_type}, Dim {srcZone.DimensionIndex}\n");
            chainedPuzzlesInfo.Append($"Alarm name: {chainedPuzzleInstance.Data.PublicAlarmName}:\n");

            if (BioscanInstanceIndex.ContainsKey(chainedPuzzleInstance.Pointer))
            {
                uint puzzleOverrideIndex = BioscanInstanceIndex[chainedPuzzleInstance.Pointer];
                chainedPuzzlesInfo.Append($"ChainedPuzzleInstanceIndex: {puzzleOverrideIndex}\n");
            }
            else
            {
                ECPFLogger.Error("Unregistered iChainedPuzzleCore found...");
            }
                
            chainedPuzzlesInfo.Append('\n');
        }

        ECPFLogger.Debug(chainedPuzzlesInfo.ToString());
        }
    
    public uint GetBioscanInstanceIndex(IntPtr pointer) => !BioscanInstanceIndex.ContainsKey(pointer) ? 0u : BioscanInstanceIndex[pointer];
    
    public uint GetBioscanInstanceIndex(ChainedPuzzleInstance core) => !BioscanInstanceIndex.ContainsKey(core.Pointer) ? 0u : BioscanInstanceIndex[core.Pointer];
    
    public ChainedPuzzleInstance GetBioscanInstance(uint puzzleOverrideIndex) => Index2BioscanInstance.ContainsKey(puzzleOverrideIndex) ? Index2BioscanInstance[puzzleOverrideIndex] : null;
    
    public void Clear()
    {
        _bioscanInstanceIndex = 1u;
        BioscanInstanceIndex.Clear();
        Index2BioscanInstance.Clear();
    }

    static ChainedPuzzleInstanceManager()
    {
        Current = new();
        LevelAPI.OnEnterLevel += Current.OutputLevelPuzzleInfo;
        LevelAPI.OnBuildStart += Current.Clear;
        LevelAPI.OnLevelCleanup += Current.Clear;
    }
}