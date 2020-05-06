using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Graph
{
    public interface IEncryptedContentBearer
    {
        IDecryptableContent EncryptedContent { get; set; }
    }
}
