using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Godot;

public enum FlightResult { NONE, SUCCEEDED, FAILED, ABORTED }

public struct SkillDefinition
{
    public SkillDefinition(float initialValue, float incrementPerUpgrade, int maxUpgrades, int intialCost, int costIncrement = 0)
    {
        InitialValue = initialValue;
        IncrementPerUpgrade = incrementPerUpgrade;
        MaxUpgrades = maxUpgrades;
        InitialCost = intialCost;
        CostIncrement = costIncrement;
    }

    public float InitialValue { get; }
    public float IncrementPerUpgrade { get; }
    public int MaxUpgrades { get; }
    public int InitialCost { get; }
    public int CostIncrement { get; }

    public float Value(int upgrades) => InitialValue + upgrades * IncrementPerUpgrade;
    public int NextCost(int upgrades) => InitialCost + upgrades * CostIncrement;
}

public class SkillState
{
    private readonly SkillDefinition definition;
    private readonly Func<float, string> valueFormatter;
    private int upgrades = 0;

    public SkillState(SkillDefinition definition, Func<float, string> valueFormatter)
    {
        this.definition = definition;
        this.valueFormatter = valueFormatter;
    }

    public float Value => definition.Value(upgrades);
    public float NextValue => definition.Value(upgrades + 1);
    public int NextCost => definition.NextCost(upgrades);
    public bool CanUpgrade => upgrades < definition.MaxUpgrades;
    public float UpgradeProgress => (float)(upgrades) / definition.MaxUpgrades;

    public void Upgrade() => upgrades++;
    public string Format(float value) => valueFormatter(value);
}

public class Global : Node
{
    private const int DEFAULT_SCORE = -1;

    private const string SAVE_DIRECTORY = "user://";
    private const string SAVE_FILE_PREFIX = "save-";
    private const string SAVE_FILE_SUFFIX = ".json";

    private const string SETTINGS_SAVE_FILE = "settings";
    private const string DEFAULT_SAVE_FILE = "default";

    private const string GAME_DATA_FILE = "gamedata";
    private const string SCORES_KEY = "scores";

    private const string VOLUME_KEY = "volume";
    private const string QUALITY_KEY = "quality";

    private static List<int> bestScores = new List<int>();

    private const int NO_LEVEL = 0;
    private static int currentLevel = NO_LEVEL;
    public static int CurrentLevel { get => currentLevel; set => currentLevel = value; }

    public static string CurrentLevelPath { get => GetLevelScenePath(currentLevel); }

    private static GameSettings settings;
    public static GameSettings Settings { get => settings; }

    public static bool ReturningToMenu = false;

    private static readonly Regex LEVEL_NUMBER_REGEX = new Regex(@"(\d+)\.tscn\b", RegexOptions.Compiled);

    public override void _Ready()
    {
        // if (save_file_exists(SETTINGS_SAVE_FILE))
        // {
        //     settings.initialize_from_dictionary(load_data(SETTINGS_SAVE_FILE));
        // }
    }

    public static bool TryWinLevel(int score)
    {
        if (currentLevel <= NO_LEVEL)
        {
            return false;
        }
        while (bestScores.Count < currentLevel)
        {
            bestScores.Add(DEFAULT_SCORE);
        }
        bestScores[currentLevel - 1] = Mathf.Max(bestScores[currentLevel - 1], score);
        SaveGameData();
        // return TryGoToLevel(currentLevel + 1);
        return true;
    }

    public static bool LevelHasBeenCleared(int level)
    {
        return level == 0 || (level <= bestScores.Count && bestScores[level - 1] > DEFAULT_SCORE);
    }

    public static int GetLevelScore(int level)
    {
        return level == 0 || level > bestScores.Count ? DEFAULT_SCORE : bestScores[level - 1];
    }

    public static int GetLastLevel()
    {
        int lastLevel = 0;
        while (LevelExists(lastLevel + 1))
        {
            lastLevel++;
        }
        return lastLevel;
    }

