using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DesktopUI;
using umi3d.cdk;
using umi3d.common;

public class InteractionMapper : AbstractPlayer
{
    //toolboxes
    public Toolbox mainBox;
    public GameObject toolBoxHeaderPrefab;
    public GameObject toolBoxPanelPrefab;
    public Transform toolBoxHeaders;
    public Transform toolBoxPanels;
    List<Toolbox> toolboxes = new List<Toolbox>();
    Dictionary<string, Toolbox> map = new Dictionary<string, Toolbox>();
    Dictionary<string, AbstractInteractionDto> IntMap = new Dictionary<string, AbstractInteractionDto>();

    public override IEnumerable<string> InteractionIds() {
        return new List<string>(IntMap.Keys);
    }

    //direct interactions
    private List<HoverDto> hoverList = new List<HoverDto>();
    private List<PickDto> pickList = new List<PickDto>();

    //manipulation
    public EventSystem eventsystem;
    public float degreePerMeter = 50f;
    public Action<Vector2, Vector3> On2DMoved;
    public string currentDof = null;
    public DofGroupEnum currentDofGroup;


    Toolbox GetToolbox(AbstractInteractionDto item, string parent = null) {
        if (map.ContainsKey(item.Id))
            return map[item.Id];
        else if (parent != null && map.ContainsKey(parent))
            return map[parent];
        else
            return mainBox;
    }

    public void RegiterDtoItem(AbstractInteractionDto dto, string parent = null)
    {
        if (!IntMap.ContainsKey(dto.Id))
        {   //Register Interaction for future updates
            IntMap.Add(dto.Id, dto);
            if (!map.ContainsKey(dto.Id))
                map.Add(dto.Id, GetToolbox(dto, parent));
        }
    }

    public override void RemoveInteraction(string id)
    {
        if (!IntMap.ContainsKey(id))
            return;
        AbstractInteractionDto item = IntMap[id];
        if (item is PickDto)
            RemovePick(item as PickDto);
        else if (item is HoverDto)
            RemoveHover(item as HoverDto);
        else if (item is ManipulationDto)
        {
            if (item.Id == currentDof)
            {
                ClearDof();
            }
            GetToolbox(item).RemoveInteraction(id);
        }
        else if (item is InteractionListDto)
        {
            var list = item as InteractionListDto;
            var box = GetToolbox(item);
            if (list == box.dto)
                RemoveToolbox(item as InteractionListDto);
            else
                GetToolbox(item).RemoveInteraction(id);
        }
        else
            GetToolbox(item).RemoveInteraction(id);
        IntMap.Remove(id);
        map.Remove(id);
    }

    public override void ResetInteractions()
    {
        foreach (var tools in toolboxes.ToArray())
            RemoveToolbox(tools.dto);
        mainBox.ResetInteractions();
        IntMap.Clear();
        toolboxes.Clear();
        map.Clear();
        hoverList.Clear();
        pickList.Clear();
        if (UMI3DBrowser.Scene != null)
        {
            foreach (var hover in UMI3DBrowser.Scene.GetComponentsInChildren<HoverListener>(true))
                Destroy(hover);
            foreach (var pick in UMI3DBrowser.Scene.GetComponentsInChildren<PickListener>(true))
                Destroy(pick);
            foreach (var dd in UMI3DBrowser.Scene.GetComponentsInChildren<DrapDropListener>(true))
                Destroy(dd);
        }
    }

    protected override void AddAction(ActionDto dto)
    {
        mainBox.AddInteraction(dto, null);
    }

    protected override void AddDirectInput(DirectInputDto dto)
    {
        mainBox.AddInteraction(dto, null);
    }

    protected override void AddHover(HoverDto dto)
    {
        if (dto == null || dto.targetID == null)
            return;
        if (!IntMap.ContainsKey(dto.Id))
        {
            IntMap.Add(dto.Id, dto);
        }
        var target = UMI3DBrowser.Scene.GetObject(dto.targetID);
        if (target == null)
            hoverList.Add(dto);
        else
        {
            HoverListener h = target.AddComponent<HoverListener>();
            h.onHover += (bool val) =>
            {
                UMI3DWebSocketClient.Interact(dto.Id, val);                
            };

            if (dto.shouldTrackPosition)
            {
                h.onHovered += (Vector3 pos, Vector3 normal) =>
                {
                    HoveredDto arg = new HoveredDto();
                    arg.State = true;
                    arg.Position = pos;
                    arg.Normal = normal;
                    UMI3DWebSocketClient.Interact(dto.Id, arg);
                };
            }
        }
    }

