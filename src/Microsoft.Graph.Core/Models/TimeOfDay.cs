// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;

    using Newtonsoft.Json;

    [JsonConverter(typeof(TimeOfDayConverter))]
    public class TimeOfDay
    {
        internal DateTime DateTime { get; set; }

        internal TimeOfDay(DateTime dateTime)
        {
            this.DateTime = dateTime;
        }

        public TimeOfDay(int hour, int minute, int second)
            : this(new DateTime(1, 1, 1, hour, minute, second))
        {
        }
        
        public int Hour
        {
            get
            {
                return this.DateTime.Hour;
            }
        }

        public int Minute
        {
            get
            {
                return this.DateTime.Minute;
            }
        }

        public int Second
        {
            get
            {
                return this.DateTime.Second;
            }
        }

        public override string ToString()
        {
            return this.DateTime.ToString("HH:mm:ss");
        }
    }
}
