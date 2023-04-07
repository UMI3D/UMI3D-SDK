/*
Copyright 2019 - 2021 Inetum

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
using System.Globalization;

namespace umi3d
{
    /// <summary>
    /// Contains the current UMI3D version number.
    /// </summary>
    public static class UMI3DVersion
    {
        /// <summary>
        /// Current UMI3D SDK full version number.
        /// </summary>
        public static string version => major + "." + minor + "." + status + "." + date;
        public static readonly string major = "2";
        public static readonly string minor = "6";
        public static readonly string status = "p";
        public static readonly string date = "280323";


        public class Version
        {
            public readonly string version;
            public readonly int? major;
            public readonly int? minor;
            public readonly string status;
            public readonly DateTime? date;

            public Version(string version)
            {
                this.version = version;

                var split = version.Split('.');

                int res;
                DateTime dt;

                if (split.Length > 0 && int.TryParse(split[0], out res)) major = res;
                if (split.Length > 1 && int.TryParse(split[1], out res)) minor = res;
                status = (split.Length > 2) ? split[2] : null;
                if (split.Length > 3 && DateTime.TryParseExact(split[3], "yyMMdd", System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                    date = dt;



                this.version = date?.ToString("yyMMdd");
                this.version = (this.version == null) ? status : (status ?? "*") + this.version;
                this.version = (this.version == null) ? minor?.ToString() : (minor?.ToString() ?? "*") + this.version;
                this.version = (this.version == null) ? major?.ToString() : (major?.ToString() ?? "*") + this.version;
            }

            public Version(int major, int minor, string status, DateTime date)
            {
                this.version = $"{major}.{minor}.{status}.{date.ToString("yyMMdd")}";
                this.major = major;
                this.minor = minor;
                this.status = status;
                this.date = date;
            }
        }

        public class VersionCompatibility
        {
            public readonly int major_max;
            public readonly int major_min;

            public readonly int minor_max;  
            public readonly int minor_min;  

            public readonly string status;
            public readonly bool status_all = false;

            public readonly DateTime date_max;
            public readonly DateTime date_min;


            public VersionCompatibility(string min, string max) : this(new Version(min),new Version(max))
            { }

            public VersionCompatibility(Version min, Version max)
            {
                major_max = max.major ?? int.MaxValue;
                major_min= min.major ?? int.MinValue;

                minor_max= max.minor ?? int.MaxValue;
                minor_min = min.minor ?? int.MinValue;

                if (min.status != max.status || min.status == null || min.status == string.Empty)
                {
                    status = "*";
                    status_all = true;
                }
                else
                    status = min.status;

                date_max = min.date ?? DateTime.MaxValue;
                date_min = min.date ?? DateTime.MinValue;
            }

            public VersionCompatibility(string pattern)
            {
                var split = pattern.Split('.');

                (major_min, major_max) = MatchPatterInt(split.Length > 0 ? split[0] : string.Empty);
                (minor_min, minor_max) = MatchPatterInt(split.Length > 1 ? split[1] : string.Empty);
                status = split.Length > 2 ? split[2] : "*";
                status_all = status.Equals("*");
                (date_min, date_max) = MatchPatterDate(split.Length > 3 ? split[3] : string.Empty);
            }

            (int min, int max) MatchPatterInt(string pattern)
            {
                int min = int.MinValue, max = int.MaxValue;
                int result;

                if (pattern == null || pattern == string.Empty || pattern == "*") { }
                else if (pattern.Contains("-"))
                {
                    var split2 = pattern.Split('-');

                    if (int.TryParse(split2[0], out result))
                        min = result;

                    if (split2.Length > 1 && int.TryParse(split2[1], out result))
                            max = result;
                }
                else if (int.TryParse(pattern, out result))
                    max = min = result;
                
                return (min, max);
            }

            (DateTime min, DateTime max) MatchPatterDate(string pattern)
            {
                DateTime min = DateTime.MinValue, max = DateTime.MaxValue;
                DateTime result;

                if (pattern == null || pattern == string.Empty || pattern == "*") { }
                else if (pattern.Contains("-")) 
                {
                    var split2 = pattern.Split('-');

                    if (DateTime.TryParseExact(split2[0], "yyMMdd", System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                        min = result;

                    if (split2.Length > 1 && DateTime.TryParseExact(split2[1], "yyMMdd", System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                        max = result;
                }
                else if (DateTime.TryParseExact(pattern, "yyMMdd", System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                    max = min = result;
                
                return (min, max);
            }

            public bool IsCompatible(Version version)
            {
                if(version == null)
                    return false;

                if (version.major < major_min || version.major > major_max) return false;
                if (version.minor < minor_min || version.minor > minor_max) return false;
                if(!status_all && status != version.status) return false;
                if(version.date < date_min || version.date > date_max) return false;

                return true;
            }

        }
    }
}