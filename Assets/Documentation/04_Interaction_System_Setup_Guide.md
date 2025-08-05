# 🖱️ 상호작용 시스템 설정 가이드

## 🎯 개요
이 가이드는 AFKS 게임의 고급 상호작용 시스템을 설정하는 방법을 설명합니다. 2단계 상호작용, 줌 시스템, 픽셀 퍼펙트 클릭 등을 포함합니다.

---

## 📋 1. 상호작용 시스템 아키텍처

### 1.1 시스템 구성 요소
```
상호작용 시스템
├── 기본 상호작용 (StageInteractionController)
├── 2단계 상호작용 (TwoStageInteractionController)  
├── 픽셀 퍼펙트 상호작용 (PixelInteractionController)
├── 줌 시스템 (ZoomController)
└── 상호작용 관리 (InteractionManager)
```

### 1.2 각 시스템의 역할
- **StageInteractionController**: 일반적인 클릭 상호작용
- **TwoStageInteractionController**: 확대 → 세부 상호작용 순서
- **PixelInteractionController**: 이미지의 투명하지 않은 부분만 클릭 가능
- **ZoomController**: 이미지 확대/축소 관리
- **InteractionManager**: 전역 상호작용 이벤트 관리

---

## 🔍 2. ZoomController 상세 설정

### 2.1 ZoomController 생성 및 배치
1. **GameplayCanvas → 우클릭 → Create Empty**
2. 이름을 `ZoomController`로 변경
3. **Add Component → Scripts → UISystem → ZoomController**
4. **Transform** 설정:
   - **Position**: (0, 0, 0)
   - **Rotation**: (0, 0, 0)
   - **Scale**: (1, 1, 1)

### 2.2 Background Overlay 설정

#### 2.2.1 자동 생성 방식 (권장)
ZoomController가 자동으로 배경 오버레이를 생성하지만, 수동 설정을 원한다면:

#### 2.2.2 수동 생성 방식
1. **ZoomController → 우클릭 → UI → Image**
2. 이름을 `ZoomBackgroundOverlay`로 변경
3. **RectTransform** 설정:
   - **Anchor Presets**: Stretch (Alt+Shift+클릭)
   - **Left, Top, Right, Bottom**: 모두 0
   - **Position**: (0, 0, 0)
4. **Image** 컴포넌트 설정:
   - **Source Image**: None (단색 배경 사용)
   - **Color**: (0, 0, 0, 0.7) - 70% 투명도의 검정
   - **Material**: None
   - **Raycast Target**: ✅ 체크 (클릭 감지용)
5. **Add Component → Button**
6. **Button** 설정:
   - **Interactable**: ✅ 체크
   - **Transition**: None
   - **Navigation**: None
   - **On Click ()**: ZoomController.ZoomOut 연결
7. **GameObject → Set Active**: ❌ 비활성화 (초기 상태)

### 2.3 ZoomController Inspector 설정

**ZoomController** 선택 후 Inspector에서 다음과 같이 설정:

#### 🔧 ZoomController 컴포넌트 상세 설정
```yaml
🔍 줌 설정:
  Max Zoom Scale: 3.0 (1.0~5.0 범위, 최대 확대 배율)
  Zoom Duration: 0.8 (0.1~2.0초 범위, 애니메이션 시간)
  Target Image: [런타임에 설정됨 - 비워둠]
  Background Overlay: [ZoomBackgroundOverlay 할당]

🎨 시각적 설정:
  Overlay Color: (0, 0, 0, 0.7) - 70% 투명도의 검정색
  Zoom Curve: [EaseInOut AnimationCurve]
    - Time 0: Value 0 (In Tangent: 0, Out Tangent: 0)
    - Time 1: Value 1 (In Tangent: 0, Out Tangent: 0)

🔊 오디오:
  Zoom In Sound: [확대 효과음 클립 - 선택사항]
  Zoom Out Sound: [축소 효과음 클립 - 선택사항]
```

