# 🎮 AFKS 완전한 씬 구성 가이드 (고급 스테이지 시스템)

## 🆕 **업데이트된 시스템 개요**

이 가이드는 **고급 스테이지 시스템**을 기반으로 합니다:
- ✅ **픽셀 퍼펙트 상호작용**: 이미지 외곽선 정확 클릭 감지
- ✅ **자동 스테이지 관리**: 상호작용 객체 자동 생성/제거
- ✅ **영구 상태 저장**: 게임 재시작시에도 진행 상황 유지
- ✅ **자유로운 네비게이션**: M키로 스테이지 선택 및 이동
- ✅ **TextMeshPro 기본**: 한글 폰트 완벽 지원

## ⚠️ **중요: TextMeshPro 사용**

이 가이드는 **TextMeshPro**를 사용합니다! Legacy Text보다 성능과 품질이 우수합니다.

### TextMeshPro 첫 사용 시
1. **Window > TextMeshPro > Import TMP Essential Resources** 실행
2. 폰트 에셋 생성: **Window > TextMeshPro > Font Asset Creator**
3. **UI > Text - TextMeshPro** 선택하여 텍스트 생성

### TextMeshPro 성능 장점
- **🚀 렌더링 성능**: SDF 렌더링으로 50-70% 빠른 렌더링
- **⚡ 메모리 효율**: 50% 이상 메모리 절약, Draw Call 최적화
- **🎯 배칭**: Dynamic Batching으로 UI 성능 향상
- **🎨 스케일링**: 무손실 벡터 기반으로 모든 해상도 지원
- **🔧 고급 기능**: Rich Text, Auto Size, 링크 이벤트

---

## 📋 필요한 씬 목록

### 1. MainMenu 씬 (메인 메뉴)
### 2. GameplayScene 씬 (게임플레이 + 고급 스테이지 시스템)

## 🎯 **새로운 고급 스테이지 시스템 특징**

### **픽셀 퍼펙트 상호작용**
- **이미지 기반 클릭**: 십자가, 복잡한 모양도 정확한 외곽선 감지
- **Alpha 채널 활용**: 투명한 배경은 클릭되지 않음
- **성능 최적화**: 미리 계산된 마스크로 빠른 처리

### **자동 스테이지 관리**
- **자동 생성/제거**: 스테이지 변경시 상호작용 객체 자동 관리
- **부드러운 전환**: CanvasGroup 기반 페이드 효과
- **상태 저장**: 모든 클릭 진행 상황 영구 보존

### **스테이지 네비게이션**
- **M키 단축키**: 언제든 스테이지 선택 패널
- **자유 이동**: 잠금 해제된 스테이지 자유 탐험 (1→2는 일방통행)
- **시각적 피드백**: 색상으로 스테이지 상태 구분

---

## ⚠️ **중요: 버튼 이벤트 연결 방법**

**실제 SceneController 메서드들:**
- `LoadMainMenu()` - 메인 메뉴로 이동
- `LoadGameScene()` - 게임 씬으로 이동  
- `QuitGame()` - 게임 종료
- `ReloadCurrentScene()` - 현재 씬 재시작

**AdvancedStageManager 메서드들 (새로 추가):**
- `ChangeStage(int stageIndex)` - 특정 스테이지로 이동
- `GoToNextStage()` - 다음 스테이지로 이동
- `GoToPreviousStage()` - 이전 스테이지로 이동
- `UnlockStage(int stageIndex)` - 스테이지 잠금 해제

**버튼 이벤트 연결 순서:**
1. 버튼 선택 > Inspector > Button 컴포넌트
2. **On Click ()** > **+** 클릭
3. **None (Object)** 필드에 **SceneController** 또는 **AdvancedStageManager GameObject** 드래그
4. **No Function** 드롭다운에서 **원하는 메서드** 선택

---

## 🎯 STEP 1: InventorySlot 프리팹 생성 (최우선)

