using GuildTools.ExternalServices.Blizzard.JsonParsing;
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
    }
}
