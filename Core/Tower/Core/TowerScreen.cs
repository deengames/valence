using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using FlatSilverBallTemplate.Screens;
using FlatRedBall.Graphics;
using FlatRedBall;
using DeenGames.Utils.Tower.Controls;
using FlatRedBall.Math.Geometry;
using DeenGames.Valence.Tower.Controls;
using System.Collections.Generic;

namespace DeenGames.Utils.Tower.Core
{
    /// <summary>
    /// Our Tower framework's screen class. Makes life simpler.
    /// </summary>
    public class TowerScreen : Screen
    {
        public delegate void FadeEventDelegate(FadeOutMode mode);
        public event FadeEventDelegate FadeOutComplete;
        public event FadeEventDelegate FadeInComplete;

        private Sprite _fadeOut;
        public enum FadeOutMode { Full, Half };
        private FadeOutMode _fadeOutMode = FadeOutMode.Full;
        private bool _isFadingIn = false;
        // Are we fading in or out at all? Prevents firing events multiple times.
        private bool _isFadeActive = false;

        /// <summary>
        /// Base Screen constructor requires this. Get it done,
        /// so that we don't have to to add this to all screens.
        /// </summary>
        public TowerScreen() : base(typeof(TowerScreen).FullName) {
            Camera camera = SpriteManager.Camera;
            
            this._fadeOut = this.AddSprite("Content/1x1.png");
            this._fadeOut.ScaleX = camera.Width / 2;
            this._fadeOut.ScaleY = camera.Height / 2;
            this._fadeOut.Alpha = 0;

            // Make it black!
            this._fadeOut.Red = 0;
            this._fadeOut.Green = 0;
            this._fadeOut.Blue = 0;
            this._fadeOut.ColorOperation = ColorOperation.Color;

            this.GetTopZValueAndMoveFadeToTop(); // puts blackout on top
        }

        // Convenient. Cuts down on code.
        public void MoveToScreen(Type screenType) {
            MoveToScreen(screenType.FullName);
        }

        public void ManageForGarbageCollection(Object target)
        {
            if (target is Sprite) {
                this.mSprites.Add((Sprite)target);
            }
            else if (target is Text)
            {
                this.mTexts.Add((Text)target);
            }
            else if (target is AxisAlignedRectangle)
            {
                this.mAxisAlignedRectangles.Add((AxisAlignedRectangle)target);
            }
            else if (target is TowerText)
            {
                TowerText text = (TowerText)target;
                this.mTexts.Add(text.BaseText);
                this.mTexts.Add(text.EffectText);
            }
            else
            {
                throw new ArgumentException("Not sure how to manage " + target);
            }
        }

        public void FadeOutImmediately()
        {
            this._fadeOut.Alpha = 1;
        }

        public void FadeInImmediately()
        {
            this._fadeOut.Alpha = 0;
        }

        public void FadeOutHalf()
        {
            this.FadeOut(FadeOutMode.Half);
        }

        public void FadeOut()
        {
            this.FadeOut(FadeOutMode.Full);
        }

        public void FadeIn()
        {
            this._fadeOut.AlphaRate = -2;
            this._isFadingIn = true;
            this._isFadeActive = true;
        }

        public void FadeOut(FadeOutMode mode)
        {
            this._fadeOutMode = mode;
            this._fadeOut.AlphaRate = 2;
            this._isFadingIn = false;
            this._isFadeActive = true;
        }

        public override void Activity(bool firstTimeCalled)
        {
            if (this._isFadeActive == true)
            {
                if (this._isFadingIn == false && ((this._fadeOutMode == FadeOutMode.Half && this._fadeOut.Alpha >= 0.5) ||
                    (this._fadeOutMode == FadeOutMode.Full && this._fadeOut.Alpha >= 1)))
                {
                    this._fadeOut.AlphaRate = 0;
                    if (this.FadeOutComplete != null)
                    {
                        this.FadeOutComplete.Invoke(this._fadeOutMode);
                        this._isFadeActive = false;
                    }
                }
                else if (this._isFadingIn == true)
                {
                    if (this._fadeOut.Alpha <= 0)
                    {
                        this._fadeOut.AlphaRate = 0;
                        if (this.FadeInComplete != null)
                        {
                            this.FadeInComplete.Invoke(this._fadeOutMode);
                            this._isFadeActive = false;
                        }
                    }
                }
            }
            base.Activity(firstTimeCalled);
        }

