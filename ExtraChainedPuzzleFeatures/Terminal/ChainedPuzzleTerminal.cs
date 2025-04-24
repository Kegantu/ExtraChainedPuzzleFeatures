using ChainedPuzzles;
using ExtraChainedPuzzleFeatures.Patches;
using FluffyUnderware.DevTools.Extensions;
using Il2CppInterop.Runtime.Injection;
using LevelGeneration;
using Localization;
using SNetwork;
using UnityEngine;

namespace ExtraChainedPuzzleFeatures.Terminal;

public class ChainedPuzzleTerminal : MonoBehaviour
{
    static ChainedPuzzleTerminal()
    {
        ClassInjector.RegisterTypeInIl2Cpp<ChainedPuzzleTerminal>();
    }
    
    public const TERM_Command ChainedPuzzleCommand = (TERM_Command)200;
    
    public iChainedPuzzleCore Core { get; set; }
    public LG_ComputerTerminal Terminal { get; set; }
    public LG_ComputerTerminalCommandInterpreter Command { get; set; }
    
    public void Setup(LG_ComputerTerminal terminal, LG_ComputerTerminalCommandInterpreter command, iChainedPuzzleCore bioscan)
    {
        Terminal = terminal;
        Command = command;
        Core = bioscan;
        
        PatchLG_ComputerTerminalCommandInterpreter.CustomOnReceiveCommand += OnReceiveCommand;
        
        UnHide();
        
        AddCommand(ChainedPuzzleCommand, "COMPLETE_BIOSCAN", "", TERM_CommandRule.OnlyOnceDelete);
    }

    public void UnHide()
    {
        if (Command.HasRegisteredCommand(ChainedPuzzleCommand))
        {
            Terminal.TrySyncSetCommandShow(ChainedPuzzleCommand);
        }
    }
    
    private void AddCommand(TERM_Command termCommand, string command, string commandDescription, TERM_CommandRule rule)
    {
        var localizedText = new LocalizedText
        {
            UntranslatedText = commandDescription,
            Id = 0u
        };
        
        Command.AddCommand(termCommand, command, localizedText, rule);
    }
    
    private void OnReceiveCommand(LG_ComputerTerminalCommandInterpreter interpreter, TERM_Command cmd, string inputLine, string param1, string param2)
    {
        if (interpreter.m_terminal.m_syncID != Terminal.m_syncID)
        {
            return;
        }

        if (cmd != ChainedPuzzleCommand)
        {
            return;
        }

        var bioscan = Core.TryCast<CP_Bioscan_Core>();
        
        if (!bioscan || !bioscan.Owner.TryCast<ChainedPuzzleInstance>())
        {
            CP_Cluster_Core clusterOwner = !bioscan ? Core.TryCast<CP_Cluster_Core>() : bioscan.Owner.TryCast<CP_Cluster_Core>();

            foreach (var core in clusterOwner.m_childCores)
            {
                var scanner = core.TryCast<CP_Bioscan_Core>().gameObject.GetComponent<CP_PlayerScanner>();
                scanner.m_reduceWhenNoPlayer = true;
                scanner.m_scanProgression = 2;
            }
            return;
        }
        
        //interpreter.AddOutput(TerminalLineType.ProgressWait, "Wait", 5f);
        
        var playerScanner = bioscan.GetComponent<CP_PlayerScanner>();
        
        playerScanner.m_reduceWhenNoPlayer = true;
        playerScanner.m_scanProgression = 2;
    }
}