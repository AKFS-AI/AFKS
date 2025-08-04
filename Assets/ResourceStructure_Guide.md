# 🗂️ "수수께끼 괴담" 리소스 구조 완전 가이드

## 📁 **최적화된 전체 폴더 구조**

```
Assets/
├── 📜 Scripts/                     # 스크립트 (이미 생성됨)
│   ├── Core/
│   ├── StageSystem/
│   ├── InteractionSystem/
│   ├── InventorySystem/
│   ├── HorrorSystem/
│   ├── AudioSystem/
│   ├── UISystem/
│   └── Shared/
│
├── 🎨 Art/                         # 아트 리소스
│   ├── Textures/
│   │   ├── Backgrounds/            # 병원 스테이지 배경 (6개)
│   │   │   ├── Stage01_HospitalFront_1920x1080.png
│   │   │   ├── Stage02_HospitalLobby_1920x1080.png
│   │   │   ├── Stage03_ManagementRoom_1920x1080.png
│   │   │   ├── Stage04_DeliveryRoom_1920x1080.png
│   │   │   ├── Stage05_NurseryRoom_1920x1080.png
│   │   │   └── Stage06_PrayerRoom_1920x1080.png
│   │   │
│   │   ├── Items/                  # 아이템 이미지 (10개)
│   │   │   ├── Item_Cross_Icon.png
│   │   │   ├── Item_Cross_Detail.png
│   │   │   ├── Item_Permit_Icon.png
│   │   │   ├── Item_Permit_Detail.png
│   │   │   ├── Item_Letter_Icon.png
│   │   │   ├── Item_Letter_Detail.png
│   │   │   ├── Item_Flashlight_Icon.png
│   │   │   ├── Item_Flashlight_Detail.png
│   │   │   ├── Item_Keys_Icon.png
│   │   │   ├── Item_Keys_Detail.png
│   │   │   ├── Item_Report_Icon.png
│   │   │   ├── Item_Report_Detail.png
│   │   │   ├── Item_Blueprint_Icon.png
│   │   │   ├── Item_Blueprint_Detail.png
│   │   │   ├── Item_UltrasoundPhoto_Icon.png
│   │   │   ├── Item_UltrasoundPhoto_Detail.png
│   │   │   ├── Item_BloodTest_Icon.png
│   │   │   ├── Item_BloodTest_Detail.png
│   │   │   ├── Item_ConfessionNote_Icon.png
│   │   │   └── Item_ConfessionNote_Detail.png
│   │   │
│   │   ├── Horror/                 # 공포 연출 이미지 (5개)
│   │   │   ├── Horror_Lobby_GhostAppear.png
│   │   │   ├── Horror_Management_CCTVGhost.png
│   │   │   ├── Horror_Delivery_BodyFall.png
│   │   │   ├── Horror_Nursery_BabyGhost.png
│   │   │   └── Horror_Prayer_HospitalDirector.png
│   │   │
│   │   ├── UI/                     # UI 이미지
│   │   │   ├── Buttons/
│   │   │   ├── Icons/
│   │   │   ├── Panels/
│   │   │   └── Cursors/
│   │   │
│   │   └── Effects/                # 이펙트 텍스처
│   │       ├── Blood/
│   │       ├── Distortion/
│   │       └── Particles/
│   │
│   ├── Sprites/                    # 스프라이트 시트
│   └── Fonts/                      # 폰트 리소스
│
├── 🎵 Audio/                       # 오디오 리소스
│   ├── Music/
│   │   ├── BGM/
│   │   │   ├── BGM_Hospital_Ambient.ogg
│   │   │   └── BGM_Horror_Theme.ogg
│   │   └── Ambient/
│   │       ├── AMB_Hospital_Night.ogg
│   │       └── AMB_Wind_Howling.ogg
│   │
│   ├── SFX/
│   │   ├── UI/
│   │   │   ├── SFX_UI_Click.wav
│   │   │   ├── SFX_UI_Hover.wav
│   │   │   └── SFX_UI_Select.wav
│   │   │
│   │   ├── Interaction/
│   │   │   ├── SFX_Door_Open.wav
│   │   │   ├── SFX_Item_Pickup.wav
│   │   │   ├── SFX_Keys_Jingle.wav
│   │   │   └── SFX_Flashlight_Click.wav
│   │   │
│   │   ├── Horror/
│   │   │   ├── SFX_Horror_Scream.wav
│   │   │   ├── SFX_Horror_Whisper.wav
│   │   │   ├── SFX_Horror_HeartBeat.wav
│   │   │   ├── SFX_Horror_Static.wav
│   │   │   └── SFX_Horror_BabyCry.wav
│   │   │
│   │   └── Environment/
│   │       ├── SFX_Footsteps.wav
│   │       ├── SFX_Door_Creak.wav
│   │       └── SFX_Wind.wav
│   │
│   ├── Voice/
│   │   └── Narration/
│   └── AudioMixers/
│       ├── Master.mixer
│       ├── BGM.mixer
│       ├── SFX.mixer
│       └── Horror.mixer
│
├── 📊 Data/                        # 데이터 리소스
│   ├── ScriptableObjects/
│   │   ├── Stages/
│   │   │   ├── Stage01_HospitalFront.asset
│   │   │   ├── Stage02_HospitalLobby.asset
│   │   │   ├── Stage03_ManagementRoom.asset
│   │   │   ├── Stage04_DeliveryRoom.asset
│   │   │   ├── Stage05_NurseryRoom.asset
│   │   │   └── Stage06_PrayerRoom.asset
│   │   │
│   │   ├── Items/
│   │   │   ├── Item_Cross.asset
│   │   │   ├── Item_Permit.asset
│   │   │   ├── Item_Letter.asset
│   │   │   ├── Item_Flashlight.asset
│   │   │   ├── Item_Keys.asset
│   │   │   ├── Item_Report.asset
│   │   │   ├── Item_Blueprint.asset
│   │   │   ├── Item_UltrasoundPhoto.asset
│   │   │   ├── Item_BloodTest.asset
│   │   │   └── Item_ConfessionNote.asset
│   │   │
│   │   ├── Horror/
│   │   │   ├── Horror_LobbyGhost.asset
│   │   │   ├── Horror_CCTVGhost.asset
│   │   │   ├── Horror_BodyFall.asset
│   │   │   ├── Horror_BabyGhost.asset
│   │   │   └── Horror_HospitalDirector.asset
│   │   │
│   │   ├── Audio/
│   │   │   └── AudioConfig.asset
│   │   │
│   │   └── Config/
│   │       ├── GameConfig.asset
│   │       └── StageConfig.asset
│   │
│   ├── JSON/                       # JSON 데이터 (필요시)
│   ├── Localization/               # 다국어 데이터
│   └── Settings/                   # 에디터 설정
│
├── 🧩 Prefabs/                     # 프리팹
│   ├── Core/
│   │   ├── Managers/
│   │   │   ├── GameManager.prefab
│   │   │   ├── StageManager.prefab
│   │   │   ├── AudioManager.prefab
│   │   │   ├── UIManager.prefab
│   │   │   ├── InventoryManager.prefab
│   │   │   ├── InteractionManager.prefab
│   │   │   └── HorrorEventManager.prefab
│   │   └── Systems/
│   │
│   ├── UI/
│   │   ├── Panels/
│   │   │   ├── MainCanvas.prefab
│   │   │   ├── InventoryPanel.prefab
│   │   │   ├── DialogPanel.prefab
│   │   │   ├── MenuPanel.prefab
│   │   │   ├── SettingsPanel.prefab
│   │   │   └── LoadingPanel.prefab
│   │   │
│   │   ├── Components/
│   │   │   ├── InventorySlot.prefab
│   │   │   ├── ItemIcon.prefab
│   │   │   └── Button_Default.prefab
│   │   │
│   │   └── Effects/
│   │       ├── FadeOverlay.prefab
│   │       └── TransitionEffect.prefab
│   │
│   ├── Gameplay/
│   │   ├── Interactions/
│   │   │   ├── InteractionPoint.prefab
│   │   │   ├── ItemPickup.prefab
│   │   │   └── HotspotArea.prefab
│   │   │
│   │   ├── Items/
│   │   │   ├── Cross.prefab
│   │   │   ├── Flashlight.prefab
│   │   │   └── Keys.prefab
│   │   │
│   │   └── Stages/
│   │       ├── StageBackground.prefab
│   │       └── StageCanvas.prefab
│   │
│   ├── Horror/
│   │   ├── Effects/
│   │   │   ├── JumpscareEffect.prefab
│   │   │   ├── ScreenShake.prefab
│   │   │   ├── HorrorOverlay.prefab
│   │   │   └── GlitchEffect.prefab
│   │   │
│   │   └── Events/
│   │       ├── GhostAppearance.prefab
│   │       ├── BodyFall.prefab
│   │       └── StaticNoise.prefab
│   │
│   └── Audio/
│       ├── Sources/
│       │   ├── BGMSource.prefab
│       │   ├── SFXSource.prefab
│       │   └── AmbientSource.prefab
│       └── Mixers/
│
├── 🎨 Materials/                   # 머티리얼
│   ├── UI/
│   ├── Sprites/
│   ├── Effects/
│   └── PostProcess/
│
├── 🎬 Animation/                   # 애니메이션
│   ├── UI/
│   ├── Horror/
│   ├── Items/
│   ├── Stages/
│   └── Controllers/
│
├── 📦 StreamingAssets/             # 스트리밍 에셋
│   ├── Config/
│   ├── Video/
│   └── Database/
│
├── 📚 Resources/                   # 리소스 (최소 사용)
│   ├── DefaultSettings/
│   └── Fallback/
│
├── 📖 Documentation/               # 문서
│   ├── API/
│   ├── Design/
│   ├── Tutorials/
│   └── References/
│
├── ⚙️ Settings/                    # Unity 설정 (기존)
└── 🎮 Scenes/                      # 씬 파일 (기존)
    ├── Main.unity
    ├── Menu.unity
    └── Loading.unity
```

