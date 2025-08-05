# 🔄 레거시 시스템 마이그레이션 가이드 (Scripts 최적화 반영)

## 📋 **마이그레이션 개요**

기존 시스템에서 **고급 픽셀 퍼펙트 스테이지 시스템**으로의 완전한 업그레이드 가이드입니다.

---

## 🔄 **시스템 변경 사항**

### **컴포넌트 변경**
```yaml
이전 시스템 → 새로운 시스템:
  StageInteractionController → PixelPerfectInteractionController
  StageManager → AdvancedStageManager
  기본 UI 시스템 → StageNavigationUI

추가된 시스템:
  + 픽셀 퍼펙트 클릭 감지
  + 자동 상태 저장/복원
  + 스테이지 네비게이션 UI
  + 스테이지 잠금/해제 시스템
```

### **파일 구조 변경**
```
Assets/Scripts/StageSystem/Scripts/
├── StageInteractionController.cs (레거시 - 호환성 유지)
├── PixelPerfectInteractionController.cs ⭐ (새로 추가)
├── StageManager.cs (레거시 - 호환성 유지)
├── AdvancedStageManager.cs ⭐ (새로 추가)
└── StageData.cs (확장됨 - InteractionResult 업데이트)

Assets/Scripts/UISystem/Scripts/
└── StageNavigationUI.cs ⭐ (새로 추가)
```

---

## 🚀 **마이그레이션 단계**

### **1단계: 새로운 컴포넌트 추가**

#### **AdvancedStageManager 설정**
1. **기존 StageManager 비활성화** (삭제하지 말고 참조용 유지)
2. **새 GameObject 생성**: "AdvancedStageManager"
3. **AdvancedStageManager 컴포넌트 추가**
4. **설정 복사**:
   ```csharp
   기존 StageManager 설정을 참조하여:
   - Stages 배열 → 동일하게 설정
   - Background Image → 동일하게 설정
   - Stage Canvas → CanvasGroup 컴포넌트로 변경
   - Transition Canvas → CanvasGroup 컴포넌트로 변경
   ```

#### **StageNavigationUI 추가**
1. **UI Canvas에 새 Panel 생성**: "StageNavigationPanel"
2. **StageNavigationUI 컴포넌트 추가**
3. **UI 요소들 생성**:
   - Stage Button Parent
   - Stage Button Prefab
   - Current Stage Text

### **2단계: 상호작용 포인트 업그레이드**

#### **기존 상호작용 포인트 교체**
```csharp
기존 방식:
StageInteractionController (박스 클릭)

새로운 방식:
PixelPerfectInteractionController (이미지 기반)
```

#### **이미지 준비**
```yaml
상호작용 이미지 Import Settings:
✅ Read/Write Enabled: true (필수!)
✅ Format: RGBA32
✅ Alpha Source: From Input
✅ Alpha is Transparency: true
```

#### **컴포넌트 교체 과정**
1. **기존 StageInteractionController 컴포넌트 제거**
2. **PixelPerfectInteractionController 컴포넌트 추가**
3. **Image 컴포넌트 연결** (Target Image 필드)
4. **설정 복사**:
   ```csharp
   - Interaction ID → 동일
   - Display Name → 동일  
   - Description → 동일
   - Required Click Count → 동일
   - Target Stage Index → 동일
   ```

### **3단계: 씬 구조 업데이트**

#### **CanvasGroup 추가**
```
기존 구조:
StageCanvas [Canvas]
TransitionCanvas [Canvas]

새로운 구조:
StageCanvas [Canvas + CanvasGroup]
TransitionCanvas [Canvas + CanvasGroup]
```

#### **InteractionPoints 구조 생성**
```
새로운 구조:
StageCanvas/
├── BackgroundImage
└── InteractionPoints/
    ├── Stage0_InteractionPoints [CanvasGroup]
    ├── Stage1_InteractionPoints [CanvasGroup]
    └── Stage2_InteractionPoints [CanvasGroup]
```

