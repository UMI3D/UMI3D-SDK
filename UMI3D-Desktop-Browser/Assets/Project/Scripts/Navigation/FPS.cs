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

        public Transform _viewpoint;
        public Transform head;
        public Transform Neck;
        public Transform TorsoUpAnchor;
        public enum State { Default, FreeHead, FreeMousse }
        public enum Navigation { Walking, Flying }

        public State state;
        bool changeToDefault = false;
        Vector3 LastAngleView;
        public Navigation navigation;

        public FpsScriptableAsset data;

        Quaternion UserYRotation;

        float MaxJumpVelocity;
        float MinJumpVelocity;

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

            jumpData.velocity += data.gravity * Time.deltaTime;
            jumpData.deltaHeight = Mathf.Max(0, jumpData.deltaHeight + jumpData.velocity * Time.deltaTime);
            return jumpData.deltaHeight;
        }

        public override void Setup(Transform trackingZone, Transform viewpoint)
        {
            base.Setup(trackingZone, viewpoint);
            state = State.Default;
            jumpData = new JumpData();
            CursorHandler.Movement = CursorHandler.CursorMovement.Center;

            MaxJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(data.gravity) * data.MaxJumpHeight);
            MinJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(data.gravity) * data.MinJumpHeight);
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

            if (MainMenu.IsDisplaying || CursorHandler.Movement == CursorHandler.CursorMovement.Free || CursorHandler.Movement == CursorHandler.CursorMovement.FreeHiden)
                return;

            if (state == State.Default && Input.GetKey(InputLayoutManager.GetInputCode(InputLayoutManager.Input.FreeView))) { state = State.FreeHead; }
            else if (state == State.FreeHead && !Input.GetKey(InputLayoutManager.GetInputCode(InputLayoutManager.Input.FreeView))) { state = State.Default; changeToDefault = true; }
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
                    jumpData.heigth = height;
                    height += jumpData.deltaHeight;
                    break;
                case Navigation.Flying:
                    Fly(ref Move, ref height);
                    jumpData.heigth = height;
                    break;
            }
            Move *= Time.deltaTime;

            
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
                Move.x *= (Move.x > 0) ? data.forwardSpeed.y : data.backwardSpeed.y;
                Move.y *= data.lateralSpeed.y;
            }
            else if (Input.GetKey(InputLayoutManager.GetInputCode(InputLayoutManager.Input.Sprint)))
            {
                Move.x *= (Move.x > 0) ? data.forwardSpeed.z : data.backwardSpeed.z;
                Move.y *= data.lateralSpeed.z;
            }
            else
            {
                Move.x *= (Move.x > 0) ? data.forwardSpeed.x : data.backwardSpeed.x;
                Move.y *= data.lateralSpeed.x;
            }
            bool Squatting = Input.GetKey(InputLayoutManager.GetInputCode(InputLayoutManager.Input.Squat));
            height = Mathf.Lerp(height,(Squatting) ? data.squatHeight : data.standHeight, data.squatSpeed == 0 ? 1000000 : Time.deltaTime/ data.squatSpeed);
            TorsoUpAnchor.localRotation = Quaternion.Euler((Squatting) ? data.squatTorsoAngle : 0, 0, 0);
            ComputeJump(Input.GetKey(InputLayoutManager.GetInputCode(InputLayoutManager.Input.Jump)));
        }

        void Fly(ref Vector2 Move, ref float height)
        {
            Move.x *= data.flyingSpeed;
            Move.y *= data.flyingSpeed;
            height += data.flyingSpeed * ((Input.GetKey(InputLayoutManager.GetInputCode(InputLayoutManager.Input.Squat)) ? -1 : 0) + (Input.GetKey(InputLayoutManager.GetInputCode(InputLayoutManager.Input.Jump)) ? 1 : 0));
        }

        void HandleView()
        {
            if (state == State.FreeMousse) return;
            Vector3 angleView = NormalizeAngle(_viewpoint.rotation.eulerAngles);

            Vector2 angularSpeed = new Vector2(-1 * Input.GetAxis("Mouse Y") * data.AngularViewSpeed.x, Input.GetAxis("Mouse X") * data.AngularViewSpeed.y);
            Vector3 result = NormalizeAngle(angleView + (Vector3)angularSpeed);
            if (changeToDefault)
            {
                result = LastAngleView;
                changeToDefault = false;
            }
            Vector3 displayResult;

            if (result.x < data.XAngleRange.x) result.x = data.XAngleRange.x;
            if (result.x > data.XAngleRange.y) result.x = data.XAngleRange.y;
            displayResult = result;
            if (displayResult.x < data.XDisplayAngleRange.x) displayResult.x = data.XDisplayAngleRange.x;
            if (displayResult.x > data.XDisplayAngleRange.y) displayResult.x = data.XDisplayAngleRange.y;

            if (state == State.Default)
            {
                Neck.transform.rotation = Quaternion.Euler(new Vector3(0, result.y, 0));
                LastAngleView = result;
            }
            else
            {
                Vector3 angleNeck = NormalizeAngle(Neck.rotation.eulerAngles);
                float delta = Mathf.DeltaAngle(result.y, angleNeck.y);

                if (delta < data.YAngleRange.x) result.y = -data.YAngleRange.x + angleNeck.y;
                if (delta > data.YAngleRange.y) result.y = -data.YAngleRange.y + angleNeck.y;
            }
            _viewpoint.transform.rotation = Quaternion.Euler(result);
            head.transform.rotation = Quaternion.Euler(displayResult);
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
            Vector3 rot = _viewpoint.rotation.eulerAngles;
            return Quaternion.Euler(0, rot.y, 0);
        }
    }
}