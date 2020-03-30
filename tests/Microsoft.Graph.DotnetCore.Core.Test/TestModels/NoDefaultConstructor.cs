// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.TestModels
{
    using System.Text.Json.Serialization;
    /// <summary>
    /// A property bag class with no default constructor for unit testing purposes.
    /// </summary>
    [JsonConverter(typeof(DerivedTypeConverter<NoDefaultConstructor>))]
    public class NoDefaultConstructor
    {
        static NoDefaultConstructor()
        {

        }

        public NoDefaultConstructor(string parameter)
        {

        }
    }
}
