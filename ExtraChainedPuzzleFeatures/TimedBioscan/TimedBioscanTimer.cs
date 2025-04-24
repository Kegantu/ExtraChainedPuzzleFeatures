using System.Collections;
using BepInEx.Unity.IL2CPP.Hook;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using ChainedPuzzles;
using Enemies;
using ExtraChainedPuzzleFeatures.JsonStructure;
using ExtraChainedPuzzleFeatures.Terminal;
using GameData;
using GTFO.API.Extensions;
using Il2CppInterop.Runtime.Injection;
using LevelGeneration;
using Player;
using SNetwork;
using UnityEngine;

namespace ExtraChainedPuzzleFeatures.JsonStructure;

public class TimedBioscanTimer : MonoBehaviour
{
    static TimedBioscanTimer()
    {
        ClassInjector.RegisterTypeInIl2Cpp<TimedBioscanTimer>();
    }
    
    private float timer;

    public void SetTimer(float time) => timer = time;

    public void StartCountDown(ExtraChainedPuzzleSettings.TimedBioscan def, ChainedPuzzleInstance instance)
    {
        GuiManager.PlayerLayer.m_objectiveTimer.Setup();
        GuiManager.PlayerLayer.m_objectiveTimer.SetTimerActive(true);
        GuiManager.PlayerLayer.m_objectiveTimer.UpdateTimerTitle("<color=red>Timed Bioscan Detected</color>");
        GuiManager.PlayerLayer.m_objectiveTimer.UpdateTimerText(timer, def.TimeLeft, Color.yellow);
        
        StartCoroutine(CountDown(def, instance).WrapToIl2Cpp());
    }

    public void StopCountDown()
    {
        StopAllCoroutines();
        GuiManager.PlayerLayer.m_objectiveTimer.SetTimerActive(false);
        Destroy(this);
    }
    
    private IEnumerator CountDown(ExtraChainedPuzzleSettings.TimedBioscan def, ChainedPuzzleInstance instance)
    {
        while (timer > 0)
        {
            yield return new WaitForSeconds(1f);
            timer--;
            yield return null;
            
            GuiManager.PlayerLayer.m_objectiveTimer.UpdateTimerText(timer, def.TimeLeft, Color.yellow);

            if (timer <= 0)
            {
                GuiManager.PlayerLayer.m_objectiveTimer.SetTimerActive(false);
                
                WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(def.EventsOnTimeExpired.ToIl2Cpp(), eWardenObjectiveEventTrigger.None, true);
                
                if (!def.ResetOnTimeExpired)
                {
                    yield break;
                }
       
                ResetBioscan(instance, def);
                
                yield break;
            }
        }
    }
    
    private void ResetBioscanChild(iChainedPuzzleCore puzzleCore)
    {
        var bioscanCore = puzzleCore.TryCast<CP_Bioscan_Core>();

        if (!bioscanCore)
        {
            var clusterCore = puzzleCore.TryCast<CP_Cluster_Core>();

            if (!clusterCore)
            {
                return;
            }

            var splineCluster = clusterCore.m_spline.Cast<CP_Holopath_Spline>();
            splineCluster.SetSplineProgress(0);
            
            foreach (var clusterChildPuzzleCore in clusterCore.m_childCores)
            {
                ResetBioscanChild(clusterChildPuzzleCore);
            }
            
            clusterCore.Deactivate();
            return;
        }
            
        var spline = bioscanCore.m_spline.Cast<CP_Holopath_Spline>();
        spline.SetSplineProgress(0);
        
        var scanner = bioscanCore.PlayerScanner.Cast<CP_PlayerScanner>();
        scanner.ResetScanProgression(0.0f);
        scanner.StopScan();

        var chainedTerminal = bioscanCore.gameObject.GetComponent<ChainedPuzzleTerminal>();
        
        if (chainedTerminal)
        {
            chainedTerminal.Terminal.TrySyncSetCommandHidden(ChainedPuzzleTerminal.ChainedPuzzleCommand);
        }
        
        bioscanCore.Deactivate();
    }

    private void ResetBioscan(ChainedPuzzleInstance instance, ExtraChainedPuzzleSettings.TimedBioscan def)
    {
        foreach (var puzzleCore in instance.m_chainedPuzzleCores)
        {
            ResetBioscanChild(puzzleCore);
        }
        
        if (SNet.IsMaster)
        {
            var oldState = instance.m_stateReplicator.State;
            var newState = new pChainedPuzzleState()
            {
                status = eChainedPuzzleStatus.Disabled,
                currentSurvivalWave_EventID = oldState.currentSurvivalWave_EventID,
                isSolved = false,
                isActive = false,
            };
            instance.m_stateReplicator.InteractWithState(newState, new() { type = eChainedPuzzleInteraction.Deactivate });
            
            var door = instance.m_parent.GetComponent<LG_SecurityDoor>();
            pDoorState state = new pDoorState
            {
                hasBeenOpenedDuringGame = false,
                status = eDoorStatus.Closed_LockedWithChainedPuzzle_Alarm,
                animProgress = 0
            };
            door.OnSyncDoorStatusChange(state, true);
            door.AttemptOpenCloseInteraction(true);
        }
        
        if (!def.StopAlarmOnTimeExpired)
        {
            return;
        }
                
        WardenObjectiveManager.StopAlarms();
        WardenObjectiveManager.StopAllWardenObjectiveEnemyWaves();
    }
}