// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    /// When placed on an Interface, specifies the converter type to use.
    /// </summary>
    /// <remarks>
    /// The specified converter type must derive from <see cref="JsonConverter"/>.
    /// When placed on a property, the specified converter will always be used.
    /// When placed on a type, the specified converter will be used unless a compatible converter is added to
    /// of the same type.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Interface)]
    public class InterfaceConverterAttribute: JsonConverterAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="JsonConverterAttribute"/> with the specified converter type.
        /// </summary>
        /// <param name="converterType">The type of the converter.</param>
        public InterfaceConverterAttribute(Type converterType)
        : base(converterType)
        {
        }

    }
}