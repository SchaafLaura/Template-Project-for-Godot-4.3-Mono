using Godot;
using Godot.Collections;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Linq;

public partial class KeybindMenuBuilder: GridContainer
{
    [Export]
    KeybindButton prototypeButton;
    [Export]
    bool excludeGodotActions = true;
    System.Collections.Generic.Dictionary<string, InputEvent[]> Keybinds = [];

    // TODO: have a "default" save that never gets touched that you can reload

    public override void _Ready()
    {
        prototypeButton.Visible = true;
        var actions = InputMap.GetActions();
        // TODO:
        // this isn't technically how you would exclude godot actions only!
        // A user can make their own actions starting with "ui"
        // find out if there is a fix for this
        if (excludeGodotActions)
            actions = new Array<StringName>(actions.Where((a) =>
                !a.ToString().StartsWith("ui")));

        int k = 0;
        foreach (var action in actions)
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
                    if (events[i] is InputEventKey key)
                        key.CommandOrControlAutoremap = false;
                    binds[i] = events[i];
                }
                else
                {
                    var e = new InputEventKey
                    {
                        CommandOrControlAutoremap = false
                    };
                    binds[i] = e;
                    InputMap.ActionAddEvent(action, e);
                }
            }
            Keybinds.TryAdd(name, binds);

            var lbl = new Label() { Text = name };
            AddChild(lbl);
            for (int i = 0; i < 4; i++)
            {
                var btn = (KeybindButton)prototypeButton.Duplicate();
                btn.Init(name, i);
                //btn.OnRemap = RecievedInputRemap;
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
                    ZIndex = 999, // TODO: this is kinda dumb - find out why the color rects aren't properly sorted
                };
                lbl.AddChild(rect);
            }
            k++;
        }
        prototypeButton.Visible = false;
    }

    private void RecievedInputRemap(string action, InputEvent e, int index)
    {
        
        Keybinds[action][index] = e;
        foreach(var pair in Keybinds)
            Debug.Print(
                pair.Key + ": " + 
                pair.Value
                    .Select(x => x.AsText())
                    .Aggregate((str0, str1) => str0 + str1));
    }

    public void Save()
    {
        var saveData = GetSaveData();
        var json = JsonConvert.SerializeObject(saveData, Formatting.Indented, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        });
        File.WriteAllText("keybindSettings.json", json);
    }
    
    public void Load()
    {
        var json = File.ReadAllText("keybindSettings.json");
        var saveStructure = GetSaveData(empty: true);
        var load = (dynamic)JsonConvert.DeserializeAnonymousType(json, saveStructure, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        })!;

        Keybinds = load.keybinds;

        foreach (var pair in Keybinds)
        {
            var action = pair.Key;
            var binds = pair.Value;

            InputMap.ActionEraseEvents(action);
            foreach(var bind in binds)
                InputMap.ActionAddEvent(action, bind);  
        }

        foreach (var child in GetChildren())
        {
            RemoveChild(child);
            child.Dispose();
        }

        _Ready();
    }

    private object GetSaveData(bool empty = false)
    {
        return new
        {
            keybinds = empty ? [] : Keybinds,
        };
    }
}
