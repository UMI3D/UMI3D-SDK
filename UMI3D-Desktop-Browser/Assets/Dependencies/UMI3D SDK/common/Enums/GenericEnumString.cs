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


namespace umi3d.common
{
    public class GenericEnumString
    {
        protected GenericEnumString(string value) { Value = value; }

        public readonly string Value;

        //Operators
        public static bool operator ==(GenericEnumString a, GenericEnumString b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return a.Value.Equals(b.Value);
        }
        public static bool operator !=(GenericEnumString a, GenericEnumString b)
        {
            return !(a == b);
        }
        public static bool operator ==(GenericEnumString a, string b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return a.Value.Equals(b);
        }
        public static bool operator !=(GenericEnumString a, string b)
        {
            return !(a == b);
        }
        public static bool operator ==(string a, GenericEnumString b)
        {
            return b == a;
        }
        public static bool operator !=(string a, GenericEnumString b)
        {
            return b != a;
        }

        public override bool Equals(System.Object obj)
        {
            return obj == null ? false : Equals(obj.ToString());
        }
        public bool Equals(GenericEnumString p)
        {
            // Return true if the fields match:
            return Value.Equals(p.Value);
        }
        public bool Equals(string s)
        {
            // Return true if the fields match:
            return Value.Equals(s);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override string ToString()
        {
            return Value;
        }

        public static implicit operator string(GenericEnumString instance)
        {
            if (instance == null)
            {
                return null;
            }
            return instance.Value;
        }

    }
}
