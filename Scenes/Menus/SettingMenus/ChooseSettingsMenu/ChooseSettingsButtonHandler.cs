using Godot;

public partial class ChooseSettingsButtonHandler : Node
{
    public void OnAudioPressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/Menus/SettingMenus/AudioMenu/AudioMenuScene.tscn");
    }
    public void OnKeybindsPressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/Menus/SettingMenus/KeybindMenu/KeybindMenuScene.tscn");
    }
}