    protected override void AddInteractionList(InteractionListDto dto)
    {
        if (IntMap.ContainsKey(dto.Id))
            return;
        IntMap.Add(dto.Id, dto);

        var box = gameObject.AddComponent<Toolbox>();
        var header = Instantiate(toolBoxHeaderPrefab);
        var panel = Instantiate(toolBoxPanelPrefab);
        header.name = "["+ dto.Name + "]";
        panel.name = "[" + dto.Name + "]";
        header.transform.SetParent(toolBoxHeaders, false);
        panel.transform.SetParent(toolBoxPanels, false);
        box.dto = dto;
        box.header = header.GetComponent<ApplicationBarHeader>();
        box.panel = panel.GetComponent<ToolboxPanel>();
        box.header.tab = box.panel.GetComponent<ApplicationBarTab>();
        box.header.label.text = dto.Name;

        if (!IntMap.ContainsKey(dto.Id))
        {
            IntMap.Add(dto.Id, dto);
        }
        toolboxes.Add(box);
        map.Add(dto.Id, box);
        box.mapper = this;
        box.LoadFromDto();
    }

    protected override void AddManipulation(ManipulationDto dto)
    {
            mainBox.AddInteraction(dto, null);
    }

    protected override void AddPick(PickDto dto)
    {
        if (dto == null || dto.Target == null)
            return;
        if (!IntMap.ContainsKey(dto.Id))
        {
            IntMap.Add(dto.Id, dto);
        }
        var target = UMI3DBrowser.Scene.GetObject(dto.Target);
        if (target == null)
            pickList.Add(dto);
        else
        {
            target.AddComponent<PickListener>().onPick += (Vector3 val) =>
            {
                object args = null;
                if (UMI3DBrowser.Scene != null)
                {
                    var impact = UMI3DBrowser.Scene.transform.worldToLocalMatrix.MultiplyVector(val);
                    args = (SerializableVector3)impact;
                }
                UMI3DHttpClient.Interact(dto.Id, args);
            };
        }
    }

    protected override void UpdateAction(ActionDto dto)
    {
        if (IntMap.ContainsKey(dto.Id))
        {
            var _dto = IntMap[dto.Id] as ActionDto;
            _dto.Name = dto.Name;
            _dto.Icon = dto.Icon;
            _dto.Inputs = dto.Inputs;
            GetToolbox(_dto).UpdateAction(_dto);
        }
    }

    protected override void UpdateDirectInput(DirectInputDto dto)
    {
        if (IntMap.ContainsKey(dto.Id))
        {
            var _dto = IntMap[dto.Id] as DirectInputDto;
            _dto.Name = dto.Name;
            _dto.Icon = dto.Icon;
            _dto.Input.DefaultValue = dto.Input.DefaultValue;
            GetToolbox(_dto).UpdateDirectInput(_dto);
        }
    }

    protected override void UpdateHover(HoverDto dto)
    {
        //Not updateable !
    }

    protected override void UpdateInteractionList(InteractionListDto dto)
    {
        if (IntMap.ContainsKey(dto.Id))
        {
            var _dto = IntMap[dto.Id] as InteractionListDto;
            _dto.Name = dto.Name;
            _dto.Icon = dto.Icon;
            _dto.Interactions = dto.Interactions;
            GetToolbox(_dto).UpdateInteractionList(_dto);
        }
        GetToolbox(dto).UpdateInteractionList(dto);
    }

    protected override void UpdateManipulation(ManipulationDto dto)
    {
        if (IntMap.ContainsKey(dto.Id))
        {
            var _dto = IntMap[dto.Id] as ManipulationDto;
            _dto.Name = dto.Name;
            _dto.Icon = dto.Icon;
            GetToolbox(dto).UpdateManipulation(_dto);
        }
    }

    protected override void UpdatePick(PickDto dto)
    {
        //Not updateable !
    }




    private void RemovePick(PickDto dto)
    {
        if (IntMap.ContainsKey(dto.Id))
        {
            var _dto = IntMap[dto.Id] as PickDto;
            var target = UMI3DBrowser.Scene.GetObject(dto.Target);
            if (target != null)
            {
                var h = target.GetComponent<PickListener>();
                if (h != null)
                    Destroy(h);
            }
            if (pickList.Contains(_dto))
                pickList.Remove(_dto);
            IntMap.Remove(dto.Id);
        }
    }

