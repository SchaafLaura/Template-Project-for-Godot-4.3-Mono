using Godot;

public class Sound
{
    private bool looping = false;

    public AudioStreamPlayer player;
    public readonly SoundTags tag;

    public float masterLinearVolume { get; private set; } = 1.0f;
    public float selfLinearVolume   { get; private set; } = 1.0f;
    public float tagLinearVolume    { get; private set; } = 1.0f;
    public float linearVolume       { get; private set; } = 1.0f;

    public bool Is(SoundTags tag) => tag == this.tag;

    public Sound(Resource stream, int polyphony, SoundTags tag)
    {
        player = new AudioStreamPlayer()
        {
            Stream       = (AudioStream) stream,
            MaxPolyphony = polyphony,
        };

        this.tag = tag;
    }

    public void ToggleLooping()
    {
        looping = !looping;
        if (looping)
            player.Finished += () => player.Play();
        else
            player.Finished -= () => player.Play();
    }

    public void Toggle()
    {
        if(player.Playing)
            player.Stop();
        else
            player.Play();
    }

    public void Play(float fromPosition)
    {
        player.Play(fromPosition);
    }

    public void Stop()
    {
        player.Stop();
    }

    private void RecomputeVolume()
    {
        linearVolume = selfLinearVolume * tagLinearVolume * masterLinearVolume;
        player.VolumeDb = Mathf.LinearToDb(linearVolume);
    }

    public void SetMasterLinearVolume(float newMasterLinearVolume)
    {
        masterLinearVolume = newMasterLinearVolume;
        RecomputeVolume();
    }

    public void SetSelfLinearVolume(float newSelfLinearVolume)
    {
        selfLinearVolume = newSelfLinearVolume;
        RecomputeVolume();
    }

    public void SetTagLinearVolume(float newTagLinearVolume)
    {
        tagLinearVolume = newTagLinearVolume;
        RecomputeVolume();
    }
}