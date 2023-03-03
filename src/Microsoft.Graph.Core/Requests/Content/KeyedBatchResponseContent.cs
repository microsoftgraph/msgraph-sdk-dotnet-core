

namespace Microsoft.Graph
{
    using System.Collections.Generic;
    internal class KeyedBatchResponseContent
    {
        internal readonly IEnumerable<string> Keys;
        internal readonly BatchResponseContent Response;

        public KeyedBatchResponseContent(IEnumerable<string> keys, BatchResponseContent response)
        {
            Keys = keys;
            Response = response;
        }
    }
}
