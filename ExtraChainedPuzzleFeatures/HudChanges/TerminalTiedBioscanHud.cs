using ChainedPuzzles;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.Text;
using LevelGeneration;
using UnityEngine;

namespace ExtraChainedPuzzleFeatures.HudChanges;

public class TerminalTiedBioscanHud : MonoBehaviour
{
    static TerminalTiedBioscanHud()
    {
        ClassInjector.RegisterTypeInIl2Cpp<TerminalTiedBioscanHud>();
    }
    
    private CP_Bioscan_Hud Hud { get; set; }
    private LG_ComputerTerminal TiedTerminal { get; set; }
    private CP_Bioscan_Core Core { get; set; }

    private StringBuilder _text = new();

    public void Setup(iChainedPuzzleHUD hud, LG_ComputerTerminal terminal)
    {
        TiedTerminal = terminal;

        var bioscanHud = hud.TryCast<CP_Bioscan_Hud>();

        if (!bioscanHud)
        {
            var clusterHud = hud.TryCast<CP_Cluster_Hud>();
            bioscanHud = clusterHud.m_hud.TryCast<CP_Bioscan_Hud>();
        }

        Hud = bioscanHud;
    }
    
    private void LateUpdate()
    {
        if (!Hud.m_visible)
        {
            return;
        }
        
        _text.Clear();
        _text.Append("<color=red>");

        _text.Append("Complete bioscan on ").Append("</color=red>");
        
        _text.Append("<color=yellow>").Append(TiedTerminal.PublicName);
        Hud.m_msgCharBuffer.Set(_text.ToString());
        GuiManager.InteractionLayer.SetMessage(Hud.m_msgCharBuffer, ePUIMessageStyle.BioscanAlarm, 0);
    }
    
    private void OnDestroy()
    {
        Hud = null;
        TiedTerminal = null;
        Core = null;
        _text = null;
    }

}