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

namespace umi3d.cdk.interaction.selection.feedback
{
    /// <summary>
    /// Interface for selection feedbacks
    /// </summary>
    public interface IFeedback
    { }

    /// <summary>
    /// Interface for feedback that happens once
    /// </summary>
    public interface IInstantaneousFeedback : IFeedback
    {
        void Trigger();
    }

    /// <summary>
    /// Interface for feedbacks that have an ON/OFF mode
    /// </summary>
    public interface IPersistentFeedback : IFeedback
    {
        /// <summary>
        /// Activate a feedback that persists in time
        /// </summary>
        /// <param name="selectionData"></param>
        void Activate(ISelectionData selectionData);

        /// <summary>
        /// Dectivate a feedback that persists in time
        /// </summary>
        /// <param name="selectionData"></param>
        void Deactivate(ISelectionData selectionData);
    }
}