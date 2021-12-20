using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace umi3d.edk.volume
{
    public class VolumeSimpleModificationListener : MonoBehaviour
    {
        public float maxFrameRate = 30;

        private List<UMI3DAsyncProperty> modifiedProperties = new List<UMI3DAsyncProperty>();

        protected virtual void Start()
        {
            List<Box> boxes = FindObjectsOfType<Box>().ToList();
            foreach (Box box in boxes)
            {
                SubscribeProperty<Vector3>(box.center);
                SubscribeProperty<Vector3>(box.size);
            }

            List<Cylinder> cylinders = FindObjectsOfType<Cylinder>().ToList();
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

        IEnumerator SendOperations()
        {
            while (maxFrameRate > 0)
            {
                if (modifiedProperties.Count > 0)
                {
                    Transaction transaction = new Transaction();
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