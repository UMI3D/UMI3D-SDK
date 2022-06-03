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

namespace umi3d.cdk.interaction.selection.projector
{
    /// <summary>
    /// Projector interface. Projects interactions on controllers
    /// </summary>
    /// <typeparam name="T">Selectable or Interactable</typeparam>
    public interface IProjector<T>
    {
        /// <summary>
        /// Enables the interaction of an object on a controller
        /// </summary>
        /// <param name="objToProjec"></param>
        /// <param name="controller"></param>
        void Project(T objToProjec, AbstractController controller);
    }
}