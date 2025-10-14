using SampleGame.Scenes;
using Gum.Forms;
using Gum.Forms.Controls;
using MonoGameLibrary;
using MonoGameGum;
using Microsoft.Xna.Framework.Graphics;

namespace SampleGame;

public class Game1 : Core
{

    /// <summary>
    /// The universal size for Tiles
    /// </summary>
    public const int TileSize = 80;

    /// <summary>
    /// The delay between enemy spawning.
    /// </summary>
    public const int SpawnDelay = 5;

    public Game1() : base("Sample Game", 1280, 720, false)
    {
    }

    protected override void Initialize()
    {
        base.Initialize();

        // Initialize the UI manager
        InitializeGum();

        // Set fullscreen
        // Graphics.IsFullScreen = true;
        // Graphics.ApplyChanges();

        // Start the game with the title scene. (change when game is ready and Title scene is to be made)
        ChangeScene(new GameScene());
    }

    protected override void LoadContent()
    {
    }

    private void InitializeGum()
    {
        // Initialize the Gum service. The second parameter specifies
        // the version of the default visuals to use. V2 is the latest
        // version.
        GumService.Default.Initialize(this, DefaultVisualsVersion.V2);

        // Tell the Gum service which content manager to use.  We will tell it to
        // use the global content manager from our Core.
        GumService.Default.ContentLoader.XnaContentManager = Core.Content;

        // Register keyboard input for UI control.
        FrameworkElement.KeyboardsForUiControl.Add(GumService.Default.Keyboard);

        // Register gamepad input for Ui control.
        FrameworkElement.GamePadsForUiControl.AddRange(GumService.Default.Gamepads);

        // Customize the tab reverse UI navigation to also trigger when the keyboard
        // Up arrow key is pushed.
        FrameworkElement.TabReverseKeyCombos.Add(
        new KeyCombo() { PushedKey = Microsoft.Xna.Framework.Input.Keys.Up });

        // Customize the tab UI navigation to also trigger when the keyboard
        // Down arrow key is pushed.
        FrameworkElement.TabKeyCombos.Add(
        new KeyCombo() { PushedKey = Microsoft.Xna.Framework.Input.Keys.Down });

        // The assets created for the UI were done so at 1/4th the size to keep the size of the
        // texture atlas small.  So we will set the default canvas size to be 1/4th the size of
        // the game's resolution then tell gum to zoom in by a factor of 4.
        GumService.Default.CanvasWidth = GraphicsDevice.PresentationParameters.BackBufferWidth / 5.0f;
        GumService.Default.CanvasHeight = GraphicsDevice.PresentationParameters.BackBufferHeight / 5.0f;
        GumService.Default.Renderer.Camera.Zoom = 5.0f;
    }
}