> **참고**: 오디오 클립들은 선택사항이며, 할당하지 않아도 기능에는 문제없습니다.

---

## 🎯 3. PixelInteractionController 설정

### 3.1 컴포넌트 추가
1. **상호작용할 이미지** 선택
2. **Add Component → Scripts → StageSystem → PixelInteractionController**

### 3.2 Inspector 설정

#### 🔧 PixelInteractionController 컴포넌트 상세 설정
```yaml
🎯 이미지 기반 설정:
  Target Image: [상호작용할 Image 컴포넌트 할당]
  Alpha Threshold: 0.1 (0.0~1.0 범위, 투명도 임계값)
  Debug Mode: ❌ (클릭 영역 시각화 - 개발 시에만 체크)

⚙️ 상호작용 설정:
  Interaction Id: [고유 ID 입력]
  Display Name: [표시될 이름]
  Description: [설명 입력]
  Required Click Count: 1 (필요한 클릭 횟수)
  Target Stage Index: -1 (이동할 스테이지, -1은 이동 없음)
  Result Message: [결과 메시지]

🎨 시각적 피드백:
  Hover Color: (1, 1, 0, 0.5) - 노란색 반투명
  Hover Scale: 1.1 (호버 시 크기 배율)
  Click Feedback Duration: 0.2 (클릭 피드백 지속시간)

🔊 오디오:
  Hover Sound: [호버 사운드 - 선택사항]
  Click Sound: [클릭 사운드 - 선택사항]

🚀 최적화 설정:
  Use Pre Calculated Mask: ✅ (미리 계산된 마스크 사용)
  Mask Resolution: 128 (마스크 해상도)
```

> **참고**: PixelInteractionController는 이미지의 투명하지 않은 부분만 클릭 가능하게 만드는 정밀한 상호작용을 제공합니다.

---

## 🚪 4. TwoStageInteractionController 설정

### 4.1 컴포넌트 추가
1. **상호작용할 메인 이미지** (예: 철문) 선택
2. **Add Component → Scripts → StageSystem → TwoStageInteractionController**

### 4.2 Inspector 설정

#### 🔧 TwoStageInteractionController 컴포넌트 상세 설정
```yaml
🎯 1단계: 초기 상호작용 (철문):
  Door Image: [메인 상호작용 이미지 할당]
  Door Interaction Id: "hospital_door" 
  Door Display Name: "병원 철문"

🔍 확대 설정:
  Zoom Scale: 2.5 (1.5~4.0 범위)
  Zoom Focus Offset: (0, 0) - Vector2 좌표

⛓️ 2단계: 쇠사슬 상호작용:
  Chain Image: [세부 상호작용 이미지 할당]
  Chain Interaction Id: "hospital_door_chain"
  Chain Display Name: "철문의 쇠사슬"
  Required Chain Clicks: 5
  Next Stage Index: 1

🎨 시각적 피드백:
  Hover Color: (1, 1, 0, 0.3) - 노란색 반투명
  Hover Scale: 1.05 (5% 크기 증가)
  Click Feedback Duration: 0.2 (초)

🔊 오디오:
  Door Click Sound: [철문 클릭 사운드 - 선택사항]
  Chain Click Sound: [쇠사슬 클릭 사운드 - 선택사항]
  Chain Break Sound: [쇠사슬 해제 사운드 - 선택사항]

💬 메시지:
  Door Click Message: "문을 자세히 살펴보자..."
  Chain Progress Message: "쇠사슬을 부수고 있다... ({0}/{1})"
  Door Unlock Message: "쇠사슬이 끊어졌다! 문이 열렸다!"
```

### 4.3 쇠사슬 이미지 설정
1. **메인 이미지 → 우클릭 → UI → Image**
2. 이름을 `ChainImage`로 변경
3. **RectTransform** 설정:
   - **Anchor**: Center
   - **Position**: 실제 쇠사슬 위치에 맞게 조정
   - **Width/Height**: 쇠사슬 크기에 맞게 설정
