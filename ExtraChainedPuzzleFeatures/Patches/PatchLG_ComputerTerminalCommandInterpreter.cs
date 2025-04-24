using GTFO.API;
using HarmonyLib;
using LevelGeneration;

namespace ExtraChainedPuzzleFeatures.Patches;

[HarmonyPatch]
public class PatchLG_ComputerTerminalCommandInterpreter
{
    public static event Action<LG_ComputerTerminalCommandInterpreter, TERM_Command, string, string, string> CustomOnReceiveCommand;

    static PatchLG_ComputerTerminalCommandInterpreter()
    {
        LevelAPI.OnLevelCleanup += delegate
        {
            CustomOnReceiveCommand = null;
        };
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(LG_ComputerTerminalCommandInterpreter), nameof(LG_ComputerTerminalCommandInterpreter.ReceiveCommand))]
    private static void ReceiveCommand(LG_ComputerTerminalCommandInterpreter __instance, TERM_Command cmd, string inputLine, string param1, string param2)
    {
        CustomOnReceiveCommand?.Invoke(__instance, cmd, inputLine, param1, param2);
    }
}