### 프리팹 생성
- **위치**: Assets/Resources/Prefabs/UI/Elements/
- **파일명**: InventorySlot.prefab

### 프리팹 구조
```
InventorySlot (Prefab Root)
├── ItemIcon
├── QuantityText
└── SelectionHighlight
```

### 상세 설정

#### Root (InventorySlot)
- **생성**: UI > Image
- **컴포넌트**:
  - RectTransform
    ```
    Width: 80, Height: 80
    Anchor: Middle Center
    ```
  - Image
    ```
    Color: (0.3, 0.3, 0.3, 1) - 회색
    ```
  - InventorySlotUI.cs 스크립트 추가

#### ItemIcon (Root 하위)
- **생성**: UI > Image
- **이름**: "ItemIcon"
- **컴포넌트**:
  - RectTransform
    ```
    Width: 70, Height: 70
    Anchor: Middle Center
    Position: (0, 0, 0)
    ```
  - Image
    ```
    Color: (1, 1, 1, 0) - 투명
    Preserve Aspect: true
    ```

#### QuantityText (Root 하위)
- **생성**: UI > Text - TextMeshPro
- **이름**: "QuantityText"
- **컴포넌트**:
  - RectTransform
    ```
    Width: 20, Height: 20
    Anchor: Bottom Right
    Anchored Position: (-5, 5)
    ```
  - TextMeshProUGUI
    ```
    Text: ""
    Font Size: 12
    Color: White
    Alignment: Center
    Auto Size: Best Fit
    ```

#### SelectionHighlight (Root 하위)
- **생성**: UI > Image
- **이름**: "SelectionHighlight"
- **컴포넌트**:
  - RectTransform
    ```
    Width: 80, Height: 80
    Anchor: Middle Center
    Position: (0, 0, 0)
    ```
  - Image
    ```
    Color: (0, 1, 0, 0.5) - 반투명 녹색
    ```
- **GameObject 초기 비활성화**

### 프리팹 저장
1. Hierarchy에서 InventorySlot 선택
2. Project 창으로 드래그하여 프리팹 생성
3. Hierarchy에서 원본 삭제

---

## 🏠 STEP 2: MainMenu 씬 구성

### 씬 생성
- **생성**: File > New Scene > 2D (Built-in Render Pipeline)
- **저장**: Assets/Scenes/MainMenu.unity

### 하이어라키 구조
```
MainMenu
├── --- MANAGERS ---
│   ├── AudioManager
│   ├── GameManager
│   └── UIManager
├── MenuCanvas
│   ├── BackgroundImage
│   ├── TitleText
│   ├── ButtonContainer
│   │   ├── StartButton
│   │   ├── SettingsButton
│   │   └── ExitButton
│   └── VersionText
├── SettingsCanvas
│   └── SettingsPanel (비활성화)
├── Main Camera
├── EventSystem (자동 생성 확인)
└── SceneController
```

### 상세 구성

#### 1. MANAGERS 그룹
##### --- MANAGERS ---
- **생성**: Create Empty
- **Transform**: (0, 0, 0)

##### AudioManager
- **생성**: Create Empty (MANAGERS 하위)
- **컴포넌트**: AudioManager.cs
- **설정**: 모든 기본값 사용

##### GameManager
- **생성**: Create Empty (MANAGERS 하위)
- **컴포넌트**: GameManager.cs
- **설정**: Current State = MainMenu

##### UIManager
- **생성**: Create Empty (MANAGERS 하위)
- **컴포넌트**: UIManager.cs

#### 2. MenuCanvas
- **생성**: UI > Canvas
- **컴포넌트**:
  - Canvas
    ```
    Render Mode: Screen Space - Overlay
    Sort Order: 0
    ```
  - Canvas Scaler
    ```
    UI Scale Mode: Scale With Screen Size
    Reference Resolution: 1920 x 1080
    Match: 0.5
    ```

