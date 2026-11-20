using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Gives a text button clear visual feedback while it is being pressed.</summary>
[DisallowMultipleComponent]
public sealed class ButtonPressAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, ISubmitHandler
{
    [SerializeField, Range(0.8f, 1f)] private float pressedScale = 0.92f;
    [SerializeField, Min(0f)] private float pressDuration = 0.07f;
    [SerializeField, Min(0f)] private float releaseDuration = 0.12f;

    private Button button;
    private RectTransform buttonTransform;
    private Vector3 restingScale;
    private Tween scaleTween;
    private bool isPointerDown;

    private void Awake()
    {
        CacheReferences();
        restingScale = buttonTransform.localScale;
    }

    private void OnEnable()
    {
        CacheReferences();
        restingScale = buttonTransform.localScale;
    }

    private void OnDisable()
    {
        isPointerDown = false;
        StopAnimation();

        if (buttonTransform != null)
        {
            buttonTransform.localScale = restingScale;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || !CanAnimate())
        {
            return;
        }

        isPointerDown = true;
        AnimateTo(restingScale * pressedScale, pressDuration, Ease.OutQuad);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || !isPointerDown)
        {
            return;
        }

        isPointerDown = false;
        AnimateTo(restingScale, releaseDuration, Ease.OutBack);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isPointerDown)
        {
            return;
        }

        isPointerDown = false;
        AnimateTo(restingScale, releaseDuration, Ease.OutBack);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (!CanAnimate())
        {
            return;
        }

        StopAnimation();
        scaleTween = DOTween.Sequence()
            .Append(buttonTransform.DOScale(restingScale * pressedScale, pressDuration).SetEase(Ease.OutQuad))
            .Append(buttonTransform.DOScale(restingScale, releaseDuration).SetEase(Ease.OutBack))
            .SetUpdate(true)
            .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
    }

    private bool CanAnimate()
    {
        return button != null && button.IsActive() && button.IsInteractable();
    }

    private void AnimateTo(Vector3 scale, float duration, Ease ease)
    {
        StopAnimation();
        scaleTween = buttonTransform.DOScale(scale, duration)
            .SetEase(ease)
            .SetUpdate(true)
            .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
    }

    private void StopAnimation()
    {
        if (scaleTween != null && scaleTween.IsActive())
        {
            scaleTween.Kill();
        }

        scaleTween = null;
    }

    private void CacheReferences()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (buttonTransform == null)
        {
            buttonTransform = (RectTransform)transform;
        }
    }
}
