using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
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

        /// <summary>
        /// 从文件加载服务端证书。
        /// 服务端TLS必须使用带私钥的证书，X509Certificate2.CreateFromCertFile
        /// 不支持带密码的pfx，遇到只含公钥的cer/crt也不会报错，
        /// 结果是每一个进来的连接都在握手时抛异常，表现为访问一直卡住。
        /// 这里在加载阶段就把问题暴露出来。
        /// </summary>
        /// <param name="path">证书文件路径（pfx/p12）</param>
        /// <param name="password">证书密码，无密码传null</param>
        public static X509Certificate2 LoadServerCertificate(string path, string password = null)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("证书文件不存在。", path);

            var flags = X509KeyStorageFlags.Exportable;
            //避免在Windows上把私钥写入用户密钥容器（容器/服务账户下经常没有可写的用户配置目录）
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                flags |= X509KeyStorageFlags.MachineKeySet;
            }

            var cert = new X509Certificate2(File.ReadAllBytes(path), password, flags);
            if (!cert.HasPrivateKey)
            {
                cert.Dispose();
                throw new InvalidOperationException(
                    $"证书文件“{path}”不包含私钥，无法用于服务端TLS。请上传pfx/p12格式（含私钥）的证书。");
            }

            return cert;
        }
    }
}
