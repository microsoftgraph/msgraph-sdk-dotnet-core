// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.TestModels.ServiceModels
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// The enum BodyType.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TestBodyType
    {

        /// <summary>
        /// Text
        /// </summary>
        Text = 0,

        /// <summary>
        /// Html
        /// </summary>
        Html = 1,

    }
}