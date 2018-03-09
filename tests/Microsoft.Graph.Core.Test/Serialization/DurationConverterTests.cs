// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.Core.Test.Serialization
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;
    using System.IO;

    //[Ignore]
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
            var json = "\"PT2H\"";
            var serializer = new Serializer();
            var derivedType = serializer.DeserializeObject<Duration>(json);
            Assert.IsNotNull(derivedType, "Object not correctly deserialized.");
            Assert.AreEqual(2, derivedType.TimeSpan.Hours);
        }
    }
}
