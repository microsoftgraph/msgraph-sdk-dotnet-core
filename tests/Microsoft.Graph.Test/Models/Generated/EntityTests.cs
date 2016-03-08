// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.Test.Models.Generated
{
    using System.Linq;
    using System.Reflection;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class EntityTests
    {
        [TestMethod]
        public void AbstractEntity_DefaultConstructorGeneration()
        {
            var entityType = typeof(Entity);
            var constructors = entityType.GetConstructors(
                BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.AreEqual(1, constructors.Count(), "Unexpected number of constructors on Entity.");

            var defaultConstructor = constructors.First();
            Assert.IsFalse(defaultConstructor.IsPrivate, "Constructor is private.");
            Assert.IsFalse(defaultConstructor.IsPublic, "Constructor is public.");
            Assert.IsFalse(defaultConstructor.IsStatic, "Constructor is static.");
            Assert.IsFalse(defaultConstructor.GetParameters().Any(), "Constructor has arguments.");
        }
    }
}