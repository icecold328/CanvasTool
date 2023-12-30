using Flax.Build;

public class CanvasToolEditorTarget : GameProjectEditorTarget
{
    /// <inheritdoc />
    public override void Init()
    {
        base.Init();

        // Reference the modules for editor
        Modules.Add("CanvasTool");
        Modules.Add("CanvasToolEditor");
    }
}
