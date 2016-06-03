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

        public PlarfMonoGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            PlarfGame.Instance.Initialize(new Size(50, 50));

            Resource res;
            res = PlarfGame.Instance.World.AddPlaceable(PlarfGame.Instance.ResourceTemplates["Stones"], 3, 3) as Resource;
            PlarfGame.Instance.World.MarkResourceForHarvest(res);
            res = PlarfGame.Instance.World.AddPlaceable(PlarfGame.Instance.ResourceTemplates["Stones"], 5, 3) as Resource;
            PlarfGame.Instance.World.MarkResourceForHarvest(res);
            res = PlarfGame.Instance.World.AddPlaceable(PlarfGame.Instance.ResourceTemplates["Stones"], 9, 3) as Resource;
            PlarfGame.Instance.World.MarkResourceForHarvest(res);

            PlarfGame.Instance.World.AddActor(PlarfGame.Instance.HumanTemplate, 0, 0);
            PlarfGame.Instance.World.AddActor(PlarfGame.Instance.HumanTemplate, 6, 7);

            PlarfGame.Instance.World.AddPlaceable(PlarfGame.Instance.BuildingClasses["Storage"], 0, 8, 12, 4);

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

            PlarfGame.Instance.Run(gameTime.ElapsedGameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            const int gridsize = 20, logH = 90;

            // the main view
            GraphicsDevice.Viewport = new Viewport(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height - logH);
            spriteBatch.Begin();

            // background
            for (int x = 0; x < PlarfGame.Instance.World.Size.Width; ++x)
                for (int y = 0; y < PlarfGame.Instance.World.Size.Height; ++y)
                    spriteBatch.Draw(GrassTexture, new Rectangle(x * gridsize, y * gridsize, gridsize, gridsize), Color.White);

            // grid
            for (int x = 1; x < PlarfGame.Instance.World.Size.Width; ++x)
                spriteBatch.DrawLine(LineTexture, new Vector2(x * gridsize, 0), new Vector2(x * gridsize, PlarfGame.Instance.World.Size.Height * gridsize), Color.Black);
            for (int y = 1; y < PlarfGame.Instance.World.Size.Height; ++y)
                spriteBatch.DrawLine(LineTexture, new Vector2(0, y * gridsize), new Vector2(PlarfGame.Instance.World.Size.Width * gridsize, y * gridsize), Color.Black);

            // resources
            foreach (var res in PlarfGame.Instance.World.Placeables.OfType<Resource>())
                if (MiscTextures.ContainsKey(res.Texture))
                    spriteBatch.Draw(MiscTextures[res.Texture],
                        new Rectangle((int)(res.Location.X * gridsize), (int)(res.Location.Y * gridsize), gridsize * res.Size.Width, gridsize * res.Size.Height), Color.White);

            // actors
            foreach (var actor in PlarfGame.Instance.World.Actors)
                if (MiscTextures.ContainsKey(actor.Texture))
                {
                    spriteBatch.Draw(MiscTextures[actor.Texture],
                        new Rectangle((int)(actor.Location.X * gridsize), (int)(actor.Location.Y * gridsize), gridsize, gridsize), Color.White);

                    var h = actor as Human;

                    // draw the job step progress
                    const int progressH = 4;
                    if (h?.CurrentJobStep.Type != JobType.Invalid)
                        spriteBatch.Draw(LineTexture, new Rectangle(
                            (int)h.Location.X * gridsize, (int)(h.Location.Y) * gridsize - progressH,
                            (int)((float)gridsize / h.JobStepDuration * h.JobStepBuildup),
                            progressH), Color.Purple);

                    // draw the AI target
                    if (h?.AssignedJob != null && h.AssignedJob.Target != null)
                        spriteBatch.DrawLine(LineTexture,
                            new Vector2((float)((h.Location.X + .5) * gridsize), (float)((h.Location.Y + .5) * gridsize)),
                            new Vector2((float)((h.AssignedJob.Target.Location.X + h.AssignedJob.Target.Size.Width / 2.0) * gridsize), (float)((h.AssignedJob.Target.Location.Y + h.AssignedJob.Target.Size.Height / 2.0) * gridsize)),
                            Color.Yellow);
                }

            // buildings
            foreach (var b in PlarfGame.Instance.World.Placeables.OfType<Building>())
                if (MiscTextures.ContainsKey(b.Texture))
                    spriteBatch.Draw(MiscTextures[b.Texture],
                        new Rectangle((int)(b.Location.X * gridsize), (int)(b.Location.Y * gridsize), gridsize * b.Size.Width, gridsize * b.Size.Height), Color.White);

            spriteBatch.End();

            // the console
            GraphicsDevice.Viewport = new Viewport(0, Window.ClientBounds.Height - logH, Window.ClientBounds.Width, logH);
            spriteBatch.Begin();

            // humans log
            int idx = 0;
            foreach (var h in PlarfGame.Instance.World.Actors.OfType<Human>())
            {
                spriteBatch.DrawString(DefaultFont, string.Format("H{0} @ {1} carrying {2} assigned {3} step {4}",
                    idx, h.Location, h.ResourcesCarried, h.AssignedJob, h.CurrentJobStep),
                    new Vector2(0, idx * 15), Color.White);

                ++idx;
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
