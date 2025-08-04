# 스테이지 데이터 예제

## 🏥 병원 스테이지 설정 가이드

### 스테이지 1: 병원 앞
```
StageName: "병원 앞"
BackgroundImage: Stage01_Hospital_Front.png
InteractionPoints:
  - ID: "hospital_door"
    Position: (0, -100)
    Size: (200, 300)
    Type: Click
    Result: ChangeStage(1)
```

### 스테이지 2: 병원 로비
```
StageName: "병원 로비"
BackgroundImage: Stage02_Hospital_Lobby.png
InteractionPoints:
  - ID: "cross_pickup"
    Position: (-150, 50)
    Type: Collect
    Result: GiveItem("cross")
  
  - ID: "permit_examine"
    Position: (100, -50)
    Type: Examine
    Result: ShowDialog("병원 개설 허가증...")
  
  - ID: "management_door"
    Position: (200, 0)
    Type: Click
    RequiredItems: ["cross"]
    Result: ChangeStage(2)

HorrorEvents:
  - ID: "lobby_ghost_appear"
    TriggerType: OnStageEnter
    TriggerDelay: 3.0
    JumpscareImage: Ghost_Lobby.png
    HorrorSound: Scream.wav
    DisplayDuration: 2.0
```

### 스테이지 3: 관리실
```
StageName: "관리실"
BackgroundImage: Stage03_Management_Room.png
InteractionPoints:
  - ID: "cctv_monitor"
    Position: (0, 100)
    Type: Click
    Result: TriggerHorrorEvent("cctv_ghost")
  
  - ID: "flashlight_pickup"
    Position: (-100, -150)
    Type: Collect
    Result: GiveItem("flashlight")
  
  - ID: "keys_pickup"
    Position: (150, -100)
    Type: Collect
    Result: GiveItem("keys")

HorrorEvents:
  - ID: "cctv_ghost"
    TriggerType: OnInteraction
    JumpscareImage: CCTV_Ghost.png
    HorrorSound: Static_Sound.wav
    EnableScreenShake: true
    ShakeIntensity: 0.8
```

## 📝 설정 방법

1. Unity에서 StageData ScriptableObject 생성
2. 위 설정값들을 입력
3. StageManager의 stages 리스트에 순서대로 추가
4. 배경 이미지들을 Art/Backgrounds/ 폴더에 배치
5. 각 상호작용 포인트의 위치를 Scene View에서 조정

## 🎭 공포 이벤트 타이밍

- **OnStageEnter**: 스테이지 진입 후 지정된 시간 후 발동
- **OnInteraction**: 특정 오브젝트와 상호작용 시 발동
- **OnTimer**: 스테이지 진입 후 일정 시간 경과 시 발동
- **OnCondition**: 특정 조건 달성 시 발동

## 🔧 디버깅 팁

1. StageInteractionController의 debugMode를 true로 설정
2. 상호작용 영역이 시각적으로 표시됨
3. Scene View에서 상호작용 포인트 위치 조정 가능
4. Console 로그로 상호작용 이벤트 확인 가능