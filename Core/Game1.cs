using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

using GraphicsDeviceManager = SilverArcade.SilverSprite.GraphicsDeviceManager;
using FlatRedBall;
using FlatRedBall.Math.Geometry;
using FlatRedBall.Graphics;
using FlatRedBall.Input;
using FlatRedBall.AI.Pathfinding;
using Texture2D = SilverArcade.SilverSprite.Graphics.Texture2D;
using DeenGames.Valence.Screens;
using FlatSilverBallTemplate.Screens;
using DeenGames.Valence.Utils;
using CSharpCity.Utils.Tools;

namespace FlatSilverBallTemplate
{
    public class Game1 : SilverArcade.SilverSprite.Game
    {
        GraphicsDeviceManager graphics;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            FlatRedBallServices.InitializeFlatRedBall(this, graphics);

            // Currently this is required for FlatSilverBall.
            // If this is removed, then the engine will throw an exception.
            SpriteManager.Camera.UsePixelCoordinates(false);

            ScreenManager.Start(typeof(SplashScreen).FullName);

            base.Initialize();
        }

        protected override void Update(SilverArcade.SilverSprite.GameTime gameTime)
        {
            FlatRedBallServices.Update(gameTime);
            Screens.ScreenManager.Activity();
            base.Update(gameTime);
        }

        protected override void Draw(SilverArcade.SilverSprite.GameTime gameTime)
        {
            FlatRedBallServices.Draw();

            base.Draw(gameTime);
        }
    }
}
