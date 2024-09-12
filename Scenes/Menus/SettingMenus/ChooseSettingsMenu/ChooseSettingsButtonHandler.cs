using Godot;

public partial class ChooseSettingsButtonHandler : Node
{
    public void OnAudioPressed()
    {
        SceneManager.ChangeSceneForward("res://Scenes/Menus/SettingMenus/AudioMenu/AudioMenuScene.tscn", this);
    }
    public void OnKeybindsPressed()
    {
        SceneManager.ChangeSceneForward("res://Scenes/Menus/SettingMenus/KeybindMenu/KeybindMenuScene.tscn", this);
    }
}