### **4단계: 설정 마이그레이션**

#### **StageData 호환성**
```csharp
기존 StageData:
- 모든 필드 호환됨
- InteractionResult 확장됨 (새 필드 추가)
- 기존 상호작용 데이터 유지됨
```

#### **저장 데이터 마이그레이션**
```csharp
기존 저장 데이터:
- PlayerPrefs 키 변경됨
- 자동 마이그레이션 스크립트 제공 (선택사항)

새로운 저장 형식:
- 더 상세한 상태 정보
- 영구 저장 보장
- 세션간 완벽 복원
```

---

## ⚠️ **호환성 및 주의사항**

### **레거시 지원**
```yaml
호환성 유지:
✅ 기존 StageData 에셋 완전 호환
✅ 기존 스크립트 파일 유지 (삭제 안함)
✅ 점진적 마이그레이션 가능

주의사항:
⚠️ 두 시스템 동시 사용 불가
⚠️ CanvasGroup 컴포넌트 필수 추가
⚠️ 이미지 Read/Write 설정 필수
```

### **성능 영향**
```yaml
메모리 사용량:
+ 약 100KB 추가 (마스크 데이터)
+ PlayerPrefs 저장 데이터 증가

CPU 성능:
+ 픽셀 체크로 약간의 오버헤드
+ 미리 계산된 마스크로 최적화
+ 전체적으로 성능 향상
```

---

## 🔧 **마이그레이션 체크리스트**

### **필수 작업**
- [ ] AdvancedStageManager 추가 및 설정
- [ ] StageNavigationUI 추가 및 설정
- [ ] CanvasGroup 컴포넌트 추가
- [ ] 상호작용 이미지 Import Settings 수정
- [ ] PixelPerfectInteractionController로 교체
- [ ] InteractionPoints 구조 생성

### **선택적 작업**
- [ ] 기존 StageManager 제거 (참조 확인 후)
- [ ] 레거시 저장 데이터 마이그레이션
- [ ] 기존 StageInteractionController 제거
- [ ] 문서 및 주석 업데이트

### **테스트 확인**
- [ ] 픽셀 퍼펙트 클릭 감지 테스트
- [ ] 스테이지 전환 테스트
- [ ] 상태 저장/복원 테스트
- [ ] M키 네비게이션 테스트
- [ ] 성능 테스트

---

## 🎯 **마이그레이션 후 혜택**

### **개발자 혜택**
- ✅ **정확한 클릭 감지**: 복잡한 모양도 완벽 인식
- ✅ **자동 관리**: 스테이지 생성/제거 자동화
- ✅ **상태 보존**: 모든 진행 상황 영구 저장
- ✅ **쉬운 확장**: 새 스테이지 추가 간편화

### **플레이어 혜택**
- ✅ **향상된 UX**: 정확한 상호작용으로 몰입감 증대
- ✅ **편리한 탐험**: M키로 자유로운 스테이지 이동
- ✅ **진행 보존**: 게임 재시작해도 상태 유지
- ✅ **부드러운 전환**: 페이드 효과로 자연스러운 이동

---

## 📞 **마이그레이션 지원**

### **문제 해결**
```yaml
일반적인 문제들:
Q: 클릭이 안됨
A: Read/Write Enabled 확인

Q: 이미지가 안보임  
A: CanvasGroup 설정 확인

Q: 상태가 저장 안됨
A: AdvancedStageManager 활성화 확인

Q: 전환 효과가 안됨
A: Transition Canvas의 CanvasGroup 확인
```

### **점진적 마이그레이션**
1. **테스트 씬 생성**으로 새 시스템 검증
2. **하나씩 교체**하여 안정성 확보
3. **기존 시스템 백업** 후 완전 교체

**완벽한 마이그레이션으로 차세대 상호작용 시스템을 경험하세요!** 🚀✨