using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Creates and controls only the visual representation of a pipe obstacle.
/// </summary>
[DisallowMultipleComponent]
public sealed class PipeObstacleView : MonoBehaviour
{
    [SerializeField] private Color pipeColor = new Color(0.24f, 0.52f, 0.59f, 1f);
    [SerializeField] private Color pipeDarkColor = new Color(0.07f, 0.19f, 0.23f, 1f);
    [SerializeField] private Color pipeHighlightColor = new Color(0.55f, 0.82f, 0.84f, 0.9f);
    [SerializeField] private Color pipeCollarColor = new Color(0.16f, 0.36f, 0.42f, 1f);
    [SerializeField, Range(0.1f, 0.8f)] private float pipeThickness = 0.34f;

    private readonly List<CanvasGroup> sections = new List<CanvasGroup>();

    public IReadOnlyList<CanvasGroup> Sections => sections;

    public static PipeObstacleView Create(Transform parent, Table table, IReadOnlyList<Vector2Int> cells, string colorHex)
    {
        ConfigureObstacleRect((RectTransform)parent);

        GameObject viewObject = new GameObject("PipeVisual", typeof(RectTransform), typeof(PipeObstacleView));
        viewObject.transform.SetParent(parent, false);

        PipeObstacleView view = viewObject.GetComponent<PipeObstacleView>();
        view.ApplyColor(colorHex);
        view.Build(table, cells);
        return view;
    }

    private void ApplyColor(string colorHex)
    {
        if (string.IsNullOrWhiteSpace(colorHex))
        {
            return;
        }

        if (!ColorUtility.TryParseHtmlString(colorHex, out Color configuredColor))
        {
            Debug.LogWarning($"PipeObstacleView: '{colorHex}' is not a valid HTML color. The default color will be used.", this);
            return;
        }

        pipeColor = configuredColor;
        pipeDarkColor = Color.Lerp(configuredColor, Color.black, 0.62f);
        pipeHighlightColor = Color.Lerp(configuredColor, Color.white, 0.58f);
        pipeHighlightColor.a = configuredColor.a * 0.9f;
        pipeCollarColor = Color.Lerp(configuredColor, pipeDarkColor, 0.35f);
    }

    private static void ConfigureObstacleRect(RectTransform obstacleRect)
    {
        obstacleRect.anchorMin = Vector2.zero;
        obstacleRect.anchorMax = Vector2.one;
        obstacleRect.offsetMin = Vector2.zero;
        obstacleRect.offsetMax = Vector2.zero;
        obstacleRect.SetAsLastSibling();
    }

    public void SetVisible(bool isVisible)
    {
        gameObject.SetActive(isVisible);
    }

    private void Build(Table table, IReadOnlyList<Vector2Int> cells)
    {
        Vector2Int first = cells[0];
        int minX = first.x;
        int maxX = first.x;
        int minY = first.y;
        int maxY = first.y;

        for (int index = 1; index < cells.Count; index++)
        {
            minX = Mathf.Min(minX, cells[index].x);
            maxX = Mathf.Max(maxX, cells[index].x);
            minY = Mathf.Min(minY, cells[index].y);
            maxY = Mathf.Max(maxY, cells[index].y);
        }

        RectTransform visualRect = (RectTransform)transform;
        visualRect.anchorMin = new Vector2(0.5f, 0.5f);
        visualRect.anchorMax = new Vector2(0.5f, 0.5f);
        visualRect.pivot = new Vector2(0.5f, 0.5f);

        Vector2 start = table.GetCellCenter(minX, minY);
        Vector2 end = table.GetCellCenter(maxX, maxY);
        visualRect.anchoredPosition = (start + end) * 0.5f;

        bool isHorizontal = minY == maxY;
        float cellLength = isHorizontal ? table.CellSize.x : table.CellSize.y;
        float pipeLength = cells.Count * cellLength;
        float pipeDiameter = (isHorizontal ? table.CellSize.y : table.CellSize.x) * pipeThickness;
        visualRect.sizeDelta = isHorizontal ? new Vector2(pipeLength, pipeDiameter * 1.5f) : new Vector2(pipeDiameter * 1.5f, pipeLength);

        CreatePipeSections(isHorizontal, cells.Count, cellLength, pipeLength, pipeDiameter);
    }

