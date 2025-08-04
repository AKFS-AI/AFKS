# 🎮 1스테이지 완전 작동 가이드

## 🎯 **목표: 완전히 작동하는 1스테이지 구현**

### ✅ **완료 후 결과**
- MainMenu → GameplayScene 씬 전환 ✅
- 1스테이지 배경 이미지 표시 ✅
- 기본 상호작용 포인트 작동 ✅
- 인벤토리 시스템 작동 ✅
- 스테이지 시스템 완전 가동 ✅

---

## 📋 **STEP 1: 버튼 이벤트 연결** (필수 기본 작업)

### 1.1 MainMenu 씬에서 버튼 이벤트 설정

#### StartButton 설정
1. **MainMenu 씬 열기**
2. **StartButton 선택** > Inspector > Button 컴포넌트
3. **On Click ()** > **+** 클릭
4. **None (Object)** 필드에 **SceneController GameObject** 드래그
5. **No Function** 드롭다운 > **SceneController > LoadGameScene()** 선택

#### ExitButton 설정
1. **ExitButton 선택** > Inspector > Button 컴포넌트
2. **On Click ()** > **+** 클릭
3. **None (Object)** 필드에 **SceneController GameObject** 드래그
4. **No Function** 드롭다운 > **SceneController > QuitGame()** 선택

#### 테스트
```
PlayMode 진입 > "게임 시작" 클릭 > GameplayScene 전환 확인
```

---

## 📦 **STEP 2: 1스테이지 데이터 생성**

### 2.1 기본 배경 이미지 준비

#### 임시 배경 이미지 생성 (테스트용)
1. **Unity에서 새 이미지 생성**:
   - Project 창 우클릭 > Create > RenderTexture
   - 이름: "Stage1_Background_Temp"
   - Size: 1920x1080

