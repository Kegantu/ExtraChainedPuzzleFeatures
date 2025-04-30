using System.Text;
using ChainedPuzzles;
using GTFO.API;
using LevelGeneration;

namespace ExtraChainedPuzzleFeatures;

public class ChainedPuzzleInstanceManager
{
    static ChainedPuzzleInstanceManager()
    {
        Current = new();
        LevelAPI.OnBuildStart += Current.Clear;
        LevelAPI.OnLevelCleanup += Current.Clear;
    }
    
    public static uint MainLevelLayout => RundownManager.ActiveExpedition.LevelLayoutData;

    public static readonly ChainedPuzzleInstanceManager Current;
    
    private Dictionary<IntPtr, uint> BioscanInstanceIndex { get; } = new(); 
    
    private Dictionary<uint, ChainedPuzzleInstance> Index2BioscanInstance { get; } = new();

    private uint _bioscanInstanceIndex = 1u;
    
    public uint Register(ChainedPuzzleInstance __instance)
    { 
        if (__instance == null) return 0u;

        var allotedIndex = _bioscanInstanceIndex;
        _bioscanInstanceIndex += 1;
        if (BioscanInstanceIndex.TryAdd(__instance.Pointer, allotedIndex))
        {
            Index2BioscanInstance.Add(allotedIndex, __instance);
            return allotedIndex;
        }
            
        return GetBioscanInstanceIndex(__instance);
    }
    
    public uint GetBioscanInstanceIndex(ChainedPuzzleInstance core) => !BioscanInstanceIndex.ContainsKey(core.Pointer) ? 0u : BioscanInstanceIndex[core.Pointer];
    
    public ChainedPuzzleInstance GetBioscanInstance(uint puzzleOverrideIndex) => Index2BioscanInstance.ContainsKey(puzzleOverrideIndex) ? Index2BioscanInstance[puzzleOverrideIndex] : null;
    
    public void Clear()
    {
        _bioscanInstanceIndex = 1u;
        BioscanInstanceIndex.Clear();
        Index2BioscanInstance.Clear();
    }
}