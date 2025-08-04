# 🎨 Materials 관리 가이드

## 📁 폴더 구조

```
Materials/
├── UI/                     # UI 머티리얼
│   ├── Buttons/           # 버튼 머티리얼
│   ├── Panels/            # 패널 머티리얼
│   └── Effects/           # UI 이펙트 머티리얼
├── Sprites/               # 스프라이트 머티리얼
│   ├── Items/             # 아이템 머티리얼
│   └── Characters/        # 캐릭터 머티리얼
├── Effects/               # 이펙트 머티리얼
│   ├── Particles/         # 파티클 머티리얼
│   ├── Distortion/        # 왜곡 효과
│   └── Transition/        # 전환 효과
└── PostProcess/           # 포스트 프로세싱
    ├── Horror/            # 공포 효과
    └── Atmosphere/        # 분위기 효과
```

## 🎯 머티리얼 타입별 설정

### UI 머티리얼 (UI/)
- **셰이더**: UI/Default, UI/Unlit
- **렌더 큐**: Overlay (3000+)
- **블렌딩**: Alpha Blend

### 스프라이트 머티리얼 (Sprites/)
- **셰이더**: Sprites/Default, Sprites/Lit
- **렌더 큐**: Transparent (3000)
- **필터링**: Point (픽셀 아트) / Bilinear (일반)

### 공포 효과 머티리얼 (Effects/Horror/)
- **셰이더**: Custom Horror Shaders
- **효과**: 글리치, 왜곡, 페이드
- **애니메이션**: 시간 기반 변화

## 📋 네이밍 규칙

### 머티리얼 이름
- `MAT_{카테고리}_{이름}.mat`
- 예: `MAT_UI_ButtonHover.mat`
- 예: `MAT_Horror_GlitchEffect.mat`
- 예: `MAT_Sprite_ItemIcon.mat`