# 04. UML 図

GitHub 上では Mermaid 図がそのまま描画されます。

## 1. コンポーネント図

```mermaid
flowchart TB
    subgraph Unity["Unity シーン (Main.unity)"]
        Bootstrap["GameBootstrap<br/>(MonoBehaviour)"]
        Doc["UIDocument<br/>Main.uxml / Main.uss"]
        Cam["Main Camera"]
        Sun["Sun (Directional Light)"]
    end
    subgraph GameAsm["PieceOfCake.Game"]
        Flow["GameFlow"]
        Session["StageSession"]
        View["StageView"]
        Presenter["GameEventPresenter"]
        UI["UiRoot"]
        Input["PlayerControls"]
        Sfx["SfxPlayer"]
        Store["PlayerPrefsProgressStore"]
        Catalog["LevelCatalog"]
    end
    subgraph CoreAsm["PieceOfCake.Core"]
        Sim["StageSimulation"]
        Parser["LevelParser"]
    end
    Data[("levels.json")]
    Art[("GameArt.asset")]
    Prefs[("PlayerPrefs")]

    Bootstrap --> Flow
    Bootstrap --> UI
    Bootstrap --> Input
    Bootstrap --> Sfx
    Bootstrap --> Store
    Bootstrap --> Catalog
    Catalog --> Data
    Catalog --> Parser
    Flow --> Session
    Flow --> UI
    Session --> Sim
    Session --> View
    Session --> Presenter
    View --> Art
    View --> Cam
    View --> Sun
    Presenter --> Sfx
    UI --> Doc
    Store --> Prefs
```

## 2. クラス図：Core（ルール）

```mermaid
classDiagram
    direction LR
    class StageSimulation {
        +LevelLayout Layout
        +IReadOnlyList~Piece~ Pieces
        +IReadOnlyList~Ant~ Ants
        +IReadOnlyList~Checkpoint~ Checkpoints
        +Player Player
        +RunStats Stats
        +GamePhase Phase
        +double StageTime
        +int Misses
        +StageResult Result
        +Step(InputFrame)
        +RetryFromCheckpoint() bool
        +DrainEvents(List~GameEvent~)
    }
    class StageState {
        +List~Piece~ Pieces
        +List~Ant~ Ants
        +Player Player
        +OccupancyGrid Occupancy
        +EventBuffer Events
        +ReplacePiece(Piece, List~Piece~)
    }
    class PieceSystem
    class PlayerSystem
    class PlayerActions {
        +Cut()
        +Eat()
    }
    class CakeOperations {
        +TryTopple(Piece, int) bool
        +TryPush(Piece, int)
        +TrySplit(Piece, int, int, bool) bool
    }
    class AntSystem
    class CrushSystem
    class ProgressSystem
    class CheckpointService {
        +Capture()
        +Restore()
    }
    class WorldQueries {
        +IsBlocked(int, int, ICollection) bool
        +IsSupported(Piece) bool
        +CollectBoxesAround(Box, List)
        +CollectRiders(Piece) List
    }
    class GameplayOutcomes {
        +Say(PlayerMessage)
        +Refuse(PlayerMessage)
        +KillPlayer(DeathCause)
        +SquashAnt(Ant)
    }
    class PieceTopology {
        <<static>>
        +Components(StageState, Piece, Cell[,]) List~Piece~
    }
    class LevelParser {
        <<static>>
        +Parse(LevelDefinition) LevelLayout
    }
    class LevelLayout {
        +int Width
        +int Height
        +GridPoint Start
        +GridPoint Goal
        +TileAt(int, int) TileType
    }
    class Piece {
        +int GridX
        +int GridY
        +int Width
        +int Height
        +PieceState State
        +PieceAnimation Animation
        +GetCell(int, int) Cell
    }
    class Cell {
        +CellType Type
        +double MeltTimer
    }
    class PieceAnimation {
        <<abstract>>
        +double Elapsed
        +double Duration
    }
    class SlideAnimation
    class ToppleAnimation
    class Player
    class Ant

    StageSimulation *-- StageState
    StageSimulation *-- PieceSystem
    StageSimulation *-- PlayerSystem
    StageSimulation *-- AntSystem
    StageSimulation *-- CrushSystem
    StageSimulation *-- ProgressSystem
    StageSimulation *-- CheckpointService
    PlayerSystem --> PlayerActions
    PlayerSystem --> CakeOperations
    PlayerActions --> CakeOperations
    CakeOperations --> WorldQueries
    CakeOperations --> PieceTopology
    PieceSystem --> WorldQueries
    AntSystem --> WorldQueries
    PlayerSystem --> GameplayOutcomes
    AntSystem --> GameplayOutcomes
    CrushSystem --> GameplayOutcomes
    ProgressSystem --> CheckpointService
    StageState o-- Piece
    StageState o-- Ant
    StageState o-- Player
    StageState --> LevelLayout
    Piece o-- Cell
    Piece --> PieceAnimation
    PieceAnimation <|-- SlideAnimation
    PieceAnimation <|-- ToppleAnimation
    LevelParser ..> LevelLayout : creates
```

## 3. クラス図：Game（表示と入出力）

