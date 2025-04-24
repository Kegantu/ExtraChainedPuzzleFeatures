using ChainedPuzzles;
using ExtraChainedPuzzleFeatures.HudChanges;
using ExtraChainedPuzzleFeatures.JsonStructure;
using ExtraChainedPuzzleFeatures.Terminal;
using HarmonyLib;
using LevelGeneration;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ExtraChainedPuzzleFeatures.Patches;

[HarmonyPatch]
public class PatchCP_Cluster_Core
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(CP_Cluster_Core), nameof(CP_Cluster_Core.Setup))]
    private static void PreSetup(
        CP_Cluster_Core __instance, int puzzleIndex, iChainedPuzzleOwner owner,
        ref Vector3 prevPuzzlePos, ref bool revealWithHoloPath)
    {
        var puzzleOverrideIndex = CorruptedBioscanManager.Current.Register(__instance);
        var def = Main.GetCorruptedBioscan(CorruptedBioscanManager.MainLevelLayout, puzzleOverrideIndex);
        
        if (def == null)
        {
            return;
        }
        
        if (def.CompleteWithTerminalCommand)
        {
            return;
        }

        foreach (var core in __instance.m_childCores)
        {
            var playerScanner = core.TryCast<CP_Bioscan_Core>().GetComponent<CP_PlayerScanner>();
            
            for (int i = 0; i < playerScanner.m_scanSpeeds.Count; i++)
            {
                __instance.GetComponent<CP_PlayerScanner>().m_scanSpeeds[i] = 0;
            }
        } 
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CP_Cluster_Core), nameof(CP_Cluster_Core.Activate))]
    private static void PostActivate(CP_Cluster_Core __instance)
    {
        var overrideIndex = CorruptedBioscanManager.Current.GetClusterCoreOverrideIndex(__instance);
        var def = Main.GetCorruptedBioscan(CorruptedBioscanManager.MainLevelLayout, overrideIndex);
        var hud = __instance.m_hud.TryCast<CP_Cluster_Hud>(); 
        
        if (def == null)
        {
            return;
        }
        
        if (!def.CompleteWithTerminalCommand)
        {
            return;
        }
        
        Builder.CurrentFloor.TryGetZoneByLocalIndex(eDimensionIndex.Reality, def.TerminalLayerType, def.TerminalZoneIndex, out var zone);
            
        var index = Random.Range(0, zone.TerminalsSpawnedInZone._size);
        var terminal = zone.TerminalsSpawnedInZone[(Index)index].TryCast<LG_ComputerTerminal>();
        
        if (!terminal)
        {
            ECPFLogger.Error($"There is no terminal inside {zone.ID}");
            return;
        }
        
        var chainedPuzzleTerminal = __instance.gameObject.GetComponent<ChainedPuzzleTerminal>();
        
        if (chainedPuzzleTerminal)
        {
            chainedPuzzleTerminal.UnHide();
        }
        else
        {
            __instance.gameObject.AddComponent<ChainedPuzzleTerminal>().Setup(terminal, terminal.m_command, __instance.TryCast<iChainedPuzzleCore>());
        }
        
        if (hud.gameObject.GetComponent<TerminalTiedBioscanHud>())
        {
            return;
        }
            
        hud.gameObject.AddComponent<TerminalTiedBioscanHud>().Setup(__instance.m_hud, terminal);
    }
}