##### BackgroundImage (MenuCanvas 하위)
- **생성**: UI > Image
- **컴포넌트**:
  - RectTransform (Stretch 전체)
  - Image (메인메뉴 배경 이미지)

##### TitleText (MenuCanvas 하위)
- **생성**: UI > Text - TextMeshPro
- **컴포넌트**:
  - RectTransform (Top-Center, 800x100)
  - TextMeshProUGUI
    ```
    Text: "AFKS"
    Font Size: 72
    Alignment: Center
    Color: White
    Auto Size: Best Fit
    ```

##### ButtonContainer (MenuCanvas 하위)
- **생성**: Create Empty
- **컴포넌트**:
  - RectTransform (Center, 300x400)
  - Vertical Layout Group
    ```
    Spacing: 20
    Child Alignment: Middle Center
    ```

##### StartButton (ButtonContainer 하위)
- **생성**: UI > Button - TextMeshPro
- **컴포넌트**:
  - RectTransform (300x60)
  - Button
- **하위 Text (자동 생성됨)**:
  - TextMeshProUGUI: "게임 시작"
  - Font Size: 24
  - Color: Black

##### SettingsButton (ButtonContainer 하위)
- **생성**: UI > Button - TextMeshPro
- **하위 Text (자동 생성됨)**:
  - TextMeshProUGUI: "설정"
  - Font Size: 24
  - Color: Black

##### ExitButton (ButtonContainer 하위)
- **생성**: UI > Button - TextMeshPro
- **하위 Text (자동 생성됨)**:
  - TextMeshProUGUI: "종료"
  - Font Size: 24
  - Color: Black

##### VersionText (MenuCanvas 하위)
- **생성**: UI > Text - TextMeshPro
- **컴포넌트**:
  - RectTransform (Bottom-Right, 200x30)
  - TextMeshProUGUI
    ```
    Text: "Version 1.0"
    Font Size: 16
    Color: Gray
    ```

#### 3. SettingsCanvas
- **생성**: UI > Canvas
- **컴포넌트**:
  - Canvas
    ```
    Render Mode: Screen Space - Overlay
    Sort Order: 10
    ```

##### SettingsPanel (SettingsCanvas 하위)
- **생성**: UI > Panel
- **컴포넌트**:
  - RectTransform (Center, 600x400)
  - CanvasGroup
- **GameObject 비활성화**

#### 4. Main Camera
- **컴포넌트**:
  - Camera
    ```
    Clear Flags: Solid Color
    Background: (0, 0, 0, 1)
    Projection: Orthographic
    Size: 5
    ```

#### 5. EventSystem 확인
- **확인**: Hierarchy에 "EventSystem" 존재하는지 체크
- **없으면**: UI > Event System 생성

#### 6. SceneController
- **생성**: Create Empty
- **컴포넌트**: SceneController.cs
- **설정**:
  ```
  Main Menu Scene Name: "MainMenu"
  Game Scene Name: "GameplayScene"
  Loading Scene Name: "Loading"
  Minimum Loading Time: 1
  Enable Fade Transition: true
  Fade Transition Duration: 0.5
  ```

### MainMenu UIManager 연결
```
Menu Canvas: MenuCanvas
Settings Panel: SettingsPanel
```

### 버튼 이벤트 연결 (정확한 메서드명)

#### StartButton 설정
1. **StartButton 선택** > Inspector > Button 컴포넌트
2. **On Click ()** > **+** 클릭
3. **None (Object)** 필드에 **SceneController** 드래그
4. **No Function** 드롭다운에서 **SceneController > LoadGameScene()** 선택

#### ExitButton 설정  
1. **ExitButton 선택** > Inspector > Button 컴포넌트
2. **On Click ()** > **+** 클릭
3. **None (Object)** 필드에 **SceneController** 드래그
4. **No Function** 드롭다운에서 **SceneController > QuitGame()** 선택

