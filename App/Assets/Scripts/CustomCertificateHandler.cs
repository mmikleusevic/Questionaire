using System.Security.Cryptography.X509Certificates;
using UnityEngine.Networking;


//public class CustomCertificateHandler : CertificateHandler
//{
    //This is a proper way to handle this, but for the purpose of this application we will bypass it
    // private readonly string validThumbprint;
    //
    // public CustomCertificateHandler(string certificateThumbprint)
    // {
    //     validThumbprint = certificateThumbprint;
    // }
    //
    // protected override bool ValidateCertificate(byte[] certificateData)
    // {
    //     X509Certificate2 certificate = new X509Certificate2(certificateData);
    //     
    //     string thumbprint = certificate.Thumbprint;
    //     
    //     return thumbprint == validThumbprint;
    // }
//}
