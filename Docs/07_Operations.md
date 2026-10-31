# 07. 運用手順書

## 1. 開発環境

| 項目 | バージョン |
|---|---|
| Unity | 6000.6.6f1（Unity Hub からインストール） |
| レンダーパイプライン | URP 17.6 |
| 主要パッケージ | Input System 1.20、Test Framework 1.8、uGUI 2.6（UI Toolkit 同梱） |
| Blender（モデルを作り直す場合のみ） | 4.2 以降（5.2 で確認） |
| Git | 2.4x 以上 |

## 2. 初回セットアップ

1. `git clone https://github.com/sawai-naoki-portfolio/aPieceOfCake.git`
2. Unity Hub ▸ 追加 ▸ クローンしたフォルダを開く（`Library/` は自動生成）。
3. `Assets/PieceOfCake/Scenes/Main.unity` を開いて Play。

シーン・カタログ・マテリアルはリポジトリに含まれているので、通常は再生成不要です。

## 3. エディタメニュー（Tools ▸ Piece of Cake）

| メニュー | 内容 | 使う場面 |
|---|---|---|
| Rebuild Art Catalog | マテリアルの不足分を作成 → FBX の取り込み設定を修正 → `GameArt.asset` の参照を全部つなぎ直す | モデルを追加・作り直したとき |
| Rebuild Main Scene | 上の処理の後、`Main.unity` を作り直してビルド対象に設定 | シーンが壊れたとき、初期構成に戻したいとき |
| Validate Levels | `levels.json` を検証して結果をコンソールに出す | ステージを編集したとき |

- マテリアルは **存在しなければ作る** だけです。色を変えたいときは `Art/Materials/*.mat` を直接編集してください（再生成で上書きされません）。
- テーマ（空・地形の色など）は `Config/GameArt.asset` の `Themes` で編集します。

## 4. ビルド

1. File ▸ Build Profiles で対象プラットフォーム（Windows / macOS / Android / iOS / WebGL）を選ぶ。
2. Scene List に `Assets/PieceOfCake/Scenes/Main.unity` だけが入っていることを確認（`Rebuild Main Scene` で自動設定）。
3. Build。
4. Player Settings の `Company Name` / `Product Name` / バージョン（現在 0.1.0）はリリース前に設定してください。

## 5. セーブデータ

- 保存先：`PlayerPrefs`（Windows はレジストリ `HKCU\Software\<Company>\<Product>`）、キー `pieceofcake.progress.v1`。
- 消す：エディタでは Edit ▸ Clear All PlayerPrefs。
- 形式を変えるときはキーの末尾のバージョンを上げ、古いデータの移行処理を `PlayerPrefsProgressStore` に追加します。

## 6. トラブルシューティング

| 症状 | 原因と対処 |
|---|---|
| 起動時「Game Art is missing: …」 | カタログの参照切れ。Tools ▸ Piece of Cake ▸ Rebuild Art Catalog |
| 起動時「データを よみこめませんでした」 | `levels.json` の誤り。画面とコンソールに **ステージ ID と問題** が出るので修正し、Validate Levels で確認 |
| 日本語が □ になる | OS に日本語フォントがない。`FontLoader` が探すフォント（BIZ UDPGothic、Yu Gothic UI、メイリオ、ヒラギノ、Noto Sans CJK JP など）のどれかを入れるか、`Assets` にフォントを追加して `FontLoader` に登録 |
| エディタで一瞬ゼリーやアイスが見えない、水色になる | シェーダーの非同期コンパイル中の仮表示。数秒で直る。ビルドでは起きない |
| 「NullReferenceException while resolving binding …」 | 入力アクションの処理指定の誤り。`GameControls.inputactions` を Input Actions エディタで開いて修正 |
| UI が表示されない | `Main.unity` の UIDocument に PanelSettings が無い。Rebuild Main Scene |
| タッチボタンを PC で確認したい | `Game` オブジェクトの `GameBootstrap.forceTouchControls` をオン |

## 7. Git 運用

- `main` ブランチに小さな単位でコミットし、こまめにプッシュします。
- `.gitignore` で `Library/`、`Temp/`、`Logs/`、`UserSettings/`、生成される `.csproj` / `.sln` を除外しています。
- `.gitattributes` で改行を正規化し、Unity の YAML（シーン・プレハブ・アセット）は UnityYAMLMerge で合流できるようにしています。使う場合は Git に次を設定します：

```bash
git config merge.unityyamlmerge.name "Unity SmartMerge"
git config merge.unityyamlmerge.driver "'C:/Program Files/Unity/Hub/Editor/6000.6.6f1/Editor/Data/Tools/UnityYAMLMerge.exe' merge -p %O %B %A %A"
```

- 100MB を超えるファイルは GitHub に置けません。大きなテクスチャや音声を足すときは Git LFS を使ってください（現在の FBX はすべて 150KB 以下）。

## 8. 変更時のチェックリスト

- [ ] EditMode テストがすべて通る
- [ ] Validate Levels でエラーなし（ステージを触った場合）
- [ ] Play モードでタイトル → プレイ → 結果まで確認
- [ ] 必要ならスクリーンショットとドキュメントを更新