---

## 🎯 **우리 게임에 특화된 리소스 계획**

### **📋 필요한 리소스 목록**

#### **🎨 아트 리소스 (총 21개)**
```
✅ 배경 이미지: 6개 (각 스테이지별)
✅ 아이템 아이콘: 10개 (Icon + Detail)
✅ 공포 이미지: 5개 (점프스케어)
```

#### **🎵 오디오 리소스 (총 15개)**
```
✅ BGM: 2개 (병원 앰비언트, 공포 테마)
✅ 효과음: 10개 (상호작용, 공포, 환경)
✅ 앰비언트: 3개 (병원 분위기)
```

#### **📊 데이터 리소스 (총 22개)**
```
✅ 스테이지 데이터: 6개
✅ 아이템 데이터: 10개
✅ 공포 이벤트 데이터: 5개
✅ 설정 파일: 1개
```

---

## 🚀 **리소스 생성 순서 (권장)**

### **Phase 1: 핵심 리소스 (1-2일)**
```
1. 📊 ScriptableObject 생성
   - GameConfig.asset
   - 6개 StageData.asset (기본 설정)
   - 10개 ItemData.asset (기본 설정)

2. 🎵 기본 오디오
   - BGM_Hospital_Ambient.ogg
   - SFX_UI_Click.wav
   - SFX_Item_Pickup.wav

3. 🧩 핵심 프리팹
   - 모든 Manager 프리팹
   - UI 기본 프리팹
```

