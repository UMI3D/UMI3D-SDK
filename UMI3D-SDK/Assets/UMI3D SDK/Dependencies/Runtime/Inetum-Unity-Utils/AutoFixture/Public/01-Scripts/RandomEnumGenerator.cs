/*
Copyright 2019 - 2025 Inetum

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using AutoFixture.Kernel;
using System;

namespace inetum.unityUtils.autoFixture
{
    /// <summary>
    /// AutoFixture random generator for enums
    /// </summary>
    public class RandomEnumGenerator : ISpecimenBuilder
    {
        private static readonly Random _randomizer = new();

        /// <summary>
        /// This method creates a random enum value based on the provided request and context.<br/>
        /// <br/>
        /// <example>
        /// Given a fixture when applying a random enum generator then create a random enum.<br/>
        /// <code>
        /// public void GivenAFixture_WhenApplyRandomEnumGenerator_ThenCreateARandomEnum()
        /// {
        ///     Fixture fixture = new Fixture();
        ///     fixture.Customizations.Add(new RandomEnumGenerator());
        ///
        ///     List&lt;FooEnum&gt; enums = Enumerable
        ///         .Range(1, 2)
        ///         .Select(_ => fixture.Create&lt;FooEnum&gt;())
        ///         .ToList();
        /// }
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="request">The request object which should be a SeededRequest containing a Type.</param>
        /// <param name="context">The context in which the specimen is created.</param>
        /// <returns>A random enum value if the request is a valid enum type; otherwise, a NoSpecimen object.</returns>
        public object Create(object request, ISpecimenContext context)
        {
            SeededRequest seededRequest = request as SeededRequest;
            Type type = seededRequest?.Request as Type;
            if (type == null || !type.IsEnum)
            {
                return new NoSpecimen();
            }

            var values = Enum.GetValues(type);
            var index = _randomizer.Next(values.Length);
            return values.GetValue(index)!;
        }
    }
}