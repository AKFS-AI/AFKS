# 🎮 게임플레이 씬 설정 가이드

## 🎯 개요
이 가이드는 AFKS 게임의 메인 게임플레이 씬을 처음부터 설정하는 방법을 단계별로 설명합니다.

---

## 📁 1. 씬 생성 및 기본 설정

### 1.1 새 씬 생성
1. **File → New Scene** 선택
2. **2D Template** 선택
3. **File → Save As** → `Assets/Scenes/GameplayScene.unity`로 저장

### 1.2 씬 설정 확인
- **Camera** 설정:
  - Position: (0, 0, -10)
  - Projection: Orthographic
  - Size: 5
  - Background Color: #1a1a1a (어두운 회색)

---

## 🏗️ 1.3 최종 하이어라키 구조도

완성된 게임플레이 씬의 하이어라키 구조는 다음과 같습니다:

```
GameplayScene (Scene)
├── 📷 Main Camera
│   └── [Camera, AudioListener]
├── 🖼️ GameplayCanvas
│   ├── [Canvas, CanvasScaler, GraphicRaycaster]
│   ├── 🖼️ StageCanvas
│   │   ├── [RectTransform, CanvasGroup]
│   │   └── 🏥 Stages
│   │       ├── [RectTransform]
│   │       ├── 🏗️ Stage0_HospitalExterior
│   │       │   ├── [RectTransform]
│   │       │   ├── 🖼️ Background
│   │       │   ├── 🚪 HospitalDoorImage
│   │       │   │   ├── [RectTransform, Image, TwoStageInteractionController]
│   │       │   │   └── ⛓️ ChainImage
│   │       │   │       └── [RectTransform, Image]
│   │       │   └── ... (기타 상호작용 오브젝트들)
│   │       └── 🏗️ Stage1_HospitalLobby
│   │           ├── [RectTransform]
│   │           ├── 🖼️ LobbyBackground
│   │           ├── 💡 LightingOverlay
│   │           ├── 🗃️ ReceptionDesk
│   │           ├── 🏥 MedicalCabinet
│   │           ├── 🛗 ElevatorDoor
│   │           └── 📺 CCTVMonitor
│   ├── 🔍 ZoomController
│   │   ├── [RectTransform, ZoomController]
│   │   └── 🖼️ ZoomBackgroundOverlay
│   │       └── [RectTransform, Image, Button]
│   └── 🎮 HUD
│       ├── [RectTransform]
│       ├── 🎒 InventoryPanel
│       │   ├── [RectTransform, Image]
│       │   ├── 🔑 KeySlot
│       │   │   └── [RectTransform, Image, Button]
│       │   └── 🔦 FlashlightSlot
│       │       └── [RectTransform, Image, Button]
│       ├── 🧭 StageNavigation
│       │   └── [RectTransform, StageNavigationUI]
│       ├── 💬 MessagePanel
│       │   ├── [RectTransform, Image]
│       │   └── 📝 MessageText
│       │       └── [RectTransform, TextMeshProUGUI]
│       └── 🐛 DebugPanel [개발용]
│           └── [RectTransform, Image]
├── ⚙️ CoreManagers
│   ├── [Transform]
│   ├── 🎮 GameManager
│   │   └── [Transform, GameManager]
│   ├── 🎬 SceneController
│   │   └── [Transform, SceneController]
│   ├── 🏗️ StageManager
│   │   └── [Transform, StageManager]
│   ├── 🔊 AudioManager
│   │   └── [Transform, AudioManager, AudioSource]
│   ├── 🖱️ UIManager
│   │   └── [Transform, UIManager]
│   ├── 🔗 InteractionManager
│   │   └── [Transform, InteractionManager]
│   ├── 👻 HorrorEventManager
│   │   └── [Transform, HorrorEventManager]
│   └── 🎒 ItemManager
│       └── [Transform, ItemManager]
└── 🎮 EventSystem
    └── [Transform, EventSystem, StandaloneInputModule]
```

**범례**: [ ] 안은 해당 GameObject에 필요한 컴포넌트들입니다.

---

## 🖼️ 2. UI Canvas 설정

