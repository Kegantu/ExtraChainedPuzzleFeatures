using BepInEx;
using BepInEx.Unity.IL2CPP;
using ExtraChainedPuzzleFeatures.JsonStructure;
using GTFO.API.Utilities;
using HarmonyLib;
using MTFO.API;
using UnityEngine;

namespace ExtraChainedPuzzleFeatures;

[BepInDependency("com.dak.MTFO")]
[BepInDependency("dev.gtfomodding.gtfo-api")]
[BepInDependency(MTFOPartialUtil.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInPlugin(Identifier, PluginName, PluginVersion)]
public class Main : BasePlugin
{
    private const string Identifier = $"Kegantu.{PluginName}";
    private const string PluginName = "ExtraChainedPuzzleFeatures";
    private const string PluginVersion = "1.0.0";

    private Harmony _harmony;
    
    private static LiveEditListener _listener;
    private static Dictionary<uint, Dictionary<uint, ExtraChainedPuzzleSettings.BioscanFeature>> _bioscansFeatures = new();
    private static Dictionary<uint, Dictionary<uint, ExtraChainedPuzzleSettings.TimedBioscan>> _timedBioscans = new();
    public static readonly string ExtraChainedPuzzleFeaturesPath = Path.Combine(MTFOPathAPI.CustomPath, "PuzzleFeatures");
    
    public override void Load()
    {
        _harmony = new Harmony(Identifier);
        _harmony.PatchAll();
        
        ECPFLogger.Error(ExtraChainedPuzzleFeaturesPath);
        if (!Directory.Exists(ExtraChainedPuzzleFeaturesPath))
        {
            Directory.CreateDirectory(ExtraChainedPuzzleFeaturesPath);
            var file = File.CreateText(Path.Combine(ExtraChainedPuzzleFeaturesPath, "Test.json"));
            file.WriteLine(ECPFJsonSerializer.Serialize(new ExtraChainedPuzzleSettingsJson()));
            file.Flush();
            file.Close(); 
            
            return;
        }

        //Debug
        if (!File.Exists(Path.Combine(ExtraChainedPuzzleFeaturesPath, "Test.json")))
        {
            var file = File.CreateText(Path.Combine(ExtraChainedPuzzleFeaturesPath, "Test.json"));
            file.WriteLine(ECPFJsonSerializer.Serialize(new ExtraChainedPuzzleSettingsJson()));
            file.Flush();
            file.Close();
        }
            
        foreach (var configFile in Directory.EnumerateFiles(ExtraChainedPuzzleFeaturesPath, "*.json", SearchOption.AllDirectories))
        { 
            ECPFJsonSerializer.Load<ExtraChainedPuzzleSettingsJson>(configFile, out var puzzleOverrideConfig);

            if (_bioscansFeatures.ContainsKey(puzzleOverrideConfig.MainLevelLayout))
            {
                ECPFLogger.Warning("Duplicate MainLevelLayout {0}, use only one MainLevelLayout", puzzleOverrideConfig.MainLevelLayout);
                continue;
            }
            
            if (_timedBioscans.ContainsKey(puzzleOverrideConfig.MainLevelLayout))
            {
                ECPFLogger.Warning("TimeBioscan Duplicate MainLevelLayout {0}, use only one MainLevelLayout", puzzleOverrideConfig.MainLevelLayout);
                continue;
            }

            Dictionary<uint, ExtraChainedPuzzleSettings.BioscanFeature> levelPuzzleToOverride = new();
            foreach(var puzzleToOverride in puzzleOverrideConfig.BioscanFeatures)
            {
                levelPuzzleToOverride.TryAdd(puzzleToOverride.BioscanIndex, puzzleToOverride);
            }

            _bioscansFeatures.Add(puzzleOverrideConfig.MainLevelLayout, levelPuzzleToOverride);
            
            Dictionary<uint, ExtraChainedPuzzleSettings.TimedBioscan> chainedPuzzleInstanceToOverride = new();
            foreach(var puzzleToOverride in puzzleOverrideConfig.TimedBioscans)
            {
                chainedPuzzleInstanceToOverride.TryAdd(puzzleToOverride.BioscanInstanceIndex, puzzleToOverride);
            }

            _timedBioscans.Add(puzzleOverrideConfig.MainLevelLayout, chainedPuzzleInstanceToOverride);
        }
        
        _listener = LiveEdit.CreateListener(ExtraChainedPuzzleFeaturesPath, "*.json", includeSubDir: true);
        _listener.FileChanged += OnFileChanged;
    }

    public static ExtraChainedPuzzleSettings.BioscanFeature GetCorruptedBioscan(uint mainLevelLayout, uint puzzleIndex)
    {
        if (!_bioscansFeatures.ContainsKey(mainLevelLayout)) return null;

        var levelPuzzleToOverride = _bioscansFeatures[mainLevelLayout];

        if (!levelPuzzleToOverride.ContainsKey(puzzleIndex)) return null;

        return levelPuzzleToOverride[puzzleIndex];
    }
    
    public static ExtraChainedPuzzleSettings.TimedBioscan GetBioscanInstance(uint mainLevelLayout, uint instanceIndex)
    {
        if (!_timedBioscans.ContainsKey(mainLevelLayout)) return null;

        var levelPuzzleToOverride = _timedBioscans[mainLevelLayout];

        if (!levelPuzzleToOverride.ContainsKey(instanceIndex)) return null;

        return levelPuzzleToOverride[instanceIndex];
    }
    
    private void OnFileChanged(LiveEditEventArgs editEventArgs)
    {
        LiveEdit.TryReadFileContent(editEventArgs.FullPath, content =>
        {
            var overrideConfig = ECPFJsonSerializer.Deserialize<ExtraChainedPuzzleSettingsJson>(content);
            if(!_bioscansFeatures.TryGetValue(overrideConfig.MainLevelLayout, out var levelPuzzleToOverride))
            {
                return;
            }
            
            if(!_timedBioscans.TryGetValue(overrideConfig.MainLevelLayout, out var chainedPuzzleToOverride))
            {
                return;
            }

            levelPuzzleToOverride.Clear();
            chainedPuzzleToOverride.Clear();
        });
    }
}