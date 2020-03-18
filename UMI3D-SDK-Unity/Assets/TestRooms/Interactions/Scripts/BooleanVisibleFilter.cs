using System.Collections;
using System.Collections.Generic;
using umi3d.edk;
using UnityEngine;

public class BooleanVisibleFilter : umi3d.edk.VisibilityFilter,IHasAsyncProperties
{
    public UMI3DAsyncProperty<bool> display;

    private void Start()
    {
        display = new UMI3DAsyncProperty<bool>(this,true);
    }

    public override bool Accept(UMI3DUser user)
    {
        return display.GetValue(user);
    }

    public void NotifyUpdate()
    {
        
    }

    public void NotifyUpdate(UMI3DUser u)
    {
        
    }
}
