# 🏥 스테이지 1 제작 가이드 (병원 로비)

## 🎯 개요
이 가이드는 병원 내부 로비인 스테이지 1을 처음부터 제작하는 방법을 단계별로 설명합니다. 플레이어가 철문을 통과한 후 처음 마주하는 내부 공간입니다.

---

## 📋 1. 스테이지 1 기본 설정

### 1.1 스테이지 데이터 생성
1. **Assets → Create → AFKS → Stage → Stage Data**
2. 이름을 `Stage1_HospitalLobby`로 설정
3. **Inspector 기본 설정**:
   - **Stage Index**: 1
   - **Stage Name**: "병원 로비"
   - **Stage Description**: "오랫동안 버려진 병원의 로비입니다. 어둠 속에서 무언가가 움직이는 것 같습니다."
   - **Background Image**: 병원 로비 배경 이미지 할당
   - **Ambient Color**: (0.6, 0.6, 0.8, 1) - 차가운 푸른 톤
   - **Enable Dark Mode**: ✅ 체크

### 1.2 오디오 설정
**Stage1_HospitalLobby** Inspector에서:
- **Background Music**: 병원 내부 BGM (불안한 분위기)
- **Ambient Sound**: 병원 내부 환경음 (에어컨 소리, 먼 곳의 발소리 등)
- **Music Volume**: 0.5
- **Ambient Volume**: 0.3

### 1.3 StageManager에 등록
1. **StageManager** 선택
2. **Stages** 배열 크기를 2로 확장
3. **Element 1**에 `Stage1_HospitalLobby` 할당

---

## 🖼️ 2. 스테이지 1 시각적 구성

### 2.1 Stage1 Container 생성
1. **GameplayScene → StageCanvas → Stages → 우클릭 → Create Empty**
2. 이름을 `Stage1_HospitalLobby`로 변경
3. **RectTransform** 설정:
   - **Anchor Presets**: Stretch
   - **Left, Top, Right, Bottom**: 모두 0
4. **GameObject → Set Active**: ❌ 비활성화 (스테이지 0에서 시작)

### 2.2 배경 이미지 설정
1. **Stage1_HospitalLobby → 우클릭 → UI → Image**
2. 이름을 `LobbyBackground`로 변경
3. **RectTransform** 설정:
   - **Anchor Presets**: Stretch
   - **Left, Top, Right, Bottom**: 모두 0
4. **Image** 설정:
   - **Source Image**: 병원 로비 배경 이미지
   - **Preserve Aspect**: ❌ 체크 해제 (전체 화면 덮도록)
   - **Raycast Target**: ❌ 체크 해제

### 2.3 조명 효과 레이어
1. **LobbyBackground → 우클릭 → UI → Image**
2. 이름을 `LightingOverlay`로 변경
3. **RectTransform**: 부모와 동일하게 설정
4. **Image** 설정:
   - **Source Image**: 조명 효과 이미지 (선택사항)
   - **Color**: (1, 1, 1, 0.1) - 매우 연한 흰색
   - **Raycast Target**: ❌ 체크 해제

---

## 🔍 3. 상호작용 포인트 설정

### 3.1 리셉션 데스크 상호작용
1. **Stage1_HospitalLobby → 우클릭 → UI → Image**
2. 이름을 `ReceptionDesk`로 변경
3. **RectTransform** 설정:
   - **Anchor**: Center
   - **Position**: (-200, -100, 0) (리셉션 데스크 위치)
   - **Width**: 300, **Height**: 200
4. **Image** 설정:
   - **Source Image**: 투명 이미지 (상호작용 영역만)
   - **Color**: (1, 1, 1, 0) - 완전 투명
   - **Raycast Target**: ✅ 체크
5. **Add Component → Scripts → StageSystem → StageInteractionController**

#### 3.1.1 리셉션 데스크 InteractionPoint 설정
**Assets → Create → AFKS → Stage → Interaction Point Data**로 데이터 생성 또는 직접 설정:
- **Id**: "reception_desk"
- **Display Name**: "리셉션 데스크"
- **Description**: "오래된 리셉션 데스크입니다. 무언가 있을지도 모릅니다."
- **Position**: (-200, -100)
- **Size**: (300, 200)
- **Interaction Type**: Click
- **Is Enabled**: ✅ 체크
- **Is Visible**: ✅ 체크
- **Interaction Sound**: 서랍 열리는 소리
- **Result**:
  - **Result Type**: 1 (아이템)
  - **Give Item**: ✅ 체크
  - **Item Id**: "key"
  - **Message**: "열쇠를 발견했습니다!"