```mermaid
classDiagram
    direction LR
    class GameBootstrap {
        <<MonoBehaviour>>
        -TextAsset levelFile
        -GameArt art
        -UIDocument uiDocument
        -Camera worldCamera
        -Light sun
    }
    class GameFlow {
        +FlowState State
        +Update(float)
        +Render(float)
    }
    class StageSession {
        +StageSimulation Simulation
        +StageView View
        +Tick(float)
        +Render(float)
    }
    class StageView {
        +WorldMapper Map
        +Render(StageSimulation, float, float)
    }
    class CakeRenderer
    class InstanceBatch
    class ChefView
    class AntViews
    class PropViews
    class BackgroundView
    class CameraRig
    class GameEventPresenter
    class EffectsPlayer
    class ISfxPlayer {
        <<interface>>
        +bool Muted
        +Play(SoundId)
    }
    class SfxPlayer
    class IProgressStore {
        <<interface>>
        +BestStars(string) int
        +RecordStars(string, int) bool
        +bool Muted
    }
    class PlayerPrefsProgressStore
    class IControlSource {
        <<interface>>
        +Sample() ControlSample
    }
    class InputSystemControls
    class TouchControls
    class PlayerControls {
        +Poll()
        +ConsumeFrame() InputFrame
    }
    class UiRoot
    class GameArt {
        <<ScriptableObject>>
    }

    GameBootstrap --> GameFlow
    GameFlow --> StageSession
    GameFlow --> UiRoot
    GameFlow --> ISfxPlayer
    GameFlow --> IProgressStore
    StageSession --> StageView
    StageSession --> GameEventPresenter
    StageSession --> PlayerControls
    StageView *-- CakeRenderer
    StageView *-- ChefView
    StageView *-- AntViews
    StageView *-- PropViews
    StageView *-- BackgroundView
    CakeRenderer *-- InstanceBatch
    GameEventPresenter --> EffectsPlayer
    GameEventPresenter --> ISfxPlayer
    GameEventPresenter --> CameraRig
    ISfxPlayer <|.. SfxPlayer
    IProgressStore <|.. PlayerPrefsProgressStore
    IControlSource <|.. InputSystemControls
    IControlSource <|.. TouchControls
    PlayerControls o-- IControlSource
    StageView --> GameArt
```

## 4. シーケンス図：1 フレームの更新

```mermaid
sequenceDiagram
    participant U as Unity
    participant B as GameBootstrap
    participant C as PlayerControls
    participant F as GameFlow
    participant S as StageSession
    participant Sim as StageSimulation
    participant P as GameEventPresenter
    participant V as StageView

    U->>B: Update()
    B->>C: Poll()
    B->>F: Update(dt)
    F->>S: Tick(dt)
    loop 蓄積時間 ≥ 1/60 秒（最大 6 回）
        S->>C: ConsumeFrame()
        C-->>S: InputFrame
        S->>Sim: Step(frame)
    end
    S->>Sim: DrainEvents(list)
    S->>P: Present(events)
    U->>B: LateUpdate()
    B->>F: Render(dt)
    F->>S: Render(dt)
    S->>V: Render(sim, time, dt)
    S->>S: CameraRig.Follow(...)
```

## 5. シーケンス図：ケーキの上で「切る」

```mermaid
sequenceDiagram
    participant PS as PlayerSystem
    participant PA as PlayerActions
    participant CO as CakeOperations
    participant PT as PieceTopology
    participant St as StageState

    PS->>PA: Cut()（CutPressed かつ クールダウン終了）
    PA->>St: Occupancy.IdleAt(前の列, 体の行)
    alt 前にケーキがない、かつ接地
        PA->>St: Occupancy.IdleAt(足もとの列, 足の下の行)
        PA->>CO: TrySplit(piece, 足の先の線, 向き, flop=true)
        alt 切れ目にチョコ
            CO-->>PA: true（カチン！、ChocolateClink）
        else 切れる
            CO->>PT: Components(左側) / Components(右側)
            CO->>St: ReplacePiece(元, 分割後)
            CO->>CO: 向こう側の細い列を TryTopple
            CO-->>PA: true（CakeCut）
        end
    end
```

## 6. 状態遷移図

### GameFlow（画面）

```mermaid
stateDiagram-v2
    [*] --> Title
    Title --> StageSelect : PlayPressed
    StageSelect --> Title : BackPressed
    StageSelect --> Playing : StageChosen
    Playing --> Paused : Pause
    Paused --> Playing : Resume / Checkpoint
    Paused --> Playing : Restart
    Paused --> StageSelect
    Playing --> Result : Phase == Completed
    Result --> Playing : Next / Again
    Result --> StageSelect
```

### GamePhase（シミュレーション）

```mermaid
stateDiagram-v2
    [*] --> Playing
    Playing --> Delivering : ごちゅうもんを持ってゴールに触れる
    Delivering --> Completed : 1.7 秒後
    Completed --> [*]
```

### プレイヤーの生死とチェックポイント

```mermaid
stateDiagram-v2
    [*] --> Alive
    Alive --> Dead : トゲ / 落下 / アリ / 落ちてきたケーキ
    Dead --> Alive : 1.2 秒後に Restore（無敵 1.5 秒）
    Alive --> Alive : ごちゅうもん落下 → 1.6 秒後に Restore
    Alive --> Alive : Retry（R キー）→ Restore
```

### ピース

```mermaid
stateDiagram-v2
    [*] --> Resting
    Resting --> Sliding : 押された
    Sliding --> Resting : 0.14 秒後
    Resting --> Toppling : 押された / 切られた（幅 1・高さ 2 以上）
    Toppling --> Resting : 0.3 秒後（ToppleLanded）
    Resting --> Falling : 支えがない
    Falling --> Resting : 着地（PieceLanded）
    Falling --> [*] : マップ外（ロス加算 / ごちゅうもんなら OrderLost）
```
