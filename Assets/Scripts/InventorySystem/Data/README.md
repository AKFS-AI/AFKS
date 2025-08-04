# 아이템 데이터 예제

## 🎒 기획서 기반 아이템 목록

### 수집 아이템 (Collectible)

#### 휴대용 십자가
```
ItemID: "cross"
ItemName: "휴대용 십자가"
Description: "작은 금속 십자가. 차가운 느낌이 든다."
ItemType: Collectible
Rarity: Common
IsKeyItem: true
Icon: Cross_Icon.png
DetailImage: Cross_Detail.png
PickupSound: Item_Pickup.wav
```

#### 병원 개설 허가증
```
ItemID: "hospital_permit"
ItemName: "병원 개설 허가증"
Description: "낡은 허가증. 1990년에 발급되었다."
ItemType: Document
Rarity: Uncommon
IsKeyItem: true
Icon: Permit_Icon.png
DetailImage: Permit_Detail.png
```

#### 사고 편지
```
ItemID: "accident_letter"
ItemName: "사고 편지"
Description: "피로 얼룩진 편지. 무슨 사고가 있었던 것 같다."
ItemType: Document
Rarity: Rare
IsKeyItem: true
Icon: Letter_Icon.png
DetailImage: Letter_Detail.png
```

### 도구 아이템 (Tool)

#### 손전등
```
ItemID: "flashlight"
ItemName: "손전등"
Description: "오래된 손전등. 배터리가 얼마 남지 않은 것 같다."
ItemType: Tool
Rarity: Common
IsUsable: true
Icon: Flashlight_Icon.png
DetailImage: Flashlight_Detail.png
UseSound: Flashlight_Click.wav
Effects:
  - EffectType: UnlockArea
    StringValue: "dark_room"
```

#### 열쇠 꾸러미
```
ItemID: "keys"
ItemName: "열쇠 꾸러미"
Description: "여러 개의 열쇠가 달린 꾸러미. 어느 문을 여는 걸까?"
ItemType: Key
Rarity: Uncommon
IsUsable: true
Icon: Keys_Icon.png
DetailImage: Keys_Detail.png
UseSound: Keys_Jingle.wav
Effects:
  - EffectType: UnlockArea
    StringValue: "locked_doors"
```

### 문서 아이템 (Document)

#### 병원 사건사고 서류
```
ItemID: "incident_report"
ItemName: "병원 사건사고 서류"
Description: "여러 사건들이 기록된 서류. 읽기 무서운 내용들이다."
ItemType: Document
Rarity: Epic
IsKeyItem: true
Icon: Report_Icon.png
DetailImage: Report_Detail.png
```

#### 병원 단면도
```
ItemID: "hospital_blueprint"
ItemName: "병원 단면도"
Description: "병원의 구조가 그려진 도면. 숨겨진 방이 있는 것 같다."
ItemType: Document
Rarity: Rare
IsKeyItem: true
Icon: Blueprint_Icon.png
DetailImage: Blueprint_Detail.png
Effects:
  - EffectType: UnlockArea
    StringValue: "secret_room"
```

#### 초음파 사진
```
ItemID: "ultrasound_photo"
ItemName: "초음파 사진"
Description: "이상한 초음파 사진. 뭔가 섬뜩한 느낌이 든다."
ItemType: Document
Rarity: Epic
IsKeyItem: true
Icon: Ultrasound_Icon.png
DetailImage: Ultrasound_Detail.png
```

#### 혈액 검사 자료
```
ItemID: "blood_test"
ItemName: "혈액 검사 자료"
Description: "소수 혈액형에 대한 검사 자료. 무슨 목적이었을까?"
ItemType: Document
Rarity: Legendary
IsKeyItem: true
Icon: BloodTest_Icon.png
DetailImage: BloodTest_Detail.png
```

#### 관계자의 양심 고백 쪽지
```
ItemID: "confession_note"
ItemName: "양심 고백 쪽지"
Description: "참혹한 일을 자책하는 누군가의 메모. 진실에 가까워지고 있다."
ItemType: Document
Rarity: Legendary
IsKeyItem: true
Icon: Note_Icon.png
DetailImage: Note_Detail.png
```

### 특수 아이템 (Special)

#### 기도문
```
ItemID: "prayer_text"
ItemName: "기도문"
Description: "사이비 종교의 기도문. 읽으면 안 될 것 같다."
ItemType: Special
Rarity: Epic
IsKeyItem: true
Icon: Prayer_Icon.png
DetailImage: Prayer_Detail.png
```

#### 병원 개원 사진
```
ItemID: "opening_photo"
ItemName: "병원 개원 사진"
Description: "병원 개원 당시의 기념사진. 모든 진실을 담고 있는 것 같다."
ItemType: Special
Rarity: Legendary
IsKeyItem: true
Icon: Photo_Icon.png
DetailImage: Photo_Detail.png
```

## 🎮 사용법

1. Unity에서 ItemData ScriptableObject 생성
2. 위 정보들을 입력
3. 아이콘 이미지를 Art/Items/ 폴더에 배치
4. InventoryManager의 itemDatabase에 추가

## 🎯 아이템 상호작용

- **수집 시**: PickupSound 재생
- **사용 시**: UseSound 재생 및 Effects 실행
- **검사 시**: DetailImage 및 Description 표시
- **키 아이템**: 특정 지역이나 문 잠금 해제에 필요

## 🔧 확장성

새로운 아이템 추가 시:
1. ItemData.asset 생성
2. 필요한 이미지 리소스 준비
3. 상호작용 로직 구현
4. 스테이지의 InteractionPoint에 연결