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
        [CanBeNull] private ButtonEvent _alternateMoveEvent;
        [SerializeField] private Vector2 _inDir;
        [SerializeField] private Buttons _inButtons;
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
                    // _runner.Timeline.SetInputOnNextTickedFrame(_gameInput);//don't need to call this twice?
                    _currentJumpEvent = null;
                }

                _inDir = _moveAction.action.ReadValue<Vector2>();
                _inButtons = DirToMovement(_inDir);
                if (_inButtons > 0)
                {
                    if (_currentMoveEvent == null)
                    {
                        //we first pressed a direction this frame
                        _currentMoveEvent = new ButtonEvent()
                        {
                            Button = _inButtons,
                            PressFrame = _runner.PendingFrame
                        };
                        _gameInput.ArrowButton = _currentMoveEvent;
                        Debug.Assert(_gameInput.ArrowButtonB == null);
                    }
                    else
                    {
                        //already pressing a direction, we rotated the stick. Which is a release and new press.
                        if (_currentMoveEvent.Button != _inButtons)
                        {
                            //changed a direction!
                            _currentMoveEvent.ReleaseFrame  = _runner.PendingFrame;
                            _gameInput.ArrowButtonB = _currentMoveEvent;
                            _currentMoveEvent = new ButtonEvent()
                            {
                                Button = _inButtons,
                                PressFrame = _runner.PendingFrame
                            };
                            _gameInput.ArrowButton = _currentMoveEvent;
                        }
                        else
                        {
                            if (_gameInput.ArrowButtonB != null)
                            {
                                //shift back into primary
                                _gameInput.ArrowButton = _currentMoveEvent;
                                _gameInput.ArrowButtonB = null;
                            }
                            //we are continuing to press the button.
                            Debug.Assert(_currentMoveEvent.Button != Buttons.None && _currentMoveEvent.Button != Buttons.Jump);
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
                _gameInput.ArrowButton = null;//This was the bug! this line missing!
                _gameInput.ArrowButtonB = null;
            }
            
        }
        

        
        public static Buttons DirToMovement(Vector2 dir)
        {
            var h = Mathf.RoundToInt(dir.normalized.x);
            var v = Mathf.RoundToInt(dir.normalized.y);
            Buttons b = 0;
            if (h == 1d)
            {
                b |= Buttons.Right;
            }

            if (h == -1)
            {
                b |= Buttons.Left;
            }

            if (v == 1)
            {
                b |= Buttons.Up;
            }

            if (v == -1)
            {
                b |= Buttons.Down;
            }
            return b;
        }
    }
}