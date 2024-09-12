using Godot;

public partial class KeybindButton : Button
{
    int index;
    [Export]
    string action;

    public delegate void OnRemappedEvent(string action, InputEvent e, int index);
    public OnRemappedEvent OnRemap;

    public KeybindButton() : this("test", 0) { }

    public KeybindButton(string action, int index)
    {
        this.action = action;
        this.index = index;
    }

    public void Init(string action, int index)
    {
        this.action = action;
        this.index = index;
        ToggleMode = true;
        Toggled += OnActionButtonToggled;
    }
    
    public override void _Ready()
    {
        SetProcessUnhandledKeyInput(false);
        DisplayKey();
    }

    public void DisplayKey()
    {
        var events = InputMap.ActionGetEvents(action);
        if (events.Count == 0)
            Text = "Unmapped";
        else
        {
            var text = events[index].AsText();
            if(text.Length > 15)
                text = text[..15];
            Text = text;
            TooltipText = events[index].AsText();
        }
    }

    public void OnActionButtonToggled(bool buttonPressed)
    {
        SetProcessUnhandledKeyInput(buttonPressed);
        if (buttonPressed)
            Text = "...";
        else
            DisplayKey();
    }

    public override void _UnhandledKeyInput(InputEvent e)
    {
        // TODO: don't just blindly accept rebinds.. if there's conflicts or someone binds some silly shit
        RemapKey(e);
        ButtonPressed = false;
        this.GetViewport().SetInputAsHandled();
    }

    private void RemapKey(InputEvent e)
    {
        var events = InputMap.ActionGetEvents(action);
        InputMap.ActionEraseEvents(action);

        events[index] = e;
        foreach(var @event in events)
            InputMap.ActionAddEvent(action, @event);

        ReleaseFocus();
        DisplayKey();
        OnRemap?.Invoke(action, e, index);
    }
}
