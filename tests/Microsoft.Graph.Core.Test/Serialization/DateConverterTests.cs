// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.Core.Test.Serialization
{
    using System;
    using Microsoft.Graph;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DateConverterTests
    {
        private DateConverter converter;

        [TestInitialize]
        public void Setup()
        {
            this.converter = new DateConverter();
        }

        [TestMethod]
        public void CanConvert_Date()
        {
            Assert.IsTrue(this.converter.CanConvert(typeof(Date)), "Unexpected value for CanConvert.");
        }

        [TestMethod]
        public void CanConvert_InvalidType()
        {
            Assert.IsFalse(this.converter.CanConvert(typeof(DateTime)), "Unexpected value for CanConvert.");
        }

        [TestMethod]
        public void SerializerRoundTripsDates()
        {
            var eventIn = new Event { Start = new DateTimeTimeZone { DateTime = "2017-10-11T07:30:00.0000+00:00", TimeZone = "UTC" } };
            var serializer = new Serializer();
            var json = serializer.SerializeObject(eventIn);
            var eventRoundTrip = serializer.DeserializeObject<Event>(json);

            Assert.AreEqual(eventIn.Start.DateTime, eventRoundTrip.Start.DateTime);
        }
    }
}
