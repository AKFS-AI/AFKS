# 🏗️ 하이어라키 구조 및 컴포넌트 참조 가이드

## 🎯 개요
이 문서는 AFKS 게임의 모든 씬에서 사용되는 하이어라키 구조와 각 GameObject의 컴포넌트 설정을 상세히 설명합니다.

---

## 📋 목차
1. [메인메뉴 씬 하이어라키](#1-메인메뉴-씬-하이어라키)
2. [게임플레이 씬 하이어라키](#2-게임플레이-씬-하이어라키)
3. [스테이지 오브젝트 구조](#3-스테이지-오브젝트-구조)
4. [컴포넌트 설정 완전 가이드](#4-컴포넌트-설정-완전-가이드)

---

## 1. 메인메뉴 씬 하이어라키

### 1.1 전체 구조도
```
MainMenu (Scene)
├── 📷 Main Camera
│   └── [Components: Camera, AudioListener]
├── 🖼️ MainMenuCanvas
│   ├── [Components: Canvas, CanvasScaler, GraphicRaycaster]
│   ├── 🖼️ BackgroundImage
│   │   └── [Components: RectTransform, Image]
│   ├── 📝 TitleText (TMP)
│   │   └── [Components: RectTransform, TextMeshProUGUI]
│   ├── 🔘 StartGameButton
│   │   ├── [Components: RectTransform, Image, Button, AudioSource]
│   │   └── 📝 Text (TMP)
│   │       └── [Components: RectTransform, TextMeshProUGUI]
│   ├── 🔘 SettingsButton
│   │   ├── [Components: RectTransform, Image, Button, AudioSource]
│   │   └── 📝 Text (TMP)
│   │       └── [Components: RectTransform, TextMeshProUGUI]
│   ├── 🔘 ExitButton
│   │   ├── [Components: RectTransform, Image, Button, AudioSource]
│   │   └── 📝 Text (TMP)
│   │       └── [Components: RectTransform, TextMeshProUGUI]
│   └── 📝 VersionText (TMP) [선택사항]
│       └── [Components: RectTransform, TextMeshProUGUI]
├── ⚙️ GameManager
│   └── [Components: Transform, GameManager]
├── 🎬 SceneController
│   └── [Components: Transform, SceneController]
├── 🔊 AudioManager
│   └── [Components: Transform, AudioManager, AudioSource]
├── 🖱️ UIManager
│   └── [Components: Transform, UIManager]
└── 🎮 EventSystem
    ├── [Components: Transform, EventSystem, StandaloneInputModule]
    └── (자동 생성됨)
```

### 1.2 메인메뉴 씬 상세 설정

#### 📷 Main Camera
```yaml
Transform:
  Position: (0, 0, -10)
  Rotation: (0, 0, 0)
  Scale: (1, 1, 1)

Camera:
  Clear Flags: Solid Color
  Background: #000000 (검은색)
  Culling Mask: Everything
  Projection: Orthographic
  Size: 5
  Clipping Planes:
    Near: 0.3
    Far: 1000
  Viewport Rect:
    X: 0, Y: 0
    W: 1, H: 1
  Depth: -1
  Rendering Path: Use Graphics Settings
  Target Texture: None
  Occlusion Culling: ✅ 체크
  HDR: ❌ 체크 해제
  MSAA: ❌ 체크 해제
  Dynamic Resolution: ❌ 체크 해제

AudioListener:
  (기본 설정)
```

#### 🖼️ MainMenuCanvas
```yaml
RectTransform:
  Anchor: Center
  Position: (0, 0, 0)
  Width: 100, Height: 100
  Anchors: (0, 0, 1, 1)
  Pivot: (0.5, 0.5)

Canvas:
  Render Mode: Screen Space - Overlay
  Pixel Perfect: ✅ 체크
  Sort Order: 0
  Target Display: Display 1
  Additional Shader Channels: None

CanvasScaler:
  UI Scale Mode: Scale With Screen Size
  Reference Resolution: 1920 x 1080
  Screen Match Mode: Match Width Or Height
  Match: 0.5
  Reference Pixels Per Unit: 100

GraphicRaycaster:
  Ignore Reversed Graphics: ✅ 체크
  Blocking Objects: None
  Blocking Mask: Everything
```

#### 🖼️ BackgroundImage
```yaml
RectTransform:
  Anchor: Stretch
  Position: (0, 0, 0)
  Left: 0, Top: 0, Right: 0, Bottom: 0
  Anchors: (0, 0, 1, 1)
  Pivot: (0.5, 0.5)

Image:
  Source Image: [메인메뉴 배경 이미지]
  Color: (1, 1, 1, 1) - 흰색
  Material: None
  Raycast Target: ❌ 체크 해제
  Maskable: ✅ 체크
  Preserve Aspect: ❌ 체크 해제
  Set Native Size: ❌ 체크 해제
  Type: Simple
  Use Sprite Mesh: ❌ 체크 해제
  Fill Center: ✅ 체크
```

#### 📝 TitleText (TMP)
```yaml
RectTransform:
  Anchor: Top Center
  Position: (0, 200, 0)
  Width: 800, Height: 150
  Anchors: (0.5, 1, 0.5, 1)
  Pivot: (0.5, 0.5)

TextMeshProUGUI:
  Text: "AFKS"
  Font Asset: [Default Font]
  Material: [Default Material]
  Font Size: 120
  Auto Size: ❌ 체크 해제
  Color: (1, 1, 1, 1) - 흰색
  Spacing:
    Character: 0
    Word: 0
    Line: 0
    Paragraph: 0
  Alignment: Center and Middle
  Wrapping: ❌ 체크 해제
  Overflow: Overflow
  Vertical Alignment: Middle
  Horizontal Alignment: Center
  Margin: (0, 0, 0, 0)
  Raycast Target: ❌ 체크 해제
```

#### 🔘 StartGameButton
```yaml
RectTransform:
  Anchor: Center
  Position: (0, 50, 0)
  Width: 300, Height: 80
  Anchors: (0.5, 0.5, 0.5, 0.5)
  Pivot: (0.5, 0.5)

Image:
  Source Image: [UI Sprite]
  Color: (1, 1, 1, 1)
  Material: None
  Raycast Target: ✅ 체크
  Type: Sliced
  Fill Center: ✅ 체크

Button:
  Interactable: ✅ 체크
  Transition: Color Tint
  Target Graphic: [자기 자신의 Image]
  Normal Color: (1, 1, 1, 1) - 흰색
  Highlighted Color: (0.96, 0.96, 0.96, 1)
  Pressed Color: (0.78, 0.78, 0.78, 1)
  Selected Color: (0.96, 0.96, 0.96, 1)
  Disabled Color: (0.78, 0.78, 0.78, 0.5)
  Color Multiplier: 1
  Fade Duration: 0.1
  Navigation: Automatic
  On Click (): SceneController.LoadGameScene()

AudioSource:
  AudioClip: [버튼 클릭 사운드]
  Output: None
  Mute: ❌ 체크 해제
  Bypass Effects: ❌ 체크 해제
  Bypass Listener Effects: ❌ 체크 해제
  Bypass Reverb Zones: ❌ 체크 해제
  Play On Awake: ❌ 체크 해제
  Loop: ❌ 체크 해제
  Priority: 128
  Volume: 0.5
  Pitch: 1
  Stereo Pan: 0
  Spatial Blend: 0 (2D)
  Reverb Zone Mix: 1
```

---

## 2. 게임플레이 씬 하이어라키

### 2.1 전체 구조도
```
GameplayScene (Scene)
├── 📷 Main Camera
│   └── [Components: Camera, AudioListener]
├── 🖼️ GameplayCanvas
│   ├── [Components: Canvas, CanvasScaler, GraphicRaycaster]
│   ├── 🖼️ StageCanvas
│   │   ├── [Components: RectTransform, CanvasGroup]
│   │   └── 🏥 Stages
│   │       ├── [Components: RectTransform]
│   │       ├── 🏗️ Stage0_HospitalExterior
│   │       │   ├── [Components: RectTransform]
│   │       │   ├── 🖼️ Background
│   │       │   ├── 🚪 HospitalDoorImage
│   │       │   │   ├── [Components: RectTransform, Image, TwoStageInteractionController]
│   │       │   │   └── ⛓️ ChainImage
│   │       │   │       └── [Components: RectTransform, Image]
│   │       │   └── ... (기타 상호작용 오브젝트들)
│   │       └── 🏗️ Stage1_HospitalLobby
│   │           ├── [Components: RectTransform]
│   │           ├── 🖼️ LobbyBackground
│   │           ├── 💡 LightingOverlay
│   │           ├── 🗃️ ReceptionDesk
│   │           ├── 🏥 MedicalCabinet
│   │           ├── 🛗 ElevatorDoor
│   │           └── 📺 CCTVMonitor
│   ├── 🔍 ZoomController
│   │   ├── [Components: RectTransform, ZoomController]
│   │   └── 🖼️ ZoomBackgroundOverlay
│   │       └── [Components: RectTransform, Image, Button]
│   └── 🎮 HUD
│       ├── [Components: RectTransform]
│       ├── 🎒 InventoryPanel
│       │   ├── [Components: RectTransform, Image]
│       │   ├── 🔑 KeySlot
│       │   │   └── [Components: RectTransform, Image, Button]
│       │   └── 🔦 FlashlightSlot
│       │       └── [Components: RectTransform, Image, Button]
│       ├── 🧭 StageNavigation
│       │   └── [Components: RectTransform, StageNavigationUI]
│       ├── 💬 MessagePanel
│       │   ├── [Components: RectTransform, Image]
│       │   └── 📝 MessageText
│       │       └── [Components: RectTransform, TextMeshProUGUI]
│       └── 🐛 DebugPanel [개발용]
│           └── [Components: RectTransform, Image]
├── ⚙️ CoreManagers
│   ├── [Components: Transform]
│   ├── 🎮 GameManager
│   │   └── [Components: Transform, GameManager]
│   ├── 🎬 SceneController
│   │   └── [Components: Transform, SceneController]
│   ├── 🏗️ StageManager
│   │   └── [Components: Transform, StageManager]
│   ├── 🔊 AudioManager
│   │   └── [Components: Transform, AudioManager, AudioSource]
│   ├── 🖱️ UIManager
│   │   └── [Components: Transform, UIManager]
│   ├── 🔗 InteractionManager
│   │   └── [Components: Transform, InteractionManager]
│   ├── 👻 HorrorEventManager
│   │   └── [Components: Transform, HorrorEventManager]
│   └── 🎒 ItemManager
│       └── [Components: Transform, ItemManager]
└── 🎮 EventSystem
    ├── [Components: Transform, EventSystem, StandaloneInputModule]
    └── (자동 생성됨)
```

### 2.2 게임플레이 씬 상세 설정

#### 🖼️ StageCanvas (CanvasGroup)
```yaml
RectTransform:
  Anchor: Stretch
  Position: (0, 0, 0)
  Left: 0, Top: 0, Right: 0, Bottom: 0
  Anchors: (0, 0, 1, 1)
  Pivot: (0.5, 0.5)

CanvasGroup:
  Alpha: 1
  Interactable: ✅ 체크
  Blocks Raycasts: ✅ 체크
  Ignore Parent Groups: ❌ 체크 해제
```

#### 🚪 HospitalDoorImage (TwoStageInteractionController)
```yaml
RectTransform:
  Anchor: Center
  Position: (0, 0, 0)
  Width: 800, Height: 600
  Anchors: (0.5, 0.5, 0.5, 0.5)
  Pivot: (0.5, 0.5)

Image:
  Source Image: [병원 철문 이미지]
  Color: (1, 1, 1, 1)
  Material: None
  Raycast Target: ✅ 체크
  Preserve Aspect: ✅ 체크
  Type: Simple

TwoStageInteractionController:
  # === 1단계: 철문 설정 ===
  Door Image: [자기 자신]
  Door Interaction Id: "hospital_door"
  Door Display Name: "병원 철문"
  
  # === 확대 설정 ===
  Zoom Scale: 2.5
  Zoom Focus Offset: (0, -50)
  
  # === 2단계: 쇠사슬 설정 ===
  Chain Image: [ChainImage GameObject]
  Chain Interaction Id: "hospital_door_chain"
  Chain Display Name: "철문의 쇠사슬"
  Required Chain Clicks: 5
  Next Stage Index: 1
  
  # === 시각적 피드백 ===
  Hover Color: (1, 1, 0, 0.3)
  Hover Scale: 1.05
  Click Feedback Duration: 0.2
  
  # === 오디오 ===
  Door Click Sound: [문 클릭 사운드]
  Chain Click Sound: [쇠사슬 클릭 사운드]
  Chain Break Sound: [쇠사슬 끊어지는 사운드]
  
  # === 메시지 ===
  Door Click Message: "문을 자세히 살펴보자..."
  Chain Progress Message: "쇠사슬을 부수고 있다... ({0}/{1})"
  Door Unlock Message: "쇠사슬이 끊어졌다! 문이 열렸다!"
  
  # === 디버그 ===
  Debug Mode: ❌ 체크 해제
```

#### ⛓️ ChainImage
```yaml
RectTransform:
  Anchor: Center
  Position: (0, -100, 0)
  Width: 300, Height: 200
  Anchors: (0.5, 0.5, 0.5, 0.5)
  Pivot: (0.5, 0.5)

Image:
  Source Image: [쇠사슬 이미지]
  Color: (1, 1, 1, 1)
  Material: None
  Raycast Target: ✅ 체크
  Preserve Aspect: ✅ 체크
  Type: Simple

GameObject:
  Active: ❌ 비활성화 (초기 상태)
```

#### 🔍 ZoomController
```yaml
RectTransform:
  Anchor: Center
  Position: (0, 0, 0)
  Width: 100, Height: 100
  Anchors: (0.5, 0.5, 0.5, 0.5)
  Pivot: (0.5, 0.5)

ZoomController:
  # === 줌 설정 ===
  Max Zoom Scale: 3.0
  Zoom Duration: 0.8
  Target Image: [런타임에 설정됨]
  Background Overlay: [ZoomBackgroundOverlay]
  
  # === 시각적 설정 ===
  Overlay Color: (0, 0, 0, 0.7)
  Zoom Curve: [EaseInOut AnimationCurve]
  
  # === 오디오 ===
  Zoom In Sound: [줌 인 효과음]
  Zoom Out Sound: [줌 아웃 효과음]
```

#### 🖼️ ZoomBackgroundOverlay
```yaml
RectTransform:
  Anchor: Stretch
  Position: (0, 0, 0)
  Left: 0, Top: 0, Right: 0, Bottom: 0
  Anchors: (0, 0, 1, 1)
  Pivot: (0.5, 0.5)

Image:
  Source Image: None
  Color: (0, 0, 0, 0.7)
  Material: None
  Raycast Target: ✅ 체크
  Type: Simple

Button:
  Interactable: ✅ 체크
  Transition: None
  Navigation: None
  On Click (): ZoomController.ZoomOut()

GameObject:
  Active: ❌ 비활성화 (초기 상태)
```

---

## 3. 스테이지 오브젝트 구조

### 3.1 스테이지 1 (병원 로비) 상세 구조
```
Stage1_HospitalLobby
├── [Components: RectTransform]
├── 🖼️ LobbyBackground
│   └── [Components: RectTransform, Image]
├── 💡 LightingOverlay
│   └── [Components: RectTransform, Image, Animator]
├── 🌑 DarkAreas
│   └── [Components: RectTransform, DarkAreaController]
├── 🗃️ ReceptionDesk
│   └── [Components: RectTransform, Image, StageInteractionController]
├── 🏥 MedicalCabinet
│   └── [Components: RectTransform, Image, StageInteractionController]
├── 🛗 ElevatorDoor
│   └── [Components: RectTransform, Image, StageInteractionController]
└── 📺 CCTVMonitor
    └── [Components: RectTransform, Image, StageInteractionController]
```

### 3.2 스테이지 상호작용 컴포넌트 설정

#### 🗃️ ReceptionDesk (StageInteractionController)
```yaml
RectTransform:
  Anchor: Center
  Position: (-200, -100, 0)
  Width: 300, Height: 200
  Anchors: (0.5, 0.5, 0.5, 0.5)
  Pivot: (0.5, 0.5)

Image:
  Source Image: None (투명 상호작용 영역)
  Color: (1, 1, 1, 0) - 완전 투명
  Material: None
  Raycast Target: ✅ 체크
  Type: Simple

StageInteractionController:
  # === 상호작용 데이터 ===
  Interaction Data: [ReceptionDesk_InteractionPoint ScriptableObject]
  
  # 또는 직접 설정:
  Id: "reception_desk"
  Display Name: "리셉션 데스크"
  Description: "오래된 리셉션 데스크입니다. 무언가 있을지도 모릅니다."
  Position: (-200, -100)
  Size: (300, 200)
  Interaction Type: Click
  Is Enabled: ✅ 체크
  Is Visible: ✅ 체크
  
  # === 시각적 피드백 ===
  Hover Color: (1, 1, 0, 0.5)
  Hover Scale: 1.1
  Click Feedback Duration: 0.2
  
  # === 오디오 ===
  Interaction Sound: [서랍 열리는 소리]
  Hover Sound: [호버 효과음 - 선택사항]
  
  # === 조건 ===
  Required Items: [] (없음)
  
  # === 결과 ===
  Result Type: 1 (아이템 지급)
  Message: "열쇠를 발견했습니다!"
  Give Item: ✅ 체크
  Item Id: "key"
  Sound Effect: [아이템 획득 효과음]
```

#### 🛗 ElevatorDoor (조건부 상호작용)
```yaml
StageInteractionController:
  # === 상호작용 데이터 ===
  Id: "elevator_door"
  Display Name: "엘리베이터"
  Description: "상층으로 올라가는 엘리베이터입니다."
  Position: (0, 200)
  Size: (150, 250)
  Interaction Type: Click
  Is Enabled: ✅ 체크
  Is Visible: ✅ 체크
  
  # === 조건 (중요!) ===
  Required Items: ["key"] (열쇠 필요)
  
  # === 결과 ===
  Result Type: 2 (스테이지 변경)
  Change Stage: ✅ 체크
  Target Stage Index: 2
  Message: "엘리베이터가 작동합니다..."
```

---

## 4. 컴포넌트 설정 완전 가이드

### 4.1 매니저 컴포넌트 설정

#### 🎮 GameManager
```yaml
🎮 게임 설정:
  Game Config: [GameConfig ScriptableObject 할당]

📊 현재 게임 상태:
  Current State: MainMenu (MainMenu, Playing, Paused 등)
  Current Stage Index: 0 (0~10 범위)
  Game Time: 0.0 (게임 진행 시간)
```

#### 🏗️ StageManager
```yaml
🎭 스테이지 설정:
  Stages: [Stage0_HospitalExterior, Stage1_HospitalLobby, ...] 
  Current Stage Index: 0 (0~10 범위)

🖼️ UI 참조:
  Background Image: [UI/BackgroundImage 할당]
  Stage Canvas: [GameplayCanvas 할당]
  Transition Canvas: [UI/TransitionCanvas 할당]

🌊 전환 설정:
  Transition Duration: 1.0 (0.1~5.0초 범위)
  Enable Preloading: ✅ 체크
  Max Preload Stages: 2 (1~5개 범위)

🔒 고급 스테이지 시스템:
  Unlocked Stages: [시스템이 자동 관리]
  Enable Pixel Perfect Interaction: ✅ 체크
  Auto Save Interval: 30.0 (초)
```

#### 🔊 AudioManager
```yaml
🔊 오디오 소스:
  BGM Source: [시스템이 자동 생성]
  SFX Sources: [시스템이 자동 생성 배열]
  Ambient Source: [시스템이 자동 생성]

⚙️ 설정:
  Max SFX Sources: 8 (1~16개 범위)
  Crossfade Duration: 1.0 (0.1~5.0초 범위)

🎵 볼륨 조절:
  Master Volume: 1.0 (0.0~1.0 범위)
  BGM Volume: 0.7 (0.0~1.0 범위)
  SFX Volume: 0.8 (0.0~1.0 범위)
  Ambient Volume: 0.5 (0.0~1.0 범위)
```

#### 🖱️ UIManager
```yaml
🖼️ 메인 UI 패널:
  Main Canvas: [MainCanvas 할당]
  Gameplay Panel: [UI/GameplayPanel 할당]
  Inventory Panel: [UI/InventoryPanel 할당]
  Menu Panel: [UI/MenuPanel 할당]
  Settings Panel: [UI/SettingsPanel 할당]
  Loading Panel: [UI/LoadingPanel 할당]

📱 오버레이 패널:
  Fade Overlay: [UI/FadeOverlay 할당]
  Dialog Panel: [UI/DialogPanel 할당]
  Notification Panel: [UI/NotificationPanel 할당]

🎒 인벤토리 UI:
  Inventory Container: [UI/InventoryPanel/Content 할당]
  Inventory Slot Prefab: [Prefabs/UI/InventorySlot 할당]
  Item Name Text: [UI/ItemDetail/ItemName 할당]
  Item Description Text: [UI/ItemDetail/Description 할당]
  Item Detail Image: [UI/ItemDetail/Image 할당]

⚙️ 설정:
  Panel Transition Duration: 0.3 (0.1~2.0초 범위)
  Fade Transition Duration: 0.5 (0.1~2.0초 범위)
  Enable UI Animations: ✅ 체크
```

#### 🔗 InteractionManager
```yaml
⚙️ 상호작용 설정:
  Enable Global Interactions: ✅ 체크
  Interaction Cooldown: 0.2 (0.1~2.0초 범위)
  Interaction Layer: Everything (-1)

🐛 디버그:
  Show Debug Info: ❌ 체크 해제 (프로덕션)
```

#### 👻 HorrorEventManager
```yaml
😱 공포 설정:
  Enable Horror Events: ✅ 체크
  Global Cooldown: 5.0 (1.0~30.0초 범위)
  Max Concurrent Events: 1 (1~5개 범위)

🎬 컴포넌트:
  Horror Canvas: [공포 효과 전용 Canvas 할당 - 선택사항]
  Horror Camera: [공포 효과 전용 Camera 할당 - 선택사항]
```

#### 🎒 ItemManager
```yaml
🔑 아이템 상태:
  Has Key: ❌ (런타임에 변경됨)
  Has Flashlight: ❌ (런타임에 변경됨)

🎵 오디오:
  Key Pickup Sound: [열쇠 획득 사운드 - 선택사항]
  Flashlight Pickup Sound: [손전등 획득 사운드 - 선택사항]
  Use Sound: [아이템 사용 사운드 - 선택사항]

⚙️ 설정:
  Auto Save: ✅ 체크
```

### 4.2 UI 컴포넌트 설정

#### 🧭 StageNavigationUI
```yaml
StageNavigationUI:
  # === 네비게이션 설정 ===
  Stage Manager: [StageManager GameObject]
  Panel Visibility: ❌ (초기 비활성화)
  Quick Nav Key: Tab
  
  # === UI 요소 ===
  Stage Button Prefab: [StageButton_Template Prefab]
  Button Container: [자동 생성됨]
  
  # === 스타일 ===
  Button Spacing: 10
  Buttons Per Row: 5
  
  # === 애니메이션 ===
  Fade Duration: 0.3
  Slide Animation: ✅ 체크
  
  # === 디버그 ===
  Show Debug Info: ❌ 체크 해제
```

### 4.3 특수 컴포넌트 설정

#### 💡 LightingOverlay (Animator)
```yaml
Animator:
  Controller: [LobbyLightFlicker AnimatorController]
  Avatar: None
  Apply Root Motion: ❌ 체크 해제
  Update Mode: Normal
  Culling Mode: Always Animate

AnimatorController (LobbyLightFlicker):
  Parameters: [없음]
  Layers:
    - Default Layer
      States:
        - LightFlicker (기본 상태)
          Motion: LobbyLightFlicker Animation
          Speed: 1.0
          Loop: ✅ 체크

Animation (LobbyLightFlicker):
  Length: 2.0초
  Sample Rate: 60
  Keyframes:
    - 0초: Alpha = 0.05
    - 1초: Alpha = 0.15
    - 2초: Alpha = 0.05
  Interpolation: Linear
```

### 4.4 ScriptableObject 데이터 설정

#### 📄 Stage0_HospitalExterior (StageData)
```yaml
StageData:
  # === 기본 정보 ===
  Stage Index: 0
  Stage Name: "병원 외부"
  Stage Description: "버려진 병원의 입구입니다."
  
  # === 시각적 설정 ===
  Background Image: [병원 외부 배경 이미지]
  Ambient Color: (0.8, 0.8, 1, 1)
  Enable Dark Mode: ✅ 체크
  
  # === 오디오 ===
  Background Music: [병원 외부 BGM]
  Ambient Sound: [바람소리, 먼 거리 소음]
  Music Volume: 0.6
  Ambient Volume: 0.4
  
  # === 상호작용 포인트 ===
  Interaction Points: [HospitalDoor InteractionPoint]
  
  # === 진행 조건 ===
  Completion Condition: CompleteAllInteractions
  Required Items: [] (없음)
  Next Stage Index: 1
```

#### 📄 TwoStageInteractionData
```yaml
TwoStageInteractionData:
  # === 1단계 설정 (철문) ===
  Stage1 Id: "hospital_door"
  Zoom Scale: 2.5
  Stage1 Sound: [문 클릭 사운드]
  
  # === 2단계 설정 (쇠사슬) ===
  Stage2 Id: "chains"
  Required Clicks: 5
  Stage2 Click Sound: [쇠사슬 클릭 사운드]
  Stage2 Complete Sound: [쇠사슬 끊어지는 사운드]
  Progress Message: "쇠사슬을 부수고 있다... ({0}/{1})"
  Complete Message: "쇠사슬이 부서졌다! 문이 열린다."
  
  # === 스테이지 전환 ===
  Next Stage Index: 1
```

---

## 📚 5. 추가 참고사항

### 5.1 네이밍 컨벤션
- **GameObject**: PascalCase (예: HospitalDoorImage)
- **Component Variables**: camelCase (예: doorImage)
- **IDs**: snake_case (예: hospital_door)
- **Messages**: 한국어 (예: "쇠사슬을 부수고 있다...")

### 5.2 레이어 및 태그 설정
```yaml
Layers:
  - Default (0)
  - TransparentFX (1)
  - Ignore Raycast (2)
  - Water (4)
  - UI (5) ← UI 요소들 사용

Tags:
  - Untagged (기본)
  - Respawn
  - Finish
  - EditorOnly
  - MainCamera
  - Player
  - GameController
```

### 5.3 Build Settings 확인
```yaml
Scenes In Build:
  0: Assets/Scenes/MainMenu.unity
  1: Assets/Scenes/GameplayScene.unity
  2: Assets/Scenes/Stage2.unity (향후 추가)
  3: Assets/Scenes/Stage3.unity (향후 추가)

Platform Settings:
  Target Platform: PC, Mac & Linux Standalone
  Architecture: x86_64
  Scripting Backend: Mono
  Api Compatibility Level: .NET Framework
```

---

이 문서를 참고하여 정확한 하이어라키 구조와 모든 컴포넌트 설정을 확인하실 수 있습니다. 각 설정값은 최적화되어 있으며, 프로덕션 환경에서 바로 사용 가능합니다! 🎮✨