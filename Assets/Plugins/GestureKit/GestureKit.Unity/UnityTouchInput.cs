using System;
using GestureKit.Core;
using UnityEngine;
using Touch = GestureKit.Core.Touch;
using TouchPhase = GestureKit.Core.TouchPhase;

namespace GestureKit.Unity
{
    public class UnityTouchInput : ITouchInput
    {
        public event Action<Touch> Touch;

        private GameObject gameObject;

        public UnityTouchInput()
        {
            gameObject = new GameObject(GetType().Name);
            gameObject.hideFlags = UnityEngine.HideFlags.HideAndDontSave;
            gameObject.AddComponent<UnityTouchInputBehaviour>().Touch += OnTouch;
            GameObject.DontDestroyOnLoad(gameObject);
        }

        private void OnTouch(Touch t)
        {
            if (Touch != null)
                Touch(t);
        }

        ~UnityTouchInput()
        {
            GameObject.DestroyImmediate(gameObject);
        }
    }

    internal class UnityTouchInputBehaviour : MonoBehaviour
    {
        public event Action<Touch> Touch;

        private Touch touch = new Touch();

        private void Update()
        {
            var isMouseButtonDown = Input.GetMouseButtonDown(0);
            var isMouseButtonUp = Input.GetMouseButtonUp(0);
            var isMouseButton = Input.GetMouseButton(0);

            var hasTouch = isMouseButtonDown || isMouseButtonUp || isMouseButton;
            if (hasTouch)
            {
                var oldX = touch.X;
                var oldY = touch.Y;

                var newX = Input.mousePosition.x;
                var newY = Input.mousePosition.y;

                if (isMouseButtonUp)
                {
                    touch.Phase = TouchPhase.Ended;
                }
                else if (isMouseButtonDown)
                {
                    oldX = newX;
                    oldY = newY;

                    touch.Phase = TouchPhase.Began;
                }
                else if (isMouseButton)
                {
                     var idle = Mathf.Approximately(0, oldX - newX)
                             && Mathf.Approximately(0, oldY - newY);

                    touch.Phase = idle ? TouchPhase.Stationary : TouchPhase.Moved;
                }

                touch.PrevX = oldX;
                touch.PrevY = oldY;
                touch.X = newX;
                touch.Y = newY;

                Touch(touch);
            }
        }
    }
}