### **Phase 2: 아트 리소스 (2-3일)**
```
1. 🎨 배경 이미지 6개
   - 흑백 스타일, 1920x1080
   - 병원 분위기에 맞는 톤

2. 🎒 아이템 이미지 20개
   - 아이콘: 512x512
   - 상세보기: 1024x1024

3. 😱 공포 이미지 5개
   - 점프스케어용, 1024x1024
   - 무서운 분위기 연출
```

### **Phase 3: 완성도 향상 (1일)**
```
1. 🎵 모든 오디오 리소스
2. 🎬 애니메이션 효과
3. 🎨 UI 머티리얼
4. 📋 최종 데이터 설정
```

---

## 📊 **메모리 사용량 예측**

### **아트 리소스**
```
배경 이미지 (6개): 18MB (압축 후)
아이템 이미지 (20개): 8MB (압축 후)
공포 이미지 (5개): 5MB (압축 후)
UI 이미지: 2MB
총 아트 메모리: ~33MB
```

### **오디오 리소스**
```
BGM (스트리밍): 2MB
효과음 (메모리 로드): 5MB
앰비언트 (스트리밍): 3MB
총 오디오 메모리: ~10MB
```

### **전체 예상 사용량**
```
🎯 총 메모리 사용량: ~50MB
🎯 빌드 크기: ~80MB (압축 포함)
🎯 로딩 시간: <3초 (첫 실행)
```

---

## 🛠️ **리소스 최적화 가이드**

### **텍스처 설정**
```csharp
// 배경 이미지
Max Size: 2048
Format: RGB Compressed DXT1 (PC) / ASTC 4x4 (Mobile)
Generate Mip Maps: false

// 아이템 아이콘
Max Size: 512
Format: RGBA Compressed DXT5 (PC) / ASTC 4x4 (Mobile)
Generate Mip Maps: false

// 공포 이미지
Max Size: 1024
Format: RGB Compressed DXT1
Generate Mip Maps: false
```

### **오디오 설정**
```csharp
// BGM
Load Type: Streaming
Compression Format: Vorbis
Quality: 70%

// 효과음
Load Type: Decompress On Load
Compression Format: PCM (짧은 소리) / Vorbis (긴 소리)
```

---

## 🎉 **완성된 리소스 구조의 장점**

### **1. 성능 최적화** ⭐
- ScriptableObject 기반으로 빠른 로딩
- 메모리 효율적 관리
- 플랫폼별 자동 최적화

### **2. 개발 효율성** ⭐
- 명확한 폴더 구조
- 일관된 네이밍 규칙
- 쉬운 리소스 찾기

### **3. 확장성** ⭐
- 새 스테이지 쉽게 추가
- 모듈별 독립적 관리
- 팀 작업 충돌 최소화

### **4. 유지보수성** ⭐
- 체계적인 분류
- 문서화된 가이드
- 버전 관리 친화적

---

**이제 이 구조에 따라 리소스를 생성하고 배치하면 완벽한 "수수께끼 괴담" 게임이 완성됩니다!** 🎮👻

어떤 리소스부터 시작하시겠습니까? 🚀