using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

/// <summary>
/// Keeps the wall texture on one shared grid even when a wall tile is extended
/// toward several neighbours. This prevents the sprite from being stretched at
/// corners while still using the original wall sprite.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Image))]
public sealed class WallTextureContinuity : BaseMeshEffect
{
    private Rect patternRect = new Rect(0f, 0f, 1f, 1f);

    public static void ApplyWallConnections(Table table, IReadOnlyDictionary<Vector2Int, TableObject> objectsByCell, float wallConnectionOverlap)
    {
        if (table == null || objectsByCell == null)
        {
            return;
        }

        float horizontalOverlap = table.CellSize.x * wallConnectionOverlap;
        float verticalOverlap = table.CellSize.y * wallConnectionOverlap;

        foreach (KeyValuePair<Vector2Int, TableObject> entry in objectsByCell)
        {
            Vector2Int cell = entry.Key;
            TableObject tableObject = entry.Value;

            if (tableObject == null || tableObject.ObjectState != ObjectState.Wall)
            {
                continue;
            }

            RectTransform wallRect = tableObject.GetComponent<RectTransform>();
            bool connectsLeft = IsWall(objectsByCell, cell + Vector2Int.left);
            bool connectsRight = IsWall(objectsByCell, cell + Vector2Int.right);
            bool connectsDown = IsWall(objectsByCell, cell + Vector2Int.down);
            bool connectsUp = IsWall(objectsByCell, cell + Vector2Int.up);

            float leftExtension = connectsLeft ? horizontalOverlap : 0f;
            float rightExtension = connectsRight ? horizontalOverlap : 0f;
            float downExtension = connectsDown ? verticalOverlap : 0f;
            float upExtension = connectsUp ? verticalOverlap : 0f;

            Vector2 wallSize = new Vector2(table.CellSize.x + leftExtension + rightExtension, table.CellSize.y + downExtension + upExtension);

            wallRect.sizeDelta = wallSize;
            wallRect.anchoredPosition = table.GetCellCenter(cell.x, cell.y) + new Vector2((rightExtension - leftExtension) * 0.5f, (upExtension - downExtension) * 0.5f);

            WallTextureContinuity textureContinuity = wallRect.GetComponent<WallTextureContinuity>();

            if (textureContinuity == null)
            {
                textureContinuity = wallRect.gameObject.AddComponent<WallTextureContinuity>();
            }

            textureContinuity.SetPatternRect(new Rect(cell.x - leftExtension / table.CellSize.x, cell.y - downExtension / table.CellSize.y, wallSize.x / table.CellSize.x, wallSize.y / table.CellSize.y));
        }
    }

    private static bool IsWall(IReadOnlyDictionary<Vector2Int, TableObject> objectsByCell, Vector2Int cell)
    {
        return objectsByCell.TryGetValue(cell, out TableObject tableObject) &&
               tableObject != null &&
               tableObject.ObjectState == ObjectState.Wall;
    }

    public void SetPatternRect(Rect value)
    {
        patternRect = value;

        if (graphic != null)
        {
            graphic.SetVerticesDirty();
        }
    }

    public override void ModifyMesh(VertexHelper vertexHelper)
    {
        if (!IsActive() || vertexHelper.currentVertCount == 0)
        {
            return;
        }

        Image image = graphic as Image;
        Sprite sprite = image != null ? image.overrideSprite : null;

        if (sprite == null)
        {
            return;
        }

        Rect localRect = graphic.rectTransform.rect;

        if (Mathf.Approximately(localRect.width, 0f) || Mathf.Approximately(localRect.height, 0f))
        {
            return;
        }

        Vector4 spriteUv = DataUtility.GetOuterUV(sprite);
        UIVertex vertex = new UIVertex();

        for (int index = 0; index < vertexHelper.currentVertCount; index++)
        {
            vertexHelper.PopulateUIVertex(ref vertex, index);

            float localU = Mathf.InverseLerp(localRect.xMin, localRect.xMax, vertex.position.x);
            float localV = Mathf.InverseLerp(localRect.yMin, localRect.yMax, vertex.position.y);
            float patternU = patternRect.x + localU * patternRect.width;
            float patternV = patternRect.y + localV * patternRect.height;

            vertex.uv0 = new Vector2(Mathf.LerpUnclamped(spriteUv.x, spriteUv.z, patternU), Mathf.LerpUnclamped(spriteUv.y, spriteUv.w, patternV));

            vertexHelper.SetUIVertex(vertex, index);
        }
    }
}
