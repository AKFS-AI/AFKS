# 📋 메인메뉴 씬 설정 가이드

## 🎯 개요
이 가이드는 AFKS 게임의 메인메뉴 씬을 처음부터 설정하는 방법을 단계별로 설명합니다.

---

## 📁 1. 씬 생성 및 기본 설정

### 1.1 새 씬 생성
1. **File → New Scene** 선택
2. **2D Template** 선택
3. **File → Save As** → `Assets/Scenes/MainMenu.unity`로 저장

### 1.2 씬 설정 확인
- **Camera** 설정:
  - Position: (0, 0, -10)
  - Projection: Orthographic
  - Size: 5
  - Background Color: #000000 (검은색)

---

## 🏗️ 1.3 최종 하이어라키 구조도

완성된 메인메뉴 씬의 하이어라키 구조는 다음과 같습니다:

```
MainMenu (Scene)
├── 📷 Main Camera
│   └── [Camera, AudioListener]
├── 🖼️ MainMenuCanvas
│   ├── [Canvas, CanvasScaler, GraphicRaycaster]
│   ├── 🖼️ BackgroundImage
│   │   └── [RectTransform, Image]
│   ├── 📝 TitleText (TMP)
│   │   └── [RectTransform, TextMeshProUGUI]
│   ├── 🔘 StartGameButton
│   │   ├── [RectTransform, Image, Button, AudioSource]
│   │   └── 📝 Text (TMP)
│   │       └── [RectTransform, TextMeshProUGUI]
│   ├── 🔘 SettingsButton
│   │   ├── [RectTransform, Image, Button, AudioSource]
│   │   └── 📝 Text (TMP)
│   │       └── [RectTransform, TextMeshProUGUI]
│   ├── 🔘 ExitButton
│   │   ├── [RectTransform, Image, Button, AudioSource]
│   │   └── 📝 Text (TMP)
│   │       └── [RectTransform, TextMeshProUGUI]
│   └── 📝 VersionText (TMP) [선택사항]
│       └── [RectTransform, TextMeshProUGUI]
├── ⚙️ GameManager
│   └── [Transform, GameManager]
├── 🎬 SceneController
│   └── [Transform, SceneController]
├── 🔊 AudioManager
│   └── [Transform, AudioManager, AudioSource]
├── 🖱️ UIManager
│   └── [Transform, UIManager]
└── 🎮 EventSystem
    └── [Transform, EventSystem, StandaloneInputModule]
```

**범례**: [ ] 안은 해당 GameObject에 필요한 컴포넌트들입니다.

---

## 🖼️ 2. UI Canvas 설정

### 2.1 Canvas 생성
1. **Hierarchy → 우클릭 → UI → Canvas**
2. Canvas 이름을 `MainMenuCanvas`로 변경

#### 🔧 Canvas 컴포넌트 상세 설정
```yaml
Canvas:
  Render Mode: Screen Space - Overlay
  Pixel Perfect: ✅ 체크
  Sort Order: 0
  Target Display: Display 1
  Additional Shader Channels: None
```

#### 🔧 GraphicRaycaster 컴포넌트 설정
```yaml
GraphicRaycaster:
  Ignore Reversed Graphics: ✅ 체크
  Blocking Objects: None
  Blocking Mask: Everything
```

### 2.2 Canvas Scaler 설정
Canvas 선택 → Inspector에서 **Canvas Scaler** 컴포넌트:

#### 🔧 CanvasScaler 컴포넌트 상세 설정
```yaml
CanvasScaler:
  UI Scale Mode: Scale With Screen Size
  Reference Resolution: 1920 x 1080
  Screen Match Mode: Match Width Or Height
  Match: 0.5
  Reference Pixels Per Unit: 100
```

### 2.3 EventSystem 확인
- Canvas 생성 시 자동으로 생성되는 **EventSystem** 오브젝트 확인
- 없다면: **Hierarchy → 우클릭 → UI → Event System**

#### 🔧 EventSystem 컴포넌트 설정
```yaml
EventSystem:
  First Selected: None
  Send Navigation Events: ✅ 체크
  Drag Threshold: 10

StandaloneInputModule:
  Horizontal Axis: Horizontal
  Vertical Axis: Vertical
  Submit Button: Submit
  Cancel Button: Cancel
  Input Actions Per Second: 10
  Repeat Delay: 0.5
```

