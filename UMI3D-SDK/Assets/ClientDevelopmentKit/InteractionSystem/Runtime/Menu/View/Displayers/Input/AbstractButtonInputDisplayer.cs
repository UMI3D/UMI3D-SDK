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

namespace umi3d.cdk.menu.view
{
    public abstract class AbstractButtonInputDisplayer : AbstractInputMenuItemDisplayer<bool>
    {
        /// <summary>
        /// Button menu item to display.
        /// </summary>
        protected ButtonInputMenuItem menuItem;

        /// <summary>
        /// Notify that the button has been pressed.
        /// </summary>
        public void NotifyPress()
        {
            menuItem.NotifyValueChange(!menuItem.GetValue());
        }

        /// <summary>
        /// Set menu item to display and initialise the display.
        /// </summary>
        /// <param name="item">Item to display</param>
        /// <returns></returns>
        public override void SetMenuItem(AbstractMenuItem item)
        {
            if (item is ButtonInputMenuItem)
            {
                menuItem = item as ButtonInputMenuItem;
            }
            else
                throw new System.Exception("MenuItem must be a ButtonInput");
        }

        /// <summary>
        /// State is a the displayer is suitable for an AbstractMenuItem
        /// </summary>
        /// <param name="menu">The Menu to evaluate</param>
        /// <returns>Return a int. 0 and negative is not suitable, and int.MaxValue is the most suitable. </returns>
        public override int IsSuitableFor(umi3d.cdk.menu.AbstractMenuItem menu)
        {
            return (menu is ButtonInputMenuItem) ? 2 : 0;
        }
    }
}