using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Hides a straight line of cars until its key car is selected.
/// </summary>
[DisallowMultipleComponent]
public sealed class PipeObstacle : TableObject
{
    private TableManager tableManager;
    private CarInteraction keyCar;
    private readonly List<CarInteraction> blockedCars = new List<CarInteraction>();
    private PipeObstacleView pipeView;
    private PipeKeyView keyView;
    private bool isInitialized;
    private bool isUnlocked;

    private void Awake()
    {
        SetObjectState(ObjectState.Pipe);
    }

    public bool TryInitialize(TableManager owner, Table table, LevelPipeData pipeData, int pipeIndex)
    {
        if (pipeData == null || pipeData.keyPosition == null || pipeData.blockedPositions == null || pipeData.blockedPositions.Length == 0)
        {
            return false;
        }

        Vector2Int keyCell = new Vector2Int(pipeData.keyPosition.x, pipeData.keyPosition.y);

        if (!owner.TryGetCarAtCell(keyCell, out CarInteraction key))
        {
            Debug.LogError($"PipeObstacle: pipe {pipeIndex} key position ({keyCell.x}, {keyCell.y}) does not contain a car.", owner);
            return false;
        }

        var blockedCells = new List<Vector2Int>();
        var uniqueCells = new HashSet<Vector2Int>();

        foreach (LevelPositionData position in pipeData.blockedPositions)
        {
            if (position == null)
            {
                return false;
            }

            Vector2Int cell = new Vector2Int(position.x, position.y);

            if (cell == keyCell || !uniqueCells.Add(cell) || !owner.TryGetCarAtCell(cell, out CarInteraction blockedCar))
            {
                Debug.LogError($"PipeObstacle: pipe {pipeIndex} blocked position ({cell.x}, {cell.y}) must contain a unique non-key car.", owner);
                return false;
            }

            blockedCells.Add(cell);
            blockedCars.Add(blockedCar);
        }

        if (!AreCellsInStraightLine(blockedCells))
        {
            Debug.LogError($"PipeObstacle: pipe {pipeIndex} blocked positions must form one continuous horizontal or vertical line.", owner);
            blockedCars.Clear();
            return false;
        }

        tableManager = owner;
        keyCar = key;
        isInitialized = true;

        pipeView = PipeObstacleView.Create(transform, table, blockedCells, pipeData.color);
        keyView = PipeKeyView.Create(keyCar.transform, owner.PipeKeySprite, pipeData.color);

        keyCar.RemovalRequested += HandleKeySelected;
        tableManager.RegisterPipe(keyCar, blockedCars);
        return true;
    }

    public void Dispose()
    {
        if (!isInitialized)
        {
            return;
        }

        if (keyCar != null)
        {
            keyCar.RemovalRequested -= HandleKeySelected;
        }

        if (keyView != null)
        {
            Destroy(keyView.gameObject);
            keyView = null;
        }

        isInitialized = false;
    }

    public static bool AreCellsInStraightLine(IReadOnlyList<Vector2Int> cells)
    {
        if (cells == null || cells.Count == 0)
        {
            return false;
        }

        int x = cells[0].x;
        int y = cells[0].y;
        bool sameColumn = true;
        bool sameRow = true;

        for (int index = 1; index < cells.Count; index++)
        {
            sameColumn &= cells[index].x == x;
            sameRow &= cells[index].y == y;
        }

        if (!sameColumn && !sameRow)
        {
            return false;
        }

        int minimum = sameRow ? cells[0].x : cells[0].y;
        int maximum = minimum;

        for (int index = 1; index < cells.Count; index++)
        {
            int coordinate = sameRow ? cells[index].x : cells[index].y;
            minimum = Mathf.Min(minimum, coordinate);
            maximum = Mathf.Max(maximum, coordinate);
        }

        return maximum - minimum + 1 == cells.Count;
    }

    private void HandleKeySelected(CarInteraction selectedCar)
    {
        if (isUnlocked || selectedCar != keyCar)
        {
            return;
        }

        isUnlocked = true;
        keyCar.RemovalRequested -= HandleKeySelected;

        if (keyView != null)
        {
            keyView.SetVisible(false);
        }

        PlayUnlockAnimation().Forget();
    }

    private async UniTaskVoid PlayUnlockAnimation()
    {
        Vector3 targetCarScale = GetBlockedCarScale();
        await PipeUnlockAnimation.PlayAsync(pipeView != null ? pipeView.Sections : null);

        if (this == null || tableManager == null)
        {
            return;
        }

        if (pipeView != null)
        {
            pipeView.SetVisible(false);
        }

        List<CarInteraction> revealedCars = tableManager.UnlockPipe(keyCar, blockedCars);
        await WaitingAreaMatchAnimation.PlayReverseAsync(revealedCars, targetCarScale);

        if (this != null && tableManager != null)
        {
            tableManager.CompletePipeUnlock(revealedCars);
        }
    }

    private Vector3 GetBlockedCarScale()
    {
        foreach (CarInteraction car in blockedCars)
        {
            if (car != null)
            {
                return car.transform.localScale;
            }
        }

        return Vector3.one;
    }

    private void OnDestroy()
    {
        Dispose();
    }
}
