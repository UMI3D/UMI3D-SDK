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

using System;
using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    public class UMI3DAnimatorAnimation : UMI3DAbstractAnimation
    {
        new public static UMI3DAnimatorAnimation Get(string id) { return UMI3DAbstractAnimation.Get(id) as UMI3DAnimatorAnimation; }
        protected new UMI3DAnimatorAnimationDto dto { get => base.dto as UMI3DAnimatorAnimationDto; set => base.dto = value; }

        bool started = false;

        public UMI3DAnimatorAnimation(UMI3DAnimatorAnimationDto dto) : base(dto)
        {
        }

        ///<inheritdoc/>
        public override float GetProgress()
        {
            return 0;
        }

        ///<inheritdoc/>
        public override void Start()
        {
            Debug.Log("Start");
            if (started) return;
            GameObject go = (UMI3DEnvironmentLoader.GetEntity(dto.nodeId) as UMI3DNodeInstance)?.gameObject;
            
            go.GetComponentInChildren<Animator>().Play(dto.stateName);        
        }

        ///<inheritdoc/>
        public override void Stop()
        {
        }

        ///<inheritdoc/>
        public override void OnEnd()
        {
            started = false;
            base.OnEnd();
        }

        bool LaunchAnimation(float waitFor)
        {
            return dto.playing && GetProgress() >= waitFor;
        }

        IEnumerator WaitForProgress(float waitFor, Action action)
        {
            yield return new WaitUntil(() => LaunchAnimation(waitFor));
            action.Invoke();
        }

        IEnumerator Playing(Action action)
        {
            yield return null;
            action.Invoke();
        }

        ///<inheritdoc/>
        public override bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            if (base.SetUMI3DProperty(entity, property)) return true;
            switch (property.property)
            {
                case UMI3DPropertyKeys.AnimationNodeId:
                    dto.nodeId = (string)property.value;
                    break;
                case UMI3DPropertyKeys.AnimationStateName:
                    dto.stateName = (string)property.value;
                    break;
                default:
                    return false;
            }

            return true;
        }

        ///<inheritdoc/>
        public override void Start(float atTime)
        {
        }

        public override void SetProgress(long frame)
        {
        }
    }
}