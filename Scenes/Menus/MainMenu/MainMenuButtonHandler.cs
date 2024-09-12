using Godot;
using System;

public partial class MainMenuButtonHandler : Node
{
    public void OnStartPressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/Game/GameScene.tscn");
    }
    public void OnSettingsPressed()
    {
        GetTree().ChangeSceneToFile("res://Scenes/Menus/SettingMenus/ChooseSettingsMenu/ChooseSettingsMenuScene.tscn");
    }
    public void OnQuitPressed()
    {
        GetTree().Quit();
    }
}
