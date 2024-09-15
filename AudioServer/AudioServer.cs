using Godot;
using Newtonsoft.Json;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public enum Sounds
{
    Bloop           = 0,
    Bleep           = 1,
    Explosion       = 2,
    Mrrp            = 3,
    ExplosionB      = 4,
    ShortSong       = 5,

    _PickSound_     = int.MaxValue, // leave this untouched for nice editor adding
}

public enum SoundLists
{
    Menu            = 0,
    Sfx             = 1,
    Player          = 2,
    Music           = 3,

    _PickCategory_  = int.MaxValue, // leave this untouched for nice editor adding
}

public enum SoundTags
{
    General         = 0,
    Detonations     = 1,
    Music           = 2,
}

public static partial class AudioServer
{
    private static readonly FrozenDictionary<Sounds, (string resourceLocation, int polyphony, SoundTags tag)> soundDict = new Dictionary<Sounds, (string resourceLocation, int polyphony, SoundTags tag)>()
    {
        // sound identifier         resource location           polyphony           tag
        { Sounds.Bleep,         ("res://AudioServer/sfx/bleep.wav",        1,      SoundTags.General)       },
        { Sounds.Bloop,         ("res://AudioServer/sfx/bloop.wav",        1,      SoundTags.General)       },
        { Sounds.Explosion,     ("res://AudioServer/sfx/explosion.wav",    1,      SoundTags.Detonations)   },
        { Sounds.Mrrp,          ("res://AudioServer/sfx/mrrrrp.wav",       1,      SoundTags.General)       },
        { Sounds.ExplosionB,    ("res://AudioServer/sfx/explosionB.wav",   1,      SoundTags.Detonations)   },
        { Sounds.ShortSong,     ("res://AudioServer/music/shortMusic.mp3", 1,      SoundTags.Music)         },

    }.ToFrozenDictionary();

    private static readonly FrozenDictionary<SoundLists, Sounds[]> soundLists = new Dictionary<SoundLists, Sounds[]>()
    {
        // list identifier             sound list
        { SoundLists.Menu,      new Sounds[]{ Sounds.Bleep, Sounds.Bloop, }},
        { SoundLists.Sfx,       new Sounds[]{ Sounds.Explosion, Sounds.ExplosionB, }},
        { SoundLists.Player,    new Sounds[]{ Sounds.Mrrp, }},
        { SoundLists.Music,     new Sounds[]{ Sounds.ShortSong, }},

    }.ToFrozenDictionary();

    public static Dictionary<Sounds, float> IndividualVolumes { get; private set; } = [];
    public static Dictionary<SoundTags, float> CategoryVolumes { get; private set; } = [];
    public static float MasterVolume { get; private set; } = 1.0f;

    static AudioServer()
    {
        Load();
    }

    public static (float masterVolume, Dictionary<Sounds, float> individualVolumes, Dictionary<SoundTags, float> categoryVolumes) Load()
    {
        var json = File.ReadAllText("soundSettings.json");
        var saveStructure = GetSaveData(empty: true);
        var load = (dynamic) JsonConvert.DeserializeAnonymousType(json, saveStructure)!;

        MasterVolume = (float)load.masterVolume;
        CategoryVolumes = load.categoryVolumes.ToObject<Dictionary<SoundTags, float>>();
        IndividualVolumes = load.individualVolumes.ToObject<Dictionary<Sounds, float>>();
        return (MasterVolume, IndividualVolumes, CategoryVolumes);
    }

    public static void Save(float masterVolume, Dictionary<Sounds, float> individualVolumes, Dictionary<SoundTags, float> categoryVolumes)
    {
        MasterVolume = masterVolume;
        IndividualVolumes = new(individualVolumes);
        CategoryVolumes = new(categoryVolumes);

        var saveData = GetSaveData();
        var json = JsonConvert.SerializeObject(saveData);
        File.WriteAllText("soundSettings.json", json);
    }

    private static object GetSaveData(bool empty = false)
    {
        return new
        {
            masterVolume        = empty ? 0.0f : MasterVolume,
            categoryVolumes     = empty ? [] : CategoryVolumes,
            individualVolumes   = empty ? [] : IndividualVolumes,
        };
    }

    public static void SetLinearVolumeMaster(float volume)
    {
        MasterVolume = volume;
    }

    public static void SetLinearVolume(float volume, Sounds sound)
    {
        if (!IndividualVolumes.ContainsKey(sound))
        {
            Debug.Print("Sound '" + sound.ToString() + "' was not loaded or does not exist, so its volume can't be set (from static audioserver)");
            return;
        }
        IndividualVolumes[sound] = volume;
    }

    public static void SetLinearVolumeTagged(float volume, SoundTags tag)
    {
        if (!CategoryVolumes.ContainsKey(tag))
        {
            Debug.Print("Sound category '" + tag.ToString() + "' was not loaded or does not exist, so its volume can't be set (from static audioserver)");
            return;
        }
        CategoryVolumes[tag] = volume;
    }

    public static Sounds[] GetSoundsFromCategory(SoundLists category)
    {
        if (!soundLists.TryGetValue(category, out var ret))
        {
            Debug.Print("Tried getting sounds, but the category '" + category.ToString() + "' doesn't exist or doesn't have an entry in 'soundLists' in the (static) AudioServer");
            return [];
        }
        return ret;
    }

    public static FrozenDictionary<Sounds, Sound> GetSoundList(IEnumerable<Sounds> sounds)
    {
        var soundList = new Dictionary<Sounds, Sound>();
        foreach(var sound in sounds)
        {
            var soundName = "'" + sound.ToString() + "'";
            if (!soundDict.ContainsKey(sound))
            {
                Debug.Print("Tried loading " + soundName + " but it either doesn't exist or wasn't yet setup in the (static) AudioServer.");
                continue;
            }

            if (soundList.ContainsKey(sound))
            {
                Debug.Print("The sound " + soundName + " was already added. duplicate in 'SoundsToAdd'. Nothing *should* break, but you should remove the duplicate entry in your AudioServerInstance.");
                continue;
            }

            var (resourceLocation, polyphony, tag) = soundDict[sound];
            soundList.Add(sound, GetSoundResource(sound, tag, resourceLocation, polyphony));
        }
        return soundList.ToFrozenDictionary(); 
    }

    private static Sound GetSoundResource(
        Sounds sound, SoundTags tag, string resourceLocation = null, int polyphony = 1)
    {
        var stream = resourceLocation is null ? null : GD.Load(resourceLocation);
        var ret = new Sound(stream, polyphony, tag);
        ret.SetSelfLinearVolume(IndividualVolumes[sound]);
        ret.SetTagLinearVolume(CategoryVolumes[tag]);
        ret.SetMasterLinearVolume(MasterVolume);
        return ret;
    }
}