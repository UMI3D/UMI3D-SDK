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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using umi3d.common;
using UnityEngine;
using System.Threading;
using umi3d.cdk.utils.extrapolation;
using umi3d.common.utils.serialization;
using MainThreadDispatcher;

namespace umi3d.cdk
{
    public class UMI3DEntities
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.Core | DebugScope.Loading;

        public readonly ulong EnvironmentId;
        public readonly string ReourcesUrl;

        /// <summary>
        /// Index of any 3D object loaded.
        /// </summary>
        private readonly Dictionary<ulong, UMI3DEntityInstance> entities = new Dictionary<ulong, UMI3DEntityInstance>();
        private readonly Dictionary<ulong, List<(Action<UMI3DEntityInstance>, Action)>> entitywaited = new Dictionary<ulong, List<(Action<UMI3DEntityInstance>, Action)>>();
        private readonly HashSet<ulong> entityToBeLoaded = new HashSet<ulong>();
        private readonly HashSet<ulong> entityFailedToBeLoaded = new HashSet<ulong>();

        /// <summary>
        /// Call a callback when an entity is registerd.
        /// The entity might not be totaly loaded when the callback is called.
        /// all property of UMI3DEntityInstance and UMI3DNodeInstance are set.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="entityLoaded"></param>
        /// <param name="entityFailedToLoad"></para
        public virtual void WaitUntilEntityLoaded(ulong id, Action<UMI3DEntityInstance> entityLoaded, Action entityFailedToLoad = null)
        {
            UMI3DEntityInstance node = TryGetEntityInstance(id);
            if (node != null && node.IsLoaded)
            {
                entityLoaded?.Invoke(node);
            }
            else
            {
                if (entitywaited.ContainsKey(id))
                    entitywaited[id].Add((entityLoaded, entityFailedToLoad));
                else
                    entitywaited[id] = new List<(Action<UMI3DEntityInstance>, Action)>() { (entityLoaded, entityFailedToLoad) };
            }

            return;
        }

        public virtual async Task<UMI3DEntityInstance> WaitUntilEntityLoaded(ulong id, List<CancellationToken> tokens)
        {
            UMI3DEntityInstance node = TryGetEntityInstance(id);
            if (node != null && node.IsLoaded)
            {
                return (node);
            }
            UMI3DEntityInstance loaded = null;
            bool error = false;
            bool finished = false;

            Action<UMI3DEntityInstance> entityLoaded = (e) => { loaded = e; finished = true; };
            Action entityFailedToLoad = () => { error = true; finished = true; };

            WaitUntilEntityLoaded(id, entityLoaded, entityFailedToLoad);

            while (!finished)
                await UMI3DAsyncManager.Yield(tokens, false);
            if (error)
                throw new Umi3dException($"Failed to load entity. Entity id: {id}.");

            return loaded;
        }

        private bool NotifyEntityToBeLoaded(ulong id)
        {
            return entityToBeLoaded.Add(id);
        }

        private bool IsEntityToBeLoaded(ulong id)
        {
            return entityToBeLoaded.Contains(id);
        }
        private bool IsEntityToFailedBeLoaded(ulong id)
        {
            return entityFailedToBeLoaded.Contains(id);
        }

        private bool RemoveEntityToFailedBeLoaded(ulong id)
        {
            return entityFailedToBeLoaded.Remove(id);
        }

        private void NotifyEntityLoad(ulong id)
        {
            UMI3DEntityInstance node = GetEntityInstance(id);
            if (node != null)
            {
                if (entitywaited.ContainsKey(id))
                {
                    entitywaited[id].ForEach(a => a.Item1?.Invoke(node));
                    entitywaited.Remove(id);
                }
                entityToBeLoaded.Remove(id);
            }
        }

        private void NotifyEntityFailedToLoad(ulong id)
        {
                if (entitywaited.ContainsKey(id))
                {
                    entitywaited[id].ForEach(a => a.Item2?.Invoke());
                    entitywaited.Remove(id);
                }
                entityToBeLoaded.Remove(id);
                entityFailedToBeLoaded.Add(id);
        }

        /// <summary>
        /// Return a list of all registered entities.
        /// </summary>
        /// <returns></returns>
        public List<UMI3DEntityInstance> Entities() { return entities.Values.ToList(); }

