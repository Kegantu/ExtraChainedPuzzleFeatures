using System.Text;
using ChainedPuzzles;
using GTFO.API;
using LevelGeneration;
using Player;
using SNetwork;

namespace ExtraChainedPuzzleFeatures;

public class CorruptedBioscanManager
{
    public static uint MainLevelLayout => RundownManager.ActiveExpedition.LevelLayoutData;

    public static readonly CorruptedBioscanManager Current;
    
    private Dictionary<IntPtr, uint> bioscanIndex { get; } = new(); 
    
    private Dictionary<IntPtr, uint> clusterIndex { get; } = new();

    private Dictionary<uint, CP_Bioscan_Core> index2Bioscan { get; } = new();

    private Dictionary<uint, CP_Cluster_Core> index2Cluster { get; } = new();

    private uint puzzleOverrideIndex = 1u;

    public uint Register(CP_Bioscan_Core __instance)
    { 
        if (__instance == null)
        {
            return 0u;
        }
        
        uint allotedIndex = puzzleOverrideIndex;
        puzzleOverrideIndex += 1;
        if (bioscanIndex.TryAdd(__instance.Pointer, allotedIndex))
        {
            index2Bioscan.Add(allotedIndex, __instance);
            return allotedIndex;
        }
            
        return GetBioscanCoreOverrideIndex(__instance);
    }

    public uint Register(CP_Cluster_Core __instance)
    {
        if (__instance == null)
        {
            return 0u;
        }

        uint allotedIndex = puzzleOverrideIndex;
        puzzleOverrideIndex += 1;
        if (clusterIndex.TryAdd(__instance.Pointer, allotedIndex))
        {
            index2Cluster.Add(allotedIndex, __instance);
            return allotedIndex;
        }
        
        return GetClusterCoreOverrideIndex(__instance);
    }

    public uint GetBioscanCoreOverrideIndex(CP_Bioscan_Core core) => !bioscanIndex.ContainsKey(core.Pointer) ? 0u : bioscanIndex[core.Pointer];

    public uint GetClusterCoreOverrideIndex(CP_Cluster_Core core) => !clusterIndex.ContainsKey(core.Pointer) ? 0u : clusterIndex[core.Pointer];
    
    public CP_Bioscan_Core GetBioscanCore(uint puzzleOverrideIndex) => index2Bioscan.ContainsKey(puzzleOverrideIndex) ? index2Bioscan[puzzleOverrideIndex] : null;
        
    public CP_Cluster_Core GetClusterCore(uint puzzleOverrideIndex) => index2Cluster.ContainsKey(puzzleOverrideIndex) ? index2Cluster[puzzleOverrideIndex] : null;

    public void Clear()
    {
        puzzleOverrideIndex = 1u;
        bioscanIndex.Clear();
        clusterIndex.Clear();
        index2Bioscan.Clear();
        index2Cluster.Clear();
    }

    static CorruptedBioscanManager()
    {
        Current = new();
        LevelAPI.OnBuildStart += Current.Clear;
        LevelAPI.OnLevelCleanup += Current.Clear;
    }
}