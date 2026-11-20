using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Finds a clear route to the bottom row and animates a car through its cells.
/// </summary>
public static class CarRemovalAnimation
{
    private const float SecondsPerCell = 0.12f;
    private const float MinimumStepDuration = 0.07f;

    private static readonly Vector2Int[] SearchDirections =
    {
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right,
        Vector2Int.up
    };

    public static bool TryFindPath(Vector2Int startCell, Table table, Func<Vector2Int, bool> isBlocked, out List<Vector2Int> path)
    {
        path = null;

        if (table == null || isBlocked == null || !table.IsValidCell(startCell.x, startCell.y))
        {
            return false;
        }

        var cellsToVisit = new Queue<Vector2Int>();
        var previousCells = new Dictionary<Vector2Int, Vector2Int>();

        cellsToVisit.Enqueue(startCell);
        previousCells.Add(startCell, startCell);

        while (cellsToVisit.Count > 0)
        {
            Vector2Int currentCell = cellsToVisit.Dequeue();

            if (currentCell.y == 0)
            {
                path = BuildPath(startCell, currentCell, previousCells);
                return true;
            }

            foreach (Vector2Int direction in SearchDirections)
            {
                Vector2Int nextCell = currentCell + direction;

                if (!table.IsValidCell(nextCell.x, nextCell.y) || previousCells.ContainsKey(nextCell) || isBlocked(nextCell))
                {
                    continue;
                }

                previousCells.Add(nextCell, currentCell);
                cellsToVisit.Enqueue(nextCell);
            }
        }

        return false;
    }

    public static async UniTask PlayAsync(RectTransform carRect, Table table, IReadOnlyList<Vector2Int> path)
    {
        if (carRect == null || table == null || path == null || path.Count <= 1)
        {
            return;
        }

        float stepDuration = Mathf.Max(MinimumStepDuration, SecondsPerCell);
        Sequence sequence = DOTween.Sequence();

        for (int index = 1; index < path.Count; index++)
        {
            Vector2 targetPosition = table.GetCellCenter(path[index].x, path[index].y);
            _ = sequence.Append(carRect.DOAnchorPos(targetPosition, stepDuration).SetEase(Ease.Linear));
        }

        var completionSource = new UniTaskCompletionSource();
        _ = sequence.SetLink(carRect.gameObject, LinkBehaviour.KillOnDestroy).OnComplete(() => completionSource.TrySetResult()).OnKill(() => completionSource.TrySetResult());

        await completionSource.Task;
    }

    private static List<Vector2Int> BuildPath(Vector2Int startCell, Vector2Int destinationCell, IReadOnlyDictionary<Vector2Int, Vector2Int> previousCells)
    {
        var path = new List<Vector2Int>();
        Vector2Int currentCell = destinationCell;

        while (currentCell != startCell)
        {
            path.Add(currentCell);
            currentCell = previousCells[currentCell];
        }

        path.Add(startCell);
        path.Reverse();
        return path;
    }
}
