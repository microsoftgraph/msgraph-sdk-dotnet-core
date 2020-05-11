namespace Microsoft.Graph
{
    public interface IDecryptableContent
    {
        string Data { get; set; }
        string DataKey { get; set; }
        string DataSignature { get; set; }
        string EncryptionCertificateId { get; set; }
        string EncryptionCertificateThumbprint { get; set; }
    }
}
