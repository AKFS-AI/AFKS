# 🏠 MainMenu.unity 하이어라키 완전 가이드

## 📁 **완전한 하이어라키 구조**

```
MainMenu (Scene)
├── 📷 Main Camera                    [Camera, AudioListener]
├── ☀️ Directional Light              [Light]
├── 🌫️ EventSystem                    [EventSystem, StandaloneInputModule]
│
├── 🎮 === CORE MANAGERS ===           [빈 GameObject - 구분용]
│   └── 📋 CoreManagers                [빈 GameObject - 컨테이너]
│       ├── GameManager                [GameManager.cs]
│       ├── SceneController            [SceneController.cs]
│       ├── AudioManager               [AudioManager.cs]
│       └── UIManager                  [UIManager.cs]
│
├── 🎨 === UI CANVAS ===               [빈 GameObject - 구분용]
│   └── 📱 MainMenuCanvas              [Canvas, CanvasScaler, GraphicRaycaster]
│       ├── 🖼️ BackgroundImage          [Image]
│       ├── 📋 MainPanel                [Image]
│       │   ├── 🎮 StartButton           [Button, Image]
│       │   │   └── Text                [Text]
│       │   ├── ⚙️ SettingsButton        [Button, Image]
│       │   │   └── Text                [Text]
│       │   ├── 📖 CreditsButton         [Button, Image]
│       │   │   └── Text                [Text]
│       │   └── 🚪 QuitButton            [Button, Image]
│       │       └── Text                [Text]
│       ├── 📋 SettingsPanel            [Image, CanvasGroup]
│       │   ├── 🎵 BGMSlider             [Slider]
│       │   ├── 🔊 SFXSlider             [Slider]
│       │   ├── 🖼️ FullscreenToggle      [Toggle]
│       │   └── ❌ CloseButton           [Button, Image]
│       │       └── Text                [Text]
│       └── 📋 CreditsPanel             [Image, CanvasGroup]
│           ├── 📝 CreditsText           [Text]
│           └── ❌ CloseButton           [Button, Image]
│               └── Text                [Text]
│
└── 🎵 === AUDIO SYSTEM ===            [빈 GameObject - 구분용]
    └── 🔊 MenuAudioSource             [AudioSource]
```

---

## 🛠️ **단계별 생성 가이드**

### **1단계: 기본 씬 준비**
```
File > New Scene > Basic (Built-in)
→ Main Camera, Directional Light, EventSystem 자동 생성됨
```

### **2단계: 구분용 빈 오브젝트 생성**
```
Hierarchy 우클릭 > Create Empty
이름: "🎮 === CORE MANAGERS ==="

Hierarchy 우클릭 > Create Empty  
이름: "🎨 === UI CANVAS ==="

Hierarchy 우클릭 > Create Empty
이름: "🎵 === AUDIO SYSTEM ==="
```

---

## 📋 **각 오브젝트별 상세 설정**

### **🎮 === CORE MANAGERS ===**

#### **📋 CoreManagers**
```
생성: === CORE MANAGERS === 우클릭 > Create Empty
이름: "📋 CoreManagers"
컴포넌트: Transform만 (빈 컨테이너)
```

#### **GameManager**
```
생성: CoreManagers 우클릭 > Create Empty
이름: "GameManager"
컴포넌트: 
├── Transform
└── GameManager (Script) [Add Component > AFKS.Core.GameManager]
```

#### **SceneController**
```
생성: CoreManagers 우클릭 > Create Empty
이름: "SceneController"  
컴포넌트:
├── Transform
└── SceneController (Script) [Add Component > AFKS.Core.SceneController]
```

#### **AudioManager**
```
생성: CoreManagers 우클릭 > Create Empty
이름: "AudioManager"
컴포넌트:
├── Transform  
└── AudioManager (Script) [Add Component > AFKS.AudioSystem.AudioManager]
```

#### **UIManager**
```
생성: CoreManagers 우클릭 > Create Empty
이름: "UIManager"
컴포넌트:
├── Transform
└── UIManager (Script) [Add Component > AFKS.UISystem.UIManager]
```

---

### **🎨 === UI CANVAS ===**

#### **📱 MainMenuCanvas**
```
생성: === UI CANVAS === 우클릭 > UI > Canvas
이름: "📱 MainMenuCanvas"
컴포넌트:
├── RectTransform
├── Canvas
│   ├── Render Mode: Screen Space - Overlay
│   ├── Pixel Perfect: ✓
│   └── Sort Order: 0
├── CanvasScaler
│   ├── UI Scale Mode: Scale With Screen Size  
│   ├── Reference Resolution: 1920 x 1080
│   ├── Screen Match Mode: Match Width Or Height
│   └── Match: 0.5
└── GraphicRaycaster
    └── Ignore Reversed Graphics: ✓
```

#### **🖼️ BackgroundImage**
```
생성: MainMenuCanvas 우클릭 > UI > Image
이름: "🖼️ BackgroundImage"
컴포넌트:
├── RectTransform
│   ├── Anchor Presets: Stretch (전체 화면)
│   ├── Left: 0, Top: 0, Right: 0, Bottom: 0
│   └── Pivot: 0.5, 0.5
└── Image
    ├── Source Image: None (또는 배경 이미지)
    ├── Color: (0.05, 0.05, 0.05, 1) # 어두운 배경
    └── Raycast Target: ✗ (클릭 차단 방지)
```

