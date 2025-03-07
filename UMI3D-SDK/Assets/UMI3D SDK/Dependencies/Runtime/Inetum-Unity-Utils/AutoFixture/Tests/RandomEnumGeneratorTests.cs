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

using AutoFixture;
using inetum.unityUtils.autoFixture;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

public class RandomEnumGeneratorTests
{
    enum FooEnum
    {
        Foo,
        Bar,
        Baz,
        Qux
    }

    [Test]
    public void GivenAFixture_WhenApplyRandomEnumGenerator_ThenCreateARandomEnum()
    {
        Fixture fixture = new Fixture();
        fixture.Customizations.Add(new RandomEnumGenerator());

        List<FooEnum> enums = Enumerable
            .Range(1, 2)
            .Select(_ => fixture.Create<FooEnum>())
            .ToList();

        Assert.AreNotSame(enums[0], FooEnum.Foo);
        Assert.AreNotSame(enums[0], enums[1]);
    }
}
