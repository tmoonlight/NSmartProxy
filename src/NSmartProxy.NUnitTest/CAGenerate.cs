using System;
using System.Collections.Generic;
using System.Text;
using NSmartProxy.Extension;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace NSmartProxy.NUnitTest
{
    public class CAGenerate
    {

        [SetUp]
        public void Setup()
        {
        }
        [Test]
        public void TestCAGen()
        {
            var ca = CAGen.GenerateCA("shao");
            Assert.NotNull(ca);
        }

    }
}
