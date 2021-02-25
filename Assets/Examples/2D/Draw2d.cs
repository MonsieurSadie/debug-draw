using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Draw2d : MonoBehaviour
{
    public Color start = new Color(1, 0, 0, 0.5f);
    public Color end = new Color(0, 0, 1, 0.5f);
    public float duration = 5;

    float xpos = 0;

    float interp = 0;

    void Start()
    {
        Draw.EnableTransparentMode(true);
        Draw.SetValuesAsPixels(true);
    }

    
    void Update()
    {
        interp = Time.time / duration;
        //Draw.color = Color.Lerp(start, end, interp);
        Draw.color = start;
        Draw.Rect2D(Input.mousePosition.x, Input.mousePosition.y, 20, 20);
        xpos += 1f * Time.deltaTime;
    }
}
