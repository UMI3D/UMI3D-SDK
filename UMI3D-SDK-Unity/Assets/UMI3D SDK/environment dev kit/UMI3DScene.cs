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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    public class UMI3DScene : MonoBehaviour
    {

        public OSQualitycollection OSQualitycollection;

        /// <summary>
        /// Scene's name.
        /// </summary>
        public string _name = "test";

        /// <summary>
        /// Scene's type.
        /// </summary>
        public EnvironmentType _type = EnvironmentType.ImmersiveVR;

        /// <summary>
        /// Scene's navigation default method.
        /// </summary>
        public NavigationType _navigation = NavigationType.Walk;

        [SerializeField]
        private List<CVEResource> requiredResources = new List<CVEResource>();
        


        #region Dictionaries

        /// <summary>
        /// Contains the objects stored in the scene.
        /// </summary>
        DictionaryGenerator<AbstractObject3D> objectsDictionary = new DictionaryGenerator<AbstractObject3D>("object3d_");
        /// <summary>
        /// Contains the interactions stored in the scene.
        /// </summary>
        DictionaryGenerator<AbstractInteraction> interactionsDictionary = new DictionaryGenerator<AbstractInteraction>("interaction_");
        /// <summary>
        /// Contains the tools stored in the scene.
        /// </summary>
        DictionaryGenerator<CVETool> toolsDictionary = new DictionaryGenerator<CVETool>("tool_");
        /// <summary>
        /// Contains the interactables stored in the scene.
        /// </summary>
        DictionaryGenerator<CVEInteractable> interactablesDictionary = new DictionaryGenerator<CVEInteractable>("interactable_");
        /// <summary>
        /// Contains the toolboxes stored in the scene.
        /// </summary>
        DictionaryGenerator<CVEToolbox> toolboxesDictionary = new DictionaryGenerator<CVEToolbox>("toolbox_");

        /// <summary>
        /// Access to the objects stored in the scene.
        /// </summary>
        public AbstractObject3D[] Objects { get { return objectsDictionary.Values.ToArray(); } }
        /// <summary>
        /// Access to the interactions stored in the scene.
        /// </summary>
        public AbstractInteraction[] Interactions { get { return interactionsDictionary.Values.ToArray(); } }
        /// <summary>
        /// Access to the tools stored in the scene.
        /// </summary>
        public CVETool[] Tools { get { return toolsDictionary.Values.ToArray(); } }
        /// <summary>
        /// Access to the toolboxes stored in the scene.
        /// </summary>
        public CVEToolbox[] Toolboxes { get { return toolboxesDictionary.Values.ToArray(); } }

        /// <summary>
        /// Get scene object by id.
        /// </summary>
        /// <param name="id">Object to get id</param>
        public AbstractObject3D GetObject(string id) { return objectsDictionary[id]; }
        /// <summary>
        /// Get scene interaction by id.
        /// </summary>
        /// <param name="id">Object to get id</param>
        public AbstractInteraction GetInteraction(string id) { return interactionsDictionary[id]; }
        /// <summary>
        /// Get scene tool by id.
        /// </summary>
        /// <param name="id">Object to get id</param>
        public CVETool GetTool(string id) { return toolsDictionary[id]; }
        /// <summary>
        /// Get scene toolbox by id.
        /// </summary>
        /// <param name="id">Object to get id</param>
        public CVEToolbox GetToolbox(string id) { return toolboxesDictionary[id]; }

        /// <summary>
        /// Register an object to the scene, and return it's id. 
        /// Supported Types: AbstractObject3D, GenericInteraction, Tool, Toolbox
        /// </summary>
        /// <param name="obj">Object to register</param>
        /// <returns>Registered object's id.</returns>
        public string Register(object obj)
        {
            if (obj is AbstractObject3D)
                return objectsDictionary.Register(obj as AbstractObject3D);
            else if (obj is AbstractInteraction)
                return interactionsDictionary.Register(obj as AbstractInteraction);
            else if (obj is CVETool)
                return toolsDictionary.Register(obj as CVETool);
            else if (obj is CVEInteractable)
                return interactablesDictionary.Register(obj as CVEInteractable);
            else if (obj is CVEToolbox)
                return toolboxesDictionary.Register(obj as CVEToolbox);
            else
                throw new System.ArgumentException("Unsupported type [" + obj.GetType() + "]", "obj");
        }
        
        /// <summary>
        /// Remove an object from the scene. 
        /// Supported Types: AbstractObject3D, GenericInteraction, Tool, Toolbox
        /// </summary>
        /// <param name="obj">Object to remove</param>
        public void Remove(object obj)
        {
            if (obj is AbstractObject3D)
                objectsDictionary.Remove((obj as AbstractObject3D).Id);
            else if (obj is AbstractInteraction)
                interactionsDictionary.Remove((obj as AbstractInteraction).Id);
            else if (obj is CVETool)
                toolsDictionary.Remove((obj as CVETool).Id);
            else if (obj is CVEInteractable)
                interactablesDictionary.Remove((obj as CVEInteractable).Id);
            else if (obj is CVEToolbox)
                toolboxesDictionary.Remove((obj as CVEToolbox).Id);
            else
                throw new System.ArgumentException("Unsupported type [" + obj.GetType() + "]", "obj");
        }


        public class DictionaryGenerator<A>
        {
            private int seq = 0;

            private string prefix;

            /// <summary>
            /// Contains the  stored objects.
            /// </summary>
            Dictionary<string, A> objects = new Dictionary<string, A>();

            public Dictionary<string, A>.ValueCollection Values { get { return objects.Values; } }

            public DictionaryGenerator(string prefix)
            {
                this.prefix = prefix;
            }

            public A this[string key]
            {
                get
                {
                    if (key == null || key.Length == 0)
                        return default;
                    else if (objects.ContainsKey(key))
                        return objects[key];
                    else return default;
                }
            }

            public string Register(A obj)
            {
                var key = prefix + seq;
                seq++;
                objects.Add(key, obj);
                return key;
            }

            public void Remove(string key)
            {
                objects.Remove(key);
            }

        }
        
        #endregion

        #region AsyncProperties
        /// <summary>
        /// Type of Ambient light.
        /// </summary>
        public AmbientType AmbientType;
        /// <summary>
        /// Color of Ambient light.
        /// </summary>
        [ColorUsage(false, true)]
        public Color AmbientColor = Color.white;
        [ColorUsage(false, true)]
        public Color SkyColor = Color.white;
        [ColorUsage(false, true)]
        public Color HorizonColor = Color.white;
        [ColorUsage(false, true)]
        public Color GroundColor = Color.white;
        /// <summary>
        /// Intensity of ambient light.
        /// </summary>
        [Range(0,5)]
        public float AmbientIntensity = 1;

        ///// <summary>
        ///// Resource Image for Skybox;
        ///// </summary>
        //public CVEResource SkyboxImage;

        /// <summary>
        /// Are the AsyncProperties inited.
        /// </summary>
        bool inited = false;
        /// <summary>
        /// Class that will manage the AsyncProperties.
        /// </summary>
        public AsyncPropertiesHandler PropertiesHandler { protected set; get; }
        /// <summary>
        /// AsyncProperties of the ambient Type.
        /// </summary>
        public UMI3DAsyncProperty<AmbientType> AmbientTypeProperty;
        /// <summary>
        /// AsyncProperties of the ambient color.
        /// </summary>
        public UMI3DAsyncProperty<Color> AmbientColorProperty;
        /// <summary>
        /// AsyncProperties of the ambient color.
        /// </summary>
        public UMI3DAsyncProperty<Color> SkyColorProperty;
        /// <summary>
        /// AsyncProperties of the ambient color.
        /// </summary>
        public UMI3DAsyncProperty<Color> HorizonColorProperty;
        /// <summary>
        /// AsyncProperties of the ambient color.
        /// </summary>
        public UMI3DAsyncProperty<Color> GroundColorProperty;
        /// <summary>
        /// AsyncProperties of the ambient Intensity.
        /// </summary>
        public UMI3DAsyncProperty<float> AmbientIntensityProperty;

        public UMI3DAsyncProperty<CVEResource> AmbientSkyboxImage;

        /// <summary>
        /// Init the AsyncProperties.
        /// </summary>
        void initDefinition()
        {
            PropertiesHandler = new AsyncPropertiesHandler();
            PropertiesHandler.DelegateBroadcastUpdate += BroadcastUpdates;
            PropertiesHandler.DelegatebroadcastUpdateForUser += BroadcastUpdates;

            AmbientColorProperty = new UMI3DAsyncProperty<Color>(PropertiesHandler, AmbientColor);
            AmbientColorProperty.OnValueChanged += (Color value) => AmbientColor = value;

            SkyColorProperty = new UMI3DAsyncProperty<Color>(PropertiesHandler, SkyColor);
            SkyColorProperty.OnValueChanged += (Color value) => SkyColor = value;

            HorizonColorProperty = new UMI3DAsyncProperty<Color>(PropertiesHandler, HorizonColor);
            HorizonColorProperty.OnValueChanged += (Color value) => HorizonColor = value;

            GroundColorProperty = new UMI3DAsyncProperty<Color>(PropertiesHandler, GroundColor);
            GroundColorProperty.OnValueChanged += (Color value) => GroundColor = value;

            AmbientIntensityProperty = new UMI3DAsyncProperty<float>(PropertiesHandler, AmbientIntensity);
            AmbientIntensityProperty.OnValueChanged += (float value) => AmbientIntensity = value;

            AmbientTypeProperty = new UMI3DAsyncProperty<AmbientType>(PropertiesHandler, AmbientType);
            AmbientTypeProperty.OnValueChanged += (AmbientType value) => AmbientType = value;

            //AmbientSkyboxImage = new UMI3DAsyncProperty<CVEResource>(PropertiesHandler, SkyboxImage);
            //AmbientSkyboxImage.OnValueChanged += (CVEResource value) => SkyboxImage = value;

            inited = true;
        }

        /// <summary>
        /// Syncronise AsyncProperties and set RendererSettings
        /// </summary>
        public void SyncProperties()
        {
            if (inited)
            {
                AmbientColorProperty.SetValue(AmbientColor);
                SkyColorProperty.SetValue(SkyColor);
                HorizonColorProperty.SetValue(HorizonColor);
                GroundColorProperty.SetValue(GroundColor);
                AmbientIntensityProperty.SetValue(AmbientIntensity);
                AmbientTypeProperty.SetValue(AmbientType);
                //AmbientSkyboxImage.SetValue(SkyboxImage);
            }
            RenderSettings.ambientMode = AmbientType.Convert();
            switch (AmbientType)
            {
                case AmbientType.Skybox:
                    RenderSettings.ambientSkyColor = AmbientColor;
                    RenderSettings.ambientIntensity = AmbientIntensity;
                    break;
                case AmbientType.Flat:
                    RenderSettings.ambientLight = AmbientColor;
                    break;
                case AmbientType.Gradient:
                    RenderSettings.ambientSkyColor = SkyColor;
                    RenderSettings.ambientEquatorColor = HorizonColor;
                    RenderSettings.ambientGroundColor = GroundColor;
                    break;
            }

        }

        /// <summary>
        /// Send updates to concerned users.
        /// </summary>
        private void BroadcastUpdates()
        {
            foreach (UMI3DUser user in UMI3D.UserManager.GetUsers())
                PropertiesHandler.BroadcastUpdates(user);
        }

        /// <summary>
        /// Send updates to concerned users.
        /// </summary>
        /// <param name="user">the user</param>
        private void BroadcastUpdates(UMI3DUser user)
        {
            user.Send(ToUpdateDto(user));
        }


        #endregion

        /// <summary>
        /// Scene's preview icon.
        /// </summary>
        [SerializeField]
        protected CVEResource icon = new CVEResource()
        {
            IsLocalFile = true,
            Path = "/picture.png"
        };

        /// <summary>
        /// Scene's preview icon 3D.
        /// </summary>
        [SerializeField]
        protected CVEResource icon3D = new CVEResource()
        {
            IsLocalFile = true
        };

        /// <summary>
        /// Scene's preview icon.
        /// </summary>
        [SerializeField]
        protected CVEResource skybox = new CVEResource()
        {
            IsLocalFile = true
        };

        /// <summary>
        /// List of extension that could be send by the environement in ResourceDto
        /// </summary>
        public List<string> extensionNeeded;

        /// <summary>
        /// Get scene's information required for client connection.
        /// </summary>
        public MediaDto ToDto()
        {
            var res = new MediaDto();
            res.Url = UMI3D.Server.GetEnvironmentUrl();
            res.Name = _name;
            res.EnvironmentType = _type;
            res.NavigationType = _navigation;
            res.Connection = UMI3D.Server.ToDto();
            res.Icon = icon.ToDto();
            res.Icon3D = icon3D.ToDto();
            res.Skybox = skybox.ToDto();
            res.extensionNeeded = extensionNeeded;
            res.requiredResources = requiredResources.ConvertAll<ResourceDto>(cver => cver.ToDto());

            res.VersionMajor = UMI3DVersion.major;
            res.VersionMinor = UMI3DVersion.minor;
            res.VersionStatus = UMI3DVersion.status;
            res.VersionDate = UMI3DVersion.date;

            return res;
        }

        /// <summary>
        /// Get scene's updatable information.
        /// </summary>
        public MediaUpdateDto ToUpdateDto(UMI3DUser user)
        {
            var res = new MediaUpdateDto();
            res.AmbientColor = AmbientColorProperty.GetValue(user);
            res.Intensity = AmbientIntensityProperty.GetValue(user);
            res.Type = AmbientTypeProperty.GetValue(user);
            res.SkyColor = SkyColorProperty.GetValue(user);
            res.HorizonColor = HorizonColorProperty.GetValue(user);
            res.GroundColor = GroundColorProperty.GetValue(user);
            //res.SkyboxImage = AmbientSkyboxImage.GetValue(user).ToDto();
            return res;
        }


        /// <summary>
        /// Get a list of children of a given object for a given user.
        /// </summary>
        /// <param name="pid">Parent object id to get children of</param>
        /// <param name="user">User to get children for</param>
        /// <returns></returns>
        public List<AbstractObject3D> GetChildren(string pid, UMI3DUser user)
        {
            var res = new List<AbstractObject3D>();
            if (pid == null || pid.Length == 0)
            {
                foreach (Transform ct in transform)
                {
                    var child = ct.gameObject.GetComponent<AbstractObject3D>();
                    if (child != null && ct.gameObject.activeInHierarchy)
                        res.Add(child);
                }
            }
            else
            {
                var parent = GetObject(pid);
                if (parent != null)
                {
                    res = parent.GetChildren(user);
                }
            }

            return res;
        }






        /// <summary>
        /// Get scene origin.
        /// </summary>
        public Transform Origin
        {
            get
            {
                return this.transform;
            }
            set { }
        }

        /// <summary>
        /// Unity MonoBehaviour Awake method.
        /// </summary>
        protected virtual void Awake()
        {
            initDefinition();
        }

        protected virtual void Start()
        {
            UMI3D.OnUserCreate.AddListener((user) => { StartCoroutine( FirstUserUpdate(1,user)); });
        }

        IEnumerator FirstUserUpdate(float t,UMI3DUser user)
        {
            yield return new WaitForSeconds(t);
            PropertiesHandler.NotifyUpdate(user);
        }


        private float clock = 0;
        private void Update()
        {
            if (clock > 1f / UMI3D.TargetFrameRate)
            {
                PropertiesHandler.BroadcastUpdates();
                clock = 0;
            }
            clock += Time.deltaTime;
        }

        /// <summary>
        /// Unity MonoBehaviour OnValidate method.
        /// </summary>
        protected virtual void OnValidate()
        {
            SyncProperties();
        }
    }
}
