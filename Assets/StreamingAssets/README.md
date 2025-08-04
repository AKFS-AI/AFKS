# 📦 StreamingAssets 관리 가이드

## 📁 폴더 구조

```
StreamingAssets/
├── Config/                 # 외부 설정 파일
│   ├── GameSettings.json  # 게임 설정
│   ├── StageConfig.json   # 스테이지 설정
│   └── Localization/      # 다국어 파일
├── Video/                 # 비디오 파일
│   ├── Intro/             # 인트로 영상
│   └── Cutscenes/         # 컷신 영상
├── Database/              # 데이터베이스 파일
│   └── GameData.db        # SQLite 데이터베이스
└── Patches/               # 패치 파일
    └── Updates/           # 업데이트 파일
```

## 🎯 StreamingAssets 사용 목적

### 외부 설정 파일 (Config/)
- 게임 밸런스 조정용 JSON
- 빌드 후 수정 가능한 설정
- 다국어 지원 데이터

### 비디오 파일 (Video/)
- 플랫폼별 호환성이 필요한 영상
- 대용량 컷신 영상
- 스트리밍 재생이 필요한 콘텐츠

### 데이터베이스 (Database/)
- 대량의 게임 데이터
- 사용자 생성 콘텐츠
- 서버 연동 데이터

## ⚠️ 주의사항

- StreamingAssets은 그대로 빌드에 포함됨
- 압축되지 않아 빌드 크기 증가
- 보안이 필요한 데이터는 포함하지 말 것
- 모바일에서는 읽기 전용