#### 사용 가능한 메서드들
```
SceneController.LoadMainMenu() - 메인 메뉴로 이동
SceneController.LoadGameScene() - 게임 씬으로 이동
SceneController.QuitGame() - 게임 종료
SceneController.ReloadCurrentScene() - 현재 씬 재시작
```

---

## 🎮 STEP 3: GameplayScene 씬 구성

### 씬 생성
- **생성**: File > New Scene > 2D (Built-in Render Pipeline)
- **저장**: Assets/Scenes/GameplayScene.unity

### 하이어라키 구조
```
GameplayScene
├── --- MANAGERS ---
│   ├── AudioManager
│   ├── GameManager
│   ├── UIManager
│   ├── InventoryManager
│   ├── StageManager
│   ├── HorrorEventManager
│   └── InteractionManager
├── MainCanvas
│   ├── GameplayPanel
│   ├── InventoryPanel
│   │   ├── Background
│   │   ├── InventoryContainer
│   │   └── ItemDetailPanel
│   │       ├── ItemDetailImage
│   │       ├── ItemNameText
│   │       └── ItemDescriptionText
│   ├── MenuPanel
│   ├── SettingsPanel
│   └── LoadingPanel
├── OverlayCanvas
│   ├── FadeOverlay
│   ├── DialogPanel
│   └── NotificationPanel
├── StageCanvas
│   └── BackgroundImage
├── TransitionCanvas
│   └── TransitionOverlay
├── HorrorCanvas
├── Main Camera
├── EventSystem (자동 생성 확인)
└── SceneController
```

### 상세 구성

#### 1. MANAGERS 그룹
##### --- MANAGERS ---
- **생성**: Create Empty

##### AudioManager
- **생성**: Create Empty (MANAGERS 하위)
- **컴포넌트**: AudioManager.cs
- **설정**:
  ```
  Max Sfx Sources: 8
  Crossfade Duration: 1.0
  Master Volume: 1.0
  BGM Volume: 0.7
  SFX Volume: 0.8
  Ambient Volume: 0.5
  ```

##### GameManager
- **생성**: Create Empty (MANAGERS 하위)
- **컴포넌트**: GameManager.cs
- **설정**:
  ```
  Current State: Playing
  Current Stage Index: 0
  ```

##### UIManager
- **생성**: Create Empty (MANAGERS 하위)
- **컴포넌트**: UIManager.cs

##### KeyItemManager (권장) ⭐
- **생성**: Create Empty (MANAGERS 하위)  
- **컴포넌트**: KeyItemManager.cs
- **설정**:
  ```
  Key Items: [Key ItemData, Flashlight ItemData] (2개)
  Auto Save: true
  ```

##### InventoryManager (레거시)
- **생성**: Create Empty (MANAGERS 하위)
- **컴포넌트**: InventoryManager.cs (Obsolete)
- **설정**:
  ```
  Max Slots: 2 (KeyItem 시스템에 맞게 조정)
  Allow Duplicates: false
  Auto Save: true
  ```
- **⚠️ 참고**: KeyItemManager 사용 권장

##### StageManager
- **생성**: Create Empty (MANAGERS 하위)
- **컴포넌트**: StageManager.cs
- **설정**:
  ```
  Current Stage Index: 0
  Transition Duration: 1.0
  Enable Preloading: true
  Max Preload Stages: 2
  ```

##### HorrorEventManager
- **생성**: Create Empty (MANAGERS 하위)
- **컴포넌트**: HorrorEventManager.cs
- **설정**:
  ```
  Enable Horror Events: true
  Global Cooldown: 5.0
  Max Concurrent Events: 1
  ```

##### InteractionManager
- **생성**: Create Empty (MANAGERS 하위)
- **컴포넌트**: InteractionManager.cs

