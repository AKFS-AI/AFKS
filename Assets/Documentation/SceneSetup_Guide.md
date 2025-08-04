# 🎬 씬 구조 설정 완전 가이드

## 📋 **1단계: 씬 파일 생성**

### ✅ **MainMenu.unity 생성**
```
1. Unity 메뉴 > File > New Scene
2. File > Save As > "MainMenu"
3. 위치: Assets/Scenes/MainMenu.unity
```

### ✅ **Game.unity 생성**  
```
1. Unity 메뉴 > File > New Scene
2. File > Save As > "Game"  
3. 위치: Assets/Scenes/Game.unity
```

---

## 🏗️ **2단계: MainMenu.unity 하이어라키 구성**

### **📁 CoreManagers 생성**
```
1. Hierarchy에서 우클릭 > Create Empty
2. 이름: "CoreManagers"
3. 하위에 빈 GameObject 4개 생성:
   - GameManager
   - SceneController  
   - AudioManager
   - UIManager
```

### **📱 UI Canvas 생성**
```
1. Hierarchy에서 우클릭 > UI > Canvas
2. 이름: "MainMenuCanvas"
3. Canvas 설정:
   - Render Mode: Screen Space - Overlay
   - Canvas Scaler: Scale With Screen Size
   - Reference Resolution: 1920x1080
```

### **🖼️ 배경 및 버튼 생성**
```
MainMenuCanvas 하위:
1. UI > Image (이름: BackgroundImage)
2. UI > Panel (이름: MainPanel)
   MainPanel 하위:
   - UI > Button (이름: StartButton)
   - UI > Button (이름: SettingsButton)  
   - UI > Button (이름: CreditsButton)
   - UI > Button (이름: QuitButton)
```

---

## 🎮 **3단계: Game.unity 하이어라키 구성**

### **📁 SystemManagers 생성**
```
1. Hierarchy에서 우클릭 > Create Empty
2. 이름: "SystemManagers"
3. 하위에 빈 GameObject 4개 생성:
   - StageManager
   - InteractionManager
   - InventoryManager  
   - HorrorEventManager
```

### **📱 StageCanvas 생성**
```
1. Hierarchy에서 우클릭 > UI > Canvas
2. 이름: "StageCanvas"
3. Canvas 설정:
   - Render Mode: Screen Space - Overlay
   - Sort Order: 0
```

### **🖼️ 스테이지 CanvasGroup 생성**
```
StageCanvas 하위에 6개 CanvasGroup 생성:
1. Stage01_HospitalFront
2. Stage02_HospitalLobby
3. Stage03_ManagementRoom
4. Stage04_DeliveryRoom
5. Stage05_NurseryRoom
6. Stage06_PrayerRoom

각 CanvasGroup 하위:
- UI > Image (이름: BackgroundImage)
- Create Empty (이름: InteractionPoints)
```

### **📱 UICanvas 생성**
```
1. Hierarchy에서 우클릭 > UI > Canvas
2. 이름: "UICanvas"  
3. Canvas 설정:
   - Render Mode: Screen Space - Overlay
   - Sort Order: 100
   
UICanvas 하위:
- UI > Panel (이름: InventoryPanel)
- UI > Panel (이름: DialogPanel)
- UI > Panel (이름: PausePanel)
- UI > Image (이름: FadeOverlay)
```

---

## 🎵 **4단계: 오디오 시스템 구성**

### **🔊 AudioSources 생성**
```
1. Hierarchy에서 우클릭 > Create Empty
2. 이름: "GameAudioSources"
3. 하위에 AudioSource 4개 생성:
   - BGMSource
   - SFXSource
   - AmbientSource
   - HorrorSource
```

---

## 😱 **5단계: 공포 효과 시스템**

### **🎭 HorrorEffects Canvas 생성**
```
1. Hierarchy에서 우클릭 > UI > Canvas
2. 이름: "HorrorEffects"
3. Canvas 설정:
   - Render Mode: Screen Space - Overlay
   - Sort Order: 200
   
HorrorEffects 하위:
- UI > Image (이름: JumpscareOverlay)
- UI > Image (이름: StaticOverlay)  
- UI > Image (이름: BloodOverlay)
```

---

## 🔧 **6단계: 스크립트 연결**

### **MainMenu.unity**
```
1. CoreManagers의 각 자식에 해당 스크립트 추가:
   - GameManager에 GameManager.cs
   - SceneController에 SceneController.cs
   - AudioManager에 AudioManager.cs
   - UIManager에 UIManager.cs

2. MainMenuCanvas에 MainMenuSetup.cs 추가
3. Inspector에서 필요한 참조들 할당
```

### **Game.unity**  
```
1. SystemManagers의 각 자식에 해당 스크립트 추가:
   - StageManager에 StageManager.cs
   - InteractionManager에 InteractionManager.cs
   - InventoryManager에 InventoryManager.cs
   - HorrorEventManager에 HorrorEventManager.cs

2. StageCanvas에 GameSceneSetup.cs 추가
3. Inspector에서 StageData 배열 할당
```

---

## ⚙️ **7단계: 프로젝트 설정**

### **Build Settings 구성**
```
1. File > Build Settings 열기
2. Scenes To Build에 추가:
   - [0] MainMenu
   - [1] Game
3. Player Settings 확인:
   - Company Name
   - Product Name  
   - Version
```

### **Input System 설정**
```
1. 이미 InputSystem_Actions.inputactions 존재 확인
2. UI EventSystem이 각 씬에 있는지 확인
```

---

## 🎯 **8단계: 테스트 및 검증**

### **MainMenu 테스트**
```
1. MainMenu.unity를 열고 Play
2. 버튼 클릭 테스트:
   - Start 버튼 → Game 씬 로드
   - Quit 버튼 → 게임 종료
3. Console에서 에러 메시지 확인
```

### **Game 테스트**  
```
1. Game.unity를 열고 Play
2. 매니저들이 제대로 생성되는지 확인
3. 스테이지 전환 테스트 (키보드 단축키로)
4. 인벤토리 UI 표시 확인
```

---

## 📊 **완성도 체크리스트**

### ✅ **MainMenu.unity**
- [ ] CoreManagers 구조 완성
- [ ] UI Canvas 및 버튼 구성
- [ ] 스크립트 연결 완료
- [ ] 버튼 이벤트 작동 확인

### ✅ **Game.unity**
- [ ] SystemManagers 구조 완성  
- [ ] 6개 스테이지 CanvasGroup 생성
- [ ] UI Overlay 시스템 구성
- [ ] 오디오 시스템 구성
- [ ] 공포 효과 시스템 구성
- [ ] 스크립트 연결 완료

### ✅ **프로젝트 설정**
- [ ] Build Settings 구성
- [ ] 씬 전환 테스트 성공
- [ ] 모든 매니저 정상 작동 확인

---

## 🚀 **다음 단계 예고**

1. **리소스 배치**: 배경 이미지, 아이템 아이콘 등
2. **데이터 설정**: StageData.asset, ItemData.asset 생성
3. **상호작용 구현**: 클릭 포인트 및 이벤트 연결
4. **공포 이벤트 설정**: 점프스케어 타이밍 및 효과
5. **최종 테스트**: 전체 게임 플로우 검증

**이 가이드를 따르면 완전한 씬 구조가 완성됩니다!** 🎉