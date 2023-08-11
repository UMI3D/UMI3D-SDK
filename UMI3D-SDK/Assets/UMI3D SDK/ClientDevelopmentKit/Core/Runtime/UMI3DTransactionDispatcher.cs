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
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{

    public class UMI3DTransactionDispatcher
    {

        private const DebugScope scope = DebugScope.CDK | DebugScope.Collaboration | DebugScope.Networking;

        Func<DtoContainer, Task<bool>> OperationDto;
        Func<uint, ByteContainer, Task<bool>> Operation;

        /// <summary>
        /// Unpack the transaction and apply the operations.
        /// </summary>
        /// <param name="transaction">Transaction to unpack.</param>
        /// <returns></returns>
        ///  A transaction is composed of a set of operations to be performed on entities (e.g Scenes, Nodes, Materials).
        ///  Operations should be applied in the same order as stored in the transaction.
        public async Task PerformTransaction(TransactionDto transaction)
        {
            int _transaction = count++;
            int opCount = 0;
            foreach (var operation in transaction.operations.Select(o => new DtoContainer(o)))
            {
                bool performed = false;
                var ErrorTime = Time.time + secondBeforeError;
                int op = opCount++;
                int cfail = 0;

                CancellationTokenSource source = new CancellationTokenSource();
                operation.tokens.Add(source.Token);

                async void isOk()
                {
                    while (!performed)
                    {
                        if (Time.time > ErrorTime)
                        {
                            cfail++;
                            if (cfail >= 3)
                            {
                                UMI3DLogger.LogError($"Operation took more than {secondBeforeError} sec MARK AS failed !!!!!!.\n Transaction count {transaction}.\n Operation count {op}.\n Operation : {operation} ", scope);
                                source.Cancel();
                                return;
                            }
                            UMI3DLogger.LogError($"Operation took more than {secondBeforeError} sec it might have failed.\n Transaction count {transaction}.\n Operation count {op}.\n Operation : {operation} ", scope);
                        }
                        await UMI3DAsyncManager.Yield();
                    }
                }

                isOk();
                await PerformOperation(operation);
                performed = true;
            }
        }

        static int count = 0;
        const int secondBeforeError = 300;

        public UMI3DTransactionDispatcher(Func<DtoContainer, Task<bool>> operationDto, Func<uint, ByteContainer, Task<bool>> operation)
        {
            OperationDto = operationDto;
            Operation = operation;
        }

        /// <summary>
        /// Unpack the transaction from a <paramref name="container"/> and apply the operations.
        /// </summary>
        /// <param name="container">Transaction in a container.</param>
        /// <returns></returns>
        ///  A transaction is composed of a set of operations to be performed on entities (e.g Scenes, Nodes, Materials).
        ///  Operations should be applied in the same order as stored in the transaction.
        public async Task PerformTransaction(ByteContainer container)
        {
            int transaction = count++;
            int opCount = 0;
            foreach (ByteContainer c in UMI3DSerializer.ReadIndexesList(container))
            {
                bool performed = false;
                var ErrorTime = Time.time + secondBeforeError;
                int op = opCount++;
                int cfail = 0;

                CancellationTokenSource source = new CancellationTokenSource();
                container.tokens.Add(source.Token);

                async void isOk()
                {
                    while (!performed)
                    {
                        if (Time.time > ErrorTime)
                        {
                            cfail++;
                            if (cfail >= 3)
                            {
                                UMI3DLogger.LogError($"Operation took more than {secondBeforeError} sec MARK AS failed !!!!!!.\n Transaction count {transaction}.\n Operation count {op}.\n Container : {container}\n SubContainer : {c}", scope);
                                source.Cancel();
                                return;
                            }
                            UMI3DLogger.LogError($"Operation took more than {secondBeforeError} sec it might have failed.\n Transaction count {transaction}.\n Operation count {op}.\n Container : {container}\n SubContainer : {c}", scope);
                            await UMI3DAsyncManager.Delay(1000);
                        }
                        await UMI3DAsyncManager.Yield();
                    }
                }

                isOk();
                await PerformOperation(c);
                performed = true;
            }
        }

        /// <summary>
        /// Apply an <paramref name="operation"/>.
        /// </summary>
        /// <param name="operation">Operation to apply.</param>
        /// <param name="performed">Callback.</param>
        public async Task PerformOperation(DtoContainer operation)
        {
            switch (operation.operation)
            {
                case LoadEntityDto load:
                    await Task.WhenAll(load.entities.Select(async entity =>
                    {
                        await UMI3DEnvironmentLoader.LoadEntity(entity, operation.tokens);
                    }));
                    break;
                case DeleteEntityDto delete:
                    await UMI3DEnvironmentLoader.DeleteEntity(delete.entityId, operation.tokens);
                    break;
                case SetEntityPropertyDto set:
                    await UMI3DEnvironmentLoader.SetEntity(set, operation.tokens);
                    break;
                case MultiSetEntityPropertyDto multiSet:
                    await UMI3DEnvironmentLoader.SetMultiEntity(multiSet, operation.tokens);
                    break;
                case StartInterpolationPropertyDto interpolationStart:
                    await UMI3DEnvironmentLoader.StartInterpolation(interpolationStart, operation.tokens);
                    break;
                case StopInterpolationPropertyDto interpolationStop:
                    await UMI3DEnvironmentLoader.StopInterpolation(interpolationStop, operation.tokens);
                    break;
                default:
                    if (!await OperationDto(operation))
                        await UMI3DEnvironmentLoader.AbstractParameters.UnknownOperationHandler(operation);
                    break;
            }
        }

        /// <summary>
        /// Apply an operation in a <paramref name="container"/>.
        /// </summary>
        /// <param name="container">Operation to apply as a container.</param>
        /// <param name="performed">Callback.</param>
        public async Task PerformOperation(ByteContainer container)
        {
            uint operationId = UMI3DSerializer.Read<uint>(container);

            switch (operationId)
            {
                case UMI3DOperationKeys.LoadEntity:
                    await UMI3DEnvironmentLoader.LoadEntity(container);
                    break;
                case UMI3DOperationKeys.DeleteEntity:
                    {
                        ulong entityId = UMI3DSerializer.Read<ulong>(container);
                        await UMI3DEnvironmentLoader.DeleteEntity(entityId, container.tokens);
                        break;
                    }
                case UMI3DOperationKeys.MultiSetEntityProperty:
                    await UMI3DEnvironmentLoader.SetMultiEntity(container);
                    break;
                case UMI3DOperationKeys.StartInterpolationProperty:
                    await UMI3DEnvironmentLoader.StartInterpolation(container);
                    break;
                case UMI3DOperationKeys.StopInterpolationProperty:
                    await UMI3DEnvironmentLoader.StopInterpolation(container);
                    break;

                default:
                    if (UMI3DOperationKeys.SetEntityProperty <= operationId && operationId <= UMI3DOperationKeys.SetEntityMatrixProperty)
                    {
                        ulong entityId = UMI3DSerializer.Read<ulong>(container);
                        uint propertyKey = UMI3DSerializer.Read<uint>(container);
                        await UMI3DEnvironmentLoader.SetEntity(operationId, entityId, propertyKey, container);
                    }
                    else if (!await Operation(operationId, container))
                        await UMI3DEnvironmentLoader.AbstractParameters.UnknownOperationHandler(operationId, container);
                    break;
            }
        }
    }
}