Title: Scripts 가이드(구조/네이밍/폴딩/한글화)
Version: v1.0

1) 구조
- Core/: 서비스/시스템/UI 등 전역 모듈
- Features/: 기능 단위(메뉴/스테이지/인터랙션 등)

2) 네이밍
- 클래스/컴포넌트: UpperCamelCase (예: SceneService)
- 상수/ID: PascalCase 또는 SNAKE_CASE(프로젝트 정책에 따름)
- 파일: 클래스명과 동일

3) 폴딩(#region)
- 권장 순서: 필드 → 유니티 수명주기 → 공개 API → 내부 메서드 → 코루틴 → 이벤트 핸들러
- 영역명은 한국어 사용(예: #region 필드)

4) 한글화 규칙
- 주석/XML: 한국어로 작성. 공개 API는 /// 요약 우선
- 인스펙터: InspectorName/Tooltip/AddComponentMenu 모두 한국어
- 메뉴 경로: AFKS/카테고리/이름

5) 품질
- 가드절·조기 반환, 의미 있는 네이밍, 깊은 중첩 회피
- try/catch 최소화, 빈 catch 금지
- 문서(StageHierarchy)와 동기화 유지


