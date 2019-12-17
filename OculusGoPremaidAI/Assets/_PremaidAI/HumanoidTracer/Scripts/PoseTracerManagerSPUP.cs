using System;
using System.Collections;
using System.Collections.Generic;
using PreMaid.RemoteController;
using TMPro;
using UnityEngine;

namespace PreMaid.HumanoidTracer
{
    /// <summary>
    /// MecanimというかHumanoidのアバターからモーションを実機に反映するスクリプト
    /// 仮説として、全サーボ情報を送ると結構応答性が悪い（10FPS程度）ので、
    /// キーフレームを1秒ごと+差分フレームを送る
    /// これで1個や2個しかサーボが動かないモーションだと応答性がよくなる、はず
    /// </summary>
    [RequireComponent(typeof(PreMaid.RemoteController.PreMaidControllerSPUP))]
    [DefaultExecutionOrder(11001)] //after VRIK calclate
    public class PoseTracerManagerSPUP : MonoBehaviour
    {
        private PreMaid.RemoteController.PreMaidControllerSPUP _controller;

        [SerializeField] private Animator target;

        [SerializeField] private HumanoidModelJoint[] _joints;

        [SerializeField] private TMPro.TMP_Dropdown _serialPortsDropdown = null;

        private bool _initialized = false;

        //差分フレームタイマー
        private float coolTime = 0f;

        //キーフレームは1秒ごとに打つ
        private float keyFrameTimer = 0f;

        private float lastSentTime = 0f;    // 最後に送信したタイムスタンプ
        private float sendingInterval = 0.1f;   // 送信間隔

        List<PreMaidServo> latestServos = new List<PreMaidServo>();


        [SerializeField] private int currentFPS = 0;


        // Start is called before the first frame update
        void Start()
        {
            _controller = GetComponent<PreMaid.RemoteController.PreMaidControllerSPUP>();
            List<TMP_Dropdown.OptionData> serialPortNamesList = new List<TMP_Dropdown.OptionData>();

            var portNames = SerialPortUtility.SerialPortUtilityPro.GetConnectedDeviceList(SerialPortUtility.SerialPortUtilityPro.OpenSystem.BluetoothSSP);

            if (portNames != null)
            {
                foreach (var VARIABLE in portNames)
                {
                    TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData(VARIABLE.SerialNumber);
                    serialPortNamesList.Add(optionData);

                    Debug.Log(VARIABLE);
                }

                _serialPortsDropdown.ClearOptions();
                _serialPortsDropdown.AddOptions(serialPortNamesList);
            } else
            {
                // Android実機でのデバッグ用
                serialPortNamesList.Add(new TMP_Dropdown.OptionData("RNBT-4FFA"));
                serialPortNamesList.Add(new TMP_Dropdown.OptionData("RNBT-50D6"));
                serialPortNamesList.Add(new TMP_Dropdown.OptionData("RNBT-9C50"));
                serialPortNamesList.Add(new TMP_Dropdown.OptionData("RNBT-94F6"));

                _serialPortsDropdown.ClearOptions();
                _serialPortsDropdown.AddOptions(serialPortNamesList);
                _serialPortsDropdown.SetValueWithoutNotify(0);
            }

            // 上半身だけを操作対象とする
            _controller.jointMask = (uint)(PreMaidControllerSPUP.JointMask.UpperBody);

            // 関節速度制限
            ModelJoint.SetAllJointsMaxSpeed(90f);

            //対象のAnimatorにBoneにHumanoidModelJoint.csのアタッチ漏れがあるかもしれない
            //なので、一旦全部検索して、見つからなかったサーボ情報はspineに全部動的にアタッチする
            Transform spineBone = target.GetBoneTransform(HumanBodyBones.Spine);
            //仮でspineにでも付けておこう
            if (target != null)
            {
                var joints = target.GetComponentsInChildren<HumanoidModelJoint>();

                foreach (PreMaidServo.ServoPosition item in Enum.GetValues(typeof(PreMaidServo.ServoPosition)))
                {
                    if (Array.FindIndex(joints, joint => joint.TargetServo == item) == -1)
                    {
                        var jointScript = spineBone.gameObject.AddComponent<HumanoidModelJoint>();
                        jointScript.TargetServo = item;
                    }
                }

                // 手首だけは最高速度を高くしておく
                var modeljoints = target.GetComponentsInChildren<ModelJoint>();
                foreach (ModelJoint j in modeljoints)
                {
                    if (j.ServoID == "15" || j.ServoID == "17")
                    {
                        j.maxSpeed = 180f;
                    }
                }

            }

            _joints = target.GetComponentsInChildren<HumanoidModelJoint>();
        }


