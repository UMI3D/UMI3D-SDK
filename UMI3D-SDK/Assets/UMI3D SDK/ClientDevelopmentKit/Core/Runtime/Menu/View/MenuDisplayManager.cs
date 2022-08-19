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
        [SerializeField]
        private bool m_displayedOnStart = false;
        [SerializeField]
        private bool m_collapseOnStart = false;
        /// <summary>
        /// Updates display to display depth when a new item is added.
        /// </summary>
        [SerializeField]
        private bool m_displayWhenAddNewItem = false;
        [SerializeField]
        private bool m_collapseWhenAddNewItem = false;
        /// <summary>
        /// Should call the Hide function when the Back function is called on root.
        /// </summary>
        public bool hideOnBack = false;
        /// <summary>
        /// Displayer : Display <= displayDepth < Hide
        /// Container : Display <= displayDepth + 1 < Hide
        /// Container : Expand <= displayDepth < Collapse
        /// </summary>
        [Tooltip("Displayer : Display <= displayDepth < Hide. Container : Display <= displayDepth + 1 < Hide; Expand <= displayDepth < Collapse")]
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
        public bool IsMenuCreated { get; protected set; } = false;
        /// <summary>
        /// Is the menu being displayed ?
        /// </summary>
        public bool isDisplaying { get; protected set; } = false;
        public AbstractMenuDisplayContainer lastMenuContainerUnderNavigation { get; protected set; }

        /// <summary>
        /// Instantiated container (if any)
        /// </summary>
        private readonly List<AbstractMenuDisplayContainer> containers = new List<AbstractMenuDisplayContainer>();

        /// <summary>
        /// Associate a menu to its displayer (if any).
        /// </summary>
        private readonly Dictionary<AbstractMenu, AbstractMenuDisplayContainer> menuToDisplayer = new Dictionary<AbstractMenu, AbstractMenuDisplayContainer>();
        private readonly Dictionary<AbstractMenuItem, AbstractDisplayer> itemToDisplayer = new Dictionary<AbstractMenuItem, AbstractDisplayer>();

        private AbstractMenuDisplayContainer m_root { get; set; } = null;
    }

    public partial class MenuDisplayManager
    {
        /// <summary>
        /// Creates the menu, and displays it.
        /// </summary>
        /// <param name="update"></param>
        /// <param name="navigateToLastMenu"></param>
        public void CreateMenuAndDisplay(bool update, bool navigateToLastMenu)
        {
            if (IsMenuCreated && !update)
                return;
            IsMenuCreated = true;

            Clear();
            m_root = CreateSubMenu(null, menu, 0);
            m_root.backButtonPressed.AddListener(firstButtonBackButtonPressed.Invoke);
            if (lastMenuContainerUnderNavigation == null)
                lastMenuContainerUnderNavigation = m_root;
            if (navigateToLastMenu)
            {
                AbstractMenuItem last = lastMenuContainerUnderNavigation.menu;
                if (last is AbstractMenu lastMenu)
                    Navigate(lastMenu);
                onDisplay.Invoke();
            }
            else
                Display(update);
        }

        /// <summary>
        /// Clear the display.
        /// </summary>
        [ContextMenu("Clear")]
        public void Clear()
        {
            if (!IsMenuCreated)
                return;

            menuToDisplayer.Clear();
            itemToDisplayer.Clear();
            isDisplaying = false;

            if ((containers == null) || (containers.Count == 0))
                return;

            containers[0].Hide();
            containers[0].backButtonPressed.RemoveListener(firstButtonBackButtonPressed.Invoke);

            for (int i = 0; i < containers.Count; i++)
            {
                containers[i].Clear();
                if (firstContainerInScene && i == 0)
                    continue;
                if (containers[i] != null)
                    Destroy(containers[i].gameObject);
            }

            containers.Clear();
        }

        /// <summary>
        /// Displays the Root MenuDisplayer.
        /// </summary>
        /// <param name="update"></param>
        public void Display(bool update)
        {
            if (isDisplaying && !update)
                return;
            CreateMenuAndDisplay();
            ApplyDisplayDepthOnDisplayerDisplayStatus(m_root, 0);
            isDisplaying = true;
            onDisplay.Invoke();
        }

        /// <summary>
        /// Displays And Expands All containers.
        /// <param name="update"></param>
        /// </summary>
        public void DisplayAndExpandAll(bool update)
        {
            if (isDisplaying && !update)
                return;

            foreach (AbstractMenuDisplayContainer container in containers)
            {
                container.Display();
                container.Expand();
            }

            isDisplaying = true;
            onDisplay.Invoke();
        }

        /// <summary>
        /// Hide the menu.
        /// </summary>
        /// <param name="clear">Should the display be cleared</param>
        public void Hide(bool clear = false)
        {
            onHide.Invoke();
            if (!clear)
            {
                foreach (AbstractMenuDisplayContainer container in containers)
                    container.Hide();
                isDisplaying = false;
            }
            else
                Clear();
        }

        /// <summary>
        /// Expand the Root MenuDisplayer.
        /// </summary>
        /// <param name="update">this argument is used in Expand(bool) function of the root Menudisplayer</param>
        /// <see cref="AbstractMenuDisplayContainer.Expand(bool)"/>
        public void Expand(bool update)
        {
            if (isDisplaying && !update)
                return;
            if (containers.Count > 0) containers[0].Expand(update);
            isDisplaying = true;
            onDisplay.Invoke();
        }

        /// <summary>
        /// Expand all the menus.
        /// </summary>
        public void ExpandAll()
        {
            foreach (AbstractMenuDisplayContainer container in containers)
                container.Expand();
            isDisplaying = true;
            onDisplay.Invoke();
        }

        /// <summary>
        /// Collapse the Root MenuDisplayer.
        /// </summary>
        public void Collapse(bool update)
        {
            if (!isDisplaying && !update)
                return;
            onHide.Invoke();
            if (containers.Count > 0) containers[0].Collapse(update);
            isDisplaying = false;
        }

        /// <summary>
        /// Collapse all the menus.
        /// </summary>
        public void CollapseAll()
        {
            onHide.Invoke();
            foreach (AbstractMenuDisplayContainer container in containers)
                container.Collapse();
            isDisplaying = false;
        }

        /// <summary>
        /// Navigate to a Menu
        /// </summary>
        /// <param name="submenu"></param>
        public void Navigate(AbstractMenu submenu)
        {
            if (!menuToDisplayer.TryGetValue(submenu, out AbstractMenuDisplayContainer displayer))
                throw new System.Exception("Internal error : no displayer found for this menu");

            AbstractMenuDisplayContainer precDisplayer = displayer.parent;
            if (precDisplayer == null)
            {
                lastMenuContainerUnderNavigation = displayer;
                displayer.Display();
                displayer.Collapse();
                displayer.Expand();
                return;
            }

            AbstractMenuDisplayContainer currentDisplayer;
            if (displayer.generationOffsetOnExpand >= 0)
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
                currentDisplayer = VirtualDisplayer;
            }
            else
                currentDisplayer = displayer;

            AbstractMenuDisplayContainer precCurrentDisplayer = currentDisplayer.parent;
            if (precCurrentDisplayer != null && !precCurrentDisplayer.isExpanded)
                Navigate(precCurrentDisplayer.menu as AbstractMenu);

            lastMenuContainerUnderNavigation = displayer;
            currentDisplayer.Display();
            currentDisplayer.Collapse();

            if (currentDisplayer == displayer)
            {
                CollapseSiblings(precDisplayer, displayer);
                displayer.Expand(true);
            }
            else
                currentDisplayer.ExpandAs(displayer, true);
        }

        /// <summary>
        /// Collapse [container] siblings.
        /// </summary>
        /// <param name="precContainre"></param>
        /// <param name="container"></param>
        private void CollapseSiblings(AbstractMenuDisplayContainer precContainre, AbstractMenuDisplayContainer container)
        {
            if (precContainre == null || precContainre.parallelNavigation)
                return;

            foreach (AbstractDisplayer sibling in precContainre)
            {
                if (!(sibling is AbstractMenuDisplayContainer siblingContainter) || siblingContainter == container)
                    continue;
                siblingContainter.Collapse(true);
            }
            CollapseSiblings(precContainre.parent, precContainre);
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
        {
            Display(false);
        }

        /// <summary>
        /// Hide the menu (without clearing).
        /// </summary>
        [ContextMenu("Hide")]
        private void Hide()
        {
            Hide(false);
        }

        /// <summary>
        /// Expand the Root MenuDisplayer. Without updating.
        /// </summary>
        /// <see cref="Expand(bool)"/>
        [ContextMenu("Expand")]
        private void Expand()
        {
            Expand(false);
        }

        /// <summary>
        /// Collapse the Root MenuDisplayer. Without updating.
        /// </summary>
        /// <see cref="Collapse(bool)"/>
        [ContextMenu("Collapse")]
        private void Collapse()
        {
            Collapse(false);
        }

        /// <summary>
        /// Create the menu, and display it.
        /// </summary>
        [ContextMenu("Create Display")]
        private void CreateMenuAndDisplay()
        {
            CreateMenuAndDisplay(false, false);
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
            if ((containerDepth > 0) || !firstContainerInScene)
                subContainer = Instantiate(subContainer, this.transform);

            subContainer.SetMenuItem(subMenu);
            containers.Add(subContainer);
            menuToDisplayer.Add(subMenu, subContainer);

            if (container != null)
            {
                subContainer.parent = container;
                subContainer.backButtonPressed.AddListener(() => Navigate(container.menu as AbstractMenu));
                container.Insert(subContainer, false);
            }

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
            if (itemToDisplayer.ContainsKey(menuItem) && itemToDisplayer[menuItem] != null)
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
            UnityAction<AbstractMenuItem> onItemAdded = (subSubMenuItem) =>
            {
                if (!menuToDisplayer.TryGetValue(subMenu, out AbstractMenuDisplayContainer currentSubContainer))
                    return;
                if (subSubMenuItem is AbstractMenu subSubMenu)
                    CreateSubMenu(currentSubContainer, subSubMenu, containerDepth + 1);
                else
                    CreateItem(currentSubContainer, subSubMenuItem);
                if (m_displayWhenAddNewItem)
                    Display(true);
                if (m_collapseWhenAddNewItem)
                    Collapse(true);
            };
            subMenu.onAbstractMenuItemAdded.AddListener(onItemAdded);

            UnityAction<AbstractMenuItem> onItemRemoved = (subSubMenuItem) =>
            {
                if (!menuToDisplayer.TryGetValue(subMenu, out AbstractMenuDisplayContainer currentSubContainer))
                    return;
                if (subSubMenuItem is AbstractMenu subSubMenu)
                    RemoveSubMenu(currentSubContainer, subSubMenu);
                else
                    RemoveItem(currentSubContainer, subSubMenuItem);
            };
            subMenu.OnAbstractMenuItemRemoved.AddListener(onItemRemoved);


            UnityAction OnManagerDestroyedAction = () =>
            {
                subMenu.onAbstractMenuItemAdded.RemoveListener(onItemAdded);
                subMenu.OnAbstractMenuItemRemoved.RemoveListener(onItemRemoved);
            };
            this.onDestroy.AddListener(OnManagerDestroyedAction);

            subMenu.OnDestroy.AddListener(() =>
            {
                this.onDestroy.RemoveListener(OnManagerDestroyedAction);

                if ((container != null) && (container.gameObject != null))
                    container?.Remove(subContainer, false);

                if ((subContainer != null) && (subContainer.gameObject != null))
                    Destroy(subContainer.gameObject);
            });
        }

        /// <summary>
        /// Display, hide, expand or collapse [displayer] according to the [depth] of thi displayer and the value of [displayDepth].
        /// </summary>
        /// <param name="displayer"></param>
        /// <param name="depth"></param>
        private void ApplyDisplayDepthOnDisplayerDisplayStatus(AbstractDisplayer displayer, int depth)
        {
            if (displayer is AbstractMenuDisplayContainer container)
            {
                if (depth <= displayDepth + 1)
                {
                    container.Display(true);
                    if (depth <= displayDepth)
                        container.Expand(true);
                    else
                        container.Collapse(true);
                }
                else
                {
                    container.Collapse(true);
                    container.Hide();
                }

                foreach (AbstractDisplayer subDisplayer in container)
                    ApplyDisplayDepthOnDisplayerDisplayStatus(subDisplayer, depth + 1);
            }
            else
            {
                if (depth <= displayDepth)
                    displayer.Display(true);
                else
                    displayer.Hide();
            }
        }

        /// <summary>
        /// Remove [subMenu] from [container] and remove its children.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="subMenu"></param>
        private void RemoveSubMenu(AbstractMenuDisplayContainer container, AbstractMenu subMenu)
        {
            if (!menuToDisplayer.TryGetValue(subMenu, out AbstractMenuDisplayContainer subContainer))
                return;
            if (lastMenuContainerUnderNavigation == subContainer)
                lastMenuContainerUnderNavigation = m_root;
            container?.Remove(subContainer, false);
            menuToDisplayer.Remove(subMenu);
            foreach (AbstractMenu subSubMenu in subMenu.GetSubMenu())
                RemoveSubMenu(subContainer, subSubMenu);
            foreach (AbstractMenuItem subItem in subMenu.GetMenuItems())
                RemoveItem(subContainer, subItem);
            Destroy(subContainer.gameObject);
        }

        /// <summary>
        /// Remove [menuItem] from [container].
        /// </summary>
        /// <param name="container"></param>
        /// <param name="menuItem"></param>
        private void RemoveItem(AbstractMenuDisplayContainer container, AbstractMenuItem menuItem)
        {
            if (!itemToDisplayer.TryGetValue(menuItem, out AbstractDisplayer displayer))
                return;
            container?.Remove(displayer, false);
            itemToDisplayer.Remove(menuItem);
            Destroy(displayer.gameObject);
        }

        #endregion
    }

    public partial class MenuDisplayManager : MonoBehaviour
    {
        private void Awake()
        {
            Debug.Assert(menuAsset != null, "MenuAsset null in MenuDisplayManager.");
            if (menuAsset.menu == null)
                menuAsset.menu = new Menu();
        }

        private void Start()
        {
            if (m_displayedOnStart)
            {
                CreateMenuAndDisplay();
                if (m_collapseOnStart)
                    Collapse();
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

        private void OnDestroy()
        {
            onDestroy.Invoke();
            Clear();
        }
    }
}