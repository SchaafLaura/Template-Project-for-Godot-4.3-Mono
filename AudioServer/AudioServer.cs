using Godot;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics;

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
        { Sounds.Bleep,         ("res://AudioServer/sfx/bleep.wav",       1,      SoundTags.General)          },
        { Sounds.Bloop,         ("res://AudioServer/sfx/bloop.wav",       1,      SoundTags.General)          },
        { Sounds.Explosion,     ("res://AudioServer/sfx/explosion.wav",   1,      SoundTags.Detonations)      },
        { Sounds.Mrrp,          ("res://AudioServer/sfx/mrrrrp.wav",      1,      SoundTags.General)          },
        { Sounds.ExplosionB,    ("res://AudioServer/sfx/explosionB.wav",  1,      SoundTags.Detonations)      },
        { Sounds.ShortSong,     ("res://AudioServer/music/shortMusic.mp3",1,      SoundTags.Music)            },

    }.ToFrozenDictionary();

    private static readonly FrozenDictionary<SoundLists, Sounds[]> soundLists = new Dictionary<SoundLists, Sounds[]>()
    {
        // list identifier             sound list
        { SoundLists.Menu,      new Sounds[]{ Sounds.Bleep, Sounds.Bloop,  }},
        { SoundLists.Sfx,       new Sounds[]{ Sounds.Explosion, Sounds.ExplosionB }},
        { SoundLists.Player,    new Sounds[]{ Sounds.Mrrp,  }},
        { SoundLists.Music,     new Sounds[]{ Sounds.ShortSong, }},

    }.ToFrozenDictionary();

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
            soundList.Add(sound, GetSoundResource(tag, resourceLocation, polyphony));
        }
        return soundList.ToFrozenDictionary(); 
    }

    private static Sound GetSoundResource(
        SoundTags tag, string resourceLocation = null, int polyphony = 1)
    {
        var stream = resourceLocation is null ? null : GD.Load(resourceLocation);
        return new Sound(stream, polyphony, tag);
    }
}