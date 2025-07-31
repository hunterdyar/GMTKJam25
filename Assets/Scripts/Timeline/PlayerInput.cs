using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GMTK
{
    [RequireComponent(typeof(TimelineRunner))]
    public class PlayerInput : MonoBehaviour
    {
        private TimelineRunner _runner;
        public InputActionReference _jumpAction;
        public InputActionReference _moveAction;
        private GameInput _gameInput;
        [CanBeNull] private ButtonEvent _currentJumpEvent;
        [CanBeNull] private ButtonEvent _currentMoveEvent;
        private void Awake()
        {
            _runner = GetComponent<TimelineRunner>();
            _jumpAction.action.Enable();
        }

        void Update()
        {
            if (_runner.State == RunnerControlState.Recording && _runner.Playing)
            {
                _gameInput.JumpButton = _currentJumpEvent;
                if (_jumpAction.action.WasPerformedThisFrame())
                {
                    if (_currentJumpEvent != null)
                    {
                        Debug.LogError("Previous Jump event never reset!");
                    }

                    _currentJumpEvent = new ButtonEvent
                    {
                        Button = Buttons.Jump,
                        PressFrame = _runner.PendingFrame
                    };
                    _gameInput.JumpButton = _currentJumpEvent;
                }

                if (_jumpAction.action.WasReleasedThisFrame())
                {
                    if (_currentJumpEvent == null)
                    {
                        Debug.LogError("Previous Jump event never set!");
                        return;
                    }
                    _currentJumpEvent.ReleaseFrame = _runner.PendingFrame;
                    _gameInput.JumpButton = _currentJumpEvent;
                    _runner.Timeline.SetInputOnNextTickedFrame(_gameInput);
                    _currentJumpEvent = null;
                }
                
                var inDir = _moveAction.action.ReadValue<Vector2>();
                var inButton = DirToMovement(inDir);
                if (inButton > 0)
                {
                    if (_currentMoveEvent == null)
                    {
                        //we press direction this frame
                        _currentMoveEvent = new ButtonEvent()
                        {
                            Button = inButton,
                            PressFrame = _runner.PendingFrame
                        };
                        _gameInput.ArrowButton = _currentMoveEvent;
                    }
                    else
                    {
                        //already pressing a direction, we rotated the stick. Which is a release and new press.
                        if (_currentMoveEvent.Button != inButton)
                        {
                            //changed a direction!
                            ReleaseMoveEvent();
                            _currentMoveEvent = new ButtonEvent()
                            {
                                Button = inButton,
                                PressFrame = _runner.PendingFrame
                            };
                            _gameInput.ArrowButton = _currentMoveEvent;
                        }
                    }
                }
                else
                {
                    //if not null, we released a button this frame.
                    if (_currentMoveEvent != null)
                    {
                       ReleaseMoveEvent();
                    }
                }
            }

            if (_runner.Playing)
            {
                if (_gameInput.Any())
                {
                    _runner.Timeline.SetInputOnNextTickedFrame(_gameInput);
                }
            }

            void ReleaseMoveEvent()
            {
                _currentMoveEvent.ReleaseFrame = _runner.PendingFrame;
                _gameInput.ArrowButton = _currentMoveEvent;
                _runner.Timeline.SetInputOnNextTickedFrame(_gameInput);
                _currentMoveEvent = null;
            }
        }
        

        
        public static Buttons DirToMovement(Vector2 dir)
        {
            var h = Mathf.RoundToInt(dir.normalized.x);
            var v = Mathf.RoundToInt(dir.normalized.y);
            var hb = h == 1 ? Buttons.Right : h == -1 ? Buttons.Left : 0;
            var vb = v == 1 ? Buttons.Right : v == -1 ? Buttons.Left : 0;
            return hb | vb;
        }
    }
}