namespace ExtraChainedPuzzleFeatures.JsonStructure;

internal sealed class ExtraChainedPuzzleSettingsJson
{
    public uint MainLevelLayout { get; set; }

    public List<ExtraChainedPuzzleSettings.BioscanFeature> BioscanFeatures { get; set; } = new() { new() };
    
    public List<ExtraChainedPuzzleSettings.TimedBioscan> TimedBioscans { get; set; } = new() { new() };
}