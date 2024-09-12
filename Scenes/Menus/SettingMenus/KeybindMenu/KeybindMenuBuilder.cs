using Godot;
using Godot.Collections;
using System.Diagnostics;
using System.Linq;

public partial class KeybindMenuBuilder: GridContainer
{
    [Export]
    KeybindButton prototypeButton;
    [Export]
    bool excludeGodotActions = true;
    System.Collections.Generic.Dictionary<string, InputEvent[]> keybinds = [];
    public override void _Ready()
    {

        var actions = InputMap.GetActions();
        // TODO:
        // this isn't technically how you would exclude godot actions only!
        // A user can make their own actions starting with "ui"
        // find out if there is a fix for this
        if (excludeGodotActions)
            actions = new Array<StringName>(actions.Where((a) =>
                !a.ToString().StartsWith("ui")));

        int k = 0;
        foreach(var action in actions)
        {


            var name = action.ToString();
            if (name is null)
            {
                continue;
            }

            var events = InputMap.ActionGetEvents(action);
            var binds = new InputEvent[4];
            for (int i = 0; i < 4; i++)
            {
                if (i < events.Count)
                {
                    binds[i] = events[i];
                }
                else
                {
                    var e = new InputEventKey();
                    binds[i] = e;
                    InputMap.ActionAddEvent(action, e);
                }
            }
            keybinds.Add(name, binds);

            var lbl = new Label() { Text = name };

            AddChild(lbl);

            for (int i = 0; i < 4; i++)
            {
                var btn = (KeybindButton) prototypeButton.Duplicate();
                btn.Init(name, i);
                btn.OnRemap = RecievedInputRemap;
                btn.CustomMinimumSize = new Vector2(150, 10);
                btn.Toggled += (on) => 
                {
                    if (!on)
                    {
                        return;
                    }
                    foreach (var child in GetChildren())
                    {
                        if (child is KeybindButton b && b != btn)
                        {
                            b.OnActionButtonToggled(false);
                            b.ReleaseFocus();
                            b._Toggled(false);
                            b.ButtonPressed = false;
                        }
                    }
                };
                AddChild(btn);
            }
            if (k % 2 == 0)
            {
                var rect = new ColorRect()
                {
                    Position = new Vector2(-100, lbl.Position.Y - 5),
                    Size = new Vector2(2000, 32),
                    Color = new Color(0, 0, 0, 0.2f),
                    ZIndex = 999,
                };
                lbl.AddChild(rect);
            }
            k++;
        }
        prototypeButton.Visible = false;
    }

    private void RecievedInputRemap(string action, InputEvent e, int index)
    {
        // TODO: save and load keybinds dict
        // TODO: bind keys from the dict to actual action key thingies
        // TODO: have a "default" save that never gets touched that you can reload
        keybinds[action][index] = e;
        foreach(var pair in keybinds)
            Debug.Print(
                pair.Key + ": " + 
                pair.Value
                    .Select(x => x.AsText())
                    .Aggregate((str0, str1) => str0 + str1));
    }
}
