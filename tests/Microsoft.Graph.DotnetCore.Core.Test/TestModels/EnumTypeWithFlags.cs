// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------


using System.Runtime.Serialization;

namespace Microsoft.Graph.DotnetCore.Core.Test.TestModels
{
    /// <summary>
    /// Enum for testing enum serialization and deserialization.
    /// </summary>
    [System.Flags]
    public enum EnumTypeWithFlags
    {
        [EnumMember(Value = "firstValue")]
        FirstValue = 1,
        [EnumMember(Value = "secondValue")]
        SecondValue = 2
    }
}