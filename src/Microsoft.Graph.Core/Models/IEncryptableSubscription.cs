namespace Microsoft.Graph
{
    public interface IEncryptableSubscription
    {
        string EncryptionCertificate { get; set; }
    }
}