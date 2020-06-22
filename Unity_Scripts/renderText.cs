// Ghosting.

using UnityEngine;
using System.Collections;

[AddComponentMenu("Assets/ImageEffect_002")]
public class renderText : MonoBehaviour
{
    public int sizeX;
    public int sizeY;
    public RenderTexture pastFrame;
    private RenderTexture pastFrame1;
    private RenderTexture pastFrame2;
    private RenderTexture pastFrame3;
    private RenderTexture pastFrame4;
    private RenderTexture pastFrame5;
    private RenderTexture pastFrame6;
    private RenderTexture pastFrame7;
    private RenderTexture pastFrame8;
    private RenderTexture pastFrame9;
    private RenderTexture pastFrame10;
    private RenderTexture pastFrame11;

    void Start()
    {
        pastFrame1 = new RenderTexture(sizeX, sizeY, 16);
        pastFrame2 = new RenderTexture(sizeX, sizeY, 16);
        pastFrame3 = new RenderTexture(sizeX, sizeY, 16);
        pastFrame4 = new RenderTexture(sizeX, sizeY, 16);
        pastFrame5 = new RenderTexture(sizeX, sizeY, 16);
        pastFrame6 = new RenderTexture(sizeX, sizeY, 16);
        pastFrame7 = new RenderTexture(sizeX, sizeY, 16);
        pastFrame8 = new RenderTexture(sizeX, sizeY, 16);
        pastFrame9 = new RenderTexture(sizeX, sizeY, 16);
        pastFrame10= new RenderTexture(sizeX, sizeY, 16);
        pastFrame11= new RenderTexture(sizeX, sizeY, 16);

    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        
        Graphics.Blit(pastFrame11, pastFrame);
        Graphics.Blit(pastFrame10, pastFrame11);
        Graphics.Blit(pastFrame9, pastFrame10);
        Graphics.Blit(pastFrame8, pastFrame9);
        Graphics.Blit(pastFrame7, pastFrame8);
        Graphics.Blit(pastFrame6, pastFrame7);
        Graphics.Blit(pastFrame5, pastFrame6);
        Graphics.Blit(pastFrame4, pastFrame5);
        Graphics.Blit(pastFrame3, pastFrame4);
        Graphics.Blit(pastFrame2, pastFrame3);
        Graphics.Blit(pastFrame1, pastFrame2);
        

        Graphics.Blit(src, dst);
        Graphics.Blit(RenderTexture.active, pastFrame1);

    }
}