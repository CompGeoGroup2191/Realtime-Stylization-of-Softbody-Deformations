using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickToColor : MonoBehaviour
{
    public Renderer rend;
    private int index = 1;

    // Start is called before the first frame update
    void Start()
    {

    }
    public void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Cycle through colors when clicked
            if(rend.material.GetColor("_Color") == Color.red)       { rend.material.SetColor("_Color", Color.blue); }
            else if(rend.material.GetColor("_Color") == Color.blue) { rend.material.SetColor("_Color", Color.green); }
            else                                                    { rend.material.SetColor("_Color", Color.red); }

            // Get clicked spot
            Vector3 clickedPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Texture2D tex = rend.material.mainTexture as Texture2D;
            //print(tex.dimension.ToString());
            print(clickedPoint);
            //print(tex.GetPixel(x, y));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
