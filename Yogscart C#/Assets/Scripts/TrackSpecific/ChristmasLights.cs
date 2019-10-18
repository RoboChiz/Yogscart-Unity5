using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChristmasLights : MonoBehaviour
{
    private Material mat;
    public Color startColour;

    public float speed = 2.5f, max = 1.0f;
    public bool flip;

    private static float nonFlipEmission, flipEmission;
    private static bool first = true;

    private void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        mat = renderer.material;
    }

    void Update ()
    {
        if (first)
        {
            flipEmission = ((Mathf.Sin((Time.time * speed) + Mathf.PI) + 1) / 2f) * max;
            nonFlipEmission = ((Mathf.Sin(Time.time * speed) + 1) / 2f) * max;

            first = false;
        }

        Color finalColor = startColour * Mathf.LinearToGammaSpace(flip ? flipEmission : nonFlipEmission);
        mat.SetColor("_EmissionColor", finalColor);
    }

    private void LateUpdate()
    {
        if (!first)
            first = true;
    }
}
