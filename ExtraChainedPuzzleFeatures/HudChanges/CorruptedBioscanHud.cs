using ChainedPuzzles;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.Text;
using UnityEngine;

namespace ExtraChainedPuzzleFeatures.HudChanges;

public class CorruptedBioscanHud : MonoBehaviour
{
    static CorruptedBioscanHud()
    {
        ClassInjector.RegisterTypeInIl2Cpp<CorruptedBioscanHud>();
    }
    
    public CP_Bioscan_Hud Hud { get; set; }
    
    private StringBuilder _text = new();
    
    private void LateUpdate()
    {
        if (!Hud.m_visible)
        {
            return;
        }
        
        _text.Clear();
        _text.AppendLine().Append("<color=red>");

        _text.Append("Corrupted Bioscan Detected, Do Not Leave The Bioscan").Append("</color=red>");
        Hud.m_msgCharBuffer.Add(_text.ToString());
        GuiManager.InteractionLayer.SetMessage(Hud.m_msgCharBuffer, ePUIMessageStyle.BioscanAlarm, 0);
    }
    
    private void OnDestroy()
    {
        Hud = null;
        _text = null;
    }
}