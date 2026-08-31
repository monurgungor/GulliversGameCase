using UnityEngine;

/// <summary>
/// Keeps its RectTransform inside the part of the screen the phone actually
/// gives you, so buttons do not sit under a notch, a punch hole or the home
/// indicator.
///
/// The rect is only recalculated when the safe area or the resolution changes,
/// which on a phone means at startup and on rotation, not every frame.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    private RectTransform rectTransform;
    private Rect appliedSafeArea;
    private Vector2Int appliedResolution;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        Apply();
    }

    private void Update()
    {
        if (Screen.safeArea != appliedSafeArea ||
            Screen.width != appliedResolution.x ||
            Screen.height != appliedResolution.y)
        {
            Apply();
        }
    }

    private void Apply()
    {
        appliedSafeArea = Screen.safeArea;
        appliedResolution = new Vector2Int(Screen.width, Screen.height);

        if (appliedResolution.x <= 0 || appliedResolution.y <= 0)
        {
            return;
        }

        // Anchors are normalised, so the same values hold at every resolution.
        Vector2 min = appliedSafeArea.position;
        Vector2 max = appliedSafeArea.position + appliedSafeArea.size;

        min.x /= appliedResolution.x;
        min.y /= appliedResolution.y;
        max.x /= appliedResolution.x;
        max.y /= appliedResolution.y;

        rectTransform.anchorMin = min;
        rectTransform.anchorMax = max;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}
