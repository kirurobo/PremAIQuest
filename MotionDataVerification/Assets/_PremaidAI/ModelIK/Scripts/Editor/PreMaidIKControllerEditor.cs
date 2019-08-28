using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PreMaid
{
    [CustomEditor(typeof(PreMaidIkController))]
    public class PreMaidIKControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }

        private void OnSceneGUI()
        {
            const float diskSize = 0.03f;   // 回転ハンドルのサイズ
            const float diskSnap = 1f;      // スナップするグリッドサイズとのことですが、よくわからない

            PreMaidIkController controller = (PreMaidIkController)target;
            if (!controller || !controller.premaidRoot) return;

            Vector3 leftTargetRotAxis =
                Quaternion.Inverse(controller.premaidRoot.rotation)
                * controller.leftHandTransform.rotation
                * Vector3.left;

            Vector3 rightTargetRotAxis =
                Quaternion.Inverse(controller.premaidRoot.rotation)
                * controller.leftHandTransform.rotation
                * Vector3.right;

            Vector3 headTargetRotAxis =
                Quaternion.Inverse(controller.premaidRoot.rotation)
                * controller.headTransform.rotation
                * Vector3.forward;

            // 腕部目標のハンドル
            if (controller.armIkMode == PreMaidIkController.ArmIK.Mode.Elbow)
            {
                // 肘と手首に

                if (!controller.leftHandTarget || !controller.rightHandTarget || !controller.leftElbowTarget || !controller.rightElbowTarget) return;

                EditorGUI.BeginChangeCheck();

                Vector3 leftWristPos = Handles.PositionHandle(controller.leftHandTarget.position, Quaternion.identity);
                Vector3 rightWristPos = Handles.PositionHandle(controller.rightHandTarget.position, Quaternion.identity);
                Vector3 leftElbowPos = Handles.PositionHandle(controller.leftElbowTarget.position, Quaternion.identity);
                Vector3 rightElbowPos = Handles.PositionHandle(controller.rightElbowTarget.position, Quaternion.identity);
                Quaternion leftHandRot = Handles.Disc(
                    controller.leftHandTarget.rotation,
                    controller.leftHandTarget.position,
                    leftTargetRotAxis,
                    diskSize, false, diskSnap
                    );
                Quaternion rightHandRot = Handles.Disc(
                    controller.rightHandTarget.rotation,
                    controller.rightHandTarget.position,
                    rightTargetRotAxis,
                    diskSize, false, diskSnap
                    );

                if (EditorGUI.EndChangeCheck())
                {
                    controller.leftHandTarget.position = leftWristPos;
                    controller.rightHandTarget.position = rightWristPos;
                    controller.leftElbowTarget.position = leftElbowPos;
                    controller.rightElbowTarget.position = rightElbowPos;
                    controller.leftHandTarget.rotation = leftHandRot;
                    controller.rightHandTarget.rotation = rightHandRot;
                }
            }
            else
            {
                if (!controller.leftHandTarget || !controller.rightHandTarget) return;

                EditorGUI.BeginChangeCheck();
                Vector3 leftWristPos = Handles.PositionHandle(controller.leftHandTarget.position, Quaternion.identity);
                Vector3 rightWristPos = Handles.PositionHandle(controller.rightHandTarget.position, Quaternion.identity);
                Quaternion leftHandRot = Handles.Disc(
                    controller.leftHandTarget.rotation,
                    controller.leftHandTarget.position,
                    leftTargetRotAxis,
                    diskSize, false, diskSnap
                    );
                Quaternion rightHandRot = Handles.Disc(
                    controller.rightHandTarget.rotation,
                    controller.rightHandTarget.position,
                    rightTargetRotAxis,
                    diskSize, false, diskSnap
                    );

                if (EditorGUI.EndChangeCheck())
                {
                    controller.leftHandTarget.position = leftWristPos;
                    controller.rightHandTarget.position = rightWristPos;
                    controller.leftHandTarget.rotation = leftHandRot;
                    controller.rightHandTarget.rotation = rightHandRot;
                }
            }

            // 頭部目標のハンドル
            if (controller.headTarget)
            {
                switch (controller.headIkMode)
                {
                    case PreMaidIkController.HeadIK.Mode.Gaze:
                        EditorGUI.BeginChangeCheck();
                        Vector3 headTargetPos = Handles.PositionHandle(controller.headTarget.position, Quaternion.identity);
                        if (EditorGUI.EndChangeCheck())
                        {
                            controller.headTarget.position = headTargetPos;
                        }
                        EditorGUI.BeginChangeCheck();

                        Vector3 headOrientationTargetPos = Handles.PositionHandle(controller.HeadOrientationTarget.position, Quaternion.identity);
                        if (EditorGUI.EndChangeCheck())
                        {
                            controller.HeadOrientationTarget.position = headOrientationTargetPos;
                        }
                        break;
                    case PreMaidIkController.HeadIK.Mode.Rotation:
                        //// 基部はハンドルを出さない
                        //EditorGUI.BeginChangeCheck();
                        //Quaternion headBaseRot = Handles.RotationHandle(
                        //    controller.headTarget.rotation, controller.headTarget.position
                        //    );
                        //if (EditorGUI.EndChangeCheck())
                        //{
                        //    controller.headTarget.rotation = headBaseRot;
                        //}
                        EditorGUI.BeginChangeCheck();
                        Quaternion headOrientationRot = Handles.RotationHandle(
                            controller.HeadOrientationTarget.rotation, controller.HeadOrientationTarget.position
                            );
                        if (EditorGUI.EndChangeCheck())
                        {
                            controller.HeadOrientationTarget.rotation = headOrientationRot;
                        }
                        break;
                }
            }
        }
    }
}
