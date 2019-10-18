using DesktopUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolboxPanel : MonoBehaviour {

    public BasicList actions;
    public BasicList manipulations;
    public BasicList settings;

    private void OnEnable()
    {
        Update();
    }

    // Update is called once per frame
    void Update () {
        if (actions != null)
            actions.gameObject.SetActive(actions.Count > 0);
        if (manipulations != null)
            manipulations.gameObject.SetActive(manipulations.Count > 0);
        if (settings != null)
            settings.gameObject.SetActive(settings.Count > 0);
    }

    public int Count {
        get {
            var tot = 0;
            if (actions != null)
                tot += actions.Count;
            if (manipulations != null)
                tot += manipulations.Count;
            if (settings != null)
                tot += settings.Count;
            return tot;
        }
    }

    public void Clear()
    {
        if (actions != null)
            actions.ClearItems();
        if (manipulations != null)
            manipulations.ClearItems();
        if (settings != null)
            settings.ClearItems();
    }
}
