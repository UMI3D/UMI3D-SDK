using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This MonoBehaviour will be added on gameObject linked with UMI3DModel on server side 
/// </summary>
public class ModelTracker : MonoBehaviour
{
    public bool areSubObjectTracked = false;
    public List<Animator> animatorsToRebind = new List<Animator>();

    public void RebindAnimators()
    {
        if (animatorsToRebind.Count == 0) return;

        lastRebindCall = Time.time;
        if(currentRoutine == null)
            currentRoutine =  this.StartCoroutine(RebindRoutine());
    }

    private float lastRebindCall = 0;
    private Coroutine currentRoutine;
    private IEnumerator RebindRoutine()
    {

        yield return new WaitUntil(()=> (Time.time - lastRebindCall) > 0.4f );
        foreach (Animator animator in animatorsToRebind)
        {
            animator.Rebind();
        }
        currentRoutine = null;
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

}