### 3.2 의료 장비 캐비닛 상호작용
1. **Stage1_HospitalLobby → 우클릭 → UI → Image**
2. 이름을 `MedicalCabinet`로 변경
3. **RectTransform** 설정:
   - **Position**: (250, 50, 0)
   - **Width**: 200, **Height**: 300
4. **Image** 및 **StageInteractionController** 설정
5. **InteractionPoint** 설정:
   - **Id**: "medical_cabinet"
   - **Display Name**: "의료 장비 캐비닛"
   - **Description**: "오래된 의료 장비들이 들어있는 캐비닛입니다."
   - **Result**:
     - **Result Type**: 1 (아이템)
     - **Give Item**: ✅ 체크
     - **Item Id**: "flashlight"
     - **Message**: "손전등을 발견했습니다!"

### 3.3 엘리베이터 상호작용 (다음 스테이지)
1. **Stage1_HospitalLobby → 우클릭 → UI → Image**
2. 이름을 `ElevatorDoor`로 변경
3. **RectTransform** 설정:
   - **Position**: (0, 200, 0)
   - **Width**: 150, **Height**: 250
4. **TwoStageInteractionController** 대신 일반 **StageInteractionController** 사용
5. **InteractionPoint** 설정:
   - **Id**: "elevator_door"
   - **Display Name**: "엘리베이터"
   - **Description**: "상층으로 올라가는 엘리베이터입니다."
   - **Required Items**: ["key"] (열쇠 필요)
   - **Result**:
     - **Result Type**: 2 (스테이지 변경)
     - **Change Stage**: ✅ 체크
     - **Target Stage Index**: 2
     - **Message**: "엘리베이터가 작동합니다..."

### 3.4 CCTV 모니터 상호작용 (호러 이벤트)
1. **Stage1_HospitalLobby → 우클릭 → UI → Image**
2. 이름을 `CCTVMonitor`로 변경
3. **RectTransform** 설정:
   - **Position**: (-300, 150, 0)
   - **Width**: 120, **Height**: 80
4. **InteractionPoint** 설정:
   - **Id**: "cctv_monitor"
   - **Display Name**: "CCTV 모니터"
   - **Description**: "아직 작동하는 CCTV 모니터입니다."
   - **Result**:
     - **Result Type**: 0 (일반)
     - **Trigger Horror Event**: ✅ 체크
     - **Horror Event Id**: "cctv_ghost"
     - **Message**: "모니터에 무언가가 비쳤습니다..."

---

## 👻 4. 호러 이벤트 설정

### 4.1 CCTV 고스트 이벤트
1. **Assets → Create → AFKS → Horror → Horror Event Data**
2. 이름을 `CCTVGhostEvent`로 설정
3. **기본 정보**:
   - **Event Id**: "cctv_ghost"
   - **Event Name**: "CCTV 유령 출현"
   - **Description**: "CCTV 모니터에 갑작스럽게 나타나는 유령"

4. **트리거 설정**:
   - **Trigger Type**: OnInteraction
   - **Trigger Delay**: 1.0
   - **Trigger Condition**: "cctv_monitor"

5. **시각적 요소**:
   - **Jumpscare Image**: 유령 이미지 할당
   - **Image Position**: (0, 0) - 화면 중앙
   - **Image Size**: (400, 300)
   - **Display Duration**: 2.0

6. **오디오**:
   - **Horror Sound**: 무서운 비명 소리
   - **Volume**: 0.8

7. **특수 효과**:
   - **Enable Screen Shake**: ✅ 체크
   - **Shake Intensity**: 1.0
   - **Shake Duration**: 1.5

8. **재사용 설정**:
   - **Can Repeat**: ❌ 체크 해제 (한 번만)
   - **Cooldown Time**: 0

### 4.2 환경 호러 이벤트
1. **새 Horror Event Data 생성**: `AmbientScareEvent`
2. **설정**:
   - **Event Id**: "ambient_scare"
   - **Event Name**: "환경 공포 이벤트"
   - **Trigger Type**: OnTimer
   - **Trigger Delay**: 45.0 (45초 후)
   - **Jumpscare Image**: 없음
   - **Horror Sound**: 갑작스러운 쿵 소리
   - **Volume**: 0.6
   - **Enable Screen Shake**: ✅ 체크
   - **Shake Intensity**: 0.3
   - **Can Repeat**: ✅ 체크
   - **Cooldown Time**: 120.0 (2분)

