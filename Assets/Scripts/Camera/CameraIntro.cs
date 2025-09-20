using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraIntro : MonoBehaviour
{
    public PixelPerfectCamera pixelPerfectCamera;
    public int startPPU = 32;   // PPU khi bắt đầu (nhỏ → nhìn xa toàn cảnh)
    public int targetPPU = 100; // PPU mặc định gameplay
    public int zoomSlower = 1;

    void Start()
    {
        pixelPerfectCamera.assetsPPU = startPPU;
        StartCoroutine(ZoomIntro());
    }

    IEnumerator ZoomIntro()
    {
        int finalPPU = targetPPU;

        yield return new WaitForSeconds(1f);

        while ( pixelPerfectCamera.assetsPPU < targetPPU)
        {

            pixelPerfectCamera.assetsPPU++;

            yield return new WaitForSeconds(0.01f * zoomSlower);
        }

        pixelPerfectCamera.assetsPPU = finalPPU; 
    }
}