    private void RemoveHover(HoverDto dto)
    {
        if (IntMap.ContainsKey(dto.Id))
        {
            var _dto = IntMap[dto.Id] as HoverDto;
            var target = UMI3DBrowser.Scene.GetObject(dto.targetID);
            if (target != null)
            {
                var h = target.GetComponent<HoverListener>();
                if (h != null)
                    Destroy(h);
            }
            if (hoverList.Contains(_dto))
                hoverList.Remove(_dto);
            IntMap.Remove(dto.Id);
        }
    }

    private void RemoveToolbox(InteractionListDto dto)
    {
        if (!IntMap.ContainsKey(dto.Id))
            return;
        foreach (var i in dto.Interactions)
            RemoveInteraction((i as AbstractInteractionDto).Id);
        var box = GetToolbox(dto);
        if(box.dto == dto)
        {
            box.ResetInteractions();
            Destroy(box.header.gameObject);
            Destroy(box.panel.gameObject);
            map.Remove(dto.Id);
            toolboxes.Remove(box);
            Destroy(box);
        }
        IntMap.Remove(dto.Id);
    }


    protected Transform GetFrameOfReference(ManipulationDto dto)
    {
        Transform frameOfReference = null;
        if (dto.frameOfReference == null || dto.frameOfReference.Length == 0)
            frameOfReference = Camera.main.transform;
        else
        {
            var _p = UMI3DBrowser.Scene.GetObject(dto.frameOfReference);
            if (_p != null)
                frameOfReference = _p.transform;
        }
        return frameOfReference;
    }

    private bool IsValidOption(DofGroupOptionDto option)
    {
        DofGroupEnum[] forbidden = {
            DofGroupEnum.ALL ,
            DofGroupEnum.RX_RY_RZ,
            DofGroupEnum.X_RX,
            DofGroupEnum.Y_RY,
            DofGroupEnum.Z_RZ,
            DofGroupEnum.XYZ
        };
        foreach (var sep in option.separations)
            if (forbidden.Contains(sep.dofs))
                return false;
        return true;
    }

    public DofGroupOptionDto GetBestOption(ManipulationDto dto)
    {
        foreach (var opt in dto.dofSeparationOptions)
            if (IsValidOption(opt))
                return opt;
        return null;
    }

