using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds and manages all objects occupying cells in the level table.
/// </summary>
public sealed class TableManager : MonoBehaviour
{
    private static readonly Vector2Int[] AdjacentDirections =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    [SerializeField] private Table table;
    [SerializeField] private WaitingAreaManager waitingAreaManager;
    [SerializeField] private GameObject carPrefab;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private TextAsset carSpriteConfig;
    [SerializeField] private Sprite[] carSprites;
    [SerializeField, Range(0.5f, 1f)] private float carScale = 0.8f;
    [SerializeField, Range(0f, 0.2f)] private float wallConnectionOverlap = 0f;

    public event Action LevelCompleted;

    private readonly Dictionary<Vector2Int, TableObject> objectsByCell = new Dictionary<Vector2Int, TableObject>();
    private readonly Dictionary<CarInteraction, Vector2Int> originalCellsByCar = new Dictionary<CarInteraction, Vector2Int>();
    private readonly HashSet<CarInteraction> powerUpLockedNeighbors = new HashSet<CarInteraction>();
    private bool levelCompletionRaised;
    private bool isCarRemovalInProgress;

    public bool IsCarRemovalInProgress => isCarRemovalInProgress;

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

                if (objectsByCell.ContainsKey(cell))
                {
                    continue;
                }

                GameObject wallObject = Instantiate(wallPrefab, table.transform, false);
                wallObject.name = $"Wall_{column}_{row}";
                wallObject.transform.SetAsFirstSibling();
                WallObstacle wallObstacle = wallObject.GetComponent<WallObstacle>();

                wallObstacle.SetObjectState(ObjectState.Wall);
                objectsByCell.Add(cell, wallObstacle);

                RectTransform wallRect = wallObject.GetComponent<RectTransform>();