4. **Image** 설정:
   - **Source Image**: 쇠사슬 이미지
   - **Preserve Aspect**: ✅ 체크
   - **Raycast Target**: ✅ 체크
5. **GameObject → Set Active**: ❌ 비활성화 (1단계에서는 숨김)

---

## 🎯 4. PixelInteractionController 설정

### 4.1 사용 시나리오
정밀한 클릭이 필요한 경우:
- 복잡한 이미지에서 특정 부분만 클릭 가능
- 투명 영역 클릭 방지
- 불규칙한 모양의 상호작용 영역

### 4.2 컴포넌트 설정
1. **Image 컴포넌트가 있는 GameObject** 선택
2. **Add Component → Scripts → StageSystem → PixelInteractionController**

### 4.3 텍스처 설정 (중요!)
1. **상호작용할 이미지** 선택 (Project 창에서)
2. **Inspector → Import Settings**:
   - **Read/Write Enabled**: ✅ **반드시 체크**
   - **Apply** 버튼 클릭
3. **Advanced** 설정:
   - **Non-Power of 2**: None
   - **Generate Mip Maps**: ❌ 체크 해제

### 4.4 PixelInteractionController Inspector 설정

#### 4.4.1 이미지 기반 설정
- **Target Image**: 상호작용할 Image 컴포넌트
- **Alpha Threshold**: 0.1 (투명도 임계값)
- **Debug Mode**: ✅ 체크 (개발 중에만)

#### 4.4.2 상호작용 설정
- **Interaction Id**: 고유 식별자
- **Display Name**: 표시 이름
- **Description**: 설명 텍스트
- **Required Click Count**: 필요 클릭 횟수
- **Target Stage Index**: 목표 스테이지 번호
- **Result Message**: 결과 메시지

#### 4.4.3 시각적 피드백
- **Hover Color**: 호버 시 색상 변화
- **Hover Scale**: 호버 시 크기 변화
- **Click Feedback Duration**: 클릭 피드백 시간

#### 4.4.4 최적화 설정
- **Use Pre Calculated Mask**: ✅ 체크 (성능 향상)
- **Mask Resolution**: 128 (해상도와 성능 균형)

---

## ⚙️ 5. StageInteractionController (기본) 설정

### 5.1 사용 시나리오
- 단순한 클릭 상호작용
- 아이템 수집
- 메시지 표시
- 스테이지 전환

### 5.2 InteractionPoint 데이터 설정

#### 5.2.1 데이터 생성 방법
**방법 1: ScriptableObject 생성** (권장)
1. **Assets → Create → AFKS → Stage → Interaction Point Data**
2. 이름을 `ReceptionDesk_InteractionPoint`로 설정

**방법 2: Inspector에서 직접 설정**
StageInteractionController → **Interaction Data** 직접 입력

#### 5.2.2 기본 정보
- **Id**: "reception_desk" (고유 식별자)
- **Display Name**: "리셉션 데스크"
- **Description**: "오래된 리셉션 데스크입니다."

#### 5.2.3 위치 및 크기
- **Position**: (-200, -100) (UI 좌표)
- **Size**: (300, 200) (상호작용 영역 크기)

#### 5.2.4 상호작용 타입
- **Interaction Type**: Click
- **Is Enabled**: ✅ 체크
- **Is Visible**: ✅ 체크

#### 5.2.5 시각적 피드백
- **Hover Sprite**: 호버 시 표시할 이미지 (선택사항)
- **Hover Color**: (1, 1, 0, 0.5) - 반투명 노란색
- **Hover Scale**: 1.1 (10% 크기 증가)

#### 5.2.6 오디오
- **Interaction Sound**: 상호작용 효과음
- **Hover Sound**: 호버 효과음 (선택사항)

#### 5.2.7 조건
- **Required Items**: ["key"] (필요한 아이템 목록)
- **Enable Conditions**: 활성화 조건들

