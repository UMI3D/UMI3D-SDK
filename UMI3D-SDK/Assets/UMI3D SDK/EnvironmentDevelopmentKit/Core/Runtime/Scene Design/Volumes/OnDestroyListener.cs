using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using umi3d.edk;
using UnityEngine;
using System.Linq;

/// <summary>
/// When this component is destroy, a DestroyEntity operation will be sent for every UMI3DEntity in children.
/// </summary>
public class OnDestroyListener : MonoBehaviour
{
    void OnDestroy()
    {
        List<DeleteEntity> deleteOperations = this.GetComponentsInChildren<UMI3DEntity>().ToList()
            .ConvertAll(entity =>
                new DeleteEntity()
                {
                    entityId = entity.Id(),
                    users = UMI3DServer.Instance.UserSet()
                });

        if (deleteOperations.Count > 0)
        {
            Transaction deleteTransaction = new Transaction();
            deleteTransaction.AddIfNotNull(deleteOperations);
            deleteTransaction.reliable = true;

            UMI3DServer.Dispatch(deleteTransaction);
        }
    }
}
