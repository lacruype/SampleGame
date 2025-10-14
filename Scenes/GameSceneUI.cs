using System;
using Gum.DataTypes;
using Gum.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using MonoGameGum.Forms.Controls;
using MonoGameGum.GueDeriving;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using RenderingLibrary;

namespace SampleGame.UI;

public class GameSceneUI : ContainerRuntime
{
    /* ================================== ATTRIBUTES ================================== */
    /// <summary>
    /// The string format to use when updating the text for the score display.
    /// </summary>
    private static readonly string s_scoreFormat = "{0:D6}";

    /// <summary>
    /// The text runtime for displaying the score.
    /// </summary>
    private TextRuntime _scoreText;

    /// <summary>
    /// The panel that displays the game over screen.
    /// </summary>
    private Gum.Forms.Controls.Panel _gameOverPanel;

    /// <summary>
    /// The retry button on the game over panel. Field is used to track reference
    /// so focus can be set when the game over panel is shown.
    /// </summary>
    private AnimatedButton _retryButton;

    /// <summary>
    /// Event invoked when the Quit button on either the Pause panel or the
    /// Game Over panel is clicked.
    /// </summary>
    public event EventHandler QuitButtonClick;

    /// <summary>
    /// Event invoked when the Retry button on the Game Over panel is clicked.
    /// </summary>
    public event EventHandler RetryButtonClick;

    /*=================================== EVENTS ================================== */

    /* ================================== CONSTRUCTORS ================================== */
    public GameSceneUI()
    {
        // The game scene UI inherits from ContainerRuntime, so we set its
        // doc to fill so it fills the entire screen.
        Dock(Gum.Wireframe.Dock.Fill);

        // Add it to the root element.
        this.AddToRoot();

        // Get a reference to the content manager that was registered with the
        // GumService when it was original initialized.
        ContentManager content = GumService.Default.ContentLoader.XnaContentManager;

        /// <summary>
        /// The texture atlas used for the UI elements.
        /// </summary>
        TextureAtlas atlas = TextureAtlas.FromFile(content, "images/atlas-definition.xml");

        // Create the text that will display the players score and add it as
        // a child to this container.
        _scoreText = CreateScoreText();
        AddChild(_scoreText);

        // Create the Game Over panel that is displayed when a game over occurs
        // and add it as a child to this container
        _gameOverPanel = CreateGameOverPanel(atlas);
        AddChild(_gameOverPanel.Visual);
    }

    /* ================================== METHODS ================================== */
    /// <summary>
    /// Creates the game over panel.
    /// </summary>
    /// <param name="atlas"></param>
    /// <returns></returns>
    private Gum.Forms.Controls.Panel CreateGameOverPanel(TextureAtlas atlas)
    {
        Gum.Forms.Controls.Panel panel = new Gum.Forms.Controls.Panel();
        panel.Anchor(Gum.Wireframe.Anchor.Center);
        panel.Visual.WidthUnits = DimensionUnitType.Absolute;
        panel.Visual.HeightUnits = DimensionUnitType.Absolute;
        panel.Visual.Width = 264.0f;
        panel.Visual.Height = 70.0f;
        panel.IsVisible = false;

        TextureRegion backgroundRegion = atlas.GetRegion("panel-background");

        NineSliceRuntime background = new NineSliceRuntime();
        background.Dock(Gum.Wireframe.Dock.Fill);
        background.Texture = backgroundRegion.Texture;
        background.TextureAddress = TextureAddress.Custom;
        background.TextureHeight = backgroundRegion.Height;
        background.TextureWidth = backgroundRegion.Width;
        background.TextureTop = backgroundRegion.SourceRectangle.Top;
        background.TextureLeft = backgroundRegion.SourceRectangle.Left;
        panel.AddChild(background);

        TextRuntime text = new TextRuntime();
        text.Text = "GAME OVER";
        text.WidthUnits = DimensionUnitType.RelativeToChildren;
        text.UseCustomFont = true;
        text.CustomFontFile = "fonts/MP16REG.fnt";
        text.FontScale = 0.5f;
        text.X = 10.0f;
        text.Y = 10.0f;
        panel.AddChild(text);

        _retryButton = new AnimatedButton(atlas);
        _retryButton.Text = "RETRY";
        _retryButton.Anchor(Gum.Wireframe.Anchor.BottomLeft);
        _retryButton.Visual.X = 9.0f;
        _retryButton.Visual.Y = -9.0f;

        _retryButton.Click += OnRetryButtonClicked;
        _retryButton.GotFocus += OnElementGotFocus;

        panel.AddChild(_retryButton);

        AnimatedButton quitButton = new AnimatedButton(atlas);
        quitButton.Text = "QUIT";
        quitButton.Anchor(Gum.Wireframe.Anchor.BottomRight);
        quitButton.Visual.X = -9.0f;
        quitButton.Visual.Y = -9.0f;

        quitButton.Click += OnQuitButtonClicked;
        quitButton.GotFocus += OnElementGotFocus;

        panel.AddChild(quitButton);

        return panel;
    }

