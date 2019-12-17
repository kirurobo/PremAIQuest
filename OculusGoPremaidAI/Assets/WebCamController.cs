using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebCamController : MonoBehaviour
{

    int width = 1920;
    int height = 1080;
    int fps = 60;
    WebCamTexture webCamTexture;
    int webCamIndex = 0;

    void Start()
    {
        SelectWebCam();
    }

    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.Three)) {
            SelectWebCam(1);
        }
        else if (OVRInput.GetDown(OVRInput.Button.Four))
        {
            SelectWebCam(-1);
        }
    }

    bool SelectWebCam(int step = 0)
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length < 1) return false;

        // 次のカメラにする場合はインデックスを増やす
        int newIndex = webCamIndex + step;
        if (newIndex >= devices.Length) newIndex = 0;
        if (newIndex < 0) newIndex = devices.Length - 1;

        // すでにWebCamTextureが設定済みの場合
        if (webCamTexture)
        {
            // カメラが変わらなければ何もせずそのまま表示し続ける
            if (newIndex == webCamIndex)
            {
                return true;
            }
            else
            {
                webCamTexture.Stop();
            }
        }

        webCamIndex = newIndex;
        webCamTexture = new WebCamTexture(devices[webCamIndex].name, this.width, this.height, this.fps);
        GetComponent<Renderer>().material.mainTexture = webCamTexture;
        webCamTexture.Play();

        return true;
    }
}