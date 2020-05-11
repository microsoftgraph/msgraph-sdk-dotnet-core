namespace Microsoft.Graph
{
    public interface IEncryptedContentBearer
    {
        IDecryptableContent EncryptedContent { get; set; }
    }
}
