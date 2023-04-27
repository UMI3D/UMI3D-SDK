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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using umi3d.common;

namespace umi3d.edk
{
    /// <summary>
    /// Collection of operations with helping 
    /// </summary>
    public class Transaction : IEnumerable<Operation>
    {


        public Transaction() { }

        public Transaction(bool reliable) : this()
        {
            this.reliable = reliable;
        }

        public Transaction(bool reliable, List<Operation> operations) : this(reliable)
        {
            Operations = operations;
        }

        public Transaction(List<Operation> operations) : this(false,operations) { }



        /// <summary>
        /// Reliable transactions are transactions for which receiving is ensuring.
        /// </summary>
        /// Note that transactions that are not reliable are lighter
        public bool reliable;
        /// <summary>
        /// List of operations to execute on the client.
        /// </summary>
        private List<Operation> Operations = new List<Operation>();

        /// <summary>
        /// Convert the transaction to a BSON as a byte array.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>A couple with the BSON in the first item, and a bool that is true when the conversion was sucessful.</returns>
        public (byte[], bool) ToBson(UMI3DUser user)
        {
            var transactionDto = new TransactionDto
            {
                operations = new List<AbstractOperationDto>(Operations.Where((op) => { return op.users.Contains(user); }).Select((op) => { return op.ToOperationDto(user); }))
            };
            if (transactionDto.operations.Count > 0)
            {
                return (transactionDto.ToBson(), true);
            }
            return (null, false);
        }

        /// <summary>
        /// Convert the transaction a byte array.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>A couple with the BSON in the first item, and a bool that is true when the conversion was sucessful.</returns>
        public (byte[], bool) ToBytes(UMI3DUser user)
        {
            IEnumerable<Operation> operation = Operations.Where((op) => { return op.users.Contains(user); });
            if (operation.Count() > 0)
            {
                Bytable b = UMI3DSerializer.Write(UMI3DOperationKeys.Transaction)
                    + UMI3DSerializer.WriteIBytableCollection(operation, user);
                return (b.ToBytes(), true);
            }
            return (null, false);
        }

        public static Transaction operator +(Transaction a, Transaction b)
        {
            a.AddIfNotNull(b.Operations);
            a.reliable |= b.reliable;
            return a;
        }

        public static Transaction operator +(Transaction a, List<Operation> b)
        {
            a.AddIfNotNull(b);
            return a;
        }

        public static Transaction operator +(Transaction a, Operation b)
        {
            a.AddIfNotNull(b);
            return a;
        }

        public Operation this[int key]
        {
            get => Operations[key];
            set => Operations[key] = value;
        }

        public List<Operation> this[UMI3DUser key] => Operations.Where(o => o.users.Contains(key)).ToList();

