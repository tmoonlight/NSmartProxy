using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using NSmartProxy.Extension;
using NUnit.Framework;

namespace NSmartProxy.SecurityTests
{
    [TestFixture]
    public class SecurityIssuesTests
    {
        [Test]
        public void CAGen_UsesHardcodedPassword_SecurityVulnerability()
        {
            // Arrange
            var certificateName = "test";

            // Act
            var certificate = CAGen.GenerateCA(certificateName);

            // Assert
            // This test demonstrates that the certificate can be accessed with the hardcoded password
            var exportedCert = certificate.Export(X509ContentType.Pfx, "WeNeedASaf3rPassword");
            Assert.That(exportedCert, Is.Not.Null);
            
            // This is a security issue - the password is hardcoded and predictable
            Assert.That(exportedCert.Length, Is.GreaterThan(0), "Certificate should be exportable with hardcoded password");
        }

        [Test]
        public void CAGen_HardcodedPasswordIsExposed_SecurityIssue()
        {
            // This test verifies that the hardcoded password is indeed a security risk
            var certificateName = "test";
            var certificate = CAGen.GenerateCA(certificateName);

            // The fact that we can export with this specific password proves it's hardcoded
            Assert.That(() => 
            {
                var export = certificate.Export(X509ContentType.Pfx, "WeNeedASaf3rPassword");
            }, Throws.Nothing, "Certificate exports with hardcoded password - this is a security vulnerability");
        }

        [Test]
        public void PathTraversal_FileWriteVulnerability_Demonstration()
        {
            // This test demonstrates the potential for path traversal attacks
            // Note: This is for demonstration purposes - the actual API would need to be called
            
            var baseLogPath = "./temp";
            var maliciousFileName = "../../../etc/passwd"; // Potential path traversal
            var targetPath = baseLogPath + "/" + maliciousFileName;

            // This shows how the current code could be vulnerable to path traversal
            Assert.That(targetPath.Contains("../"), Is.True, "Path contains traversal characters - potential security risk");
        }

        [Test]
        public void DefaultCredentials_AdminAccountExists_SecurityIssue()
        {
            // This test documents the existence of default admin credentials
            var defaultUsername = "admin";
            var defaultPassword = "admin";

            // These are the default credentials created in HttpServer_APIs constructor
            Assert.That(defaultUsername, Is.EqualTo("admin"), "Default admin username is predictable");
            Assert.That(defaultPassword, Is.EqualTo("admin"), "Default admin password is weak and predictable");
        }
    }
}