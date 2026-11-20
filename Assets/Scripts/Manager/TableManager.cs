using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds the cars for a level inside the Table child of the Grid prefab.
/// </summary>
public sealed class TableManager : MonoBehaviour
{
    [SerializeField] private Table table;
    [SerializeField] private WaitingAreaManager waitingAreaManager;
    [SerializeField] private GameObject carPrefab;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private TextAsset carSpriteConfig;
    [SerializeField] private Sprite[] carSprites;
    [SerializeField, Range(0.5f, 1f)] private float carScale = 0.8f;
    [SerializeField, Range(0f, 0.2f)] private float wallConnectionOverlap = 0f;

    public event Action LevelCompleted;

    private readonly List<GameObject> spawnedCars = new List<GameObject>();
    private readonly List<GameObject> spawnedWalls = new List<GameObject>();
    private readonly Dictionary<Vector2Int, CarInteraction> carsByCell = new Dictionary<Vector2Int, CarInteraction>();
    private readonly Dictionary<Vector2Int, WallObstacle> wallsByCell = new Dictionary<Vector2Int, WallObstacle>();
    private readonly Dictionary<Vector2Int, RectTransform> wallRectsByCell = new Dictionary<Vector2Int, RectTransform>();
    private bool levelCompletionRaised;
    private bool isCarRemovalInProgress;

    public void SetLevel(int level)
    {
        var levelNumber = Mathf.Max(1, level);

        levelCompletionRaised = false;
        BuildGrid(levelNumber);
    }

    private void OnEnable()
    {
        if (waitingAreaManager != null)
        {
            waitingAreaManager.BecameEmpty += CheckLevelCompleted;
        }
    }

    private void OnDisable()
    {
        if (waitingAreaManager != null)
        {
            waitingAreaManager.BecameEmpty -= CheckLevelCompleted;
        }
    }

    private void BuildGrid(int levelNumber)
    {
        ClearGrid();

        if (table == null || waitingAreaManager == null || carPrefab == null || wallPrefab == null || carSpriteConfig == null)
        {
            Debug.LogError("TableManager: something is missing.", this);
            return;
        }

        GridLayoutGroup layoutGroup = table.GetComponent<GridLayoutGroup>();

        if (layoutGroup != null)
        {
            layoutGroup.enabled = false;
        }

        LevelData level = LevelJsonReader.Read(levelNumber);
        
        if (level == null)
        {
            return;
        }

        Dictionary<int, CarSpriteConfig> configsByType = CreateConfigLookup();
        Dictionary<string, Sprite> spritesByName = CreateSpriteLookup();

        foreach (LevelCarData car in level.cars)
        {
            if (!table.IsValidCell(car.x, car.y))
            {
                Debug.LogError($"TableManager: car {car.id} has invalid cell ({car.x}, {car.y}).", this);
                continue;
            }

            if (!configsByType.TryGetValue(car.type, out CarSpriteConfig config))
            {
                Debug.LogError($"TableManager: no CarSpriteConfig exists for car type {car.type}.", this);
                continue;
            }

            Sprite sprite = FindSprite(config.spriteName, spritesByName);

            if (sprite == null)
            {
                Debug.LogError($"TableManager: no sprite was assigned for '{config.spriteName}'.", this);
                continue;
            }

            CreateCar(car, sprite);
        }

        CreateWallsForEmptyCells();
        RefreshCarStates();
    }

