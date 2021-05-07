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
    static public class UMI3DOperationKeys
    {
        public const uint Transaction = 1;
        public const uint LoadEntity = 2;
        public const uint DeleteEntity = 3;

        public const uint SetEntityProperty = 101;
        public const uint SetEntityDictionnaryProperty = 103;
        public const uint SetEntityDictionnaryAddProperty = 104;
        public const uint SetEntityDictionnaryRemoveProperty = 105;
        public const uint SetEntityListProperty = 106;
        public const uint SetEntityListAddProperty = 107;
        public const uint SetEntityListRemoveProperty = 108;
        public const uint SetEntityMatrixProperty = 109;

        public const uint MultiSetEntityProperty = 110;


        public const uint ProjectTool = 200;
        public const uint SwitchTool = 201;
        public const uint ReleaseTool = 202;

        public const uint SetUTSTargetFPS = 300;
        public const uint SetStreamedBones = 301;
        public const uint StartInterpolationProperty =302;
        public const uint StopInterpolationProperty = 303;

    }
}
