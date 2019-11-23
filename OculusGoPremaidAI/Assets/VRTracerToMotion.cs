using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PreMaid;

public class VRTracerToMotion : MonoBehaviour
{
    public Transform headTransform;         // 頭部(HMD)
    public Transform rightHandTransform;    // 右手コントローラ
    public Transform leftHandTransform;     // 左手コントローラ

    public PreMaidIKController ikController;      // モデルのIK計算クラス
    public Transform previewModel;          // 表示用のモデル

    public float scale = 1f;

    private bool isMounted = false;
    private Vector3 headToRoot;             // モデルの頭部からみたルートの位置ベクトル

    private void Awake()
    {
        OVRManager.HMDMounted += OVRManager_HMDMounted;
        OVRManager.HMDUnmounted += OVRManager_HMDUnmounted;
        OVRManager.HMDLost += OVRManager_HMDUnmounted;
    }

    /// <summary>
    /// HMDを外されたときの処理
    /// </summary>
    private void OVRManager_HMDUnmounted()
    {
        isMounted = false;
    }

    /// <summary>
    /// HMDを装着したときの処理
    /// </summary>
    private void OVRManager_HMDMounted()
    {
        isMounted = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!ikController) ikController = GetComponent<PreMaidIKController>();
        if (!previewModel) previewModel = ikController.transform;

        Vector3 lossyScale = ikController.transform.lossyScale;
        Vector3 vec = ikController.transform.position - ikController.headTransform.position;
        headToRoot = new Vector3(vec.x / lossyScale.x, vec.y / lossyScale.y, vec.z / lossyScale.z - 0.05f);
    }

    // Update is called once per frame
    void Update()
    {
        const float scaleChangeCoef = 0.005f;
        const float translationSpeed = 0.2f;
        //const float previewRotationSpeed = 60f;

        // HMDをかけているときだけ実行
        if (isMounted)
        {
            // 頭部の向きを追従
            ikController.HeadOrientationTarget.rotation = headTransform.rotation;

            SetHeadPostion(headTransform);

            Vector3 bodyPosition = headTransform.position;
            Vector3 modelHeadPosition = ikController.headTransform.position;

            // 右手人差し指または中指トリガーが押されていれば、右手を動かす
            if (OVRInput.Get(OVRInput.RawButton.RIndexTrigger) || OVRInput.Get(OVRInput.RawButton.RHandTrigger))
            {
                ikController.rightHandTarget.rotation = rightHandTransform.rotation;
                ikController.rightHandTarget.position = (rightHandTransform.position - bodyPosition) + modelHeadPosition;
            }

            // 左手人差し指または中指トリガーが押されていれば、左手を動かす
            if (OVRInput.Get(OVRInput.RawButton.RIndexTrigger) || OVRInput.Get(OVRInput.RawButton.RHandTrigger))
            {
                ikController.leftHandTarget.rotation = leftHandTransform.rotation;
                ikController.leftHandTarget.position = (leftHandTransform.position - bodyPosition) + modelHeadPosition;
            }            
        }

        // PrimaryThumbstick を押し込みながら上下でサイズ調整
        if (OVRInput.Get(OVRInput.Button.PrimaryThumbstick))
        {
            scale *= Mathf.Pow(10f, OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y * scaleChangeCoef);
            scale = Mathf.Clamp(scale, 0.5f, 4.0f);
            ikController.transform.localScale = Vector3.one * scale;
        }
        else
        {

            if (previewModel)
            {
                Vector2 pvec= OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick) * translationSpeed * Time.deltaTime;
                Vector2 svec = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick) * translationSpeed * Time.deltaTime;
                previewModel.position += new Vector3(svec.x, pvec.y, svec.y);
            }
        }
    }

    private void SetHeadPostion(Transform hmdTransform)
    {
        if (previewModel != ikController.transform)
        {
            ikController.transform.position = hmdTransform.position + Vector3.Scale(headToRoot, ikController.transform.lossyScale);
        }
    }
}
