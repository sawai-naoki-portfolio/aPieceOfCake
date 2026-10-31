# 02. 定義書

用語・データ・定数・イベントの定義です。コード上の名前を併記します。

## 1. 用語集

| 用語 | コード | 意味 |
|---|---|---|
| セル | `Cell` | ケーキの 1 マス。種類（`CellType`）と溶け具合を持つ。切っても同じインスタンスのまま別ピースへ移る |
| ピース | `Piece` | くっついたセルの集まり。一緒に動く単位 |
| ごちゅうもん | `CellType.Order` / `Piece.IsOrder` | 届けるべきケーキ。食べられず、切れない |
| 地形 | `TileType` | 動かないマス（空・固い・フォーク） |
| ロス | `RunStats.Loss` | 食べた数 + マップ外へ落とした数 |
| 目標（par） | `LevelDefinition.Par` | ★3 になるロスの上限 |
| ミス | `StageSimulation.Misses` | シェフがやられた回数（ごちゅうもん落下は含まない） |
| チェックポイント | `Checkpoint` | ろうそく。触れると状態を保存 |
| ステップ | `StageSimulation.Step` | 1/60 秒の固定シミュレーション更新 |
| シミュレーション座標 | — | 1 マス = 40px、x は右、y は **下** が正、左上が原点（試作品と同じ） |
| ワールド座標 | `WorldMapper` | Unity 単位。1 マス = 1、y は上が正、マップ左下が原点、遊ぶ面は z = 0 |

## 2. マップ記号（levels.json の `map`）

| 記号 | 意味 | 生成されるもの |
|---|---|---|
| `.` | 空き | — |
| `#` | 固い地形 | `TileType.Solid` |
| `^` | フォーク（トゲ） | `TileType.Spike` |
| `@` | スタート（必ず 1 つ） | `LevelLayout.Start` |
| `G` | ゴール（お客さん、必ず 1 つ） | `LevelLayout.Goal` |
| `F` | チェックポイント | `Checkpoint` |
| `a` | アリ | `Ant` |
| `t` | 看板（説明文は `signs` から、列→行の順に割り当て） | `SignInfo` |
| `S C B K J I` | スポンジ・クリーム・いちご・チョコ・ゼリー・アイス。隣り合う大文字は 1 ピースになる | `Piece` |
| `s c b k j i` | 同じセルだが **独立したピース**（大文字と接していてもつながらない） | `Piece` |
| `O` | ごちゅうもん（1 つ以上。各 `O` が単独ピース） | `Piece`（`IsOrder`） |

## 3. 列挙型