    public static bool TryGoToLevel(int targetLevel)
    {
        if (LevelExists(targetLevel))
        {
            currentLevel = targetLevel;
            return true;
        }
        return false;
    }

    public static bool LevelExists(int level) => new File().FileExists(GetLevelScenePath(level));

    public static string GetLevelScenePath(int level)
    {
        return "res://scenes/levels/Level-" + level + ".tscn";
    }

    public static bool BackgroundExists(int level) => new File().FileExists(GetBackgroundScenePath(level));

    public static string GetBackgroundScenePath(int level)
    {
        return "res://scenes/backgrounds/Background-" + level + ".tscn";
    }

    public static int GetLevelPathNumber(string levelPath)
    {
        Match match = LEVEL_NUMBER_REGEX.Match(levelPath);
        return match.Success ? match.Value.Replace(".tscn", "").ToInt() : -1;
    }

    public static void SaveGameData()
    {
        var data = new Dictionary<string, object>();
        data.Add(SCORES_KEY, bestScores);
        SaveData(GAME_DATA_FILE, data);
    }

    public static bool LoadGameData()
    {
        var data = LoadData(GAME_DATA_FILE);
        if (data == null)
        {
            return false;
        }
        object rawBestScores = data.Contains(SCORES_KEY) ? data[SCORES_KEY] : null;
        bestScores = rawBestScores == null
            ? new List<int>()
            : godotArrayToIntList((Godot.Collections.Array)rawBestScores);
        return true;
    }

    private static List<int> godotArrayToIntList(Godot.Collections.Array array)
    {
        List<int> result = new List<int>(array.Count);
        foreach (object element in array)
        {
            result.Add(Convert.ToInt32(element));
        }
        return result;
    }

    private static Dictionary<string, int> godotDictionaryToDictionary(Godot.Collections.Dictionary dictionary, Dictionary<string, int> fallback)
    {
        if (dictionary == null)
        {
            return fallback;
        }
        Dictionary<string, int> result = new Dictionary<string, int>(dictionary.Count);
        foreach (object key in dictionary.Keys)
        {
            result[key.ToString()] = Convert.ToInt32(dictionary[key]);
        }
        return result;
    }

    public static void SaveSettings()
    {
        var data = new Dictionary<string, object>();
        data.Add(VOLUME_KEY, settings.Volume);
        data.Add(QUALITY_KEY, (int)settings.Quality);
        SaveData(SETTINGS_SAVE_FILE, data);
    }

    public static void LoadSettings()
    {
        if (settings != null)
        {
            return;
        }
        var data = LoadData(SETTINGS_SAVE_FILE);
        settings = new GameSettings();
        if (data == null)
        {
            GD.Print("No data for settings could be loaded");
            return;
        }

        // Generally ignoring exceptions like this is a bad idea, but keys not being present is to be expected;

        try { settings.Volume = godotDictionaryToDictionary((Godot.Collections.Dictionary)data[VOLUME_KEY], settings.Volume); }
        catch (KeyNotFoundException) { }

        try { settings.Quality = (GameQuality)(Convert.ToInt32(data[QUALITY_KEY])); }
        catch (InvalidCastException)
        {
            GD.PushError(String.Format("Failed to cast the raw quality value '{0}' to a GameQuality enum", data["quality"]));
        }
        catch (KeyNotFoundException) { }
    }


    private static void SaveData(string save_name, Dictionary<string, object> data)
    {
        var path = GetUserJsonFilePath(save_name);
        // LOG.info("Saving data to: %s" % path);
        var file = new File();
        var openError = file.Open(path, File.ModeFlags.Write);
        if (openError != 0)
        {
            GD.Print("Open of ", path, " error: ", openError);
            return;
        }
        // LOG.check_error_code(file.open(path, File.WRITE), "Opening '%s'" % path);
        // LOG.info("Saving to: " + file.get_path_absolute());
        file.StoreString(JSON.Print(data));
        file.Close();
    }

