using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace umi3d.edk.volume
{
    public class VolumeSimpleModificationListener : MonoBehaviour
    {
        public float maxFrameRate = 30;

        private List<SetEntityProperty> operationsToSend = new List<SetEntityProperty>();

        protected virtual void Start()
        {
            List<Box> boxes = FindObjectsOfType<Box>().ToList();
            foreach (Box box in boxes)
            {
                box.center.OnValueChanged += v => { operationsToSend.Add(box.center.SetValue(box.center.GetValue())); };
                box.size.OnValueChanged += v => { operationsToSend.Add(box.size.SetValue(box.size.GetValue())); };
            }
        }

        IEnumerator SendOperations()
        {
            while (true)
            {
                if (operationsToSend.Count > 0)
                {
                    Transaction transaction = new Transaction();
                    transaction.Add(operationsToSend);
                    operationsToSend = new List<SetEntityProperty>();
                }
                yield return new WaitForSeconds(60f / maxFrameRate);
            }
        }
    }
}