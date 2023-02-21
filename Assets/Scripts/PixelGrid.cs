using System;
using UnityEngine;

public sealed class PixelGrid : MonoBehaviour
{
    [SerializeField]
    private Pixel _pixel;

    [SerializeField]
    private int _gridSize;

    public event Action<int> OnPixelSelected;

    private void Start()
    {
        RectTransform rectTransform = (RectTransform)transform;
        Vector2 pixelSize = rectTransform.sizeDelta / _gridSize;

        for (int i = 0; i < _gridSize * _gridSize; i++)
        {
            int j = i;
            Pixel pixel = Instantiate(_pixel, transform);
            pixel.name = string.Format("Pixel {0}", j);
            RectTransform pixelTransform = (RectTransform)pixel.transform;
            pixelTransform.sizeDelta = pixelSize;
            pixelTransform.anchoredPosition = new Vector2(j % _gridSize * pixelSize.x, -Mathf.FloorToInt(j / _gridSize) * pixelSize.y);
            pixel.OnSelected += () =>
            {
                if (OnPixelSelected != null)
                    OnPixelSelected.Invoke(j);
            };
        }
    }
}
