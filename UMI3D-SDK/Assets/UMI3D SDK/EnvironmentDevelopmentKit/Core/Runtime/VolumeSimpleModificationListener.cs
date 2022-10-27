using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace umi3d.edk.volume
{
    /// <summary>
    /// Obsolete quick simple class to handle volume transactions. Should not be used in production.
    /// </summary>
    [System.Obsolete("This class isn't mean to be use in production", false)]
    public class VolumeSimpleModificationListener : MonoBehaviour
    {
        public float maxFrameRate = 30;

        private List<UMI3DAsyncProperty> modifiedProperties = new List<UMI3DAsyncProperty>();

        protected virtual void Start()
        {
            var boxes = FindObjectsOfType<Box>().ToList();
            foreach (Box box in boxes)
            {
                SubscribeProperty<Vector3>(box.center);
                SubscribeProperty<Vector3>(box.size);
            }

            var cylinders = FindObjectsOfType<Cylinder>().ToList();
            foreach (Cylinder cylinder in cylinders)
            {
                SubscribeProperty<float>(cylinder.height);
                SubscribeProperty<float>(cylinder.radius);
            }

            StartCoroutine(SendOperations());
        }

        private void SubscribeProperty<T>(UMI3DAsyncProperty<T> property)
        {
            property.OnValueChanged += v =>
            {
                if (!modifiedProperties.Contains(property))
                {
                    modifiedProperties.Add(property);
                }
            };
        }

        private IEnumerator SendOperations()
        {
            while (maxFrameRate > 0)
            {
                if (modifiedProperties.Count > 0)
                {
                    var transaction = new Transaction();
                    transaction.AddIfNotNull(modifiedProperties.ConvertAll(prop => prop.GetSetEntityOperationForAllUsers()));
                    transaction.reliable = false;
                    UMI3DServer.Dispatch(transaction);
                    modifiedProperties = new List<UMI3DAsyncProperty>();
                }

                yield return new WaitForSeconds(1f / maxFrameRate);
            }
        }
    }
}