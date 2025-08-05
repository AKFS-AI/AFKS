# 🚀 AFKS 게임 개발 빠른 시작 가이드

## 🎯 개요
이 문서는 AFKS (A Fear Keeps Silence) 호러 어드벤처 게임을 Unity에서 처음부터 개발하는 방법을 안내합니다.

---

## 📚 문서 구조

### 📖 가이드 순서
1. **[00_Quick_Start_Guide.md](00_Quick_Start_Guide.md)** ← 현재 문서
2. **[01_MainMenu_Scene_Setup_Guide.md](01_MainMenu_Scene_Setup_Guide.md)** - 메인메뉴 씬 제작
3. **[02_Gameplay_Scene_Setup_Guide.md](02_Gameplay_Scene_Setup_Guide.md)** - 게임플레이 씬 제작  
4. **[03_Stage1_Creation_Guide.md](03_Stage1_Creation_Guide.md)** - 스테이지 1 제작
5. **[04_Interaction_System_Setup_Guide.md](04_Interaction_System_Setup_Guide.md)** - 상호작용 시스템 설정
6. **[05_Hierarchy_Structure_Reference.md](05_Hierarchy_Structure_Reference.md)** - 하이어라키 구조 참조

### 📋 권장 개발 순서
```
1. 환경 설정 및 프로젝트 준비
    ↓
2. 메인메뉴 씬 제작 (기본 UI, 씬 전환)
    ↓
3. 게임플레이 씬 제작 (매니저 시스템, 기본 구조)
    ↓
4. 스테이지 1 제작 (첫 번째 게임플레이 단계)
    ↓
5. 고급 상호작용 시스템 구현 (줌, 2단계 상호작용)
    ↓
6. 추가 스테이지 제작 및 게임 완성
```

---

## ⚙️ 1. 환경 설정

### 1.1 Unity 버전 요구사항
- **Unity 2023.2 이상** 권장
- **TextMeshPro** 패키지 설치 필수
- **2D 프로젝트 템플릿** 사용

### 1.2 필수 패키지
```
Window → Package Manager에서 설치:
- TextMeshPro (UI 텍스트)
- 2D Sprite (2D 이미지 처리)
- Audio (오디오 시스템)
- Input System (선택사항, Legacy Input 사용 가능)
```

### 1.3 프로젝트 폴더 구조
```
Assets/
├── Scenes/
│   ├── MainMenu.unity
│   ├── GameplayScene.unity
│   └── (추가 스테이지 씬들)
├── Scripts/
│   ├── Core/                    # 핵심 시스템 (GameManager, SceneController)
│   ├── AudioSystem/             # 오디오 관리
│   ├── StageSystem/             # 스테이지 관리 및 상호작용
│   ├── UISystem/                # UI 관리
│   ├── ItemSystem/              # 아이템 관리
│   ├── InteractionSystem/       # 상호작용 관리
│   ├── HorrorSystem/           # 호러 이벤트 관리
│   └── Shared/                 # 공통 유틸리티
├── Sprites/
│   ├── Backgrounds/            # 배경 이미지
│   ├── UI/                     # UI 요소
│   ├── Characters/             # 캐릭터 (필요 시)
│   └── Effects/                # 특수 효과
├── Audio/
│   ├── BGM/                    # 배경음악
│   ├── SFX/                    # 효과음
│   └── Ambient/                # 환경음
├── Data/
│   ├── Stages/                 # 스테이지 데이터
│   ├── Interactions/           # 상호작용 데이터
│   └── Horror/                 # 호러 이벤트 데이터
└── Prefabs/
    ├── UI/                     # UI 프리팹
    ├── Managers/               # 매니저 프리팹
    └── Interactions/           # 상호작용 프리팹
```

---

## 🎮 2. 핵심 시스템 소개

### 2.1 싱글톤 매니저 시스템
게임의 핵심 기능들은 싱글톤 패턴으로 관리됩니다:

```csharp
// BaseSingleton<T>을 상속받아 자동으로 싱글톤 구현
public class GameManager : BaseSingleton<GameManager>
{
    // 자동으로 Instance 프로퍼티 생성됨
    // DontDestroyOnLoad 자동 적용
}
```

