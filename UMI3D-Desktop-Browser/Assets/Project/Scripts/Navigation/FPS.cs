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
using umi3d.cdk;
using UnityEngine;
using BrowserDesktop.Cursor;
using BrowserDesktop.Controller;
using BrowserDesktop.Menu;

namespace BrowserDesktop
{
    public class FPS : AbstractNavigation
    {
        public InteractionMapper mapper;

        private bool used = false;

        public Transform Head;
        public Transform Neck;
        public Transform TorsoUpAnchor;
        public enum State { Default, FreeHead, FreeMousse }
        public enum Navigation { Walking, Flying }

        public State state;
        public Navigation navigation;

        public Vector2 XAngleRange;
        public Vector2 YAngleRange;

        public Vector3 forwardSpeed;
        public Vector3 lateralSpeed;
        public Vector3 backwardSpeed;

        public Vector2 AngularViewSpeed;

        Quaternion UserYRotation;

        public float gravity;
        public float MaxJumpHeight;
        public float MinJumpHeight;
        float MaxJumpVelocity;
        float MinJumpVelocity;

        public float squatHeight;
        public float standHeight;

        public float squatTorsoAngle;

        public float flyingSpeed;

        struct JumpData
        {
            public bool jumping;
            public float timeSinceJump;
            public float velocity;
            public float deltaHeight;
            public float heigth;

            public JumpData(bool jumping, float timeSinceJump, float velocity, float deltaHeight) : this()
            {
                this.jumping = jumping;
                this.timeSinceJump = timeSinceJump;
                this.velocity = velocity;
                this.deltaHeight = deltaHeight;
            }
        }
        JumpData jumpData;

        float ComputeJump(bool jumping)
        {
            if (jumpData.jumping && jumpData.deltaHeight == 0)
            {
                jumpData.velocity = MaxJumpVelocity;
            }

            if (jumpData.jumping != jumping)
            {
                jumpData.jumping = jumping;

                if (!jumpData.jumping && jumpData.velocity > MinJumpVelocity)
                {
                    jumpData.velocity = MinJumpVelocity;
                }

            }

            jumpData.velocity += gravity * Time.deltaTime;

            jumpData.deltaHeight = Mathf.Max(0, jumpData.deltaHeight + jumpData.velocity * Time.deltaTime);

            return jumpData.deltaHeight;
        }

