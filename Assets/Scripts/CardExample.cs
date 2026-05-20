using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;


public class CardExample : MonoBehaviour
{
    [SerializeField] private RectTransform[] cards;
    [SerializeField] private float swipeAnimDuration = 0.25f;

    [SerializeField] private float minSwipeDistance = 50f;
    [SerializeField] private float maxSwipeTime = 0.4f;

    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 3f;

    private int currentIndex;
    private bool isAnimating;

    private Vector2 swipeStartPos;
    private float swipeStartTime;

    private float prevAngle;
    private float prevPinchDistance;

    private void OnEnable() => EnhancedTouchSupport.Enable();
    private void OnDisable() => EnhancedTouchSupport.Disable();

    void Start()
    {
        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].localScale = Vector3.one;
            cards[i].localEulerAngles = Vector3.zero;
            cards[i].gameObject.SetActive(i == 0);
        }    
    }

    void Update()
    {
        int touchCount = Touch.activeTouches.Count;

        if (touchCount == 1) HandleSwipe(Touch.activeTouches[0]);
        else if (touchCount == 2) HandlePinch(Touch.activeTouches[0], Touch.activeTouches[1]);
    }

    private void HandleSwipe(Touch touch)
    {
        if (touch.phase == TouchPhase.Began)
        {
            swipeStartPos = touch.screenPosition;
            swipeStartTime = Time.time;
        }
        else if (touch.phase == TouchPhase.Ended)
        {
            if (isAnimating) return;
            float elapsed = Time.time - swipeStartTime;
            if (elapsed > maxSwipeTime) return;
            Vector2 delta = touch.screenPosition - swipeStartPos;
            if (Mathf.Abs(delta.x) < minSwipeDistance) return;

            if (delta.x < 0) NavigateTo(currentIndex + 1);
            else NavigateTo(currentIndex - 1);
        }
    }

    private void NavigateTo(int index)
    {
        if (index < 0 || index >= cards.Length) return;

        StartCoroutine(SlideCards(currentIndex, index));
        currentIndex = index;
        
    }

    private IEnumerator SlideCards(int from, int to)
    {
        isAnimating = true;

        float screenWidth = GetComponent<RectTransform>().rect.width;
        float direction = to > from ? -1 : 1;

        RectTransform fromCard = cards[from];
        RectTransform toCard = cards[to];
        toCard.localScale = Vector3.one;
        toCard.localEulerAngles = Vector3.zero;
        toCard.anchoredPosition = new Vector2(-direction * screenWidth, 0);
        toCard.gameObject.SetActive(true);

        float elapsed = 0f;
        Vector2 fromStart = fromCard.anchoredPosition;
        Vector2 fromEnd = new Vector2(direction * screenWidth, 0);
        Vector2 toStart = toCard.anchoredPosition;
        Vector2 toEnd = Vector2.zero;

        while (elapsed < swipeAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / swipeAnimDuration);
            fromCard.anchoredPosition = Vector2.Lerp(fromStart, fromEnd, t);
            toCard.anchoredPosition = Vector2.Lerp(toStart, toEnd, t);
            yield return null;
        }

        fromCard.anchoredPosition = fromEnd;
        toCard.anchoredPosition = toEnd;
        fromCard.gameObject.SetActive(false);

        isAnimating = false;
    }

    private void HandlePinch(Touch touch1, Touch touch2)
    {
        float currentDistance = Vector2.Distance(touch1.screenPosition, touch2.screenPosition);
        float currentAngle = Mathf.Atan2(touch1.screenPosition.y - touch2.screenPosition.y,
                                         touch1.screenPosition.x - touch2.screenPosition.x)
                                           * Mathf.Rad2Deg;

        if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
        {
            prevPinchDistance = currentDistance;
            prevAngle = currentAngle;
        }

        RectTransform card = cards[currentIndex];
        
        float distDelta = currentDistance - prevPinchDistance;
        float scaleDelta = distDelta * 0.002f;
        float newScale = Mathf.Clamp(card.localScale.x + scaleDelta, minScale, maxScale);
        card.localScale = Vector3.one * newScale;
        
        float angleDelta = Mathf.DeltaAngle(prevAngle, currentAngle);
        card.localEulerAngles += new Vector3(0, 0, angleDelta);
        
        prevPinchDistance = currentDistance;
        prevAngle = currentAngle;
    }

}