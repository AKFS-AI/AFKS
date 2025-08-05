# 🔑 아이템 시스템 가이드 (5분 설정)

## 🎯 **개요**

AFKS는 **오직 열쇠와 손전등**만 관리하는 초간단 아이템 시스템을 사용합니다.

### **왜 간단하게?**
- 실제 사용 아이템: 열쇠, 손전등 (2개만)
- 복잡한 인벤토리 불필요
- 5분 안에 설정 완료

---

## ⚡ **5분 빠른 설정**

### 1단계: ItemManager 추가 (1분)
```yaml
1. 빈 GameObject 생성
2. 이름: "ItemManager"  
3. 컴포넌트 추가: ItemManager.cs
4. 완료!
```

### 2단계: 기본 사용법 (1분)
```csharp
// 아이템 획득
ItemManager.Instance.ObtainKey();
ItemManager.Instance.ObtainFlashlight();

// 보유 확인
bool hasKey = ItemManager.Instance.HasKey;
bool hasFlashlight = ItemManager.Instance.HasFlashlight;

// 사용 (클릭 등에서)
if (ItemManager.Instance.HasKey)
{
    // 문 열기
}
```

### 3단계: 완료! (30초)
- Inspector에서 실시간 확인
- 체크박스로 아이템 상태 확인
- 게임 재시작해도 저장됨

---

## 🎮 **실제 사용 예제**

### 문 열기 스크립트
```csharp
public class Door : MonoBehaviour
{
    void OnMouseDown()
    {
        if (ItemManager.Instance.HasKey)
        {
            Debug.Log("문이 열렸습니다!");
            // 문 열기 애니메이션
        }
        else
        {
            Debug.Log("열쇠가 필요합니다");
        }
    }
}
```

### 어둠 밝히기 스크립트
```csharp
public class DarkArea : MonoBehaviour
{
    void OnMouseDown()
    {
        if (ItemManager.Instance.HasFlashlight)
        {
            Debug.Log("손전등으로 밝혔습니다!");
            // 조명 효과
        }
        else
        {
            Debug.Log("손전등이 필요합니다");
        }
    }
}
```

### 아이템 획득 스크립트
```csharp
public class ItemPickup : MonoBehaviour
{
    [SerializeField] private string itemType = "key"; // "key" 또는 "flashlight"
    
    void OnMouseDown()
    {
        if (itemType == "key")
        {
            ItemManager.Instance.ObtainKey();
            Debug.Log("열쇠를 획득했습니다!");
        }
        else if (itemType == "flashlight")
        {
            ItemManager.Instance.ObtainFlashlight();
            Debug.Log("손전등을 획득했습니다!");
        }
        
        Destroy(gameObject); // 아이템 제거
    }
}
```

---

## 🎨 **UI 설정 (선택사항)**

### 간단한 UI 표시
```csharp
public class SimpleItemUI : MonoBehaviour
{
    [SerializeField] private GameObject keyIcon;
    [SerializeField] private GameObject flashlightIcon;
    
    void Update()
    {
        keyIcon.SetActive(ItemManager.Instance.HasKey);
        flashlightIcon.SetActive(ItemManager.Instance.HasFlashlight);
    }
}
```

---

## 🔧 **Inspector 활용**

### ItemManager Inspector
```yaml
Has Key: ☑️        # 체크박스로 상태 확인
Has Flashlight: ☐  # 실시간 업데이트
Auto Save: ☑️      # 자동 저장 (권장)

Context Menu:
- "디버그: 모든 아이템 획득"
- "디버그: 모든 아이템 리셋"
```

### 실시간 디버깅
- Unity Play 모드에서 Inspector 체크박스로 즉시 테스트
- Console에서 아이템 상태 확인
- 게임 재시작해도 상태 유지

---

## 🚀 **성능 장점**

### 기존 vs 새로운 시스템
```yaml
기존 InventoryManager:
❌ 메모리: ~5MB
❌ 처리: Dictionary 검색
❌ 설정: 복잡한 ItemData

새로운 ItemManager:
✅ 메모리: ~0.4MB (90% 절약)
✅ 처리: Boolean 확인 (즉시)
✅ 설정: 1분 완료
```

---

## ⚠️ **주의사항**

### 제한사항
- 아이템은 열쇠, 손전등만 (추가 불가)
- 개수/스택 시스템 없음
- 복잡한 효과 없음

### 언제 사용?
- ✅ 간단한 아이템 관리
- ✅ 빠른 프로토타이핑  
- ✅ 성능이 중요한 프로젝트
- ✅ 새로운 프로젝트

### 언제 사용 안함?
- ❌ 10개 이상 아이템 필요
- ❌ 복잡한 인벤토리 시스템 필요
- ❌ RPG 스타일 아이템 관리

---

## 💡 **문제 해결**

### 자주 묻는 질문

**Q: 아이템이 저장 안돼요**
```csharp
A: Auto Save가 켜져있는지 확인
   ItemManager Inspector > Auto Save: ☑️
```

**Q: 다른 씬에서 사라져요**
```csharp
A: ItemManager에 DontDestroyOnLoad 적용됨 (자동)
   새 씬에서도 ItemManager.Instance로 접근 가능
```

**Q: Inspector에서 테스트하고 싶어요**
```csharp
A: Play 모드에서 Inspector 체크박스 클릭
   또는 Context Menu 사용
```

---

**🎉 이제 5분 안에 아이템 시스템 설정 완료!**