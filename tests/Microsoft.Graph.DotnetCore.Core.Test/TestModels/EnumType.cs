// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------


namespace Microsoft.Graph.DotnetCore.Core.Test.TestModels
{
    using System.Text.Json.Serialization;
    /// <summary>
    /// Enum for testing enum serialization and deserialization.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EnumType
    {
        Value,
    }
}
