using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Graph
{
    public interface ITokenValidable
    {
        IEnumerable<string> ValidationTokens { get; set; }
        IEnumerable<IEncryptedContentBearer> Value { get; set; }

        Task<bool> AreTokensValid(IEnumerable<Guid> tenantIds, IEnumerable<Guid> appIds, string wellKnownUri = "https://login.microsoftonline.com/common/.well-known/openid-configuration", string issuerPrefix = "https://sts.windows.net/");
    }
}