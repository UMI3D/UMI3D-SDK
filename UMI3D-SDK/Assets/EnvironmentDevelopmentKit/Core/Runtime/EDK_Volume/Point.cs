using System.Collections;
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using umi3d.common.volume;
using UnityEngine;

namespace umi3d.edk.volume.volumedrawing
{
    [RequireComponent(typeof(Collider))]
    public class Point : AbstractMovableObject, IVolumeDescriptor
    {
        public MeshRenderer rnd;

        public string shaderColorPropertyName = "_Albedo";
        private float originalSaturation;
        private float originalValue;
        private float originalAlpha;
        private Color originalColor;

        public override void DisableHighlight()
        {
            rnd.material.SetColor(shaderColorPropertyName, originalColor);
        }

        public override void EnableHighlight()
        {
            originalColor = rnd.material.GetColor(shaderColorPropertyName);
            originalAlpha = originalColor.a;
            Color.RGBToHSV(originalColor, out float H, out originalSaturation, out originalValue);

            Color newColor = Color.HSVToRGB(H, Mathf.Lerp(originalSaturation, 1f, .5f), Mathf.Lerp(originalValue, 0f, .5f));
            newColor.a = originalAlpha;

            rnd.material.SetColor(shaderColorPropertyName, newColor);
        }

        public override void Move(Vector3 translation)
        {
            this.transform.Translate(translation, Space.World);
            base.Move(translation);
        }

        public void Display()
        {
            this.gameObject.SetActive(true);
        }

        public void Hide()
        {
            this.gameObject.SetActive(false);
        }

        /// <summary>
        /// Return load operation
        /// </summary>
        /// <returns></returns>
        public virtual LoadEntity GetLoadEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new LoadEntity()
            {
                entity = this,
                users = new HashSet<UMI3DUser>(users ?? UMI3DEnvironment.GetEntitiesWhere<UMI3DUser>(u => u.hasJoined))
            };
            return operation;
        }


        /// <summary>
        /// Return delete operation
        /// </summary>
        /// <returns></returns>
        public DeleteEntity GetDeleteEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new DeleteEntity()
            {
                entityId = Id(),
                users = new HashSet<UMI3DUser>(users ?? UMI3DEnvironment.GetEntities<UMI3DUser>())
            };
            return operation;
        }

        public IEntity ToEntityDto(UMI3DUser user)
        {
            PointDto dto = new PointDto()
            {
                id = Id(),
                position = this.transform.position
            };

            return dto;
        }

        private string id = "";
        public string Id()
        {
            if (id.Equals(""))
                id = Random.Range(0, 10000000000000000).ToString();
            return id;
        }

        #region filter
        HashSet<UMI3DUserFilter> ConnectionFilters = new HashSet<UMI3DUserFilter>();

        public bool LoadOnConnection(UMI3DUser user)
        {
            return ConnectionFilters.Count == 0 || !ConnectionFilters.Any(f => !f.Accept(user));
        }

        public bool AddConnectionFilter(UMI3DUserFilter filter)
        {
            return ConnectionFilters.Add(filter);
        }

        public bool RemoveConnectionFilter(UMI3DUserFilter filter)
        {
            return ConnectionFilters.Remove(filter);
        }
        #endregion
    }
}