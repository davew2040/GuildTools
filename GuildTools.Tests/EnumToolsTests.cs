using GuildTools.EF.Models.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace GuildTools.Tests
{
    [TestClass]
    public class EnumToolsTests
    {
        [TestMethod]
        public void TestEnumIteration()
        {
            var values = EnumUtilities.GetEnumValues<EF.Models.Enums.GuildProfilePermissionLevel>();

            var firstValue = values.ElementAt(0);

            Assert.AreEqual(firstValue.ToString(), "Admin");
            Assert.AreEqual((int)firstValue, 1);
        }
    }
}
