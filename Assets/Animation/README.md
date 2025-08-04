# 🎬 Animation 관리 가이드

## 📁 폴더 구조

```
Animation/
├── UI/                     # UI 애니메이션
│   ├── Panels/            # 패널 애니메이션
│   ├── Buttons/           # 버튼 애니메이션
│   ├── Transitions/       # 전환 애니메이션
│   └── Effects/           # UI 이펙트 애니메이션
├── Horror/                # 공포 연출 애니메이션
│   ├── Jumpscares/        # 점프스케어 애니메이션
│   ├── ScreenEffects/     # 화면 효과
│   └── Distortions/       # 왜곡 효과
├── Items/                 # 아이템 애니메이션
│   ├── Pickups/           # 수집 애니메이션
│   ├── Interactions/      # 상호작용 애니메이션
│   └── Inventory/         # 인벤토리 애니메이션
├── Stages/                # 스테이지 애니메이션
│   ├── Transitions/       # 스테이지 전환
│   └── Backgrounds/       # 배경 애니메이션
└── Controllers/           # Animator Controllers
    ├── UI/                # UI 컨트롤러
    ├── Horror/            # 공포 컨트롤러
    └── Gameplay/          # 게임플레이 컨트롤러
```

## 🎯 애니메이션 타입별 설정

### UI 애니메이션 (UI/)
- **페이드 인/아웃**: 0.3초 (기본), 0.1초 (빠름)
- **슬라이드**: 0.5초, Ease Out Cubic
- **스케일**: 0.2초, Ease Out Back
- **컬러 변경**: 0.1초, Linear

### 공포 애니메이션 (Horror/)
- **점프스케어**: 0.1초 (즉시), 2초 (지속)
- **화면 흔들림**: 1초, Random
- **글리치**: 0.05초 간격, Random
- **페이드**: 2초, Ease In Cubic

### 아이템 애니메이션 (Items/)
- **수집 효과**: 0.5초, Ease Out Quart
- **호버 효과**: 0.2초, Ease Out
- **클릭 효과**: 0.1초, Ease In Out

## 📋 네이밍 규칙

### 애니메이션 클립
- `ANIM_{카테고리}_{액션}.anim`
- 예: `ANIM_UI_PanelFadeIn.anim`
- 예: `ANIM_Horror_ScreenShake.anim`
- 예: `ANIM_Item_PickupGlow.anim`

### Animator Controller
- `AC_{카테고리}.controller`
- 예: `AC_UIPanel.controller`
- 예: `AC_HorrorEvents.controller`