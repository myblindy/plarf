using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Plarf.Engine;
using Plarf.Engine.Actors;
using Plarf.Engine.AI;
using Plarf.Engine.GameObjects;
using Plarf.Engine.Helpers.FileSystem;
using Plarf.Engine.Helpers.Types;
using Plarf.MonoGame.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Plarf.MonoGame
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class PlarfMonoGame : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Texture2D GrassTexture, LineTexture;
        IDictionary<string, Texture2D> MiscTextures = new Dictionary<string, Texture2D>();
        SpriteFont DefaultFont;

        Placeable SelectedPlaceable = null;

        TimeSpan LastFPSUpdate;
        int FPSFrameCounter;
        double FPS;

        const int GridSize = 20, LogViewportWidth = 400;

        public PlarfMonoGame()
        {
            graphics = new GraphicsDeviceManager(this) { PreferredBackBufferWidth = 1400, SynchronizeWithVerticalRetrace = false };
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            IsFixedTimeStep = false;
        }

        MouseState LastMouseState;
        protected override void Initialize()
        {
            // initialize the game
            PlarfGame.Instance.Initialize(new Size(50, 50));

            Resource res;
            res = PlarfGame.Instance.World.AddPlaceable(PlarfGame.Instance.ResourceTemplates["Stones"], 3, 3) as Resource;
            PlarfGame.Instance.World.MarkResourceForHarvest(res);
            res = PlarfGame.Instance.World.AddPlaceable(PlarfGame.Instance.ResourceTemplates["Stones"], 5, 3) as Resource;
            PlarfGame.Instance.World.MarkResourceForHarvest(res);
            res = PlarfGame.Instance.World.AddPlaceable(PlarfGame.Instance.ResourceTemplates["Stones"], 9, 3) as Resource;
            PlarfGame.Instance.World.MarkResourceForHarvest(res);
            res = PlarfGame.Instance.World.AddPlaceable(PlarfGame.Instance.ResourceTemplates["Tree"], 2, 3) as Resource;
            PlarfGame.Instance.World.MarkResourceForHarvest(res);
            res = PlarfGame.Instance.World.AddPlaceable(PlarfGame.Instance.ResourceTemplates["Tree"], 2, 4) as Resource;
            PlarfGame.Instance.World.MarkResourceForHarvest(res);

            PlarfGame.Instance.World.AddActor(PlarfGame.Instance.HumanTemplate, "C1", 0, 0);
            PlarfGame.Instance.World.AddActor(PlarfGame.Instance.HumanTemplate, "C2", 6, 7);
            ((Human)PlarfGame.Instance.World.AddActor(PlarfGame.Instance.HumanTemplate, "SM1", 0, 1)).WorkerType = PlarfGame.Instance.WorkerTypes[0];
            ((Human)PlarfGame.Instance.World.AddActor(PlarfGame.Instance.HumanTemplate, "SM2", 0, 2)).WorkerType = PlarfGame.Instance.WorkerTypes[0];

            PlarfGame.Instance.World.AddPlaceable(PlarfGame.Instance.BuildingClasses["Storage"], 6, 8, 12, 4);
            PlarfGame.Instance.World.AddPlaceable(PlarfGame.Instance.BuildingClasses["Sawmill"], 0, 8);

            // initialize the ui
            LastMouseState = Mouse.GetState();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // overall textures
            GrassTexture = Content.Load<Texture2D>("Textures/grass");

            // resource textures
            foreach (var restemplate in PlarfGame.Instance.ResourceTemplates)
                if (!MiscTextures.ContainsKey(restemplate.Value.Texture) && !string.IsNullOrWhiteSpace(restemplate.Value.Texture))
                    using (var stream = VFS.OpenStream(restemplate.Value.Texture))
                        MiscTextures.Add(restemplate.Value.Texture, TextureHelpers.FromStream(stream, GraphicsDevice));

            // building textures
            foreach (var bclass in PlarfGame.Instance.BuildingClasses)
                if (!MiscTextures.ContainsKey(bclass.Value.Texture) && !string.IsNullOrWhiteSpace(bclass.Value.Texture))
                    using (var stream = VFS.OpenStream(bclass.Value.Texture))
                        MiscTextures.Add(bclass.Value.Texture, TextureHelpers.FromStream(stream, GraphicsDevice));

            // actor resources
            if (!MiscTextures.ContainsKey(PlarfGame.Instance.HumanTemplate.Texture) && !string.IsNullOrWhiteSpace(PlarfGame.Instance.HumanTemplate.Texture))
                using (var stream = VFS.OpenStream(PlarfGame.Instance.HumanTemplate.Texture))
                    MiscTextures.Add(PlarfGame.Instance.HumanTemplate.Texture, TextureHelpers.FromStream(stream, GraphicsDevice));

            // the line texture
            LineTexture = new Texture2D(GraphicsDevice, 1, 1);
            LineTexture.SetData(new Color[] { Color.White });

            // fonts
            DefaultFont = Content.Load<SpriteFont>("Fonts/DefaultFont");
        }

        protected override void UnloadContent()
        {
            Content.Unload();
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // fps
            ++FPSFrameCounter;
            var fpsdelta = (gameTime.TotalGameTime - LastFPSUpdate).TotalSeconds;
            if (fpsdelta >= 1)
            {
                FPS = FPSFrameCounter / fpsdelta;
                FPSFrameCounter = 0;
                LastFPSUpdate = gameTime.TotalGameTime;
            }

            // update the game
            PlarfGame.Instance.Run(gameTime.ElapsedGameTime);

            // ui
            var mousestate = Mouse.GetState();
            if (mousestate.LeftButton == ButtonState.Pressed && mousestate.LeftButton != LastMouseState.LeftButton)
            {
                // selection
                SelectedPlaceable = PlarfGame.Instance.World.GetPlaceables(mousestate.X / GridSize, mousestate.Y / GridSize).FirstOrDefault();
            }

            LastMouseState = mousestate;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // the main view
            GraphicsDevice.Viewport = new Viewport(0, 0, Window.ClientBounds.Width - LogViewportWidth, Window.ClientBounds.Height);
            spriteBatch.Begin();

            // background
            for (int x = 0; x < PlarfGame.Instance.World.Size.Width; ++x)
                for (int y = 0; y < PlarfGame.Instance.World.Size.Height; ++y)
                    spriteBatch.Draw(GrassTexture, new Rectangle(x * GridSize, y * GridSize, GridSize, GridSize), Color.White);

            // grid
            for (int x = 1; x < PlarfGame.Instance.World.Size.Width; ++x)
                spriteBatch.DrawLine(LineTexture, new Vector2(x * GridSize, 0), new Vector2(x * GridSize, PlarfGame.Instance.World.Size.Height * GridSize), Color.Black);
            for (int y = 1; y < PlarfGame.Instance.World.Size.Height; ++y)
                spriteBatch.DrawLine(LineTexture, new Vector2(0, y * GridSize), new Vector2(PlarfGame.Instance.World.Size.Width * GridSize, y * GridSize), Color.Black);

            // resources
            foreach (var res in PlarfGame.Instance.World.Placeables.OfType<Resource>())
                if (MiscTextures.ContainsKey(res.Texture))
                    spriteBatch.Draw(MiscTextures[res.Texture],
                        new Rectangle((int)(res.Location.X * GridSize), (int)(res.Location.Y * GridSize), GridSize * res.Size.Width, GridSize * res.Size.Height), Color.White);

            // actors
            foreach (var actor in PlarfGame.Instance.World.Placeables.OfType<Actor>())
                if (MiscTextures.ContainsKey(actor.Texture))
                {
                    spriteBatch.Draw(MiscTextures[actor.Texture],
                        new Rectangle((int)(actor.Location.X * GridSize), (int)(actor.Location.Y * GridSize), GridSize, GridSize), Color.White);

                    var h = actor as Human;

                    // draw the job step progress
                    const int progressH = 4;
                    if (h?.CurrentJobStep.Type != JobType.Invalid)
                        spriteBatch.Draw(LineTexture, new Rectangle(
                            (int)h.Location.X * GridSize, (int)(h.Location.Y) * GridSize - progressH,
                            (int)((float)GridSize / h.JobStepDuration * h.JobStepBuildup),
                            progressH), Color.Purple);

                    // draw the AI target
                    if (h?.AssignedJob != null && h.AssignedJob.Target != null)
                        spriteBatch.DrawLine(LineTexture,
                            new Vector2((float)((h.Location.X + .5) * GridSize), (float)((h.Location.Y + .5) * GridSize)),
                            new Vector2((float)((h.AssignedJob.Target.Location.X + h.AssignedJob.Target.Size.Width / 2.0) * GridSize), (float)((h.AssignedJob.Target.Location.Y + h.AssignedJob.Target.Size.Height / 2.0) * GridSize)),
                            Color.Yellow);
                }

            // buildings
            foreach (var b in PlarfGame.Instance.World.Placeables.OfType<Building>())
                if (MiscTextures.ContainsKey(b.Texture))
                    spriteBatch.Draw(MiscTextures[b.Texture],
                        new Rectangle((int)(b.Location.X * GridSize), (int)(b.Location.Y * GridSize), GridSize * b.Size.Width, GridSize * b.Size.Height), Color.White);

            // FPS
            spriteBatch.DrawString(DefaultFont, "FPS: " + FPS.ToString("0.0"), new Vector2(0, 0), Color.White);

            spriteBatch.End();

            // the console
            GraphicsDevice.Viewport = new Viewport(Window.ClientBounds.Width - LogViewportWidth, 0, LogViewportWidth, Window.ClientBounds.Height);
            spriteBatch.Begin();

            if (SelectedPlaceable != null)
            {
                spriteBatch.DrawString(DefaultFont, "Selected: " + SelectedPlaceable.Name, new Vector2(0, 0), Color.White);

                int y = 2;
                foreach (var prop in ObjectHelpers.GetObjectProperties(SelectedPlaceable))
                    spriteBatch.DrawString(DefaultFont, prop.Item1 + ": " + prop.Item2, new Vector2(prop.Item3 * 15, y += 14), Color.White);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
