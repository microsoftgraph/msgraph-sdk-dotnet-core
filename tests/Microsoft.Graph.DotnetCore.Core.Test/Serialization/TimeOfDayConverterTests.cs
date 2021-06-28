// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.Serialization
{
    using System;
    using Xunit;

    public class TimeOfDayConverterTests
    {
        private TimeOfDayConverter converter;

        public TimeOfDayConverterTests()
        {
            this.converter = new TimeOfDayConverter();
        }

        [Fact]
        public void CanConvert_TimeOfDay()
        {
            Assert.True(this.converter.CanConvert(typeof(TimeOfDay)));
        }

        [Fact]
        public void CanConvert_TimeOfDay_InvalidType()
        {
            Assert.False(this.converter.CanConvert(typeof(DateTime)));
        }

        [Fact]
        public void CanSerializeAndDeserializeTimeOfDay()
        {
            // Arrange
            var serializer = new Serializer();
            var stringToDeserialize = "\"06:00:00\"";
            var expectedTimeDate = new TimeOfDay(6, 0, 0);
            
            // Act
            var timeOfDay = serializer.DeserializeObject<TimeOfDay>(stringToDeserialize);

            var timeOfDayString = serializer.SerializeObject(timeOfDay);

            // Assert
            Assert.Equal(expectedTimeDate.ToString(), timeOfDay.ToString());
            Assert.Equal(timeOfDayString, stringToDeserialize);
        }
    }
}
