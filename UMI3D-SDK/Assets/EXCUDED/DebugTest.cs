using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using umi3d.cdk.interaction;
using umi3d.common.interaction;
using UnityEngine.Events;

public class DebugTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ulong tbid = (ulong)Random.Range(ulong.MinValue, ulong.MaxValue);

        ToolboxDto toolboxDto = new ToolboxDto()
        {
            id = tbid,
            active = true,
            name = "root",
            tools = new List<GlobalToolDto>()
            {
                new GlobalToolDto()
                {
                    name = "1",
                    id = (ulong) Random.Range(ulong.MinValue, ulong.MaxValue)
                },
                new GlobalToolDto()
                {
                    name = "2",
                    id = (ulong) Random.Range(ulong.MinValue, ulong.MaxValue)
                },
                new GlobalToolDto()
                {
                    name = "3",
                    id = (ulong) Random.Range(ulong.MinValue, ulong.MaxValue)
                }
            }
        };

        System.Action success = () =>
        {
            Toolbox tb = Toolbox.GetToolbox(tbid);
            List<GlobalTool> tools = tb.GetTools();
            tools.ForEach(gt => Debug.Log(gt.name));
        };

        UMI3DGlobalToolLoader.ReadUMI3DExtension(toolboxDto, success, e => Debug.Log(e));
    }

}
