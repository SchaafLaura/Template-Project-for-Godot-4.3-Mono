using Godot;
using Godot.Collections;

public partial class AudioServerInstance : Node
{
    private Sounds backingFieldForNothing = Sounds._PickSound_;
    [Export]
    public Sounds AddNewSound
    {
        get
        {
            return backingFieldForNothing;
        }
        set
        {
            backingFieldForNothing = Sounds._PickSound_;

            soundsToLoad ??= [];
            if(!soundsToLoad.Contains(value))
                soundsToLoad.Add(value);

            NotifyPropertyListChanged();
        }
    }

    private SoundLists backingFieldForNothing1 = SoundLists._PickCategory_;
    [Export]
    public SoundLists AddCategory
    {
        get
        {
            return backingFieldForNothing1;
        }
        set
        {
            backingFieldForNothing1 = SoundLists._PickCategory_;

            var soundsToAdd = AudioServer.GetSoundsFromCategory(value);
            soundsToLoad ??= [];
            foreach (var sound in soundsToAdd)
                if (!soundsToLoad.Contains(sound))
                    soundsToLoad.Add(sound);

            NotifyPropertyListChanged();
        }
    }

    [Export]
    private Array<Sounds> soundsToLoad = [];
}