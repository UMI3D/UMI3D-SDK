using System.Security.Cryptography.X509Certificates;
using UnityEngine.Networking;

public class AcceptAllCertificates : CertificateHandler
{
    // Encoded RSAPublicKey
    private static string PUB_KEY = "mypublickey";
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        X509Certificate2 certificate = new X509Certificate2(certificateData);
        string pk = certificate.GetPublicKeyString();
        if (pk.ToLower().Equals(PUB_KEY.ToLower()))
            return true;
        //return false;
        return true;
    }
}