        /// <summary>
        /// Get an entity with an id.
        /// </summary>
        /// <param name="id">unique id of the entity.</param>
        /// <returns></returns>
        public virtual UMI3DEntityInstance GetEntityInstance(ulong id)
        {
            if (id == 0 || !entities.ContainsKey(id))
                throw new ArgumentException(message: $"Entity {id} does not exist.");
            else if (entities[id] != null)
                return entities[id];
            else
                throw new Umi3dException($"Entity {id} is referenced but is null.");
        }

        /// <summary>
        /// Get an entity with an id.
        /// </summary>
        /// <param name="id">unique id of the entity.</param>
        /// <returns></returns>
        public virtual UMI3DEntityInstance TryGetEntityInstance(ulong id)
        {
            if (id == 0)
                throw new ArgumentException(message: $"Entity {id} does not exist.");
            else if (!entities.ContainsKey(id))
                return null;
            else if (entities[id] != null)
                return entities[id];
            else
                throw new Umi3dException($"Entity {id} is referenced but is null.");
        }

        public virtual bool TryGetEntity<T>(ulong id, out T entity) where T : class
        {
            entity = null;
            if (id == 0)
                throw new ArgumentException(message: $"Entity {id} does not exist.");
            else if (!entities.ContainsKey(id))
                return false;
            else if (entities[id] != null)
            {
                entity = entities[id].Object as T;
                return true;
            }
            else
                throw new Umi3dException($"Entity {id} is referenced but is null.");
        }

        /// <summary>
        /// Get an entity with an id.
        /// </summary>
        /// <param name="id">unique id of the entity.</param>
        /// <returns></returns>
        public virtual T GetEntityObject<T>(ulong id) where T : class
        {
            var entity = GetEntityInstance(id);
            if (entity.Object is T objEntity)
                return objEntity;
            else
                throw new Umi3dException($"Entity {id} [{entity}:{entity?.GetType()}] is not of type {nameof(T)}.");
        }

        /// <summary>
        /// Get a node with an id.
        /// </summary>
        /// <param name="id">unique id of the entity.</param>
        /// <returns></returns>
        public UMI3DNodeInstance GetNode(ulong id) { return id != 0 && entities.ContainsKey(id) ? entities[id] as UMI3DNodeInstance : null; }

        /// <summary>
        /// Get a node with an id.
        /// </summary>
        /// <param name="id">unique id of the entity.</param>
        /// <returns>Node instance or null if node does not exist.</returns>
        public virtual UMI3DNodeInstance GetNodeInstance(ulong id)
        {
            if (GetEntityInstance(id) is not UMI3DNodeInstance node)
                throw new Umi3dException($"Entity {id} is not an UMI3DNodeInstance.");
            return node;
        }

        /// <summary>
        /// Get a node id with a collider.
        /// </summary>
        /// <param name="collider">collider.</param>
        /// <returns></returns>
        public ulong GetNodeID(Collider collider) { return entities.Where(k => k.Value is UMI3DNodeInstance).FirstOrDefault(k => (k.Value as UMI3DNodeInstance).colliders.Any(c => c == collider)).Key; }

        /// <summary>
        /// Get node id associated to <paramref name="t"/>.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public ulong GetNodeID(Transform t) { return entities.Where(k => k.Value is UMI3DNodeInstance).FirstOrDefault(k => (k.Value as UMI3DNodeInstance).transform == t).Key; }

        /// <summary>
        /// Register a node instance.
        /// </summary>
        /// <param name="id">unique id of the node.</param>
        /// <param name="dto">dto of the node.</param>
        /// <param name="instance">gameobject of the node.</param>
        /// <returns></returns>
        public UMI3DNodeInstance RegisterNodeInstance(ulong id, UMI3DDto dto, GameObject instance, Action delete = null)
        {
            UMI3DNodeInstance node = null;
            if (instance == null)
                return null;
            else if (entities.ContainsKey(id))
            {
                node = entities[id] as UMI3DNodeInstance;
                if (node == null)
                    throw new Exception($"id:{id} found but the value was of type {entities[id].GetType()}");
                if (node.gameObject != instance)
                    UnityEngine.Object.Destroy(instance);
                return node;
            }
            else
            {
                node = new UMI3DNodeInstance(EnvironmentId, () => NotifyEntityLoad(id), id) { gameObject = instance, dto = dto, Delete = delete };
                entities.Add(id, node);
            }

            return node;
        }

