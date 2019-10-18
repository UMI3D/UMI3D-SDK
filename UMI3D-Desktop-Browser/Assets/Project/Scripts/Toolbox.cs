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
using DesktopUI;
using System;
using System.Collections.Generic;
using System.Linq;
using umi3d.cdk;
using umi3d.common;
using UnityEngine;

public class Toolbox : MonoBehaviour
{

    public InteractionListDto dto;
    public InteractionMapper mapper;
    public ToolboxPanel panel;
    public ApplicationBarHeader header;
    private Dictionary<string, List<GameObject>> itemMap = new Dictionary<string, List<GameObject>>();


    public void LoadFromDto()
    {
        ResetInteractions();
        if (dto != null)
            foreach (var block in dto.Interactions)
                AddInteraction(block as AbstractInteractionDto, dto.Id);
    }

    public void RemoveInteraction(string id)
    {
        if (itemMap.ContainsKey(id))
        {
            var gl = itemMap[id].ToArray();
            foreach(var g in gl)
                Destroy(g);
            itemMap.Remove(id);
        }
    }

    public void ResetInteractions()
    {
        panel.Clear();
        itemMap.Clear();
    }

    public void AddInteraction(AbstractInteractionDto item, string parent)
    {
        if (item is ManipulationDto)
            AddManipulation(item as ManipulationDto, parent);
        else if (item is InteractionListDto)
        {
            AddInteractionList(item as InteractionListDto, parent);
        }
        else if (item is DirectInputDto)
            AddDirectInput(item as DirectInputDto, parent);
        else if (item is ActionDto)
            AddAction(item as ActionDto, parent);
        else
            Debug.Log("unsupported dto type !");
        panel.gameObject.GetComponentInParent<ApplicationBar>().ShowTab(panel.GetComponent<ApplicationBarTab>(),false);
    }

