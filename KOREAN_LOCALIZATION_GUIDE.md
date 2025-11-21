# vMenu 한글화 가이드 (Korean Localization Guide)

## 개요 (Overview)

이 프로젝트는 vMenu의 전역 한글화 작업입니다. 현재 인프라가 구축되어 있으며, 17개 메뉴 파일 중 6개가 완료되었습니다.

This project is for the global Korean localization of vMenu. The infrastructure is now in place, and 6 out of 17 menu files have been completed.

## 완료된 작업 (Completed Work)

### ✅ 인프라 (Infrastructure)
- `vMenu/Localization.cs` - 한글 리소스 접근 헬퍼 클래스
- `vMenu/Properties/Resources.ko.resx` - 한글 번역 리소스 파일
- `vMenu/vMenuClient.csproj` - 한글 리소스 포함하도록 업데이트됨

### ✅ 완료된 메뉴 파일 (Completed Menu Files)
1. **MainMenu.cs** - 모든 메인 메뉴 진입점
2. **About.cs** - vMenu 정보 메뉴
3. **Recording.cs** - 녹화 옵션 메뉴
4. **TimeOptions.cs** - 시간 옵션 메뉴
5. **WeatherOptions.cs** - 날씨 옵션 메뉴
6. **VoiceChat.cs** - 음성 채팅 설정 메뉴

## 남은 작업 (Remaining Work)

### 📋 남은 메뉴 파일 (Remaining Menu Files)

1. **WeaponLoadouts.cs** (~20 strings)
2. **BannedPlayers.cs** (~20 strings)
3. **PersonalVehicle.cs** (~30 strings)
4. **VehicleSpawner.cs** (~30 strings)
5. **OnlinePlayers.cs** (~30 strings)
6. **SavedVehicles.cs** (~40 strings)
7. **WeaponOptions.cs** (~50 strings)
8. **MiscSettings.cs** (~50 strings)
9. **PlayerOptions.cs** (~60 strings)
10. **PlayerAppearance.cs** (~80 strings)
11. **MpPedCustomization.cs** (~150 strings)

### 📝 추가 작업 (Additional Tasks)
- CommonFunctions.cs의 알림 메시지
- Notification.cs의 메시지
- 기타 하드코딩된 문자열

## 로컬라이제이션 패턴 (Localization Pattern)

### 1단계: Resources.ko.resx에 번역 추가

```xml
<data name="MenuName_ItemName" xml:space="preserve">
  <value>한글 번역</value>
</data>
<data name="MenuName_ItemName_Desc" xml:space="preserve">
  <value>한글 설명</value>
</data>
```

### 2단계: 메뉴 파일 업데이트

**변경 전:**
```csharp
menu = new Menu(Game.Player.Name, "English Title");
var item = new MenuItem("English Name", "English description");
```

**변경 후:**
```csharp
menu = new Menu(Game.Player.Name, Localization.GetString("MenuName_Title"));
var item = new MenuItem(
    Localization.GetString("MenuName_ItemName"), 
    Localization.GetString("MenuName_ItemName_Desc")
);
```

### 3단계: 동적 문자열 처리

**변경 전:**
```csharp
Notify.Custom($"Setting changed to {value}");
```

**변경 후:**
```csharp
// Resources.ko.resx에 추가:
// <data name="MenuName_SettingChanged"><value>설정이 {0}(으)로 변경되었습니다</value></data>

Notify.Custom(Localization.GetString("MenuName_SettingChanged", value));
```

## 리소스 키 명명 규칙 (Resource Key Naming Convention)

- **메뉴 제목**: `MenuName_Title`
- **메뉴 항목**: `MenuName_ItemName`
- **항목 설명**: `MenuName_ItemName_Desc`
- **알림 메시지**: `MenuName_NotificationDescription`
- **공통 레이블**: `Common_LabelName`

## 빌드 및 테스트 (Build and Test)

### 빌드 명령어
```bash
cd /home/runner/work/vMenu/vMenu
dotnet build vMenu.sln --configuration Release
```

### 성공 확인
- `build/vMenu/vMenuClient.net.dll` 생성 확인
- `build/vMenu/ko/vMenuClient.net.resources.dll` 생성 확인

### 일반적인 문제
- **오류**: 중복 EmbeddedResource
  - **해결**: `<EmbeddedResource Include>` 대신 `<EmbeddedResource Update>` 사용
  
- **오류**: 리소스 키를 찾을 수 없음
  - **해결**: Resources.ko.resx에 키가 올바르게 추가되었는지 확인

## 예제: WeaponLoadouts.cs 로컬라이제이션

