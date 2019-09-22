using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace NSmartProxy.Extension
{
    public class CAGen
    {
        /// <summary>
        /// 生成CA，TODO 2 如何通过根证书生成接下来的证书
        /// </summary>
        /// <param name="CertificateName"></param>
        /// <param name="hosts"></param>
        /// <returns></returns>
        public static X509Certificate2 GenerateCA(string CertificateName,string hosts = null)
        {
            SubjectAlternativeNameBuilder sanBuilder = new SubjectAlternativeNameBuilder();
            sanBuilder.AddIpAddress(IPAddress.Loopback);
            sanBuilder.AddIpAddress(IPAddress.IPv6Loopback);
            sanBuilder.AddDnsName("localhost");
            if (hosts != null)
            {
                string[] strings = hosts.Split(',');
                foreach (var str in strings)
                {
                    sanBuilder.AddDnsName(str);
                }
            }

            sanBuilder.AddDnsName(Environment.MachineName);

            X500DistinguishedName distinguishedName = new X500DistinguishedName($"CN={CertificateName}");

            using (RSA rsa = RSA.Create(2048))
            {
                var request = new CertificateRequest(distinguishedName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                request.CertificateExtensions.Add(
                    new X509KeyUsageExtension(X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature, false));


                request.CertificateExtensions.Add(
                    new X509EnhancedKeyUsageExtension(
                        new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, false));

                request.CertificateExtensions.Add(sanBuilder.Build());

                var certificate = request.CreateSelfSigned(new DateTimeOffset(DateTime.UtcNow.AddDays(-1)), new DateTimeOffset(DateTime.UtcNow.AddDays(3650)));
                //certificate.FriendlyName = CertificateName;
                //return certificate;

                return new X509Certificate2(certificate.Export(X509ContentType.Pfx, "WeNeedASaf3rPassword"),
                    "WeNeedASaf3rPassword", X509KeyStorageFlags.Exportable);

            }
        }
    }
}