#### 5.2.8 결과 설정
**InteractionResult** 설정:
- **Result Type**: 1 (아이템 지급)
- **Message**: "열쇠를 발견했습니다!"
- **Give Item**: ✅ 체크
- **Item Id**: "key"
- **Sound Effect**: 아이템 획득 효과음

---

## 🎮 6. 상호작용 플로우 설계

### 6.1 기본 플로우
```
플레이어 클릭
    ↓
EventSystem에서 감지
    ↓
해당 Controller의 OnPointerClick 호출
    ↓
CanInteract() 검사
    ↓
ExecuteInteraction() 실행
    ↓
결과 처리 (아이템, 스테이지 변경 등)
```

### 6.2 2단계 상호작용 플로우
```
1단계: 메인 객체 클릭
    ↓
ZoomController.ZoomToImage() 호출
    ↓
확대 애니메이션 실행
    ↓
2단계 객체 활성화
    ↓
2단계: 세부 객체 멀티 클릭
    ↓
클릭 카운터 증가
    ↓
목표 달성 시 결과 실행
    ↓
ZoomController.ZoomOut() 호출
    ↓
다음 스테이지로 이동
```

---

## 🔧 7. 고급 설정 및 팁

### 7.1 TwoStageInteractionData 활용
더 쉬운 설정을 위해 ScriptableObject 데이터 사용:

1. **Assets → Create → AFKS → Stage → Two Stage Interaction Data**
2. 이름을 `HospitalDoor_TwoStageData`로 설정
3. **Inspector에서 모든 설정 완료**
4. **TwoStageInteractionController**에서 **Load From Data** 사용

### 7.2 성능 최적화

#### 7.2.1 PixelInteractionController 최적화
- **Mask Resolution**: 128 (기본값, 성능과 정확도 균형)
- **Use Pre Calculated Mask**: ✅ 체크 (런타임 성능 향상)
- **텍스처 크기**: 가능한 한 작게 (512x512 이하 권장)

#### 7.2.2 ZoomController 최적화
- **Animation Curve**: 미리 베이크된 커브 사용
- **Background Overlay**: 단순한 색상 사용 (이미지 대신)
- **Update 호출 최소화**: 애니메이션 중에만 활성화

### 7.3 디버깅 도구

#### 7.3.1 Debug Mode 활용
- **PixelInteractionController**: 클릭 가능 영역 시각화
- **TwoStageInteractionController**: 상호작용 단계 로그
- **ZoomController**: 줌 상태 정보 표시

#### 7.3.2 콘솔 로그 확인
```csharp
// 상호작용 시스템 로그 예시
[TwoStageInteraction] 1단계 실행: 병원 철문 클릭
[ZoomController] 줌 확대 완료: HospitalDoorImage
[TwoStageInteraction] 2단계 실행: 철문의 쇠사슬 클릭 (3/5)
[TwoStageInteraction] 상호작용 완료: 병원 철문
```

---

## ✅ 8. 설정 검증 체크리스트

### 8.1 ZoomController 검증
- [ ] **ZoomController** 컴포넌트 추가됨
- [ ] **Background Overlay** 설정됨
- [ ] **줌 사운드** 할당됨
- [ ] **Animation Curve** 설정됨
- [ ] **Max Zoom Scale** 적절히 설정됨 (2.0 ~ 3.0)

### 8.2 TwoStageInteractionController 검증
- [ ] **Door Image** 할당됨
- [ ] **Chain Image** 할당됨 및 비활성화됨
- [ ] **Required Chain Clicks** 설정됨 (3 ~ 7 권장)
- [ ] **Next Stage Index** 올바른 값
- [ ] **모든 사운드** 할당됨
- [ ] **메시지** 작성됨

### 8.3 PixelInteractionController 검증
- [ ] **Target Image** 텍스처에서 **Read/Write Enabled** ✅
- [ ] **Alpha Threshold** 적절히 설정됨 (0.1 ~ 0.5)
- [ ] **Interaction Id** 고유함
- [ ] **Use Pre Calculated Mask** ✅ 체크됨

