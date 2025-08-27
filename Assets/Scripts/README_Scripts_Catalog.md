# AFKS Scripts 카탈로그

> 목적: `Assets/Scripts/` 내 스크립트를 카테고리별로 한눈에 파악하고, 책임과 의존 경계를 빠르게 이해하기 위한 안내서입니다. 상세 코딩 규칙은 `Assets/Scripts/README_Scripts_Guide.txt`를 참고하십시오.

---

## 목차
- Core
  - Events
  - Services (Audio / Input / Inventory / Save / Scene / ServiceLocator)
  - Systems
  - UI
  - Utils
- Features
  - Menu
  - Stage
  - Items
  - Interaction
- Editor
- 유지보수 규칙 및 업데이트 절차

---

## Core

### Events
- `Core/Events/GameEvents.cs`: 전역 이벤트 허브. 기능 간 결합도를 낮추는 경량 C# 이벤트 모음.

### Services
- `Core/Services/Audio/IAudioService.cs`: 오디오 서비스 인터페이스.
- `Core/Services/Audio/AudioService.cs`: BGM/SE 재생 등의 오디오 구현(확장 지점).
- `Core/Services/Input/IInputService.cs`: 입력 상태/이벤트·전역 잠금 인터페이스. 게임 코드는 이 인터페이스만 참조.
- `Core/Services/Input/InputService.cs`: 클릭/포인터 추상화, UI 위 입력 무시, 전역 잠금 제공.
- `Core/Services/Inventory/IInventoryService.cs`: 인벤토리 서비스 인터페이스.
- `Core/Services/Inventory/InventoryService.cs`: 인벤토리 보유/획득 로직 구현(확장 지점).
- `Core/Services/Save/ISaveService.cs`: 저장 서비스 인터페이스(구현 예정).
- `Core/Services/Scene/ISceneService.cs`: 씬 전환/페이드/활성화 인터페이스.
- `Core/Services/Scene/SceneService.cs`: 페이드·입력 잠금 포함 Additive 스테이지 로드/언로드, 전환 안전장치.
- `Core/Services/ServiceLocator.cs`: 런타임 DI 대체용 간단 서비스 로케이터(싱글톤 등록/검색/해제).

### Systems
- `Core/Systems/StartupLoader.cs`: 초기 진입 시 서비스 부트스트랩/상태 준비.

### UI
- `Core/UI/FadeCanvas.cs`: 화면 페이드 인/아웃.
- `Core/UI/CloseupViewer.cs`: 클로즈업 이미지/패널 표시 제어.

### Utils
- `Core/Utils/Log.cs`: 표준 로그 유틸(Info/Warn/Error 형식화).
- `Core/Utils/GlobalSingletonGuard.cs`: 전역 싱글톤 중복 생성 가드.

---

## Features

### Menu
- `Features/Menu/MainMenuUI.cs`: 메인 메뉴 버튼 바인딩(새 게임/계속/종료), 저장 유무에 따른 계속 버튼 제어.
- `Features/Menu/SettingsPanel.cs`: 설정 패널 표시/숨김/토글 컨트롤러.

### Stage
- `Features/Stage/StageIds.cs`: 스테이지 식별자 상수 모음.
- `Features/Stage/StageRoot.cs`: 스테이지 진입/종료 라이프사이클 훅(Initialize/Teardown).
- `Features/Stage/StageEventSystem.cs`: 데이터 드리븐 이벤트 시퀀스 제어(표준 경로).
- `Features/Stage/Animations/StageAnimationController.cs`: 체인/귀신/오브젝트 애니메이션 통합. 카메라 줌은 미포함.

### Items
- `Features/Items/ItemData.cs`: 아이템 데이터 ScriptableObject(표시명/설명/이미지 포함).
- `Features/Items/ItemIds.cs`: 설계서 기반 아이템 ID 상수.

