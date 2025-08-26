# 씬 스캐폴더(에디터 툴)

- 목적: Core/Stage 씬을 클릭 한 번으로 생성하고 필수 컴포넌트를 자동 배치/와이어링해 초기 셋업을 10초 내로 완료.

- 배치 요소:
  - Core 씬: Main Camera, FadeCanvas(AFKS.Core.UI.FadeCanvas), EventSystem, AudioService, InventoryService
  - Stage 씬: StageRoot(AFKS.Features.Stage.StageRoot), EventSystem(중복 시 Core 유지)

- 의존/역할:
  - SceneService: Additive 전환과 페이드 담당
  - FadeCanvas: 입력 차단 + 페이드 애니메이션
  - AudioService/InventoryService: ServiceLocator 전역 등록

- 사용 규칙:
  1) 메뉴 AFKS > 도구 > 씬 스캐폴더 열기
  2) 코어 씬 생성/갱신 실행 후 저장 확인
  3) 스테이지 이름 입력 후 생성
  4) 전환 테스트 로드 버튼으로 Core+Stage 동시 로드

- 확장 포인트:
  - Addressables 통합, 스테이지 템플릿 프리셋, 인터랙션 핫스팟 자동 배치

- 테스트:
  - 저장 후 콘솔 에러 없음
  - Core+Stage 로드시 EventSystem 중복 자동 정리 확인
