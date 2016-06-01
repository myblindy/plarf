using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Plarf.Engine;
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

        public PlarfMonoGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            PlarfGame.Instance.Initialize(new Size(50, 50));

            Resource res;
            res = PlarfGame.Instance.World.AddPlaceable(PlarfGame.Instance.ResourceTemplates["Stones"], 0, 0) as Resource;
            PlarfGame.Instance.World.MarkResourceForHarvest(res);
            res = PlarfGame.Instance.World.AddPlaceable(PlarfGame.Instance.ResourceTemplates["Stones"], 2, 0) as Resource;
            PlarfGame.Instance.World.MarkResourceForHarvest(res);
            res = PlarfGame.Instance.World.AddPlaceable(PlarfGame.Instance.ResourceTemplates["Stones"], 4, 0) as Resource;
            PlarfGame.Instance.World.MarkResourceForHarvest(res);

            PlarfGame.Instance.World.AddActor(PlarfGame.Instance.HumanTemplate, 1, 5);
            PlarfGame.Instance.World.AddActor(PlarfGame.Instance.HumanTemplate, 4, 7);

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

            // actor resources
            if (!MiscTextures.ContainsKey(PlarfGame.Instance.HumanTemplate.Texture) && !string.IsNullOrWhiteSpace(PlarfGame.Instance.HumanTemplate.Texture))
                using (var stream = VFS.OpenStream(PlarfGame.Instance.HumanTemplate.Texture))
                    MiscTextures.Add(PlarfGame.Instance.HumanTemplate.Texture, TextureHelpers.FromStream(stream, GraphicsDevice));

            // the line texture
            LineTexture = new Texture2D(GraphicsDevice, 1, 1);
            LineTexture.SetData(new Color[] { Color.White });
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

            const int gridsize = 20;

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
                    spriteBatch.Draw(MiscTextures[actor.Texture],
                        new Rectangle((int)(actor.Location.X * gridsize), (int)(actor.Location.Y * gridsize), gridsize, gridsize), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