        /// <summary>
        /// Register an entity without a gameobject.
        /// </summary>
        /// <param name="id">unique id of the node.</param>
        /// <param name="dto">dto of the node.</param>
        /// <returns></returns>
        public virtual UMI3DEntityInstance RegisterEntity(ulong id, UMI3DDto dto, object objectInstance, Action delete = null)
        {
            UMI3DEntityInstance node = null;
            
            if (entities.ContainsKey(id))
                node = entities[id];

            else
            {
                node = new UMI3DEntityInstance(EnvironmentId, () => NotifyEntityLoad(id), id) { dto = dto, Object = objectInstance, Delete = delete };
                entities.Add(id, node);
            }

            return node;
        }

        /// <summary>
        /// Load IEntity.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="performed"></param>
        public async Task _LoadEntity(ByteContainer container, Func<IEntity, List<CancellationToken>, Task> loadEntityTask)
        {
            List<ulong> ids = UMI3DSerializer.ReadList<ulong>(container);
            ids.ForEach(id => NotifyEntityToBeLoaded(id));

            try
            {
                var load = await UMI3DClientServer.GetEntity(container.environmentId, ids);
                await Task.WhenAll(
                    load.entities.Select(async item =>
                    {
                        if (item is MissingEntityDto missing)
                        {
                            NotifyEntityFailedToLoad(missing.id);
                            UMI3DLogger.Log($"Get entity [{missing.id}] failed : {missing.reason}", scope);
                        }
                        else
                            await loadEntityTask(item, container.tokens);
                    }));
            }
            catch (Exception e)
            {
                UMI3DLogger.LogException(e, scope);
            }
        }

        /// <summary>
        /// Delete IEntity
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="performed"></param>
        public async Task DeleteEntity(ulong entityId, List<CancellationToken> tokens)
        {
            if (entities.ContainsKey(entityId))
            {
                entityFilters.Remove(entityId);

                UMI3DEntityInstance entity = entities[entityId];

                if (entity.Object is UMI3DAbstractAnimation animation)
                {
                    animation.Stop();
                }

                if (entity is UMI3DNodeInstance)
                {
                    var node = entity as UMI3DNodeInstance;
                    node.ClearBeforeDestroy();
                    UnityEngine.Object.Destroy(node.gameObject);
                }
                entities[entityId].Delete?.Invoke();
                entities.Remove(entityId);
            }
            else if (IsEntityToBeLoaded(entityId))
            {
                var e = await WaitUntilEntityLoaded(entityId, tokens);
                await DeleteEntity(entityId, tokens);
            }
            else if (IsEntityToFailedBeLoaded(entityId))
            {
                RemoveEntityToFailedBeLoaded(entityId);
            }
            else
            {
                UMI3DLogger.LogError($"Entity [{entityId}] To Destroy Not Found And Not in Entities to be loaded", scope);
            }
        }

        /// <summary>
        /// Clear an environement and make the client ready to load a new environment.
        /// </summary>
        public void Clear()
        {
            entityFilters.Clear();
            foreach (ulong entity in entities.ToList().Select(p => { return p.Key; }))
            {
                DeleteEntity(entity, null);
            }
        }

        public virtual void InternalClear()
        {
            UMI3DVideoPlayerLoader.Clear();

            entities.Clear();
            entitywaited.Clear();
            entityToBeLoaded.Clear();
            entityFailedToBeLoaded.Clear();
        }


        private readonly Dictionary<ulong, Dictionary<ulong, IExtrapolator>> entityFilters = new Dictionary<ulong, Dictionary<ulong, IExtrapolator>>();

        public UMI3DEntities(ulong environmentId, string url)
        {
            this.EnvironmentId = environmentId;
            this.ReourcesUrl = url;
        }

