# 注意
もし本当に動かそうとする場合は、ワイヤーでプリメイドAI胴体を吊るすなど安全策をとる必要があります。  
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


# 動かす場合
## インストール
[Releases](https://github.com/kirurobo/PremAIQuest/releases) にある apk をインストールしておきます。
[SideQuest](https://sidequestvr.com/)を使うと便利かも。

## ペアリング
### Oculus Quest（初代）の場合
1. プリメイドAIの電源を入れます
1. Questで歯車アイコンの設定画面 テスト機能 > Bluetoothペアリング のペアリング ボタン を押します
1. 「RNBT-○○○○」という機器がプリメイドAIです。番号は特に気にせずペア設定をすればOKです
![image](https://user-images.githubusercontent.com/1019117/98305065-7b912580-2004-11eb-9ac2-c776061fdd59.png)
![image](https://user-images.githubusercontent.com/1019117/98305642-9ca64600-2005-11eb-98b3-12899661ab52.png)

### Oculus Quest 2 の場合
テスト機能で Bluetoothペアリング はあるのですが、機器を選んだ後に「OK」に相当するボタンが表示されず、接続できません…。  
誰か接続方法をご存じないですかね…。

## 起動
アプリ一覧で「すべて」ではなく「提供元不明」を選ぶことで一覧が出ますので、起動できます。

## 操作
1. 画面が出たら、まず両手を左右に伸ばした状態で右コントローラの [A]+[B] 同時押しをするとキャリブレーションが行われます。表示が消えるまでその姿勢にしておきます。
1. メニューで「RNBT-○○○○」が選ばれていることを確認し、「OPEN」を押すと接続されます。
1. 接続されると頭部は常に Quest の向きに合わせて動作します。
1. コントローラの人差し指または中指部分のトリガーを押すと、押している間、手の位置と向きをIKで一致させるよう動作します。
1. スティックでCGモデルの位置を調整できます。
1. 左コントローラの [MENU] ボタンでメニューは表示／非表示にできます。
1. 「CLOSE」を押すと接続を閉じます。（CLOSEせずに終了しても大丈夫です。）


# ビルドする場合の環境（必要アセット）
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