        public AxisAlignedRectangle AddAxisAlignedRectangle()
        {
            return this.AddAxisAlignedRectangle(0.5f, 0.5f);
        }

        public AxisAlignedRectangle AddAxisAlignedRectangle(float scaleX, float scaleY)
        {
            return this.AddAxisAlignedRectangle(scaleX, scaleY, 0, 0);
        }

        public AxisAlignedRectangle AddAxisAlignedRectangle(float scaleX, float scaleY, int x, int y) {
            AxisAlignedRectangle toReturn = new AxisAlignedRectangle();
            toReturn.X = x;
            toReturn.Y = y;
            toReturn.ScaleX = scaleX;
            toReturn.ScaleY = scaleY;

            this.ManageForGarbageCollection(toReturn);
            ShapeManager.AddAxisAlignedRectangle(toReturn);
            
            return toReturn;
        }

        /// <summary>
        /// Adds a text object to our internal collection, so we don't
        /// have to clean it up manually later.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public Text AddText(string text)
        {
            Text t = TextManager.AddText(text);
            t.HorizontalAlignment = FlatRedBall.Graphics.HorizontalAlignment.Center;
            this.ManageForGarbageCollection(t);
            return t;
        }

        public void AddText(Text text)
        {
            TextManager.AddText(text);
            text.HorizontalAlignment = FlatRedBall.Graphics.HorizontalAlignment.Center;
            this.ManageForGarbageCollection(text);
        }

        public void RemoveResource(TowerText t)
        {
            this.RemoveText(t.BaseText);
            this.RemoveText(t.EffectText);
        }

        public void RemoveResource(PositionedObject p)
        {
            if (p is Sprite)
            {
                this.RemoveSprite((Sprite)p);
            }
            else if (p is Text)
            {
                this.RemoveText((Text)p);
            }
            else
            {
                throw new ArgumentException("Not sure how to remove " + p);
            }
        }

        /// <summary>
        /// Remove the text from the screen. Note that if you
        /// have a reference to this text, that still needs to be set to null.
        /// </summary>
        /// <param name="text"></param>
        internal void RemoveText(Text text)
        {
            TextManager.RemoveText(text);
            this.mTexts.Remove(text);
        }

        /// <summary>
        /// Adds a sprite to our internal collection, so we don't
        /// have to clean it up manually later.
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        public Sprite AddSprite(string texture)
        {
            Sprite s = SpriteManager.AddSprite(texture);
            s.PixelSize = 0.5f;
            this.ManageForGarbageCollection(s);
            return s;
        }

        /*
        public TowerSprite AddTowerSprite(string texture)
        {
            // self-managed GC
            return new TowerSprite(texture);
        }
        */

        /// <summary>
        /// Removes a sprite from the screen. Note that if you
        /// have a reference to this sprite, that needs to be set to null.
        /// </summary>
        /// <param name="s"></param>
        internal void RemoveSprite(Sprite sprite)
        {
            SpriteManager.RemoveSprite(sprite);
            this.mSprites.Remove(sprite);
        }

        /// <summary>
        /// Find some text by content. This is useful because we
        /// generally create texts in Initialize, and reference
        /// them (if need be) in Activity/Update methods.
        /// Note that if there are multiple text objects, it returns
        /// the first one, depending on the order FRB stores them
        /// (usually, the one created first).
        /// If no Text object exists with the specified displayText,
        /// then we return null.
        /// </summary>
        /// <param name="content"></param>
        /// <returns>the relevant Text object, by content, or null.</returns>
        public Text FindText(string displayText)
        {
            for (int i = 0; i < this.mTexts.Count; i++)
            {
                Text current = this.mTexts[i];
                if (current.DisplayText == displayText)
                {
                    return current;
                }
            }

            return null;
        }

        public float GetTopZValueAndMoveFadeToTop()
        {
            float max = 0;
            foreach (Sprite s in this.mSprites)
            {
                if (s.Z > max)
                {
                    max = s.Z;
                }
            }

            foreach (Text t in this.mTexts)
            {
                if (t.Z > max)
                {
                    max = t.Z;
                }
            }

            // Keep Blackout on top
            if (this._fadeOut.Z <= max)
            {
                this._fadeOut.Z = max + 1;
                max++;
            }

            return max; // next free top Z
        }
    }
}