#### 2. MainCanvas
- **생성**: UI > Canvas
- **컴포넌트**:
  - Canvas
    ```
    Render Mode: Screen Space - Overlay
    Sort Order: 0
    ```
  - Canvas Scaler (MainMenu와 동일)

##### GameplayPanel (MainCanvas 하위)
- **생성**: UI > Panel
- **컴포넌트**:
  - RectTransform (Stretch 전체)
  - Image
    ```
    Color: (0, 0, 0, 0) - 투명
    ```
  - CanvasGroup
    ```
    Alpha: 1
    Interactable: true
    Blocks Raycasts: true
    ```

##### InventoryPanel (MainCanvas 하위)
- **생성**: UI > Panel
- **컴포넌트**:
  - RectTransform (Stretch 전체)
  - Image
    ```
    Color: (0, 0, 0, 0.8) - 반투명 검정
    ```
  - CanvasGroup
    ```
    Alpha: 0
    Interactable: false
    Blocks Raycasts: false
    ```
- **GameObject 비활성화**

###### Background (InventoryPanel 하위)
- **생성**: UI > Image
- **컴포넌트**:
  - RectTransform (Center, 800x600)
  - Image
    ```
    Color: (0.2, 0.2, 0.2, 1)
    ```

###### InventoryContainer (Background 하위)
- **생성**: Create Empty
- **이름**: "InventoryContainer"
- **컴포넌트**:
  - RectTransform (Center-Left, 400x500)
  - GridLayoutGroup
    ```
    Cell Size: 80 x 80
    Spacing: 10 x 10
    Start Corner: Upper Left
    Start Axis: Horizontal
    Child Alignment: Upper Left
    Constraint: Fixed Column Count (5)
    ```

###### ItemDetailPanel (Background 하위)
- **생성**: UI > Panel
- **컴포넌트**:
  - RectTransform (Center-Right, 300x500)

###### ItemDetailImage (ItemDetailPanel 하위)
- **생성**: UI > Image
- **이름**: "ItemDetailImage"
- **컴포넌트**:
  - RectTransform (Top, 200x200)

###### ItemNameText (ItemDetailPanel 하위)
- **생성**: UI > Text - TextMeshPro
- **이름**: "ItemNameText"
- **컴포넌트**:
  - RectTransform (Center, 280x30)
  - TextMeshProUGUI
    ```
    Font Size: 24
    Alignment: Center
    Color: White
    Auto Size: Best Fit
    ```

###### ItemDescriptionText (ItemDetailPanel 하위)
- **생성**: UI > Text - TextMeshPro
- **이름**: "ItemDescriptionText"
- **컴포넌트**:
  - RectTransform (Bottom, 280x200)
  - TextMeshProUGUI
    ```
    Font Size: 16
    Alignment: Upper Left
    Color: White
    Enable Word Wrapping: true
    ```

##### MenuPanel (MainCanvas 하위)
- **생성**: UI > Panel
- **컴포넌트**: InventoryPanel과 동일
- **GameObject 비활성화**

##### SettingsPanel (MainCanvas 하위)
- **생성**: UI > Panel
- **컴포넌트**: InventoryPanel과 동일
- **GameObject 비활성화**

##### LoadingPanel (MainCanvas 하위)
- **생성**: UI > Panel
- **컴포넌트**: InventoryPanel과 동일
- **GameObject 비활성화**

#### 3. OverlayCanvas
- **생성**: UI > Canvas
- **컴포넌트**:
  - Canvas
    ```
    Render Mode: Screen Space - Overlay
    Sort Order: 100
    ```

##### FadeOverlay (OverlayCanvas 하위)
- **생성**: UI > Image
- **컴포넌트**:
  - RectTransform (Stretch 전체)
  - Image
    ```
    Color: (0, 0, 0, 0)
    Raycast Target: false
    ```
  - CanvasGroup
    ```
    Alpha: 0
    Interactable: false
    Blocks Raycasts: false
    ```

