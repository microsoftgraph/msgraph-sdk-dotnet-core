// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public partial class PlannerExternalReferences : IEnumerable<KeyValuePair<string, PlannerExternalReference>>
    {
        private static readonly string[,] Conversions = new string[,] { { "%", "%25" }, { "@", "%40" }, { ".", "%2E" }, { ":", "%3A" } };

        public PlannerExternalReference this[string url]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(url))
                {
                    throw new ArgumentNullException(nameof(url));
                }

                if (!this.AdditionalData.TryGetValue(Encode(url), out object referenceObject))
                {
                    return null;
                }

                return referenceObject as PlannerExternalReference;
            }

            set
            {
                if (string.IsNullOrWhiteSpace(url))
                {
                    throw new ArgumentNullException(nameof(url));
                }

                this.AdditionalData[Encode(url)] = value;
            }
        }

        public void AddReference(string url, string alias)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            if (string.IsNullOrEmpty(alias))
            {
                throw new ArgumentNullException(nameof(alias));
            }

            var plannerExternalReference = new PlannerExternalReference();

            plannerExternalReference.Alias = alias;

            this.AdditionalData.Add(Encode(url), plannerExternalReference);
        }

        public IEnumerator<KeyValuePair<string, PlannerExternalReference>> GetEnumerator()
        {
            return this.AdditionalData
               .Where(kvp => kvp.Value is PlannerExternalReference)
               .Select(kvp => new KeyValuePair<string, PlannerExternalReference>(Decode(kvp.Key), (PlannerExternalReference)kvp.Value))
               .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private static string Encode(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            for (int i = 0; i < Conversions.GetLength(0); i++)
            {
                propertyName = propertyName.Replace(Conversions[i, 0], Conversions[i, 1]);
            }

            return propertyName;
        }

        private static string Decode(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            for (int i = Conversions.GetLength(0) - 1; i >= 0; i--)
            {
                propertyName = propertyName.Replace(Conversions[i, 1], Conversions[i, 0]);
            }

            return propertyName;
        }
    }
}
