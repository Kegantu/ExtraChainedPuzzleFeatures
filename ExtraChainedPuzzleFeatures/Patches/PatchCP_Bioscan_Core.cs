using ChainedPuzzles;
using ExtraChainedPuzzleFeatures.HudChanges;
using ExtraChainedPuzzleFeatures.JsonStructure;
using ExtraChainedPuzzleFeatures.Terminal;
using Il2cppPlayerList = Il2CppSystem.Collections.Generic.List<Player.PlayerAgent>;
using GameData;
using GTFO.API.Extensions;
using HarmonyLib;
using LevelGeneration;
using Localization;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ExtraChainedPuzzleFeatures.Patches;

[HarmonyPatch]
public class PatchCP_Bioscan_Core
{
    private static bool _readyToCorrupt;
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(CP_Bioscan_Core), nameof(CP_Bioscan_Core.Setup))]
    private static void PreSetup(CP_Bioscan_Core __instance,
        int puzzleIndex, iChainedPuzzleOwner owner, ref Vector3 prevPuzzlePos, ref bool revealWithHoloPath, ref bool onlyShowHUDWhenPlayerIsClose)
    {
        uint puzzleOverrideIndex = CorruptedBioscanManager.Current.Register(__instance);
        ExtraChainedPuzzleSettings.BioscanFeature def = Main.GetCorruptedBioscan(CorruptedBioscanManager.MainLevelLayout, puzzleOverrideIndex);
        
        if (def == null)
        {
            return;
        }
        
        if (def.CorruptedBioscan && def.CompleteWithTerminalCommand)
        {
            ECPFLogger.Error("Corrupted Scan and Complete On Terminal Command detected on the same scan, please choose only one");
            return;
        }

        if (def.CompleteWithTerminalCommand)
        {
            var playerScanner = __instance.GetComponent<CP_PlayerScanner>();
            
            for (int i = 0; i < playerScanner.m_scanSpeeds.Count; i++)
            {
                __instance.GetComponent<CP_PlayerScanner>().m_scanSpeeds[i] = 0;
            } 
            
            return;
        }
        
        if (!def.CorruptedBioscan)
        {
            return;
        }

        if (!def.SkipBioscan)
        {
            return;
        }
        
        if (def.EventsOnPlayersLeft.Count == 0)
        {
            return;
        }
        
        __instance.GetComponent<CP_PlayerScanner>().m_reduceWhenNoPlayer = true;
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CP_Bioscan_Core), nameof(CP_Bioscan_Core.OnSyncStateChange))]
    private static void PostOnSyncStateChange(CP_Bioscan_Core __instance, 
        eBioscanStatus status, float progress, Il2cppPlayerList playersInScan, int playersMax, bool[] reqItemStatus, bool isDropinState)
    {
        var overrideIndex = CorruptedBioscanManager.Current.GetBioscanCoreOverrideIndex(__instance);
        var def = Main.GetCorruptedBioscan(CorruptedBioscanManager.MainLevelLayout, overrideIndex);
        var scanner = __instance.m_PlayerScannerComp.TryCast<CP_PlayerScanner>();

        if (status == eBioscanStatus.Finished)
        {
            return;
        }
        
        if (scanner.m_scanProgression == 0)
        {
            return;
        }

        if (scanner.m_playerRequirement == PlayerRequirement.None)
        {
            ECPFLogger.Error("Corrupted Bioscan only works on scans that require full team");
            return;
        }

        if (playersInScan.Count == playersMax)
        {
            _readyToCorrupt = true;
            return;
        }

        if (!_readyToCorrupt)
        {
            return;
        }
        
        if (def == null)
        {
            return;
        }
        
        if (def.CorruptedBioscan && def.CompleteWithTerminalCommand)
        {
            return;
        }

        if (def.EventsOnPlayersLeft.Count == 0)
        {
            return;
        }

        if (!def.CorruptedBioscan)
        {
            return;
        }
        
        WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(def.EventsOnPlayersLeft.ToIl2Cpp(), eWardenObjectiveEventTrigger.None, true);
        
        if (!def.SkipBioscan)
        {
            return;
        }
        
        __instance.GetComponent<CP_PlayerScanner>().m_scanProgression = 2;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CP_Bioscan_Core), nameof(CP_Bioscan_Core.IsFinished))]
    private static void PostIsFinished(CP_Bioscan_Core __instance)
    {
        var overrideIndex = CorruptedBioscanManager.Current.GetBioscanCoreOverrideIndex(__instance);
        var def = Main.GetCorruptedBioscan(CorruptedBioscanManager.MainLevelLayout, overrideIndex);
        
        if (def == null)
        {
            return;
        }
        
        _readyToCorrupt = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CP_Bioscan_Core), nameof(CP_Bioscan_Core.Activate))]
    private static void PostActivate(CP_Bioscan_Core __instance)
    {
        var overrideIndex = CorruptedBioscanManager.Current.GetBioscanCoreOverrideIndex(__instance);
        var def = Main.GetCorruptedBioscan(CorruptedBioscanManager.MainLevelLayout, overrideIndex);
        var hud = __instance.m_hud.TryCast<CP_Bioscan_Hud>();
        
        if (def == null)
        {
            return;
        }

        if (!__instance.Owner.TryCast<ChainedPuzzleInstance>())
        {
            return;
        }

        if (def.CorruptedBioscan && def.CompleteWithTerminalCommand)
        {
            ECPFLogger.Error("Corrupted Scan and Complete On Terminal Command detected on the same scan, please choose only one");
            return;
        }
        
        if (def.CorruptedBioscan)
        {
            var corruptedHud = hud.gameObject.AddComponent<CorruptedBioscanHud>();
            corruptedHud.Hud = hud;
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

        if (!hud)
        {
            var clusterHud = __instance.m_hud.TryCast<CP_Cluster_Hud>();
            
            if (clusterHud.gameObject.GetComponent<TerminalTiedBioscanHud>())
            {
                return;
            }
            
            clusterHud.gameObject.AddComponent<TerminalTiedBioscanHud>().Setup(__instance.m_hud, terminal);
            return;
        }
        
        if (hud.gameObject.GetComponent<TerminalTiedBioscanHud>())
        {
            return;
        }
        
        hud.gameObject.AddComponent<TerminalTiedBioscanHud>().Setup(__instance.m_hud, terminal);
    }
}