**주요 매니저들:**
- **GameManager**: 게임 전체 상태 관리
- **SceneController**: 씬 전환 관리
- **StageManager**: 스테이지 시스템 관리
- **AudioManager**: BGM/SFX 관리
- **UIManager**: UI 시스템 관리
- **InteractionManager**: 상호작용 이벤트 관리
- **HorrorEventManager**: 호러 이벤트 관리
- **ItemManager**: 아이템 시스템 관리

### 2.2 이벤트 시스템
시스템 간 통신을 위한 GameEvent 시스템:

```csharp
// 이벤트 정의
public static readonly GameEvent<int> OnStageChanged = new GameEvent<int>();

// 이벤트 발생
OnStageChanged.Raise(newStageIndex);

// 이벤트 구독
OnStageChanged.AddListener(HandleStageChanged);
```

### 2.3 상호작용 시스템
3가지 상호작용 방식 지원:

1. **기본 상호작용 (StageInteractionController)**
   - 단순 클릭으로 아이템 획득, 메시지 표시, 스테이지 전환

2. **2단계 상호작용 (TwoStageInteractionController)**
   - 1단계: 객체 클릭 → 확대
   - 2단계: 확대된 상태에서 세부 상호작용

3. **픽셀 퍼펙트 상호작용 (PixelInteractionController)**
   - 이미지의 투명하지 않은 부분만 클릭 가능
   - 정밀한 상호작용 영역 설정

### 2.4 줌 시스템 (ZoomController)
이미지 확대/축소 기능:
- 부드러운 애니메이션
- 배경 어둡게 처리
- ESC 키 또는 배경 클릭으로 해제

---

## 🏗️ 3. 빠른 구현 로드맵

### 3.1 1단계: 기본 씬 구성 (30분)
1. **메인메뉴 씬** 생성
   - Canvas 설정
   - 게임 시작/설정/종료 버튼
   - 기본 매니저들 배치

2. **게임플레이 씬** 생성
   - 모든 매니저 시스템 설정
   - 기본 UI 구성

### 3.2 2단계: 스테이지 0 구현 (45분)
1. **병원 외부 배경** 설정
2. **철문 2단계 상호작용** 구현
   - 철문 클릭 → 확대
   - 쇠사슬 5번 클릭 → 스테이지 전환

### 3.3 3단계: 스테이지 1 구현 (60분)
1. **병원 로비 배경** 설정
2. **4개 상호작용 포인트** 구현
   - 리셉션 데스크 (열쇠)
   - 의료 캐비닛 (손전등)
   - CCTV 모니터 (호러 이벤트)
   - 엘리베이터 (다음 스테이지)

### 3.4 4단계: 호러 시스템 (30분)
1. **호러 이벤트** 설정
2. **환경 분위기** 조성
3. **오디오 시스템** 완성

---

## 🎯 4. 핵심 기능 구현 체크리스트

### 4.1 필수 기능
- [ ] **씬 전환** (메인메뉴 ↔ 게임플레이)
- [ ] **스테이지 시스템** (순차적 진행)
- [ ] **아이템 시스템** (열쇠, 손전등)
- [ ] **기본 상호작용** (클릭으로 상호작용)
- [ ] **오디오 시스템** (BGM, 효과음)

### 4.2 고급 기능
- [ ] **2단계 상호작용** (확대 → 세부 클릭)
- [ ] **줌 시스템** (부드러운 확대/축소)
- [ ] **호러 이벤트** (점프스케어, 환경음)
- [ ] **픽셀 퍼펙트 클릭** (정밀한 상호작용)
- [ ] **UI 애니메이션** (전환 효과)

### 4.3 선택적 기능
- [ ] **세이브/로드** 시스템
- [ ] **설정 메뉴** (볼륨, 그래픽)
- [ ] **성취도 시스템**
- [ ] **다국어 지원**

---

## 🎨 5. 아트 에셋 가이드라인

### 5.1 화면 해상도
- **기준 해상도**: 1920 x 1080
- **UI 스케일링**: Canvas Scaler 사용
- **다양한 화면비** 대응

