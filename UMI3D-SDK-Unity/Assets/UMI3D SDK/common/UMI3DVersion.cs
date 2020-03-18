/*
Copyright 2019 Gfi Informatique

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
using UnityEngine;
using System.Collections;

namespace umi3d
{
    static public class UMI3DVersion
    {
        public static string version { get { return major + "." + minor + "." + status + "." + date; } }
        public readonly static string major = "1";
        public readonly static string minor = "3";
        public readonly static string status = "b";
        public readonly static string date = "200318";

    }
}