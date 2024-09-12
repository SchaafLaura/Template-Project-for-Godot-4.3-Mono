using Godot;
public partial class BackButtonScript : Button
{
    public override void _Pressed()
    {
        SceneManager.ChangeSceneBackward(this);
        base._Pressed();
    }
}