    private void OnQuitButtonClicked(object sender, EventArgs args)
    {
        // Button was clicked, play the ui sound effect for auditory feedback.
        // Core.Audio.PlaySoundEffect(_uiSoundEffect);

        // Both panels have a quit button, so hide both panels
        // HidePausePanel();
        HideGameOverPanel();

        // Invoke the QuitButtonClick event.
        if (QuitButtonClick != null)
        {
            QuitButtonClick(sender, args);
        }
    }

    private void OnElementGotFocus(object sender, EventArgs args)
    {
        // A ui element that can receive focus has received focus, play the
        // ui sound effect for auditory feedback.
        // Core.Audio.PlaySoundEffect(_uiSoundEffect);
    }

    /// <summary>
    /// Tells the game scene ui to show the game over panel.
    /// </summary>
    public void ShowGameOverPanel()
    {
        _gameOverPanel.IsVisible = true;

        // Give the retry button focus for keyboard/gamepad input.
        _retryButton.IsFocused = true;

        // Ensure the pause panel isn't visible.
        // _pausePanel.IsVisible = false;
    }
    
    /// <summary>
    /// Tells the game scene ui to hide the game over panel.
    /// </summary>
    public void HideGameOverPanel()
    {
        _gameOverPanel.IsVisible = false;
    }

    private void OnRetryButtonClicked(object sender, EventArgs args)
    {
        // Button was clicked, play the ui sound effect for auditory feedback.
        // Core.Audio.PlaySoundEffect(_uiSoundEffect);

        // Since the retry button was clicked, we need to hide the game over panel.
        HideGameOverPanel();

        // Invoke the RetryButtonClick event.
        if (RetryButtonClick != null)
        {
            RetryButtonClick(sender, args);
        }
    }

    /// <summary>
    /// Creates the score text display.
    /// </summary>
    /// <returns></returns>
    private TextRuntime CreateScoreText()
    {
        TextRuntime text = new TextRuntime();
        text.Anchor(Gum.Wireframe.Anchor.TopLeft);
        text.WidthUnits = DimensionUnitType.RelativeToChildren;
        text.X = 26.50f;
        text.Y = -1.0f;
        text.UseCustomFont = true;
        text.CustomFontFile = @"fonts/MP16REG.fnt";
        text.FontScale = 0.45f;
        text.Text = string.Format(s_scoreFormat, 0);

        return text;
    }

    /// <summary>
    /// Updates the text on the score display.
    /// </summary>
    /// <param name="score">The score to display.</param>
    public void UpdateScoreText(int score)
    {
        _scoreText.Text = string.Format(s_scoreFormat, score);
    }

    /// Updates the game scene ui.
    /// </summary>
    /// <param name="gameTime">A snapshot of the timing values for the current update cycle.</param>
    public void Update(GameTime gameTime)
    {
        GumService.Default.Update(gameTime);
    }

    /// <summary>
    /// Draws the game scene ui.
    /// </summary>
    public void Draw()
    {
        GumService.Default.Draw();
    }

}