### 2.1 Main Canvas 생성
1. **Hierarchy → 우클릭 → UI → Canvas**
2. Canvas 이름을 `GameplayCanvas`로 변경

#### 🔧 GameplayCanvas 컴포넌트 상세 설정
```yaml
Canvas:
  Render Mode: Screen Space - Overlay
  Pixel Perfect: ✅ 체크
  Sort Order: 0
  Target Display: Display 1
  Additional Shader Channels: None

GraphicRaycaster:
  Ignore Reversed Graphics: ✅ 체크
  Blocking Objects: None
  Blocking Mask: Everything
```

### 2.2 Canvas Scaler 설정
GameplayCanvas 선택 → Inspector에서 **Canvas Scaler** 컴포넌트:

#### 🔧 CanvasScaler 컴포넌트 상세 설정
```yaml
CanvasScaler:
  UI Scale Mode: Scale With Screen Size
  Reference Resolution: 1920 x 1080
  Screen Match Mode: Match Width Or Height
  Match: 0.5
  Reference Pixels Per Unit: 100
```

### 2.3 Stage Canvas 생성 (스테이지 전용)
1. **GameplayCanvas → 우클릭 → Create Empty**
2. 이름을 `StageCanvas`로 변경
3. **Add Component → Canvas Group**

#### 🔧 StageCanvas 컴포넌트 상세 설정
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

---

## ⚙️ 3. 필수 매니저 시스템 설정

### 3.1 Core Managers 그룹 생성
1. **Hierarchy → 우클릭 → Create Empty**
2. 이름을 `CoreManagers`로 변경
3. **Position**: (0, 0, 0)

### 3.2 GameManager 설정
1. **CoreManagers → 우클릭 → Create Empty**
2. 이름을 `GameManager`로 변경
3. **Add Component → Scripts → Core → GameManager**

#### 🔧 GameManager 컴포넌트 상세 설정
```yaml
🎮 게임 설정:
  Game Config: [GameConfig ScriptableObject 할당]

📊 현재 게임 상태:
  Current State: MainMenu (MainMenu, Playing, Paused 등)
  Current Stage Index: 0 (0~10 범위)
  Game Time: 0.0 (게임 진행 시간)
```

### 3.3 SceneController 설정
1. **CoreManagers → 우클릭 → Create Empty**
2. 이름을 `SceneController`로 변경
3. **Add Component → Scripts → Core → SceneController**

#### 🔧 SceneController 컴포넌트 상세 설정
```yaml
🎬 씬 설정:
  Main Menu Scene Name: "MainMenu"
  Game Scene Name: "Main"

⏳ 로딩 설정:
  Minimum Loading Time: 1.0 (0.5~5.0초 범위)
  Enable Fade Transition: ✅ 체크
  Fade Transition Duration: 0.5 (0.1~2.0초 범위)
```

### 3.4 StageManager 설정
1. **CoreManagers → 우클릭 → Create Empty**
2. 이름을 `StageManager`로 변경
3. **Add Component → Scripts → StageSystem → StageManager**

#### 🔧 StageManager 컴포넌트 상세 설정
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

### 3.5 AudioManager 설정
1. **CoreManagers → 우클릭 → Create Empty**
2. 이름을 `AudioManager`로 변경
3. **Add Component → Scripts → AudioSystem → AudioManager**

#### 🔧 AudioManager 컴포넌트 상세 설정
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

### 3.6 UIManager 설정
1. **CoreManagers → 우클릭 → Create Empty**
2. 이름을 `UIManager`로 변경
3. **Add Component → Scripts → UISystem → UIManager**
4. **Inspector 설정**:

#### 🔧 UIManager 컴포넌트 상세 설정
```yaml
🖼️ 메인 UI 패널:
  Main Canvas: [GameplayCanvas 할당]
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

### 3.7 InteractionManager 설정
1. **CoreManagers → 우클릭 → Create Empty**
2. 이름을 `InteractionManager`로 변경
3. **Add Component → Scripts → InteractionSystem → InteractionManager**

#### 🔧 InteractionManager 컴포넌트 상세 설정
```yaml
⚙️ 상호작용 설정:
  Enable Global Interactions: ✅ 체크
  Interaction Cooldown: 0.2 (0.1~2.0초 범위)
  Interaction Layer: Everything (-1)

