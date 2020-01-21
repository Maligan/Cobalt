using System;
using Netouch.Core;
using UnityEngine;
using Touch = Netouch.Core.Touch;
using TouchPhase = Netouch.Core.TouchPhase;

namespace Netouch.Unity
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

	/// Helper class for access to Unity Update() event
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
            else if (touch.Phase != TouchPhase.Canceled)
            {
                touch.Phase = TouchPhase.Canceled;
                touch.PrevTime = touch.Time;
                touch.PrevX = touch.X;
                touch.PrevY = touch.Y;
                touch.Time = Time.unscaledTime;

                Touch(touch);
            }
        }
    }
}