### `CellType`
`Sponge, Cream, Strawberry, Chocolate, Jelly, Ice, Order`（ルールは [01_GameDesign.md](01_GameDesign.md#31-ケーキピース)）

### `TileType`
`Empty = 0, Solid = 1, Spike = 2`

### `GamePhase`
| 値 | 意味 |
|---|---|
| `Playing` | 通常プレイ |
| `Delivering` | 届けた後の演出（1.7 秒）。操作不可、時間は止まる |
| `Completed` | 終了。`StageSimulation.Result` が読める。`Step` は何もしない |

### `PieceState`
`Resting`（静止）, `Falling`（落下中）。アニメーション中（`SlideAnimation` / `ToppleAnimation`）は操作できない。

### `DeathCause`
`Spike, Fall, Ant, Crushed`

### `PlayerMessage`（シェフのセリフ。文言は `Texts.Speech`）
| 値 | 文言 |
|---|---|
| `CannotCutHere` | ここでは 切れない |
| `TooHeavy` | おもくて うごかない… |
| `ChocolateTooHard` | カチン！ チョコは かたくて 切れない |
| `OrderIsNotFood` | これは ごちゅうもんの しな！ |
| `StrawberryPower` | いちごパワー！ ジャンプ力アップ |
| `ChocolatePower` | チョコパワー！ ちからもち |
| `Yummy` | もぐもぐ／おいしい！／あまーい（食べた数で選ぶ） |
| `OrderPickedUp` | ごちゅうもん ゲット！ |
| `CheckpointReached` | チェックポイント！ |
| `CustomerWaiting` | ごちゅうもんの ケーキを まってるよ |

## 4. ゲームイベント（`GameEventType`）

シミュレーションが出し、表示側（`GameEventPresenter`）が音・パーティクル・画面揺れに変換します。座標はシミュレーション座標。

| イベント | 発生条件 | 表示 |
|---|---|---|
| `Jump` | ジャンプ | 音 |
| `JellyBounce` | ゼリーで跳ねた | 音 |
| `KnifeSwing` | 切る動作・ケーキが倒れ始めた | 音 |
| `CakeCut` | 切れた | 音、くず |
| `ChocolateClink` | チョコに阻まれた | 音、火花、揺れ 3 |
| `CakePushed` | 押せた | 音 |
| `ToppleLanded` | 倒れて着地 | 音、くず、揺れ 6 |
| `PieceLanded` | 落下して着地 | 音、くず |
| `CellEaten` | 食べた（`CellType` 付き） | 音（パワーアップ時以外）、セル色のくず |
| `PowerUp` | いちご／チョコを食べた | 音、火花 |
| `Refused` | 断りのセリフ | 音 |
| `AntSquashed` | アリを倒した | 音、火花 |
| `OrderPickedUp` | ごちゅうもんを拾った | 音、火花 |
| `CheckpointReached` | チェックポイント | 音 |
| `PlayerMissed` | ミス | 音 |
| `CellMelted` | アイスが溶けた | 音（0.12 秒に 1 回まで）、しずく |
| `OrderLost` | ごちゅうもんを落とした | 音、トースト |
| `OrderDelivered` | 届けた | 音、ハート |

イベントは 1 回の取り出しまでに最大 512 件までためます（取り出し忘れでもメモリが増え続けない）。

## 5. 定数（`SimConstants`）

単位はシミュレーション px・秒。試作品と完全に同じ値です（変更すると試作品との一致テストが失敗します）。

| 定数 | 値 | 意味 |
|---|---|---|
| `CellSize` | 40 | 1 マスの px |
| `FixedDeltaTime` | 1/60 | 1 ステップの秒数 |
| `PlayerWidth` × `PlayerHeight` | 26 × 34 | シェフの当たり判定 |
| `Gravity` / `MaxFallSpeed` | 2000 / 900 | 重力・最大落下速度 |
| `RunSpeed` / `PoweredRunSpeed` | 180 / 220 | 走る速さ（通常／いちご） |
| `GroundAcceleration` / `AirAcceleration` / `GroundFriction` | 2400 / 1600 / 2800 | 加減速 |
| `AirFrictionFactor` | 0.7 | 空中で手を離したときの減速倍率 |
| `JumpVelocity` / `PoweredJumpVelocity` | 600 / 760 | ジャンプ初速 |
| `JellyBounceVelocity` / `StompBounceVelocity` / `DeathHopVelocity` | 880 / 430 / 520 | 跳ね返り |
| `CoyoteTime` / `JumpBufferTime` | 0.07 / 0.12 | ジャンプの猶予 |
| `PushDelay` / `SlideDuration` / `ToppleDuration` | 0.12 / 0.14 / 0.3 | 押す・滑る・倒れる時間 |
| `PushWeightLimit` / `StrongPushWeightLimit` | 4 / 12 | 押せる重さ |
| `MeltDuration` / `PowerUpDuration` | 0.6 / 12 | 溶ける時間・パワーアップ時間 |
| `ActionReach` | 14 | 体の前の判定距離 |
| `CutCooldown` / `EatCooldown` | 0.28 / 0.3 | 連打防止 |
| `AntWidth` × `AntHeight` / `AntSpeed` | 30 × 16 / 42 | アリ |
| `RespawnDelayAfterMiss` / `RespawnDelayAfterOrderLost` | 1.2 / 1.6 | 再開までの時間 |
| `DeliveryCelebrationTime` / `RespawnInvulnerability` | 1.7 / 1.5 | 演出・無敵時間 |
| `MessageDuration` / `MessageCooldown` / `GoalHintCooldown` | 1.8 / 1.6 / 2.5 | セリフ |

評価：`Scoring.StarsFor(loss, par)` — `loss ≤ par` で 3、`≤ par + 3` で 2、それ以外 1。

## 6. セーブデータ

`PlayerPrefs` キー `pieceofcake.progress.v1` に JSON で保存（`PlayerPrefsProgressStore`）。

```json
{ "muted": false, "entries": [ { "id": "stage1", "stars": 3 } ] }
```

| 項目 | 型 | 検証 |
|---|---|---|
| `muted` | bool | — |
| `entries[].id` | string | 空・65 文字以上は無視 |
| `entries[].stars` | int | 0〜3 に丸める |
| 件数 | — | 最大 256 件まで読む |

壊れた JSON は警告を出して破棄します（ゲームは止まりません）。ベスト更新時のみ書き込みます。

## 7. 入力アクション（`Assets/PieceOfCake/Input/GameControls.inputactions`）

プロジェクト全体の入力アクション（Project-wide Actions）に設定済みです。

| マップ | アクション | 種類 |
|---|---|---|
| `Gameplay` | `Move` | Value / Axis（-1〜1、±0.5 で左右判定） |
| `Gameplay` | `Jump` `Cut` `Eat` `Retry` `Pause` | Button |
| `UI` | Navigate, Submit, Cancel, Point, Click など | UI Toolkit の操作用（Unity 標準） |

## 8. コンテンツのキー

| 種類 | キー | 定義場所 |
|---|---|---|
| テーマ | `shortcake` `tart` `jellychoco` `icecave` `wedding` | `GameArt.themes`（空の色、地形色、光） |
| お客さん | `bear` `rabbit` `cat` `penguin` `princess` | `GameArt.customers` |

未知のキーは先頭の要素で代用します（データ誤りでも止まらない）。
