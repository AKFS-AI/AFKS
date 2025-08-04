# AFKS 게임 시스템 구조

## 📁 프로젝트 구조

```
Assets/GameSystems/
├── 🎮 Core/                          # 핵심 게임 시스템
│   ├── Scripts/
│   │   ├── GameManager.cs           # 게임 전체 관리
│   │   ├── SceneController.cs       # 씬 전환 관리
│   │   └── GameConfig.cs           # 게임 설정
│   └── Data/
│       └── GameConfig.asset        # 게임 설정 에셋
│
├── 🎯 StageSystem/                   # 스테이지 관리
│   ├── Scripts/
│   │   ├── StageManager.cs         # 스테이지 관리자
│   │   ├── StageData.cs            # 스테이지 데이터
│   │   └── StageInteractionController.cs
│   └── Data/
│       └── Stages/                 # 각 스테이지별 데이터
│
├── 🖱️ InteractionSystem/             # 상호작용 시스템
│   ├── Scripts/
│   │   └── InteractionManager.cs   # 상호작용 관리자
│   └── Data/
│       └── InteractionConfig.asset
│
├── 🎒 InventorySystem/               # 인벤토리 시스템
│   ├── Scripts/
│   │   ├── InventoryManager.cs     # 인벤토리 관리자
│   │   └── ItemData.cs            # 아이템 데이터
│   └── Data/
│       └── Items/                 # 아이템별 데이터
│
├── 😱 HorrorSystem/                  # 공포 연출 시스템
│   ├── Scripts/
│   │   ├── HorrorEventManager.cs   # 공포 이벤트 관리자
│   │   └── HorrorEventController.cs
│   └── Data/
│       └── HorrorEvents/          # 공포 이벤트별 데이터
│
├── 🔊 AudioSystem/                   # 오디오 시스템
│   ├── Scripts/
│   │   └── AudioManager.cs        # 오디오 관리자
│   └── Data/
│       └── AudioConfig.asset
│
├── 🎨 UISystem/                      # UI 시스템
│   ├── Scripts/
│   │   ├── UIManager.cs           # UI 관리자
│   │   └── InventorySlotUI.cs     # 인벤토리 슬롯 UI
│   └── Prefabs/
│       └── UI 프리팹들
│
└── 🔧 Shared/                        # 공통 시스템
    ├── Scripts/
    │   ├── Utils/                 # 유틸리티
    │   ├── Events/                # 이벤트 시스템
    │   └── Interfaces/            # 인터페이스
    └── Data/
        └── BaseClasses/           # 기본 클래스들
```

## 🚀 시작하기

### 1. 기본 설정
1. GameManager를 씬에 배치
2. GameConfig.asset 생성 및 설정
3. 각 매니저들을 씬에 배치

### 2. 스테이지 설정
1. StageData.asset들을 생성
2. 배경 이미지 및 상호작용 포인트 설정
3. StageManager에 스테이지 데이터 연결

### 3. 아이템 설정
1. ItemData.asset들을 생성
2. 아이템 아이콘 및 속성 설정
3. InventoryManager에 아이템 데이터베이스 연결

### 4. 공포 이벤트 설정
1. 각 스테이지별 공포 이벤트 데이터 설정
2. 점프스케어 이미지 및 사운드 준비
3. 트리거 조건 설정

## 📋 주요 기능

### ✅ 완성된 시스템들
- [x] 게임 상태 관리 (GameManager)
- [x] 씬 전환 시스템 (SceneController)
- [x] 스테이지 관리 시스템 (StageManager)
- [x] 상호작용 시스템 (InteractionManager)
- [x] 인벤토리 시스템 (InventoryManager)
- [x] 공포 연출 시스템 (HorrorEventManager)
- [x] 오디오 시스템 (AudioManager)
- [x] UI 시스템 (UIManager)
- [x] 이벤트 시스템 (GameEvent)
- [x] 저장/로드 시스템 (ISaveable)

### 🎯 핵심 특징
- **모듈러 설계**: 각 시스템이 독립적으로 작동
- **이벤트 기반**: 느슨한 결합으로 시스템 간 통신
- **데이터 드리븐**: ScriptableObject 기반 설정
- **성능 최적화**: 메모리 효율적 리소스 관리
- **확장성**: 새로운 기능 쉽게 추가 가능

## 🔧 커스터마이징

### 새 스테이지 추가
1. StageData.asset 생성
2. 배경 이미지 및 상호작용 포인트 설정
3. StageManager의 stages 리스트에 추가

### 새 아이템 추가
1. ItemData.asset 생성
2. 아이콘 및 속성 설정
3. InventoryManager의 itemDatabase에 추가

### 새 공포 이벤트 추가
1. HorrorEventData 설정
2. StageData의 horrorEvents에 추가
3. 트리거 조건 및 효과 설정

## 📞 지원

문제가 발생하거나 질문이 있으시면:
1. 디버그 모드를 활성화하여 로그 확인
2. 각 매니저의 PrintDebugInfo() 메서드 사용
3. 콘솔 로그를 통한 시스템 상태 모니터링

---

**"수수께끼 괴담" 게임 개발을 위한 완전한 시스템 구조**  
Unity 6000.0.42f1 호환, 프로덕션 레벨 품질