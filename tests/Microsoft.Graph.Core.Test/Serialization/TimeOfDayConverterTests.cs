// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.Core.Test.Serialization
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TimeOfDayConverterTests
    {
        private TimeOfDayConverter converter;

        [TestInitialize]
        public void Setup()
        {
            this.converter = new TimeOfDayConverter();
        }

        [TestMethod]
        public void CanConvert_TimeOfDay()
        {
            Assert.IsTrue(this.converter.CanConvert(typeof(TimeOfDay)), "Unexpected value for CanConvert.");
        }

        [TestMethod]
        public void CanConvert_TimeOfDay_InvalidType()
        {
            Assert.IsFalse(this.converter.CanConvert(typeof(DateTime)), "Unexpected value for CanConvert.");
        }
    }
}