### Resources.ko.resx에 추가할 번역:

```xml
<!-- Weapon Loadouts Menu -->
<data name="WeaponLoadouts_Title" xml:space="preserve">
  <value>무기 로드아웃 관리</value>
</data>
<data name="WeaponLoadouts_SaveLoadout" xml:space="preserve">
  <value>로드아웃 저장</value>
</data>
<data name="WeaponLoadouts_SaveLoadout_Desc" xml:space="preserve">
  <value>현재 무기를 새 로드아웃 슬롯에 저장합니다.</value>
</data>
<data name="WeaponLoadouts_ManageLoadouts" xml:space="preserve">
  <value>로드아웃 관리</value>
</data>
<data name="WeaponLoadouts_ManageLoadouts_Desc" xml:space="preserve">
  <value>저장된 무기 로드아웃을 관리합니다.</value>
</data>
<data name="WeaponLoadouts_RestoreDefault" xml:space="preserve">
  <value>리스폰 시 기본 로드아웃 복원</value>
</data>
<data name="WeaponLoadouts_RestoreDefault_Desc" xml:space="preserve">
  <value>로드아웃을 기본 로드아웃으로 설정한 경우, 리스폰할 때마다 로드아웃이 자동으로 장착됩니다.</value>
</data>
```

### WeaponLoadouts.cs 수정:

```csharp
public void CreateMenu()
{
    menu = new Menu(Game.Player.Name, Localization.GetString("WeaponLoadouts_Title"));

    var saveLoadout = new MenuItem(
        Localization.GetString("WeaponLoadouts_SaveLoadout"), 
        Localization.GetString("WeaponLoadouts_SaveLoadout_Desc")
    );
    var savedLoadoutsMenuBtn = new MenuItem(
        Localization.GetString("WeaponLoadouts_ManageLoadouts"), 
        Localization.GetString("WeaponLoadouts_ManageLoadouts_Desc")
    ) { Label = Localization.GetString("Common_Label_Arrow") };
    var enableDefaultLoadouts = new MenuCheckboxItem(
        Localization.GetString("WeaponLoadouts_RestoreDefault"), 
        Localization.GetString("WeaponLoadouts_RestoreDefault_Desc"), 
        WeaponLoadoutsSetLoadoutOnRespawn
    );
    
    // ... 나머지 코드
}
```

## 기여 가이드라인 (Contribution Guidelines)

1. **일관성 유지**: 기존 번역 스타일과 명명 규칙을 따르세요.
2. **빌드 테스트**: 모든 변경 후 프로젝트가 빌드되는지 확인하세요.
3. **리소스 키 문서화**: 새로운 리소스 키를 추가할 때는 명확한 주석을 포함하세요.
4. **단계별 커밋**: 각 메뉴 파일을 완료한 후 별도로 커밋하세요.

## 프로젝트 상태 (Project Status)

**진행률**: 약 40% 완료 (6/17 메뉴 파일)

**예상 남은 작업**: 11개 메뉴 파일 + 알림 메시지 + 공통 함수

**다음 우선순위**:
1. 작은 메뉴 파일 (WeaponLoadouts, BannedPlayers)
2. 중간 크기 메뉴 파일 (PersonalVehicle, VehicleSpawner, OnlinePlayers)
3. 큰 메뉴 파일 (PlayerOptions, VehicleOptions, PlayerAppearance, MpPedCustomization)
4. 시스템 메시지 및 알림

## 참고 자료 (References)

- [.NET Localization Documentation](https://docs.microsoft.com/en-us/dotnet/core/extensions/localization)
- [Resource File Format](https://docs.microsoft.com/en-us/dotnet/framework/resources/creating-resource-files-for-desktop-apps)

## 향후 개선 사항 (Future Enhancements)

현재 구현은 한글 전용이지만, 다음과 같은 개선이 가능합니다:

1. **다중 언어 지원**: Localization.cs를 수정하여 여러 언어를 지원하도록 구성 가능하게 만들기
2. **시스템 언어 감지**: 사용자의 시스템 언어를 자동 감지하여 적절한 리소스 파일 로드
3. **런타임 언어 전환**: 게임 중 언어를 전환할 수 있는 기능 추가
4. **폴백 메커니즘**: 번역이 없는 경우 영어로 자동 폴백

이러한 기능은 현재 한글화 작업이 완료된 후 별도 PR로 추가할 수 있습니다.

---

**마지막 업데이트**: 2024-11-21  
**작성자**: GitHub Copilot for Pull Requests  
**저장소**: [TeamConceptKR/vMenu](https://github.com/TeamConceptKR/vMenu)
