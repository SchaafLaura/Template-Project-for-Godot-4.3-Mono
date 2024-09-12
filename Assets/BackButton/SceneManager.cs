using Godot;
using System.Collections.Generic;
using System.Linq;

public static partial class SceneManager
{
    static List<string> scenePaths = new();
    public static void ChangeSceneForward(string newScenePath, Node callingNode)
    {
        scenePaths.Add(callingNode.GetTree().CurrentScene.SceneFilePath);
        callingNode.GetTree().ChangeSceneToFile(newScenePath);
    }
    public static void ChangeSceneBackward(Node callingNode)
    {
        callingNode.GetTree().ChangeSceneToFile(scenePaths.Last());
        scenePaths.RemoveAt(scenePaths.Count - 1);
    }
}
