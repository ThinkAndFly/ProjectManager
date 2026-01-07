using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace ProjectManager.Infraestructure.Security
{
    public static class RSAKeyHelper
    {
        public static RsaSecurityKey GetPublicKey(IConfiguration config)
        {
            var pem = config["Jwt:PublicKeyPem"]
                      ?? throw new InvalidOperationException("Jwt:PublicKeyPem missing");
            return FromPem(pem);
        }

        public static RsaSecurityKey GetPrivateKey(IConfiguration config)
        {
            var pem = config["Jwt:PrivateKeyPem"]
                      ?? throw new InvalidOperationException("Jwt:PrivateKeyPem missing");
            return FromPem(pem);
        }

        private static RsaSecurityKey FromPem(string pem)
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(pem);
            return new RsaSecurityKey(rsa);
        }
    }
}