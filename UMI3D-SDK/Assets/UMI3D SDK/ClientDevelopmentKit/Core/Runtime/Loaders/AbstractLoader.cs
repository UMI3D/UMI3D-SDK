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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    public class ReadUMI3DExtensionData
    {
        public ulong environmentId { get; protected set; }
        public UMI3DDto dto;
        public GameObject node;
        public List<CancellationToken> tokens;

        public ReadUMI3DExtensionData(ulong environmentId, UMI3DDto dto) : this(environmentId, dto, null, new())
        {
        }

        public ReadUMI3DExtensionData(ulong environmentId, UMI3DDto dto, List<CancellationToken> tokens) : this(environmentId, dto, null, tokens)
        {
        }

        public ReadUMI3DExtensionData(ulong environmentId, UMI3DDto dto, GameObject node) : this(environmentId, dto, node, new())
        { }

        public ReadUMI3DExtensionData(ulong environmentId, UMI3DDto dto, GameObject node, List<CancellationToken> tokens)
        {
            this.dto = dto;
            this.node = node;
            this.tokens = tokens;
            this.environmentId = environmentId;
        }

        public override string ToString()
        {
            return $"ReadUMI3DData [{dto} : {node?.name}]";
        }
    }

    public class SetUMI3DPropertyData
    {
        public ulong environmentId { get; protected set; }
        public UMI3DEntityInstance entity;
        public SetEntityPropertyDto property;
        public List<CancellationToken> tokens;

        public SetUMI3DPropertyData(ulong environmentId, SetEntityPropertyDto property, UMI3DEntityInstance entity) : this(environmentId, property,entity,new())
        { }

        public SetUMI3DPropertyData(ulong environmentId, SetEntityPropertyDto property, UMI3DEntityInstance entity, List<CancellationToken> tokens)
        {
            this.environmentId = environmentId;
            this.entity = entity;
            this.property = property;
            this.tokens = tokens;
        }


        public override string ToString()
        {
            return $"SetUMI3DPropertyData [{property} : {entity}]";
        }
    }

    public class SetUMI3DPropertyContainerData
    {
        public ulong environmentId { get; protected set; }
        public UMI3DEntityInstance entity;
        public uint operationId;
        public uint propertyKey;
        public ByteContainer container;
        public List<CancellationToken> tokens => container?.tokens;

        public SetUMI3DPropertyContainerData(ulong environmentId,UMI3DEntityInstance entity, uint operationId, uint propertyKey, ByteContainer container)
        {
            this.environmentId = environmentId;
            this.entity = entity;
            this.operationId = operationId;
            this.propertyKey = propertyKey;
            this.container = container;
        }

        public override string ToString()
        {
            return $"SetUMI3DPropertyData [{operationId} : {propertyKey} : {entity} : {container}]";
        }
    }

    public class ReadUMI3DPropertyData
    {
        public ulong environmentId { get; protected set; }
        public uint propertyKey;
        public ByteContainer container;
        public object result;

        public ReadUMI3DPropertyData(ulong environmentId, uint propertyKey, ByteContainer container)
        {
            this.environmentId = environmentId;
            this.propertyKey = propertyKey;
            this.container = container;
            result = null;
        }

        public override string ToString()
        {
            return $"ReadUMI3DPropertyData [{propertyKey} : {container} : {result}]";
        }
    }

    public abstract class AbstractLoader : IHandler<ReadUMI3DExtensionData, bool>, IHandler<SetUMI3DPropertyData, bool>, IHandler<SetUMI3DPropertyContainerData, bool>, IHandler<ReadUMI3DPropertyData, bool>
    {

        public abstract UMI3DVersion.VersionCompatibility version { get; }

        AbstractLoader successor;

        public AbstractLoader GetNext()
        {
            return successor;
        }

        public AbstractLoader SetNext(AbstractLoader successor)
        {
            this.successor = successor;
            return this.successor;
        }


        IHandler<ReadUMI3DExtensionData, bool> IHandler<ReadUMI3DExtensionData, bool>.GetNext()
        {
            return successor;
        }

        IHandler<SetUMI3DPropertyData, bool> IHandler<SetUMI3DPropertyData, bool>.GetNext()
        {
            return this.successor;
        }

        IHandler<SetUMI3DPropertyContainerData, bool> IHandler<SetUMI3DPropertyContainerData, bool>.GetNext()
        {
            return this.successor;
        }

        IHandler<ReadUMI3DPropertyData, bool> IHandler<ReadUMI3DPropertyData, bool>.GetNext()
        {
            return this.successor;
        }

        IHandler<ReadUMI3DExtensionData, bool> IHandler<ReadUMI3DExtensionData, bool>.SetNext(IHandler<ReadUMI3DExtensionData, bool> successor)
        {
            if (successor is AbstractLoader loader)
                this.successor = loader;
            return this.successor;
        }

        IHandler<SetUMI3DPropertyData, bool> IHandler<SetUMI3DPropertyData, bool>.SetNext(IHandler<SetUMI3DPropertyData, bool> value)
        {
            if (successor is AbstractLoader loader)
                this.successor = loader;
            return this.successor;
        }

        IHandler<SetUMI3DPropertyContainerData, bool> IHandler<SetUMI3DPropertyContainerData, bool>.SetNext(IHandler<SetUMI3DPropertyContainerData, bool> value)
        {
            if (successor is AbstractLoader loader)
                this.successor = loader;
            return this.successor;
        }

        IHandler<ReadUMI3DPropertyData, bool> IHandler<ReadUMI3DPropertyData, bool>.SetNext(IHandler<ReadUMI3DPropertyData, bool> value)
        {
            if (successor is AbstractLoader loader)
                this.successor = loader;
            return this.successor;
        }

        public async Task<bool> Handle(ReadUMI3DExtensionData value)
        {
            if (version.IsCompatible(UMI3DClientServer.Instance.version) && CanReadUMI3DExtension(value))
            {
                await ReadUMI3DExtension(value);
                return true;
            }
            if (successor != null)
                return await successor.Handle(value);
            throw new Umi3dException($"No loader for this data {value}");
        }

        public async Task<bool> Handle(SetUMI3DPropertyData value)
        {
            if (version.IsCompatible(UMI3DClientServer.Instance.version) && await SetUMI3DProperty(value))
                return true;
            if (successor != null)
                return await successor.Handle(value);
            throw new Umi3dException($"No loader for this data {value}");
        }

        public async Task<bool> Handle(SetUMI3DPropertyContainerData value)
        {
            if (version.IsCompatible(UMI3DClientServer.Instance.version) && await SetUMI3DProperty(value))
                return true;
            if (successor != null)
                return await successor.Handle(value);
            throw new Umi3dException($"No loader for this data {value}");
        }

        public async Task<bool> Handle(ReadUMI3DPropertyData value)
        {
            if (version.IsCompatible(UMI3DClientServer.Instance.version) && await ReadUMI3DProperty(value))
                return true;
            if (successor != null)
                return await successor.Handle(value);
            throw new Umi3dException($"No loader for this data {value}");
        }

        /// <summary>
        /// Can this loader load this piece of <paramref name="data"/> ?
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public abstract bool CanReadUMI3DExtension(ReadUMI3DExtensionData data);

        /// <summary>
        /// Load data from a received GLTF extension.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public abstract Task ReadUMI3DExtension(ReadUMI3DExtensionData value);

        /// <summary>
        /// Load data into a UMI3D property.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual Task<bool> ReadUMI3DProperty(ReadUMI3DPropertyData value)
        {
            return Task.FromResult(false);
        }

        /// <summary>
        /// Set a property value received from a <see cref="UMI3DDto"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public abstract Task<bool> SetUMI3DProperty(SetUMI3DPropertyData value);

        /// <summary>
        /// Set a property value received from a <see cref="ByteContainer"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public abstract Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData value);
    }

    public abstract class AbstractLoader<T> : AbstractLoader where T : UMI3DDto
    {
        public abstract Task Load(ulong environmentId, T dto);

        public abstract void Delete(ulong id);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            return data.dto is T;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override Task ReadUMI3DExtension(ReadUMI3DExtensionData value)
        {
            switch (value.dto)
            {
                case T dto:
                    Load(value.environmentId,dto);
                    break;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>>
        public override Task<bool> SetUMI3DProperty(SetUMI3DPropertyData value)
        {
            return Task.FromResult(false);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData value)
        {
            return Task.FromResult(false);
        }
    }


    public abstract class AbstractLoader<DtoType, LoadedType> : AbstractLoader where DtoType : UMI3DDto
    {
        public abstract Task<LoadedType> Load(ulong environmnetId, DtoType dto);

        public abstract void Delete(ulong environmentId, ulong id);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            return data.dto is DtoType;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override Task ReadUMI3DExtension(ReadUMI3DExtensionData value)
        {
            switch (value.dto)
            {
                case DtoType dto:
                    Load(value.environmentId, dto);
                    break;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>>
        public override Task<bool> SetUMI3DProperty(SetUMI3DPropertyData value)
        {
            return Task.FromResult(false);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData value)
        {
            return Task.FromResult(false);
        }
    }
}