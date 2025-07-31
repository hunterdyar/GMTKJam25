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
        public InputAction _jumpAction;
        private GameInput _gameInput;
        [CanBeNull] private ButtonEvent _currentJumpEvent;
        private void Awake()
        {
            _runner = GetComponent<TimelineRunner>();
            _jumpAction.Enable();
        }

        void Update()
        {
            if (_runner.State == RunnerControlState.Recording && _runner.Playing)
            {
                _gameInput.JumpButton = _currentJumpEvent;
                if (_jumpAction.WasPerformedThisFrame())
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

                if (_jumpAction.WasReleasedThisFrame())
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
            }

            if (_runner.Playing)
            {
                if (_gameInput.Any())
                {
                    _runner.Timeline.SetInputOnNextTickedFrame(_gameInput);
                }
            }
        }
    }
}