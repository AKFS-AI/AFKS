# 📚 Resources 폴더 사용 가이드

## ⚠️ 중요: Resources 폴더 사용 최소화

Unity에서는 **Resources 폴더 사용을 권장하지 않습니다**.
대신 **Addressables** 또는 **ScriptableObject**를 사용하세요.

## 📁 제한적 사용 구조

```
Resources/
├── DefaultSettings/        # 기본 설정만
│   ├── DefaultGameConfig.asset
│   └── DefaultAudioConfig.asset
├── Fallback/              # 폴백 리소스
│   ├── ErrorTexture.png
│   └── DefaultFont.ttf
└── EditorOnly/            # 에디터 전용
    └── EditorIcons/
```

## 🚫 Resources 폴더를 피해야 하는 이유

### 1. 메모리 문제
- 빌드 시 모든 파일이 메모리에 로드됨
- 언로드가 어려워 메모리 누수 위험
- 사용하지 않는 리소스도 메모리 점유

### 2. 빌드 크기 증가
- 사용되지 않는 리소스도 빌드에 포함
- 플랫폼별 최적화 불가
- 불필요한 다운로드 크기 증가

### 3. 성능 문제
- Resources.Load()는 동기식 로딩
- 메인 스레드 블로킹 발생
- 초기 로딩 시간 증가

## ✅ 대안 방법

### Addressables 시스템 (권장)
```csharp
// 비동기 로딩
var handle = Addressables.LoadAssetAsync<Sprite>("item_cross");
await handle.Task;
```

### ScriptableObject (현재 사용)
```csharp
// Inspector에서 직접 할당
[SerializeField] private StageData stageData;
```

### Asset References
```csharp
// 타입 안전한 참조
[SerializeField] private AssetReference itemSprite;
```

## 📋 Resources 폴더가 필요한 경우

### 1. 에디터 도구
- 에디터 스크립트에서만 사용
- 빌드에 포함되지 않음

### 2. 초기 설정
- 게임 최초 실행 시 필요한 설정
- 매우 제한적으로만 사용

### 3. 폴백 리소스
- 오류 발생 시 대체 리소스
- 최소한의 안전장치