### 5.2 이미지 사양
```
배경 이미지:
- 해상도: 1920 x 1080 이상
- 포맷: PNG (투명도 필요 시), JPG (배경용)
- 압축: High Quality

상호작용 이미지:
- 해상도: 512 x 512 이하
- 포맷: PNG (투명도 중요)
- 압축: True Color

UI 요소:
- 해상도: 적절한 크기 (64x64, 128x128 등)
- 포맷: PNG
- 9-slice 적용 (버튼, 패널 등)
```

### 5.3 오디오 사양
```
BGM:
- 포맷: OGG Vorbis
- 품질: Medium-High (압축률 중요)
- 길이: 2-5분 (루프 가능)

SFX:
- 포맷: WAV (짧은 효과음), OGG (긴 효과음)
- 품질: High (사용자 경험 중요)
- 길이: 0.1-3초

환경음:
- 포맷: OGG Vorbis
- 품질: Medium
- 길이: 30초-2분 (루프)
```

---

## 🧪 6. 테스트 가이드

### 6.1 기능 테스트 순서
1. **메인메뉴**: 모든 버튼 작동 확인
2. **씬 전환**: 로딩 없이 부드럽게 전환
3. **스테이지 0**: 철문 상호작용 완전히 작동
4. **스테이지 1**: 모든 상호작용 포인트 작동
5. **오디오**: BGM/SFX 모두 재생됨
6. **UI**: 인벤토리, 메시지 등 정상 표시

### 6.2 사용자 경험 테스트
- **직관성**: 플레이어가 무엇을 해야 하는지 명확한가?
- **반응성**: 클릭 시 즉각적인 피드백이 있는가?
- **일관성**: UI와 상호작용이 일관된 패턴을 따르는가?
- **접근성**: 모든 기능을 쉽게 찾을 수 있는가?

---

## 📞 7. 문제 해결 빠른 참조

### 7.1 일반적인 문제들

**Q: 버튼이 클릭되지 않아요**
A: EventSystem 존재 확인, Canvas에 GraphicRaycaster 확인, Button의 Raycast Target 확인

**Q: 씬 전환이 안 돼요**
A: Build Settings에 씬 추가 확인, SceneController 설정 확인

**Q: 오디오가 재생되지 않아요**
A: Audio Listener 존재 확인 (보통 Camera에 있음), AudioManager 볼륨 설정 확인

**Q: 상호작용이 작동하지 않아요**
A: Raycast Target 활성화, InteractionManager 초기화, EventSystem 존재 확인

### 7.2 성능 문제

**Q: 게임이 느려요**
A: 텍스처 압축 설정, 불필요한 Raycast Target 해제, Canvas 분리

**Q: 메모리 사용량이 높아요**
A: 오디오 압축 설정, 텍스처 해상도 조정, 사용하지 않는 에셋 언로드

---

## 🎉 8. 다음 단계

### 8.1 기본 게임 완성 후
1. **추가 스테이지** 제작
2. **더 복잡한 상호작용** 구현
3. **스토리 요소** 강화
4. **호러 요소** 확장

### 8.2 고급 기능 확장
1. **세이브/로드** 시스템
2. **인벤토리** 확장
3. **퍼즐** 시스템
4. **멀티플랫폼** 대응

### 8.3 배포 준비
1. **빌드 최적화**
2. **품질 보장 테스트**
3. **플랫폼별 최적화**
4. **마케팅 자료** 준비

---

## 📋 마무리

이 가이드를 순서대로 따라하면 완전히 작동하는 호러 어드벤처 게임을 만들 수 있습니다. 각 단계별 자세한 내용은 해당 가이드 문서를 참고하세요.

**성공적인 게임 개발을 위한 팁:**
- 📖 **문서를 꼼꼼히 읽고 따라하세요**
- 🧪 **각 단계마다 테스트하세요**
- 🎨 **플레이어 경험을 최우선으로 생각하세요**
- 🔧 **문제가 생기면 문제 해결 섹션을 참고하세요**

**개발을 시작할 준비가 되셨나요? [메인메뉴 씬 설정 가이드](01_MainMenu_Scene_Setup_Guide.md)부터 시작하세요!** 🚀