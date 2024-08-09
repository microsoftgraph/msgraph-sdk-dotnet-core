// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System.Runtime.Serialization;

namespace Microsoft.Graph.DotnetCore.Core.Test.TestModels.ServiceModels
{
    /// <summary>
    /// The enum BodyType.
    /// </summary>
    public enum TestBodyType
    {

        /// <summary>
        /// Text
        /// </summary>
        [EnumMember(Value = "text")]
        Text = 0,

        /// <summary>
        /// Html
        /// </summary>
        [EnumMember(Value = "html")]
        Html = 1,

    }
}