#### **📋 MainPanel**
```
생성: MainMenuCanvas 우클릭 > UI > Panel
이름: "📋 MainPanel"
컴포넌트:
├── RectTransform
│   ├── Anchor Presets: Middle Center
│   ├── Pos X: 0, Pos Y: 0
│   ├── Width: 400, Height: 600
│   └── Pivot: 0.5, 0.5
└── Image
    ├── Source Image: None
    ├── Color: (0, 0, 0, 0.8) # 반투명 검정
    └── Raycast Target: ✓
```

#### **🎮 StartButton**
```
생성: MainPanel 우클릭 > UI > Button  
이름: "🎮 StartButton"
컴포넌트:
├── RectTransform
│   ├── Anchor Presets: Middle Center
│   ├── Pos X: 0, Pos Y: 100
│   ├── Width: 300, Height: 60
│   └── Pivot: 0.5, 0.5
├── Image
│   ├── Source Image: UI Sprite (기본)
│   ├── Color: (1, 1, 1, 1)
│   └── Image Type: Sliced
└── Button
    ├── Interactable: ✓
    ├── Transition: Color Tint
    ├── Target Graphic: 자기 자신 (Image)
    ├── Normal Color: (1, 1, 1, 1)
    ├── Highlighted Color: (0.96, 0.96, 0.96, 1)
    ├── Pressed Color: (0.78, 0.78, 0.78, 1)
    └── Selected Color: (0.96, 0.96, 0.96, 1)

하위 Text 오브젝트:
├── RectTransform (Stretch 전체)
└── Text
    ├── Text: "게임 시작"
    ├── Font: Arial
    ├── Font Size: 24
    ├── Color: (0.2, 0.2, 0.2, 1) # 어두운 글자
    ├── Alignment: Middle Center
    └── Best Fit: ✓
```

#### **⚙️ SettingsButton**
```
생성: MainPanel 우클릭 > UI > Button
이름: "⚙️ SettingsButton" 
설정: StartButton과 동일하되
├── Pos Y: 0
└── Text: "설정"
```

#### **📖 CreditsButton**
```
생성: MainPanel 우클릭 > UI > Button
이름: "📖 CreditsButton"
설정: StartButton과 동일하되  
├── Pos Y: -100
└── Text: "크레딧"
```

#### **🚪 QuitButton**
```
생성: MainPanel 우클릭 > UI > Button
이름: "🚪 QuitButton"
설정: StartButton과 동일하되
├── Pos Y: -200  
└── Text: "게임 종료"
```

#### **📋 SettingsPanel**
```
생성: MainMenuCanvas 우클릭 > UI > Panel
이름: "📋 SettingsPanel"
컴포넌트:
├── RectTransform
│   ├── Anchor Presets: Middle Center
│   ├── Width: 600, Height: 400
│   └── Active: False (초기 비활성화)
├── Image (반투명 어두운 배경)
└── CanvasGroup
    ├── Alpha: 1
    ├── Interactable: ✓
    └── Blocks Raycasts: ✓
```

#### **📋 CreditsPanel**
```
생성: MainMenuCanvas 우클릭 > UI > Panel  
이름: "📋 CreditsPanel"
설정: SettingsPanel과 동일
```

---

### **🎵 === AUDIO SYSTEM ===**

#### **🔊 MenuAudioSource**
```
생성: === AUDIO SYSTEM === 우클릭 > Audio > Audio Source
이름: "🔊 MenuAudioSource"
컴포넌트:
├── Transform
└── AudioSource
    ├── AudioClip: None (나중에 BGM 할당)
    ├── Output: AudioMixerGroup (None)
    ├── Mute: ✗
    ├── Bypass Effects: ✗
    ├── Bypass Listener Effects: ✗
    ├── Bypass Reverb Zones: ✗
    ├── Play On Awake: ✓
    ├── Loop: ✓
    ├── Priority: 128
    ├── Volume: 0.5
    ├── Pitch: 1
    ├── Stereo Pan: 0
    ├── Spatial Blend: 0 (2D)
    └── Reverb Zone Mix: 1
```

---

## ✅ **생성 완료 체크리스트**

### **기본 오브젝트**
- [ ] Main Camera (자동 생성됨)
- [ ] Directional Light (자동 생성됨)  
- [ ] EventSystem (자동 생성됨)

### **Core Managers**
- [ ] GameManager + GameManager.cs
- [ ] SceneController + SceneController.cs
- [ ] AudioManager + AudioManager.cs  
- [ ] UIManager + UIManager.cs

### **UI Canvas**
- [ ] MainMenuCanvas (Canvas + CanvasScaler)
- [ ] BackgroundImage (전체 화면)
- [ ] MainPanel (중앙 패널)
- [ ] 4개 버튼 (Start, Settings, Credits, Quit)
- [ ] SettingsPanel (비활성화)
- [ ] CreditsPanel (비활성화)

### **Audio System**  
- [ ] MenuAudioSource

---

## 🎯 **다음 단계**

MainMenu 하이어라키 완성 후:
1. **스크립트 연결 확인** → Inspector에서 스크립트 컴포넌트 확인
2. **UI 이벤트 연결** → 버튼 클릭 이벤트 설정
3. **씬 저장** → File > Save As > MainMenu.unity
4. **Game 씬 구성** → 동일한 방식으로 Game.unity 생성

**지금 이 구조대로 만들어보시겠습니까?** 🚀