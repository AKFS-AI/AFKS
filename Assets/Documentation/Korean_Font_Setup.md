# 🇰🇷 한글 폰트 설정 가이드 (3분)

## 🎯 **개요**

Unity TextMeshPro에서 한글 폰트를 **3분 안에** 설정하는 가이드입니다.

---

## ⚡ **3분 빠른 설정**

### 1단계: TextMeshPro 설치 (30초)
```yaml
1. Window > TextMeshPro > Import TMP Essential Resources
2. 모든 옵션 Import 클릭
3. 완료!
```

### 2단계: 한글 폰트 준비 (30초)
```yaml
추천 무료 폰트:
- 나눔고딕 (NanumGothic.ttf)
- 맑은고딕 (Windows 기본)
- Source Han Sans (Google Fonts)

폰트 위치:
- Assets/Fonts/ 폴더 생성 후 복사
```

### 3단계: Font Asset 생성 (2분)
```yaml
1. Window > TextMeshPro > Font Asset Creator
2. 설정:
   - Source Font File: [한글 폰트 선택]
   - Sampling Point Size: 32
   - Character Set: Unicode Range (Hex)
   - Character Sequence (Hex): 
     AC00-D7AF, 0020-007F
     (한글 완성형 + 영문/숫자)
3. Generate Font Atlas 클릭 (10초 대기)
4. Save 버튼 > Assets/Fonts/Korean_Font_Asset
5. 완료!
```

---

## 🎮 **사용법**

### 텍스트 생성
```yaml
1. UI > Text - TextMeshPro 선택
2. Inspector > Text (TMP) 컴포넌트
3. Font Asset: Korean_Font_Asset 선택
4. Text: "안녕하세요" 입력
5. 한글 표시 확인!
```

### 코드에서 사용
```csharp
using TMPro;

public class TextExample : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textUI;
    
    void Start()
    {
        textUI.text = "게임 시작!";
        textUI.fontSize = 24;
        textUI.color = Color.white;
    }
}
```

---

## 🎨 **추천 설정**

### 일반 UI 텍스트
```yaml
Font Size: 18-24
Color: White 또는 Black
Auto Size: 비활성화
Best Fit: 비활성화 (성능상)
```

### 제목 텍스트
```yaml
Font Size: 36-48
Color: 강조 색상
Font Style: Bold (한글에서 잘 보임)
```

### 작은 설명 텍스트
```yaml
Font Size: 14-16
Color: 회색 (#808080)
Line Spacing: 1.2 (가독성)
```

---

## ⚠️ **주의사항**

### 성능 최적화
- ✅ Font Atlas 크기는 1024x1024 권장
- ✅ 사용하지 않는 문자는 제외
- ✅ 여러 폰트보다 하나 폰트로 통일

### 자주 하는 실수
- ❌ Character Set을 Custom으로 설정 (복잡함)
- ❌ Sampling Point Size를 너무 크게 설정 (성능 저하)
- ❌ Font Asset을 Scene에 직접 배치 (Resources 사용)

---

## 🔧 **문제 해결**

### 한글이 안보여요
```yaml
확인사항:
1. Font Asset에 한글 문자가 포함되었는지 확인
2. Character Sequence에 AC00-D7AF 포함 확인
3. Generate Font Atlas 성공했는지 확인
```

### 글자가 깨져요
```yaml
해결방법:
1. Sampling Point Size 늘리기 (32 → 48)
2. Atlas Resolution 늘리기 (1024 → 2048)
3. 다른 한글 폰트 시도
```

### 성능이 느려요
```yaml
최적화:
1. Font Atlas 크기 줄이기
2. 사용 안하는 문자 제거
3. 여러 Font Asset 통합
```

---

## 📱 **모바일 최적화**

### Android/iOS 권장 설정
```yaml
Font Atlas Resolution: 1024x1024 (최대)
Sampling Point Size: 24-32
Character Count: 최소화
Compression: Enable
```

### 메모리 절약법
```yaml
1. 영문/숫자만: 0020-007F (128KB)
2. 기본 한글: AC00-D7AF + 0020-007F (512KB)
3. 전체 한글: 모든 문자 (2MB+)

게임에 맞는 최소 문자셋 선택 권장
```

---

## 💡 **고급 팁**

### 동적 폰트 생성 (선택사항)
```csharp
// 런타임에 없는 문자 추가
TMP_FontAsset fontAsset = textComponent.font;
fontAsset.TryAddCharacter('새'); // 새로운 문자 추가
```

### 여러 언어 지원
```yaml
Font Asset별 분리:
- Korean_Font: 한글 전용
- English_Font: 영문 전용
- Number_Font: 숫자 전용

필요에 따라 동적 교체
```

---

**🎉 3분 안에 한글 폰트 설정 완료! 이제 한글 게임을 만들어보세요!**