        public override void Setup(Transform trackingZone, Transform viewpoint)
        {
            base.Setup(trackingZone, viewpoint);
            state = State.Default;
            jumpData = new JumpData();
            CursorHandler.Movement = CursorHandler.CursorMovement.Center;

            MaxJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * MaxJumpHeight);
            MinJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * MinJumpHeight);
            //var euler = viewpoint.localEulerAngles;
            //_x = euler.x;
            //_y = euler.y;
            //_height = viewpoint.localPosition.y;
            //Move(Vector3.zero, 0, 0);
            used = true;
        }



        private void LateUpdate()
        {
            if (!used)
                return;
            if (Input.GetKeyDown(InputLayoutManager.GetInputCode(InputLayoutManager.Input.MainMenuToggle)))
            {
                if (CircleMenu.Exist && CircleMenu.Instance.IsExpanded)
                {
                    CircleMenu.Instance.Collapse();
                }

                MainMenu.Display(!MainMenu.IsDisplaying);
            }

            if (MainMenu.IsDisplaying || CursorHandler.Movement == CursorHandler.CursorMovement.Free)
                return;

            if (state == State.Default && Input.GetKey(InputLayoutManager.GetInputCode(InputLayoutManager.Input.FreeView))) { state = State.FreeHead; }
            else if (state == State.FreeHead && !Input.GetKey(InputLayoutManager.GetInputCode(InputLayoutManager.Input.FreeView))) { state = State.Default; }
            Vector2 Move = Vector2.zero;
            float height = jumpData.heigth;
            if (Input.GetKey(InputLayoutManager.GetInputCode(InputLayoutManager.Input.Forward))) { Move.x += 1; }
            if (Input.GetKey(InputLayoutManager.GetInputCode(InputLayoutManager.Input.Backward))) { Move.x -= 1; }
            if (Input.GetKey(InputLayoutManager.GetInputCode(InputLayoutManager.Input.Right))) { Move.y += 1; }
            if (Input.GetKey(InputLayoutManager.GetInputCode(InputLayoutManager.Input.Left))) { Move.y -= 1; }

            switch (navigation)
            {
                case Navigation.Walking:
                    Walk(ref Move, ref height);
                    break;
                case Navigation.Flying:
                    Fly(ref Move, ref height);
                    break;
            }
            Move *= Time.deltaTime;

            jumpData.heigth = height;
            HandleView();
            Vector3 pos = Neck.rotation * new Vector3(Move.y, 0, Move.x);
            pos += Neck.transform.position;
            pos.y = height;
            Neck.transform.position = pos;
        }

        void Walk(ref Vector2 Move, ref float height)
        {
            if (Input.GetKey(InputLayoutManager.GetInputCode(InputLayoutManager.Input.Squat)))
            {
                Move.x *= (Move.x > 0) ? forwardSpeed.y : backwardSpeed.y;
                Move.y *= lateralSpeed.y;
            }
            else if (Input.GetKey(InputLayoutManager.GetInputCode(InputLayoutManager.Input.Sprint)))
            {
                Move.x *= (Move.x > 0) ? forwardSpeed.z : backwardSpeed.z;
                Move.y *= lateralSpeed.z;
            }
            else
            {
                Move.x *= (Move.x > 0) ? forwardSpeed.x : backwardSpeed.x;
                Move.y *= lateralSpeed.x;
            }
            bool Squatting = Input.GetKey(InputLayoutManager.GetInputCode(InputLayoutManager.Input.Squat));
            height = (Squatting) ? squatHeight : standHeight;
            TorsoUpAnchor.localRotation = Quaternion.Euler((Squatting) ? squatTorsoAngle : 0, 0, 0);
            height += ComputeJump(Input.GetKey(InputLayoutManager.GetInputCode(InputLayoutManager.Input.Jump)));
        }

        void Fly(ref Vector2 Move, ref float height)
        {
            Move.x *= flyingSpeed;
            Move.y *= flyingSpeed;
            height += flyingSpeed * ((Input.GetKey(InputLayoutManager.GetInputCode(InputLayoutManager.Input.Squat)) ? -1 : 0) + (Input.GetKey(InputLayoutManager.GetInputCode(InputLayoutManager.Input.Jump)) ? 1 : 0));
        }

        void HandleView()
        {

            if (state == State.FreeMousse) return;

            Vector2 angularSpeed = new Vector2(-1 * Input.GetAxis("Mouse Y") * AngularViewSpeed.x, Input.GetAxis("Mouse X") * AngularViewSpeed.y);
            Vector3 angleView = NormalizeAngle(Head.rotation.eulerAngles);
            Vector3 result = NormalizeAngle(angleView + (Vector3)angularSpeed);

            if (result.x < XAngleRange.x) result.x = XAngleRange.x;
            if (result.x > XAngleRange.y) result.x = XAngleRange.y;

            if (state == State.Default)
            {
                Neck.transform.rotation = Quaternion.Euler(new Vector3(0, result.y, 0));
            }
            else
            {
                Vector3 angleNeck = NormalizeAngle(Neck.rotation.eulerAngles);
                float delta = Mathf.DeltaAngle(result.y, angleNeck.y);

                if (delta < YAngleRange.x) result.y = -YAngleRange.x + angleNeck.y;
                if (delta > YAngleRange.y) result.y = -YAngleRange.y + angleNeck.y;
            }
            Head.transform.rotation = Quaternion.Euler(result);
            UserYRotation = Quaternion.Euler(new Vector3(0, result.y, 0));
        }

        Vector3 NormalizeAngle(Vector3 angle)
        {
            angle.x = Mathf.DeltaAngle(0, angle.x);
            angle.y = Mathf.DeltaAngle(0, angle.y);
            angle.z = Mathf.DeltaAngle(0, angle.z);
            return angle;
        }



        public override void Disable()
        {
            used = false;
        }

        protected override Vector3 GetUserPosition()
        {
            return Neck.position - new Vector3(0,jumpData.heigth,0);
        }

        protected override Quaternion GetUserRotation()
        {
            Vector3 rot = Head.rotation.eulerAngles;
            return Quaternion.Euler(0, rot.y, 0);
        }
    }
}