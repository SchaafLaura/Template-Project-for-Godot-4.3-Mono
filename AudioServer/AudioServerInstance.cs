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

    public void Toggle(Sounds soundToToggle)
    {
        if (!sounds.TryGetValue(soundToToggle, out Sound value))
        {
            Debug.Print("Tried toggling, but sound '" + soundToToggle.ToString() + "' was not loaded or does not exist");
            return;
        }
        value.Toggle();
    }

    public void ToggleLooping(Sounds soundToToggleLooping)
    {
        if (!sounds.TryGetValue(soundToToggleLooping, out Sound value))
        {
            Debug.Print("Tried toggling, but sound '" + soundToToggleLooping.ToString() + "' was not loaded or does not exist");
            return;
        }
        value.ToggleLooping();
    }

    public void Stop(Sounds soundToStop)
    {
        if (!sounds.TryGetValue(soundToStop, out Sound value))
        {
            Debug.Print("Tried stopping, but sound '" + soundToStop.ToString() + "' was not loaded or does not exist");
            return;
        }
        value.Stop();
    }

#pragma warning disable CA1822 // Mark members as static
    public Sounds GetSoundByName(string name)
#pragma warning restore CA1822 // Mark members as static
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

    public void Play(Sounds soundToPlay, float fromPosition = 0)
    {
        if (!sounds.TryGetValue(soundToPlay, out Sound value))
        {
            Debug.Print("Tried playing, but sound '" + soundToPlay.ToString() + "' was not loaded or does not exist");
            return;
        }
        value.Play(fromPosition);
    }

    public void AddPlayersAsChildren()
    {
        sounds = AudioServer.GetSoundList(soundsToLoad
                    .Distinct() // remove duplicates and dummy sound
                    .Where((s) => s != Sounds._PickSound_));

        foreach (var sound in sounds.Values)
                AddChild(sound.player);
    }

    public void SetLinearVolumeMaster(float volume)
    {
        foreach(var sound in sounds.Values)
            sound.SetMasterLinearVolume(volume);
    }

    public void SetLinearVolume(float volume, Sounds sound)
    {
        if (!sounds.TryGetValue(sound, out Sound value))
        {
            Debug.Print("Sound '" + sound.ToString() + "' was not loaded or does not exist, so its volume can't be set");
            return;
        }
        value.SetSelfLinearVolume(volume);
    }

    public void SetLinearVolumeTagged(float volume, SoundTags tag)
    {
        foreach(var sound in sounds.Values)
            if(sound.Is(tag))
                sound.SetTagLinearVolume(volume);
    }
}
