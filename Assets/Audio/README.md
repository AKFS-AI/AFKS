# 🎵 Audio 리소스 관리 가이드

## 📁 폴더 구조

```
Audio/
├── Music/                  # 배경음악
│   ├── BGM/               # 게임 배경음악
│   └── Ambient/           # 앰비언트 사운드
├── SFX/                   # 효과음
│   ├── UI/                # UI 효과음
│   ├── Interaction/       # 상호작용 사운드
│   ├── Horror/            # 공포 효과음
│   └── Environment/       # 환경음
├── Voice/                 # 음성
│   └── Narration/         # 내레이션
└── AudioMixers/           # 오디오 믹서
```

## 🎯 최적화 설정

### BGM (Music/BGM/)
- **포맷**: Ogg Vorbis
- **샘플레이트**: 44.1kHz
- **채널**: 스테레오
- **품질**: 0.7 (압축률 고려)
- **로드타입**: Streaming

### 앰비언트 (Music/Ambient/)
- **포맷**: Ogg Vorbis
- **샘플레이트**: 22.05kHz
- **채널**: 스테레오
- **품질**: 0.5
- **로드타입**: Streaming

### 효과음 (SFX/)
- **포맷**: WAV (짧은 소리), Ogg (긴 소리)
- **샘플레이트**: 44.1kHz
- **채널**: 모노 (대부분), 스테레오 (특수한 경우)
- **로드타입**: Decompress On Load

### 공포 효과음 (SFX/Horror/)
- **포맷**: WAV (즉시 재생 필요)
- **샘플레이트**: 44.1kHz
- **채널**: 스테레오 (몰입감)
- **로드타입**: Decompress On Load

## 📋 네이밍 규칙

### BGM
- `BGM_{스테이지명}_{분위기}.확장자`
- 예: `BGM_Hospital_Ambient.ogg`

### 효과음
- `SFX_{카테고리}_{액션}.확장자`
- 예: `SFX_Horror_Scream.wav`, `SFX_UI_Click.wav`

### 앰비언트
- `AMB_{장소}_{시간대}.확장자`
- 예: `AMB_Hospital_Night.ogg`