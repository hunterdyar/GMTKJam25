using GMTK;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIScrubController : MonoBehaviour
{
    private Timeline Timeline => _manager.Timeline;
    [SerializeField] private UITimelineManager _manager;
    [SerializeField] private Button JumpLeftButton;
    [SerializeField] private Button StepLeftButton;
    [SerializeField] private Button StepRightButton;
    [SerializeField] private Button JumpRightButton;

    [SerializeField] public float _scrollSpeedMultiplier = 2;
    private bool _isScrubbing;
    public Vector2 _mouseSumPosition;
    public Vector2 _lastMousePosition;
    private long _startScrubCurrentFrame;

    private Mouse mouse;
    void Awake()
    {
        JumpLeftButton.onClick.AddListener(JumpLeft);
        StepLeftButton.onClick.AddListener(StepLeft);
        StepRightButton.onClick.AddListener(StepRight);
        JumpRightButton.onClick.AddListener(JumpRight);
    }

    void Update()
    {
        var mouseCurrent = Mouse.current.position.ReadValue();
        if (_isScrubbing)
        {
            _mouseSumPosition -= mouseCurrent - _lastMousePosition;
            float frameWidth = (_manager.transform as RectTransform).rect.width / _manager.TimelineLength;
            var targetFrameDelta = Mathf.FloorToInt(_mouseSumPosition.x / frameWidth);
            //if playing is off...
            var beforeMove = _manager.Timeline.CurrentDisplayedFrame;
            _manager.Timeline.GoToFrame(_manager.Timeline.CurrentDisplayedFrame + targetFrameDelta);
            var movedFrames = _manager.Timeline.CurrentDisplayedFrame - beforeMove;
            var movedDistance = movedFrames * frameWidth;
            _mouseSumPosition -= Vector2.right*movedDistance;
        }
        else
        {
            _mouseSumPosition = Vector2.zero;
        }
        _lastMousePosition = Mouse.current.position.ReadValue();
    }

    public void SetScrubbing(bool isScrubbing)
    {
        _isScrubbing = isScrubbing;
    }

    public void Scrub(long delta)
    {
        Debug.Assert(_isScrubbing);
        _manager.Timeline.GoToFrame(_manager.Timeline.CurrentDisplayedFrame + (delta));

    }

    public void JumpLeft()
    {
        Timeline.GoToFrame(Timeline.CurrentDisplayedFrame - 100);
    }

    public void StepLeft()
    {
        Debug.Log("StepLeft");
        Timeline.GoToFrame(Timeline.CurrentDisplayedFrame - 1);
    }

    public void JumpRight()
    {
        Timeline.GoToFrame(Timeline.CurrentDisplayedFrame + 100);
    }

    public void StepRight()
    {
        Debug.Log("StepRight");
        Timeline.GoToFrame(Timeline.CurrentDisplayedFrame + 1);
    }
}