2. **또는 기본 이미지 사용**:
   - Unity 내장 이미지나 색상으로 테스트
   - 색상: 어두운 회색 (#2D2D30)

### 2.2 StageData 에셋 생성

#### StageData 생성
1. **Project 창**에서 **Assets/Resources/Data/Stages** 폴더로 이동
2. **우클릭** > **Create** > **AFKS** > **Stage** > **Stage Data**
3. **파일명**: "Stage01_Hospital_Entrance"

#### Stage1 데이터 설정
```yaml
기본 정보:
  Stage Index: 0
  Stage Name: "병원 정문"
  Stage Description: "오래된 병원의 정문입니다. 어둠 속에서 무언가가 기다리고 있는 것 같습니다."

비주얼:
  Background Image: (준비한 배경 이미지 드래그)
  Ambient Color: (0.8, 0.8, 1.0, 1) - 차가운 푸른 톤
  Enable Dark Mode: true

오디오:
  Background Music: (없으면 null 유지)
  Ambient Sound: (없으면 null 유지)
  Music Volume: 0.7
  Ambient Volume: 0.5

스테이지 흐름:
  Is Unlocked By Default: true
  Next Stage Index: 1
```

### 2.3 기본 상호작용 포인트 추가

#### Interaction Points 설정 (1개만 기본)
```yaml
Interaction Point 1:
  Id: "entrance_door"
  Display Name: "병원 입구"
  Description: "병원 출입문입니다. 클릭하여 조사할 수 있습니다."
  
  Position: (0, 0) - 화면 중앙
  Size: (200, 300) - 세로로 긴 문 모양
  
  Interaction Type: Examine
  Is Enabled: true
  Is Visible: true
  
  Visual Feedback:
    Hover Color: (1, 1, 0, 0.5) - 반투명 노란색
    Hover Scale: 1.1
  
  Results:
    (기본값 유지)
```

---

## 🔧 **STEP 3: StageManager 연결**

### 3.1 GameplayScene에서 StageManager 설정

#### StageManager 컴포넌트 설정
1. **GameplayScene 씬 열기**
2. **Managers > StageManager** 선택
3. **Inspector**에서 다음 설정:

```yaml
스테이지 설정:
  Stages: 
    - Size: 1
    - Element 0: (Stage01_Hospital_Entrance 드래그)
  Current Stage Index: 0

UI 참조:
  Background Image: StageCanvas > BackgroundImage
  Stage Canvas: StageCanvas (Canvas 컴포넌트)
  Transition Canvas: TransitionCanvas (Canvas 컴포넌트)

전환 설정:
  Transition Duration: 1
  Enable Preloading: true
  Max Preload Stages: 2
```

---

## 🎨 **STEP 4: UI 참조 완전 연결**

### 4.1 StageCanvas 확인 및 설정

#### BackgroundImage 컴포넌트 확인
1. **StageCanvas > BackgroundImage** 선택
2. **Image 컴포넌트** 확인:
   ```yaml
   Image Type: Simple
   Preserve Aspect: true
   Set Native Size: false
   ```

#### Canvas 설정 확인
1. **StageCanvas** 선택
2. **Canvas 컴포넌트** 확인:
   ```yaml
   Render Mode: Screen Space - Camera
   Render Camera: Main Camera
   Plane Distance: 10
   Sort Order: -10
   ```

### 4.2 TransitionCanvas 설정 확인
1. **TransitionCanvas** 선택
2. **Canvas 컴포넌트** 확인:
   ```yaml
   Render Mode: Screen Space - Overlay
   Sort Order: 500
   ```

---

## 🎯 **STEP 5: 테스트 및 검증**

### 5.1 기본 작동 테스트

#### 테스트 시나리오
1. **Play Mode 시작**
2. **MainMenu**에서 "게임 시작" 클릭
3. **GameplayScene**으로 전환되는지 확인
4. **Console 메시지 확인**:
   ```
   [StageManager] Loading stage: 병원 정문
   [StageManager] Stage loaded successfully
   ```

### 5.2 1스테이지 기능 테스트

#### 확인 사항
- [ ] 배경 이미지가 정상 표시됨
- [ ] 상호작용 포인트가 생성됨 (Hierarchy에서 확인)
- [ ] 마우스 호버 시 하이라이트 효과
- [ ] 클릭 시 상호작용 반응
- [ ] Tab키로 인벤토리 열기/닫기
- [ ] ESC키로 메뉴 열기/닫기

### 5.3 문제 해결

#### 자주 발생하는 문제들

**문제 1: 배경 이미지가 안 보임**
```
해결: StageManager > Background Image 참조 확인
StageCanvas > BackgroundImage가 올바르게 연결되었는지 체크
```

**문제 2: 상호작용 포인트가 안 생성됨**
```
해결: StageData의 Interaction Points 설정 확인
StageManager > Stage Canvas 참조 확인
```

**문제 3: 씬 전환이 안됨**
```
해결: Build Settings에서 씬 순서 확인
SceneController의 gameSceneName 설정 확인
```

---

## 🎉 **완성 확인 체크리스트**

### ✅ **기본 시스템**
- [ ] MainMenu → GameplayScene 씬 전환
- [ ] SceneController 정상 작동
- [ ] EventSystem 정상 작동

### ✅ **스테이지 시스템**  
- [ ] StageData 에셋 생성 완료
- [ ] StageManager에 스테이지 연결 완료
- [ ] 배경 이미지 정상 표시
- [ ] 상호작용 포인트 생성 확인

### ✅ **UI 시스템**
- [ ] Canvas 구조 정상 작동
- [ ] 인벤토리 시스템 (Tab키)
- [ ] 메뉴 시스템 (ESC키)
- [ ] TextMeshPro 한글 표시

### ✅ **상호작용 시스템**
- [ ] 마우스 호버 효과
- [ ] 클릭 상호작용
- [ ] InteractionPoint 동적 생성

---

## 🚀 **성공!**

이제 **완전히 작동하는 1스테이지**가 완성되었습니다!

### 🎮 **플레이어 경험**
1. **메인메뉴**에서 "게임 시작" 클릭
2. **1스테이지 (병원 정문)** 진입
3. **배경 탐색** 및 **상호작용 포인트 클릭**
4. **인벤토리** 및 **메뉴** 시스템 활용

### 🔧 **확장 가능성**
- 추가 스테이지 데이터 생성
- 더 많은 상호작용 포인트
- 아이템 수집 시스템
- 공포 이벤트 추가
- 오디오 시스템 활용

**1스테이지 기반으로 전체 게임 확장 준비 완료!** 🎉