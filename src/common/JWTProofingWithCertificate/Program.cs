using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JWTProofingWithCertificate
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = ProgramOptions.GetOptionsFromCommandLine(args);

            var certBytes = System.IO.File.ReadAllBytes(options.CertificatePath);
            var certificate = (new Org.BouncyCastle.X509.X509CertificateParser()).ReadCertificate(certBytes);
            var pubKey = (Org.BouncyCastle.Crypto.Parameters.ECPublicKeyParameters)certificate.GetPublicKey();

            var jwtArray = options.JWT.Split('.');

            var baseContentBytes = System.Text.Encoding.UTF8.GetBytes($"{jwtArray[0]}.{jwtArray[1]}");
            var signedContentBytes = fiskaltrust.ifPOS.Utilities.FromBase64urlString(jwtArray[2]);

            if (fiskaltrust.service.storage.Encryption.Verify(baseContentBytes, signedContentBytes, Convert.ToBase64String(pubKey.Q.GetEncoded())))
            {
                Console.WriteLine($"Signed content verified successfully!");
            }
            else
            {
                Console.WriteLine($"Signed content verified unsuccessfully!");
            }
        }
    }
}
