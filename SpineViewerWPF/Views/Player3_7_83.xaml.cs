﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Spine3_7_83;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SpineViewerWPF.Views
{
    /// <summary>
    /// Player.xaml 的互動邏輯
    /// </summary>
    public partial class Player3_7_83 : UserControl
    {
        private SpriteBatch spriteBatch;
        private GraphicsDevice graphicsDevice;
        private Skeleton skeleton;
        private AnimationState state;
        private SkeletonRenderer skeletonRenderer;
        private System.Windows.Point mouseLocation;
        private bool isPress = false;
        private bool isNew = true;
        private ExposedList<Animation> listAnimation;
        private ExposedList<Skin> listSkin;
        private Atlas atlas;
        private SkeletonData skeletonData;
        private AnimationStateData stateData;
        private SkeletonBinary binary;
        private SkeletonJson json;

        public Player3_7_83()
        {
            InitializeComponent();
            App.AppXC.Initialize += Initialize;
            App.AppXC.Update += Update;
            App.AppXC.LoadContent += LoadContent;
            App.AppXC.Draw += Draw;
            App.AppXC.Width = App.GV.FrameWidth;
            App.AppXC.Height = App.GV.FrameHeight;

            Frame.Children.Add(App.AppXC);

        }

        private void Initialize()
        {
            PresentationParameters pp = new PresentationParameters();
            pp.BackBufferWidth = (int)App.GV.FrameWidth;
            pp.BackBufferHeight = (int)App.GV.FrameHeight;
            App.AppXC.RenderSize = new Size(pp.BackBufferWidth, pp.BackBufferHeight);
            graphicsDevice = App.AppXC.GraphicsDevice;
            graphicsDevice.PresentationParameters.BackBufferWidth = (int)App.GV.FrameWidth;
            graphicsDevice.PresentationParameters.BackBufferHeight = (int)App.GV.FrameHeight;
            spriteBatch = new SpriteBatch(graphicsDevice);
        }

        private void LoadContent(ContentManager contentManager)
        {
            skeletonRenderer = new SkeletonRenderer(graphicsDevice);
            skeletonRenderer.PremultipliedAlpha = App.GV.Alpha;

            atlas = new Atlas(App.GV.SelectFile, new XnaTextureLoader(graphicsDevice));

            if (Common.IsBinaryData(App.GV.SelectFile))
            {
                binary = new SkeletonBinary(atlas);
                binary.Scale = App.GV.Scale;
                skeletonData = binary.ReadSkeletonData(Common.GetSkelPath(App.GV.SelectFile));
            }
            else
            {
                json = new SkeletonJson(atlas);
                json.Scale = App.GV.Scale;
                skeletonData = json.ReadSkeletonData(Common.GetJsonPath(App.GV.SelectFile));
            }
            skeleton = new Skeleton(skeletonData);

            if (isNew)
            {
                App.GV.PosX = Convert.ToSingle(App.GV.FrameWidth / 2f);
                App.GV.PosY = Convert.ToSingle((skeleton.Data.Height + App.GV.FrameHeight) / 2f);
            }
            App.GV.FileHash = skeleton.Data.Hash;

            stateData = new AnimationStateData(skeleton.Data);

            state = new AnimationState(stateData);

            List<string> AnimationNames = new List<string>();
            listAnimation = state.Data.skeletonData.Animations;
            foreach (Animation An in listAnimation)
            {
                AnimationNames.Add(An.name);
            }
            App.GV.AnimeList = AnimationNames;

            List<string> SkinNames = new List<string>();
            listSkin = state.Data.skeletonData.Skins;
            foreach (Skin Sk in listSkin)
            {
                SkinNames.Add(Sk.name);
            }
            App.GV.SkinList = SkinNames;

            if (App.GV.SelectAnimeName != "")
            {
                state.SetAnimation(0, App.GV.SelectAnimeName, App.GV.IsLoop);
            }
            else
            {
                state.SetAnimation(0, state.Data.skeletonData.animations.Items[0].name, App.GV.IsLoop);
            }

            if (isNew)
            {
                MainWindow.SetCBAnimeName();
            }
            isNew = false;

        }



        private void Update(GameTime gameTime)
        {
            if (App.GV.SelectAnimeName != "" && App.GV.SetAnime)
            {
                state.ClearTracks();
                skeleton.SetToSetupPose();
                state.SetAnimation(0, App.GV.SelectAnimeName, App.GV.IsLoop);
                App.GV.SetAnime = false;
            }

            if (App.GV.SelectSkin != "" && App.GV.SetSkin)
            {
                skeleton.SetSkin(App.GV.SelectSkin);
                skeleton.SetSlotsToSetupPose();
                App.GV.SetSkin = false;
            }


        }

        private void Draw()
        {
            if (App.GV.SpineVersion != "3.7.83" || App.GV.FileHash != skeleton.Data.Hash)
            {
                state = null;
                skeletonRenderer = null;
                return;
            }
            graphicsDevice.Clear(Color.Transparent);

            if (App.GV.UseBG && App.TextureBG != null)
            {
                spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend);
                spriteBatch.Draw(App.TextureBG, new Rectangle((int)App.GV.PosBGX, (int)App.GV.PosBGY, App.TextureBG.Width, App.TextureBG.Height), Color.White);
                spriteBatch.End();
            }


            state.Update(App.GV.Speed / 1000f);
            state.Apply(skeleton);
            state.TimeScale = App.GV.TimeScale;
            if (binary != null)
            {
                if (App.GV.Scale != binary.Scale)
                {
                    binary.Scale = App.GV.Scale;
                    skeletonData = binary.ReadSkeletonData(Common.GetSkelPath(App.GV.SelectFile));
                    skeleton = new Skeleton(skeletonData);
                }
            }
            else if (json != null)
            {
                if (App.GV.Scale != json.Scale)
                {
                    json.Scale = App.GV.Scale;
                    skeletonData = json.ReadSkeletonData(Common.GetJsonPath(App.GV.SelectFile));
                    skeleton = new Skeleton(skeletonData);
                }
            }

            skeleton.X = App.GV.PosX;
            skeleton.Y = App.GV.PosY;
            skeleton.ScaleX = App.GV.FilpX ? -1 : 1;
            skeleton.ScaleY = App.GV.FilpY ? 1 : -1;


            skeleton.RootBone.Rotation = App.GV.Rotation;
            skeleton.UpdateWorldTransform();
            skeletonRenderer.PremultipliedAlpha = App.GV.Alpha;
            if (skeletonRenderer.Effect is BasicEffect)
            {
                ((BasicEffect)skeletonRenderer.Effect).Projection = Matrix.CreateOrthographicOffCenter(0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, 0, 1, 0);
            }
            else
            {
                skeletonRenderer.Effect.Parameters["Projection"].SetValue(Matrix.CreateOrthographicOffCenter(0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, 0, 1, 0));
            }
            skeletonRenderer.Begin();
            skeletonRenderer.Draw(skeleton);
            skeletonRenderer.End();

            if (state != null)
            {
                TrackEntry entry = state.GetCurrent(0);
                if (entry != null)
                {
                    if (App.GV.IsRecoding && App.GV.GifList != null && !entry.IsComplete)
                    {
                        if (App.GV.GifList.Count == 0)
                        {
                            TrackEntry te = state.GetCurrent(0);
                            te.trackTime = 0;
                            App.GV.TimeScale = 1;
                            App.GV.Lock = 0;
                        }

                        App.GV.GifList.Add(Common.TakeRecodeScreenshot(graphicsDevice));
                    }

                    if (App.GV.IsRecoding && entry.IsComplete)
                    {
                        state.TimeScale = 0;
                        App.GV.IsRecoding = false;
                        Common.RecodingEnd(entry.AnimationEnd);

                        state.TimeScale = 1;
                        App.GV.TimeScale = 1;
                    }

                    if (App.GV.TimeScale == 0)
                    {
                        entry.TrackTime = entry.AnimationEnd * App.GV.Lock;
                        entry.TimeScale = 0;
                    }
                    else
                    {
                        App.GV.Lock = entry.AnimationTime / entry.AnimationEnd;
                        entry.TimeScale = 1;
                    }
                    App.GV.LoadingProcess = $"{ Math.Round(entry.AnimationTime / entry.AnimationEnd * 100, 2)}%";
                }
            }


        }

        private void Frame_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isPress = true;
            mouseLocation = Mouse.GetPosition(this.Frame);
        }

        private void Frame_MouseMove(object sender, MouseEventArgs e)
        {
            if (isPress)
            {
                System.Windows.Point position = Mouse.GetPosition(this.Frame);
                if (App.GV.UseBG && App.GV.ControlBG)
                {
                    Common.SetBGXY(position.X, position.Y, this.mouseLocation.X, this.mouseLocation.Y);
                }
                else
                {
                    Common.SetXY(position.X, position.Y, this.mouseLocation.X, this.mouseLocation.Y);
                }
                mouseLocation = Mouse.GetPosition(this.Frame);
            }
        }

        public void ChangeSet()
        {
            App.AppXC.ContentManager.Dispose();
            atlas.Dispose();
            atlas = null;
            App.AppXC.LoadContent.Invoke(App.AppXC.ContentManager);
        }

        private void Frame_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isPress = false;
        }

        private void Frame_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                App.GV.Scale += 0.02f;
            }
            else
            {
                if (App.GV.Scale > 0.04f)
                {
                    App.GV.Scale -= 0.02f;
                }
            }

        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            App.AppXC.Width = App.GV.FrameWidth;
            App.AppXC.Height = App.GV.FrameHeight;
            if (graphicsDevice != null)
            {
                graphicsDevice.PresentationParameters.BackBufferWidth = (int)App.GV.FrameWidth;
                graphicsDevice.PresentationParameters.BackBufferHeight = (int)App.GV.FrameHeight;
            }

        }

        private void Frame_MouseLeave(object sender, MouseEventArgs e)
        {
            isPress = false;
        }
    }
}