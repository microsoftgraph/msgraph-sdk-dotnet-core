// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.Core.Test.TestModels
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    using Newtonsoft.Json;
    
    /// <summary>
    /// A class to test abstract entity serialization and deserialization.
    /// </summary>
    [JsonConverter(typeof(DerivedTypeConverter))]
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class AbstractEntityType
    {
        protected AbstractEntityType()
        {

        }

        /// <summary>
        /// Gets or sets id.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "id", Required = Required.Default)]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets additional data.
        /// </summary>
        [JsonExtensionData(ReadData = true, WriteData = true)]
        public IDictionary<string, object> AdditionalData { get; set; }
    }
}
