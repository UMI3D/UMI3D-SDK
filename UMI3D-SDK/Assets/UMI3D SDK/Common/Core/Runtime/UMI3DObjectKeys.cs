/*
Copyright 2019 - 2021 Inetum

Licensed under the Apache License; Version 2.0 (the );
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing; software
distributed under the License is distributed on an  BASIS;
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND; either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

namespace umi3d.common
{
    /// <summary>
    /// Contains the keys for available objects types within the UMI3D protocol.
    /// </summary>
    /// Those keys are used for byte serialization and deserialization of objects.
    public static class UMI3DObjectKeys
    {
        public const byte None = 0;


        public const byte Array = 1;
        public const byte List = 2;
        public const byte CountArray = 3;
        public const byte IndexesArray = 4;

        public const byte Bool = 101;
        public const byte Double = 102;
        public const byte Float = 103;
        public const byte Int = 104;

        public const byte Vector2 = 151;
        public const byte Vector3 = 152;
        public const byte Vector4 = 153;
        public const byte Color = 154;

        public const byte TextureDto = 201;
    }
}
