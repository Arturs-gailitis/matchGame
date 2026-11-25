using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Plays the disappearance animation for cars matched in the waiting area.
/// </summary>
public static class WaitingAreaMatchAnimation
{
    private const float Duration = 0.35f;
    private const float FinalScale = 0.1f;
    private const Ease MatchEase = Ease.InBack;

    public static async UniTask PlayAsync(IReadOnlyList<CarInteraction> cars)
    {
        if (cars == null || cars.Count == 0)
        {
            return;
        }

        var initialScales = new Dictionary<CarInteraction, Vector3>(cars.Count);
        var canvasGroups = new Dictionary<CarInteraction, CanvasGroup>(cars.Count);

        foreach (CarInteraction car in cars)
        {
            if (car == null)
            {
                continue;
            }

            initialScales[car] = car.transform.localScale;
            canvasGroups[car] = GetOrAddCanvasGroup(car.gameObject);
        }

        Sequence sequence = DOTween.Sequence();

        foreach (KeyValuePair<CarInteraction, Vector3> entry in initialScales)
        {
            CarInteraction car = entry.Key;
            Tweener scaleTween = car.transform.DOScale(entry.Value * FinalScale, Duration).SetEase(MatchEase);
            Tweener fadeTween = canvasGroups[car].DOFade(0f, Duration).SetEase(MatchEase);
            _ = sequence.Join(scaleTween);
            _ = sequence.Join(fadeTween);
        }

        var completionSource = new UniTaskCompletionSource();
        sequence = sequence.OnComplete(() => completionSource.TrySetResult());
        await completionSource.Task;
    }

    /// <summary>Plays the match effect backwards so reset cars appear in their original cells.</summary>
    public static async UniTask PlayReverseAsync(IReadOnlyList<CarInteraction> cars, Vector3 targetScale)
    {
        if (cars == null || cars.Count == 0)
        {
            return;
        }

        Sequence sequence = DOTween.Sequence();

        foreach (CarInteraction car in cars)
        {
            if (car == null)
            {
                continue;
            }

            CanvasGroup canvasGroup = GetOrAddCanvasGroup(car.gameObject);
            car.transform.localScale = targetScale * FinalScale;
            canvasGroup.alpha = 0f;

            Tweener scaleTween = car.transform.DOScale(targetScale, Duration).SetEase(Ease.OutBack);
            Tweener fadeTween = canvasGroup.DOFade(1f, Duration).SetEase(Ease.OutBack);
            _ = sequence.Join(scaleTween);
            _ = sequence.Join(fadeTween);
        }

        var completionSource = new UniTaskCompletionSource();
        sequence = sequence.OnComplete(() => completionSource.TrySetResult());
        await completionSource.Task;
    }

    private static CanvasGroup GetOrAddCanvasGroup(GameObject target)
    {
        CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
        return canvasGroup != null ? canvasGroup : target.AddComponent<CanvasGroup>();
    }

}
