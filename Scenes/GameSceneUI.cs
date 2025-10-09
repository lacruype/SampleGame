using Gum.DataTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using MonoGameGum.GueDeriving;
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

        // Create the text that will display the players score and add it as
        // a child to this container.
        _scoreText = CreateScoreText();
        AddChild(_scoreText);
    }

    /* ================================== METHODS ================================== */
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