    protected void AddAction(ActionDto dto, string parent)
    {
        mapper.RegiterDtoItem(dto, parent);

        PictureItem item = itemMap.ContainsKey(dto.Id) ? itemMap[dto.Id][0].GetComponent<PictureItem>() : null;
        if (item == null)
        {

            item = panel.actions.CreateItem(dto.Name).GetComponent<PictureItem>();

            Action press = () =>
                {
                    if (dto.Inputs == null || dto.Inputs.Count == 0)
                        UMI3DHttpClient.Interact(dto.Id);
                    else
                        OpenForm(dto);
                };

            item.selectBtn.onClick.AddListener(() =>
            {
                press.Invoke();
            });
            var l = new List<GameObject>();
            l.Add(item.gameObject);
            itemMap.Add(dto.Id, l);

            if (dto.Icon != null)
            {
                Resource resource = (Resource)dto.Icon;
                HDResourceCache.Download(resource, (Texture2D t) =>
                {
                    try
                    {
                        var sprite = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2());
                        item.picture.sprite = sprite;
                    }
                    catch (Exception) { }
                });
            }
        }

    }

    protected void AddDirectInput(DirectInputDto dto, string parent)
    {
        mapper.RegiterDtoItem(dto, parent);
        if (dto.Input.InputType == InputType.Checkbox)
            CreateDirectCheckbox(dto, parent);
        else if (dto.Input.InputType == InputType.Decimal)
        {
            if (dto.Input.Max != null && dto.Input.Min != null)
                CreateDirectRange(dto, parent,false);
            else
                CreateDirectDecimal(dto, parent);
        }
        else if (dto.Input.InputType == InputType.Integer)
        {
            if (dto.Input.Max != null && dto.Input.Min != null)
                CreateDirectRange(dto, parent,true);
            else
                CreateDirectInteger(dto, parent);
        }
        else if (dto.Input.InputType == InputType.Select)
            CreateDirectSelect(dto, parent);
        else if (dto.Input.InputType == InputType.Text)
            CreateDirectTextInput(dto, parent);
    }

    private void CreateDirectCheckbox(DirectInputDto dto, string parent)
    {
        var check = panel.settings.CreateCheckbox(dto.Name, false).GetComponent<Checkbox>();
        check.SetValue(bool.Parse(dto.Input.DefaultValue));
        check.SetEvent((bool val) => {
            UMI3DHttpClient.Interact(dto.Id, val);
        });
        var l = new List<GameObject>();
        l.Add(check.gameObject);
        itemMap.Add(dto.Id, l);
    }

    private void CreateDirectRange(DirectInputDto dto, string parent, bool wholeNumber)
    {
        var range = panel.settings.CreateRange(dto.Name).GetComponent<RangeInput>();
        range.SetWholeNumber(wholeNumber);
        range.SetRange(float.Parse(dto.Input.Min), float.Parse(dto.Input.Max));
        range.SetValue(float.Parse(dto.Input.DefaultValue));
        range.SetEvent((float val) =>
        {
            UMI3DWebSocketClient.Interact(dto.Id, val);
        });
        var l = new List<GameObject>();
        l.Add(range.gameObject);
        itemMap.Add(dto.Id, l);
    }

    private void CreateDirectDecimal(DirectInputDto dto, string parent)
    {
        var input = panel.settings.CreateInput(dto.Name).GetComponent<InputFieldItem>();
        input.SetContentType(UnityEngine.UI.InputField.ContentType.DecimalNumber);
        input.SetValue(dto.Input.DefaultValue);
        input.SetEvent((string val) =>
        {
            UMI3DWebSocketClient.Interact(dto.Id, float.Parse(val));
        });
        var l = new List<GameObject>();
        l.Add(input.gameObject);
        itemMap.Add(dto.Id, l);
    }

    private void CreateDirectInteger(DirectInputDto dto, string parent)
    {
        var input = panel.settings.CreateInput(dto.Name).GetComponent<InputFieldItem>();
        input.SetContentType(UnityEngine.UI.InputField.ContentType.IntegerNumber);
        input.SetValue(dto.Input.DefaultValue);
        input.SetEvent((string val) =>
        {
            UMI3DWebSocketClient.Interact(dto.Id, int.Parse(val));
        });
        var l = new List<GameObject>();
        l.Add(input.gameObject);
        itemMap.Add(dto.Id, l);
    }

    private void CreateDirectTextInput(DirectInputDto dto, string parent)
    {
        var input = panel.settings.CreateInput(dto.Name).GetComponent<InputFieldItem>();
        input.SetContentType(UnityEngine.UI.InputField.ContentType.Standard);
        input.SetValue(dto.Input.DefaultValue);
        input.SetEvent((string val) =>
        {
            UMI3DWebSocketClient.Interact(dto.Id, val);
        });
        var l = new List<GameObject>();
        l.Add(input.gameObject);
        itemMap.Add(dto.Id, l);
    }

    private void CreateDirectSelect(DirectInputDto dto, string parent)
    {
        var options = new List<string>();
        foreach (var val in dto.Input.ValuesEnum)
            options.Add(val);

        var input = panel.settings.CreateSelect(dto.Name).GetComponent<SelectInput>();
        input.SetOptions(options);
        input.SetValue(dto.Input.DefaultValue);
        input.SetEvent((string val) =>
        {
            UMI3DHttpClient.Interact(dto.Id, val);
        });
        var l = new List<GameObject>();
        l.Add(input.gameObject);
        itemMap.Add(dto.Id, l);
    }

    protected void AddInteractionList(InteractionListDto dto, string parent)
    {
        mapper.RegiterDtoItem(dto, parent);
        if (dto != null)
            foreach (var block in dto.Interactions)
                AddInteraction(block as AbstractInteractionDto, parent);
    }
    /*
    protected void OpenMenu(ManipulationDto dto, string parent, DofGroupOptionDto option)
    {
        // Clear current manipulation
        if (mapper.currentDof != null)
        {
            ManipulationDto m = new ManipulationDto();
            m.Id = mapper.currentDof;
            mapper.StartManipulation(m, mapper.currentDofGroup);
        }

        // Save dtos of current buttons
        List<ManipulationDto> dtos = new List<ManipulationDto>();
        foreach (GameObject gameObject in itemMap.Values)
        {
            if (gameObject.GetComponent<ManipulationItem>())
                dtos.Add(gameObject.GetComponent<ManipulationItem>().dto);
        }
        panel.manipulations.ClearItems();

        foreach(ManipulationDto d in dtos)
        {
            if (itemMap.ContainsKey(d.Id))
                itemMap.Remove(d.Id);
        }
        

        // Create a cancel button, to get out of the sub-menu
        var cancel = panel.manipulations.CreateItem("Cancel").GetComponent<ManipulationItem>();
        cancel.mapper = mapper;
        cancel.dto = dto;

        Action pressCancel = () => {
            foreach (GameObject gameObject in itemMap.Values)
            {
                if (gameObject.GetComponent<ManipulationItem>())
                    itemMap.Remove(gameObject.GetComponent<ManipulationItem>().dto.Id);
            }
            panel.manipulations.ClearItems();

            if (mapper.currentDof == dto.Id)
                mapper.StartManipulation(dto, mapper.currentDofGroup);

            foreach (ManipulationDto manip in dtos)
            {
                AddManipulation(manip, parent);
            }
        };

        cancel.selectBtn.onClick.AddListener(() =>
        {
            pressCancel.Invoke();
        });

        foreach (DofGroupDto dofGroupDto in option.separations)
        {
            AddManipulation(dto, dofGroupDto, parent);
        }
    }*/

    protected void AddManipulation(ManipulationDto dto, DofGroupDto dofGroup, string parent)
    {
        var item = panel.manipulations.CreateItem(dofGroup.name).GetComponent<ManipulationItem>();
        item.mapper = mapper;
        item.dto = dto;
        item.dofGroup = dofGroup.dofs;
        List<ManipulationDto> dtos = new List<ManipulationDto>();
        Action press = () => {
            var dofs = dofGroup.dofs;

            mapper.StartManipulation(dto, dofs);
        };
        item.selectBtn.onClick.AddListener(() =>
        {
            press.Invoke();
        });

        if (dto.Icon != null)
        {
            Resource resource = (Resource)dto.Icon;
            HDResourceCache.Download(resource, (Texture2D t) =>
            {
                try
                {
                    var sprite = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2());
                    item.picture.sprite = sprite;
                }
                catch (Exception) { }
            });
        }
    }

    protected void AddManipulation(ManipulationDto dto, string parent)
    {
        
        var option = mapper.GetBestOption(dto);
        if (option == null || option.separations.Count == 0)
        {
            Debug.Log("Unable to interpret manipulation !");
            return;
        }
        mapper.RegiterDtoItem(dto, parent);
        //TODO
        /*if (dto.dragAndDropReference != null)
        {
            mapper.AddDragDrop(dto);
        }
        else*/
        if (!itemMap.ContainsKey(dto.Id))
        {

            var l = new List<GameObject>();
            foreach (var separation in option.separations)
            {
                var name = option.separations.Count == 1 ? dto.Name : separation.name;
                if (name != null && name.Length > 0)
                {
                    var item = panel.manipulations.CreateItem(name).GetComponent<ManipulationItem>();
                    item.mapper = mapper;
                    item.dto = dto;
                    item.dofGroup = separation.dofs;

                    Action press = () =>
                    {
                        var dofs = item.dofGroup;
                        mapper.StartManipulation(dto, dofs);
                    };
                    item.selectBtn.onClick.AddListener(() =>
                    {
                        press.Invoke();
                    });
                    l.Add(item.gameObject);

                    if (dto.Icon != null)
                    {
                        Resource resource = (Resource)dto.Icon;
                        HDResourceCache.Download(resource, (Texture2D t) =>
                        {
                            try
                            {
                                var sprite = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2());
                                item.picture.sprite = sprite;
                            }
                            catch (Exception) { }
                        });
                    }

                    if (mapper.currentDof == null)
                        press.Invoke();
                }
            }

            itemMap.Add(dto.Id, l);
        }
    }


    private void OpenForm(ActionDto dto)
    {
        throw new NotImplementedException();
    }

    void UpdateInteraction(AbstractInteractionDto interaction)
    {

        if (interaction == null)
            return;

        if (interaction is ActionDto)
            UpdateAction(interaction as ActionDto);

        else if (interaction is DirectInputDto)
            UpdateDirectInput(interaction as DirectInputDto);

        else if (interaction is InteractionListDto)
            UpdateInteractionList(interaction as InteractionListDto);

        else if (interaction is ManipulationDto)
            UpdateManipulation(interaction as ManipulationDto);
    }

    public void UpdateAction(ActionDto dto)
    {

        var _dto = itemMap[dto.Id][0];
        PictureItem item = _dto == null ? null : _dto.GetComponent<PictureItem>();
        if(item != null)
        {
            item.label.text = dto.Name;
            if (dto.Icon != null)
            {
                Resource resource = (Resource)dto.Icon;
                HDResourceCache.Download(resource, (Texture2D t) =>
                {
                    try
                    {
                        var sprite = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2());
                        item.picture.sprite = sprite;
                    }
                    catch (Exception) { }
                });
            }
        }
    }

    public void UpdateDirectInput(DirectInputDto dto)
    {
        var item = itemMap[dto.Id][0];
        if (dto.Input.InputType == InputType.Checkbox)
            UpdateChecbox(item.GetComponent<Checkbox>(), dto);
        else if (dto.Input.InputType == InputType.Decimal || dto.Input.InputType == InputType.Integer)
        {
            if (dto.Input.Max != null && dto.Input.Min != null)
                UpdateRangeInput(item.GetComponent<RangeInput>(), dto);
            else
                UpdateNumericInput(item.GetComponent<InputFieldItem>(), dto);
        }
        else if (dto.Input.InputType == InputType.Select)
            UpdateSelectInput(item.GetComponent<SelectInput>(), dto);
        else if (dto.Input.InputType == InputType.Text)
            UpdateTextInput(item.GetComponent<InputFieldItem>(), dto);
    }

    private void UpdateChecbox(Checkbox checkbox, DirectInputDto dto)
    {
        checkbox.label.text = dto.Name;
        checkbox.SetValue(bool.Parse(dto.Input.DefaultValue));
    }

    private void UpdateRangeInput(RangeInput range, DirectInputDto dto)
    {
        range.label.text = dto.Name;
        range.SetValue(float.Parse(dto.Input.DefaultValue));
    }

    private void UpdateNumericInput(InputFieldItem input, DirectInputDto dto)
    {
        input.label.text = dto.Name;
        input.SetValue(dto.Input.DefaultValue);
    }

    private void UpdateTextInput(InputFieldItem input, DirectInputDto dto)
    {
        input.label.text = dto.Name;
        input.SetValue(dto.Input.DefaultValue);
    }

    private void UpdateSelectInput(SelectInput dropdown, DirectInputDto dto)
    {
        var value = dto.Input.DefaultValue;
        var options = new List<string>();
        foreach (var val in dto.Input.ValuesEnum)
            options.Add(val);
        dropdown.SetValue(value);
        dropdown.SetOptions(options);
        //dropdown.SetLabel(dto.Name);
    }

    public void UpdateInteractionList(InteractionListDto dto)
    {
        if (dto == this.dto)
            header.label.text = dto.Name;
        var ids = dto.Interactions.Select(c => (c as AbstractInteractionDto).Id).ToList();
        var toremove = new List<string>(itemMap.Keys.Where(id => !ids.Contains(id)));
        var toupdate = new List<string>(itemMap.Keys.Where(id => ids.Contains(id)));
        var toadd = new List<string>(ids.Where(id => !itemMap.Keys.Contains(id)));
        foreach (var i in dto.Interactions) {
            if (toupdate.Contains((i as AbstractInteractionDto).Id))
                UpdateInteraction(i as AbstractInteractionDto);
            else
                AddInteraction(i as AbstractInteractionDto, dto.Id);
        }
        foreach (var id in toremove)
            RemoveInteraction(id);

    }

    public void UpdateManipulation(ManipulationDto dto)
    {
        var _dtos = itemMap[dto.Id];
        foreach (var _dto in _dtos)
        {
            PictureItem item = _dto == null ? null : _dto.GetComponent<PictureItem>();
            if (item != null && _dtos.Count == 1)
            {
                item.label.text = dto.Name;
                if (dto.Icon != null)
                {
                    Resource resource = (Resource)dto.Icon;
                    HDResourceCache.Download(resource, (Texture2D t) =>
                    {
                        try
                        {
                            var sprite = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2());
                            item.picture.sprite = sprite;
                        }
                        catch (Exception) { }
                    });
                }
            }
        }
    }

    private void Update()
    {
        var hasContent = panel.Count > 0;
        header.gameObject.SetActive(hasContent);
        if(!hasContent)
            panel.gameObject.SetActive(false);
        
    }

}

