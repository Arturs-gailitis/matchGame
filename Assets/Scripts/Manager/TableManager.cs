using System;
using System.Collections.Generic;
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
    [SerializeField] private TextAsset carSpriteConfig;
    [SerializeField] private Sprite[] carSprites;
    [SerializeField, Range(0.5f, 1f)] private float carScale = 0.8f;

    public event Action LevelCompleted;

    private readonly List<GameObject> spawnedCars = new List<GameObject>();
    private readonly Dictionary<Vector2Int, CarInteraction> carsByCell = new Dictionary<Vector2Int, CarInteraction>();
    private bool levelCompletionRaised;

    private static readonly Vector2Int[] AdjacentDirections =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

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

        if (table == null || waitingAreaManager == null || carPrefab == null || carSpriteConfig == null)
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

        RefreshCarStates();
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
        if (!waitingAreaManager.TryMoveCar(carInteraction))
        {
            return;
        }

        Vector2Int occupiedCell = default;
        bool cellFound = false;

        foreach (KeyValuePair<Vector2Int, CarInteraction> entry in carsByCell)
        {
            if (entry.Value == carInteraction)
            {
                occupiedCell = entry.Key;
                cellFound = true;
                break;
            }
        }

        if (cellFound)
        {
            carsByCell.Remove(occupiedCell);
        }

        carInteraction.RemovalRequested -= HandleCarRemoval;
        spawnedCars.Remove(carInteraction.gameObject);
        RefreshCarStates();

        CheckLevelCompleted();
    }

    private void CheckLevelCompleted()
    {
        if (levelCompletionRaised || spawnedCars.Count > 0 || waitingAreaManager == null || !waitingAreaManager.IsEmpty)
        {
            return;
        }

        levelCompletionRaised = true;
        LevelCompleted?.Invoke();
    }

    private void RefreshCarStates()
    {
        foreach (KeyValuePair<Vector2Int, CarInteraction> entry in carsByCell)
        {
            CarInteraction.CarState state = entry.Key.y == 0 || HasFreeAdjacentCell(entry.Key) ? CarInteraction.CarState.Removable : CarInteraction.CarState.Blocked;

            entry.Value.SetState(state);
        }
    }

    private bool HasFreeAdjacentCell(Vector2Int cell)
    {
        foreach (Vector2Int direction in AdjacentDirections)
        {
            Vector2Int adjacentCell = cell + direction;

            if (table.IsValidCell(adjacentCell.x, adjacentCell.y) && !carsByCell.ContainsKey(adjacentCell))
            {
                return true;
            }
        }

        return false;
    }

    private void ClearGrid()
    {
        foreach (GameObject car in spawnedCars)
        {
            if (car != null)
            {
                CarInteraction carInteraction = car.GetComponent<CarInteraction>();

                if (carInteraction != null)
                {
                    carInteraction.RemovalRequested -= HandleCarRemoval;
                }

                Destroy(car);
            }
        }

        spawnedCars.Clear();
        carsByCell.Clear();
    }
}
