using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PreMaid;
using System.Linq;

public class VRTracerToMotion : MonoBehaviour
{
    public Transform headTransform;         // 頭部(HMD)
    public Transform rightHandTransform;    // 右手コントローラ
    public Transform leftHandTransform;     // 左手コントローラ

    public PreMaidIKController ikController;      // モデルのIK計算クラス
    public Transform previewModel;          // 表示用のモデル

    public Canvas uiCanvas;                 // 操作用のUI
    public Transform calibrationText;       // キャリブレーション時に表示するオブジェクト

    public float scale = 0.2f;

    private bool isMounted = false;
    private Vector3 headToRoot;             // モデルの頭部からみたルートの位置ベクトル

    private List<Vector3> headCalibration = new List<Vector3>();
    private List<Vector3> rightHandCalibration = new List<Vector3>();
    private List<Vector3> leftHandCalibration = new List<Vector3>();

    private Vector3 originalHeadPosition;
    private Vector3 originalRightHandPosition;
    private Vector3 originalLeftHandPosition;

    private bool isCalibrating = false;     // キャリブレーション中ならばtrueにしておく

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

        if (calibrationText) calibrationText.gameObject.SetActive(false);

        originalHeadPosition = ikController.headTransform.position;
        originalRightHandPosition = ikController.rightHandTransform.position;
        originalLeftHandPosition = ikController.leftHandTransform.position;

        //Debug.Log(originalHeadPosition + " / " + originalRightHandPosition + " / " + originalLeftHandPosition);

        Vector3 lossyScale = ikController.transform.lossyScale;
        Vector3 vec = ikController.transform.position - ikController.headTransform.position;
        headToRoot = new Vector3(vec.x / lossyScale.x, vec.y / lossyScale.y, vec.z / lossyScale.z);
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
                ikController.rightHandTarget.position = scale * (rightHandTransform.position - bodyPosition) + modelHeadPosition;
                // コントローラの正面と、元の手の初期姿勢は異なるため補正が必要
                ikController.rightHandTarget.rotation = rightHandTransform.rotation * Quaternion.Euler(90f, -90f, 0f);
            }

            // 左手人差し指または中指トリガーが押されていれば、左手を動かす
            if (OVRInput.Get(OVRInput.RawButton.RIndexTrigger) || OVRInput.Get(OVRInput.RawButton.RHandTrigger))
            {
                ikController.leftHandTarget.position = scale * (leftHandTransform.position - bodyPosition) + modelHeadPosition;
                // コントローラの正面と、元の手の初期姿勢は異なるため補正が必要
                ikController.leftHandTarget.rotation = leftHandTransform.rotation * Quaternion.Euler(90f, 90f, 0f);
            }            
        }

        // A ボタンをおしながら B ボタンを押すとキャリブレーションが始まる
        if (OVRInput.Get(OVRInput.Button.One) && OVRInput.GetDown(OVRInput.Button.Two))
        {
            Debug.Log("START Calibration");
            StartCoroutine("CalibrationCoroutine");
        }

        // メニューボタンでUIの表示切替
        if (OVRInput.GetDown(OVRInput.Button.Start))
        {
            ToggleUI();
        }

        // PrimaryThumbstick を押し込みながら上下でサイズ調整
        if (OVRInput.Get(OVRInput.Button.PrimaryThumbstick))
        {
            scale *= Mathf.Pow(10f, OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y * scaleChangeCoef);
            scale = Mathf.Clamp(scale, 0.05f, 4.0f);
            //ikController.transform.localScale = Vector3.one * scale;
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

    // モデルの表示・非表示を設定
    void SetVisible(bool visible)
    {
        var renderers = ikController.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.enabled = visible;
        }
    }

    // UIの表示・非表示を切り替え
    void ToggleUI()
    {
        if (uiCanvas)
        {
            uiCanvas.gameObject.SetActive(!uiCanvas.isActiveAndEnabled);
        }
    }

    /// <summary>
    /// 両手を真横に伸ばしてキャリブレーション
    /// </summary>
    /// <returns></returns>
    IEnumerator CalibrationCoroutine()
    {
        if (isCalibrating)
        {
            yield break;
        }

        // キャリブレーション開始
        isCalibrating = true;
        SetVisible(false);
        if (calibrationText) calibrationText.gameObject.SetActive(true);
        
        // 手を真横にする時間待つ
        yield return new WaitForSeconds(2f);

        // 1秒間計測
        float startedTime = Time.time;
        headCalibration.Clear();
        rightHandCalibration.Clear();
        leftHandCalibration.Clear();
        while ((Time.time - startedTime) < 1f)
        {
            headCalibration.Add(headTransform.position);
            rightHandCalibration.Add(rightHandTransform.position);
            leftHandCalibration.Add(leftHandTransform.position);
            yield return null;
        }

        // 平均を求める
        Vector3 headPos = headTransform.position;
        Vector3 rightPos = rightHandTransform.position;
        Vector3 leftPos = leftHandTransform.position;
        if (headCalibration.Count > 0)
            headPos = headCalibration.Aggregate(Vector3.zero, (s, v) => s + v) / headCalibration.Count;
        if (rightHandCalibration.Count > 0)
            rightPos = rightHandCalibration.Aggregate(Vector3.zero, (s, v) => s + v) / rightHandCalibration.Count;
        if (leftHandCalibration.Count > 0)
            leftPos = leftHandCalibration.Aggregate(Vector3.zero, (s, v) => s + v) / leftHandCalibration.Count;

        //Debug.Log(headPos + " / " + rightPos + " / " + leftPos);

        // 大きさ調整
        const float lenOffset = -0.5f;
        scale = (originalRightHandPosition - originalLeftHandPosition).magnitude / ((rightPos - leftPos).magnitude + lenOffset);
        //ikController.transform.localScale = Vector3.one / scale;
        //ikController.transform.position = headTransform.position + Vector3.Scale(headToRoot, ikController.transform.lossyScale);

        // キャリブレーション終了
        isCalibrating = false;
        SetVisible(true);
        if (calibrationText) calibrationText.gameObject.SetActive(false);
    }
}
