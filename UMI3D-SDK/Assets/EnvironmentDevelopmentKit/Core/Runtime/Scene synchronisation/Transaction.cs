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
using umi3d.common;

namespace umi3d.edk
{
    public class Transaction
    {
        public bool reliable;
        public List<Operation> Operations = new List<Operation>();

        public (byte[], bool) ToBson(UMI3DUser user) {
            var transactionDto = new TransactionDto();
            transactionDto.operations = new List<AbstractOperationDto>(Operations.Where((op) => { return op.users.Contains(user); }).Select((op) => { return op.ToOperationDto(user); }));
            if (transactionDto.operations.Count > 0)
            {
                return (transactionDto.ToBson(), true);
            }
            return (null, false);
        }

        public (byte[],bool) ToBytes(UMI3DUser user)
        {
            Func<byte[], int, int, (int, int, int)> f3 = (byte[] by, int i, int j) =>
            {
                return (0, i, j);
            };
            var operation = Operations.Where((op) => { return op.users.Contains(user); });
            if (operation.Count() > 0)
            {
                int indexPos = UMI3DNetworkingHelper.GetSize(UMI3DOperationKeys.Transaction);
                int size = indexPos + operation.Count() * sizeof(int);
                var func = operation
                    .Select(o => o.ToBytes(user))
                    .Select(c =>
                    {
                        Func<byte[], int, int, (int, int, int)> f1 = (byte[] by, int i, int j) => (c.Item2(by, i), i, j);
                        return (c.Item1, f1);
                    })
                    .Aggregate((0, f3)
                    , (a, b) =>
                    {
                        Func<byte[], int, int, (int, int, int)> f2 = (byte[] by, int i, int j) =>
                        {
                            int s;
                            (s, i, j) = a.Item2(by, i, j);
                            (s, i, j) = b.Item2(by, i, j);
                            j += UMI3DNetworkingHelper.Write(i, by, j);
                            i += s;
                            return (s, i, j);
                        };
                        return (a.Item1 + b.Item1, f2);
                    });
                var data = new byte[size + func.Item1];
                UMI3DNetworkingHelper.Write(UMI3DOperationKeys.Transaction, data, 0);
                var couple = func.Item2(data, size, indexPos);
                return (data,true);
            }
            return (null,false);
        }

        public static Transaction operator +(Transaction a, Transaction b)
        {
            a.Operations.AddRange(b.Operations);
            a.reliable |= b.reliable;
            return a;
        }

        public static Transaction operator +(Transaction a, List<Operation> b)
        {
            a.Operations.AddRange(b);
            return a;
        }

        public static Transaction operator +(Transaction a, Operation b)
        {
            a.Operations.Add(b);
            return a;
        }

        public Operation this[int key]
        {
            get => Operations[key];
            set => Operations[key] = value;
        }

        public List<Operation> this[UMI3DUser key]
        {
            get => Operations.Where(o => o.users.Contains(key)).ToList();
        }

        public void Simplify()
        {
            List<Operation> newOperations = new List<Operation>();
            foreach (Operation op in Operations)
            {
                switch (op)
                {
                    case StartInterpolationProperty starti:
                    case StopInterpolationProperty stopi:
                    case SetEntityDictionaryAddProperty a:
                    case SetEntityDictionaryRemoveProperty r:
                    case SetEntityListAddProperty al:
                    case SetEntityListRemoveProperty rl:
                        newOperations.Add(op);
                        break;

                    case SetEntityListProperty sl:
                        var inverted = newOperations.ToList();
                        inverted.Reverse();
                        foreach (var nop in inverted)
                        {
                            if (nop is SetEntityListAddProperty || nop is SetEntityListRemoveProperty)
                                break;
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
                        foreach (var nop in inverted2)
                        {
                            if (nop is SetEntityDictionaryAddProperty || nop is SetEntityDictionaryRemoveProperty)
                                break;
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
                        foreach (var nop in newOperations.ToList())
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
                        foreach (var nop in newOperations.ToList())
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
                                        if (nl.entity.Id() == d.entityId)
                                        {
                                            nl -= d.users;
                                            if (nl.users.Count == 0)
                                                newOperations.Remove(nl);
                                        }

                                        break;
                                    }
                            }
                        }
                        newOperations.Add(d);
                        break;

                    case LoadEntity l:
                        foreach (var nop in newOperations.ToList())
                        {
                            switch (nop)
                            {
                                case DeleteEntity nd:
                                    {
                                        if (nd.entityId == l.entity.Id())
                                        {
                                            nd -= l.users;
                                            if (nd.users.Count == 0)
                                                newOperations.Remove(nd);
                                        }

                                        break;
                                    }

                                case SetEntityProperty ne:
                                    {
                                        if (ne.entityId == l.entity.Id())
                                        {
                                            ne -= l.users;
                                            if (ne.users.Count == 0)
                                                newOperations.Remove(ne);
                                        }

                                        break;
                                    }

                                case LoadEntity nl:
                                    {
                                        if (nl.entity.Id() == l.entity.Id())
                                        {
                                            nl -= l.users;
                                            if (nl.users.Count == 0)
                                                newOperations.Remove(nl);
                                        }

                                        break;
                                    }
                            }
                        }
                        newOperations.Add(l);
                        break;

                    default:
                        throw new System.Exception($"Missing type {op.GetType()}");
                }
            }
            Operations = newOperations;
        }
    }
}