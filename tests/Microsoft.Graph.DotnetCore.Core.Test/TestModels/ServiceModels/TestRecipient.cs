// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.TestModels.ServiceModels
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class TestRecipient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestRecipient"/> class.
        /// </summary>
        public TestRecipient()
        {
            this.ODataType = "microsoft.graph.recipient";
        }

        /// <summary>
        /// Gets or sets emailAddress.
        /// The recipient's email address.
        /// </summary>
        [JsonPropertyName("emailAddress")]
        public TestEmailAddress EmailAddress { get; set; }

        /// <summary>
        /// Gets or sets additional data.
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, object> AdditionalData { get; set; }

        /// <summary>
        /// Gets or sets @odata.type.
        /// </summary>
        [JsonPropertyName("@odata.type")]
        public string ODataType { get; set; }
    }
}