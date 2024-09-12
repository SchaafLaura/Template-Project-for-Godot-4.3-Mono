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
        soundsToLoad = new();
        foreach(var s in sounds)
            soundsToLoad.Add(s);
    }

    public override void _Ready()
    {
        AddToGroup("audio_server");
        AddPlayersAsChildren();
    }

    public void Toggle(Sounds soundToToggle)
    {
        if (!sounds.ContainsKey(soundToToggle))
        {
            Debug.Print("Tried toggling, but sound '" + soundToToggle.ToString() + "' was not loaded or does not exist");
            return;
        }
        //sounds[soundToToggle].Play(fromPosition);
        sounds[soundToToggle].Toggle();
    }

    public void ToggleLooping(Sounds soundToToggleLooping)
    {
        if (!sounds.ContainsKey(soundToToggleLooping))
        {
            Debug.Print("Tried toggling, but sound '" + soundToToggleLooping.ToString() + "' was not loaded or does not exist");
            return;
        }

        sounds[soundToToggleLooping].ToggleLooping();

    }

    public void Stop(Sounds soundToStop)
    {
        if (!sounds.ContainsKey(soundToStop))
        {
            Debug.Print("Tried stopping, but sound '" + soundToStop.ToString() + "' was not loaded or does not exist");
            return;
        }
        sounds[soundToStop].Stop();
    }

    public Sounds GetSoundByName(string name)
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
        if (!sounds.ContainsKey(soundToPlay))
        {
            Debug.Print("Tried playing, but sound '" + soundToPlay.ToString() + "' was not loaded or does not exist");
            return;
        }
        sounds[soundToPlay].Play(fromPosition);
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
        if (!sounds.ContainsKey(sound))
        {
            Debug.Print("Sound '" + sound.ToString() + "' was not loaded or does not exist, so its volume can't be set");
            return;
        }
        sounds[sound].SetSelfLinearVolume(volume);
    }

    public void SetLinearVolumeTagged(float volume, SoundTags tag)
    {
        foreach(var sound in sounds.Values)
            if(sound.Is(tag))
                sound.SetTagLinearVolume(volume);
    }
}