                wallRect.anchorMin = new Vector2(0.5f, 0.5f);
                wallRect.anchorMax = new Vector2(0.5f, 0.5f);
                wallRect.pivot = new Vector2(0.5f, 0.5f);
                wallRect.sizeDelta = table.CellSize;
                wallRect.anchoredPosition = table.GetCellCenter(column, row);
            }
        }

        WallTextureContinuity.ApplyWallConnections(table, objectsByCell, wallConnectionOverlap);
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
        RectTransform carRect = carObject.GetComponent<RectTransform>();
        Image image = carObject.GetComponentInChildren<Image>(true);
        CarInteraction carInteraction = carObject.GetComponent<CarInteraction>();
        carInteraction.SetCarType(car.type);

        Vector2Int cell = new Vector2Int(car.x, car.y);

        objectsByCell.Add(cell, carInteraction);
        originalCellsByCar.Add(carInteraction, cell);
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

        objectsByCell.Remove(occupiedCell);
        carInteraction.RemovalRequested -= HandleCarRemoval;
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
        foreach (TableObject tableObject in objectsByCell.Values)
        {
            if (tableObject != null && tableObject.ObjectState == ObjectState.Car)
            {
                return true;
            }
        }

        return false;
    }

    public List<CarInteraction> TakeNearestCarsForPowerUp(int carType, int count, Vector3 referencePosition)
    {
        var matchingCars = new List<CarInteraction>();

        if (count <= 0)
        {
            return matchingCars;
        }

        foreach (TableObject tableObject in objectsByCell.Values)
        {
            if (tableObject is CarInteraction car && car.CarType == carType)
            {
                matchingCars.Add(car);
            }
        }

        matchingCars.Sort((first, second) =>
        {
            float firstDistance = (first.transform.position - referencePosition).sqrMagnitude;
            float secondDistance = (second.transform.position - referencePosition).sqrMagnitude;
            return firstDistance.CompareTo(secondDistance);
        });

        if (matchingCars.Count > count)
        {
            matchingCars.RemoveRange(count, matchingCars.Count - count);
        }

        foreach (CarInteraction car in matchingCars)
        {
            if (!TryGetCarCell(car, out Vector2Int cell))
            {
                continue;
            }

            if (IsSurroundedByCarsOrWalls(cell))
            {
                RememberBlockedAdjacentCars(cell);
            }

            objectsByCell.Remove(cell);
            originalCellsByCar.Remove(car);
            car.RemovalRequested -= HandleCarRemoval;
            car.SetState(CarState.Matched);
        }

        return matchingCars;
    }

    public async UniTask ResetWaitingCarsForPowerUp()
    {
        if (isCarRemovalInProgress || waitingAreaManager == null)
        {
            return;
        }

        List<CarInteraction> waitingCars = waitingAreaManager.GetWaitingCars();
        waitingCars.RemoveAll(car => car == null || !originalCellsByCar.ContainsKey(car));

        if (waitingCars.Count == 0)
        {
            return;
        }

        isCarRemovalInProgress = true;

        foreach (CarInteraction car in waitingCars)
        {
            car.SetState(CarState.Blocked);
        }

        await WaitingAreaMatchAnimation.PlayAsync(waitingCars);

        if (this == null)
        {
            return;
        }

        foreach (CarInteraction car in waitingCars)
        {
            if (car == null)
            {
                continue;
            }

            Vector2Int originalCell = originalCellsByCar[car];

            if (objectsByCell.ContainsKey(originalCell))
            {
                car.GetComponent<RectTransform>().localScale = Vector3.one * carScale;
                car.SetState(CarState.Waiting);
                continue;
            }

            RectTransform carRect = car.GetComponent<RectTransform>();
            carRect.SetParent(table.transform, false);
            carRect.anchorMin = new Vector2(0.5f, 0.5f);
            carRect.anchorMax = new Vector2(0.5f, 0.5f);
            carRect.pivot = new Vector2(0.5f, 0.5f);
            carRect.sizeDelta = table.CellSize;
            carRect.anchoredPosition = table.GetCellCenter(originalCell.x, originalCell.y);
            carRect.SetAsLastSibling();

            objectsByCell.Add(originalCell, car);
            car.RemovalRequested -= HandleCarRemoval;
            car.RemovalRequested += HandleCarRemoval;
        }

        waitingAreaManager.CompactAfterReset();
        await WaitingAreaMatchAnimation.PlayReverseAsync(waitingCars, Vector3.one * carScale);

        if (this == null)
        {
            return;
        }

        isCarRemovalInProgress = false;
        RefreshCarStates();
    }

    public bool TryReshuffleCarsForPowerUp()
    {
        if (isCarRemovalInProgress)
        {
            return false;
        }

        var carCells = new List<Vector2Int>();
        var cars = new List<CarInteraction>();

        foreach (KeyValuePair<Vector2Int, TableObject> entry in objectsByCell)
        {
            if (entry.Value is CarInteraction car && car.ObjectState == ObjectState.Car)
            {
                carCells.Add(entry.Key);
                cars.Add(car);
            }
        }

        if (cars.Count < 2)
        {
            return false;
        }

        isCarRemovalInProgress = true;

        foreach (CarInteraction car in cars)
        {
            car.SetState(CarState.Blocked);
        }

        ReshuffleCarsAfterAnimation(carCells, cars).Forget();
        return true;
    }

    private async UniTask ReshuffleCarsAfterAnimation(List<Vector2Int> carCells, List<CarInteraction> cars)
    {
        await WaitingAreaMatchAnimation.PlayAsync(cars);

        if (this == null)
        {
            return;
        }

        for (int index = cars.Count - 1; index > 0; index--)
        {
            int swapIndex = UnityEngine.Random.Range(0, index + 1);
            CarInteraction temporary = cars[index];
            cars[index] = cars[swapIndex];
            cars[swapIndex] = temporary;
        }

        bool anyCarMoved = false;

        for (int index = 0; index < cars.Count; index++)
        {
            if (objectsByCell[carCells[index]] != cars[index])
            {
                anyCarMoved = true;
                break;
            }
        }

        if (!anyCarMoved)
        {
            CarInteraction temporary = cars[0];
            cars[0] = cars[1];
            cars[1] = temporary;
        }

        foreach (Vector2Int cell in carCells)
        {
            objectsByCell.Remove(cell);
        }

        for (int index = 0; index < cars.Count; index++)
        {
            Vector2Int cell = carCells[index];
            CarInteraction car = cars[index];
            objectsByCell.Add(cell, car);

            RectTransform carRect = car.GetComponent<RectTransform>();
            carRect.anchoredPosition = table.GetCellCenter(cell.x, cell.y);
            carRect.SetAsLastSibling();
        }

        await WaitingAreaMatchAnimation.PlayReverseAsync(cars, Vector3.one * carScale);

        if (this == null)
        {
            return;
        }

        isCarRemovalInProgress = false;
        RefreshCarStates();
    }

    public void NotifyPowerUpRemovalCompleted()
    {
        RefreshCarStates();

        foreach (CarInteraction car in powerUpLockedNeighbors)
        {
            if (car != null && TryGetCarCell(car, out _))
            {
                car.SetState(CarState.Blocked);
            }
        }

        powerUpLockedNeighbors.Clear();
        CheckLevelCompleted();
    }

    private bool IsSurroundedByCarsOrWalls(Vector2Int cell)
    {
        foreach (Vector2Int direction in AdjacentDirections)
        {
            Vector2Int adjacentCell = cell + direction;

            if (!table.IsValidCell(adjacentCell.x, adjacentCell.y) || !objectsByCell.TryGetValue(adjacentCell, out TableObject adjacentObject) || adjacentObject == null || (adjacentObject.ObjectState != ObjectState.Car && adjacentObject.ObjectState != ObjectState.Wall))
            {
                return false;
            }
        }

        return true;
    }

    private void RememberBlockedAdjacentCars(Vector2Int cell)
    {
        foreach (Vector2Int direction in AdjacentDirections)
        {
            Vector2Int adjacentCell = cell + direction;

            if (objectsByCell.TryGetValue(adjacentCell, out TableObject adjacentObject) && adjacentObject is CarInteraction adjacentCar && adjacentCar.State == CarState.Blocked)
            {
                powerUpLockedNeighbors.Add(adjacentCar);
            }
        }
    }

    private void RefreshCarStates()
    {
        foreach (KeyValuePair<Vector2Int, TableObject> entry in objectsByCell)
        {
            if (entry.Value == null || entry.Value.ObjectState != ObjectState.Car || !(entry.Value is CarInteraction carInteraction))
            {
                continue;
            }

            CarState state = TryGetRemovalPath(entry.Key, carInteraction, out _) ? CarState.Removable : CarState.Blocked;

            carInteraction.SetState(state);
        }
    }

    private bool TryGetRemovalPath(Vector2Int startCell, CarInteraction movingCar, out List<Vector2Int> path)
    {
        return CarRemovalAnimation.TryFindPath(startCell, table, cell => IsCellBlocked(cell, movingCar), out path);
    }

    private bool IsCellBlocked(Vector2Int cell, CarInteraction movingCar)
    {
        if (!objectsByCell.TryGetValue(cell, out TableObject tableObject) || tableObject == null)
        {
            return false;
        }

        return tableObject.ObjectState == ObjectState.Wall ||
               (tableObject.ObjectState == ObjectState.Car && tableObject != movingCar);
    }

    private bool TryGetCarCell(CarInteraction carInteraction, out Vector2Int cell)
    {
        foreach (KeyValuePair<Vector2Int, TableObject> entry in objectsByCell)
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
        foreach (TableObject tableObject in objectsByCell.Values)
        {
            if (tableObject == null)
            {
                continue;
            }

            if (tableObject.ObjectState == ObjectState.Car && tableObject is CarInteraction carInteraction)
            {
                carInteraction.RemovalRequested -= HandleCarRemoval;
            }

            Destroy(tableObject.gameObject);
        }

        objectsByCell.Clear();
        originalCellsByCar.Clear();
        powerUpLockedNeighbors.Clear();
        levelCompletionRaised = false;
        isCarRemovalInProgress = false;
    }

}
