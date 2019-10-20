using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace PreMaid
{
    public class PreMaidIKController : MonoBehaviour
    {
        #region IK solver classes
        /// <summary>
        /// IKソルバの元クラス
        /// </summary>
        public abstract class IKSolver
        {
            private Transform baseTransform;

            public abstract void Initialize();
            public abstract void ApplyIK();
            public abstract void DrawGizmos();
            
            /// <summary>
            /// Quaternion で渡された姿勢のうち、X, Y, Z 軸いずれか周り成分を抽出してサーボ角に反映します
            /// </summary>
            /// <param name="rot">目標姿勢</param>
            /// <param name="joint">指定サーボ</param>
            /// <returns>回転させた軸成分を除いた残りの回転 Quaternion</returns>
            internal Quaternion ApplyPartialRotation(Quaternion rot, ModelJoint joint)
            {
                Quaternion q = rot;
                Vector3 axis = Vector3.right;
                float direction = (joint.isInverse ? -1f : 1f);     // 逆転なら-1
                switch (joint.targetAxis)
                {
                    case ModelJoint.Axis.X:
                        q.y = q.z = 0;
                        if (q.x < 0) direction = -direction;
                        axis = Vector3.right;
                        break;
                    case ModelJoint.Axis.Y:
                        q.x = q.z = 0;
                        if (q.y < 0) direction = -direction;
                        axis = Vector3.up;
                        break;
                    case ModelJoint.Axis.Z:
                        q.x = q.y = 0;
                        if (q.z < 0) direction = -direction;
                        axis = Vector3.forward;
                        break;
                }
                if (q.w == 0 && q.x == 0 && q.y == 0 && q.z == 0)
                {
                    Debug.Log("Joint: " + joint.name + " rotation N/A");
                    q = Quaternion.identity;
                }
                q.Normalize();
                float angle = Mathf.Acos(q.w) * 2.0f * Mathf.Rad2Deg * direction;

                var actualAngle = joint.SetServoValue(angle);

                return rot * Quaternion.Inverse(q);
            }
        }

        /// <summary>
        /// 頭部のIKソルバ
        /// </summary>
        public class HeadIK : IKSolver
        {
            private Transform baseTransform;
            public ModelJoint neckYaw;
            public ModelJoint headPitch;
            public ModelJoint headRoll;     // 所謂萌軸

            public Transform headBaseTarget;
            public Transform headOrientationTarget;

            public Mode method; // IKの解き方

            /// <summary>
            /// アルゴリズムの指定
            /// </summary>
            public enum Mode
            {
                None,       // IKを利用しない
                Gaze,       // 指定Transformの座標を向かせる
                Orientation,   // 指定Transformの方向をトレースする
            }

            public override void Initialize()
            {
                baseTransform = neckYaw.transform.parent;

                // 目標点が無ければ自動生成
                if (!headBaseTarget)
                {
                    var obj = new GameObject("HeadBaseTarget");
                    headBaseTarget = obj.transform;
                    if (headOrientationTarget)
                    {
                        // もし姿勢目標の方があれば，その親として作成
                        const float distance = 0.05f;
                        headBaseTarget.position = headOrientationTarget.position + baseTransform.rotation * Vector3.down * distance;
                        headBaseTarget.rotation = headOrientationTarget.rotation;
                        headBaseTarget.parent = headOrientationTarget.parent;
                        headOrientationTarget.parent = headBaseTarget;
                    }
                    else
                    {
                        const float distance = 0.3f;
                        headBaseTarget.position = headRoll.transform.position + baseTransform.rotation * Vector3.forward * distance;
                        headBaseTarget.rotation = baseTransform.rotation;
                        headBaseTarget.parent = baseTransform;
                    }
                }

                // 姿勢目標が無ければ自動生成
                if (!headOrientationTarget)
                {
                    const float distance = 0.05f;
                    var obj = new GameObject("HeadOrientationTarget");
                    headOrientationTarget = obj.transform;
                    headOrientationTarget.position = headBaseTarget.position + baseTransform.rotation * Vector3.up * distance;
                    headOrientationTarget.rotation = headBaseTarget.rotation;
                    headOrientationTarget.parent = headBaseTarget;  // 基部の子にする
                }
            }

            /// <summary>
            /// IKを適用
            /// </summary>
            public override void ApplyIK()
            {
                if (!headBaseTarget || !headOrientationTarget) return;

                switch (method) {
                    case Mode.Gaze:
                        ApplyIK_Gaze();
                        break;
                    case Mode.Orientation:
                        ApplyIK_Orientation();
                        break;
                }
            }

            /// <summary>
            /// 視線を指定座標に向ける（LookAt）パターンでIKを適用
            /// </summary>
            private void ApplyIK_Gaze()
            {
                Quaternion invBaseRotation = Quaternion.Inverse(baseTransform.rotation);
                Vector3 gazeVec = invBaseRotation * (headBaseTarget.position - headRoll.transform.position);

                Quaternion lookAtRot = Quaternion.LookRotation(gazeVec);
                Vector3 eular = lookAtRot.eulerAngles;
                float yaw = eular.y - (eular.y > 180f ? 360f : 0f);
                float pitch = eular.x - (eular.x > 180f ? 360f : 0f);

                yaw = neckYaw.SetServoValue(yaw);       // 戻り値は制限後の角度
                pitch = headPitch.SetServoValue(pitch); // 戻り値は制限後の角度

                // ロール（萌え軸）の向きを2点の目標から求める
                Quaternion rot = Quaternion.AngleAxis(-yaw, Vector3.up) * Quaternion.AngleAxis(-pitch, Vector3.right);
                Vector3 upVec = rot * invBaseRotation * (headOrientationTarget.position - headBaseTarget.position);
                float roll = 0f;
                if (!Mathf.Approximately(upVec.x, 0f) || !Mathf.Approximately(upVec.y, 0f))
                {
                    roll = -Mathf.Atan2(upVec.x, upVec.y) * Mathf.Rad2Deg;
                }
                headRoll.SetServoValue(roll);
            }

            /// <summary>
            /// 基部と目標のTransformを指定し，基部からの目標相対姿勢に合わせて頭を回す
            /// </summary>
            private void ApplyIK_Orientation()
            {
                Quaternion invBaseRotation = Quaternion.Inverse(baseTransform.rotation);
                Vector3 gazeVec = invBaseRotation * (headOrientationTarget.position - headRoll.transform.position);

                Quaternion rot = invBaseRotation * headOrientationTarget.rotation;
                //Debug.log(rot);
                rot = ApplyPartialRotation(rot, neckYaw);
                rot = ApplyPartialRotation(rot, headRoll);
                rot = ApplyPartialRotation(rot, headPitch);
            }

            /// <summary>
            /// ギズモを描画
            /// </summary>
            public override void DrawGizmos()
            {
                const float gizmoRadius = 0.005f;


                if (headBaseTarget)
                {
                    Gizmos.color = Color.red;
                    if (method == Mode.Gaze)
                    {
                        Gizmos.DrawLine(headRoll.transform.position, headBaseTarget.position);
                    }
                    Gizmos.DrawSphere(headBaseTarget.position, gizmoRadius);
                }

                if (headOrientationTarget)
                {
                    Gizmos.color = Color.yellow;
                    if (headBaseTarget)
                    {
                        Gizmos.DrawLine(headBaseTarget.position, headOrientationTarget.position);
                    }
                    Gizmos.DrawSphere(headOrientationTarget.position, gizmoRadius);
                }
            }
        }

        /// <summary>
        /// 腕のIKソルバ
        /// </summary>
        public class ArmIK : IKSolver
        {
            private Transform baseTransform;
            public ModelJoint shoulderPitch;
            public ModelJoint upperArmRoll;
            public ModelJoint upperArmPitch;
            public ModelJoint lowerArmRoll;
            public ModelJoint handPitch;
            private Transform handTip;

            public Transform elbowTarget;
            public Transform handTarget;
            public float handAngle;

            /// <summary>
            /// 右手なら true、左手なら false にしておく
            /// </summary>
            public bool isRightSide = false;

            public Mode method = Mode.Elbow;

            /// <summary>
            /// アルゴリズムの指定
            /// </summary>
            public enum Mode
            {
                None,   // IKを利用しない
                Elbow,  // 肘位置をまず決め、次に手の位置を定める
                Hand,   // 手の位置と向きを元にする
            }

            float lengthShoulder;
            float lengthUpperArm;
            float lengthLowerArm;
            float zShoulderToHand;

            public override void Initialize()
            {
                baseTransform = shoulderPitch.transform.parent;
                if (handPitch.transform.childCount < 1)
                {
                    handTip = handPitch.transform;  // 子が無かった場合はhandPitchを手先とする
                }
                else
                {
                    handTip = handPitch.transform.GetChild(0);  // 子があればそれを手先とする
                }

                lengthShoulder = (upperArmRoll.transform.position - shoulderPitch.transform.position).magnitude;
                lengthUpperArm = (lowerArmRoll.transform.position - upperArmRoll.transform.position).magnitude;
                lengthLowerArm = (handTip.position - lowerArmRoll.transform.position).magnitude;
                zShoulderToHand = (handTip.position - shoulderPitch.transform.position).z;  // 手は肩の回転軸からわずかに前に出ている。その距離[m]
                
                // 肘の目標点が無ければ自動生成
                if (!elbowTarget)
                {
                    var obj = new GameObject((isRightSide ? "Right" : "Left") + "ElbowTarget");
                    elbowTarget = obj.transform;
                    elbowTarget.parent = baseTransform;
                    elbowTarget.position = lowerArmRoll.transform.position;
                }

                // 手首の目標点が無ければ自動生成
                if (!handTarget)
                {
                    var obj = new GameObject((isRightSide ? "Right" : "Left") + "HandTarget");
                    handTarget = obj.transform;
                    handTarget.parent = baseTransform;
                    handTarget.position = handPitch.transform.position;
                }
            }

            public override void ApplyIK()
            {
                if (!elbowTarget || !handTarget) return;

                switch (method)
                {
                    case Mode.Elbow:
                        ApplyIK_ElbowFirst();
                        break;
                    case Mode.Hand:
                        ApplyIK_HandFirst();
                        break;
                }
            }

            /// <summary>
            /// 肘座標優先モードで腕のIKを解き、順次サーボ角度を設定していく
            /// </summary>
            private void ApplyIK_ElbowFirst()
            {
                // これ以下に肩に近づきすぎた肘目標点は無視する閾値
                const float sqrMinDistance = 0.0001f;   // [m^2]

                float sign = (isRightSide ? -1f : 1f);  // 左右の腕による方向入れ替え用

                Vector3 x0 = shoulderPitch.transform.position;  // UpperArmJointの座標
                Quaternion invBaseRotation = Quaternion.Inverse(baseTransform.rotation);
                Vector3 x0e = invBaseRotation * (elbowTarget.position - x0);    // x0から肘目標点までのベクトル

                // 閾値より肘目標点が肩に近すぎる場合、手首優先モードのIKにする
                if (x0e.sqrMagnitude < sqrMinDistance)
                {
                    ApplyIK_HandFirst();
                    return;
                }

                float a0 = Mathf.Atan2(sign * x0e.z, -x0e.y) * Mathf.Rad2Deg; // 肩のX軸周り回転[deg]
                shoulderPitch.SetServoValue(a0);

                Vector3 x1 = upperArmRoll.transform.position;   // 上腕始点の座標
                Quaternion invShoulderRotation = Quaternion.Inverse(shoulderPitch.normalizedRotation);
                Vector3 x1e = invShoulderRotation * (elbowTarget.position - x1);
                float a1 = Mathf.Atan2(sign * x1e.y, sign * -x1e.x) * Mathf.Rad2Deg; // 上腕のZ軸周り回転[deg]
                upperArmRoll.SetServoValue(a1);

                Vector3 x3 = lowerArmRoll.transform.position;   // 肘座標
                Quaternion invUpperArmRotation = Quaternion.Inverse(upperArmRoll.normalizedRotation);
                Vector3 x3h = invUpperArmRotation * (handTarget.position - x3);    // 肘から手首目標点へ向かうベクトル

                // 閾値より手首目標点が肘に近すぎる場合は、肘から先は処理しない
                if (x3h.sqrMagnitude < sqrMinDistance)
                {
                    return;
                }

                float a2 = Mathf.Atan2(-x3h.x, -x3h.z) * Mathf.Rad2Deg; // 上腕のX軸周り回転[deg]
                upperArmPitch.SetServoValue(a2);

                Vector3 x4 = handPitch.transform.position;
                float l3h = Mathf.Sqrt(x3h.z * x3h.z + x3h.x * x3h.x);
                float a3 = Mathf.Atan2(sign * -l3h, x3h.y) * Mathf.Rad2Deg; // 前腕のZ軸周り回転[deg]
                lowerArmRoll.SetServoValue(a3);
            }

            /// <summary>
            /// 手先座標優先モードで腕のIKを解き、順次サーボ角度を設定していく
            /// </summary>
            private void ApplyIK_HandFirst()
            {
                const float sqrMinDistance = 0.000025f; // 肩に近づきすぎた目標点は無視するための閾値 [m^2]
                const float maxExtensionAngle = 10f;    // 過伸展の最大角度 [deg]
                const float maxExtensionLength = 0.05f; // 腕を伸ばしてもこれ以上の距離があれば最大過伸展とする [m]

                float sign = (isRightSide ? 1f : -1f);  // 左右の腕による方向入れ替え用

                Vector3 x0 = shoulderPitch.transform.position;  // UpperArmJointの座標
                Quaternion invBaseRotation = Quaternion.Inverse(baseTransform.rotation);
                Vector3 x0h = invBaseRotation * (handTarget.position - x0);    // x0から手先目標点までのベクトル

                #region 肩関節の回転を求める
                float zh = x0h.z;
                float yh = x0h.y;
                float yt, zt;
                float w = zShoulderToHand;
                float sqrW = w * w;

                float sqrXh = zh * zh + yh * yh;
                float a0;

                if (Mathf.Approximately(sqrXh, sqrW))
                {
                    a0 = sign * Mathf.Atan2(-yh, zh) * Mathf.Rad2Deg; // 肩のX軸周り回転[deg]
                }
                else if (sqrXh < sqrW)
                {
                    // 近すぎるので回転なし
                    a0 = 0f;
                }
                else
                {
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
                    a0 = sign * Mathf.Atan2(-yt, zt) * Mathf.Rad2Deg;
                }
                #endregion
                shoulderPitch.SetServoValue(a0);

                Vector3 x1c = upperArmRoll.transform.position + shoulderPitch.normalizedRotation * Vector3.forward * zShoulderToHand;
                Quaternion invShoulderRotation = Quaternion.Inverse(shoulderPitch.normalizedRotation);
                Vector3 x1h = invShoulderRotation * (handTarget.position - x1c);

                float a1 = 0f;
                float a2 = 0f;
                float a3 = 0f;

                float x1h_sqrlen = x1h.sqrMagnitude;

                if (x1h_sqrlen <= (lengthUpperArm - lengthLowerArm) * (lengthUpperArm - lengthLowerArm))
                {
                    // 上腕始点に手首目標点が近すぎて三角形にならない場合
                    a3 = sign * 135f;
                    a1 = sign * 0f;
                }
                else if (x1h_sqrlen >= (lengthUpperArm + lengthLowerArm) * (lengthUpperArm + lengthLowerArm))
                {
                    // 腕を伸ばした以上に手首目標点が遠くて三角形にならない場合
                    
                    // 手を伸ばすときには、あえて過伸展として肘を逆に曲げる
                    float overlen = Mathf.Sqrt(x1h_sqrlen) - (lengthUpperArm + lengthLowerArm);
                    a3 = -sign * maxExtensionAngle * Mathf.Clamp01((overlen - maxExtensionLength) / maxExtensionLength);
                    a1 = -sign * Mathf.Atan2(x1h.y, sign * x1h.x) * Mathf.Rad2Deg - a3 / 2f;
                }
                else
                {
                    // 三角形になる場合、余弦定理により肘の角度を求める
                    float cosx3 = (lengthUpperArm * lengthUpperArm + lengthLowerArm * lengthLowerArm - x1h_sqrlen) / (2f * lengthUpperArm * lengthLowerArm);
                    a3 = -sign * (Mathf.Acos(cosx3) * Mathf.Rad2Deg - 180f);
                    float cosa1d = (lengthUpperArm * lengthUpperArm + x1h_sqrlen - lengthLowerArm * lengthLowerArm) / (2f * lengthUpperArm * Mathf.Sqrt(x1h_sqrlen));
                    float a1sub = Mathf.Acos(cosa1d) * Mathf.Rad2Deg;
                    a1 = -sign * (Mathf.Atan2(x1h.y, sign * x1h.x) * Mathf.Rad2Deg + a1sub);
                }
                upperArmRoll.SetServoValue(a1);
                upperArmPitch.SetServoValue(a2);
                lowerArmRoll.SetServoValue(a3);

                // 手首回転
                Quaternion invWristRotation = Quaternion.Inverse(lowerArmRoll.normalizedRotation);
                Quaternion rot = invWristRotation * handTarget.rotation;
                ApplyPartialRotation(rot, handPitch);
            }

            public override void DrawGizmos()
            {
                float gizmoRadius = 0.01f;

                // 肘優先モードならば、肘のギズモも表示
                if (method == ArmIK.Mode.Elbow)
                {
                    Gizmos.color = Color.red;

                    if (elbowTarget)
                    {
                        Gizmos.DrawLine(lowerArmRoll.transform.position, elbowTarget.position);
                        Gizmos.DrawSphere(elbowTarget.position, gizmoRadius);
                    }
                }

                if (method == ArmIK.Mode.Elbow)
                {
                    // 肘優先なら、手のギズモは別の色にしておく
                    Gizmos.color = Color.yellow;
                }
                else
                {
                    Gizmos.color = Color.red;
                }

                if (handTarget)
                {
                    Gizmos.DrawLine(handPitch.transform.position, handTarget.position);
                    Gizmos.DrawSphere(handTarget.position, gizmoRadius);
                }

            }
        }

        /// <summary>
        /// 脚IKソルバ
        /// </summary>
        public class LegIK
        {
            private Transform baseTransform;
            public ModelJoint upperLegYaw;      // x0
            public ModelJoint upperLegRoll;     // x1
            public ModelJoint upperLegPitch;    // x2
            public ModelJoint kneePitch;        // x3
            public ModelJoint anklePitch;       // x4
            public ModelJoint footRoll;         // x5
            private Vector3 footEndPos;         // x6

            public Transform footTarget;

            /// <summary>
            /// 右脚なら true、左脚なら false にしておく
            /// </summary>
            public bool isRightSide = false;

            public Mode method = Mode.None;

            private Vector3 xo01, xo12, xo23, xo34, xo45, xo15, xo06;

            /// <summary>
            /// アルゴリズムの指定
            /// </summary>
            public enum Mode
            {
                None,   // IKを利用しない
                Sole,   // ターゲットに足の裏が付くようにします
            }

            public void Initialize()
            {
                baseTransform = upperLegYaw.transform.parent;

                footEndPos = footRoll.transform.position;
                footEndPos.y = baseTransform.position.y;        // 初期状態は水平であるとして、足裏はbaseTransformの高さだとする

                // 目標が無ければ自動生成
                if (!footTarget)
                {
                    var obj = new GameObject((isRightSide ? "Right" : "Left") + "FootTarget");
                    footTarget = obj.transform;
                    footTarget.parent = baseTransform;
                    footTarget.position = footEndPos;
                    footTarget.rotation = baseTransform.rotation;
                }

                // ※これが呼ばれる時点（初期状態）ではモデルは T-Pose であること
                Quaternion invBaseRotation = Quaternion.Inverse(baseTransform.rotation);
                xo12 = invBaseRotation * (upperLegPitch.transform.position - upperLegRoll.transform.position);
                xo23 = invBaseRotation * (kneePitch.transform.position - upperLegPitch.transform.position);
                xo34 = invBaseRotation * (anklePitch.transform.position - kneePitch.transform.position);
                xo45 = invBaseRotation * (footRoll.transform.position - anklePitch.transform.position);
                xo15 = invBaseRotation * (footRoll.transform.position - upperLegRoll.transform.position);
                xo06 = invBaseRotation * (footEndPos - upperLegYaw.transform.position);
            }

            public void ApplyIK()
            {
                if (!footTarget) return;

                // これ以下に元位置に近づきすぎた目標は無視する閾値
                const float sqrMinDistance = 0.000025f;   // [m^2]
                const float a1Limit = 0.1f;               // [deg]

                float sign = (isRightSide ? -1f : 1f);  // 左右の腕による方向入れ替え用

                Quaternion invBaseRotation = Quaternion.Inverse(baseTransform.rotation);

                Quaternion targetRotation = invBaseRotation * footTarget.rotation;

                // Unityでは Z,Y,X の順番なので、ロボットとは一致しないはず
                Vector3 rt = targetRotation.eulerAngles;
                float yaw = -rt.y;
                float pitch = -rt.x;
                float roll = -rt.z;

                float a0, a1, a2, a3, a4, a5;
                a0 = yaw;

                Vector3 dx = invBaseRotation * (upperLegYaw.transform.position + (baseTransform.rotation * xo06) - footTarget.position);    // 初期姿勢時足元に対する足目標の変位
                dx = Quaternion.AngleAxis(-a0, Vector3.up) * dx;     // ヨーがある場合は目標変位も座標変換

                dx.z = 0f;  // ひとまず Z は無視してのIKを実装

                if (dx.sqrMagnitude < sqrMinDistance)
                {
                    // 目標が直立姿勢に近ければ、指令値はゼロとする
                    a1 = a2 = a3 = a4 = a5 = 0f;
                }
                else
                {
                    a1 = Mathf.Atan2(dx.x, -xo15.y + dx.y) * Mathf.Rad2Deg;
                    a5 = -a1;

                    //float len15 = dx.x / Mathf.Sin(a1 * Mathf.Deg2Rad); // 屈伸した状態での x1-x5 間長さ // sinだと a1==0 のとき失敗する
                    float len15 = (-xo15.y + dx.y) / Mathf.Cos(a1 * Mathf.Deg2Rad); // 屈伸した状態での x1-x5 間長さ
                    float cosa2 = (len15 + xo12.y + xo45.y) / (-xo23.y - xo34.y);
                    if (cosa2 >= 1f)
                    {
                        // 可動範囲以上に伸ばされそうな場合
                        a2 = 0f;
                    }
                    else if (cosa2 <= 0f)
                    {
                        // 完全に屈曲よりもさらに下げられそうな場合
                        a2 = sign * 90f;
                    }
                    else
                    {
                        // 屈伸の形に収まりそうな場合
                        a2 = sign * Mathf.Acos(cosa2) * Mathf.Rad2Deg;
                    }
                    a4 = a2;  // ひとまず、 a4 は a2 と同じとなる動作のみ可
                    a3 = a2 + a4;
                }

                upperLegYaw.SetServoValue(a0);
                upperLegRoll.SetServoValue(a1);
                upperLegPitch.SetServoValue(a2);
                kneePitch.SetServoValue(a3);
                anklePitch.SetServoValue(a4);
                footRoll.SetServoValue(a5);

            }

            public void DrawGizmos()
            {
                Vector3 gizmoSize = new Vector3(0.02f, 0.002f, 0.05f);

                Gizmos.color = Color.red;

                if (footTarget)
                {
                    Gizmos.DrawLine(footEndPos, footTarget.position);
                    //Gizmos.DrawCube(footTarget.position, gizmoSize);

                    var matrix = Gizmos.matrix;
                    Gizmos.matrix = Matrix4x4.TRS(footTarget.position, footTarget.rotation, Vector3.one);
                    Gizmos.DrawCube(Vector3.zero, gizmoSize);
                    Gizmos.matrix = matrix;
                }
            }
        }
        #endregion

        [Tooltip("ロボットモデルです。それ自体にアタッチされていれば未指定で構いません")]
        public Transform premaidRoot;


        [Tooltip("頭IKの解き方"), Header("Head")]
        public HeadIK.Mode headIkMode = HeadIK.Mode.Gaze;

        [FormerlySerializedAs("headTarget")]
        [Tooltip("Gazeでは見つめる先となります。Orientationでは無視されます")]
        public Transform headGazeTarget;

        [Tooltip("Orientationでの姿勢目標です。Gazeではロール軸に反映されます")]
        public Transform HeadOrientationTarget;


        [Tooltip("腕IKの解き方"), Header("Arms")]
        public ArmIK.Mode armIkMode = ArmIK.Mode.Hand;

        [Tooltip("左手先目標です。未指定ならば自動生成します")]
        public Transform leftHandTarget;

        [Tooltip("右手先目標です。未指定ならば自動生成します")]
        public Transform rightHandTarget;

        [Tooltip("左手肘目標です。Hand基準では使われません。未指定ならば自動生成します")]
        public Transform leftElbowTarget;

        [Tooltip("右手肘目標です。Hand基準では使われません。未指定ならば自動生成します")]
        public Transform rightElbowTarget;


        [Tooltip("脚IKの解き方"), Header("Legs")]
        public LegIK.Mode legIkMode = LegIK.Mode.Sole;

        [Tooltip("左脚目標です。未指定ならば自動生成します")]
        public Transform leftFootTarget;

        [Tooltip("右脚目標です。未指定ならば自動生成します")]
        public Transform rightFootTarget;

        private HeadIK headIK;
        private ArmIK leftArmIK;
        private ArmIK rightArmIK;
        private LegIK leftLegIK;
        private LegIK rightLegIK;

        // 左右の手、頭の最終的なTransformを取得できるようにします
        public Transform leftHandTransform { get { return leftArmIK.lowerArmRoll.transform; } }
        public Transform rightHandTransform { get { return rightArmIK.lowerArmRoll.transform; } }
        public Transform headTransform { get { return headIK.headRoll.transform; } }

        private ModelJoint[] _joints;

        // Start is called before the first frame update
        void Start()
        {
            if (!premaidRoot)
            {
                // 未指定ならモデルのルートにこのスクリプトがアタッチされているものとする
                premaidRoot = transform;
            }

            if (premaidRoot != null)
            {
                _joints = premaidRoot.GetComponentsInChildren<ModelJoint>();
            }

            // 頭部IKソルバを準備
            headIK = new HeadIK();
            headIK.neckYaw = GetJointById("05");
            headIK.headPitch = GetJointById("03");
            headIK.headRoll = GetJointById("07");
            headIK.headBaseTarget = headGazeTarget;
            headIK.headOrientationTarget = HeadOrientationTarget;
            headIK.Initialize();
            headGazeTarget = headIK.headBaseTarget;         // 自動生成されていたら、controller側に代入
            HeadOrientationTarget = headIK.headOrientationTarget;         // 自動生成されていたら、controller側に代入

            // 左腕IKソルバを準備
            leftArmIK = new ArmIK();
            leftArmIK.isRightSide = false;
            leftArmIK.shoulderPitch = GetJointById("04");
            leftArmIK.upperArmRoll = GetJointById("0B");
            leftArmIK.upperArmPitch = GetJointById("0F");
            leftArmIK.lowerArmRoll = GetJointById("13");
            leftArmIK.handPitch = GetJointById("17");
            leftArmIK.handTarget = leftHandTarget;
            leftArmIK.elbowTarget = leftElbowTarget;
            leftArmIK.Initialize();
            leftElbowTarget = leftArmIK.elbowTarget;  // 自動生成されていたら、controller側に代入
            leftHandTarget = leftArmIK.handTarget;    // 自動生成されていたら、controller側に代入

            // 右腕IKソルバを準備
            rightArmIK = new ArmIK();
            rightArmIK.isRightSide = true;
            rightArmIK.shoulderPitch = GetJointById("02");
            rightArmIK.upperArmRoll = GetJointById("09");
            rightArmIK.upperArmPitch = GetJointById("0D");
            rightArmIK.lowerArmRoll = GetJointById("11");
            rightArmIK.handPitch = GetJointById("15");
            rightArmIK.handTarget = rightHandTarget;
            rightArmIK.elbowTarget = rightElbowTarget;
            rightArmIK.Initialize();
            rightElbowTarget = rightArmIK.elbowTarget;    // 自動生成されていたら、controller側に代入
            rightHandTarget = rightArmIK.handTarget;      // 自動生成されていたら、controller側に代入

            // 左脚IKソルバを準備
            leftLegIK = new LegIK();
            leftLegIK.isRightSide = false;
            leftLegIK.upperLegYaw = GetJointById("08");
            leftLegIK.upperLegRoll = GetJointById("0C");
            leftLegIK.upperLegPitch = GetJointById("10");
            leftLegIK.kneePitch = GetJointById("14");
            leftLegIK.anklePitch = GetJointById("18");
            leftLegIK.footRoll = GetJointById("1C");
            leftLegIK.footTarget = leftFootTarget;
            leftLegIK.Initialize();
            leftFootTarget = leftLegIK.footTarget;

            // 右脚IKソルバを準備
            rightLegIK = new LegIK();
            rightLegIK.isRightSide = true;
            rightLegIK.upperLegYaw = GetJointById("06");
            rightLegIK.upperLegRoll = GetJointById("0A");
            rightLegIK.upperLegPitch = GetJointById("0E");
            rightLegIK.kneePitch = GetJointById("12");
            rightLegIK.anklePitch = GetJointById("16");
            rightLegIK.footRoll = GetJointById("1A");
            rightLegIK.footTarget = rightFootTarget;
            rightLegIK.Initialize();
            rightFootTarget = rightLegIK.footTarget;
        }

        /// <summary>
        /// "0C"などのサーボIDから該当するModelJointを返す
        /// </summary>
        /// <param name="servoId"></param>
        /// <returns></returns>
        ModelJoint GetJointById(string servoId)
        {
            foreach (var joint in _joints)
            {
                if (joint.ServoID.Equals(servoId)) return joint;
            }
            return null;
        }

        /// <summary>
        /// 毎フレームでのIK処理
        /// </summary>
        void LateUpdate()
        {
            leftLegIK.method = legIkMode;
            leftLegIK.ApplyIK();

            rightLegIK.method = legIkMode;
            rightLegIK.ApplyIK();

            leftArmIK.method = armIkMode;
            leftArmIK.ApplyIK();

            rightArmIK.method = armIkMode;
            rightArmIK.ApplyIK();

            headIK.method = headIkMode;
            headIK.ApplyIK();
        }

        /// <summary>
        /// ギズモの描画
        /// </summary>
        private void OnDrawGizmos()
        {
            if (leftLegIK != null) leftLegIK.DrawGizmos();
            if (rightLegIK != null) rightLegIK.DrawGizmos();
            if (leftArmIK != null) leftArmIK.DrawGizmos();
            if (rightArmIK != null) rightArmIK.DrawGizmos();
            if (headIK != null) headIK.DrawGizmos();
        }
    }
}
