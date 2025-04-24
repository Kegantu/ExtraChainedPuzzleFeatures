using System.Text.Json;
using System.Text.Json.Serialization;
using ExtraChainedPuzzleFeatures.JsonStructure;
using GTFO.API.JSON.Converters;

namespace ExtraChainedPuzzleFeatures.JsonStructure;

public static class ECPFJsonSerializer
{
    private static readonly JsonSerializerOptions _setting = new()
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            IncludeFields = false,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            IgnoreReadOnlyProperties = true
        };

        static ECPFJsonSerializer()
        {
            _setting.Converters.Add(new JsonStringEnumConverter());
            if (MTFOPartialUtil.IsLoaded && MTFOPartialUtil.Initialized)
            {
                _setting.Converters.Add(MTFOPartialUtil.PersistentIDConverter);
                _setting.Converters.Add(MTFOPartialUtil.LocalizedTextConverter);
                ECPFLogger.Log("PartialData Support Found!");
            }
            else
            {
                _setting.Converters.Add(new TextConverter());
            }
        }

        public static string Serialize<T>(T value)
        {
            return JsonSerializer.Serialize(value, _setting);
        }

        public static void Load<T>(string file, out T config) where T : new()
        {
            if (file.Length < ".json".Length)
            {
                config = default;
                return;
            }

            if (file.Substring(file.Length - ".json".Length) != ".json")
            {
                file += ".json";
            }

            var filePath = Path.Combine(Main.ExtraChainedPuzzleFeaturesPath, file);

            file = File.ReadAllText(filePath);
            config = Deserialize<T>(file);
        }
        
        public static T Deserialize<T>(string json)
        {
            var jsonObject = JsonSerializer.Deserialize<T>(json, _setting);
            
            if (jsonObject == null)
            {
                ECPFLogger.Error("JsonObject is null");
                return jsonObject;
            }
            
            return jsonObject;
        }

        public static object Deserialize(Type type, string json)
        {
            var jsonObject = JsonSerializer.Deserialize(json, type, _setting);

            if (jsonObject == null)
            {
                ECPFLogger.Error("JsonObject is null");
                return jsonObject;
            }
            
            return jsonObject;
        }
}