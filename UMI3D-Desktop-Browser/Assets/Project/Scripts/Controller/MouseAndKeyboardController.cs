/*
Copyright 2019 Gfi Informatique

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
using BrowserDesktop.Cursor;
using BrowserDesktop.Interaction;
using BrowserDesktop.Menu;
using BrowserDesktop.Parameters;
using System.Collections.Generic;
using umi3d.cdk;
using umi3d.common;
using UnityEngine;

namespace BrowserDesktop.Controller
{
    public class MouseAndKeyboardController : AbstractController
    {

        public UMI3DBrowserAvatar Avatar;
        public Camera Camera;

        public InteractionMapper InteractionMapper;

        public umi3d.cdk.menu.core.Menu contextualMenu;

        int navigationDirect = 0;
        [SerializeField]
        List<DofGroupEnum> dofGroups = new List<DofGroupEnum>();
        [SerializeField]
        List<CursorKeyInput> ManipulationActionInput = new List<CursorKeyInput>();

        /// <summary>
        /// Avatar bone linked to this input.
        /// </summary>
        public BoneType bone = BoneType.Hand_Right;

        protected BoneDto boneDto;


        #region Hover

        public struct MouseData
        {
            public Interactable OldHovered;
            public Interactable CurentHovered;

            public Vector3 point;
            public Vector3 worldPoint;
            public Vector3 centeredWorldPoint;
            public Vector3 normal;
            public Vector3 worldNormal;
            public Vector3 cursorOffset;

            public HoverState HoverState;

            public int saveDelay;

            public void save()
            {
                if (saveDelay > 0)
                {
                    saveDelay--;
                }
                else
                {
                    if (saveDelay < 0) saveDelay = 0;
                    OldHovered = CurentHovered;
                    CurentHovered = null;
                }
            }

            public bool isDelaying()
            {
                return saveDelay > 0;
            }
        }
        public enum HoverState { None, Hovering, AutoProjected }

        [SerializeField]
        public MouseData mouseData;
        public const float timeToHold = 0.1f;

        #endregion

        #region Inputs

        List<ManipulationGroup> ManipulationInputs = new List<ManipulationGroup>();
        List<KeyInput> KeyInputs = new List<KeyInput>();

        List<KeyMenuInput> KeyMenuInputs = new List<KeyMenuInput>();

        /// <summary>
        /// Instantiated float parameter inputs.
        /// </summary>
        /// <see cref="FindInput(AbstractParameterDto, bool)"/>
        protected List<FloatParameterInput> floatParameterInputs = new List<FloatParameterInput>();

        /// <summary>
        /// Instantiated float range parameter inputs.
        /// </summary>
        /// <see cref="FindInput(AbstractParameterDto, bool)"/>
        protected List<FloatRangeParameterInput> floatRangeParameterInputs = new List<FloatRangeParameterInput>();

        /// <summary>
        /// Instantiated int parameter inputs.
        /// </summary>
        /// <see cref="FindInput(AbstractParameterDto, bool)"/>
        protected List<IntParameterInput> intParameterInputs = new List<IntParameterInput>();

        /// <summary>
        /// Instantiated bool parameter inputs.
        /// </summary>
        /// <see cref="FindInput(AbstractParameterDto, bool)"/>
        protected List<BooleanParameterInput> boolParameterInputs = new List<BooleanParameterInput>();

        /// <summary>
        /// Instantiated string parameter inputs.
        /// </summary>
        /// <see cref="FindInput(AbstractParameterDto, bool)"/>
        protected List<StringParameterInput> stringParameterInputs = new List<StringParameterInput>();

        /// <summary>
        /// Instantiated string enum parameter inputs.
        /// </summary>
        /// <see cref="FindInput(AbstractParameterDto, bool)"/>
        protected List<StringEnumParameterInput> stringEnumParameterInputs = new List<StringEnumParameterInput>();

        public override List<AbstractUMI3DInput> inputs
        {
            get {
                List<AbstractUMI3DInput> list = new List<AbstractUMI3DInput>();
                list.AddRange(ManipulationInputs);
                list.AddRange(KeyInputs);
                list.AddRange(KeyMenuInputs);
                list.AddRange(floatParameterInputs);
                list.AddRange(floatRangeParameterInputs);
                list.AddRange(intParameterInputs);
                list.AddRange(boolParameterInputs);
                list.AddRange(stringParameterInputs);
                list.AddRange(stringEnumParameterInputs);
                return list;
            }
        }

        #endregion

        public void Awake()
        {
            foreach (KeyInput input in GetComponentsInChildren<KeyInput>())
            {
                KeyInputs.Add(input);
                input.Init(this);
                input.bone = BoneType.Cursor;
            }
            mouseData.saveDelay = 0;
        }

        void InputDown(KeyInput input) { }
        void InputUp(KeyInput input) { }

        private void Update()
        {
            if (Input.GetKeyDown(InputLayoutManager.GetInputCode(InputLayoutManager.Input.ContextualMenuNavigationDirect)) || Input.mouseScrollDelta.y > 0)
            {
                navigationDirect++;
            }
            else if (Input.mouseScrollDelta.y < 0)
            {
                navigationDirect--;
            }
        }


        private void FixedUpdate()
        {
            if (MainMenu.IsDisplaying)
            {
                mouseData.save();
                Hover();
            }
            else
            {
                if (navigationDirect != 0)
                {
                    if (navigationDirect > 0)
                        ManipulationInput.NextManipulation();
                    else
                        ManipulationInput.PreviousManipulation();
                    navigationDirect = 0;
                }
                MouseHandler();
            }
        }

        public void CircleMenuColapsed()
        {
            if (mouseData.CurentHovered == null) return;
            // CircleMenu.Instance.MenuColapsed.RemoveListener(CircleMenuColapsed);
            CursorHandler.State = CursorHandler.CursorState.Hover;
            mouseData.saveDelay = 3;
        }

        void MouseHandler()
        {


            if (!(
                        mouseData.HoverState == HoverState.AutoProjected
                        && (CursorHandler.State == CursorHandler.CursorState.Clicked || CircleMenu.Exist && CircleMenu.Instance.IsExpanded)
               ))
            {
                mouseData.save();
                Vector3 screenPosition = Input.mousePosition;
                Ray ray = Camera.main.ScreenPointToRay(screenPosition);
                Debug.DrawRay(ray.origin, ray.direction.normalized * 100f, Color.red, 0, true);
                RaycastHit[] hits = umi3d.Physics.RaycastAll(ray, 100f);
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.gameObject.GetComponentInParent<HDScene>() == null)
                        continue;
                    var Interactable = hit.collider.gameObject.GetComponent<Interactable>();
                    if (Interactable == null)
                        Interactable = hit.collider.gameObject.GetComponentInParent<Interactable>();
                    if (Interactable != null)
                    {
                        mouseData.CurentHovered = Interactable;
                        mouseData.point = hit.transform.InverseTransformPoint(hit.point);
                        mouseData.worldPoint = hit.point;
                        if (Vector3.Distance(mouseData.worldPoint, hit.transform.position) < 0.1f) mouseData.centeredWorldPoint = hit.transform.position;
                        else mouseData.centeredWorldPoint = mouseData.worldPoint;

                        mouseData.normal = hit.transform.InverseTransformDirection(hit.normal);
                        mouseData.worldNormal = hit.normal;
                        break;
                    }
                }
                Hover();
            }
            else
            {
                CircleMenu.Instance.Follow(mouseData.centeredWorldPoint);
            }
        }

        void Hover()
        {
            if (boneDto == null)
            {
                boneDto = UMI3DBrowserAvatar.Instance.avatar.boneList.Find(b => b.type == bone);
            }
                

            if (mouseData.CurentHovered != null)
            {
                if (mouseData.CurentHovered != mouseData.OldHovered)
                {
                    if (mouseData.OldHovered != null)
                    {
                        if (mouseData.HoverState == HoverState.AutoProjected)
                        {
                            InteractionMapper.ReleaseTool(mouseData.OldHovered.dto.Id, new RequestedByUser());
                        }
                        mouseData.OldHovered.HoverExit(boneDto.Id);
                        CircleMenu.Instance.Collapse();
                        mouseData.OldHovered = null;
                    }
                    mouseData.HoverState = HoverState.Hovering;
                    if (mouseData.CurentHovered.dto.interactions.Count > 0 && IsCompatibleWith(mouseData.CurentHovered))
                    {
                        InteractionMapper.SelectTool(mouseData.CurentHovered.dto.Id, this);
                        CursorHandler.State = CursorHandler.CursorState.Hover;
                        mouseData.HoverState = HoverState.AutoProjected;
                        CircleMenu.Instance.MenuColapsed.AddListener(CircleMenuColapsed);
                        mouseData.OldHovered = mouseData.CurentHovered;
                    }
                    mouseData.CurentHovered.HoverEnter(boneDto.Id);
                }
                mouseData.CurentHovered.Hovered(boneDto.Id, mouseData.point, mouseData.normal);
            }
            else if (mouseData.OldHovered != null)
            {
                if (mouseData.HoverState == HoverState.AutoProjected)
                {
                    CircleMenu.Instance.MenuColapsed.RemoveListener(CircleMenuColapsed);
                    InteractionMapper.ReleaseTool(mouseData.OldHovered.dto.Id, new RequestedByUser());
                }
                mouseData.OldHovered.HoverExit(boneDto.Id);
                CircleMenu.Instance.Collapse();
                CursorHandler.State = CursorHandler.CursorState.Default;
                mouseData.OldHovered = null;
                mouseData.HoverState = HoverState.None;
            }
        }

        bool ShouldAutoProject(InteractableDto tool)
        {
            List<AbstractInteractionDto> manips = tool.interactions.FindAll(x => x is ManipulationDto);
            List<AbstractInteractionDto> events = tool.interactions.FindAll(x => x is EventDto);
            List<AbstractInteractionDto> parameters = tool.interactions.FindAll(x => x is AbstractParameterDto);
            return (((parameters.Count == 0) && (events.Count <= 7) && (manips.Count == 0)));
        }

        public override void Clear()
        {
            foreach (ManipulationGroup input in ManipulationInputs)
            {
                if (!input.IsAvailable())
                    input.Dissociate();
            }
            foreach (KeyInput input in KeyInputs)
            {
                if (!input.IsAvailable())
                    input.Dissociate();
            }
            ClearParameters();
        }

        public void ClearParameters()
        {
            KeyMenuInputs.ForEach((a) => { Destroy(a); });
            KeyMenuInputs = new List<KeyMenuInput>();

            floatParameterInputs.ForEach((a) => { Destroy(a); });
            floatParameterInputs = new List<FloatParameterInput>();
            floatRangeParameterInputs.ForEach((a) => { Destroy(a); });
            floatRangeParameterInputs = new List<FloatRangeParameterInput>();
            intParameterInputs.ForEach((a) => { Destroy(a); });
            intParameterInputs = new List<IntParameterInput>();
            boolParameterInputs.ForEach((a) => { Destroy(a); });
            boolParameterInputs = new List<BooleanParameterInput>();
            stringParameterInputs.ForEach((a) => { Destroy(a); });
            stringParameterInputs = new List<StringParameterInput>();
            stringEnumParameterInputs.ForEach((a) => { Destroy(a); });
            stringEnumParameterInputs = new List<StringEnumParameterInput>();
        }

        /// <summary>
        /// Create a menu to access each interactions of a tool separately.
        /// </summary>
        /// <param name="interactions"></param>
        public override void CreateInteractionsMenuFor(AbstractTool tool)
        {
            //ToolMenuItem toolSubMenu;
            //if (!InteractionMapper.Instance.toolsIdToMenu.TryGetValue(tool.dto.Id, out toolSubMenu))
            //{
            //    InteractionMapper.Instance.CreateTool(tool.dto);
            //}
            //if (!InteractionMapper.Instance.toolsIdToMenu.TryGetValue(tool.dto.Id, out toolSubMenu))
            //{
            //    throw new System.Exception("Internal error");
            //}

            //umi3d.cdk.menu.core.Menu interactions;
            //if (!toolSubMenu.SubMenu.Exists(m => m.Name.Equals("Interactions")))
            //{
            //    interactions = new umi3d.cdk.menu.core.Menu() { Name = "Interactions", navigable = true };
            //    toolSubMenu.Add(interactions);
            //}
            //else
            //{
            //    interactions = toolSubMenu.SubMenu.Find(m => m.Name.Equals("Interactions"));
            //}

            //List<AbstractInteractionDto> interactions = tool.interactions.ConvertAll(id => InteractionMapper.Instance.GetInteraction(id));
            //List<AbstractInteractionDto> manips = interactions.FindAll(inter => inter is ManipulationDto);
            //foreach (AbstractInteractionDto manip in manips)
            //{
            //    DofGroupOptionDto bestSeparationOption = FindBest((manip as ManipulationDto).dofSeparationOptions.ToArray());
            //    foreach (DofGroupDto sep in bestSeparationOption.separations)
            //    {
            //        ManipulationMenuItem manipSeparationMenu = new ManipulationMenuItem()
            //        {
            //            Name = manip.Name + "-" + sep.name,
            //            dof = sep,
            //            interaction = manip as ManipulationDto
            //        };

            //        manipSeparationMenu.Subscribe(() =>
            //        {
            //            projectionMemory.PartialProject(this, manip as ManipulationDto, sep, true);
            //        });

            //        contextualMenu.Add(manipSeparationMenu);

            //        if (FindInput(manip as ManipulationDto, sep, true) != null)
            //        {
            //            manipSeparationMenu.Select();
            //        }
            //    }
            //}

            //List<AbstractInteractionDto> events = interactions.FindAll(x => x is EventDto);
            //foreach (AbstractInteractionDto evt in events)
            //{
            //    EventMenuItem eventMenu = new EventMenuItem()
            //    {
            //        interaction = evt as EventDto,
            //        Name = evt.Name
            //    };

            //    eventMenu.Subscribe(() =>
            //    {
            //        projectionMemory.PartialProject(this, evt as EventDto, true);
            //    });

            //    contextualMenu.Add(eventMenu);
            //    if (FindInput(evt as EventDto, true) != null)
            //    {
            //        eventMenu.Select();
            //    }
            //}
            Debug.Log("oups");
        }

        /// <summary>
        /// Check if a tool can be projected on this controller.
        /// </summary>
        /// <param name="tool"> The tool to be projected.</param>
        /// <returns></returns>
        public override bool IsCompatibleWith(AbstractTool tool)
        {
            List<AbstractInteractionDto> interactions = tool.interactions.ConvertAll(id => InteractionMapper.Instance.GetInteraction(id));
            List<AbstractInteractionDto> manips = interactions.FindAll(x => x is ManipulationDto);
            foreach (var man in manips)
            {
                var man2 = man as ManipulationDto;
                if (
                    !man2.dofSeparationOptions.Exists((sep) =>
                         {
                             foreach (DofGroupDto dof in sep.separations)
                             {
                                 if (!dofGroups.Contains(dof.dofs))
                                     return false;
                             }
                             return true;
                         }))
                    return false;
            }
            return true;

        }

        /// <summary>
        /// Check if a tool requires the generation of a menu to be projected.
        /// </summary>
        /// <param name="tool"> The tool to be projected.</param>
        /// <returns></returns>
        public override bool RequiresMenu(AbstractTool tool)
        {
            List<AbstractInteractionDto> interactions = tool.interactions.ConvertAll(id => InteractionMapper.Instance.GetInteraction(id));
            List<AbstractInteractionDto> manips = interactions.FindAll(x => x is ManipulationDto);
            List<AbstractInteractionDto> events = interactions.FindAll(x => x is EventDto);
            //List<AbstractInteractionDto> parameters = tool.Interactions.FindAll(x => x is AbstractParameterDto);
            // return ((events.Count > 7 || manips.Count > 0) && (events.Count > 6 || manips.Count > 1));
            return false; // (/*(parameters.Count > 0) ||*/ (events.Count > 7) || (manips.Count > 1) || ((manips.Count > 0) && (events.Count > 6)));
        }

        public bool RequiresParametersMenu(AbstractTool tool)
        {
            List<AbstractInteractionDto> interactions = tool.interactions.ConvertAll(id => InteractionMapper.Instance.GetInteraction(id));
            List<AbstractInteractionDto> parameters = interactions.FindAll(x => x is AbstractParameterDto);
            // return ((events.Count > 7 || manips.Count > 0) && (events.Count > 6 || manips.Count > 1));
            return (parameters.Count > 0);
        }

        protected override bool isInteracting()
        {
            throw new System.NotImplementedException();
        }

        protected override bool isNavigating()
        {
            throw new System.NotImplementedException();
        }

        public override DofGroupOptionDto FindBest(DofGroupOptionDto[] options)
        {

            foreach (var GroupOption in options)
            {
                bool ok = true;
                foreach (DofGroupDto dof in GroupOption.separations)
                {
                    if (!dofGroups.Contains(dof.dofs))
                    {
                        ok = false;
                        break;
                    }
                }
                if (ok) return GroupOption;
            }

            throw new System.NotImplementedException();
        }



        public override AbstractUMI3DInput FindInput(ManipulationDto manip, DofGroupDto dof, bool unused = true)
        {
            ManipulationGroup group = ManipulationInputs.Find(i => i.IsAvailableFor(manip));
            if (group == null)
            {
                group = ManipulationGroup.Instanciate(this, ManipulationActionInput, dofGroups, transform);
                if (group == null)
                {
                    Debug.LogWarning("find manip input FAILED");
                    return null;
                }
                ManipulationInputs.Add(group);
            }
            return group;
        }

        public override AbstractUMI3DInput FindInput(EventDto evt, bool unused = true)
        {
            KeyInput input = KeyInputs.Find(i => i.IsAvailable() || !unused);
            if (input == null)
            {
                KeyMenuInput inputMenu = KeyMenuInputs.Find(i => i.IsAvailable() || !unused);
                if (inputMenu == null)
                {
                    inputMenu = this.gameObject.AddComponent<KeyMenuInput>();
                    inputMenu.bone = BoneType.Cursor;
                    KeyMenuInputs.Add(inputMenu);
                }
                return inputMenu;
            }
            return input;
        }

        public override AbstractUMI3DInput FindInput(AbstractParameterDto param, bool unused = true)
        {
            if (param is FloatRangeParameterDto)
            {
                FloatRangeParameterInput floatRangeInput = floatRangeParameterInputs.Find(i => i.IsAvailable());
                if (floatRangeInput == null)
                {
                    floatRangeInput = this.gameObject.AddComponent<FloatRangeParameterInput>();
                    floatRangeParameterInputs.Add(floatRangeInput);
                }
                return floatRangeInput;
            }
            else if (param is FloatParameterDto)
            {
                FloatParameterInput floatInput = floatParameterInputs.Find(i => i.IsAvailable());
                if (floatInput == null)
                {
                    floatInput = this.gameObject.AddComponent<FloatParameterInput>();
                    floatParameterInputs.Add(floatInput);
                }
                return floatInput;
            }
            else if (param is IntegerParameterDto)
            {
                IntParameterInput intInput = intParameterInputs.Find(i => i.IsAvailable());
                if (intInput == null)
                {
                    intInput = new IntParameterInput();
                    intParameterInputs.Add(intInput);
                }
                return intInput;

            }
            else if (param is IntegerRangeParameterDto)
            {
                throw new System.NotImplementedException();
            }
            else if (param is BooleanParameterDto)
            {
                BooleanParameterInput boolInput = boolParameterInputs.Find(i => i.IsAvailable());
                if (boolInput == null)
                {
                    boolInput = this.gameObject.AddComponent<BooleanParameterInput>();
                    boolParameterInputs.Add(boolInput);
                }
                return boolInput;
            }
            else if (param is StringParameterDto)
            {
                StringParameterInput stringInput = stringParameterInputs.Find(i => i.IsAvailable());
                if (stringInput == null)
                {
                    stringInput = this.gameObject.AddComponent<StringParameterInput>();
                    stringParameterInputs.Add(stringInput);
                }
                return stringInput;
            }
            else if (param is EnumParameterDto<string>)
            {
                StringEnumParameterInput stringEnumInput = stringEnumParameterInputs.Find(i => i.IsAvailable());
                if (stringEnumInput == null)
                {
                    stringEnumInput = this.gameObject.AddComponent<StringEnumParameterInput>();
                    stringEnumParameterInputs.Add(stringEnumInput);
                }
                return stringEnumInput;
            }
            else
            {
                return null;
            }
        }

        public override void Release(AbstractTool tool)
        {
            //try
            //{
            base.Release(tool);
            if (mouseData.CurentHovered != null && mouseData.CurentHovered.dto.Id == tool.id)
            {
                mouseData.CurentHovered = null;
                mouseData.HoverState = HoverState.None;
                CursorHandler.State = CursorHandler.CursorState.Default;
            }


            //}
            //catch { }
        }
    }
}