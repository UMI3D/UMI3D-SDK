/*
Copyright 2019 Gfi Informatique

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
using UnityEngine;

[CreateAssetMenu(fileName = "FPSData", menuName = "UMI3D/FPS Data", order = 1)]
public class FpsScriptableAsset : ScriptableObject
{
    [Header("View range")]
    [Tooltip("Range of the viwpoint x angle (down to up)")]
    public Vector2 XAngleRange;
    [Tooltip("Range of the head x angle (down to up). \n" +
        "The head will follow the viewpoint bounded by this range")]
    public Vector2 XDisplayAngleRange;
    [Tooltip("Range of the viwpoint/head y angle (left to right)")]
    public Vector2 YAngleRange;

    [Header("Walk speed")]
    [Tooltip("speed when moving forward (normal, squatting, running)")]
    public Vector3 forwardSpeed;
    [Tooltip("speed when moving sideway (normal, squatting, running)")]
    public Vector3 lateralSpeed;
    [Tooltip("speed when moving backward (normal, squatting, running)")]
    public Vector3 backwardSpeed;

    [Header("view speed")]
    [Tooltip("Angular speed of the viewpoint (up/down, left/right)")]
    public Vector2 AngularViewSpeed;

    [Header("jump and squat fields")]
    [Tooltip("gravity force")]
    public float gravity;
    [Tooltip("max jump height when long pressing jump action")]
    public float MaxJumpHeight;
    [Tooltip("min jump height when short pressing jump action")]
    public float MinJumpHeight;
    [Tooltip("time to switch between standing and squatting (both ways)")]
    public float squatSpeed;

    [Tooltip("player height while squatting")]
    public float squatHeight;
    [Tooltip("player height while standing")]
    public float standHeight;

    [Tooltip("Torso angle when squatting")]
    public float squatTorsoAngle;

    [Header("Fly")]
    [Tooltip("speed when moving in every direction while flying")]
    public float flyingSpeed;
}
