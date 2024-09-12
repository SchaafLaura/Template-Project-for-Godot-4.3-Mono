using Godot;
using System;

public partial class MainMenuButtonHandler : Node
{
    public void OnStartPressed()
    {
        GetTree().ChangeSceneToFile("res://Game/GameScene.tscn");
    }
    public void OnSettingsPressed()
    {
        GetTree().ChangeSceneToFile("res://Menus/SettingMenus/ChooseSettingsMenu/ChooseSettingsMenuScene.tscn");
    }
    public void OnQuitPressed()
    {
        GetTree().Quit();
    }
}
