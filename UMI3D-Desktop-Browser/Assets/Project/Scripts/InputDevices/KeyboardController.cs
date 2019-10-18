using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardController : MonoBehaviour {

    public InteractionMapper player;

    public Vector2 speed = new Vector2(1f, 1f);


    void Update () {
        if (UIDetector.isUI)
            return;

        float dt = Time.deltaTime;
        float dx = Input.GetAxis("Horizontal") * speed.x;
        float dy = Input.GetAxis("Vertical") * speed.y;

        if (Mathf.Abs(dx) > 0f || Mathf.Abs(dy) > 0f)
            if (player != null && player.On2DMoved != null)
            {
                player.On2DMoved(Vector2.zero, new Vector2(dx, dy));
            }
    }
}
