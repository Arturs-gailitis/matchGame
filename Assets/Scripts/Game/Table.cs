using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Describes the rectangular board formed by the Table UI object.
/// Rows are indexed from bottom to top and columns from left to right.
/// </summary>
public sealed class Table : MonoBehaviour
{
    [SerializeField, Min(1)] private int columns = 7;
    [SerializeField, Min(1)] private int rows = 8;

    private RectTransform tableRect;
    private GridLayoutGroup layoutGroup;

    public int Columns => columns;
    public int Rows => rows;
    public int CellCount => columns * rows;
    public Vector2 CellSize { get; private set; }

    private void Awake()
    {
        CacheComponents();
        Recalculate();
    }

    private void OnEnable()
    {
        CacheComponents();
        Recalculate();
    }

    private void OnRectTransformDimensionsChange()
    {
        if (tableRect != null)
        {
            Recalculate();
        }
    }

    private void OnValidate()
    {
        columns = Mathf.Max(1, columns);
        rows = Mathf.Max(1, rows);

        if (isActiveAndEnabled)
        {
            CacheComponents();
            Recalculate();
        }
    }

    public bool IsValidCell(int column, int row)
    {
        return column >= 0 && column < columns && row >= 0 && row < rows;
    }

    private Rect GetCellRect(int column, int row)
    {
        if (!IsValidCell(column, row))
        {
            Debug.LogError($"Table cell ({column}, {row}) is outside the {columns}x{rows} table.", this);
            return default;
        }

        Vector2 origin = -tableRect.rect.size * 0.5f;
        return new Rect(origin + new Vector2(column * CellSize.x, row * CellSize.y), CellSize);
    }

    /// <summary>Returns the local-space center point for a cell; row 0 is the bottom row.</summary>
    public Vector2 GetCellCenter(int column, int row)
    {
        return GetCellRect(column, row).center;
    }

    private void CacheComponents()
    {
        if (tableRect == null)
        {
            tableRect = GetComponent<RectTransform>();
        }

        if (layoutGroup == null)
        {
            layoutGroup = GetComponent<GridLayoutGroup>();
        }
    }

    private void Recalculate()
    {
        if (tableRect == null || layoutGroup == null)
        {
            return;
        }

        Vector2 tableSize = tableRect.rect.size;
        CellSize = new Vector2(tableSize.x / columns, tableSize.y / rows);

        layoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        layoutGroup.constraintCount = columns;
        layoutGroup.cellSize = CellSize;
        layoutGroup.spacing = Vector2.zero;
        layoutGroup.padding = new RectOffset();
        layoutGroup.startCorner = GridLayoutGroup.Corner.UpperLeft;
        layoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
        layoutGroup.childAlignment = TextAnchor.UpperLeft;
    }
}
