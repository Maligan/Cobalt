using System;
using Netouch.Core;
using UnityEngine;
using Touch = Netouch.Core.Touch;
using TouchPhase = Netouch.Core.TouchPhase;

namespace Netouch.Unity
{
    public class UnityTouchInput : ITouchInput
    {
        public event Action<Touch> Touch;
		public event Action<float> Frame;

        private GameObject gameObject;

        public UnityTouchInput()
        {
            gameObject = new GameObject(GetType().Name);
            gameObject.hideFlags = UnityEngine.HideFlags.HideAndDontSave;

            var script = gameObject.AddComponent<UnityMouseInputBehaviour>();
			script.Touch += OnTouch;
			script.Frame += OnFrame;

            GameObject.DontDestroyOnLoad(gameObject);
        }

        private void OnTouch(Touch touch)
        {
            if (Touch != null)
                Touch(touch);
        }

        private void OnFrame(float time)
        {
            if (Frame != null)
                Frame(time);
        }

        ~UnityTouchInput()
        {
            GameObject.DestroyImmediate(gameObject);
        }
    }

	/// Helper class for access to Unity Update() event
    internal class UnityMouseInputBehaviour : MonoBehaviour
    {
        public event Action<Touch> Touch;
        public event Action<float> Frame;

        private Touch touch = new Touch() { Phase = TouchPhase.Canceled };
        private Touch[] touches = new Touch[32];

        private void Update()
        {
			Frame(Time.unscaledTime);

            var useTouches = true;
            if (useTouches)
            {
                UpdateTouches();
            }
            else
            {
                UpdateMouseButton();
            }
        }

        private void UpdateTouches()
        {
            Debug.Log(Input.touchCount);

            for (var i = 0; i < Input.touchCount; i++)
            {
                var source = Input.touches[i];
                var target = touches[source.fingerId];

                var oldTime = target.Time;
                var oldX = target.X;
                var oldY = target.Y;

                var newTime = Time.unscaledTime;
                var newX = source.position.x;
                var newY = source.position.y;

                if (source.phase == UnityEngine.TouchPhase.Began)
                {
                    target.BeginTime = oldTime = newTime;
                    target.BeginX = oldX = newX;
                    target.BeginY = oldY = newY;
                }

                target.Phase = Convert(source.phase);

                target.PrevTime = oldTime;
                target.PrevX = oldX;
                target.PrevY = oldY;

                target.Time = newTime;
                target.X = newX;
                target.Y = newY;

                Touch(target);
            }
        }

        private TouchPhase Convert(UnityEngine.TouchPhase phase)
        {
            switch (phase)
            {
                case UnityEngine.TouchPhase.Began: return TouchPhase.Began;
                case UnityEngine.TouchPhase.Canceled: return TouchPhase.Canceled;
                case UnityEngine.TouchPhase.Ended: return TouchPhase.Ended;
                case UnityEngine.TouchPhase.Moved: return TouchPhase.Moved;
                case UnityEngine.TouchPhase.Stationary: return TouchPhase.Stationary;
            }

            throw new Exception($"Unknown UnityEngine.TouchPhase: ({phase})");
        }

        private void UpdateMouseButton()
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
                    touch.Phase = TouchPhase.Began;
                    touch.BeginTime = oldTime = newTime;
                    touch.BeginX = oldX = newX;
                    touch.BeginY = oldY = newY;
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