---

## 🎮 3. 메인메뉴 UI 구성

### 3.1 배경 이미지 설정
1. **MainMenuCanvas → 우클릭 → UI → Image**
2. 이름을 `BackgroundImage`로 변경

#### 🔧 BackgroundImage 컴포넌트 상세 설정
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

### 3.2 타이틀 텍스트
1. **MainMenuCanvas → 우클릭 → UI → Text - TextMeshPro**
2. 이름을 `TitleText`로 변경

#### 🔧 TitleText 컴포넌트 상세 설정
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

### 3.3 메뉴 버튼들 생성

#### 3.3.1 게임 시작 버튼
1. **MainMenuCanvas → 우클릭 → UI → Button - TextMeshPro**
2. 이름을 `StartGameButton`로 변경

#### 🔧 StartGameButton 컴포넌트 상세 설정
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
  On Click(): SceneController.LoadGameScene()

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

# Text (TMP) 자식 오브젝트:
Text_TextMeshProUGUI:
  Text: "게임 시작"
  Font Size: 36
  Color: (0, 0, 0, 1) - 검은색
  Alignment: Center and Middle
  Auto Size: ❌ 체크 해제
  Raycast Target: ❌ 체크 해제
```

#### 3.3.2 설정 버튼
1. **StartGameButton 복사** (Ctrl+D)
2. 이름을 `SettingsButton`으로 변경
3. **Position**: (0, -50, 0)
4. **Text**: "설정"
5. **On Click()**: UIManager.ShowPanel(Settings)

#### 3.3.3 종료 버튼
1. **SettingsButton 복사** (Ctrl+D)
2. 이름을 `ExitButton`으로 변경
3. **Position**: (0, -150, 0)
4. **Text**: "게임 종료"
5. **On Click()**: GameManager.QuitGame()

---

## ⚙️ 4. 매니저 시스템 설정

### 4.1 GameManager 설정
1. **Hierarchy → 우클릭 → Create Empty**
2. 이름을 `GameManager`로 변경
3. **Add Component → Scripts → Core → GameManager**

#### 🔧 GameManager 컴포넌트 상세 설정
```yaml
Transform:
  Position: (0, 0, 0)
  Rotation: (0, 0, 0)
  Scale: (1, 1, 1)

🎮 게임 설정:
  Game Config: [GameConfig ScriptableObject 할당]

📊 현재 게임 상태:
  Current State: MainMenu (MainMenu, Playing, Paused 등)
  Current Stage Index: 0 (0~10 범위)
  Game Time: 0.0 (게임 진행 시간)
```

### 4.2 SceneController 설정
1. **Hierarchy → 우클릭 → Create Empty**
2. 이름을 `SceneController`로 변경
3. **Add Component → Scripts → Core → SceneController**

#### 🔧 SceneController 컴포넌트 상세 설정
```yaml
Transform:
  Position: (0, 0, 0)
  Rotation: (0, 0, 0)
  Scale: (1, 1, 1)

🎬 씬 설정:
  Main Menu Scene Name: "MainMenu"
  Game Scene Name: "Main"

⏳ 로딩 설정:
  Minimum Loading Time: 1.0 (0.5~5.0초 범위)
  Enable Fade Transition: ✅ 체크
  Fade Transition Duration: 0.5 (0.1~2.0초 범위)
```

### 4.3 AudioManager 설정
1. **Hierarchy → 우클릭 → Create Empty**
2. 이름을 `AudioManager`로 변경
3. **Add Component → Scripts → AudioSystem → AudioManager**

#### 🔧 AudioManager 컴포넌트 상세 설정
```yaml
Transform:
  Position: (0, 0, 0)
  Rotation: (0, 0, 0)
  Scale: (1, 1, 1)

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

### 4.4 UIManager 설정
1. **Hierarchy → 우클릭 → Create Empty**
2. 이름을 `UIManager`로 변경
3. **Add Component → Scripts → UISystem → UIManager**

