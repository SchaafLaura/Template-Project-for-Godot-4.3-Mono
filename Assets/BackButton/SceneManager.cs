using Godot;
using System.Collections.Generic;
using System.Linq;

public static partial class SceneManager
{
    static readonly List<string> scenePaths = [];
    /// <summary>
    /// Uses GetTree().ChangeSceneToFile() to change the scene, but saves the path of the current one for use of back buttons
    /// </summary>
    /// <param name="newScenePath">The new scene to change to</param>
    /// <param name="callingNode">The node this is getting called by, usually "this", but anything in the tree will do</param>
    public static void ChangeSceneForward(string newScenePath, Node callingNode)
    {
        scenePaths.Add(callingNode.GetTree().CurrentScene.SceneFilePath);
        callingNode.GetTree().ChangeSceneToFile(newScenePath);
    }
    /// <summary>
    /// Changes scene to whatever it was before. If there wasn't anything before it probably crashes, get rekt.
    /// </summary>
    /// <param name="callingNode">The node this is getting called by, usually "this", but anything in the tree will do</param>
    public static void ChangeSceneBackward(Node callingNode)
    {
        callingNode.GetTree().ChangeSceneToFile(scenePaths.Last());
        scenePaths.RemoveAt(scenePaths.Count - 1);
    }
}