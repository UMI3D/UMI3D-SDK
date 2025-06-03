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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using umi3d.common.interaction;

namespace umi3d.cdk.interaction
{
    public interface ISelectorDataDelegate 
    {
        /// <summary>
        /// How many tools can be projected at the same time on the controllers of this selector.
        /// </summary>
        int toolCountLimitation { get; }

        void AssociateInteractionAndInput(List<(Interaction interaction, Input input)> associations, ReadOnlyDictionary<Interaction, ReadOnlyCollection<Input>> inputsByInteractions);

        // ----- Button action type -----
        /// <summary>
        /// Try to find all inputs compatible with this interactions.<br/>
        /// </summary>
        /// <param name="inputs">The list of compatible inputs. Should be filled with compatible inputs at the end of the method.</param>
        /// <param name="interaction">The interaction.</param>
        /// <returns></returns>
        bool TryGetInputsFor(out ReadOnlyCollection<Input> inputs, EventDto interaction);
        /// <summary>
        /// Try to find all inputs compatible with this interactions.<br/>
        /// </summary>
        /// <param name="inputs">The list of compatible inputs. Should be filled with compatible inputs at the end of the method.</param>
        /// <param name="interaction">The interaction.</param>
        /// <returns></returns>
        bool TryGetInputsFor(out ReadOnlyCollection<Input> inputs, DrawingDto interaction);

        // ----- PathThrough bool action type -----
        /// <summary>
        /// Try to find all inputs compatible with this interactions.<br/>
        /// </summary>
        /// <param name="inputs"></param>
        /// <param name="interaction"></param>
        /// <returns></returns>
        bool TryGetInputsFor(out ReadOnlyCollection<Input> inputs, BooleanParameterDto interaction);

        // ----- PathThrough float or int action type -----
        /// <summary>
        /// Try to find all inputs compatible with this interactions.<br/>
        /// </summary>
        /// <param name="inputs"></param>
        /// <param name="interaction"></param>
        /// <returns></returns>
        bool TryGetInputsFor(out ReadOnlyCollection<Input> inputs, FloatParameterDto interaction);
        /// <summary>
        /// Try to find all inputs compatible with this interactions.<br/>
        /// </summary>
        /// <param name="inputs"></param>
        /// <param name="interaction"></param>
        /// <returns></returns>
        bool TryGetInputsFor(out ReadOnlyCollection<Input> inputs, IntegerParameterDto interaction);

        // ----- PathThrough axis float or int action type -----
        /// <summary>
        /// Try to find all inputs compatible with this interactions.<br/>
        /// </summary>
        /// <param name="inputs"></param>
        /// <param name="interaction"></param>
        /// <returns></returns>
        bool TryGetInputsFor(out ReadOnlyCollection<Input> inputs, FloatRangeParameterDto interaction);
        /// <summary>
        /// Try to find all inputs compatible with this interactions.<br/>
        /// </summary>
        /// <param name="inputs"></param>
        /// <param name="interaction"></param>
        /// <returns></returns>
        bool TryGetInputsFor(out ReadOnlyCollection<Input> inputs, IntegerRangeParameterDto interaction);

        // ----- PathThrough vector2, 3 or 4 action type -----
        /// <summary>
        /// Try to find all inputs compatible with this interactions.<br/>
        /// </summary>
        /// <param name="inputs"></param>
        /// <param name="interaction"
        /// <returns></returns>
        bool TryGetInputsFor(out ReadOnlyCollection<Input> inputs, Vector2ParameterDto interaction);
        /// <summary>
        /// Try to find all inputs compatible with this interactions.<br/>
        /// </summary>
        /// <param name="inputs"></param>
        /// <param name="interaction"></param>
        /// <returns></returns>
        bool TryGetInputsFor(out ReadOnlyCollection<Input> inputs, Vector3ParameterDto interaction);
        /// <summary>
        /// Try to find all inputs compatible with this interactions.<br/>
        /// </summary>
        /// <param name="inputs"></param>
        /// <param name="interaction"></param>
        /// <returns></returns>
        bool TryGetInputsFor(out ReadOnlyCollection<Input> inputs, Vector4ParameterDto interaction);

        // Custom ui input system.
        /// <summary>
        /// Try to find all inputs compatible with this interactions.<br/>
        /// </summary>
        /// <param name="inputs"></param>
        /// <param name="interaction"></param>
        /// <returns></returns>
        bool TryGetInputsFor(out ReadOnlyCollection<Input> inputs, StringParameterDto interaction);
        /// <summary>
        /// Try to find all inputs compatible with this interactions.<br/>
        /// </summary>
        /// <param name="inputs"></param>
        /// <param name="interaction"></param>
        /// <returns></returns>
        bool TryGetInputsFor(out ReadOnlyCollection<Input> inputs, ColorParameterDto interaction);
        /// <summary>
        /// Try to find all inputs compatible with this interactions.<br/>
        /// </summary>
        /// <param name="inputs"></param>
        /// <param name="interaction"></param>
        /// <returns></returns>
        bool TryGetInputsFor(out ReadOnlyCollection<Input> inputs, EnumParameterDto<string> interaction);
        /// <summary>
        /// Try to find all inputs compatible with this interactions.<br/>
        /// </summary>
        /// <param name="inputs"></param>
        /// <param name="interaction"></param>
        /// <returns></returns>
        bool TryGetInputsFor(out ReadOnlyCollection<Input> inputs, UploadFileParameterDto interaction);
        /// <summary>
        /// Try to find all inputs compatible with this interactions.<br/>
        /// </summary>
        /// <param name="inputs"></param>
        /// <param name="interaction"></param>
        /// <returns></returns>
        bool TryGetInputsFor(out ReadOnlyCollection<Input> inputs, LocalInfoRequestParameterDto interaction);
    }
}