#### 🔧 UIManager 컴포넌트 상세 설정
```yaml
Transform:
  Position: (0, 0, 0)
  Rotation: (0, 0, 0)
  Scale: (1, 1, 1)

🖼️ 메인 UI 패널:
  Main Canvas: [MainMenuCanvas 할당]
  Gameplay Panel: [UI/GameplayPanel 할당 - 선택사항]
  Inventory Panel: [UI/InventoryPanel 할당 - 선택사항]
  Menu Panel: [UI/MenuPanel 할당 - 선택사항]
  Settings Panel: [UI/SettingsPanel 할당 - 선택사항]
  Loading Panel: [UI/LoadingPanel 할당 - 선택사항]

📱 오버레이 패널:
  Fade Overlay: [UI/FadeOverlay 할당 - 선택사항]
  Dialog Panel: [UI/DialogPanel 할당 - 선택사항]
  Notification Panel: [UI/NotificationPanel 할당 - 선택사항]

🎒 인벤토리 UI:
  Inventory Container: [선택사항]
  Inventory Slot Prefab: [선택사항]
  Item Name Text: [선택사항]
  Item Description Text: [선택사항]
  Item Detail Image: [선택사항]

⚙️ 설정:
  Panel Transition Duration: 0.3 (0.1~2.0초 범위)
  Fade Transition Duration: 0.5 (0.1~2.0초 범위)
  Enable UI Animations: ✅ 체크
```

---

## 🔗 5. 버튼 이벤트 연결

### 5.1 게임 시작 버튼
1. **StartGameButton** 선택 → **Button** 컴포넌트
2. **On Click ()** 에서 **+** 버튼 클릭
3. **None (Object)** 영역에 **SceneController** 오브젝트 드래그
4. **No Function** 드롭다운에서 **SceneController → LoadGameScene()**

### 5.2 설정 버튼
1. **SettingsButton** 선택 → **Button** 컴포넌트
2. **On Click ()** 에서 **+** 버튼 클릭
3. **None (Object)** 영역에 **UIManager** 오브젝트 드래그
4. **No Function** 드롭다운에서 **UIManager → ShowPanel(UIPanel)**
5. **UIPanel** 값을 **Settings**로 설정

### 5.3 종료 버튼
1. **ExitButton** 선택 → **Button** 컴포넌트
2. **On Click ()** 에서 **+** 버튼 클릭
3. **None (Object)** 영역에 **GameManager** 오브젝트 드래그
4. **No Function** 드롭다운에서 **GameManager → QuitGame()**

---

## 🎵 6. 오디오 설정

### 6.1 BGM 설정
1. **AudioManager** 선택
2. **Play BGM On Start**: ✅ 체크
3. **Default BGM Clip**: 메인메뉴 BGM 오디오 클립 할당

### 6.2 버튼 사운드 설정
각 버튼에 클릭 사운드 추가:
1. 버튼 선택 → **Add Component → Audio → Audio Source**
2. **Settings**:
   - **AudioClip**: 버튼 클릭 사운드
   - **Play On Awake**: ❌ 체크 해제
   - **Volume**: 0.5

---

## 📱 7. 추가 UI 요소 (선택사항)

### 7.1 버전 정보 텍스트
1. **MainMenuCanvas → 우클릭 → UI → Text - TextMeshPro**
2. 이름을 `VersionText`로 변경
3. 설정:
   - **Text**: "v1.0.0"
   - **Font Size**: 24
   - **Color**: Gray
   - **Anchor**: Bottom Right
   - **Position**: (-50, 50, 0)

### 7.2 로딩 애니메이션 (선택사항)
1. **MainMenuCanvas → 우클릭 → UI → Image**
2. 이름을 `LoadingSpinner`로 변경
3. **Source Image**: 로딩 스피너 이미지
4. **Add Component → Animation → Animator**
5. 로딩 회전 애니메이션 설정

---

## ✅ 8. 씬 검증 체크리스트

### 8.1 필수 오브젝트 확인
- [ ] **MainMenuCanvas** - UI 캔버스
- [ ] **BackgroundImage** - 배경 이미지
- [ ] **TitleText** - 게임 제목
- [ ] **StartGameButton** - 게임 시작 버튼
- [ ] **SettingsButton** - 설정 버튼
- [ ] **ExitButton** - 종료 버튼
- [ ] **GameManager** - 게임 매니저
- [ ] **SceneController** - 씬 컨트롤러
- [ ] **AudioManager** - 오디오 매니저
- [ ] **UIManager** - UI 매니저
- [ ] **EventSystem** - 이벤트 시스템

