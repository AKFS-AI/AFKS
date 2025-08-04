# 🎨 Art 리소스 관리 가이드

## 📁 폴더 구조

```
Art/
├── Textures/               # 텍스처 리소스
│   ├── Backgrounds/        # 배경 이미지
│   ├── Items/              # 아이템 관련 이미지
│   ├── Horror/             # 공포 연출 이미지
│   ├── UI/                 # UI 관련 이미지
│   └── Effects/            # 이펙트 텍스처
├── Materials/              # 머티리얼
├── Sprites/                # 스프라이트 시트
└── Fonts/                  # 폰트 리소스
```

## 🎯 최적화 설정

### 배경 이미지 (Backgrounds/)
- **해상도**: 1920x1080 (PC), 1280x720 (모바일)
- **포맷**: PNG (투명도 필요시), JPG (불필요시)
- **압축**: RGB Compressed DXT1/BC7 (PC), ASTC 4x4 (모바일)
- **밉맵**: 비활성화 (2D 게임)

### 아이템 이미지 (Items/)
- **해상도**: 512x512 (아이콘), 1024x1024 (상세보기)
- **포맷**: PNG (투명도 필수)
- **압축**: RGBA Compressed DXT5/BC7 (PC), ASTC 4x4 (모바일)

### 공포 이미지 (Horror/)
- **해상도**: 1024x1024 또는 1920x1080
- **포맷**: PNG
- **압축**: RGB Compressed DXT1/BC7

## 📋 네이밍 규칙

### 배경 이미지
- `Stage{번호:00}_{영문이름}_{해상도}.확장자`
- 예: `Stage01_HospitalFront_1920x1080.png`

### 아이템 이미지
- `Item_{영문이름}_{타입}.확장자`
- 예: `Item_Cross_Icon.png`, `Item_Cross_Detail.png`

### 공포 이미지
- `Horror_{스테이지}_{이벤트명}.확장자`
- 예: `Horror_Lobby_GhostAppear.png`