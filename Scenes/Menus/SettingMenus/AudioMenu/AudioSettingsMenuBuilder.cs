using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using System.IO;

public partial class AudioSettingsMenuBuilder : Node2D
{
    [Export]
    private HSlider sliderPrototype;

    [Export]
    private Button buttonPrototype;

    Dictionary<Sounds, float> individualVolumes;
    Dictionary<Sounds, HSlider> individualVolumeSliders = [];
    Dictionary<SoundTags, float> categoryVolumes;
    Dictionary<SoundTags, HSlider> categoryVolumeSliders = [];
    float masterVolume;
    HSlider masterVolumeSlider;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        var allSounds = Enum.GetValues<Sounds>().Where(s => (int)s < int.MaxValue);
        var _audioServer = new AudioServerInstance(allSounds);
        AddChild(_audioServer);

        var soundDict = AudioServer.GetSoundList(allSounds);
        var soundsByTag = new Dictionary<SoundTags, List<(Sounds identifier, Sound sound)>>();
        individualVolumes = [];
        categoryVolumes = [];
        foreach (var pair in soundDict)
        {
            var identifier = pair.Key;
            var sound = pair.Value;
            var tag = sound.tag;

            soundsByTag.TryAdd(tag, new List<(Sounds identifiert, Sound sound)>());
            soundsByTag[tag].Add((identifier, sound));

            individualVolumes.Add(identifier, 1.0f);
            categoryVolumes.TryAdd(tag, 1.0f);
        }
        masterVolume = 1.0f;


        sliderPrototype.Visible = false;
        buttonPrototype.Visible = false;

        AddChild(new Label() { Text = "Master", Position = new Vector2(40, 0) });
        masterVolumeSlider = (HSlider)sliderPrototype.Duplicate();
        masterVolumeSlider.Position = new Vector2(140, 0);
        masterVolumeSlider.Size = new Vector2(100, 10);
        masterVolumeSlider.Visible = true;
        masterVolumeSlider.ValueChanged += (newValue) => { _audioServer.SetLinearVolumeMaster((float)newValue); masterVolume = (float)newValue; };
        AddChild(masterVolumeSlider);

        var y = 50;
        var x = 50;
        foreach (var tag in soundsByTag.Keys)
        {
            var tagAsString = tag.ToString();
            var sounds = soundsByTag[tag];
            var label = new Label() { Text = tagAsString, Position = new Vector2(x, y) };
            AddChild(label);

            var slider = (HSlider)sliderPrototype.Duplicate();
            slider.Position = new Vector2(x + 100, y);
            slider.Size = new Vector2(100, 10);
            slider.Visible = true;
            slider.ValueChanged += (newValue) => { _audioServer.SetLinearVolumeTagged((float)newValue, tag); categoryVolumes[tag] = (float)newValue; };
            AddChild(slider);
            categoryVolumeSliders.Add(tag, slider);

            x = 60;
            foreach (var sound in sounds)
            {
                y += 40;

                var btn = (Button)buttonPrototype.Duplicate();
                btn.Position = new Vector2(x, y);
                btn.Visible = true;
                btn.Text = sound.identifier.ToString();
                btn.Pressed += () => { _audioServer.Toggle(sound.identifier); };
                AddChild(btn);

                var individualSlider = (HSlider)sliderPrototype.Duplicate();
                individualSlider.Position = new Vector2(x + btn.Size.X + 20, y);
                individualSlider.Size = new Vector2(100, 10);
                individualSlider.Visible = true;
                individualSlider.ValueChanged += (newValue) => { _audioServer.SetLinearVolume((float)newValue, sound.identifier); individualVolumes[sound.identifier] = (float)newValue; };
                AddChild(individualSlider);
                individualVolumeSliders.Add(sound.identifier, individualSlider);
            }
            x = 50;
            y += 80;
        }
    }

    public void PrintValues()
    {
        Debug.Print("master Volume: " + masterVolume.ToString());
        foreach (var pair in categoryVolumes)
            Debug.Print(pair.Key.ToString() + " Volume: " + pair.Value.ToString());

        foreach (var pair in individualVolumes)
            Debug.Print(pair.Key.ToString() + " Volume: " + pair.Value.ToString());
    }

    public void Save()
    {
        var saveData = GetSaveData();
        var json = JsonConvert.SerializeObject(saveData);
        File.WriteAllText("soundSettings.json", json);
    }

    public void Load()
    {
        var json = File.ReadAllText("soundSettings.json");
        var load = (dynamic)JsonConvert.DeserializeAnonymousType(json, GetSaveData(empty: true))!;
        masterVolume = (float)load.masterVolume;
        categoryVolumes = load.categoryVolumes.ToObject<Dictionary<SoundTags, float>>();
        individualVolumes = load.individualVolumes.ToObject<Dictionary<Sounds, float>>();

        masterVolumeSlider.Value = masterVolume;
        foreach (var sound in individualVolumes.Keys)
            individualVolumeSliders[sound].Value = individualVolumes[sound];
        foreach (var soundTag in categoryVolumes.Keys)
            categoryVolumeSliders[soundTag].Value = categoryVolumes[soundTag];
    }

    private object GetSaveData(bool empty = false)
    {
        return new
        {
            masterVolume = empty ? 0.0f : masterVolume,
            categoryVolumes = empty ? [] : categoryVolumes,
            individualVolumes = empty ? [] : individualVolumes,
        };
    }
}