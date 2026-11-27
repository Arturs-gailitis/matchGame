using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays a colored key above the car that unlocks a pipe obstacle.
/// </summary>
[DisallowMultipleComponent]
public sealed class PipeKeyView : MonoBehaviour
{
    private static readonly Color DefaultKeyColor = new Color(0.24f, 0.52f, 0.59f, 1f);

    public static PipeKeyView Create(Transform carTransform, Sprite keySprite, string colorHex)
    {
        RectTransform carRect = (RectTransform)carTransform;

        GameObject keyObject = new GameObject("PipeKey", typeof(RectTransform), typeof(Image), typeof(PipeKeyView));
        keyObject.transform.SetParent(carTransform, false);
        keyObject.transform.SetAsLastSibling();

        PipeKeyView view = keyObject.GetComponent<PipeKeyView>();
        view.Build(carRect, keySprite, ResolveColor(colorHex));
        return view;
    }

    public void SetVisible(bool isVisible)
    {
        gameObject.SetActive(isVisible);
    }

    private void Build(RectTransform carRect, Sprite keySprite, Color keyColor)
    {
        float iconBaseSize = Mathf.Min(carRect.rect.width, carRect.rect.height);
        float iconWidth = iconBaseSize * 1.5f;
        float iconHeight = iconBaseSize * 1.05f;

        RectTransform keyRect = (RectTransform)transform;
        keyRect.anchorMin = new Vector2(0.5f, 0.5f);
        keyRect.anchorMax = new Vector2(0.5f, 0.5f);
        keyRect.pivot = new Vector2(0.5f, 0.5f);
        keyRect.sizeDelta = new Vector2(iconWidth, iconHeight);
        keyRect.anchoredPosition = Vector2.zero;

        Image image = GetComponent<Image>();
        image.sprite = keySprite;
        image.color = keyColor;
        image.preserveAspect = false;
        image.raycastTarget = false;

        if (image.sprite == null)
        {
            Debug.LogError("PipeKeyView: no key sprite was assigned.", this);
        }
    }

    private static Color ResolveColor(string colorHex)
    {
        if (!string.IsNullOrWhiteSpace(colorHex) && ColorUtility.TryParseHtmlString(colorHex, out Color configuredColor))
        {
            return configuredColor;
        }

        return DefaultKeyColor;
    }
}
