﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace PreMaid
{
    public class PreMaidUtility : MonoBehaviour
    {
        /// <summary>
        /// 末尾のチェックバイトを計算して、正しい値に書き換えます
        /// 05 1F 00 01 FF を渡したら05 1F 00 01 1Bが返ってきます
        /// </summary>
        /// <param name="spaceSplitedByteString"></param>
        /// <returns></returns>
        public static string RewriteXorString(string spaceSplitedByteString)
        {
            var hexBase = BuildByteDataFromStringOrder(spaceSplitedByteString);
            byte xor = new byte();
            for (int i = 0; i < hexBase.Length - 1; i++)
            {
                xor ^= (byte) (hexBase[i]);
            }

            var str = string.Format("{0:X2}", xor);
            var retString = spaceSplitedByteString.Substring(0, spaceSplitedByteString.Length - 2);
            retString += str;
            //Debug.Log(str);
            return retString;
        }

        /// <summary>
        /// 末尾のチェックバイトを計算します
        /// 05 1F 00 01 を渡したら1Bが返ってきます
        /// </summary>
        /// <param name="spaceSplitedByteString"></param>
        /// <returns></returns>
        public string CalcXorString(string spaceSplitedByteString)
        {
            var hexBase = BuildByteDataFromStringOrder(spaceSplitedByteString);
            byte xor = new byte();
            for (int i = 0; i < hexBase.Length; i++)
            {
                xor ^= (byte) (hexBase[i]);
            }

            var str = string.Format("{0:X2}", xor);
            //Debug.Log(str);
            return str;
        }

        /// <summary>
        /// byte[]の中身を直接文字列としてDebug.Logに出す
        /// 0xff0x63の場合は、FF63と表示される
        /// </summary>
        /// <param name="hex"></param>
        public static void DumpDebugLogToHex(byte[] hex)
        {
            string str = "";
            for (int i = 0; i < hex.Length; i++)
            {
                str += string.Format("{0:X2}", hex[i]);
            }

            Debug.Log(str);
        }

        public static string DumpBytesToHexString(byte[] hex, int length)
        {
            string str = "";
            for (int i = 0; i < length; i++)
            {
                str += string.Format("{0:X2}", hex[i]);
            }

            return str;
        }

        /// <summary>
        /// "1F"を渡したら25が返ってくるやつ
        /// </summary>
        /// <returns></returns>
        public static int HexStringToInt(string hex)
        {
            if (hex.Length > 4)
            {
                throw new InvalidOperationException("入力文字数は4文字以内を想定しています");
            }

            var ret = Int32.Parse(hex, NumberStyles.AllowHexSpecifier);
            return ret;
        }

        /// <summary>
        /// スペース区切りの文字列からbyte配列を作る。このコードを読む人はこれだけ使ってもらえれば！
        /// </summary>
        /// <param name="spaceSplitedByteString"></param>
        /// <returns></returns>
        public static byte[] BuildByteDataFromStringOrder(string spaceSplitedByteString)
        {
            var hexString = RemoveWhitespace(spaceSplitedByteString);

            return HexStringToByteArray(hexString);
        }

        /// <summary>
        /// "FF0063"みたいな文字列を0xff,0x00,0x63みたいなbyte配列にする
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static byte[] HexStringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }

        /// <summary>
        /// スペース区切りのモーション文字列をスペース消して返す
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string RemoveWhitespace(string input)
        {
            return new string(input.ToCharArray()
                .Where(c => !Char.IsWhiteSpace(c))
                .ToArray());
        }
        
        /// <summary>
        /// 4C1Dを入れたら1D4Cが返ってくる
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ConvertEndian(string data)
        {
            int sValueAsInt = int.Parse(data, System.Globalization.NumberStyles.HexNumber);
            byte[] bytes = BitConverter.GetBytes(sValueAsInt);
            string retval = "";
            foreach (byte b in bytes)
                retval += b.ToString("X2");
            return retval.Substring(0, 4);
        }
    }
}