    private string[] GetSaveFiles()
    {
        var dir = OpenSaveDirectory();
        dir.ListDirBegin(false, false);
        // LOG.check_error_code(dir.list_dir_begin(false, false), "Listing the files of " + SAVE_DIRECTORY);
        var file_name = dir.GetNext();

        List<string> result = new List<string>();
        while (file_name != "")
        {
            if (!dir.CurrentIsDir() && file_name.StartsWith(SAVE_FILE_PREFIX)
                    && file_name.EndsWith(SAVE_FILE_SUFFIX))
            {
                result.Add(file_name.Substr(SAVE_FILE_PREFIX.Length,
                    file_name.Length - SAVE_FILE_PREFIX.Length - SAVE_FILE_SUFFIX.Length));
            }
            file_name = dir.GetNext();
        }
        dir.ListDirEnd();

        result.Sort();
        return result.ToArray();
    }

    Node GetSingleNodeInGroup(string group)
    {
        Godot.Collections.Array nodes = GetTree().GetNodesInGroup(group);
        return nodes.Count > 0 ? (Node)nodes[0] : null;
    }

    private Directory OpenSaveDirectory()
    {
        var dir = new Directory();
        // LOG.check_error_code(dir.open(SAVE_DIRECTORY), "Opening " + SAVE_DIRECTORY);
        return dir;
    }

    private bool LoadGame(string save_name = DEFAULT_SAVE_FILE)
    {
        if (!SaveFileExists(save_name))
        {
            return false;
        }
        var loaded_data = LoadData(save_name);
        if (loaded_data.Count == 0)
        {
            return false;
        }
        var game_data = loaded_data;
        return true;
    }

    private static bool SaveFileExists(string save_name)
    {
        var path = GetUserJsonFilePath(save_name);
        return new File().FileExists(path);
    }

    private static Godot.Collections.Dictionary LoadData(string fileName)
    {
        var path = GetUserJsonFilePath(fileName);
        var file = new File();
        if (!file.FileExists(path))
        {
            return null;
        }
        file.Open(path, File.ModeFlags.Read);
        // LOG.check_error_code(file.open(path, File.READ), "Opening file " + path);
        var raw_data = file.GetAsText();
        file.Close();
        var loaded_data = JSON.Parse(raw_data);
        if (loaded_data.Result != null && loaded_data.Result is Godot.Collections.Dictionary)
        {
            return (Godot.Collections.Dictionary)loaded_data.Result;
        }
        else
        {
            GD.PushWarning(String.Format("Corrupted data in file '{0}'!", path));
            return null;
        }
    }

    private static string GetUserJsonFilePath(string save_name)
    {
        return SAVE_DIRECTORY + save_name + ".json";
    }
}

public class GameSettings
{
    public static string MASTER_BUS_NAME = "Master";
    private static int MIN_DB = -60;
    private static int MAX_DB = 6;

    public Dictionary<string, int> Volume = new Dictionary<string, int>() {
        {MASTER_BUS_NAME, 50},
        {"SFX", 50},
        {"Music", 50},
    };

    public int MasterVolume = 20;
    public float NormalizedMasterVolume { get => MasterVolume * .01f; }

    public int SfxVolume = 80;
    public float NormalizedSfxVolume { get => SfxVolume * .01f; }

    public int MusicVolume = 80;
    public float NormalizedMusicVolume { get => MusicVolume * .01f; }

    public GameQuality Quality = GameQuality.MEDIUM;

    public static float PercentageToDb(int percentage) => PercentageToDb((float)percentage);
    public static float PercentageToDb(double percentage) => PercentageToDb((float)percentage);
    public static float PercentageToDb(float percentage) => Mathf.Lerp(MIN_DB, MAX_DB, percentage * .01f);
}

public enum GameQuality
{
    LOW, MEDIUM, HIGH
}