        public async void InterpolationRoutine(Action<SetUMI3DPropertyData> SimulatedSetEntity)
        {
                foreach (ulong entityId in entityFilters.Keys)
                {
                    foreach (ulong property in entityFilters[entityId].Keys)
                    {
                        if (entityId == 0 || !entities.ContainsKey(entityId))
                        {
                            Debug.LogWarning($"[Interpolation] Warning: Entity {entityId} does not exist.");
                            continue;
                        }

                        UMI3DEntityInstance node = GetEntityInstance(entityId);
                        IExtrapolator extrapolator = entityFilters[entityId][property];

                        extrapolator.ComputeExtrapolatedValue();

                        var entityPropertyDto = new SetEntityPropertyDto()
                        {
                            entityId = entityId,
                            property = property,
                            value = extrapolator.ExtrapolatedValue.ToSerializable()
                        };
                        //SimulatedSetEntity(new SetUMI3DPropertyData(entityPropertyDto, node));
                        SimulatedSetEntity(new SetUMI3DPropertyData(EnvironmentId, entityPropertyDto, node));
                    }
                }
        }

        /// <summary>
        /// Handle SetEntityPropertyDto operation.
        /// </summary>
        /// <param name="node">Node on which the dto should be applied.</param>
        /// <param name="dto">Set operation to handle.</param>
        /// <returns></returns>
        public bool SetEntity(SetUMI3DPropertyData data)
        {
            if (entityFilters.ContainsKey(data.property.entityId) && entityFilters[data.property.entityId].ContainsKey(data.property.property))
            {
                entityFilters[data.property.entityId][data.property.property].AddMeasure(data.property.value.Deserialize());
                return true;
            }
            return false;
        }

        /// <summary>
        /// Handle SetEntityPropertyDto operation.
        /// </summary>
        /// <param name="node">Node on which the dto should be applied.</param>
        /// <param name="dto">Set operation to handle.</param>
        /// <returns></returns>
        public async Task<bool> SetEntity(ulong entityId, SetUMI3DPropertyContainerData data, Func<ReadUMI3DPropertyData,Task<bool>> ReadValueEntity)
        {
            //UnityMainThreadDispatcher.Instance().Enqueue(() => UnityEngine.Debug.Log($"SetEntity {entityId}  {entityFilters.ContainsKey(entityId)} && {entityFilters.ContainsKey(entityId) && entityFilters[entityId].ContainsKey(data.propertyKey)}"));
            if (entityFilters.ContainsKey(entityId) && entityFilters[entityId].ContainsKey(data.propertyKey))
            {

                var value = new ReadUMI3DPropertyData(EnvironmentId, data.propertyKey, data.container);
                await ReadValueEntity(value);
                entityFilters[entityId][data.propertyKey].AddMeasure(value.result.Deserialize());
                return true;
            }
            return false;
        }

        public bool StartInterpolation(UMI3DEntityInstance node, ulong entityId, ulong propertyKey, object startValue, List<CancellationToken> tokens)
        {
            if (!entityFilters.ContainsKey(entityId))
            {
                entityFilters.Add(entityId, new Dictionary<ulong, IExtrapolator>());
            }

            if (!entityFilters[entityId].ContainsKey(propertyKey))
            {
                IExtrapolator newExtrapolator;
                if (propertyKey == UMI3DPropertyKeys.Rotation)
                    newExtrapolator = new QuaternionLinearDelayedExtrapolator();
                else
                    newExtrapolator = new Vector3LinearDelayedExtrapolator();

                entityFilters[entityId].Add(propertyKey, newExtrapolator);

                newExtrapolator.AddMeasure(startValue);

                var entityPropertyDto = new SetEntityPropertyDto()
                {
                    entityId = entityId,
                    property = propertyKey,
                    value = startValue.ToSerializable()
                };
                return SetEntity(new SetUMI3DPropertyData(EnvironmentId ,entityPropertyDto, node, tokens));
            }
            return false;
        }

        public bool StopInterpolation(UMI3DEntityInstance node, ulong entityId, uint property, object stopValue, List<CancellationToken> tokens)
        {
            if (entityFilters.ContainsKey(entityId) && entityFilters[entityId].ContainsKey(property))
            {
                entityFilters[entityId].Remove(property);
                var entityPropertyDto = new SetEntityPropertyDto()
                {
                    entityId = entityId,
                    property = property,
                    value = stopValue.ToSerializable()
                };

                return SetEntity(new SetUMI3DPropertyData(EnvironmentId, entityPropertyDto, node, tokens));
            }
            return false;
        }

    }
}