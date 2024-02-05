﻿/*
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
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace umi3d.cdk
{
    /// <summary>
    /// Object containing a dto for entity with gameobject
    /// </summary>
    public class UMI3DNodeInstance : UMI3DEntityInstance
    {
        public GameObject gameObject;
        public virtual Transform transform => gameObject.transform;

        private bool isPartOfNavmesh = false;

        public GameObject mainInstance;
        /// <summary>
        /// Is this node part of the navmesh ?
        /// </summary>
        public bool IsPartOfNavmesh
        {
            get => isPartOfNavmesh;

            set
            {
                if (isPartOfNavmesh != value)
                {
                    isPartOfNavmesh = value;
                    UMI3DEnvironmentLoader.Instance.SetNodePartOfNavmesh(this);
                }
            }
        }


        private bool isTraversable = true;

        /// <summary>
        /// Is this node traversable ?
        /// </summary>
        public bool IsTraversable
        {
            get => isTraversable;

            set
            {
                if (isTraversable != value)
                {
                    isTraversable = value;
                    UMI3DEnvironmentLoader.Instance.SetNodeTraversable(this);
                }
            }
        }

        /// <summary>
        /// Event call when the transform is updated.
        /// </summary>
        public UnityEvent OnPoseUpdated = new UnityEvent();

        public void SendOnPoseUpdated() { OnPoseUpdated.Invoke(); }

        public override string ToString()
        {
            if (dto is GlTFNodeDto gltf)
                return $"UMI3DNodeInstance [{dto} : {gltf.name} : {gltf.extensions?.umi3d} : {Object} : {gameObject}]";
            return $"UMI3DNodeInstance [{dto} : {Object} : {gameObject}]";
        }

        public bool updatePose = true;

        private List<Renderer> _renderers;
        public List<Renderer> renderers
        {
            get
            {
                if (_renderers == null)
                    _renderers = new List<Renderer>();
                return _renderers;
            }
            set => _renderers = value;
        }

        private List<Collider> _colliders;
        public List<Collider> colliders
        {
            get
            {
                if (_colliders == null)
                    _colliders = new List<Collider>();
                return _colliders;
            }
            set => _colliders = value;
        }

        public PrefabLightmapData prefabLightmapData;

        /// <summary>
        /// The list of Subnode instance when the model has tracked subMeshs. Empty if sub Models are not tracked.
        /// </summary>
        private List<UMI3DNodeInstance> _subNodeInstances;

        /// <summary>
        /// If this object is a Unity Scene Bundle.
        /// </summary>
        public Scene scene;

        public UMI3DNodeInstance(Action loadedCallback) : base(loadedCallback)
        {
        }

        public List<UMI3DNodeInstance> subNodeInstances
        {
            get
            {
                if (_subNodeInstances == null)
                    _subNodeInstances = new List<UMI3DNodeInstance>();
                return _subNodeInstances;
            }
            set => _subNodeInstances = value;
        }

        /// <summary>
        /// Clean all loaded data before destroying <see cref="gameObject"/>.
        /// </summary>
        public virtual async void ClearBeforeDestroy()
        {
            if (scene != null && scene.isLoaded)
            {
                var op = SceneManager.UnloadSceneAsync(scene);

                while (!op.isDone)
                    await UMI3DAsyncManager.Yield();

                LightProbes.TetrahedralizeAsync();
            }
        }
    }
}