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
        // すでにWebCamTextureが設定済みなら、停止
        if (webCamTexture) webCamTexture.Stop();

        WebCamDevice[] devices = WebCamTexture.devices;

        // 次のカメラにする場合はインデックスを増やす
        webCamIndex += step;
        if (webCamIndex >= devices.Length) webCamIndex = 0;
        if (devices.Length < 1) return false;

        webCamTexture = new WebCamTexture(devices[webCamIndex].name, this.width, this.height, this.fps);
        GetComponent<Renderer>().material.mainTexture = webCamTexture;
        webCamTexture.Play();

        return true;
    }
}