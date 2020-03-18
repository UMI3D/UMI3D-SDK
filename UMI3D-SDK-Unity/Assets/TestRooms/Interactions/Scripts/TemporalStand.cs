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
using UnityEngine;
using umi3d.edk;
using UnityEngine.UI;

public class TemporalStand : MonoBehaviour
{
    public CVEInteractable Interaction1;
    public CVEInteractable Interaction2;
    public AbstractObject3D Interactible;
    [Range(1,30)]
    public float TimeBetweenChange;
    float time;
    bool state;
    public Text text;
    public Text text2;

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        if(time > TimeBetweenChange)
        {
            time = 0;
            if (state) Interactible.objectInteractable.SetValue(Interaction1);
            else Interactible.objectInteractable.SetValue(Interaction2);
            state = !state;
            text.text = "Interaction :" + ((state) ? 2 : 1);
        }
       // text2.text = (TimeBetweenChange - time).ToString();
    }
}
