using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PreMaid
{
    public class ServoConstants
    {
        /// <summary>
        /// サーボの取り付け位置
        /// </summary>
        public enum ServoPosition
        {
            RightShoulderPitch = 0x02,  //肩ピッチR
            HeadPitch = 0x03,           //頭ピッチ
            LeftShoulderPitch = 0x04,   //肩ピッチL
            HeadYaw = 0x05,             //頭ヨー
            RightHipYaw = 0x06,         //ヒップヨーR
            HeadRoll = 0x07             /*萌え軸*/,
            LeftHipYaw = 0x08,          //ヒップヨーL
            RightShoulderRoll = 0x09,   //肩ロールR
            RightHipRoll = 0x0A,        //ヒップロールR
            LeftShoulderRoll = 0x0B,    //肩ロールL
            LeftHipRoll = 0x0C,         //ヒップロールL
            RightUpperArmYaw = 0x0D,    //上腕ヨーR
            RightUpperLegPitch = 0x0E,  //腿ピッチR
            LeftUpperArmYaw = 0x0F,     //上腕ヨーL
            LeftUpperLegPitch = 0x10,   //腿ピッチL
            RightLowerArmPitch = 0x11,  //肘ピッチR
            RightLowerLegPitch = 0x12,  //膝ピッチR
            LeftLowerArmPitch = 0x13,   //肘ピッチL
            LeftLowerLegPitch = 0x14,   //肘ピッチL
            RightHandYaw = 0x15,        //手首ヨーR
            RightFootPitch = 0x16,      //足首ピッチR
            LeftHandYaw = 0x17,         //手首ヨーL
            LeftFootPitch = 0x18,       //足首ピッチL
            RightFootRoll = 0x1A,       //足首ロールR
            LeftFootRoll = 0x1C,        //足首ロールL
        }

        //public static readonly uint fullBodyMask = 0;

        [Flags]
        public enum ServosMask
        {
            None = 0,
            UppeerBody = 1,
            LowerBody = 2,
        }


        /// <summary>
        /// 全身の関節すべて
        /// </summary>
        public static readonly ServoPosition[] fullBodyServoPositions =
        {
            ServoPosition.HeadYaw,
            ServoPosition.HeadPitch,
            ServoPosition.HeadRoll,
            ServoPosition.RightShoulderPitch,
            ServoPosition.RightShoulderRoll,
            ServoPosition.RightUpperArmYaw,
            ServoPosition.RightLowerArmPitch,
            ServoPosition.RightHandYaw,
            ServoPosition.LeftShoulderPitch,
            ServoPosition.LeftShoulderRoll,
            ServoPosition.LeftUpperArmYaw,
            ServoPosition.LeftLowerArmPitch,
            ServoPosition.RightHipYaw,
            ServoPosition.RightHipRoll,
            ServoPosition.RightUpperLegPitch,
            ServoPosition.RightLowerLegPitch,
            ServoPosition.RightFootPitch,
            ServoPosition.RightFootRoll,
            ServoPosition.LeftHipYaw,
            ServoPosition.LeftHipRoll,
            ServoPosition.LeftUpperLegPitch,
            ServoPosition.LeftLowerLegPitch,
            ServoPosition.LeftHandYaw,
            ServoPosition.LeftFootPitch,
            ServoPosition.LeftFootRoll,
        };

        /// <summary>
        /// 上半身の関節
        /// </summary>
        public static readonly ServoPosition[] upperBodyServoPositions =
        {
            ServoPosition.HeadYaw,
            ServoPosition.HeadPitch,
            ServoPosition.HeadRoll,
            ServoPosition.RightShoulderPitch,
            ServoPosition.RightShoulderRoll,
            ServoPosition.RightUpperArmYaw,
            ServoPosition.RightLowerArmPitch,
            ServoPosition.RightHandYaw,
            ServoPosition.LeftShoulderPitch,
            ServoPosition.LeftShoulderRoll,
            ServoPosition.LeftUpperArmYaw,
            ServoPosition.LeftLowerArmPitch,
        };

        /// <summary>
        /// 下半身の関節すべて
        /// </summary>
        public static readonly ServoPosition[] lowerBodyServoPositions =
        {
            ServoPosition.RightHipYaw,
            ServoPosition.RightHipRoll,
            ServoPosition.RightUpperLegPitch,
            ServoPosition.RightLowerLegPitch,
            ServoPosition.RightFootPitch,
            ServoPosition.RightFootRoll,
            ServoPosition.LeftHipYaw,
            ServoPosition.LeftHipRoll,
            ServoPosition.LeftUpperLegPitch,
            ServoPosition.LeftLowerLegPitch,
            ServoPosition.LeftHandYaw,
            ServoPosition.LeftFootPitch,
            ServoPosition.LeftFootRoll,
        };
    }
}
