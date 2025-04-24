using ChainedPuzzles;
using ExtraChainedPuzzleFeatures.JsonStructure;
using GameData;
using HarmonyLib;
using LevelGeneration;
using Localization;
using SNetwork;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ExtraChainedPuzzleFeatures.Patches;

[HarmonyPatch]
public class PatchChainedPuzzleInstance
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ChainedPuzzleInstance), nameof(ChainedPuzzleInstance.Setup))]
    private static void PostSetup(ChainedPuzzleInstance __instance, 
        ChainedPuzzleDataBlock data, LG_Area sourceArea, Vector3 sourcePos, Transform parent, LG_Area targetArea = default(LG_Area), bool overrideUseStaticBioscanPoints = false)
    {
        uint puzzleOverrideIndex = ChainedPuzzleInstanceManager.Current.Register(__instance);
        ExtraChainedPuzzleSettings.TimedBioscan def = Main.GetBioscanInstance(ChainedPuzzleInstanceManager.MainLevelLayout, puzzleOverrideIndex);
        
        if (def == null)
        {
            return;
        }
        
        if (def.TimeLeft <= 0)
        {
            return;
        }

        data.PublicAlarmName += " TIMED";

        parent.gameObject.AddComponent<TimedBioscanTimer>();
        
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ChainedPuzzleInstance), nameof(ChainedPuzzleInstance.MasterActivate))]
    private static void PostMasterActivate(ChainedPuzzleInstance __instance)
    {
        uint puzzleOverrideIndex = ChainedPuzzleInstanceManager.Current.GetBioscanInstanceIndex(__instance);
        ExtraChainedPuzzleSettings.TimedBioscan def = Main.GetBioscanInstance(ChainedPuzzleInstanceManager.MainLevelLayout, puzzleOverrideIndex);
        
        if (def == null)
        {
            return;
        }
        
        var timer = __instance.m_parent.GetComponent<TimedBioscanTimer>();
        
        timer.SetTimer(def.TimeLeft);
        timer.StartCountDown(def, __instance);
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ChainedPuzzleInstance), nameof(ChainedPuzzleInstance.OnStateChange))]
    private static void PostOnStateChange(ChainedPuzzleInstance __instance, pChainedPuzzleState oldState, pChainedPuzzleState newState, bool isRecall)
    {
        uint puzzleOverrideIndex = ChainedPuzzleInstanceManager.Current.GetBioscanInstanceIndex(__instance);
        ExtraChainedPuzzleSettings.TimedBioscan def = Main.GetBioscanInstance(ChainedPuzzleInstanceManager.MainLevelLayout, puzzleOverrideIndex);

        if (def == null)
        {
            return;
        }

        if (newState.status != eChainedPuzzleStatus.Solved)
        {
            return;
        }
        
        var timer = __instance.m_parent.GetComponent<TimedBioscanTimer>();
        
       timer.StopCountDown();
    }
}