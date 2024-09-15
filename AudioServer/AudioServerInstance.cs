using Godot;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

[GlobalClass, Tool]
public partial class AudioServerInstance : Node
{
    private FrozenDictionary<Sounds, Sound> sounds;
    public AudioServerInstance() { } // needed for global classes
    public AudioServerInstance(IEnumerable<Sounds> sounds)
    {
        soundsToLoad = [.. sounds];
    }

    public override void _Ready()
    {
        AddToGroup("audio_server");
        AddPlayersAsChildren();
    }

    /// <summary>
    /// Stops the playback of all sounds
    /// </summary>
    public void StopAllSounds()
    {
        foreach (var sound in sounds.Values)
            sound.Stop();
    }

    /// <summary>
    /// Either calls Play() or Stop() on a specific sound
    /// </summary>
    /// <param name="soundToToggle">The sound to toggle</param>
    public void Toggle(Sounds soundToToggle)
    {
        if (!sounds.TryGetValue(soundToToggle, out Sound value))
        {
            Debug.Print("Tried toggling, but sound '" + soundToToggle.ToString() + "' was not loaded or does not exist");
            return;
        }
        value.Toggle();
    }

    /// <summary>
    /// En-/disable looping for a sound
    /// </summary>
    /// <param name="soundToToggleLooping">Which sound to toggle the looping status of</param>
    public void ToggleLooping(Sounds soundToToggleLooping)
    {
        if (!sounds.TryGetValue(soundToToggleLooping, out Sound value))
        {
            Debug.Print("Tried toggling, but sound '" + soundToToggleLooping.ToString() + "' was not loaded or does not exist");
            return;
        }
        value.ToggleLooping();
    }

    /// <summary>
    /// Stops the sound from playing
    /// </summary>
    /// <param name="soundToStop">Which sound to stop</param>
    public void Stop(Sounds soundToStop)
    {
        if (!sounds.TryGetValue(soundToStop, out Sound value))
        {
            Debug.Print("Tried stopping, but sound '" + soundToStop.ToString() + "' was not loaded or does not exist");
            return;
        }
        value.Stop();
    }

    /// <summary>
    /// For usage from GDScript in calls like <br/>
    /// <c>audioServerInstance.Play(GetSoundByName("Bloop"))</c> <br/> <br/>
    /// Ignores the case of the name so these two:
    /// <code>
    ///     GetSoundByName("abcdefg"); 
    ///     GetSoundByName("AbCDefG");
    /// </code> will return the same thing.
    /// </summary>
    /// <param name="name">Name of the sound to get the Enum value for</param>
    /// <returns>The value of the enum "Sounds" with the name "name". Returns -1 (cast to Sounds) if the sound does not exist (and prints a message to the console)</returns>
    #pragma warning disable CA1822 // disable "Mark members as static"
    public Sounds GetSoundByName(string name)
    #pragma warning restore CA1822
    {
        Sounds sound = (Sounds)(-1);
        try
        {
            sound = (Sounds)Enum.Parse(typeof(Sounds), name, true);
        }
        catch
        {
            Debug.Print("Tried getting sound '" + name + "', but no such sound could be found or Enum.Parse failed");
        }
        return sound;
    }

    /// <summary>
    /// Plays a sound
    /// </summary>
    /// <param name="soundToPlay">Which sound to play</param>
    /// <param name="fromPosition">(optional) From where to start playback (in seconds)</param>
    public void Play(Sounds soundToPlay, float fromPosition = 0)
    {
        if (!sounds.TryGetValue(soundToPlay, out Sound value))
        {
            Debug.Print("Tried playing, but sound '" + soundToPlay.ToString() + "' was not loaded or does not exist");
            return;
        }
        value.Play(fromPosition);
    }

    private void AddPlayersAsChildren()
    {
        sounds = AudioServer.GetSoundList(soundsToLoad
                    .Distinct() // remove duplicates and dummy sound
                    .Where((s) => s != Sounds._PickSound_));

        foreach (var sound in sounds.Values)
                AddChild(sound.player);
    }

    /// <summary>
    /// Sets the master volume of every loaded sound to volume
    /// </summary>
    /// <param name="volume">The new volume</param>
    public void SetLinearVolumeMaster(float volume)
    {
        foreach(var sound in sounds.Values)
            sound.SetMasterLinearVolume(volume);
        AudioServer.SetLinearVolumeMaster(volume);
    }

    /// <summary>
    /// Sets the individual volume of the sound given by its identifier
    /// </summary>
    /// <param name="volume">The new volume</param>
    /// <param name="sound">The sound</param>
    public void SetLinearVolume(float volume, Sounds sound)
    {
        if (!sounds.TryGetValue(sound, out Sound value))
        {
            Debug.Print("Sound '" + sound.ToString() + "' was not loaded or does not exist, so its volume can't be set");
            return;
        }
        value.SetSelfLinearVolume(volume);
        AudioServer.SetLinearVolume(volume, sound);
    }

    /// <summary>
    /// Sets the the category volume of every sound of the given category to volume
    /// </summary>
    /// <param name="volume">The new volume</param>
    /// <param name="tag">The category of sounds</param>
    public void SetLinearVolumeTagged(float volume, SoundTags tag)
    {
        foreach(var sound in sounds.Values)
            if(sound.Is(tag))
                sound.SetTagLinearVolume(volume);
        AudioServer.SetLinearVolumeTagged(volume, tag);
    }
}
