using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services.Securities.Configs
{
    public class RSAGenerator
    {
        public static void GenerateKey()
        {
            using var rsa = RSA.Create(3072);
            string privateKey = rsa.ExportPkcs8PrivateKeyPem();
            string publicKey = rsa.ExportSubjectPublicKeyInfoPem();

            Directory.CreateDirectory("keys");

            File.WriteAllText("keys/private.pem", privateKey);
            File.WriteAllText("keys/public.pem", publicKey);
            

        }
    }
}
