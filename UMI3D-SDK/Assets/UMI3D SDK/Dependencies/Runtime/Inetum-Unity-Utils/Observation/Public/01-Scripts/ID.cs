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

namespace inetum.unityUtils.observation
{
    public struct ID 
    {
        string _id;
        public string id => _id;

        public ID(string id)
        {
            this._id = id;
        }

        public static ID FromType<T>()
        {
            return new ID(typeof(T).FullName);
        }

        public static implicit operator string(ID id)
        {
            return id.id;
        }

        public static implicit operator ID(string id)
        {
            return new ID(id);
        }

        public override bool Equals(object obj)
        {
            if (obj is string str) { return id == str; }
            else { return base.Equals(obj); }
        }

        public override int GetHashCode()
        {
            return _id?.GetHashCode() ?? base.GetHashCode();
        }

        public override string ToString()
        {
            return _id?.ToString() ?? "";
        }
    }
}