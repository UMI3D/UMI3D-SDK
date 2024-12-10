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

namespace umi3d.cdk
{
    public class PropertyList<T> : PropertyList<T, T>
    {
        public PropertyList() : base(Converter)
        {
        }

        public PropertyList(IEnumerable<T> collection) : base(collection, Converter)
        {
        }

        public PropertyList(int capacity) : base(capacity, Converter)
        {
        }

        static T Converter(T value) => value;
    }

    public class PropertyList<S, T> : List<T>
    {
        public event Action<int,T> OnValueChanged;
        public event Action<int, T> OnValueAdded;
        public event Action<int> OnValueRemoved;
        public event Action<PropertyList<S, T>> OnCollectionUpdated;

        public PropertyList(Func<S, T> converter)
        {
            this.Converter = converter;
        }

        public PropertyList(IEnumerable<S> collection, Func<S, T> converter) : base(collection.Select(converter))
        {
            this.Converter = converter;
        }

        public PropertyList(int capacity, Func<S, T> converter) : base(capacity)
        {
            this.Converter = converter;
        }

        Func<S, T> Converter;

        public bool SetEntity(SetUMI3DPropertyContainerData data)
        {
            switch (data.operationId)
            {
                case UMI3DOperationKeys.SetEntityListAddProperty:
                    {
                        var index = UMI3DSerializer.Read<int>(data.container);
                        var value = Converter(UMI3DSerializer.Read<S>(data.container));
                        if (index == this.Count)
                            this.Add(value);
                        else if (index < this.Count && index >= 0)
                            this.Insert(index, value);
                        else
                            return false;

                        OnValueAdded?.Invoke(index, value);

                        OnCollectionUpdated?.Invoke(this);
                        return true;
                    }
                case UMI3DOperationKeys.SetEntityListRemoveProperty:
                    {
                        var index = UMI3DSerializer.Read<int>(data.container);
                        if (index < this.Count && index >= 0)
                            this.RemoveAt(index);
                        else
                            return false;

                        OnValueRemoved?.Invoke(index);

                        OnCollectionUpdated?.Invoke(this);
                        return true;
                    }
                case UMI3DOperationKeys.SetEntityListProperty:
                    {
                        var index = UMI3DSerializer.Read<int>(data.container);
                        var value = Converter(UMI3DSerializer.Read<S>(data.container));

                        if (index < this.Count && index >= 0)
                            this[index] = value;
                        else
                            return false;

                        OnValueChanged?.Invoke(index, value);

                        OnCollectionUpdated?.Invoke(this);
                        return true;
                    }
                default:
                    SetList(UMI3DSerializer.ReadList<S>(data.container));

                    OnCollectionUpdated?.Invoke(this);
                    return true;
            }
        }

        public bool SetEntity(SetUMI3DPropertyData data)
        {
            switch (data.property)
            {
                case SetEntityListAddPropertyDto add:
                    {
                        if (add.value != null && !typeof(S).IsAssignableFrom(add.value.GetType()))
                            return false;

                        var index = add.index;
                        var value = Converter((S)add.value);

                        if (index == this.Count)
                            this.Add(value);
                        else if (index < this.Count && index >= 0)
                            this.Insert(index, value);
                        else
                            return false;
                        return true;
                    }
                case SetEntityListRemovePropertyDto remove:
                    {
                        var index = remove.index;
                        if (index < this.Count && index >= 0)
                            this.RemoveAt(index);
                        else
                            return false;
                        return true;
                    }
                case SetEntityListPropertyDto change:
                    {
                        if (change.value != null && !typeof(S).IsAssignableFrom(change.value.GetType()))
                            return false;

                        var index = change.index;
                        var value = Converter((S)change.value);

                        if (index < this.Count && index >= 0)
                            this[index] = value;
                        else
                            return false;
                        return true;
                    }
                case SetEntityPropertyDto set:
                    SetList((List<S>)set.value);
                    break;
                default:
                    return false;
            }
            return true;
        }

        public void SetList(IEnumerable<S> values)
        {
            this.Clear();
            this.AddRange(values.Select(v => Converter(v)));
        }



    }
    public static class PropertyListSetter
    {
        public static bool SetEntity<S>(SetUMI3DPropertyContainerData data, List<S> list)
        {
            return SetEntity<S, S>(data, s => s, list);
        }

        public static bool SetEntity<S, T>(SetUMI3DPropertyContainerData data, Func<S, T> converter, List<T> list)
        {
            switch (data.operationId)
            {
                case UMI3DOperationKeys.SetEntityListAddProperty:
                    {
                        var index = UMI3DSerializer.Read<int>(data.container);
                        var value = converter(UMI3DSerializer.Read<S>(data.container));
                        if (index == list.Count)
                            list.Add(value);
                        else if (index < list.Count && index >= 0)
                            list.Insert(index, value);
                        else
                            return false;
                        return true;
                    }
                case UMI3DOperationKeys.SetEntityListRemoveProperty:
                    {
                        var index = UMI3DSerializer.Read<int>(data.container);
                        if (index < list.Count && index >= 0)
                            list.RemoveAt(index);
                        else
                            return false;
                        return true;
                    }
                case UMI3DOperationKeys.SetEntityListProperty:
                    {
                        var index = UMI3DSerializer.Read<int>(data.container);
                        var value = converter(UMI3DSerializer.Read<S>(data.container));

                        if (index < list.Count && index >= 0)
                            list[index] = value;
                        else
                            return false;
                        return true;
                    }
                default:
                    SetList(UMI3DSerializer.ReadList<S>(data.container), converter, list);
                    return true;
            }
        }


        public static bool SetEntity<S>(SetUMI3DPropertyData data, List<S> list)
        {
            return SetEntity<S, S>(data, s => s, list);
        }

        public static bool SetEntity<S, T>(SetUMI3DPropertyData data, Func<S, T> converter, List<T> list)
        {
            switch (data.property)
            {
                case SetEntityListAddPropertyDto add:
                    {
                        if (add.value != null && !typeof(S).IsAssignableFrom(add.value.GetType()))
                            return false;

                        var index = add.index;
                        var value = converter((S)add.value);

                        if (index == list.Count)
                            list.Add(value);
                        else if (index < list.Count && index >= 0)
                            list.Insert(index, value);
                        else
                            return false;
                        return true;
                    }
                case SetEntityListRemovePropertyDto remove:
                    {
                        var index = remove.index;
                        if (index < list.Count && index >= 0)
                            list.RemoveAt(index);
                        else
                            return false;
                        return true;
                    }
                case SetEntityListPropertyDto change:
                    {
                        if (change.value != null && !typeof(S).IsAssignableFrom(change.value.GetType()))
                            return false;

                        var index = change.index;
                        var value = converter((S)change.value);

                        if (index < list.Count && index >= 0)
                            list[index] = value;
                        else
                            return false;
                        return true;
                    }
                case SetEntityPropertyDto set:
                    SetList((List<S>)set.value, converter, list);
                    break;
                default:
                    return false;
            }
            return true;
        }

        private static void SetList<S, T>(IEnumerable<S> values, Func<S, T> converter, List<T> list)
        {
            list.Clear();
            list.AddRange(values.Select(v => converter(v)));
        }



    }
}