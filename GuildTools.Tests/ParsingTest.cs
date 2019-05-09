using GuildTools.ExternalServices.Blizzard.JsonParsing;
using GuildTools.ExternalServices.Raiderio.JsonParsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace GuildTools.Tests
{
    [TestClass]
    public class ParsingTest
    {
        [TestMethod]
        public void TestPlayerParsing()
        {
            var read = Utilities.ReadFromAssembly("Assets.JSON.player.json");
            var parsed = PlayerParsing.GetSinglePlayerFromJson(read);

            Assert.AreEqual(parsed.Name, "Kromp");
        }

        [TestMethod]
        public void TestRaiderIoPlayerParsing()
        {
            var read = Utilities.ReadFromAssembly("Assets.JSON.RaiderIo.player.json");
            var parsed = RaiderIoParsing.GetPlayerFromJson(read);

            Assert.AreEqual(parsed.Name, "Kromzul");
        }
    }
}
