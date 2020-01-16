using System;
using GestureKit.Input;
using UnityEngine;
using Touch = GestureKit.Input.Touch;
using TouchPhase = GestureKit.Input.TouchPhase;

namespace GestureKit.Unity
{
    public class UnityMouseInput : ITouchInput
    {
        public event Action<Touch> Touch;

        private GameObject gameObject;

        public UnityMouseInput()
        {
            gameObject = new GameObject(GetType().Name);
            gameObject.hideFlags = UnityEngine.HideFlags.HideAndDontSave;
            gameObject.AddComponent<UnityMouseInputBehaviour>().Touch += OnTouch;
            GameObject.DontDestroyOnLoad(gameObject);
        }

        private void OnTouch(Touch t)
        {
            if (Touch != null)
                Touch(t);
        }

        ~UnityMouseInput()
        {
            GameObject.DestroyImmediate(gameObject);
        }
    }

    internal class UnityMouseInputBehaviour : MonoBehaviour
    {
        public event Action<Touch> Touch;

        private Touch touch = new Touch();

        private void Update()
        {
            var isMouseButtonDown = UnityEngine.Input.GetMouseButtonDown(0);
            var isMouseButtonUp = UnityEngine.Input.GetMouseButtonUp(0);
            var isMouseButton = UnityEngine.Input.GetMouseButton(0);

            var hasTouch = isMouseButtonDown || isMouseButtonUp || isMouseButton;
            if (hasTouch)
            {
                var oldX = touch.X;
                var oldY = touch.Y;

                var newX = UnityEngine.Input.mousePosition.x;
                var newY = UnityEngine.Input.mousePosition.y;

                if (isMouseButtonUp)
                {
                    touch.Phase = TouchPhase.Ended;
                }
                else if (isMouseButtonDown)
                {
                    oldX = newX;
                    oldY = newY;

                    touch.Phase = TouchPhase.Began;
                    touch.BeginX = newX;
                    touch.BeginY = newX;
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

                // UnityEngine.Touch

                Touch(touch);
            }
        }
    }
}