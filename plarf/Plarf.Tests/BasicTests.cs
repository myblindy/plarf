using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Plarf.Engine;
using Plarf.Engine.Helpers;
using Plarf.Engine.Helpers.Types;
using System.Linq;
using Plarf.Engine.GameObjects;
using Plarf.Tests.Helpers;
using Plarf.Engine.Helpers.Exceptions;
using Plarf.Engine.Actors;

namespace Plarf.Tests
{
    [TestClass]
    public class BasicTests
    {
        [TestMethod]
        public void Warmup()
        {
            PlarfGame.Instance.Initialize(new Size(50, 50));
            PlarfGame.Instance.LuaScript.DoString(@"g = game;");
        }

        [TestMethod]
        public void BasicWorld()
        {
            Resource res;
            PlarfGame.Instance.Initialize(new Size(50, 50));

            // resources
            res = PlarfGame.Instance.World.AddPlaceable(PlarfGame.Instance.ResourceTemplates["Stones"], new Location(0, 0)) as Resource;
            PlarfGame.Instance.World.MarkResourceForHarvest(res);

            ExtraAssert.Throws<LocationAlreadyInUseException>(() => PlarfGame.Instance.World.AddPlaceable(
                PlarfGame.Instance.ResourceTemplates["Stones"], new Location(0, 1)));

            res = PlarfGame.Instance.World.AddPlaceable(PlarfGame.Instance.ResourceTemplates["Tree"], new Location(0, 2)) as Resource;
            PlarfGame.Instance.World.MarkResourceForHarvest(res);
            res = PlarfGame.Instance.World.AddPlaceable(PlarfGame.Instance.ResourceTemplates["Tree"], new Location(1, 2)) as Resource;
            PlarfGame.Instance.World.MarkResourceForHarvest(res);

            res = PlarfGame.Instance.World.AddPlaceable(PlarfGame.Instance.ResourceTemplates["Stones"], new Location(0, 3)) as Resource;
            PlarfGame.Instance.World.MarkResourceForHarvest(res);

            ExtraAssert.Throws<LocationAlreadyInUseException>(() => PlarfGame.Instance.World.AddPlaceable(
                PlarfGame.Instance.ResourceTemplates["Stones"], new Location(0, 4)));

            // some basic bounds tests
            for (int x = 0; x <= 1; ++x)
                for (int y = 0; y <= 1; ++y)
                    Assert.IsTrue(PlarfGame.Instance.World.GetPlaceables(x, y).Cast<Resource>().Single().Name == "Stones");
            Assert.IsTrue(PlarfGame.Instance.World.GetPlaceables(0, 2).Cast<Resource>().Single().Name == "Tree");

            // actors
            var h = PlarfGame.Instance.World.AddActor(PlarfGame.Instance.HumanTemplate, new Location(2, 2)) as Human;
            //Game.Instance.World.AddActor(Game.Instance.HumanTemplate, new Location(0, 2));

            // simulation loop
            for (int cnt = 0; cnt < 50000; ++cnt)
                PlarfGame.Instance.Run(TimeSpan.FromMilliseconds(1000.0 / 30));

            Assert.IsTrue(h.Location == new Location(0, 0));
        }

        [TestMethod]
        public void BasicWorldLua()
        {
            PlarfGame.Instance.Initialize(new Size(50, 50));

            // resources
            PlarfGame.Instance.LuaScript.DoString(@"game.world.markResourceForHarvest(game.world.addPlaceable(game.resourceTemplates.Stones, 0, 0));");
            ExtraAssert.Throws<LocationAlreadyInUseException>(() => PlarfGame.Instance.LuaScript.DoString(
                @"game.world.addPlaceable(game.resourceTemplates.Stones, 0, 1);"));
            PlarfGame.Instance.LuaScript.DoString(@"game.world.markResourceForHarvest(game.world.addPlaceable(game.resourceTemplates.Tree, 0, 2));");
            PlarfGame.Instance.LuaScript.DoString(@"game.world.markResourceForHarvest(game.world.addPlaceable(game.resourceTemplates.Tree, 1, 2));");
            PlarfGame.Instance.LuaScript.DoString(@"game.world.markResourceForHarvest(game.world.addPlaceable(game.resourceTemplates.Stones, 0, 3));");
            ExtraAssert.Throws<LocationAlreadyInUseException>(() => PlarfGame.Instance.LuaScript.DoString(
                @"game.world.addPlaceable(game.resourceTemplates.Stones, 0, 4);"));

            // some basic bounds tests
            for (int x = 0; x <= 1; ++x)
                for (int y = 0; y <= 1; ++y)
                    Assert.IsTrue(PlarfGame.Instance.World.GetPlaceables(x, y).Cast<Resource>().Single().Name == "Stones");
            Assert.IsTrue(PlarfGame.Instance.World.GetPlaceables(0, 2).Cast<Resource>().Single().Name == "Tree");

            // actors
            PlarfGame.Instance.LuaScript.DoString(@"game.world.addActor(game.humanTemplate, 2, 2);");
            PlarfGame.Instance.LuaScript.DoString(@"game.world.addActor(game.humanTemplate, 0, 2);");

            // simulation loop
            for (int cnt = 0; cnt < 50000; ++cnt)
                PlarfGame.Instance.Run(TimeSpan.FromMilliseconds(1000.0 / 30));
        }
    }
}
