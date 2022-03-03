// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.TestModels.ServiceModels
{
    using Microsoft.Kiota.Abstractions.Serialization;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The type User.
    /// </summary>
    public partial class TestUser :IParsable
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
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets @odata.type.
        /// </summary>
        public string ODataType { get; set; }

        /// <summary>
        /// Gets or sets additional data.
        /// </summary>
        public IDictionary<string, object> AdditionalData { get; set; }

        /// <summary>
        /// Gets or sets given name.
        /// The given name (first name) of the user. Supports $filter.
        /// </summary>
        public string GivenName { get; set; }

        /// <summary>
        /// Gets or sets Display name.
        /// The displayName of the user. Supports $filter.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets state.
        /// The state or province in the user's address. Supports $filter.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets surname.
        /// The user's surname (family name or last name). Supports $filter.
        /// </summary>
        public string Surname { get; set; }

        /// <summary>
        /// Gets or sets eventDeltas.
        /// The user's event deltas. This property is just a testing value.
        /// </summary>
        public List<TestEvent> EventDeltas { get; set; }

        /// <summary>
        /// Gets the field deserializers for the <see cref="TestUser"/> instance
        /// </summary>
        /// <typeparam name="T">The type to deserialize</typeparam>
        /// <returns></returns>
        public IDictionary<string, Action<T, IParseNode>> GetFieldDeserializers<T>()
        {
            return new Dictionary<string, Action<T, IParseNode>>
            {
                {"id", (o,n) => { (o as TestUser).Id = n.GetStringValue(); } },
                {"@odata.type", (o,n) => { (o as TestUser).ODataType = n.GetStringValue(); } },
                {"givenName", (o,n) => { (o as TestUser).GivenName = n.GetStringValue(); } },
                {"displayName", (o,n) => { (o as TestUser).DisplayName = n.GetStringValue(); } },
                {"state", (o,n) => { (o as TestUser).State = n.GetStringValue(); } },
                {"surname", (o,n) => { (o as TestUser).Surname = n.GetStringValue(); } },
                {"eventDeltas", (o,n) => { (o as TestUser).EventDeltas = n.GetCollectionOfObjectValues<TestEvent>(TestEvent.CreateFromDiscriminatorValue).ToList(); } },
            };
        }

        /// <summary>
        /// Serialize the <see cref="TestUser"/> instance
        /// </summary>
        /// <param name="writer">The <see cref="ISerializationWriter"/> to serialize the instance</param>
        /// <exception cref="ArgumentNullException">Thrown when the writer is null</exception>
        public void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteStringValue("id", Id);
            writer.WriteStringValue("@odata.type", ODataType);
            writer.WriteStringValue("givenName", GivenName);
            writer.WriteStringValue("displayName", DisplayName);
            writer.WriteStringValue("state", State);
            writer.WriteStringValue("surname", Surname);
            writer.WriteCollectionOfObjectValues("eventDeltas", EventDeltas);
            writer.WriteAdditionalData(AdditionalData);
        }

        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        /// </summary>
        public static TestUser CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new TestUser();
        }
    }
}