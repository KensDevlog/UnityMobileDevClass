using System;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class TouchExample : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private float _moveSpeed = 15f;

    private Vector3 _targetPosition;
    private bool _isDragging;

    #region MonoBehaviour

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    void Start()
    {
        _targetPosition = transform.position;
    }

    void Update()
    {
        HandleTouch();
        MoveToTarget();
    }

    #endregion

    private void HandleTouch()
    {
        if (Touch.activeTouches.Count == 0)
        {
            _isDragging = false;
            return;
        }

        var touch = Touch.activeTouches[0];
        if (touch.phase == TouchPhase.Began)
        {
            _targetPosition = ScreenToWorld(touch.screenPosition);
            _isDragging = true;
        }
        else if (touch.phase == TouchPhase.Moved && _isDragging)
        {
            _targetPosition = ScreenToWorld(touch.screenPosition);
        }
        else if (touch.phase is TouchPhase.Ended or TouchPhase.Canceled)
        {
            _isDragging = false;
        }
    }

    private void MoveToTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, _moveSpeed * Time.deltaTime);
    }

    private Vector3 ScreenToWorld(Vector2 screenPos)
    {
        float z = Mathf.Abs(_mainCamera.transform.position.z - transform.position.z);
        return _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, z));
    }

}