🐛 디버그:
  Show Debug Info: ❌ 체크 해제 (프로덕션)
```

### 3.8 HorrorEventManager 설정
1. **CoreManagers → 우클릭 → Create Empty**
2. 이름을 `HorrorEventManager`로 변경
3. **Add Component → Scripts → HorrorSystem → HorrorEventManager**

#### 🔧 HorrorEventManager 컴포넌트 상세 설정
```yaml
😱 공포 설정:
  Enable Horror Events: ✅ 체크
  Global Cooldown: 5.0 (1.0~30.0초 범위)
  Max Concurrent Events: 1 (1~5개 범위)

🎬 컴포넌트:
  Horror Canvas: [공포 효과 전용 Canvas 할당 - 선택사항]
  Horror Camera: [공포 효과 전용 Camera 할당 - 선택사항]
```

### 3.9 ItemManager 설정
1. **CoreManagers → 우클릭 → Create Empty**
2. 이름을 `ItemManager`로 변경
3. **Add Component → Scripts → ItemSystem → ItemManager**

#### 🔧 ItemManager 컴포넌트 상세 설정
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

---

## 🔍 4. ZoomController 설정

### 4.1 ZoomController 생성
1. **GameplayCanvas → 우클릭 → Create Empty**
2. 이름을 `ZoomController`로 변경
3. **Add Component → Scripts → UISystem → ZoomController**

#### 🔧 ZoomController 컴포넌트 상세 설정
```yaml
RectTransform:
  Anchor: Center
  Position: (0, 0, 0)
  Width: 100, Height: 100
  Anchors: (0.5, 0.5, 0.5, 0.5)
  Pivot: (0.5, 0.5)

ZoomController:
  🔍 줌 설정:
    Max Zoom Scale: 3.0 (1.0~5.0 범위)
    Zoom Duration: 0.8 (0.1~2.0초 범위)
    Target Image: [런타임에 설정됨]
    Background Overlay: [ZoomBackgroundOverlay 할당]
  
  🎨 시각적 설정:
    Overlay Color: (0, 0, 0, 0.7) - 반투명 검정
    Zoom Curve: [EaseInOut AnimationCurve]
  
  🔊 오디오:
    Zoom In Sound: [줌 인 효과음 - 선택사항]
    Zoom Out Sound: [줌 아웃 효과음 - 선택사항]
```

### 4.2 Background Overlay 생성
1. **ZoomController → 우클릭 → UI → Image**
2. 이름을 `ZoomBackgroundOverlay`로 변경

#### 🔧 ZoomBackgroundOverlay 컴포넌트 상세 설정
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
  On Click(): ZoomController.ZoomOut()

GameObject:
  Active: ❌ 비활성화 (초기 상태)
```

---

## 🎮 5. 게임플레이 UI 구성

### 5.1 HUD Container 생성
1. **GameplayCanvas → 우클릭 → Create Empty**
2. 이름을 `HUD`로 변경
3. **RectTransform** 설정:
   - **Anchor Presets**: Stretch
   - **Left, Top, Right, Bottom**: 모두 0

### 5.2 인벤토리 UI
1. **HUD → 우클릭 → UI → Image**
2. 이름을 `InventoryPanel`로 변경
3. **RectTransform** 설정:
   - **Anchor**: Top Right
   - **Position**: (-100, -50, 0)
   - **Width**: 200, **Height**: 100
4. **Image** 설정:
   - **Source Image**: 인벤토리 배경 이미지
   - **Color**: (1, 1, 1, 0.8) - 반투명

#### 5.2.1 아이템 슬롯들
1. **InventoryPanel → 우클릭 → UI → Button**
2. 이름을 `KeySlot`로 변경
3. **RectTransform** 설정:
   - **Position**: (-50, 0, 0)
   - **Width**: 80, **Height**: 80
4. **Button** 설정:
   - **Source Image**: 아이템 슬롯 배경
   - **Target Graphic**: 자기 자신
5. **KeySlot 복사**하여 `FlashlightSlot` 생성
6. **Position**: (50, 0, 0)

