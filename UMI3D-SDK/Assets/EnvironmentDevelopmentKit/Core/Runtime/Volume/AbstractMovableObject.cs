using UnityEngine;
using UnityEngine.Events;

namespace umi3d.edk.volume.volumedrawing
{
    public abstract class AbstractMovableObject : AbstractSelectable
    {
        protected class Vector3Event : UnityEvent<Vector3> { }

        protected Vector3Event onMove = new Vector3Event();
        protected UnityEvent onMoveBegin = new UnityEvent();
        protected UnityEvent onMoveEnd = new UnityEvent();


        public virtual void MoveBegin()
        {
            onMoveBegin.Invoke();
        }

        public virtual void MoveEnd()
        {
            onMoveEnd.Invoke();
        }

        public virtual void Move(Vector3 translation)
        {
            onMove.Invoke(this.transform.position);
        }

        public virtual void SubscribeToMoveBegin(UnityAction callback)
        {
            onMoveBegin.AddListener(callback);
        }

        public virtual void SubscribeToMoveEnd(UnityAction callback)
        {
            onMoveEnd.AddListener(callback);
        }

        public virtual void SubscribeToMove(UnityAction<Vector3> callback, float period = 0)
        {
            onMove.AddListener(callback);
        }

        public virtual void UnsubscribeToMoveBegin(UnityAction callback)
        {
            onMoveBegin.RemoveListener(callback);
        }

        public virtual void UnsubscribeToMoveEnd(UnityAction callback)
        {
            onMoveEnd.RemoveListener(callback);
        }

        public virtual void UnsubscribeToMove(UnityAction<Vector3> callback, float period = 0)
        {
            onMove.RemoveListener(callback);
        }
    }
}
