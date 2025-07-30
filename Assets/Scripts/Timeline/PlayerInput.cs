using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GMTK
{
    [RequireComponent(typeof(TimelineRunner))]
    public class PlayerInput : MonoBehaviour
    {
        private TimelineRunner _runner;
        public InputAction _jumpAction;

        private void Awake()
        {
            _runner = GetComponent<TimelineRunner>();
            _jumpAction.Enable();
        }

        // Update is called once per frame
        void Update()
        {
            if (_jumpAction.WasPerformedThisFrame())
            {
                if (_runner.Playing)
                {
                    Debug.Log("We should jump on the current playing frame!");
                    _runner.Timeline.SetInputOnNextTickedFrame(GameInput.AButton);
                }
            }
        }
    }
}