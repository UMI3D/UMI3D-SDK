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

using UnityEngine;

namespace umi3d.cdk.interaction.selection.feedback
{
    /// <summary>
    /// Base class for selection feedbkac handlers
    /// Mostly useful for Unity's serialization
    /// </summary>
    public abstract class AbstractSelectionFeedbackHandler<T> : MonoBehaviour where T : MonoBehaviour
    {
        /// <summary>
        /// True if the selection feedback is ON
        /// </summary>
        protected bool isRunning = false;

        /// <summary>
        /// Activates all the necessary feedbacks related to selection
        /// </summary>
        /// <param name="selectionData"></param>
        public abstract void StartFeedback(ISelectionData selectionData);

        /// <summary>
        /// Ends all the necessary feedbacks related to selection
        /// </summary>
        public abstract void EndFeedback(ISelectionData selectionData);
    }
}