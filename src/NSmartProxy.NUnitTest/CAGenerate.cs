using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Security.Cryptography.X509Certificates;
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
            var export = ca.Export(X509ContentType.Pfx);
            File.WriteAllBytes("c:\\test.pfx", export);
            Assert.NotNull(ca);
        }

    }
}
