using System.Collections.Generic;

namespace Microsoft.Graph
{
    public interface ITokenValidable
    {
        IEnumerable<string> ValidationTokens { get; set; }
        IEnumerable<IEncryptedContentBearer> Value { get; set; }
    }
}