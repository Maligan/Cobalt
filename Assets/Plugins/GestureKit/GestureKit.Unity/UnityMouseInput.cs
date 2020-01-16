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
                var oldTime = touch.Time;
                var oldX = touch.X;
                var oldY = touch.Y;

                var newTime = Time.unscaledTime;
                var newX = UnityEngine.Input.mousePosition.x;
                var newY = UnityEngine.Input.mousePosition.y;

                if (isMouseButtonUp)
                {
                    touch.Phase = TouchPhase.Ended;
                }
                else if (isMouseButtonDown)
                {
                    oldTime = newTime;
                    oldX = newX;
                    oldY = newY;

                    touch.Phase = TouchPhase.Began;
                    touch.BeginTime = newTime;
                    touch.BeginX = newX;
                    touch.BeginY = newX;
                }
                else if (isMouseButton)
                {
                     var idle = Mathf.Approximately(0, oldX - newX)
                             && Mathf.Approximately(0, oldY - newY);

                    touch.Phase = idle ? TouchPhase.Stationary : TouchPhase.Moved;
                }

                touch.PrevTime = oldTime;
                touch.PrevX = oldX;
                touch.PrevY = oldY;

                touch.Time = newTime;
                touch.X = newX;
                touch.Y = newY;

                Touch(touch);
            }
        }
    }
}