        /// <summary>
        /// UGUIのOpenボタンを押したときの処理
        /// </summary>
        public void Open()
        {
            var willOpenSerialPortName = _serialPortsDropdown.options[_serialPortsDropdown.value].text;
            Debug.Log(willOpenSerialPortName + "を開きます");
            _initialized = false;

            var openSuccess = _controller.OpenSerialPort(willOpenSerialPortName);
            if (openSuccess)
            {
                StartCoroutine(PreMaidParamInitilize());
                Debug.Log("コルーチンを開始しました");
            }
        }

        /// <summary>
        /// UGUIのテスト送信ボタンを押したときの処理
        /// </summary>
        public void Send()
        {
        }

        /// <summary>
        /// ドロップダウンで選択されているものを一つ次に進める
        /// </summary>
        public void ForwardDropdown()
        {
            int index = _serialPortsDropdown.value + 1;
            if (index >= _serialPortsDropdown.options.Count) index = 0;

            _serialPortsDropdown.value = index;
            _serialPortsDropdown.RefreshShownValue();
        }

        /// <summary>
        /// ドロップダウンで選択されているものを一つ前に戻す
        /// </summary>
        public void BackwardDropdown()
        {
            int index = _serialPortsDropdown.value - 1;
            if (index < 0) index = 0;

            _serialPortsDropdown.value = index;
            _serialPortsDropdown.RefreshShownValue();
        }

        IEnumerator PreMaidParamInitilize()
        {
            yield return new WaitForSeconds(1f);
            //ここらへんでサーボパラメータ入れたりする
            Invoke(nameof(ApplyStretch), 2f);
            yield return new WaitForSeconds(1f);
            Invoke(nameof(ApplyMecanimPoseWithDiff), 2f);
        }

        void ApplyStretch()
        {
            _controller.ForceAllServoStretchProperty(40, 70);
        }

        /// <summary>
        /// 現在のAnimatorについているサーボの値を参照しながら差分だけ送る
        /// </summary>
        void ApplyMecanimPoseWithDiff()
        {
            if ((Time.time - lastSentTime) < sendingInterval) return;

            var servos = _controller.Servos;

            List<ModelJoint> orders = new List<ModelJoint>();

            foreach (var servo in servos)
            {
                orders.Add(servo);
            }

            //ここでordersに差分だけ送れます
            coolTime = orders.Count * 0.005f; //25個あると0.08くらい、1個だと0.01くらいのクールタイムが良い

            if (orders.Count > 0)
            {
                currentFPS++;
                //Debug.Log("Servo Num:" + orders.Count);
                _controller.ApplyPoseFromServos(orders, Mathf.Clamp(orders.Count*2,10,40));
            }
            lastSentTime = Time.time;

            if (_initialized == false)
            {
                _initialized = true;
            }
        }

        /// <summary>
        /// 現在のAnimatorについているサーボの値を参照しながら全て送る
        /// </summary>
        void ApplyMecanimPoseAll()
        {
            if ((Time.time - lastSentTime) < sendingInterval) return;

            var servos = _controller.Servos;

            List<ModelJoint> orders = new List<ModelJoint>();

            foreach (var servo in servos)
            {
                orders.Add(servo);
            }

            //ここでordersに差分だけ送れます。speed=40でcooltime=0.05fでいけた！？！？
            //つまり20FPSだとたまに送信失敗するけど意外と通る。
            //BT環境が悪かったらもっと速度を落とすとか？
            //cooltime=0.08f  だと12FPS送信になって結構失敗しないです
            coolTime = 0.05f; 
            
            keyFrameTimer = 1f;
            //Debug.Log("全フレーム転送 :" + orders.Count+" FPS:"+currentFPS);
            _controller.ApplyPoseFromServos(orders, 40);

            currentFPS = 0;
            lastSentTime = Time.time;

            if (_initialized == false)
            {
                _initialized = true;
            }
        }

        private void Update()
        {
            //if (Input.GetKeyDown(KeyCode.A))
            //{
            //    target.SetTrigger("TestMotion");
            //}
        }

        void LateUpdate()
        {
            if (_initialized == false)
            {
                return;
            }

            coolTime -= Time.deltaTime;
            keyFrameTimer -= Time.deltaTime;
            if (coolTime <= 0)
            {
                
                //if (keyFrameTimer <= 0)
                {
                    ApplyMecanimPoseAll();
                }
                //else
                {
                  //  ApplyMecanimPoseWithDiff();
                }
            }
        }
    }
}