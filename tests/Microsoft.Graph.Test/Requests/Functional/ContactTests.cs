using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Graph.Test.Requests.Functional
{
    [Ignore]
    [TestClass]
    public class ContactTests : GraphTestBase
    {
        // 
        // http://graph.microsoft.io/en-us/docs/api-reference/v1.0/api/singlevaluelegacyextendedproperty_post_singlevalueextendedproperties
        [TestMethod]
        public async Task ContactsSetGetSingleExtendedProperty()
        {
            try
            {
                var contact = new Contact();
                contact.GivenName = "_Tom" + Guid.NewGuid().ToString();

                var customProperty = new SingleValueLegacyExtendedProperty();
                var namespaceGuid = "f5939744-0f22-4f03-b33c-f18a8acfa20b";
                var mapiPropertyType = "String";
                var propertyName = "CustomProperty";
                var propertyId = $"{mapiPropertyType} {{{namespaceGuid}}} Name {propertyName}";
                customProperty.Id = propertyId;
                customProperty.Value = "My custom property value";

                var extendedValueCollection = new ContactSingleValueExtendedPropertiesCollectionPage();
                extendedValueCollection.Add(customProperty);

                contact.SingleValueExtendedProperties = extendedValueCollection;

                // This results in a call to the service. It adds a contact with the extended property set on it.
                var partiallySyncdContact = await graphClient.Me.Contacts.Request().AddAsync(contact);

                Assert.IsNotNull(partiallySyncdContact.Id, "The ID property is not set on the contact.");

                // This results in a call to the service. It gets the contact with the extended property.
                // http://graph.microsoft.io/en-us/docs/api-reference/v1.0/api/singlevaluelegacyextendedproperty_get
                var syncdContact = await graphClient.Me.Contacts[partiallySyncdContact.Id].Request().Expand($"singleValueExtendedProperties($filter=id eq '{propertyId}')").GetAsync();

                Assert.IsNotNull(syncdContact.SingleValueExtendedProperties, "The expected extended property was not set on the contact");

            }
            catch (Microsoft.Graph.ServiceException e)
            {
                Assert.Fail("Something happened, check out a trace. Error code: {0}", e.Error.Code);
            }
        }
    }
}
