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
        [SerializeField] private UIScrubController _scrubber;
        [Header("Player Game Controls")]
        public InputActionReference _jumpAction;
        public InputActionReference _moveAction;
        [Header("Player Timeline Controls")]
        public InputActionReference _recordModeToggle;
        public InputActionReference _playPauseToggle;
        [Header("Scrubbing Controls")]
        public InputActionReference _stepForwardsOne;
        public InputActionReference _stepBackwardsOne;
        public InputActionReference _jumpForward;
        public InputActionReference _jumpBackward;
        public InputActionReference _holdToScrubWithMove;
        //
        private GameInput _gameInput;
        [CanBeNull] private ButtonEvent _currentJumpEvent;
        [CanBeNull] private ButtonEvent _currentMoveEvent;
        [CanBeNull] private ButtonEvent _alternateMoveEvent;
        [SerializeField] private Vector2 _inDir;
        [SerializeField] private Buttons _inButtons;
        private bool _prevTickIsRecording;

        private bool _jumpPressedThisFrame;
        private bool _jumpReleasedThisFrame;
        private bool _movePressedThisFrame;
        private bool _moveReleasedThisFrame;
        private void Awake()
        {
            _runner = GetComponent<TimelineRunner>();
            _jumpAction.action.Enable();
            _moveAction.action.Enable();
            _recordModeToggle.action.Enable();
            _playPauseToggle.action.Enable();
            _stepForwardsOne.action.Enable();
            _stepBackwardsOne.action.Enable();
            _jumpForward.action.Enable();
            _jumpBackward.action.Enable();
            _holdToScrubWithMove.action.Enable();
            
        }

        void Update()
        {
            if (_playPauseToggle.action.WasPerformedThisFrame())
            {
                _runner.PlayPauseToggle();
            }

            if (_recordModeToggle.action.WasPerformedThisFrame())
            {
                _runner.ToggleRecordState();
            }

            if (_stepForwardsOne.action.WasPerformedThisFrame())
            {
                _runner.PauseIfPlaying();
                _scrubber.StepRight();
            }else if (_stepBackwardsOne.action.WasPerformedThisFrame())
            {
                _runner.PauseIfPlaying();
                _scrubber.StepLeft();
            }else if (_jumpForward.action.WasPerformedThisFrame())
            {
                _runner.PauseIfPlaying();
                _scrubber.JumpRight();
            }else if (_jumpBackward.action.WasPerformedThisFrame())
            {
                _runner.PauseIfPlaying();
                _scrubber.JumpLeft();
            }

            bool scrubbing = _holdToScrubWithMove.action.IsPressed();
            _scrubber.SetScrubbing(scrubbing);
            if (scrubbing)
            {
                _runner.PauseIfPlaying();
                //stop recording if recording?
                long delta = (long)(Mathf.RoundToInt(_moveAction.action.ReadValue<Vector2>().x));
                _scrubber.Scrub(delta);
            }
            
            if (_runner.State == RunnerControlState.Recording && _runner.Playing)
            {
                if (!_prevTickIsRecording)
                {
                    //first tick we are recording, the ThisFrame() may or may not be true...
                    _jumpPressedThisFrame = _jumpAction.action.IsPressed();
                    //we force input on exit, so if we aren't pressing, it shouldn't matter...
                    _jumpReleasedThisFrame = _jumpAction.action.WasReleasedThisFrame();
                }
                else
                {
                    _jumpPressedThisFrame = _jumpAction.action.WasPerformedThisFrame();
                    _jumpReleasedThisFrame = _jumpAction.action.WasReleasedThisFrame();
                }

                //same for these....
                _inDir = _moveAction.action.ReadValue<Vector2>();
                _inButtons = DirToMovement(_inDir);
                
                
                _gameInput.JumpButton = _currentJumpEvent;
                if (_jumpPressedThisFrame)
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

                if (_jumpReleasedThisFrame)
                {
                    if (_currentJumpEvent == null)
                    {
                        Debug.LogError("Previous Jump event never set! Need to set jump when entering playback.");
                        return;
                    }
                    _currentJumpEvent.ReleaseFrame = _runner.PendingFrame;
                    _gameInput.JumpButton = _currentJumpEvent;
                    // _runner.Timeline.SetInputOnNextTickedFrame(_gameInput);//don't need to call this twice?
                    _currentJumpEvent = null;
                }

                
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

            //set last.
            _prevTickIsRecording = _runner.State == RunnerControlState.Recording;
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