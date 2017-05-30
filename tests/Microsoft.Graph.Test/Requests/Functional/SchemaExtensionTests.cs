using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using Async = System.Threading.Tasks;

namespace Microsoft.Graph.Test.Requests.Functional
{
    [Ignore]
    [TestClass]
    public class SchemaExtensionTests : GraphTestBase
    {
        /// <summary>
        /// Create a schema extension test 
        /// https://developer.microsoft.com/en-us/graph/docs/api-reference/v1.0/api/schemaextension_post_schemaextensions
        /// </summary>
        [TestMethod]
        public async Async.Task CreateSchemaExtension()
        {
            // Create a schema extension on a contact.
            SchemaExtension extensionPayload = new SchemaExtension()
            {
                Description = "This extension correlates a group with a foreign database.",
                Id = $"crmForeignKey", // Microsoft Graph will prepend 8 chars
                Properties = new List<ExtensionSchemaProperty>()
                {
                    new ExtensionSchemaProperty() { Name = "fid", Type = "Integer" }
                },
                TargetTypes = new List<string>()
                {
                    "Group"
                }
            };

            // Create the schema extension. This results in a call to Microsoft Graph.
            SchemaExtension schemaExtension = await graphClient.SchemaExtensions.Request().AddAsync(extensionPayload);
            Assert.IsNotNull(schemaExtension, "Expected: schemaExtension is not null; Actual: it returned null");
            Assert.AreEqual(schemaExtension.Status, "InDevelopment", $"Expected: a new schem extension has a status as InDevelopment; Actual: {schemaExtension.Status}");
            StringAssert.Contains(schemaExtension.Id, extensionPayload.Id, "Expected: the payload identifier is contained in the schema extension returned by the service; Actual: it is not returned");
            Assert.IsNotNull(schemaExtension.Owner, "Expected: the owner value is set by the service; Actual: it wasn't set by the service.");

            // List all of the schema extensions available to this application.
            IGraphServiceSchemaExtensionsCollectionPage schemaExtensions = await graphClient.SchemaExtensions.Request().GetAsync();
            Assert.IsTrue(schemaExtensions.Count > 0, "Expected: at least one schema extension; Actual: 0");
            Assert.IsNotNull(schemaExtensions[0].Properties, "Expected: the extension properties are set; Actual: extension properties are not set.");
            Assert.IsTrue(schemaExtensions[0].Description.Length > 0, "Expected: the description has at least one character; Actual: the description was not set");
            Assert.IsNotNull(schemaExtensions[0].TargetTypes, "Expected: the extension targets are set; Actual: extension targets are not set.");
            Assert.IsNotNull(schemaExtensions[0].Id, "Expected: the Id property is set; Actual: Id property is not set.");

            // Get a specific schema extension.
            SchemaExtension extensionFromGet = await graphClient.SchemaExtensions[schemaExtension.Id].Request().GetAsync();
            Assert.IsNotNull(extensionFromGet, "Expected: returned a schema extension object; Actual: an object wasn't returned.");

            // Add header so we get back a representation of the updated schema extension.
            List<HeaderOption> headers = new List<HeaderOption>();
            HeaderOption preferHeader = new HeaderOption("Prefer", "return=representation");
            headers.Add(preferHeader);

            // Update a specific schema extension.
            extensionFromGet.Status = "Available";

            // Potential bug: state transition from deprecated to available is not working.
            // Potential bug here as the service is not returning the SchemaExtension on update. Must delete test until this is fixed. 5/30/2017
            SchemaExtension extensionFromUpdate = await graphClient.SchemaExtensions[extensionFromGet.Id].Request(headers).UpdateAsync(extensionFromGet);
            
            // Enable or re-write test when we learn expected behavior.
            //Assert.AreEqual(extensionFromGet.Status, extensionFromUpdate.Status, "Expected: the patch object status property matches the returned status property; Actual: they don't match.");

            // Delete a specific scheam extension.
            await graphClient.SchemaExtensions[extensionFromGet.Id].Request().DeleteAsync();
            try
            {
                var deletedSchemaExtension = await graphClient.SchemaExtensions[extensionFromGet.Id].Request().GetAsync();
                Assert.Fail("Expected: ServiceException since the schema extension ws deleted; Actual: the GET on the supposedly deleted schema extension returned successfully.");
            }
            catch (ServiceException e)
            {
                Assert.AreEqual(e.StatusCode == System.Net.HttpStatusCode.NotFound, $"Expected: {System.Net.HttpStatusCode.NotFound}; Actual: {e.StatusCode}");
            }
        }
    }
}
