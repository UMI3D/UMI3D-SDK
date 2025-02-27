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

namespace inetum.unityUtils.versioning
{
    [Serializable]
    public struct ReleaseCycle 
    {
        public const string ALPHA = "ALPHA";
        public const string BETA = "BETA";
        public const string PROD = "PROD";

        public readonly string name;
        public readonly string scriptingSymbol;
        public readonly string initial;
        public readonly string description;

        public ReleaseCycle(string name, string scriptingSymbol, string initial, string description)
        {
            this.name = name;
            this.scriptingSymbol = scriptingSymbol;
            this.initial = initial;
            this.description = description;
        }

        public static IReadOnlyList<ReleaseCycle> allCases => _allCases.Value;
        static Lazy<ReleaseCycle[]> _allCases = new(() =>
        {
            return new[] { alpha, beta, production };
        });

        public static readonly ReleaseCycle alpha = new ReleaseCycle(
            "alpha", 
            ALPHA,
            "a", 
            "Show more logs and active the development build."
        );
        public static readonly ReleaseCycle beta = new ReleaseCycle(
            "beta", 
            BETA,
            "b", 
            "Show more logs."
        );
        public static readonly ReleaseCycle production = new ReleaseCycle(
            "production", 
            PROD,
            "p", 
            "Less logs."
        );
    }
}