### 5.3 스테이지 네비게이션 UI
1. **HUD → 우클릭 → Create Empty**
2. 이름을 `StageNavigation`로 변경
3. **Add Component → Scripts → UISystem → StageNavigationUI**
4. **RectTransform** 설정:
   - **Anchor**: Bottom Center
   - **Position**: (0, 50, 0)
   - **Width**: 800, **Height**: 100

#### 5.3.1 스테이지 버튼 프리팹 생성
1. **Assets → Create → Folder** → 이름을 `Prefabs`로 설정
2. **Hierarchy에서 StageNavigation → 우클릭 → UI → Button**
3. 이름을 `StageButton_Template`로 변경
4. **Scripts → UISystem → StageButtonUI** 컴포넌트 추가
5. **Project 창으로 드래그**하여 프리팹 생성
6. **Hierarchy에서 삭제** (템플릿은 프리팹으로 보관)

---

## 🏥 6. 스테이지 시스템 구성

### 6.1 Stage Container 생성
1. **StageCanvas → 우클릭 → Create Empty**
2. 이름을 `Stages`로 변경
3. **RectTransform** 설정:
   - **Anchor Presets**: Stretch
   - **Left, Top, Right, Bottom**: 모두 0

### 6.2 스테이지 데이터 생성
1. **Assets → Create → AFKS → Stage → Stage Data**
2. 이름을 `Stage0_HospitalExterior`로 설정
3. **Inspector 설정**:
   - **Stage Index**: 0
   - **Stage Name**: "병원 외부"
   - **Stage Description**: "버려진 병원의 입구입니다."
   - **Background Image**: 병원 외부 이미지 할당
   - **Ambient Color**: (0.8, 0.8, 1, 1) - 약간 푸른 톤
   - **Enable Dark Mode**: ✅ 체크

### 6.3 StageManager에 데이터 연결
1. **StageManager** 선택
2. **Stages** 배열 크기를 1로 설정
3. **Element 0**에 위에서 생성한 `Stage0_HospitalExterior` 할당

---

## 🚪 7. 병원 철문 상호작용 설정

### 7.1 철문 이미지 생성
1. **Stages → 우클릭 → UI → Image**
2. 이름을 `HospitalDoorImage`로 변경
3. **RectTransform** 설정:
   - **Anchor**: Center
   - **Position**: (0, 0, 0)
   - **Width**: 800, **Height**: 600 (이미지 크기에 맞게 조정)
4. **Image** 설정:
   - **Source Image**: 병원 철문 이미지 할당
   - **Preserve Aspect**: ✅ 체크
   - **Raycast Target**: ✅ 체크

### 7.2 쇠사슬 이미지 생성
1. **HospitalDoorImage → 우클릭 → UI → Image**
2. 이름을 `ChainImage`로 변경
3. **RectTransform** 설정:
   - **Anchor**: Center
   - **Position**: (0, -100, 0) (쇠사슬 위치에 맞게 조정)
   - **Width**: 300, **Height**: 200
4. **Image** 설정:
   - **Source Image**: 쇠사슬 이미지 할당
   - **Preserve Aspect**: ✅ 체크
   - **Raycast Target**: ✅ 체크
5. **GameObject → Set Active**: ❌ 비활성화 (처음에는 보이지 않음)

### 7.3 TwoStageInteractionController 설정
1. **HospitalDoorImage** 선택
2. **Add Component → Scripts → StageSystem → TwoStageInteractionController**

