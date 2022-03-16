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
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.cdk.menu.view
{
    public partial class MenuDisplayManager
    {
        public MenuAsset menuAsset;
        /// <summary>
        /// Should call the Hide function when the Back function is called on root.
        /// </summary>
        public bool hideOnBack = false;
        /// <summary>
        /// All containers <= displayDepth will be Display, All displayers < ...(wip)
        /// </summary>
        public int displayDepth = 1;

        /// <summary>
        /// If true, this script will assume that the first container of containerPrefabs is already in the scene; otherwise it will be instantiated as a prefab.
        /// </summary>
        [Header("Containers")]
        public bool firstContainerInScene = true;
        /// <summary>
        /// Container selector.
        /// </summary>
        public ContainerSelector containerSelector;
        /// <summary>
        /// Displayer for menu items.
        /// </summary>
        [Header("Menu Items Displayers")]
        public List<AbstractDisplayer> DisplayerPrefab;

        public UnityEvent onDisplay = new UnityEvent();
        public UnityEvent onHide = new UnityEvent();
        public UnityEvent onDestroy = new UnityEvent();
        public UnityEvent firstButtonBackButtonPressed = new UnityEvent();

        /// <summary>
        /// Menu to display.
        /// </summary>
        public AbstractMenu menu
        {
            get
            {
                if (menuAsset.menu == null)
                    menuAsset.menu = new Menu();
                return menuAsset.menu;
            }
        }
        /// <summary>
        /// Is the menu being displayed ?
        /// </summary>
        public bool isDisplaying { get; protected set; }
        //public AbstractMenuItem LastMenuUnderNavigation
        //{
        //    get
        //    {
        //        if (lastMenuContainerUnderNavigation.menu is MenuItem lastMenuItem)
        //        if (menuToDisplayer.ContainsKey(lastMenuContainerUnderNavigation.menu))
        //        {

        //        }
        //    }
        //}
        public AbstractMenuDisplayContainer lastMenuContainerUnderNavigation { get; protected set; }


        /// <summary>
        /// Instantiated container (if any)
        /// </summary>
        private List<AbstractMenuDisplayContainer> containers = new List<AbstractMenuDisplayContainer>();

        /// <summary>
        /// Associate a menu to its displayer (if any).
        /// </summary>
        private readonly Dictionary<AbstractMenu, AbstractMenuDisplayContainer> menuToDisplayer = new Dictionary<AbstractMenu, AbstractMenuDisplayContainer>();
        private readonly Dictionary<AbstractMenuItem, AbstractDisplayer> itemToDisplayer = new Dictionary<AbstractMenuItem, AbstractDisplayer>();
    }

    public partial class MenuDisplayManager
    {
        public void AddMenuToRoot(AbstractMenu abstractMenu)
        {
            menu.Add(abstractMenu);
        }

        public void RemoveMenuFromRootWithoutDestroy(AbstractMenu abstractMenu)
        {
            menu.RemoveWithoutNotify(abstractMenu);
        }

        /// <summary>
        /// Display the menu.
        /// </summary>
        /// <param name="update">Should the display be updated (in case of changes in menu)</param>
        public void Display(bool update)
        {
            if (menu == null || menu.Count == 0)
                return;

            if (!isDisplaying)
                CreateDisplay();
            else if (update)
            {
                Hide(true);
                CreateDisplay();
                //if (LastMenuUnderNavigation is AbstractMenu lastMenu)
                //    Navigate(lastMenu);
            }
            else
            {
                foreach (AbstractMenuDisplayContainer container in containers)
                {
                    container.Display();
                    container.Expand();
                }
            }

            onDisplay.Invoke();
        }

        /// <summary>
        /// Hide the menu.
        /// </summary>
        /// <param name="clear">Should the display be cleared</param>
        public void Hide(bool clear = false)
        {
            onHide.Invoke();
            if (clear)
                Clear();
            else
            {
                foreach (AbstractMenuDisplayContainer container in containers)
                    container.Hide();
                isDisplaying = false;
            }
        }

        /// <summary>
        /// Expand the Root MenuDisplayer.
        /// </summary>
        /// <param name="update">this argument is used in Expand(bool) function of the root Menudisplayer</param>
        /// <see cref="AbstractMenuDisplayContainer.Expand(bool)"/>
        public void Expand(bool update)
        {
            if (containers.Count > 0) containers[0].Expand(update);
        }

        /// <summary>
        /// Navigate to a Menu
        /// </summary>
        /// <param name="submenu"></param>
        public void Navigate(AbstractMenu submenu)
        {
            if (!menuToDisplayer.TryGetValue(submenu, out AbstractMenuDisplayContainer displayer))
            {
                throw new System.Exception("Internal error : no displayer found for this menu");
            }
            lastMenuContainerUnderNavigation = displayer;
            if (displayer.parent == null)
            {
                displayer.Display();
                displayer.Expand(true);
            }
            else
            {
                AbstractMenuDisplayContainer Currentdisplayer;
                if (displayer.generationOffsetOnExpand < 0)
                {
                    Currentdisplayer = displayer;
                }
                else
                {
                    AbstractMenuDisplayContainer VirtualDisplayer = displayer;
                    int offset = displayer.generationOffsetOnExpand;
                    while (offset > -1 && VirtualDisplayer.parent != null)
                    {
                        while (offset > -1 && VirtualDisplayer.parent != null)
                        {
                            VirtualDisplayer = VirtualDisplayer.parent;
                            offset--;
                        }
                        offset = VirtualDisplayer.generationOffsetOnExpand;

                    }
                    Currentdisplayer = VirtualDisplayer;
                }

                if (Currentdisplayer.parent != null && !Currentdisplayer.parent.isExpanded)
                {
                    Navigate(Currentdisplayer.parent.menu as AbstractMenu);
                }
                Currentdisplayer.Display();
                Currentdisplayer.Collapse();
                if (Currentdisplayer != displayer)
                {
                    Currentdisplayer.ExpandAs(displayer, true);
                }
                else
                {
                    //Collapse all siblings if parrallel navigation isn't allowed
                    if (!displayer.parent.parallelNavigation)
                    {
                        foreach (AbstractDisplayer siblings in displayer.parent)
                        {

                            if ((siblings is AbstractMenuDisplayContainer) && ((siblings as AbstractMenuDisplayContainer) != displayer))
                                (siblings as AbstractMenuDisplayContainer).Collapse();
                        }
                    }
                    displayer.Expand(true);
                }
            }
        }

        /// <summary>
        /// Call the Back function of the last menu under navigation
        /// </summary>
        /// <see cref="AbstractMenuDisplayContainer.Back()"/>
        public void Back()
        {
            if (!isDisplaying)
                return;
            lastMenuContainerUnderNavigation.Back();
            if ((lastMenuContainerUnderNavigation.menu == menu) && hideOnBack)
                Hide(false);
        }

        #region private

        /// <summary>
        /// Display the menu (internal use), no update performed here.
        /// </summary>
        /// <see cref="Display(bool)"/>
        [ContextMenu("Display")]
        private void Display()
            => Display(false);

        /// <summary>
        /// Hide the menu (without clearing).
        /// </summary>
        [ContextMenu("Hide")]
        private void Hide()
            => Hide(false);

        /// <summary>
        /// Expand the Root MenuDisplayer. Without updating.
        /// </summary>
        /// <see cref="Expand(bool)"/>
        [ContextMenu("Expand")]
        private void Expand()
            => Expand(false);

        /// <summary>
        /// Create the menu display, and display it.
        /// </summary>
        [ContextMenu("Display Create")]
        private void CreateDisplay()
        {
            if (isDisplaying || menuAsset == null || menu == null)
                return;

            AbstractMenuDisplayContainer root = CreateSubMenu(null, menu, 0);
            root.backButtonPressed.AddListener(firstButtonBackButtonPressed.Invoke);
            lastMenuContainerUnderNavigation = root;
            recursivelyDisplayContainer(root, 0);
            isDisplaying = true;
        }

        /// <summary>
        /// Create a container for a menu and add it into a container
        /// </summary>
        /// <param name="container"></param>
        /// <param name="subMenu"></param>
        /// <param name="containerDepth">Depth of the container (zero is root)</param>
        /// <returns></returns>
        private AbstractMenuDisplayContainer CreateSubMenu(AbstractMenuDisplayContainer container, AbstractMenu subMenu, int containerDepth)
        {
            if (menuToDisplayer.ContainsKey(subMenu))
                return menuToDisplayer[subMenu];

            AbstractMenuDisplayContainer subContainer = containerSelector.ChooseContainer(subMenu, containerDepth);
            if (!firstContainerInScene)
                subContainer = Instantiate(subContainer, this.transform);

            //menu.OnDestroy.AddListener(() =>
            //{
            //    menu.onAbstractMenuItemAdded.RemoveAllListeners();
            //    this.onDestroy.RemoveListener(OnManagerDestroyedAction);
            //    if ((parentContainer != null) && (parentContainer.gameObject != null))
            //        parentContainer?.Remove(container, false);
            //    if ((container != null) && (container.gameObject != null))
            //        Destroy(container.gameObject);
            //});

            subContainer.SetMenuItem(subMenu);
            if (container != null)
            {
                subContainer.parent = container;
                subContainer.backButtonPressed.AddListener(() => Navigate(container.menu as AbstractMenu));
                container.Insert(subContainer, false);
            }
            containers.Add(subContainer);
            menuToDisplayer.Add(subMenu, subContainer);

            foreach (AbstractMenu sub in subMenu.GetSubMenu())
                CreateSubMenu(subContainer, sub, containerDepth + 1);
            foreach (AbstractMenuItem item in subMenu.GetMenuItems())
                CreateItem(subContainer, item);

            SetMenuAction(container, subMenu, subContainer, containerDepth);
            if (subMenu.navigable)
            {
                subContainer.Subscribe(() =>
                {
                    Navigate(subMenu);
                });
            }

            return subContainer;
        }

        /// <summary>
        /// Create a displayer for a menuItem and add it into a container.
        /// </summary> 
        /// <param name="container"></param>
        /// <param name="menuItem"></param>
        private void CreateItem(AbstractMenuDisplayContainer container, AbstractMenuItem menuItem)
        {
            if (itemToDisplayer.ContainsKey(menuItem) && itemToDisplayer[menuItem] == null)
                return;

            AbstractDisplayer disp = ChooseDisplayer(menuItem);
            menuItem.OnDestroy.AddListener(() =>
            {
                container.Remove(disp);
                Destroy(disp.gameObject);
            });
            disp.SetMenuItem(menuItem);
            container.Insert(disp, false);
            itemToDisplayer.Add(menuItem, disp);
        }

        /// <summary>
        /// Set Call BAck Action on menu change
        /// </summary>
        /// <param name="container">parent container</param>
        /// <param name="subMenu"></param>
        /// <param name="subContainer"></param>
        /// <param name="containerDepth"></param>
        private void SetMenuAction(AbstractMenuDisplayContainer container, AbstractMenu subMenu, AbstractMenuDisplayContainer subContainer, int containerDepth)
        {
            UnityAction<AbstractMenuItem> onItemAdded = (menuItem) =>
            {
                if (!menuToDisplayer.TryGetValue(subMenu, out AbstractMenuDisplayContainer currentSubContainer))
                    return;
                if (menuItem is AbstractMenu menu) 
                    CreateSubMenu(currentSubContainer, menu, containerDepth);
                else 
                    CreateItem(currentSubContainer, menuItem);
            };

            UnityAction OnManagerDestroyedAction = () =>
            {
                subMenu.onAbstractMenuItemAdded.RemoveListener(onItemAdded);
            };

            subMenu.onAbstractMenuItemAdded.AddListener(onItemAdded);

            subMenu.OnDestroy.AddListener(() =>
            {
                subMenu.onAbstractMenuItemAdded.RemoveAllListeners();
                this.onDestroy.RemoveListener(OnManagerDestroyedAction);
                if ((container != null) && (container.gameObject != null))
                    container?.Remove(subContainer, false);
                if ((subContainer != null) && (subContainer.gameObject != null))
                    Destroy(subContainer.gameObject);
            });

            this.onDestroy.AddListener(OnManagerDestroyedAction);
        }

        #endregion
    }

    public partial class MenuDisplayManager : MonoBehaviour
    {
        private void Awake()
        {
            if (menuAsset.menu == null)
            {
                menuAsset.menu = new Menu();
            }
        }

        /// <summary>
        /// Associate a displayer to a menu item depending on its type.
        /// </summary>
        /// <param name="menuItem">Menu item to find displayer for</param>
        /// <returns></returns>
        private AbstractDisplayer ChooseDisplayer(AbstractMenuItem menuItem, Transform parent = null)
        {
            if (parent == null)
                parent = this.transform;
            int suitableness = -1;
            AbstractDisplayer result = null;
            foreach (AbstractDisplayer displayer in DisplayerPrefab)
            {
                int tmp = displayer.IsSuitableFor(menuItem);
                if (tmp > suitableness)
                {
                    suitableness = tmp;
                    result = displayer;
                    if (suitableness == int.MaxValue)
                        break;
                }
            }
            if (suitableness < 1)
                throw new System.Exception("Internal error : no displayer prefab found for this menu of type " + menuItem.GetType());
            return Instantiate(result, parent);
        }

        /// <summary>
        /// Internal function for menu display.
        /// </summary>
        /// <param name="menu"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        private void recursivelyDisplayContainer(AbstractMenuDisplayContainer container, int depth)
        {
            if (depth > displayDepth)
            {
                if (depth > displayDepth + 1)
                    container.Hide();
                else
                    container.Display(true);
                container.Collapse(true);
            }
            else
            {
                lastMenuContainerUnderNavigation = container;
                container.Display(true);
                container.Expand(true);
            }
            foreach (AbstractDisplayer item in container)
            {
                AbstractMenuDisplayContainer subContainer;
                if ((subContainer = item as AbstractMenuDisplayContainer) != null)
                {
                    recursivelyDisplayContainer(subContainer, depth + 1);
                }
                else
                {
                    if (depth <= displayDepth)
                        item.Display(true);
                    else
                        item.Hide();
                }
            }

        }

        

        

        

        

        

        /// <summary>
        /// Find a given submenu in <see cref="menu"/> and return its path.
        /// </summary>
        /// <param name="menuToFind"></param>
        /// <returns></returns>
        private List<AbstractMenu> FindMenuPath(AbstractMenu menuToFind)
        {
            List<AbstractMenu> aux(AbstractMenu submenu)
            {
                if (submenu == menuToFind)
                {
                    return new List<AbstractMenu>();
                }

                List<AbstractMenu> buffer;
                foreach (AbstractMenu sub in submenu.GetSubMenu())
                {
                    buffer = aux(sub);
                    if (buffer != null)
                    {
                        buffer.Insert(0, submenu);
                        return buffer;
                    }
                }
                return null;
            }

            return aux(menu);
        }

        /// <summary>
        /// Clear the display.
        /// </summary>
        [ContextMenu("Clear")]
        private void Clear()
        {
            if (firstContainerInScene)
            {
                if (containers.Count > 0)
                {

                    containers[0].Hide();
                    foreach (AbstractDisplayer displayer in containers[0])
                    {
                        Destroy(displayer.gameObject);
                    }
                    containers[0].Clear();
                    for (int i = 1; i < containers.Count; i++)
                    {
                        containers[i].Clear();
                        Destroy(containers[i].gameObject);
                    }

                    containers = new List<AbstractMenuDisplayContainer>();
                }
            }
            else
            {
                if ((containers != null) && (containers.Count > 0))
                {
                    containers[0].backButtonPressed.RemoveListener(firstButtonBackButtonPressed.Invoke);
                    for (int i = 0; i < containers.Count; i++)
                    {
                        containers[i].Clear();
                        if (containers[i] != null)
                            Destroy(containers[i].gameObject);
                    }
                }
                containers = new List<AbstractMenuDisplayContainer>();
            }

            menuToDisplayer.Clear();
            itemToDisplayer.Clear();
            isDisplaying = false;
        }


        private void OnDestroy()
        {
            onDestroy.Invoke();
            Clear();
        }


    }
}