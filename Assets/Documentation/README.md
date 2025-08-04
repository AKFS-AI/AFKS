# 📚 Documentation 가이드

## 📁 폴더 구조

```
Documentation/
├── API/                    # API 문서
│   ├── Core/              # 핵심 시스템 API
│   ├── Managers/          # 매니저 클래스 API
│   └── Interfaces/        # 인터페이스 문서
├── Design/                # 설계 문서
│   ├── Architecture/      # 아키텍처 문서
│   ├── GameDesign/        # 게임 디자인 문서
│   └── TechnicalDesign/   # 기술 설계 문서
├── Tutorials/             # 튜토리얼
│   ├── GettingStarted/    # 시작 가이드
│   ├── SystemGuides/      # 시스템별 가이드
│   └── Examples/          # 예제 코드
├── Changelog/             # 변경 로그
└── References/            # 참고 자료
    ├── UnityBestPractices/
    └── PerformanceGuides/
```

## 📝 문서 작성 가이드

### API 문서 (API/)
- 모든 public 메서드 문서화
- 매개변수 및 반환값 설명
- 사용 예제 포함
- 주의사항 및 제한사항 명시

### 설계 문서 (Design/)
- 시스템 아키텍처 다이어그램
- 클래스 관계도
- 데이터 플로우 차트
- 성능 고려사항

### 튜토리얼 (Tutorials/)
- 단계별 가이드
- 스크린샷 포함
- 문제 해결 가이드
- FAQ

## 📋 문서 템플릿

### API 문서 템플릿
```markdown
# ClassName

## 개요
클래스의 목적과 역할 설명

## 사용법
기본적인 사용 방법

## 메서드
### MethodName
- **설명**: 메서드 기능 설명
- **매개변수**: 
  - `param1` (type): 설명
- **반환값**: type - 설명
- **예제**:
```csharp
// 예제 코드
```

## 주의사항
사용 시 주의할 점들
```