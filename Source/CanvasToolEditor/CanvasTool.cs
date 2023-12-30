using System;
using FlaxEditor;
using FlaxEditor.GUI.ContextMenu;
using FlaxEditor.GUI;
using FlaxEngine;

namespace CanvasTool;

/// <summary>
/// The sample game plugin.
/// </summary>
public class CanvasTool : EditorPlugin
{
    /// <inheritdoc />
    public CanvasTool()
    {
        _description = new PluginDescription
        {
            Name = "Canvas Tool",
            Category = "Canvas",
            Author = "IceCold328",
            AuthorUrl = null,
            HomepageUrl = null,
            RepositoryUrl = "https://github.com/icecold328/CanvasTool",
            Description = "Canvas Tooling, Helps Quickly change all elements of a canvas.",
            Version = new Version(1, 0, 0),
            IsAlpha = false,
            IsBeta = false,
        };
    }


    MainMenuButton _mainButton;
    ContextMenuButton _windowButton;
    CanvasToolsWindow _CanvasWindow;

    /// <inheritdoc />
    public override void InitializeEditor()
    {
        _mainButton = Editor.UI.MainMenu.GetButton("Plugins");
        if (_mainButton is null)
            _mainButton = Editor.UI.MainMenu.AddButton("Plugins");

        _windowButton = _mainButton.ContextMenu.AddButton("Canvas Tools");
        _windowButton.Clicked += OpenWindow;

        base.InitializeEditor();
    }
    void OpenWindow()
    {
        _CanvasWindow = new CanvasToolsWindow();
        _CanvasWindow.Window.Title = "Canvas Tools";
        _CanvasWindow.Show();
    }
    /// <inheritdoc />
    public override void DeinitializeEditor()
    {
        _windowButton.Clicked -= OpenWindow;
        _CanvasWindow = null;

        _mainButton.Dispose();
        _windowButton.Dispose();
        base.DeinitializeEditor();
    }
}
