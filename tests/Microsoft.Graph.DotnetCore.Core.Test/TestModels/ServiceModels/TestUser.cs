// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.TestModels.ServiceModels
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// The type User.
    /// </summary>
    public partial class TestUser 
    {

        ///<summary>
        /// The User constructor
        ///</summary>
        public TestUser()
        {
            this.ODataType = "microsoft.graph.user";
        }

        /// <summary>
        /// Gets or sets id.
        /// Read-only.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets @odata.type.
        /// </summary>
        [JsonPropertyName("@odata.type")]
        public string ODataType { get; set; }

        /// <summary>
        /// Gets or sets additional data.
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, object> AdditionalData { get; set; }

        /// <summary>
        /// Gets or sets given name.
        /// The given name (first name) of the user. Supports $filter.
        /// </summary>
        [JsonPropertyName("givenName")]
        public string GivenName { get; set; }

        /// <summary>
        /// Gets or sets Display name.
        /// The displayName of the user. Supports $filter.
        /// </summary>
        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets state.
        /// The state or province in the user's address. Supports $filter.
        /// </summary>
        [JsonPropertyName("state")]
        public string State { get; set; }

        /// <summary>
        /// Gets or sets surname.
        /// The user's surname (family name or last name). Supports $filter.
        /// </summary>
        [JsonPropertyName("surname")]
        public string Surname { get; set; }

        /// <summary>
        /// Gets or sets eventDeltas.
        /// The user's event deltas. This property is just a testing value.
        /// </summary>
        [JsonPropertyName("eventDeltas")]
        public List<TestEvent> EventDeltas { get; set; }

    }
}