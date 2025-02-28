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

using System;
using System.Collections.Generic;

namespace umi3d.common.core.target
{
    public struct Platform 
    {
        public readonly string name;

        public Platform(string name) 
        { 
            this.name = name; 
        }

        public static IReadOnlyList<Platform> allCases => _allCases.Value;
        static Lazy<Platform[]> _allCases = new(() =>
        {
            return new[] { pc, pc_vr, meta, pico, vive, androidMobile };
        });

        public static readonly Platform pc = new Platform("pc");
        public static readonly Platform pc_vr = new Platform("vr on pc");
        public static readonly Platform meta = new Platform("meta");
        public static readonly Platform pico = new Platform("pico");
        public static readonly Platform vive = new Platform("vive");
        public static readonly Platform androidMobile = new Platform("android mobile");
    }
}