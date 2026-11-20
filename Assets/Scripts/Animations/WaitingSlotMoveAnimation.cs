using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Moves a car from the table to its reserved waiting slot along a short curved route.
/// </summary>
public static class WaitingSlotMoveAnimation
{
    private const float MinimumDuration = 0.2f;
    private const float MaximumDuration = 0.45f;
    private const float CarHeightsPerSecond = 12f;
    private const float LeadInDistanceInCarHeights = 1.05f;
    private const float ArcHeightInCarHeights = 0.35f;

    public static async UniTask PlayAsync(RectTransform carRect, RectTransform slotRect)
    {
        if (carRect == null || slotRect == null)
        {
            return;
        }

        Vector3 startPosition = carRect.position;
        Vector3 targetPosition = slotRect.TransformPoint(slotRect.rect.center);
        Vector3 forwardDirection = carRect.parent != null ? -carRect.parent.up : Vector3.down;
        float carHeight = Mathf.Max(1f, carRect.rect.height * carRect.lossyScale.y);
        float distance = Vector3.Distance(startPosition, targetPosition);
        float duration = Mathf.Clamp(distance / (carHeight * CarHeightsPerSecond), MinimumDuration, MaximumDuration);

        float leadInDistance = carHeight * LeadInDistanceInCarHeights;
        float leadInDuration = leadInDistance / (carHeight * CarHeightsPerSecond);
        Vector3 leadInPosition = startPosition + forwardDirection.normalized * leadInDistance;
        Vector3 movement = targetPosition - leadInPosition;
        Vector3 perpendicular = new Vector3(-movement.y, movement.x, 0f).normalized;
        float arcHeight = Mathf.Min(distance * 0.15f, carHeight * ArcHeightInCarHeights);
        Vector3 controlPoint = (leadInPosition + targetPosition) * 0.5f + perpendicular * arcHeight;

        carRect.SetParent(slotRect, true);
        carRect.SetAsLastSibling();

        float progress = 0f;
        Tweener curveTween = DOTween.To(
                () => progress,
                value =>
                {
                    progress = value;
                    carRect.position = GetQuadraticBezierPoint(leadInPosition, controlPoint, targetPosition, value);
                },
                1f,
                duration)
            .SetEase(Ease.OutSine);

        Sequence sequence = DOTween.Sequence();
        _ = sequence.Append(carRect.DOMove(leadInPosition, leadInDuration).SetEase(Ease.Linear));
        _ = sequence.Append(curveTween);

        var completionSource = new UniTaskCompletionSource();
        _ = sequence.SetLink(carRect.gameObject, LinkBehaviour.KillOnDestroy).OnComplete(() => completionSource.TrySetResult()).OnKill(() => completionSource.TrySetResult());

        await completionSource.Task;
    }

    private static Vector3 GetQuadraticBezierPoint(Vector3 start, Vector3 control, Vector3 end, float progress)
    {
        float remaining = 1f - progress;
        return remaining * remaining * start + 2f * remaining * progress * control + progress * progress * end;
    }
}
