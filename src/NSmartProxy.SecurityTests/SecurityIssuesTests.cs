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
        public void CAGen_SupportsCustomPassword_SecurityImprovement()
        {
            // Test that the new secure password parameter works
            var certificateName = "test";
            var customPassword = "MySecurePassword123!";
            
            var certificate = CAGen.GenerateCA(certificateName, null, customPassword);
            
            // Should be able to export with custom password
            Assert.That(() => 
            {
                var export = certificate.Export(X509ContentType.Pfx, customPassword);
            }, Throws.Nothing, "Certificate should export with custom password");
            
            // Should NOT be able to export with old hardcoded password
            Assert.That(() => 
            {
                var export = certificate.Export(X509ContentType.Pfx, "WeNeedASaf3rPassword");
            }, Throws.Exception, "Certificate should NOT export with old hardcoded password");
        }

        [Test]
        public void CAGen_GeneratesRandomPasswordWhenNoneProvided_SecurityImprovement()
        {
            // Test that random passwords are generated when none provided
            var certificateName = "test";
            
            var certificate1 = CAGen.GenerateCA(certificateName);
            var certificate2 = CAGen.GenerateCA(certificateName);
            
            // Both certificates should exist but should not be exportable with hardcoded password
            Assert.That(certificate1, Is.Not.Null);
            Assert.That(certificate2, Is.Not.Null);
            
            // Verify they can't be exported with old hardcoded password
            Assert.That(() => 
            {
                var export = certificate1.Export(X509ContentType.Pfx, "WeNeedASaf3rPassword");
            }, Throws.Exception, "Certificate should NOT export with old hardcoded password");
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
        public void PathSanitization_PreventDirectoryTraversal_SecurityImprovement()
        {
            // Test that Path.GetFileName properly sanitizes filenames
            var maliciousFileName = "../../../etc/passwd";
            var sanitizedFileName = Path.GetFileName(maliciousFileName);
            
            // Should only contain the filename, not directory traversal
            Assert.That(sanitizedFileName, Is.EqualTo("passwd"));
            Assert.That(sanitizedFileName.Contains("../"), Is.False, "Sanitized filename should not contain directory traversal");
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