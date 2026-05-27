using UnityEngine;

namespace ChemSimDiploma.Indicator
{
/// <summary>
/// Цвета универсальной индикаторной бумаги по шкале pH (красный → … → фиолетовый).
/// </summary>
public static class IndicatorPhColor
{
    private static readonly (float ph, Color color)[] Stops =
    {
        (0f, new Color(0.92f, 0.12f, 0.1f)),
        (2f, new Color(0.98f, 0.42f, 0.08f)),
        (4f, new Color(1f, 0.82f, 0.12f)),
        (6f, new Color(0.75f, 0.9f, 0.18f)),
        (7f, new Color(0.22f, 0.78f, 0.28f)),
        (8f, new Color(0.12f, 0.72f, 0.52f)),
        (10f, new Color(0.15f, 0.55f, 0.92f)),
        (12f, new Color(0.35f, 0.22f, 0.9f)),
        (14f, new Color(0.55f, 0.12f, 0.72f)),
    };

    public static Color ForPh(float ph)
    {
        ph = Mathf.Clamp(ph, Stops[0].ph, Stops[^1].ph);

        for (int i = 0; i < Stops.Length - 1; i++)
        {
            (float phA, Color colorA) = Stops[i];
            (float phB, Color colorB) = Stops[i + 1];
            if (ph > phB) continue;

            float t = Mathf.InverseLerp(phA, phB, ph);
            return Color.Lerp(colorA, colorB, t);
        }

        return Stops[^1].color;
    }
}
}
