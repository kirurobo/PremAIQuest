# 注意
もし本当に動かそうとする場合は、ワイヤーで体を吊るすなど安全策をとる必要があります。  
上半身のみに指令を出している際に下半身で支える動作ができません。


# これは何
プリメイドAIを Oculus Quest から操縦しようとしてみたプロジェクトです。

[@izm](https://twitter.com/izm) さんの [PremaindAI_TechVerification](https://github.com/neon-izm/PremaindAI_TechVerification) を基にしています。
プリメイドAIの解析についてはそちらをご覧ください。

# 特徴
無改造の プリメイドAI と Oculus Quest だけで動作します。  
PCは通さず直接 Bluetooth で接続します。あらかじめペアリングを済ませておく必要があります。

2020/10/31現在、Quest2 ではペアリングができません。  
Quest1 では、先にキーボードを接続しておくことでペアリングが可能です。



# ビルド環境（必要アセット）
- Unity 2019.4.13f 
- [Serial Port Utility Pro](https://assetstore.unity.com/packages/tools/utilities/serial-port-utility-pro-125863) 2.3
- [Oculus Integration](https://assetstore.unity.com/packages/tools/integration/oculus-integration-82022) 20.1

# 関連URL
信号解析については以下のgoogle spreadsheet 上で編集中です。  
https://docs.google.com/spreadsheets/d/1c6jqMwkBroCuF74viU_q7dgSQGzacbUW4mJg-957_Rs/edit#gid=2102495394

# 同梱モデルデータについて
[黒イワシ(twitter:@Schwarz_Sardine)さんのモデル](https://github.com/kuroiwasi/PremaidAI_Model
)を基のプロジェクトから引き続き利用させていただいています。

# Contributors
- [@GOROman](https://twitter.com/GOROman) … 通信形式解析
- [@izm](https://twitter.com/izm) … Unityで動作プロジェクト作成, 公開
- [@Schwarz_Sardine](https://twitter.com/Schwarz_Sardine) … FBXモデル作成,高精度FBXモデルの作成
- [@kazzlog](https://twitter.com/kazzlog) … 直接ポーズ送信の発見, バッテリー残量問い合わせの発見,サーボのストレッチ、スピードパラメータ制御の発見,2桁シリアルポートバグの修正
- [@shi_k_7](https://twitter.com/shi_k_7) … 可動域修正、minorbugfix
