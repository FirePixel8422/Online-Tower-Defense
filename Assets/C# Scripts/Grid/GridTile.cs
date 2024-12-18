using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTile : MonoBehaviour
{
    public Material mat;


    [ColorUsage(true, true)]
    public Color[] onFireColors;

    public float colorSwapTime;
    public float fadeBackMultiplier;

    public int fireAmount;

    [ColorUsage(true, true)]
    private Color color;



    private void Start()
    {
        mat = GetComponent<Renderer>().material;

        color = mat.GetColor("_Emission_Color");

        StartCoroutine(WaitForGrid());
    }

    private IEnumerator WaitForGrid()
    {
        yield return new WaitUntil(() => GridManager.Instance.gridSizeX != 0);

        Vector3 pos = transform.position;
        pos.y = 0;

        GridManager.Instance.UpdateGridDataTile(GridManager.Instance.GridObjectFromWorldPoint(pos).gridPos, this);
    }


    public void SetOnFire(int amount)
    {
        fireAmount += amount;


        StopAllCoroutines();

        float _colorSwapTime = colorSwapTime;
        if (amount < 0)
        {
            _colorSwapTime = colorSwapTime * fadeBackMultiplier;
        }


        StartCoroutine(ChangeColor(onFireColors[Mathf.Clamp(fireAmount, 0, onFireColors.Length - 1)], _colorSwapTime));
    }



    private IEnumerator ChangeColor(Color targetColor, float colorSwapTime)
    {
        float elapsedTime = 0;

        while (color != targetColor)
        {
            yield return null;

            elapsedTime += Time.deltaTime;

            // Calculate the interpolation factor (clamped to the range [0, 1])
            float t = Mathf.Clamp01(elapsedTime / colorSwapTime);


            color = Color.Lerp(color, targetColor, t);

            mat.SetColor("_Emission_Color", color);
        }
    }
}
 