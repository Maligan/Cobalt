using System;
using Netouch.Core;
using UnityEngine;
using Touch = Netouch.Core.Touch;
using TouchPhase = Netouch.Core.TouchPhase;

namespace Netouch.Unity
{
    public class UnityInput : IInput
    {
        public event Action<Touch> Touch;
		public event Action<float> Frame;

        private GameObject gameObject;

        public UnityInput(bool useTouches = false)
        {
            gameObject = new GameObject(nameof(UnityInput));
            gameObject.hideFlags = UnityEngine.HideFlags.HideAndDontSave;
            GameObject.DontDestroyOnLoad(gameObject);

            var behaviour = gameObject.AddComponent<UnityInputBehaviour>();
            behaviour.UseTouches = useTouches;
			behaviour.Touch += OnTouch;
			behaviour.Frame += OnFrame;
        }

        ~UnityInput()
        {
            GameObject.Destroy(gameObject);
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

        /// Helper class for access to Unity Update() event
        internal class UnityInputBehaviour : MonoBehaviour
        {
            public bool UseTouches { get; set; }

            public event Action<Touch> Touch;
            public event Action<float> Frame;

            private Touch[] touches = new Touch[8];

            private void Start()
            {
                for (var i = 0; i < touches.Length; i++)
                    touches[i] = new Touch() { Phase = TouchPhase.Canceled };
            }

            private void Update()
            {
                // Frame()
                Frame(Time.unscaledTime);

                // Touch()
                if (UseTouches) ProcessTouches();
                else ProcessMouseButton0();
            }

            private void ProcessTouches()
            {
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

                    target.Phase = (TouchPhase)source.phase;

                    target.PrevTime = oldTime;
                    target.PrevX = oldX;
                    target.PrevY = oldY;

                    target.Time = newTime;
                    target.X = newX;
                    target.Y = newY;

                    Touch(target);
                }
            }

            private void ProcessMouseButton0()
            {
                // Always use first Touch from pool
                var touch = touches[0];

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
}
