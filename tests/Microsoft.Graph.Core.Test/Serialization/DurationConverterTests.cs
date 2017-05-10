// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.Core.Test.Serialization
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using System.IO;

    [Ignore]
    [TestClass]
    public class DurationConverterTests
    { 

        private DurationConverter converter;

        [TestInitialize]
        public void Setup()
        {
            this.converter = new DurationConverter();
        }

        [TestMethod]
        public void CanConvert_Duration()
        {
            Assert.IsTrue(this.converter.CanConvert(typeof(Duration)), "Unexpected value for CanConvert. Not a Duration object.");
        }

        [TestMethod]
        public void CanConvert__Duration_InvalidType()
        {
            Assert.IsFalse(this.converter.CanConvert(typeof(DateTime)), "Unexpected value for CanConvert.");
        }
        
        [TestMethod]
        public void Duration_CanDeserialize()
        {
            string json = @"{'meetingDuration': 'PT2H'}";
            JsonTextReader reader = new JsonTextReader(new StringReader(json));

            using (MemoryStream stream = new MemoryStream())
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(json);
            }

                if (reader.Read())
                {
                    var durationConverter = new DurationConverter();
                    var type = typeof(Duration);
                var duration = durationConverter.ReadJson(reader, type, null, new Newtonsoft.Json.JsonSerializer());
                Assert.IsTrue(type == duration.GetType());
                }
                else
                {
                    Assert.Fail("Did not read a JSON token.");
                }
        }
    }
}
