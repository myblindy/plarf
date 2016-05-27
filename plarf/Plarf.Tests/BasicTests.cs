using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Plarf.Engine;
using Plarf.Engine.Helpers;
using Plarf.Engine.Helpers.Types;
using System.Linq;
using Plarf.Engine.GameObjects;
using Plarf.Tests.Helpers;
using Plarf.Engine.Helpers.Exceptions;

namespace Plarf.Tests
{
    [TestClass]
    public class BasicTests
    {
        [TestMethod]
        public void Warmup()
        {
            Game.Instance.Initialize(new Size(50, 50));
            Game.Instance.LuaScript.DoString(@"g = game;");
        }

        [TestMethod]
        public void BasicWorld()
        {
            Game.Instance.Initialize(new Size(50, 50));

            Game.Instance.World.AddPlaceable(Game.Instance.ResourceTemplates["Stones"], new Location(0, 0));

            ExtraAssert.Throws<LocationAlreadyInUseException>(() => Game.Instance.World.AddPlaceable(
                Game.Instance.ResourceTemplates["Stones"], new Location(0, 1)));

            Game.Instance.World.AddPlaceable(Game.Instance.ResourceTemplates["Tree"], new Location(0, 2));
            Game.Instance.World.AddPlaceable(Game.Instance.ResourceTemplates["Tree"], new Location(1, 2));

            Game.Instance.World.AddPlaceable(Game.Instance.ResourceTemplates["Stones"], new Location(0, 3));

            ExtraAssert.Throws<LocationAlreadyInUseException>(() => Game.Instance.World.AddPlaceable(
                Game.Instance.ResourceTemplates["Stones"], new Location(0, 4)));

            for (int x = 0; x <= 1; ++x)
                for (int y = 0; y <= 1; ++y)
                    Assert.IsTrue(Game.Instance.World.GetPlaceablesAt(x, y).Cast<Resource>().Single().Name == "Stones");
            Assert.IsTrue(Game.Instance.World.GetPlaceablesAt(0, 2).Cast<Resource>().Single().Name == "Tree");
        }

        [TestMethod]
        public void BasicWorldLua()
        {
            Game.Instance.Initialize(new Size(50, 50));

            Game.Instance.LuaScript.DoString(@"game.world.addPlaceable(game.resourceTemplates.Stones, 0, 0);");
            ExtraAssert.Throws<LocationAlreadyInUseException>(() => Game.Instance.LuaScript.DoString(
                @"game.world.addPlaceable(game.resourceTemplates.Stones, 0, 1);"));
            Game.Instance.LuaScript.DoString(@"game.world.addPlaceable(game.resourceTemplates.Tree, 0, 2);");
            Game.Instance.LuaScript.DoString(@"game.world.addPlaceable(game.resourceTemplates.Tree, 1, 2);");
            Game.Instance.LuaScript.DoString(@"game.world.addPlaceable(game.resourceTemplates.Stones, 0, 3);");
            ExtraAssert.Throws<LocationAlreadyInUseException>(() => Game.Instance.LuaScript.DoString(
                @"game.world.addPlaceable(game.resourceTemplates.Stones, 0, 4);"));

            for (int x = 0; x <= 1; ++x)
                for (int y = 0; y <= 1; ++y)
                    Assert.IsTrue(Game.Instance.World.GetPlaceablesAt(x, y).Cast<Resource>().Single().Name == "Stones");
            Assert.IsTrue(Game.Instance.World.GetPlaceablesAt(0, 2).Cast<Resource>().Single().Name == "Tree");
        }
    }
}