### Interaction
- `Features/Interaction/CloseupPanelAutoWire.cs`: 클로즈업 패널 참조 자동 연결/바인딩.
- `Features/Interaction/InputLockWhileActive.cs`: 활성화 동안 입력 잠금 토글.
- `Features/Interaction/HotspotMove.cs`: 핫스팟 이동 상호작용(월드 오브젝트 이동).
- `Features/Interaction/HotspotInspect.cs`: 핫스팟 조사 상호작용(정보/클로즈업 트리거).
- `Features/Interaction/ChainCloseupController.cs`: 사슬 클로즈업 UI/상태 제어.
- `Features/Interaction/ChainUnlock.cs`: 사슬 해제 조건 검증 및 해제 처리.
- `Features/Interaction/HotspotMoveUI.cs`: 핫스팟 이동에 대한 UI 피드백/버튼 연동.
- `Features/Interaction/ChainHotspotUI.cs`: 사슬 관련 핫스팟 UI 제어.
- `Features/Interaction/PickupItem.cs`: 아이템 획득 처리 및 인벤토리 반영.
- `Features/Interaction/DocumentCloseup.cs`: 문서형 클로즈업 열기/닫기.
- `Features/Interaction/DoorLocked.cs`: 잠긴 문 상호작용(키 필요/메시지 표시 등).
- `Features/Interaction/JumpscareTrigger.cs`: 점프스케어 트리거(이벤트 발행/연출 진입점).
- `Features/Interaction/CameraZoomEffect.cs`: 카메라 줌 표준 컴포넌트(유일 경로). CameraZoomController는 제거됨.
- `Features/Interaction/ClickHandler.cs` + `ClickToEventRouter.cs`: 클릭→StageEventSystem 라우팅 표준 경로.

---

## Editor
- `Scripts/Editor/StartupLoaderEditor.cs`: `StartupLoader` 커스텀 인스펙터/에디터 유틸.

---

## 유지보수 규칙 및 업데이트 절차
- **분류 원칙**: 엔진 어댑터/전역 기능은 `Core/`, 기능 단위는 `Features/<Feature>/`, 에디터 코드는 `Editor/`.
- **의존 경계**: 게임 도메인 코드는 인터페이스(`I*`)에 의존하고, 구현은 서비스/어댑터가 담당. `Features` → `Core` 단방향을 유지.
- **입력**: 게임 코드는 `IInputService`만 참조. InputAction/UnityEngine.Input 직접 접근 금지.
- **씬 전환**: 전역 전환은 `ISceneService` 사용. 직접 `SceneManager` 호출 지양.
- **이벤트**: 전역 브로드캐스트는 `GameEvents` 사용. 이벤트 명세 변경 시 이 문서와 구독처 동시 업데이트.
- **API 폐기 대응**: Unity deprecated API 사용 금지. `Object.FindObjectsByType(FindObjectsSortMode.None)` 등 최신 API 우선.
- **파일 추가 시**:
  1) 위치 선택: `Core/Services|Systems|UI|Utils|Events` 또는 `Features/<Feature>/`.
  2) 네이밍: 파일명=클래스명, 인터페이스는 `I` 접두사. `Manager` 금지(`Service`/`System` 사용).
  3) 문서화: 공개 API에 /// 요약, 인스펙터 라벨/툴팁 제공(한국어).
  4) 본 카탈로그에 1줄 요약 추가, `README_Scripts_Guide.txt`와 정책 충돌 여부 점검.
- **변경 시 체크리스트**:
  - [ ] 인터페이스 시그니처 변경 반영(구현/구독자 포함)
  - [ ] 이벤트 명 변경 반영(발행/구독 모두)
  - [ ] 씬/빌드 설정 영향 확인(스테이지 추가/이동)
  - [ ] 프로파일 영향(핫패스 할당/박싱/LINQ 여부) 점검
  - [ ] 문서 동기화: 본 파일과 `README_Scripts_Guide.txt` 업데이트

---

마지막 업데이트: YYYY-MM-DD
