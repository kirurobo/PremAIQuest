using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PreMaid
{
    [CustomEditor(typeof(PreMaidIKController))]
    public class PreMaidIKControllerEditor : Editor
    {
        private void OnSceneGUI()
        {
            const float diskSize = 0.03f;   // 回転ハンドルのサイズ
            const float diskSnap = 1f;      // スナップするグリッドサイズとのことですが、よくわからない

            PreMaidIKController controller = (PreMaidIKController)target;
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

            if (controller.priorJoint == PreMaidIKController.ArmIK.PriorJoint.Elbow)
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

            if (controller.headTarget)
            {
                EditorGUI.BeginChangeCheck();
                Vector3 headTargetPos = Handles.PositionHandle(controller.headTarget.position, Quaternion.identity);
                Quaternion headRot = Handles.Disc(
                    controller.headTarget.rotation,
                    controller.headTarget.position,
                    headTargetRotAxis,
                    diskSize, false, diskSnap
                    );

                if (EditorGUI.EndChangeCheck())
                {
                    controller.headTarget.position = headTargetPos;
                    controller.headTarget.rotation = headRot;
                }
            }
        }
    }
}
