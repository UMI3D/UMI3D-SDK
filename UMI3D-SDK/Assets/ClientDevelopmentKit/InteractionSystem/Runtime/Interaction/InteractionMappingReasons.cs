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

namespace umi3d.cdk.interaction
{

    /// <summary>
    /// Motivation behind a tool selection/release.
    /// </summary>
    public abstract class InteractionMappingReason { }

    /// <summary>
    /// Motivation behind a tool selection/release.
    /// The controller have changed.
    /// </summary>
    /// <see cref="InteractionMappingReason"/>
    public class SwitchController : InteractionMappingReason { }

    /// <summary>
    /// Motivation behind a tool selection/release.
    /// The controller have changed.
    /// </summary>
    /// <see cref="InteractionMappingReason"/>
    public class ToolNeedToBeUpdated : InteractionMappingReason { }

    /// <summary>
    /// Motivation behind a tool selection/release.
    /// the tool has been selected/released as requested by the Environement.
    /// </summary>
    /// <see cref="InteractionMappingReason"/>
    public class RequestedByEnvironment : InteractionMappingReason { }

    /// <summary>
    /// Motivation behind a tool selection/release.
    /// the tool has been selected/released as requested by the User.
    /// </summary>
    /// <see cref="InteractionMappingReason"/>
    public class RequestedByUser : InteractionMappingReason { }

    /// <summary>
    /// Motivation behind a tool selection/release.
    /// the tool has been selected/released as requested by the User with a Menu.
    /// </summary>
    /// <see cref="InteractionMappingReason"/>
    /// <see cref="RequestedByUser"/>
    public class RequestedFromMenu : RequestedByUser { }


    /// <summary>
    /// Motivation behind a tool selection/release.
    /// the tool has been selected/released as requested by the User with a Selector.
    /// </summary>
    /// <see cref="InteractionMappingReason"/>
    /// <see cref="RequestedByUser"/>
    public class RequestedUsingSelector : RequestedByUser
    {
        public AbstractController controller;
    }

    /// <summary>
    /// Motivation behind a tool selection/release.
    /// the tool has been selected/released automaticaly after a hover
    /// </summary>
    /// <see cref="InteractionMappingReason"/>
    public class AutoProjectOnHover : RequestedUsingSelector { }


    /// <summary>
    /// Motivation behind a tool selection/release.
    /// the tool has been selected/released as requested with a specified Selector.
    /// </summary>
    /// <see cref="InteractionMappingReason"/>
    /// <see cref="RequestedUsingSelector"/>
    /// /// <see cref="AbstractSelector"/>
    public class RequestedUsingSelector<Selector> : RequestedUsingSelector where Selector : AbstractSelector { }

    /// <summary>
    /// Error class return after a failure of a tool selection/release.
    /// </summary>
    /// <typeparam name="OriginalReason">InteractionMappingReason for the tool selection/release</typeparam>
    /// <see cref="InteractionMappingReason"/>
    public class RollbackAfterFailure<OriginalReason> : InteractionMappingReason where OriginalReason : InteractionMappingReason { }
}