##### DialogPanel (OverlayCanvas 하위)
- **생성**: UI > Panel
- **컴포넌트**:
  - RectTransform (Bottom, 1200x200)
  - CanvasGroup (Alpha: 0)
- **GameObject 비활성화**

##### NotificationPanel (OverlayCanvas 하위)
- **생성**: UI > Panel
- **컴포넌트**:
  - RectTransform (Top, 400x100)
  - CanvasGroup (Alpha: 0)
- **GameObject 비활성화**

#### 4. StageCanvas
- **생성**: UI > Canvas
- **컴포넌트**:
  - Canvas
    ```
    Render Mode: Screen Space - Camera
    Render Camera: Main Camera (드래그)
    Plane Distance: 10
    Sort Order: -10
    ```

##### BackgroundImage (StageCanvas 하위)
- **생성**: UI > Image
- **이름**: "BackgroundImage"
- **컴포넌트**:
  - RectTransform (Stretch 전체)
  - Image
    ```
    Preserve Aspect: true
    ```

#### 5. TransitionCanvas
- **생성**: UI > Canvas
- **컴포넌트**:
  - Canvas
    ```
    Render Mode: Screen Space - Overlay
    Sort Order: 500
    ```

##### TransitionOverlay (TransitionCanvas 하위)
- **생성**: UI > Image
- **컴포넌트**:
  - RectTransform (Stretch 전체)
  - Image
    ```
    Color: (0, 0, 0, 0)
    ```
  - CanvasGroup (Alpha: 0)

#### 6. HorrorCanvas
- **생성**: UI > Canvas
- **컴포넌트**:
  - Canvas
    ```
    Render Mode: Screen Space - Overlay
    Sort Order: 1000
    ```
- **GameObject 비활성화**

#### 7. Main Camera
- **컴포넌트**:
  - Camera
    ```
    Clear Flags: Solid Color
    Background: (0, 0, 0, 1)
    Projection: Orthographic
    Size: 5
    ```

#### 8. EventSystem 확인
- **확인**: Hierarchy에 "EventSystem" 존재 체크

#### 9. SceneController
- **생성**: Create Empty
- **컴포넌트**: SceneController.cs
- **설정**:
  ```
  Main Menu Scene Name: "MainMenu"
  Game Scene Name: "GameplayScene"
  Loading Scene Name: "Loading"
  Minimum Loading Time: 1
  Enable Fade Transition: true
  Fade Transition Duration: 0.5
  ```

### GameplayScene 매니저 연결

#### UIManager 연결
```
Main Canvas: MainCanvas
Gameplay Panel: GameplayPanel
Inventory Panel: InventoryPanel
Menu Panel: MenuPanel
Settings Panel: SettingsPanel
Loading Panel: LoadingPanel
Fade Overlay: FadeOverlay
Dialog Panel: DialogPanel
Notification Panel: NotificationPanel
Inventory Container: InventoryContainer
Inventory Slot Prefab: InventorySlot (프리팹 드래그)
Item Name Text: ItemNameText
Item Description Text: ItemDescriptionText
Item Detail Image: ItemDetailImage
Panel Transition Duration: 0.3
Fade Transition Duration: 0.5
Enable UI Animations: true
```

#### AdvancedStageManager 연결 ⭐ (새로운 고급 시스템)
```
📋 스테이지 데이터:
  Stages: StageData 배열 (Stage1.asset, Stage2.asset 등)
  Current Stage Index: 0

🎨 UI 참조:
  Background Image: BackgroundImage (Image 컴포넌트)
  Stage Canvas: StageCanvas (CanvasGroup 컴포넌트)
  Transition Canvas: TransitionCanvas (CanvasGroup 컴포넌트)
  Interaction Points Parent: InteractionPoints (Transform)

⚙️ 전환 설정:
  Transition Duration: 1.0
  Transition Curve: AnimationCurve (EaseInOut)
  Max Preload Stages: 2

🔒 스테이지 잠금 시스템:
  One Way Transitions: 
    - From Stage: 1, To Stage: 2 (1→2 일방통행)

💾 상태 저장:
  Enable Auto Save: true
  Auto Save Interval: 30 (초)

🎮 상호작용 프리팹:
  Interaction Prefabs: 상호작용 타입별 프리팹 배열
```

