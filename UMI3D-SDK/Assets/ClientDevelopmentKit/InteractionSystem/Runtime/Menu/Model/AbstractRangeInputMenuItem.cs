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

namespace umi3d.cdk.menu
{
    public abstract class AbstractRangeInputMenuItem<T> : AbstractInputMenuItem<T> where T : System.IComparable
    {
        /// <summary>
        /// Current value.
        /// </summary>
        public T value;

        /// <summary>
        /// Minimum range value.
        /// </summary>
        public T min;

        /// <summary>
        /// Maximum range value.
        /// </summary>
        public T max;

        /// <summary>
        /// Increments in available values (0 means continuous).
        /// </summary>
        public virtual T increment
        {
            get { return inc; }
            set { inc = value; }
        }

        /// <summary>
        /// Is the range continous (false is discrete) ?
        /// </summary>
        /// <see cref="increment"/>
        public abstract bool continuousRange { get; }

        /// <summary>
        /// Private attribute for increment.
        /// </summary>
        /// <see cref="increment"/>
        protected T inc;
    }
}