    private void CreateWallsForEmptyCells()
    {
        for (int row = 0; row < table.Rows; row++)
        {
            for (int column = 0; column < table.Columns; column++)
            {
                Vector2Int cell = new Vector2Int(column, row);

                if (carsByCell.ContainsKey(cell))
                {
                    continue;
                }

                GameObject wallObject = Instantiate(wallPrefab, table.transform, false);
                wallObject.name = $"Wall_{column}_{row}";
                wallObject.transform.SetAsFirstSibling();
                spawnedWalls.Add(wallObject);

                WallObstacle wallObstacle = wallObject.GetComponent<WallObstacle>();

                wallObstacle.SetObjectState(ObjectState.Wall);
                wallsByCell.Add(cell, wallObstacle);

                RectTransform wallRect = wallObject.GetComponent<RectTransform>();

                wallRect.anchorMin = new Vector2(0.5f, 0.5f);
                wallRect.anchorMax = new Vector2(0.5f, 0.5f);
                wallRect.pivot = new Vector2(0.5f, 0.5f);
                wallRect.sizeDelta = table.CellSize;
                wallRect.anchoredPosition = table.GetCellCenter(column, row);
                wallRectsByCell.Add(cell, wallRect);
            }
        }

        WallTextureContinuity.ApplyWallConnections(table, wallsByCell, wallRectsByCell, wallConnectionOverlap);
    }

    private Dictionary<int, CarSpriteConfig> CreateConfigLookup()
    {
        var lookup = new Dictionary<int, CarSpriteConfig>();

        foreach (CarSpriteConfig config in CarSpriteReader.Read(carSpriteConfig.text))
        {
            lookup[config.type] = config;
        }

        return lookup;
    }

    private Dictionary<string, Sprite> CreateSpriteLookup()
    {
        var lookup = new Dictionary<string, Sprite>();

        foreach (Sprite sprite in carSprites)
        {
            lookup[sprite.name] = sprite;
        }

        return lookup;
    }

    private static Sprite FindSprite(string spriteName, Dictionary<string, Sprite> spritesByName)
    {
        Sprite sprite = null;

        foreach (KeyValuePair<string, Sprite> entry in spritesByName)
        {
            if (entry.Key.StartsWith(spriteName + "_"))
            {
                sprite = entry.Value;
            }
        }

        return sprite;
    }

    private void CreateCar(LevelCarData car, Sprite sprite)
    {
        UnityEngine.Object instantiatedObject = Instantiate(carPrefab, table.transform, false);
        GameObject carObject = instantiatedObject as GameObject;

        carObject.name = $"Car_{car.id}";
        spawnedCars.Add(carObject);

        RectTransform carRect = carObject.GetComponent<RectTransform>();
        Image image = carObject.GetComponentInChildren<Image>(true);
        CarInteraction carInteraction = carObject.GetComponent<CarInteraction>();
        carInteraction.SetCarType(car.type);

        Vector2Int cell = new Vector2Int(car.x, car.y);

        carsByCell.Add(cell, carInteraction);
        carInteraction.SetObjectState(ObjectState.Car);
        carInteraction.RemovalRequested += HandleCarRemoval;

        carRect.anchorMin = new Vector2(0.5f, 0.5f);
        carRect.anchorMax = new Vector2(0.5f, 0.5f);
        carRect.pivot = new Vector2(0.5f, 0.5f);
        carRect.sizeDelta = table.CellSize;
        
        // The JSON coordinate contract is x = column and y = row.
        carRect.anchoredPosition = table.GetCellCenter(column: car.x, row: car.y);
        carRect.localScale = Vector3.one * carScale;

        image.sprite = sprite;
        image.preserveAspect = false;
        image.raycastTarget = true;
    }

    private void HandleCarRemoval(CarInteraction carInteraction)
    {
        if (isCarRemovalInProgress || carInteraction == null || waitingAreaManager == null || !waitingAreaManager.HasFreeSlot)
        {
            return;
        }

        if (!TryGetCarCell(carInteraction, out Vector2Int occupiedCell) || !TryGetRemovalPath(occupiedCell, carInteraction, out List<Vector2Int> path))
        {
            RefreshCarStates();
            return;
        }

        isCarRemovalInProgress = true;
        carInteraction.SetState(CarState.Blocked);
        RemoveCarAfterAnimation(carInteraction, occupiedCell, path).Forget();
    }