### 8.2 설정 확인
- [ ] **캔버스 스케일러** 올바르게 설정됨
- [ ] **버튼 이벤트** 모두 연결됨
- [ ] **BGM** 할당 및 재생 설정됨
- [ ] **씬 전환** 설정 확인됨

### 8.3 테스트 확인
- [ ] **게임 시작 버튼** 클릭 시 게임플레이 씬으로 이동
- [ ] **설정 버튼** 클릭 시 설정 UI 표시
- [ ] **종료 버튼** 클릭 시 게임 종료
- [ ] **BGM** 정상 재생됨
- [ ] **버튼 사운드** 정상 재생됨

---

## 🚨 9. 주의사항 및 팁

### 9.1 Build Settings 확인
- **File → Build Settings**에서 MainMenu 씬이 **Index 0**에 있는지 확인
- GameplayScene이 **Index 1**에 있는지 확인

### 9.2 DontDestroyOnLoad
- GameManager, AudioManager 등은 자동으로 DontDestroyOnLoad 적용됨
- 씬 전환 시에도 유지됨

### 9.3 성능 최적화
- **Canvas** → **Additional Shader Channels**: 필요한 것만 체크
- **Text** 컴포넌트보다 **TextMeshPro** 사용 권장
- 불필요한 **Raycast Target** 해제

### 9.4 해상도 대응
- **Canvas Scaler** 설정으로 다양한 해상도 대응
- **Anchor Presets** 활용으로 UI 요소 위치 고정

---

## 📞 10. 문제 해결

### 10.1 버튼이 작동하지 않는 경우
1. **EventSystem** 오브젝트 존재 확인
2. **Canvas** → **GraphicRaycaster** 컴포넌트 확인
3. **Button** → **Interactable** 체크 확인
4. **버튼 이벤트** 올바르게 연결되었는지 확인

### 10.2 UI가 올바르게 표시되지 않는 경우
1. **Canvas Scaler** 설정 확인
2. **Anchor Presets** 올바르게 설정되었는지 확인
3. **RectTransform** 값들 확인

### 10.3 오디오가 재생되지 않는 경우
1. **AudioManager** 볼륨 설정 확인
2. **Audio Listener** 씬에 존재하는지 확인 (보통 Camera에 있음)
3. **AudioClip** 올바르게 할당되었는지 확인

---

---

## 📈 11. 개선 사항 요약

### 🆕 **새로 추가된 내용**
✅ **최종 하이어라키 구조도** - 완성된 씬의 전체 구조 시각화  
✅ **컴포넌트 상세 설정** - 모든 중요 컴포넌트의 정확한 설정값  
✅ **YAML 형식 설정** - 복사 붙여넣기 가능한 설정 포맷  
✅ **필수 컴포넌트 명시** - 각 GameObject에 필요한 컴포넌트 리스트  

### 🎯 **이제 할 수 있는 것들**
- 🔧 **정확한 설정**: 모든 컴포넌트를 정확한 값으로 설정
- 🏗️ **구조 확인**: 하이어라키 구조를 보고 빠르게 검증
- 📋 **체크리스트**: 단계별 완성도 확인
- 🚀 **빠른 복구**: 설정 오류 시 빠른 수정 가능

### 💡 **활용 팁**
1. **설정 복사**: YAML 코드 블록을 참고하여 Inspector에서 설정
2. **구조 비교**: 현재 하이어라키와 가이드의 구조도 비교
3. **단계 건너뛰기**: 경험자는 구조도만 보고 빠르게 구성 가능
4. **문제 해결**: 오류 발생 시 해당 컴포넌트 설정 재확인

---

이제 메인메뉴 씬이 완성되었습니다! 

**다음 단계**: [게임플레이 씬 설정 가이드](02_Gameplay_Scene_Setup_Guide.md)  
**구조 확인**: [하이어라키 구조 참조](05_Hierarchy_Structure_Reference.md)