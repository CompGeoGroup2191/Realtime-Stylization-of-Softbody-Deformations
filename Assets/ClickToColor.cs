using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickToColor : MonoBehaviour
{
    public Material[] materials;
    public Renderer rend;
    private int index = 1;

    // Start is called before the first frame update
    void Start()
    {
        //rend = GetComponent<Renderer>();
        //rend.enabled = true;
        //materials = GetComponent<Material[]>();
        //materials.enabled = true;
    }
    public void OnMouseDown()
    {
        //if (materials.Length == 0)
        //{
        //    return;
        //}
        if (Input.GetMouseButtonDown(0))
        {
            //            index += 1;
            //            if (index == materials.Length + 1)
            //            {
            //                index = 1;
            //            }
            //            print(index);
            //            rend.sharedMaterial = materials[index - 1];
            if(rend.material.GetColor("_Color") == Color.red)       { rend.material.SetColor("_Color", Color.blue); }
            else if(rend.material.GetColor("_Color") == Color.blue) { rend.material.SetColor("_Color", Color.green); }
            else                                                    { rend.material.SetColor("_Color", Color.red); }
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