    private async UniTask RemoveCarAfterAnimation(CarInteraction carInteraction, Vector2Int occupiedCell, IReadOnlyList<Vector2Int> path)
    {
        RectTransform carRect = carInteraction.GetComponent<RectTransform>();
        await CarRemovalAnimation.PlayAsync(carRect, table, path);

        if (this == null || carInteraction == null)
        {
            return;
        }

        bool movedToWaitingArea = await waitingAreaManager.TryMoveCar(carInteraction);

        if (this == null || carInteraction == null)
        {
            return;
        }

        if (!movedToWaitingArea)
        {
            isCarRemovalInProgress = false;
            RefreshCarStates();
            return;
        }

        carsByCell.Remove(occupiedCell);
        carInteraction.RemovalRequested -= HandleCarRemoval;
        spawnedCars.Remove(carInteraction.gameObject);
        isCarRemovalInProgress = false;
        RefreshCarStates();

        CheckLevelCompleted();
    }

    private void CheckLevelCompleted()
    {
        if (levelCompletionRaised || HasRemainingCarObjects() || waitingAreaManager == null || !waitingAreaManager.IsEmpty)
        {
            return;
        }

        levelCompletionRaised = true;
        LevelCompleted?.Invoke();
    }

    private bool HasRemainingCarObjects()
    {
        foreach (GameObject spawnedObject in spawnedCars)
        {
            if (spawnedObject == null)
            {
                continue;
            }

            CarInteraction carInteraction = spawnedObject.GetComponent<CarInteraction>();

            if (carInteraction != null && carInteraction.ObjectState == ObjectState.Car)
            {
                return true;
            }
        }

        return false;
    }

    private void RefreshCarStates()
    {
        foreach (KeyValuePair<Vector2Int, CarInteraction> entry in carsByCell)
        {
            CarState state = TryGetRemovalPath(entry.Key, entry.Value, out _) ? CarState.Removable : CarState.Blocked;

            entry.Value.SetState(state);
        }
    }

    private bool TryGetRemovalPath( Vector2Int startCell, CarInteraction movingCar, out List<Vector2Int> path)
    {
        return CarRemovalAnimation.TryFindPath(startCell, table, cell => IsCellBlocked(cell, movingCar), out path);
    }

    private bool IsCellBlocked(Vector2Int cell, CarInteraction movingCar)
    {
        if (wallsByCell.TryGetValue(cell, out WallObstacle wall) && wall.ObjectState == ObjectState.Wall)
        {
            return true;
        }

        return carsByCell.TryGetValue(cell, out CarInteraction car) && car != movingCar && car.ObjectState == ObjectState.Car;
    }

    private bool TryGetCarCell(CarInteraction carInteraction, out Vector2Int cell)
    {
        foreach (KeyValuePair<Vector2Int, CarInteraction> entry in carsByCell)
        {
            if (entry.Value != carInteraction)
            {
                continue;
            }

            cell = entry.Key;
            return true;
        }

        cell = default;
        return false;
    }

    private void ClearGrid()
    {
        for (int index = spawnedCars.Count - 1; index >= 0; index--)
        {
            GameObject spawnedObject = spawnedCars[index];

            if (spawnedObject != null)
            {
                CarInteraction carInteraction = spawnedObject.GetComponent<CarInteraction>();

                if (carInteraction != null)
                {
                    carInteraction.RemovalRequested -= HandleCarRemoval;
                }

                Destroy(spawnedObject);
            }
        }

        spawnedCars.Clear();

        for (int index = spawnedWalls.Count - 1; index >= 0; index--)
        {
            GameObject spawnedWall = spawnedWalls[index];

            if (spawnedWall != null)
            {
                Destroy(spawnedWall);
            }
        }

        spawnedWalls.Clear();
        carsByCell.Clear();
        wallsByCell.Clear();
        wallRectsByCell.Clear();
        levelCompletionRaised = false;
        isCarRemovalInProgress = false;
    }

}