#### StageNavigationUI 연결 ⭐ (새로 추가)
```
🎯 UI 요소들:
  Stage Selection Panel: StageNavigationPanel (GameObject)
  Stage Button Parent: StageButtonParent (Transform)
  Stage Button Prefab: StageButtonPrefab (GameObject)
  Current Stage Text: CurrentStageText (TextMeshProUGUI)

⚙️ 네비게이션 설정:
  Quick Nav Key: KeyCode.M
  Animation Duration: 0.3
  Auto Hide Time: 10.0

🎨 버튼 색상 설정:
  Locked Stage Color: Gray
  Unlocked Stage Color: White
  Current Stage Color: Green
  Restricted Stage Color: Red
```

#### HorrorEventManager 연결
```
Horror Canvas: HorrorCanvas
Enable Horror Events: true
Global Cooldown: 5.0
Max Concurrent Events: 1
```

---

## 🔧 STEP 4: Build Settings 설정

### Build Settings
1. File > Build Settings
2. Add Open Scenes 클릭
3. 씬 순서:
   ```
   0: MainMenu
   1: GameplayScene
   ```

---

## ✅ 완성 확인 체크리스트

### 프리팹 체크리스트
- [ ] TextMeshPro Essential Resources 임포트됨
- [ ] InventorySlot.prefab 생성됨
- [ ] InventorySlotUI.cs 컴포넌트 추가됨
- [ ] ItemIcon, QuantityText(TMP), SelectionHighlight 구조 완성됨

### MainMenu 씬 체크리스트
- [ ] 3개 매니저 생성됨 (Audio, Game, UI)
- [ ] MenuCanvas와 하위 UI 완성됨 (모든 텍스트가 TextMeshPro)
- [ ] SettingsCanvas 생성됨
- [ ] SceneController 추가됨
- [ ] EventSystem 존재함
- [ ] 버튼 이벤트 연결됨 (StartButton→LoadGameScene, ExitButton→QuitGame)

### GameplayScene 씬 체크리스트
- [ ] 7개 매니저 생성됨
- [ ] 5개 Canvas 생성됨 (Main, Overlay, Stage, Transition, Horror)
- [ ] InventoryPanel 하위 구조 완성됨 (모든 텍스트가 TextMeshPro)
- [ ] SceneController 추가됨
- [ ] EventSystem 존재함
- [ ] UIManager 모든 참조 연결됨
- [ ] StageManager 참조 연결됨
- [ ] HorrorEventManager 참조 연결됨

### 테스트 체크리스트
- [ ] MainMenu에서 "게임 시작" 버튼 클릭 → GameplayScene 전환 성공
- [ ] MainMenu에서 "종료" 버튼 클릭 → 게임 종료 성공
- [ ] GameplayScene에서 ESC키로 메뉴 토글
- [ ] GameplayScene에서 Tab키로 인벤토리 토글
- [ ] Console에 "[SceneController]" 로그 메시지 확인
- [ ] 인벤토리 슬롯 동적 생성 확인
- [ ] 모든 한글 텍스트 정상 표시 (TextMeshPro)

---

## 🎯 성공!

이제 **완전히 작동하는 AFKS 게임**이 준비되었습니다!

### 주요 기능
- ✅ 씬 전환 시스템
- ✅ 완전한 UI 시스템
- ✅ 인벤토리 시스템
- ✅ 스테이지 시스템
- ✅ 공포 이벤트 시스템
- ✅ 오디오 시스템

모든 매니저가 서로 연동되어 완전한 게임이 구성되었습니다! 🎉