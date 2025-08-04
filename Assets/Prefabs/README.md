# 🧩 Prefabs 관리 가이드

## 📁 폴더 구조

```
Prefabs/
├── Core/                   # 핵심 시스템 프리팹
│   ├── Managers/          # 매니저 프리팹들
│   └── Systems/           # 시스템 프리팹들
├── UI/                    # UI 프리팹
│   ├── Panels/            # UI 패널들
│   ├── Components/        # UI 컴포넌트들
│   └── Effects/           # UI 이펙트
├── Gameplay/              # 게임플레이 프리팹
│   ├── Interactions/      # 상호작용 오브젝트
│   ├── Items/             # 아이템 프리팹
│   └── Stages/            # 스테이지 관련
├── Horror/                # 공포 연출 프리팹
│   ├── Effects/           # 공포 이펙트
│   └── Events/            # 공포 이벤트
└── Audio/                 # 오디오 프리팹
    ├── Sources/           # 오디오 소스
    └── Mixers/            # 믹서 프리팹
```

## 🎯 프리팹 카테고리

### 핵심 시스템 (Core/)
- `GameManager.prefab` - 게임 매니저
- `StageManager.prefab` - 스테이지 매니저
- `AudioManager.prefab` - 오디오 매니저
- `UIManager.prefab` - UI 매니저

### UI 프리팹 (UI/)
- `MainCanvas.prefab` - 메인 캔버스
- `InventoryPanel.prefab` - 인벤토리 패널
- `DialogPanel.prefab` - 대화 패널
- `InventorySlot.prefab` - 인벤토리 슬롯

### 게임플레이 (Gameplay/)
- `InteractionPoint.prefab` - 상호작용 포인트
- `ItemPickup.prefab` - 아이템 수집 오브젝트
- `StageBackground.prefab` - 스테이지 배경

### 공포 연출 (Horror/)
- `JumpscareEffect.prefab` - 점프스케어 이펙트
- `HorrorOverlay.prefab` - 공포 오버레이
- `ScreenShake.prefab` - 화면 흔들림

## 📋 네이밍 규칙

### 프리팹 이름
- `{기능}{타입}.prefab`
- 예: `InventoryPanel.prefab`
- 예: `InteractionPoint.prefab`
- 예: `JumpscareEffect.prefab`

### 변형 프리팹
- `{기본이름}_{변형}.prefab`
- 예: `InventorySlot_Empty.prefab`
- 예: `HorrorEffect_Fade.prefab`