#### 🔧 TwoStageInteractionController 컴포넌트 상세 설정
```yaml
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

---

## 🎵 8. 오디오 설정

### 8.1 배경음악 설정
1. **AudioManager** 선택
2. **Play BGM On Start**: ✅ 체크
3. **Default BGM Clip**: 게임플레이 BGM 할당 (어두운 분위기)

### 8.2 환경음 설정
1. **AudioManager** 선택
2. **Environment Sounds** 배열에 다음 추가:
   - 바람 소리
   - 먼 곳의 불길한 소리
   - 간헐적인 금속음

### 8.3 상호작용 사운드
위에서 TwoStageInteractionController 설정 시 할당한 사운드들:
- **Door Click Sound**: 묵직한 철문 소리
- **Chain Click Sound**: 쇠사슬 부딪치는 소리
- **Chain Break Sound**: 쇠사슬 끊어지는 소리

---

## 👻 9. 호러 이벤트 설정

### 9.1 호러 이벤트 데이터 생성
1. **Assets → Create → AFKS → Horror → Horror Event Data**
2. 이름을 `Stage0_HorrorEvents`로 설정
3. **Inspector 설정**:
   - **Event Id**: "stage0_wind_sound"
   - **Event Name**: "불길한 바람소리"
   - **Description**: "갑작스럽게 들리는 불길한 바람소리"
   - **Trigger Type**: OnTimer
   - **Trigger Delay**: 30.0 (30초 후)
   - **Jumpscare Image**: 없음 (사운드만)
   - **Horror Sound**: 불길한 바람소리 오디오 클립
   - **Volume**: 0.6
   - **Enable Screen Shake**: ❌
   - **Can Repeat**: ✅ 체크
   - **Cooldown Time**: 60.0

### 9.2 HorrorEventManager에 연결
1. **HorrorEventManager** 선택
2. **Horror Events** 배열에 위에서 생성한 데이터 추가

---

## 🎯 10. 추가 UI 요소

### 10.1 디버그 정보 패널 (개발용)
1. **HUD → 우클릭 → UI → Panel**
2. 이름을 `DebugPanel`로 변경
3. **RectTransform** 설정:
   - **Anchor**: Top Left
   - **Position**: (10, -10, 0)
   - **Width**: 300, **Height**: 200
4. **Image** 설정:
   - **Color**: (0, 0, 0, 0.5) - 반투명 검정

#### 10.1.1 디버그 텍스트들
**DebugPanel** 안에 다음 텍스트들 추가:
- **FPS 표시**: 현재 프레임레이트
- **스테이지 정보**: 현재 스테이지 인덱스/이름
- **메모리 사용량**: 현재 메모리 사용량
- **클릭 카운터**: 현재 상호작용 진행도

### 10.2 메시지 표시 시스템
1. **HUD → 우클릭 → UI → Panel**
2. 이름을 `MessagePanel`로 변경
3. **RectTransform** 설정:
   - **Anchor**: Bottom Center
   - **Position**: (0, 100, 0)
   - **Width**: 600, **Height**: 80
4. **Panel → 우클릭 → UI → Text - TextMeshPro**
5. 이름을 `MessageText`로 변경
6. **Text** 설정:
   - **Font Size**: 24
   - **Color**: White
   - **Alignment**: Center and Middle
   - **Text**: (비워둠)

---

## ✅ 11. 씬 검증 체크리스트

### 11.1 필수 오브젝트 확인
- [ ] **GameplayCanvas** - 메인 UI 캔버스
- [ ] **StageCanvas** - 스테이지 전용 캔버스
- [ ] **CoreManagers** 그룹
  - [ ] **GameManager**
  - [ ] **SceneController** 
  - [ ] **StageManager**
  - [ ] **AudioManager**
  - [ ] **UIManager**
  - [ ] **InteractionManager**
  - [ ] **HorrorEventManager**
  - [ ] **ItemManager**
- [ ] **ZoomController**
- [ ] **HUD** 그룹
  - [ ] **InventoryPanel**
  - [ ] **StageNavigation**
  - [ ] **MessagePanel**
- [ ] **Stages** 그룹
  - [ ] **HospitalDoorImage**
  - [ ] **ChainImage**

### 11.2 컴포넌트 설정 확인
- [ ] **StageManager** → Stage Canvas 할당됨
- [ ] **ZoomController** → Background Overlay 할당됨
- [ ] **TwoStageInteractionController** → 모든 이미지 및 사운드 할당됨
- [ ] **StageData** → StageManager에 연결됨
- [ ] **HorrorEventData** → HorrorEventManager에 연결됨

### 11.3 기능 테스트
- [ ] **철문 클릭** → 확대됨
- [ ] **확대 상태에서 쇠사슬 클릭** → 카운터 증가
- [ ] **5번 클릭 완료** → 다음 스테이지로 이동
- [ ] **ESC 키** → 줌 해제 및 1단계 복귀
- [ ] **배경 클릭** → 줌 해제
- [ ] **BGM 재생** → 정상 작동
- [ ] **효과음** → 모든 상호작용에서 재생

---

## 🚨 12. 주의사항 및 팁

### 12.1 Build Settings 확인
- **File → Build Settings**에서 GameplayScene이 **Index 1**에 있는지 확인
- 다음 스테이지 씬들도 순서대로 추가

### 12.2 성능 최적화
- **Image** 컴포넌트의 **Raycast Target**을 클릭이 필요없는 곳은 해제
- **Canvas** → **Additional Shader Channels**: 필요한 것만 체크
- **오디오 클립**: 압축 설정 최적화

### 12.3 상호작용 설정 팁
- **쇠사슬 이미지 위치**: 실제 이미지에서 쇠사슬 위치와 정확히 맞춰야 함
- **줌 중심점**: 플레이어가 보고 싶어하는 부분으로 설정
- **클릭 횟수**: 플레이 테스트를 통해 적절한 값 조정

### 12.4 디버깅
- **Console 창**에서 상호작용 로그 확인
- **Scene 뷰**에서 UI 요소 위치 확인
- **Game 뷰**에서 실제 플레이 경험 테스트

---

## 📞 13. 문제 해결

### 13.1 줌이 작동하지 않는 경우
1. **ZoomController** 컴포넌트가 활성화되어 있는지 확인
2. **Target Image**가 올바르게 할당되었는지 확인
3. **Canvas Scaler** 설정 확인

### 13.2 상호작용이 작동하지 않는 경우
1. **Image** → **Raycast Target** 체크 확인
2. **TwoStageInteractionController** 설정 완료 확인
3. **EventSystem** 존재 확인

### 13.3 오디오 문제
1. **AudioManager** 볼륨 설정 확인
2. **Audio Listener** 존재 확인 (Camera에 있음)
3. **AudioClip** 포맷 및 압축 설정 확인

---

---

## 📈 14. 개선 사항 요약

### 🆕 **새로 추가된 내용**
✅ **최종 하이어라키 구조도** - 복잡한 게임플레이 씬의 전체 구조 시각화  
✅ **컴포넌트 상세 설정** - 모든 매니저와 상호작용 컴포넌트의 정확한 설정값  
✅ **YAML 형식 설정** - 복사 붙여넣기 가능한 설정 포맷  
✅ **필수 컴포넌트 명시** - 각 GameObject에 필요한 컴포넌트 리스트  
✅ **고급 상호작용 설정** - TwoStageInteractionController, ZoomController 상세 설정  

### 🎯 **이제 할 수 있는 것들**
- 🔧 **완벽한 설정**: 8개 매니저 시스템을 정확한 값으로 설정
- 🏗️ **구조 검증**: 복잡한 하이어라키 구조를 빠르게 확인
- 🎮 **고급 기능**: 줌 시스템과 2단계 상호작용 완벽 구현
- 📋 **품질 보증**: 상세한 체크리스트로 완성도 검증
- 🚀 **문제 해결**: 설정 오류 시 즉시 수정 가능

### 💡 **활용 팁**
1. **매니저 설정**: CoreManagers 그룹으로 체계적 관리
2. **상호작용**: TwoStageInteractionController로 복잡한 상호작용 구현
3. **구조 비교**: 현재 하이어라키와 가이드의 구조도 비교
4. **단계 건너뛰기**: 경험자는 구조도와 YAML 설정만으로 빠른 구성
5. **디버깅**: 각 컴포넌트별 상세 설정을 참고하여 문제 해결

### 🚀 **프로덕션 레벨 품질**
- **BaseSingleton 패턴**: 모든 매니저에 적용된 견고한 싱글톤 구조
- **이벤트 시스템**: 결합도 낮은 컴포넌트 간 통신
- **성능 최적화**: 메모리 관리와 풀링 시스템 내장
- **확장성**: 추가 스테이지와 상호작용 쉽게 확장 가능

---

이제 게임플레이 씬이 완성되었습니다! 

**다음 단계**: [스테이지 1 제작 가이드](03_Stage1_Creation_Guide.md)  
**구조 확인**: [하이어라키 구조 참조](05_Hierarchy_Structure_Reference.md)