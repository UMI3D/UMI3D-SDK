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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    public class ReadUMI3DExtensionData
    {
        public UMI3DDto dto;
        public GameObject node;
        public List<CancellationToken> tokens;

        public ReadUMI3DExtensionData(UMI3DDto dto) : this(dto, null, new())
        {
        }

        public ReadUMI3DExtensionData(UMI3DDto dto, List<CancellationToken> tokens) : this(dto, null, tokens)
        {
        }

        public ReadUMI3DExtensionData(UMI3DDto dto, GameObject node) : this(dto, node, new())
        { }

        public ReadUMI3DExtensionData(UMI3DDto dto, GameObject node, List<CancellationToken> tokens)
        {
            this.dto = dto;
            this.node = node;
            this.tokens = tokens;
        }

        public override string ToString()
        {
            return $"ReadUMI3DData [{dto} : {node?.name}]";
        }
    }

    public class SetUMI3DPropertyData
    {
        public UMI3DEntityInstance entity;
        public SetEntityPropertyDto property;
        public List<CancellationToken> tokens;

        public SetUMI3DPropertyData(SetEntityPropertyDto property, UMI3DEntityInstance entity) : this(property,entity,new())
        { }

        public SetUMI3DPropertyData(SetEntityPropertyDto property, UMI3DEntityInstance entity, List<CancellationToken> tokens)
        {
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
        public UMI3DEntityInstance entity;
        public uint operationId;
        public uint propertyKey;
        public ByteContainer container;
        public List<CancellationToken> tokens => container?.tokens;

        public SetUMI3DPropertyContainerData(UMI3DEntityInstance entity, uint operationId, uint propertyKey, ByteContainer container)
        {
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
        public uint propertyKey;
        public ByteContainer container;
        public object result;

        public ReadUMI3DPropertyData(uint propertyKey, ByteContainer container)
        {
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
}