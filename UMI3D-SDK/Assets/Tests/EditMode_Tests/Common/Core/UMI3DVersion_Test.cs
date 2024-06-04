/*
Copyright 2019 - 2023 Inetum

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

using NUnit.Framework;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using umi3d;
using umi3d.common;
using umi3d.common.userCapture.pose;
using UnityEngine;

namespace EditMode_Tests
{
    public class UMI3DVersion_Test
    {

        [OneTimeSetUp]
        public virtual void TimeSetUp()
        {

        }

        [OneTimeTearDown]
        public virtual void Teardown()
        {

        }

        [Test]
        public void ParseVersion()
        {
            int major = 2;
            int minor = 3;
            string status = "b";
            DateTime date = DateTime.Now;

            var version = new UMI3DVersion.Version(major, minor, status, date);
            var version2 = new UMI3DVersion.Version(version.version);

            Assert.AreEqual(version.major, version2.major);
            Assert.AreEqual(version.minor, version2.minor);
            Assert.AreEqual(version.status, version2.status);
            Assert.AreEqual(version.date?.ToString("yyMMdd"), version2.date?.ToString("yyMMdd"));
        }

        [Test]
        public void ParseVersion2()
        {
            int major = 2;
            int minor = 3;
            string status = "b";
            DateTime date = DateTime.Now;

            var version = new UMI3DVersion.Version("2.9.b.240529");
            var version2 = new UMI3DVersion.Version("2.9.b.240604");

            UMI3DVersion.VersionCompatibility _version = new UMI3DVersion.VersionCompatibility("2.7", "*");
            UMI3DVersion.VersionCompatibility _version2 = new UMI3DVersion.VersionCompatibility("2.7", "2.9.*.240529");


            Assert.IsTrue(_version.IsCompatible(version));
            Assert.IsTrue(_version2.IsCompatible(version));

            Assert.IsTrue(_version.IsCompatible(version2));
            Assert.IsTrue(!_version2.IsCompatible(version2));
        }
    }
}