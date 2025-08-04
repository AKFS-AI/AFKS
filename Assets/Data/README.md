# 📊 Data 리소스 관리 가이드

## 📁 폴더 구조

```
Data/
├── ScriptableObjects/      # SO 데이터 파일들
│   ├── Stages/            # 스테이지 데이터
│   ├── Items/             # 아이템 데이터
│   ├── Horror/            # 공포 이벤트 데이터
│   ├── Audio/             # 오디오 설정
│   └── Config/            # 게임 설정
├── JSON/                  # JSON 데이터 (필요시)
├── Localization/          # 다국어 데이터
└── Settings/              # 에디터 설정
```

## 🎯 ScriptableObject 관리

### 스테이지 데이터 (Stages/)
```
Stage01_HospitalFront.asset
Stage02_HospitalLobby.asset
Stage03_ManagementRoom.asset
Stage04_DeliveryRoom.asset
Stage05_NurseryRoom.asset
Stage06_PrayerRoom.asset
```

### 아이템 데이터 (Items/)
```
Item_Cross.asset
Item_Permit.asset
Item_Letter.asset
Item_Flashlight.asset
Item_Keys.asset
Item_Report.asset
Item_Blueprint.asset
Item_UltrasoundPhoto.asset
Item_BloodTest.asset
Item_ConfessionNote.asset
```

### 공포 이벤트 데이터 (Horror/)
```
Horror_LobbyGhost.asset
Horror_CCTVGhost.asset
Horror_BodyFall.asset
Horror_BabyGhost.asset
Horror_HospitalDirector.asset
```

## 📋 네이밍 규칙

### ScriptableObject
- `{타입}{번호:00}_{영문이름}.asset`
- 예: `Stage01_HospitalFront.asset`
- 예: `Item_Cross.asset`
- 예: `Horror_LobbyGhost.asset`

### 설정 파일
- `{시스템명}Config.asset`
- 예: `GameConfig.asset`, `AudioConfig.asset`