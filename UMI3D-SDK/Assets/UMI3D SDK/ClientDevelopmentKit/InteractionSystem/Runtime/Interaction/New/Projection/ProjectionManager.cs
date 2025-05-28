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

using inetum.unityUtils.observation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace umi3d.cdk.interaction
{
    public sealed class ProjectionManager 
    {
        #region Initialize

        static Lazy<ProjectionManager> _default = new(() => new());
        public static ProjectionManager @default => _default.Value;

        ProjectionManager()
        {
        }

        #endregion

        public readonly Delegates<IProjectionDelegate> delegates = new();

        List<Projection> _projections = new();
        public ReadOnlyCollection<Projection> projections => _projections.AsReadOnly();

        internal Projection Project(
            Selector selector, 
            Controller controller, 
            Tool tool, 
            Interaction interaction, 
            Input input
        )
        {
            Projection projection = _projections.Find(projection => 
                projection.selector == selector 
                && projection.controller == controller 
                && projection.tool == tool 
                && projection.interaction == interaction 
                && projection.input == input
            );

            if (projection == null)
            {
                projection = new(selector, controller, tool, interaction, input);
                _projections.Add(projection);
            }

            delegates.ForEach(@delegate =>
            {
                @delegate.OnProjected(projection);
                return Flow.Continue;
            });

            return projection;
        }


        internal bool Release(Projection projection)
        {
            bool result = _projections.Remove(projection);

            if (result)
            {
                projection.Clear();

                delegates.ForEach(@delegate =>
                {
                    @delegate.OnReleased(projection);
                    return Flow.Continue;
                });
            }

            return result;
        }
    }
}