        /// <summary>
        /// Optimize the transaction when several operations are contained.
        /// </summary>
        /// <exception cref="System.Exception">Exception is thrown when an operation in the transaction is not recognized.</exception>
        public void Simplify()
        {
            Operation lastOperation = null;

            var newOperations = new List<Operation>();
            foreach (Operation op in Operations)
            {
                //concatenate entities in LoadEntitie operations
                if (lastOperation != null && lastOperation is LoadEntity && op is LoadEntity && lastOperation.users == op.users)
                {
                    (lastOperation as LoadEntity).entities.AddRange((op as LoadEntity).entities);
                }
                else
                {
                    switch (op)
                    {
                        case StartInterpolationProperty starti:
                        case StopInterpolationProperty stopi:
                        case AbstractBinding:
                        case RemoveBinding:
                        case UpdateBindingsActivation:
                        case SetEntityDictionaryAddProperty a:
                        case SetEntityDictionaryRemoveProperty r:
                        case SetEntityListAddProperty al:
                        case SetEntityListRemoveProperty rl:
                        case MultiSetEntityProperty msep:
                            newOperations.Add(op);
                            break;

                        case SetEntityListProperty sl:
                            var inverted = newOperations.ToList();
                            inverted.Reverse();
                            foreach (Operation nop in inverted)
                            {
                                if (nop is SetEntityListAddProperty || nop is SetEntityListRemoveProperty)
                                {
                                    break;
                                }
                                else if (nop is SetEntityListProperty ne)
                                {
                                    if (ne.entityId == sl.entityId && ne.property == sl.property && ne.index == sl.index)
                                    {
                                        ne -= sl.users;
                                        if (ne.users.Count == 0)
                                            newOperations.Remove(ne);
                                    }
                                }
                            }
                            newOperations.Add(sl);
                            break;

                        case SetEntityDictionaryProperty sd:
                            var inverted2 = newOperations.ToList();
                            inverted2.Reverse();
                            foreach (Operation nop in inverted2)
                            {
                                if (nop is SetEntityDictionaryAddProperty || nop is SetEntityDictionaryRemoveProperty)
                                {
                                    break;
                                }
                                else if (nop is SetEntityDictionaryProperty)
                                {
                                    var ne = nop as SetEntityDictionaryProperty;
                                    if (ne.entityId == sd.entityId && ne.property == sd.property && ne.key == sd.key)
                                    {
                                        ne -= sd.users;
                                        if (ne.users.Count == 0)
                                            newOperations.Remove(ne);
                                    }
                                }
                            }
                            newOperations.Add(sd);
                            break;

                        case SetEntityProperty e:
                            foreach (Operation nop in newOperations.ToList())
                            {
                                if (nop is SetEntityProperty)
                                {
                                    var ne = nop as SetEntityProperty;
                                    if (ne.entityId == e.entityId && ne.property == e.property)
                                    {
                                        ne -= e.users;
                                        if (ne.users.Count == 0)
                                            newOperations.Remove(ne);
                                    }
                                }
                            }
                            newOperations.Add(e);
                            break;

                        case DeleteEntity d:
                            foreach (Operation nop in newOperations.ToList())
                            {
                                switch (nop)
                                {
                                    case DeleteEntity nd:
                                        {
                                            if (nd.entityId == d.entityId)
                                            {
                                                nd -= d.users;
                                                if (nd.users.Count == 0)
                                                    newOperations.Remove(nd);
                                            }

                                            break;
                                        }

                                    case SetEntityProperty ne:
                                        {
                                            if (ne.entityId == d.entityId)
                                            {
                                                ne -= d.users;
                                                if (ne.users.Count == 0)
                                                    newOperations.Remove(ne);
                                            }

                                            break;
                                        }

                                    case LoadEntity nl:
                                        {
                                            if (nl.users.SequenceEqual(lastOperation.users))
                                            {
                                                var entityToDelete = new List<UMI3DLoadableEntity>();
                                                foreach (UMI3DLoadableEntity entity in nl.entities)
                                                {
                                                    if (entity.Id() == d.entityId)
                                                    {
                                                        entityToDelete.Add(entity);
                                                    }
                                                }
                                                foreach (UMI3DLoadableEntity item in entityToDelete)
                                                {
                                                    nl.entities.Remove(item);
                                                }
                                            }

                                            break;
                                        }
                                }
                            }
                            newOperations.Add(d);
                            break;

                        case LoadEntity l:
                            foreach (Operation nop in newOperations.ToList())
                            {
                                switch (nop)
                                {
                                    case DeleteEntity nd:
                                        {
                                            foreach (UMI3DLoadableEntity entity in l.entities)
                                            {
                                                if (nd.entityId == entity.Id())
                                                {
                                                    nd -= l.users;
                                                    if (nd.users.Count == 0)
                                                        newOperations.Remove(nd);
                                                }
                                            }
                                            break;
                                        }

                                    case SetEntityProperty ne:
                                        {
                                            foreach (UMI3DLoadableEntity entity in l.entities)
                                            {
                                                if (ne.entityId == entity.Id())
                                                {
                                                    ne -= l.users;
                                                    if (ne.users.Count == 0)
                                                        newOperations.Remove(ne);
                                                }
                                            }

                                            break;
                                        }

                                    case LoadEntity nl:
                                        {
                                            var entityToRemove = new List<UMI3DLoadableEntity>();
                                            if (nl.users.SequenceEqual(l.users))
                                            {
                                                foreach (UMI3DLoadableEntity NOEntity in nl.entities)
                                                {
                                                    foreach (UMI3DLoadableEntity newEntity in l.entities)
                                                    {
                                                        if (NOEntity.Id() == newEntity.Id())
                                                        {
                                                            entityToRemove.Add(newEntity);
                                                        }
                                                    }
                                                }
                                            }
                                            foreach (UMI3DLoadableEntity item in entityToRemove)
                                            {
                                                l.entities.Remove(item);
                                            }
                                            if (nl.entities.SequenceEqual(l.entities))
                                            {
                                                l -= nl.users;
                                            }

                                            break;
                                        }
                                }
                            }
                            newOperations.Add(l);
                            break;

                        default:
                            newOperations.Add(op);
                            break;
                    }
                    lastOperation = op;
                }
            }
            Operations = newOperations;
        }

        public IEnumerator<Operation> GetEnumerator()
        {
            return ((IEnumerable<Operation>)Operations).GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Operation>)Operations).GetEnumerator();
        }

        /// <summary>
        /// Add an operation to the collection if the operation is not null.
        /// </summary>
        /// <param name="b"></param>
        /// <returns>True if the operation was successfully added.</returns>
        public bool AddIfNotNull(Operation b)
        {
            if (b != null)
            {
                Add(b);
                return true;
            }

            return false;
        }

        public bool AddIfNotNull(IEnumerable<Operation> b)
        {
            if (b != null)
            {
                foreach (Operation c in b)
                {
                    if (c != null)
                    {
                        Add(c);
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Add a list of operation to the transaction and apply merge between LoadEntity if next to each other with the same user list.
        /// </summary>
        /// <param name="ops"></param>
        public void Add(IEnumerable<Operation> ops)
        {
            foreach (Operation op in ops)
            {
                Add(op);
            }
        }

        private Operation lastOperation;

        /// <summary>
        /// Add an operation to the transaction and apply merge between LoadEntity if next to each other with the same user list.
        /// </summary>
        /// <param name="op"></param>
        public void Add(Operation op)
        {
            if (op is LoadEntity)
            {
                lastOperation = Operations.LastOrDefault();
                if (lastOperation != null && lastOperation is LoadEntity && lastOperation.users != null && lastOperation.users.SequenceEqual(op.users))
                {
                    (lastOperation as LoadEntity).entities.AddRange((op as LoadEntity).entities);
                    return;
                }
            }
            Operations.Add(op);

        }

        /// <summary>
        /// Return the number of operations in this transaction.
        /// </summary>
        /// <returns></returns>
        public int Count()
        {
            return Operations.Count();
        }

        public void Dispatch()
        {
            if (Count() > 0) UMI3DServer.Dispatch(this);
            else UMI3DLogger.LogWarning("Transaction does not have any operation and will be not be send",DebugScope.EDK);
        }
    }
}