    private void CreatePipeSections(bool isHorizontal, int sectionCount, float cellLength, float pipeLength, float pipeDiameter)
    {
        for (int index = 0; index < sectionCount; index++)
        {
            GameObject sectionObject = new GameObject($"PipeSection_{index}", typeof(RectTransform), typeof(CanvasGroup));
            sectionObject.transform.SetParent(transform, false);

            RectTransform sectionRect = sectionObject.GetComponent<RectTransform>();
            sectionRect.anchorMin = new Vector2(0.5f, 0.5f);
            sectionRect.anchorMax = new Vector2(0.5f, 0.5f);
            sectionRect.pivot = new Vector2(0.5f, 0.5f);
            sectionRect.sizeDelta = isHorizontal ? new Vector2(cellLength, pipeDiameter * 1.5f) : new Vector2(pipeDiameter * 1.5f, cellLength);

            float axisPosition = -pipeLength * 0.5f + (index + 0.5f) * cellLength;
            sectionRect.anchoredPosition = isHorizontal ? new Vector2(axisPosition, 0f) : new Vector2(0f, axisPosition);

            CanvasGroup canvasGroup = sectionObject.GetComponent<CanvasGroup>();
            sections.Add(canvasGroup);

            CreatePipeBody(sectionRect, isHorizontal, cellLength, pipeDiameter);
            CreatePipeCollars(sectionRect, isHorizontal, cellLength, pipeDiameter, index == sectionCount - 1);
        }
    }

    private void CreatePipeBody(Transform parent, bool isHorizontal, float length, float diameter)
    {
        Vector2 shadowSize = isHorizontal ? new Vector2(length, diameter) : new Vector2(diameter, length);
        Vector2 shadowOffset = isHorizontal ? new Vector2(0f, -diameter * 0.08f) : new Vector2(diameter * 0.08f, 0f);
        CreateImage("PipeShadow", parent, shadowSize, shadowOffset, pipeDarkColor);

        Vector2 bodySize = isHorizontal ? new Vector2(length, diameter * 0.82f) : new Vector2(diameter * 0.82f, length);
        CreateImage("PipeBody", parent, bodySize, Vector2.zero, pipeColor);

        Vector2 highlightSize = isHorizontal ? new Vector2(length, diameter * 0.16f) : new Vector2(diameter * 0.16f, length);
        Vector2 highlightOffset = isHorizontal ? new Vector2(0f, diameter * 0.23f) : new Vector2(-diameter * 0.23f, 0f);
        CreateImage("PipeHighlight", parent, highlightSize, highlightOffset, pipeHighlightColor);
    }

    private void CreatePipeCollars(Transform parent, bool isHorizontal, float cellLength, float pipeDiameter, bool includeEndCollar)
    {
        float collarWidth = Mathf.Min(cellLength * 0.14f, pipeDiameter * 0.42f);
        float collarDiameter = pipeDiameter * 1.34f;
        float startPosition = -cellLength * 0.5f + collarWidth * 0.5f;
        CreatePipeCollar(parent, isHorizontal, startPosition, collarWidth, collarDiameter, "Start");

        if (includeEndCollar)
        {
            float endPosition = cellLength * 0.5f - collarWidth * 0.5f;
            CreatePipeCollar(parent, isHorizontal, endPosition, collarWidth, collarDiameter, "End");
        }
    }

    private void CreatePipeCollar(Transform parent, bool isHorizontal, float axisPosition, float collarWidth, float collarDiameter, string suffix)
    {
        Vector2 collarSize = isHorizontal ? new Vector2(collarWidth, collarDiameter) : new Vector2(collarDiameter, collarWidth);
        Vector2 collarPosition = isHorizontal ? new Vector2(axisPosition, 0f) : new Vector2(0f, axisPosition);
        CreateImage($"PipeCollar{suffix}", parent, collarSize, collarPosition, pipeDarkColor);

        Vector2 insetSize = isHorizontal ? new Vector2(collarWidth * 0.56f, collarDiameter * 0.86f) : new Vector2(collarDiameter * 0.86f, collarWidth * 0.56f);
        CreateImage($"PipeCollarInset{suffix}", parent, insetSize, collarPosition, pipeCollarColor);
    }

    private static void CreateImage(string objectName, Transform parent, Vector2 size, Vector2 position, Color color)
    {
        GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(parent, false);

        RectTransform imageRect = imageObject.GetComponent<RectTransform>();
        imageRect.anchorMin = new Vector2(0.5f, 0.5f);
        imageRect.anchorMax = new Vector2(0.5f, 0.5f);
        imageRect.pivot = new Vector2(0.5f, 0.5f);
        imageRect.sizeDelta = size;
        imageRect.anchoredPosition = position;

        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
    }
}
