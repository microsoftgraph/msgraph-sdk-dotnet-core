// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using Newtonsoft.Json.Converters;

    public class EdmDateConverter : IsoDateTimeConverter
    {
        public EdmDateConverter()
        {
            this.DateTimeFormat = "yyyy-MM-dd";
        }
    }
}
