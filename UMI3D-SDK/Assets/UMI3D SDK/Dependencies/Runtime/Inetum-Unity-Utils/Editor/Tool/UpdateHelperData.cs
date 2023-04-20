/*
Copyright 2019 - 2022 Inetum

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
#if UNITY_EDITOR
namespace inetum.unityUtils.editor
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    //[CreateAssetMenu(fileName = "UpdateHelperData", menuName = "Build Helper/Build Helper Data", order = 1)]
    public class UpdateHelperData : ScriptableObject
    {
        //public List<ProjectData> projects = new List<ProjectData>();
        public ProjectLink[] projectsLink;
        [Tooltip("When set to true, The confirmation pop will be validated automaticaly")]
        public bool AutoValidatePopup = false;
        [Tooltip("When set to true, files in the destination folder will be send to the recycle bin")]
        public bool UseSafeMode = true;
    }

    [Serializable]
    public class ProjectData
    {
        public string sdkPath;
        public string projectName;
        public bool isSource;
    }

    [Serializable]
    public class ProjectLink
    {
        public bool expand;
        public ProjectData projectA;
        public ProjectData projectB;
        public List<string> folders;
        public bool sourceIsA;
        public void SetSource(bool sourceIsA)
        {
            this.sourceIsA = sourceIsA;
            projectA.isSource = sourceIsA;
            projectB.isSource = !sourceIsA;
        }

    }
}
#endif