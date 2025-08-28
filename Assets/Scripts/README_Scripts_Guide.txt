Title: Scripts 가이드(구조/네이밍/폴딩/한글화)
Version: v1.1

1) 구조
- Core/: 서비스/시스템/UI 등 전역 모듈
- Features/: 기능 단위(메뉴/스테이지/인터랙션 등)

2) 네이밍
- 클래스/컴포넌트: UpperCamelCase (예: SceneService)
- 상수/ID: PascalCase 또는 SNAKE_CASE(프로젝트 정책에 따름)
- 파일: 클래스명과 동일
- 접미사 규칙: Manager 금지, Service/System 사용

3) 폴딩(#region)
- 권장 순서: 필드 → 유니티 수명주기 → 공개 API → 내부 메서드 → 코루틴 → 이벤트 핸들러
- 영역명은 한국어 사용(예: #region 필드)

4) 한글화 규칙
- 주석/XML: 한국어로 작성. 공개 API는 /// 요약 우선
- 인스펙터: InspectorName/Tooltip/AddComponentMenu 모두 한국어
- 메뉴 경로: AFKS/카테고리/이름

5) 품질
- 가드절·조기 반환, 의미 있는 네이밍, 깊은 중첩 회피
- try/catch 최소화, null 체크와 가드절 우선 사용
- 문서(StageHierarchy)와 동기화 유지
- LINQ 사용 시 성능 영향 고려, 핫패스에서는 for 루프 우선

6) 상호작용/줌 표준
- 클릭 라우팅: `ClickHandler` + `ClickToEventRouter` 조합만 사용. 중앙 매니저는 레지스트리/토글 용도.
- 카메라 줌: `CameraZoomEffect`만 사용. 레거시 `CameraZoomController`는 제거됨.

7) 아키텍처 규칙
- SceneManager 직접 사용 금지: ISceneService 우선 사용
- 네이밍 규칙: Manager → Service/System 변경 완료
- 에러 처리: try-catch 대신 null 체크와 가드절 사용