    private void ClearDof()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        On2DMoved = null;
        currentDof = null;
    }

    public void StartManipulation(ManipulationDto dto, DofGroupEnum dofs)
    {
        if (currentDof == dto.Id && currentDofGroup == dofs)
        {
            ClearDof();
            return;
        }
        ClearDof();
        currentDof = dto.Id;
        currentDofGroup = dofs;
        switch (dofs)
        {
            case DofGroupEnum.X:
                StartTranslation(dto, new Vector3(1, 0, 0));
                break;
            case DofGroupEnum.Y:
                StartTranslation(dto, new Vector3(0, 1, 0));
                break;
            case DofGroupEnum.Z:
                StartTranslation(dto, new Vector3(0, 0, 1));
                break;
            case DofGroupEnum.XY:
                StartTranslation(dto, new Vector3(1, 0, 0), new Vector3(0, 1, 0));
                break;
            case DofGroupEnum.XZ:
                StartTranslation(dto, new Vector3(1, 0, 0), new Vector3(0, 0, 1));
                break;
            case DofGroupEnum.YZ:
                StartTranslation(dto, new Vector3(0, 1, 0), new Vector3(0, 0, 1));
                break;
            case DofGroupEnum.RX:
                StartRotation(dto, new Vector3(1, 0, 0));
                break;
            case DofGroupEnum.RY:
                StartRotation(dto, new Vector3(0, 1, 0));
                break;
            case DofGroupEnum.RZ:
                StartRotation(dto, new Vector3(0, 0, 1));
                break;
            case DofGroupEnum.RX_RY:
                StartRotation(dto, new Vector3(1, 0, 0), new Vector3(0, 1, 0));
                break;
            case DofGroupEnum.RX_RZ:
                StartRotation(dto, new Vector3(1, 0, 0), new Vector3(0, 0, 1));
                break;
            case DofGroupEnum.RY_RZ:
                StartRotation(dto, new Vector3(0, 1, 0), new Vector3(0, 0, 1));
                break;
            case DofGroupEnum.XYZ:
                StartTranslation(dto, new Vector3(1, 0, 0), new Vector3(0,1,0), new Vector3(0, 0, 1));
                break;
        }
    }

    protected void StartTranslation(ManipulationDto dto, Vector3 axis)
    {
        Action<Vector2, Vector3> action = (Vector2 move, Vector3 distance) => {
            float dt = Time.deltaTime;
            var dx = dt * distance.x;
            var dy = dt * distance.y;
            var parent = GetFrameOfReference(dto);
            var args = new ManipulationParametersDto();
            if (parent == null || parent == Camera.main.transform)
            {
                args.Translation = Vector3.forward * dy;
            }
            else
            {
                var v = parent.localToWorldMatrix * axis;
                v.Normalize();
                var Dx = dx * Vector3.Dot(Camera.main.transform.right, v);
                var Dy = dy * Vector3.Dot(Camera.main.transform.up, v);
                var D = Dx + Dy;
                args.Translation = D * axis;
            }
            UMI3DWebSocketClient.Interact(dto.Id, args);
        };
        On2DMoved += action;
    }

    protected void StartTranslation(ManipulationDto dto, Vector3 axis1, Vector3 axis2)
    {
        var cam = Camera.main.gameObject.transform;
        Action<Vector2, Vector3> action = (Vector2 move, Vector3 distance) => {
            float dt = Time.deltaTime;
            var dx = dt * distance.x;
            var dy = dt * distance.y;

            var parent = GetFrameOfReference(dto);
            var args = new ManipulationParametersDto();
            if (parent == null || parent == Camera.main.transform)
            {
                args.Translation = Vector3.forward * dy + Vector3.right * dx;
            } else {

                var v1 = parent.localToWorldMatrix * axis1;
                v1.Normalize();
                var v2 = parent.localToWorldMatrix * axis2;
                v2.Normalize();

                var p1 = Vector3.Dot(cam.up, v1);
                var p2 = Vector3.Dot(cam.up, v2);
                var p3 = Vector3.Dot(cam.right, v1);
                var p4 = Vector3.Dot(cam.right, v2);
                var c1 = Math.Abs(p1);
                var c2 = Math.Abs(p2);
                var c3 = Math.Abs(p3);
                var c4 = Math.Abs(p4);
                var s1 = c1 == 0 ? 1 : p1 / c1;
                var s2 = c2 == 0 ? 1 : p2 / c2;
                var s3 = c3 == 0 ? 1 : p3 / c3;
                var s4 = c4 == 0 ? 1 : p4 / c4;
                var D1 = (c1 > c2) ? s1 * dy : s3 * dx;
                var D2 = (c1 > c2) ? s4 * dx : s2 * dy;

                args.Translation = D1 * axis1 + D2 * axis2;
            }
            UMI3DWebSocketClient.Interact(dto.Id, args);
        };

        On2DMoved += action;
    }

    protected void StartTranslation(ManipulationDto dto, Vector3 axis1, Vector3 axis2, Vector3 axis3)
    {
        var cam = Camera.main.gameObject.transform;
        Action<Vector2, Vector3> action = (Vector2 move, Vector3 distance) => {
            float dt = Time.deltaTime;
            var dx = dt * distance.x;
            var dy = dt * distance.y;
            var dz = dt * distance.z;

            var parent = GetFrameOfReference(dto);
            var args = new ManipulationParametersDto();
            if (parent == null || parent == Camera.main.transform)
            {
                args.Translation = Vector3.forward * dy + Vector3.right * dx + Vector3.up * dz;
            }
            else
            {

                var v1 = parent.localToWorldMatrix * axis1;
                v1.Normalize();
                var v2 = parent.localToWorldMatrix * axis2;
                v2.Normalize();

                var p1 = Vector3.Dot(cam.up, v1);
                var p2 = Vector3.Dot(cam.up, v2);
                var p3 = Vector3.Dot(cam.right, v1);
                var p4 = Vector3.Dot(cam.right, v2);
                var c1 = Math.Abs(p1);
                var c2 = Math.Abs(p2);
                var c3 = Math.Abs(p3);
                var c4 = Math.Abs(p4);
                var s1 = c1 == 0 ? 1 : p1 / c1;
                var s2 = c2 == 0 ? 1 : p2 / c2;
                var s3 = c3 == 0 ? 1 : p3 / c3;
                var s4 = c4 == 0 ? 1 : p4 / c4;
                var D1 = (c1 > c2) ? s1 * dy : s3 * dx;
                var D2 = (c1 > c2) ? s4 * dx : s2 * dy;

                args.Translation = D1 * axis1 - D2 * axis3 + dz * axis2;
            }
            UMI3DWebSocketClient.Interact(dto.Id, args);
        };
        On2DMoved += action;

    }

    protected void StartRotation(ManipulationDto dto, Vector3 axis)
    {
        Action<Vector2, Vector3> action = (Vector2 move, Vector3 distance) =>
        {
            float dt = Time.deltaTime;
            var dx = dt * distance.x;
            var dy = dt * distance.y;
            var parent = GetFrameOfReference(dto);
            if (parent == null)
                return;
            var v = parent.localToWorldMatrix * axis;
            v.Normalize();
            var Dx = dx * Vector3.Cross(Camera.main.transform.right, v).magnitude;
            var Dy = dy * Vector3.Cross(Camera.main.transform.up, v).magnitude;
            var D = degreePerMeter * (Dx + Dy);
            var args = new ManipulationParametersDto();
            args.Rotation = Quaternion.AngleAxis(D, axis);

            UMI3DWebSocketClient.Interact(dto.Id, args);
        };
        On2DMoved += action;
    }

    protected void StartRotation(ManipulationDto dto, Vector3 axis1, Vector3 axis2)
    {
        var cam = Camera.main.gameObject.transform;

        Action<Vector2, Vector3> action = (Vector2 move, Vector3 distance) => {
            float dt = Time.deltaTime;
            var dx = dt * distance.x;
            var dy = dt * distance.y;

            var parent = GetFrameOfReference(dto);
            if (parent == null)
                return;

            var v1 = parent.localToWorldMatrix * axis1;
            v1.Normalize();
            var v2 = parent.localToWorldMatrix * axis2;
            v2.Normalize();

            var p1 = Vector3.Dot(cam.up, v1);
            var p2 = Vector3.Dot(cam.up, v2);
            var p3 = Vector3.Dot(cam.right, v1);
            var p4 = Vector3.Dot(cam.right, v2);
            var c1 = Math.Abs(p1);
            var c2 = Math.Abs(p2);
            var c3 = Math.Abs(p3);
            var c4 = Math.Abs(p4);
            var s1 = c1 == 0 ? 1 : p1 / c1;
            var s2 = c2 == 0 ? 1 : p2 / c2;
            var s3 = c3 == 0 ? 1 : p3 / c3;
            var s4 = c4 == 0 ? 1 : p4 / c4;
            var D1 = (c1 > c2) ? s1 * dy : s3 * dx;
            var D2 = (c1 > c2) ? s4 * dx : s2 * dy;

            var quatA = Quaternion.AngleAxis(degreePerMeter * D2, axis1);
            var quatB = Quaternion.AngleAxis(degreePerMeter * D1, axis2);
            var quatC = quatA * quatB;

            var args = new ManipulationParametersDto();
            args.Rotation = quatC;
            UMI3DWebSocketClient.Interact(dto.Id, args);
        };

        On2DMoved += action;
    }

    public void On2DPick(Vector3 screenPosition)
    {
        //if (!eventsystem.IsPointerOverGameObject())
        //    return;
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit[] hits = umi3d.Physics.RaycastAll(ray,  100);
        foreach(RaycastHit hit in hits)
        {
            //Debug.Log(hit.collider.gameObject.name);
            var picked = hit.collider.gameObject.GetComponent<PickListener>();
            if (hit.collider.gameObject.GetComponentInParent<HDScene>() == null)
                return;
            if (picked == null)
                picked = hit.collider.gameObject.GetComponentInParent<PickListener>();
            if (picked != null)
            {
                picked.OnPick(hit.point);
                break;
            }
        }
    }

    public void OnDragStart(Vector3 screenPosition)
    {
        if (eventsystem.IsPointerOverGameObject())
            return;
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100))
        {
            var dd = hit.collider.gameObject.GetComponent<DrapDropListener>();
            if (dd == null)
                dd = hit.collider.gameObject.GetComponentInParent<DrapDropListener>();
            if (dd != null)
            {
                dd.OnDragStart();
            }
        }
    }

    public void OnDragStop()
    {
        if (currentDof != null && IntMap.ContainsKey(currentDof))
        {
            var m = IntMap[currentDof] as ManipulationDto;
        }
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        checkHoverStack();
        checkPickStack();
    }

    void checkHoverStack()
    {
        if (hoverList.Count == 0)
            return;
        var stack = new List<HoverDto>();
        stack.AddRange(hoverList);
        hoverList.Clear();
        foreach (var dto in stack)
            AddHover(dto);
    }

    void checkPickStack()
    {
        if (pickList.Count == 0)
            return;
        var stack = new List<PickDto>();
        stack.AddRange(pickList);
        pickList.Clear();
        foreach (var dto in stack)
            AddPick(dto);
    }


}
