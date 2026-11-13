using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds the cars for a level inside the Table child of the Grid prefab.
/// </summary>
public sealed class GridManager : MonoBehaviour
{
    [SerializeField] private Table table;
    [SerializeField] private GameObject carPrefab;
    [SerializeField, Min(1)] private int levelNumber = 1;
    [SerializeField] private TextAsset carSpriteConfig;
    [SerializeField] private Sprite[] carSprites;
    [SerializeField, Range(0.5f, 1f)] private float carScale = 0.8f;

    private readonly List<GameObject> spawnedCars = new List<GameObject>();

    private void Start()
    {
        BuildGrid();
    }

    private void BuildGrid()
    {
        ClearGrid();

        if (table == null || carPrefab == null || carSpriteConfig == null)
        {
            Debug.LogError("GridManager: something is missing.", this);
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
                Debug.LogError($"GridManager: car {car.id} has invalid cell ({car.x}, {car.y}).", this);
                continue;
            }

            if (!configsByType.TryGetValue(car.type, out CarSpriteConfig config))
            {
                Debug.LogError($"GridManager: no CarSpriteConfig exists for car type {car.type}.", this);
                continue;
            }

            Sprite sprite = FindSprite(config.spriteName, spritesByName);

            if (sprite == null)
            {
                Debug.LogError($"GridManager: no sprite was assigned for '{config.spriteName}'.", this);
                continue;
            }

            CreateCar(car, sprite);
        }
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
        Object instantiatedObject = Instantiate(carPrefab as Object, table.transform, false);
        GameObject carObject = instantiatedObject as GameObject;

        carObject.name = $"Car_{car.id}";
        spawnedCars.Add(carObject);

        RectTransform carRect = carObject.GetComponent<RectTransform>();
        Image image = carObject.GetComponent<Image>();

        carRect.anchorMin = new Vector2(0.5f, 0.5f);
        carRect.anchorMax = new Vector2(0.5f, 0.5f);
        carRect.pivot = new Vector2(0.5f, 0.5f);
        carRect.sizeDelta = table.CellSize;
        
        // The JSON coordinate contract is x = column and y = row.
        carRect.anchoredPosition = table.GetCellCenter(column: car.x, row: car.y);
        carRect.localScale = Vector3.one * carScale;

        image.sprite = sprite;
        image.preserveAspect = true;
        image.raycastTarget = false;
    }

    private void ClearGrid()
    {
        foreach (GameObject car in spawnedCars)
        {
            if (car != null)
            {
                Destroy(car);
            }
        }

        spawnedCars.Clear();
    }
}