### 4.3 HorrorEventManager에 등록
1. **HorrorEventManager** 선택
2. **Horror Events** 배열에 위 이벤트들 추가
3. **Stage-Specific Events** 설정으로 스테이지 1에서만 작동하도록 설정

---

## 💡 5. 조명 및 분위기 설정

### 5.1 손전등 시스템 활성화 영역
1. **Stage1_HospitalLobby → 우클릭 → Create Empty**
2. 이름을 `DarkAreas`로 변경
3. **Add Component → Scripts → StageSystem → DarkAreaController** (새로 생성 필요)

#### 5.1.1 DarkAreaController 스크립트 (간단 버전)
```csharp
// 어두운 영역에서 손전등 사용 유도
public class DarkAreaController : MonoBehaviour
{
    [SerializeField] private List<RectTransform> darkAreas;
    [SerializeField] private Color darkOverlayColor = new Color(0, 0, 0, 0.7f);
    
    private void Start()
    {
        CheckFlashlightStatus();
    }
    
    private void CheckFlashlightStatus()
    {
        bool hasFlashlight = ItemManager.Instance.HasFlashlight;
        SetDarkAreasVisibility(!hasFlashlight);
    }
}
```

### 5.2 조명 효과 애니메이션
1. **LightingOverlay** 선택
2. **Window → Animation → Animation** 창 열기
3. **Create** 버튼으로 새 애니메이션 생성: `LobbyLightFlicker`
4. **애니메이션 설정**:
   - **0초**: Alpha = 0.05
   - **1초**: Alpha = 0.15
   - **2초**: Alpha = 0.05
   - **Loop**: ✅ 체크

---

## 🎵 6. 스테이지별 오디오 시스템

### 6.1 환경음 레이어링
**AudioManager**에서 Stage1 전용 사운드 설정:
1. **Base Ambient**: 병원 내부 환경음 (에어컨, 기계 소음)
2. **Random Events**: 간헐적 소음 (발소리, 문 삐걱거리는 소리)
3. **Interactive Sounds**: 상호작용 전용 사운드

### 6.2 스테이지 전환 시 오디오 변화
1. **StageManager** 스크립트에서 스테이지 변경 시 BGM 전환
2. **AudioManager.Instance.PlayBGM()** 호출
3. **크로스페이드** 효과로 자연스러운 전환

---

## 🎒 7. 아이템 시스템 연동

### 7.1 아이템 수집 피드백
각 아이템 수집 시:
1. **시각적 피드백**: 아이템 획득 애니메이션
2. **오디오 피드백**: 수집 사운드
3. **UI 업데이트**: 인벤토리 슬롯 활성화
4. **메시지 표시**: "열쇠를 발견했습니다!"

### 7.2 아이템 사용 조건
- **엘리베이터**: 열쇠 필요
- **어두운 영역**: 손전등 권장
- **특정 상호작용**: 두 아이템 모두 필요 (향후 확장)

---

## 🎮 8. 플레이어 가이드 시스템

### 8.1 튜토리얼 메시지
스테이지 1 진입 시 순차적 메시지 표시:
1. "병원 내부로 들어왔습니다..."
2. "주변을 탐색해서 필요한 아이템을 찾으세요."
3. "클릭하여 상호작용할 수 있습니다."

### 8.2 힌트 시스템
1. **리셉션 데스크 근처**에 "여기에 뭔가 있을 것 같다" 힌트
2. **의료 캐비닛** 근처에 "의료 장비를 확인해보자" 힌트
3. **엘리베이터** 근처에 "잠겨있다. 열쇠가 필요할 것 같다" 힌트

---

## 📱 9. UI 및 진행도 표시

### 9.1 스테이지 진행도 UI
1. **HUD → 우클릭 → UI → Panel**
2. 이름을 `ProgressPanel`로 변경
3. **진행도 표시**:
   - 수집된 아이템: 2/2
   - 상호작용 완료: 4/4
   - 스테이지 완료도: 100%

### 9.2 목표 표시 시스템
1. **현재 목표**: "아이템을 찾아서 엘리베이터를 작동시키세요"
2. **세부 목표**:
   - [ ] 열쇠 찾기
   - [ ] 손전등 찾기
   - [ ] 엘리베이터 조사하기

---

## ⚡ 10. 성능 최적화

