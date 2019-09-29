using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickToModifyTexture : MonoBehaviour
{
    public Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        //cam = GetComponent<Camera>();
    }

    // Update is called once per frame
   void Update()
    {
        if (!Input.GetMouseButton(0)) { return; }

        RaycastHit hit;
        if (!Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit))
        {
            print("Out 1");
            return;
        }

        Renderer rend = hit.transform.GetComponent<Renderer>();
        MeshCollider meshCollider = hit.collider as MeshCollider;

        if (rend == null || rend.sharedMaterial == null || rend.sharedMaterial.mainTexture == null || meshCollider == null)
        {
            //print((rend == null).ToString() + " " + (rend.sharedMaterial == null).ToString() + " " + (rend.sharedMaterial.mainTexture == null).ToString() + " " + (meshCollider == null).ToString());
            return;
        }

        Texture2D tex = rend.material.mainTexture as Texture2D;
        Vector2 pixelUV = hit.textureCoord;
        //print(pixelUV.x.ToString() + ", " + pixelUV.y.ToString());
        pixelUV.x *= tex.width;
        pixelUV.y *= tex.height;
        //print(pixelUV.x.ToString() + ", " + pixelUV.y.ToString());

        int patchW = (int) 0.1 * tex.width;
        int patchH = (int) 0.1 * tex.height;

        tex.SetPixel((int)pixelUV.x, (int)pixelUV.y, Color.black);
        //TODO: work out why these commented for loops don't work
        //for(int xp = -1*patchW; xp < patchW; ++xp)
        for(int xp = 50; xp < 100; ++xp)
        {
            //for(int yp = -1*patchH; yp < patchH; ++yp)
            for (int yp = -50; yp < 100; ++yp)
            {
                tex.SetPixel(((int)pixelUV.x + xp) % tex.width, ((int)pixelUV.y + yp) % tex.height, Color.black);
            }
        }
        

        tex.Apply();

        //print(tex.GetPixel((int)pixelUV.x, (int)pixelUV.y).ToString());
    }
}
