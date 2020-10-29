using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathTest : MonoBehaviour
{
    public Transform target;

    public Transform pointOfTangency1;
    public Transform pointOfTangency2;


    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(this.transform.lossyScale.z / 2f);
    }

    // Update is called once per frame
    void Update()
    {
        float zh = target.position.z;
        float yh = target.position.y;
        float xt = pointOfTangency1.position.x;
        float yt, zt;
        float w = this.transform.lossyScale.z / 2f;

        float sqrXh = zh * zh + yh * yh;
        if (sqrXh <= (w * w)) return;

        if (Mathf.Approximately(yh, 0f))
        {
            zt = w * w / zh;
            yt = w * Mathf.Sqrt(1f - ((w * w) / (zh * zh)));
            if (zh < 0) yt = -yt;
        }
        else if (Mathf.Approximately(zh, 0f))
        {
            yt = w * w / yh;
            zt = w * Mathf.Sqrt(1f - ((w * w) / (yh * yh)));
            if (yh >= 0) zt = -zt;
        }
        else
        {
            if (yh < 0)
            {
                zt = ((w * w * zh) + w * Mathf.Sqrt((sqrXh - w * w) * yh * yh)) / sqrXh;
            }
            else
            {
                zt = ((w * w * zh) - w * Mathf.Sqrt((sqrXh - w * w) * yh * yh)) / sqrXh;
            }
            yt = ((w * w) - (zh * zt)) / yh;
        }
        pointOfTangency1.position = new Vector3(xt, yt, zt);

        float a0 = Mathf.Atan2(-yt, zt) * Mathf.Rad2Deg;
    }
}