### 8.4 기능 테스트
- [ ] **기본 클릭** 상호작용 작동
- [ ] **2단계 상호작용** 순서대로 진행
- [ ] **줌 인/아웃** 정상 작동
- [ ] **ESC 키** 줌 해제 작동
- [ ] **배경 클릭** 줌 해제 작동
- [ ] **픽셀 퍼펙트 클릭** 정확히 감지
- [ ] **사운드** 모두 재생됨
- [ ] **메시지** 올바르게 표시됨

---

## 🚨 9. 주의사항 및 문제 해결

### 9.1 주의사항

#### 9.1.1 PixelInteractionController 사용 시
- **반드시 텍스처 Read/Write 활성화** 필요
- **텍스처 크기가 클수록 메모리 사용량 증가**
- **투명도 임계값 조정**으로 클릭 감도 조절

#### 9.1.2 ZoomController 사용 시
- **한 번에 하나의 이미지만 줌 가능**
- **Canvas Scaler 설정에 영향** 받을 수 있음
- **Background Overlay 필수** (클릭 해제용)

#### 9.1.3 TwoStageInteractionController 사용 시
- **쇠사슬 이미지 정확한 위치** 설정 중요
- **줌 중심점 조정**으로 사용자 경험 개선
- **클릭 횟수 밸런싱** 필요 (너무 많거나 적지 않게)

### 9.2 문제 해결

#### 9.2.1 줌이 작동하지 않는 경우
1. **ZoomController.Instance** null 체크
2. **Target Image** 올바르게 할당되었는지 확인
3. **Canvas 설정** 문제 없는지 확인
4. **Background Overlay** 올바르게 생성되었는지 확인

#### 9.2.2 픽셀 클릭이 정확하지 않은 경우
1. **텍스처 Read/Write** 활성화 확인
2. **Alpha Threshold** 값 조정 (0.1 ~ 0.5)
3. **텍스처 압축 설정** 확인
4. **Mask Resolution** 높이기 (128 → 256)

#### 9.2.3 2단계 상호작용 문제
1. **Chain Image Active** 상태 확인 (초기에는 비활성화)
2. **Zoom Focus Offset** 적절히 설정되었는지 확인
3. **Required Clicks** 0보다 큰 값인지 확인
4. **Next Stage Index** 유효한 스테이지인지 확인

#### 9.2.4 일반적인 상호작용 문제
1. **EventSystem** 존재 확인
2. **Raycast Target** 활성화 확인
3. **Canvas GraphicRaycaster** 컴포넌트 확인
4. **Image 컴포넌트** 존재 및 설정 확인

---

## 📚 10. 추가 리소스 및 확장

### 10.1 유용한 스크립트 스니펫

#### 10.1.1 ZoomController 상태 확인
```csharp
// 현재 줌 상태 확인
if (ZoomController.Instance != null)
{
    bool isZoomed = ZoomController.Instance.IsZoomed;
    bool isZooming = ZoomController.Instance.IsZooming;
}
```

#### 10.1.2 TwoStageInteraction 이벤트 구독
```csharp
// 이벤트 구독 예시
TwoStageInteractionController.OnDoorClicked.AddListener(OnDoorClickedHandler);
TwoStageInteractionController.OnChainClicked.AddListener(OnChainClickedHandler);
TwoStageInteractionController.OnDoorUnlocked.AddListener(OnDoorUnlockedHandler);
```

### 10.2 확장 가능한 기능들
- **3단계 상호작용**: 줌 → 서브 줌 → 마이크로 상호작용
- **애니메이션 연동**: 상호작용과 함께 객체 애니메이션
- **멀티터치 지원**: 모바일 기기용 확장
- **접근성 개선**: 키보드 네비게이션 지원

---

이제 모든 상호작용 시스템이 완성되었습니다! 

**구조 확인**: [하이어라키 구조 참조](05_Hierarchy_Structure_Reference.md)

이 가이드를 참고하여 다양하고 흥미로운 상호작용을 만들어보세요.