### 10.1 텍스처 최적화
- **배경 이미지**: 2048x1024 해상도, 압축 설정
- **상호작용 이미지**: 512x512 이하
- **호러 이미지**: 1024x1024, 고품질 유지

### 10.2 오디오 최적화
- **BGM**: OGG 포맷, 압축률 높음
- **효과음**: WAV 포맷, 압축률 중간
- **환경음**: 루프 가능한 짧은 클립

### 10.3 메모리 관리
- 사용하지 않는 **AudioClip** 언로드
- **이미지 아틀라스** 사용으로 드로우 콜 줄이기
- **오브젝트 풀링** 적용 (호러 이벤트용)

---

## ✅ 11. 스테이지 1 검증 체크리스트

### 11.1 필수 요소 확인
- [ ] **Stage1_HospitalLobby** 데이터 생성됨
- [ ] **StageManager**에 등록됨
- [ ] **배경 이미지** 올바르게 표시됨
- [ ] **4개 상호작용 포인트** 모두 설정됨
  - [ ] 리셉션 데스크 (열쇠)
  - [ ] 의료 캐비닛 (손전등)
  - [ ] 엘리베이터 (다음 스테이지)
  - [ ] CCTV 모니터 (호러 이벤트)
- [ ] **2개 호러 이벤트** 설정됨
- [ ] **BGM 및 환경음** 할당됨

### 11.2 게임플레이 테스트
- [ ] **스테이지 0에서 스테이지 1로 전환** 정상 작동
- [ ] **아이템 수집** 정상 작동
- [ ] **아이템 사용 조건** 올바르게 적용됨
- [ ] **호러 이벤트** 의도한 대로 발생
- [ ] **엘리베이터 상호작용** → 다음 스테이지로 이동
- [ ] **오디오 전환** 자연스럽게 작동

### 11.3 사용자 경험 테스트
- [ ] **직관적인 상호작용** 가능
- [ ] **적절한 난이도** (너무 쉽지도 어렵지도 않음)
- [ ] **분위기 조성** 효과적
- [ ] **메시지 및 힌트** 명확함
- [ ] **로딩 시간** 적절함

---

## 🚨 12. 주의사항 및 팁

### 12.1 상호작용 배치
- **자연스러운 위치**: 플레이어가 예상할만한 곳에 배치
- **시각적 힌트**: 미묘한 하이라이트나 색상 변화로 상호작용 가능 암시
- **접근성**: 모든 상호작용이 쉽게 발견될 수 있도록

### 12.2 호러 요소 균형
- **과도하지 않게**: 플레이어를 놀라게 하되 게임을 중단시키지 않도록
- **타이밍**: 긴장이 풀어진 순간에 호러 이벤트 발생
- **선택적 경험**: 모든 호러 이벤트가 필수가 아니도록

### 12.3 진행 플로우
- **명확한 목표**: 플레이어가 무엇을 해야 하는지 항상 알 수 있도록
- **적절한 페이싱**: 너무 빠르거나 느리지 않은 진행 속도
- **만족감**: 각 상호작용 완료 시 성취감 제공

---

## 📞 13. 문제 해결

### 13.1 상호작용이 작동하지 않는 경우
1. **InteractionPoint 데이터** 올바르게 설정되었는지 확인
2. **Raycast Target** 활성화 확인
3. **StageInteractionController** 컴포넌트 설정 확인
4. **EventSystem** 존재 확인

### 13.2 아이템 수집이 작동하지 않는 경우
1. **ItemManager** 초기화 확인
2. **Item Id** 정확히 입력되었는지 확인
3. **Result Type** 올바르게 설정되었는지 확인

### 13.3 스테이지 전환 문제
1. **Build Settings**에 다음 스테이지 추가되었는지 확인
2. **Target Stage Index** 올바른 값인지 확인
3. **StageManager** 정상 작동 확인

### 13.4 호러 이벤트 문제
1. **HorrorEventManager** 활성화 확인
2. **Horror Event Data** 올바르게 할당되었는지 확인
3. **Event Id** 정확히 일치하는지 확인

---

이제 스테이지 1이 완성되었습니다! 

**다음 단계**: [상호작용 시스템 설정 가이드](04_Interaction_System_Setup_Guide.md)  
**구조 확인**: [하이어라키 구조 참조](05_Hierarchy_Structure_Reference.md)

이 가이드를 참고하여 추가 스테이지들을 제작할 수 있습니다. 각 스테이지마다 고유한 테마와 상호작용을 추가하여 게임의 재미를 높여보세요.