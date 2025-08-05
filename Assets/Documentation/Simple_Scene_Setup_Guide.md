# 🎮 간단한 씬 설정 가이드 (10분 설정)

## 🎯 **개요**

AFKS 게임을 위한 **최소한의 씬 구성** 가이드입니다.
복잡한 설정 없이 **10분 안에 완료**할 수 있습니다.

---

## 📋 **필요한 씬**

### 1. MainMenu 씬 (메인 메뉴)
### 2. GameplayScene 씬 (게임플레이)

---

## ⚡ **STEP 1: GameplayScene 설정 (5분)**

### 기본 매니저들 추가
```yaml
빈 GameObject들 생성:
1. "MANAGERS" (빈 오브젝트, 정리용)
   ├── GameManager (GameManager.cs 추가)
   ├── ItemManager (ItemManager.cs 추가)
   ├── StageManager (StageManager.cs 추가)
   └── UIManager (UIManager.cs 추가)
```

### Canvas 설정
```yaml
1. UI > Canvas 생성
2. 이름: "MainCanvas"
3. Canvas 설정:
   - Render Mode: Screen Space - Overlay
   - Canvas Scaler 추가:
     - UI Scale Mode: Scale With Screen Size
     - Reference Resolution: 1920x1080
```

### 기본 UI 추가 (선택사항)
```yaml
MainCanvas 하위에:
1. Text (TMP): "게임 타이틀"
2. Button: "설정" (나중에 연결)
3. Text (TMP): 아이템 상태 표시 (선택사항)
```

---

## ⚡ **STEP 2: MainMenu 씬 설정 (3분)**

### Canvas 설정
```yaml
1. UI > Canvas 생성
2. 이름: "MenuCanvas"  
3. Canvas Scaler 설정 (위와 동일)
```

### 메뉴 UI 추가
```yaml
MenuCanvas 하위에:
1. Text (TMP): "AFKS" (게임 제목)
2. Button: "게임 시작"
3. Button: "설정" (선택사항)
4. Button: "종료"
```

### SceneController 추가
```yaml
1. 빈 GameObject: "SceneController"
2. 컴포넌트 추가: SceneController.cs
3. 버튼들과 연결:
   - 게임 시작 → LoadGameScene()
   - 종료 → QuitGame()
```

---

## ⚡ **STEP 3: 씬 연결 (2분)**

### Build Settings
```yaml
1. File > Build Settings
2. Add Open Scenes:
   - MainMenu (index: 0)
   - GameplayScene (index: 1)
3. 순서 확인 후 닫기
```

### SceneController 설정
```yaml
Inspector에서:
- Main Menu Scene Index: 0
- Gameplay Scene Index: 1
```

---

## 🎮 **버튼 이벤트 연결**

### 메인 메뉴 버튼들
```yaml
게임 시작 버튼:
1. Button 선택 > On Click() > +
2. SceneController 드래그
3. Function: LoadGameScene()

종료 버튼:
1. Button 선택 > On Click() > +  
2. SceneController 드래그
3. Function: QuitGame()
```

---

## 🔧 **기본 스크립트 설정**

### GameManager 기본 설정
```yaml
Inspector:
- Target Frame Rate: 60
- Enable Auto Save: ☑️
- Game State: Playing
```

### ItemManager 기본 설정
```yaml
Inspector:
- Has Key: ☐ (게임 시작 시 false)
- Has Flashlight: ☐ (게임 시작 시 false)
- Auto Save: ☑️
```

---

## 🎨 **간단한 아이템 표시 UI (선택사항)**

### UI 요소 추가
```yaml
MainCanvas 하위에:
1. Panel: "ItemStatusPanel"
   ├── Text (TMP): "아이템 상태"
   ├── Text (TMP): "열쇠: 없음"  
   └── Text (TMP): "손전등: 없음"
```

### 간단한 UI 스크립트
```csharp
public class SimpleItemDisplay : MonoBehaviour
{
    [SerializeField] private Text keyStatusText;
    [SerializeField] private Text flashlightStatusText;
    
    void Update()
    {
        if (ItemManager.Instance != null)
        {
            keyStatusText.text = ItemManager.Instance.HasKey ? "열쇠: 있음" : "열쇠: 없음";
            flashlightStatusText.text = ItemManager.Instance.HasFlashlight ? "손전등: 있음" : "손전등: 없음";
        }
    }
}
```

---

## 🧪 **테스트 방법**

### 1. 기본 동작 테스트
```yaml
1. MainMenu 씬에서 Play
2. "게임 시작" 버튼 클릭
3. GameplayScene으로 이동 확인
4. ItemManager Inspector에서 체크박스 테스트
```

### 2. 아이템 테스트
```yaml
1. Play 모드에서 ItemManager 찾기
2. Inspector에서 "Has Key" 체크박스 클릭
3. UI에 "열쇠: 있음" 표시 확인
4. 게임 재시작 후 상태 유지 확인
```

---

## ⚠️ **주의사항**

### 필수 확인사항
- ✅ 모든 Manager들이 DontDestroyOnLoad 설정됨
- ✅ ItemManager.Instance 접근 가능
- ✅ 씬 전환 시 매니저들 유지됨
- ✅ 저장/로드 정상 작동

### 자주 하는 실수
- ❌ Build Settings에 씬 추가 안함
- ❌ 버튼 OnClick 이벤트 연결 안함
- ❌ Canvas Scaler 설정 안함

---

## 🎯 **다음 단계**

### 게임 콘텐츠 추가
1. **[Item_System_Guide.md](Item_System_Guide.md)** 참조하여 아이템 상호작용 추가
2. 스테이지별 배경 이미지 추가
3. 클릭 가능한 오브젝트들 배치
4. 사운드 효과 추가 (선택사항)

### 추가 기능 (선택사항)
- 설정 메뉴 (볼륨 조절 등)
- 저장/로드 UI
- 게임 진행률 표시

---

**🎉 10분 안에 기본 씬 설정 완료! 이제 게임 콘텐츠 제작을 시작하세요!**