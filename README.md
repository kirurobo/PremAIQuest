# PremaindAI_TechVerification
プリメイドAIのダンスモーションデータの解析プロジェクトです。
総サーボ数（自由度）　25軸のヒューマノイドロボットが、値下げによって2万円で手に入るようになった為、とても魅力的なおもちゃです。

http://robots.dmm.com/robot/premaidai

モーションデータ形式は *.pma という拡張子でAndroidアプリ上でダンスモーションデータをダウンロードすると  
Android\data\com.robotyuenchi.premaidai\  
以下に保存されています。  

# 現在できる事
## モーションデータの読み込みとプレビュー
MotionDataLoadTestScene.unity を開いて*.pmaファイルを読み込み、矢印キーの左右でフレーム移動をします。
GameView上のプリメイドAIを模した箱なロボットがpmaのサーボ値を元にポーズを取ります

# 現在やっていること
## モーションプレビューの誤りを直す
腕と首はおそらくそれほど間違っていないプレビューですが、足の軸反転やオフセットがあるようです。  
これを直したい

## 一部フレームを飛ばしたモーションデータの生成
ブラックボックス化しているpmaファイルの先頭と末尾はそのままに、中間モーションを削っても再生できることは確認済みです。
その処理をUnity内で出来るようにしたい

## フレームウェイト値の反映によるリアルタイムモーションプレビュー
待ち時間を考慮して再生する仕組みを作る

# 動作環境
- Windows 10
- Unity 2018.3.14f 

# 関連URL
信号解析については以下のgoogle spreadsheet 上で編集中です。  

https://docs.google.com/spreadsheets/d/1c6jqMwkBroCuF74viU_q7dgSQGzacbUW4mJg-957_Rs/edit#gid=2102495394

