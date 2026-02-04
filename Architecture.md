# 2D Clicker Game - Architecture Overview

---

## 1. 프로젝트 구조 (Feature-based Clean Architecture)

```
Assets/01.Scripts/
├── Core/                          # 앱 초기화, 유틸리티
│   ├── FirebaseInitializer.cs
│   ├── Crypto.cs
│   └── Extension/
│       └── NumberFormatExtension.cs
│
├── Outgame/                       # 씬 독립적 상태 관리
│   ├── LoginScene.cs
│   └── Feature/
│       ├── Account/               # 인증
│       │   ├── 1.Repository/
│       │   ├── 2.Domain/
│       │   └── 3.Manager/
│       ├── Currency/              # 재화
│       │   ├── 1.Repository/
│       │   ├── 2.Domain/
│       │   └── 3.Manager/
│       ├── Monster/               # 몬스터 컬렉션
│       │   ├── 1.Repository/
│       │   ├── 2.Domain/
│       │   └── 3.Manager/
│       └── Upgrade/               # 업그레이드
│           ├── 1.Repository/
│           ├── 2.Domain/
│           └── 3.Manager/
│
├── Ingame/                        # 씬 종속 게임플레이
│   ├── Click/
│   ├── Monster/
│   ├── Resource/
│   ├── Feedback/
│   └── Interface/
│
└── UI/                            # 프레젠테이션
    ├── UI_Currency.cs
    └── Upgrade/
```

---

## 2. 레이어 아키텍처 (각 Feature 내부 구조)

모든 Feature는 동일한 3-Layer 구조를 따릅니다.

```
┌─────────────────────────────────────────────────────┐
│                    3. Manager                        │
│         (Use Case / Application Service)             │
│  비즈니스 흐름 조율, 외부 시스템과의 소통 창구              │
│  예: CurrencyManager.TrySpend()                      │
├─────────────────────────────────────────────────────┤
│                    2. Domain                         │
│            (핵심 비즈니스 규칙)                         │
│  외부 의존성 없음. 순수 로직만 포함                       │
│  예: Currency(struct), MonsterCollection              │
├─────────────────────────────────────────────────────┤
│                  1. Repository                       │
│           (데이터 영속화 추상화)                         │
│  Interface + 구현체 (Firebase / Local)                │
│  예: ICurrencyRepository, FirebaseCurrencyRepository  │
└─────────────────────────────────────────────────────┘
```

**의존성 방향 (Dependency Rule):**

```
  Manager ──depends on──> Domain    (Manager는 Domain을 안다)
  Manager ──depends on──> Repository Interface (Manager는 인터페이스만 안다)
Repository ──depends on──> Domain    (SaveData가 Domain의 enum을 참조)

  Domain은 Manager를 모른다   (위를 모름)
  Domain은 Repository를 모른다 (아래를 모름)
```

```
         Manager          ← 흐름 조율, Domain ↔ SaveData 변환 담당
          │    │
    uses  │    │  uses
          ▼    ▼
      Domain    Repository(Interface)
         ↑          │
         │  depends  │
         └───────────┘
         SaveData가 Domain의
         enum을 참조 (바깥→안쪽)
```

> **Domain이 가장 안쪽 레이어입니다.**
> Domain은 Manager도 Repository도 모릅니다.
> Domain ↔ SaveData 간 변환은 Manager가 담당합니다.
> SaveData가 Domain의 enum을 참조하는 것은 바깥 레이어가 안쪽을 의존하는 것이므로 규칙에 부합합니다.

---

## 3. Feature별 클래스 다이어그램

### 3-1. Account Feature

```
┌──────────────────────────────────────────────┐
│               AccountManager                  │
│  (Singleton, DontDestroyOnLoad)               │
│──────────────────────────────────────────────│
│  - _repository : IAccountRepository           │
│  - _currentAccount : Account                  │
│──────────────────────────────────────────────│
│  + TryLogin(email, pw) : UniTask<Result>      │
│  + TryRegister(email, pw) : UniTask<Result>   │
│  + Logout()                                   │
│  + IsLogin : bool                             │
│  + Email : string                             │
└───────────┬──────────────────────────────────┘
            │ uses
            ▼
┌───────────────────────┐     ┌──────────────────────┐
│  IAccountRepository   │◁────│ FirebaseAccountRepo  │
│───────────────────────│     │──────────────────────│
│ + Register()          │     │ FirebaseAuth 사용      │
│ + Login()             │     └──────────────────────┘
│ + Logout()            │◁────┌──────────────────────┐
└───────────────────────┘     │ LocalAccountRepo     │
                              │──────────────────────│
            ▲                 │ PlayerPrefs 사용       │
            │ validates       └──────────────────────┘
┌───────────┴───────────┐
│       Account         │
│  (Domain Model)       │
│───────────────────────│
│  + Email : string     │
│  + Password : string  │
│  (생성자에서 유효성 검증)  │
└───────────────────────┘
```

### 3-2. Currency Feature

```
┌──────────────────────────────────────────────┐
│              CurrencyManager                  │
│  (Singleton)                                  │
│──────────────────────────────────────────────│
│  - _currency[] : Currency                     │
│  - _repository : ICurrencyRepository          │
│──────────────────────────────────────────────│
│  + Add(type, amount)                          │
│  + Spend(type, amount) : bool                 │
│  + CanAfford(type, amount) : bool             │
│  + Get(type) : Currency                       │
│  + event OnCurrencyChanged                    │
└───────────┬──────────────────────────────────┘
            │ uses
            ▼
┌───────────────────────┐     ┌──────────────────────┐
│ ICurrencyRepository   │◁────│ FirebaseCurrencyRepo │
│───────────────────────│     │──────────────────────│
│ + Save(CurrencySave)  │     │ Firestore 사용        │
│ + Load() : CurrSave   │     │ Path: users/{id}/    │
└───────────────────────┘     │   currency/data      │
            ▲                 └──────────────────────┘
            │ serializes
┌───────────┴───────────┐     ┌──────────────────────┐
│   CurrencySaveData    │     │    Currency (struct)  │
│  [FirestoreData]      │     │   (Value Object)     │
│───────────────────────│     │──────────────────────│
│  + Currencies: double[]│     │  + Value : double    │
└───────────────────────┘     │  + operator +, -, >, │
                              │    <, >=, <=, ==     │
                              │  + implicit 변환      │
                              └──────────────────────┘
```

### 3-3. Monster Feature (Outgame + Ingame)

```
┌─── Outgame (씬 독립) ─────────────────────────────────────────────┐
│                                                                    │
│  ┌──────────────────────────────────────────────┐                  │
│  │         MonsterOutgameManager                 │                  │
│  │  (Singleton)                                  │                  │
│  │──────────────────────────────────────────────│                  │
│  │  - _collection : MonsterCollection            │                  │
│  │  - _repository : IMonsterRepository           │                  │
│  │──────────────────────────────────────────────│                  │
│  │  + TrySpawn() / TryMerge()                    │                  │
│  │  + GetTierCount(tier) : int                   │                  │
│  │  + static event OnDataChanged                 │                  │
│  └────────┬──────────────┬───────────────────────┘                  │
│           │ uses         │ uses                                     │
│           ▼              ▼                                          │
│  ┌─────────────────┐  ┌────────────────────┐                       │
│  │MonsterCollection│  │IMonsterRepository  │◁── FirebaseMonsterRepo│
│  │  (Domain)       │  │                    │◁── LocalMonsterRepo   │
│  │─────────────────│  └────────────────────┘                       │
│  │ - _tierCounts[] │         ▲                                     │
│  │ - _maxPerTier   │         │ serializes                          │
│  │ - _mergeRequire │  ┌──────┴─────────────┐                       │
│  │─────────────────│  │  MonsterSaveData    │                       │
│  │ + AddMonster()  │  │  [FirestoreData]    │                       │
│  │ + RemoveMonster()│  │  + TierCounts: int[]│                       │
│  │ + FindMergeable()│  └────────────────────┘                       │
│  │ + GetAllTierCounts() : int[]              │                       │
│  │ + SetTierCounts(int[])                    │                       │
│  │                  │                                                │
│  │ ※ SaveData를 모름 │  ※ Manager가 변환 담당                         │
│  └─────────────────┘                                                │
└────────────────────────────────────────────────────────────────────┘
         │
         │ event OnDataChanged
         ▼
┌─── Ingame (씬 종속) ──────────────────────────────────────────────┐
│                                                                    │
│  ┌──────────────────────────────────────────────┐                  │
│  │         MonsterInGameManager                  │                  │
│  │──────────────────────────────────────────────│                  │
│  │  - _monsters : List<Monster>                  │                  │
│  │──────────────────────────────────────────────│                  │
│  │  + TrySpawnMonster() / TryMerge()             │                  │
│  │  + LoadMonsters()    ← OnDataChanged 수신 후    │                  │
│  └────────┬──────────────────────────────────────┘                  │
│           │ creates                                                 │
│           ▼                                                         │
│  ┌─────────────────────────────────────┐                            │
│  │  Monster (GameObject)               │                            │
│  │  ├── MonsterAnimator  (애니메이션)    │                            │
│  │  ├── MonsterMovement  (이동)         │                            │
│  │  └── MonsterAttack    (공격)         │                            │
│  │                                     │                            │
│  │  State: Spawning→Idle→Moving→Attack │                            │
│  └─────────────────────────────────────┘                            │
└────────────────────────────────────────────────────────────────────┘
```

### 3-4. Upgrade Feature

```
┌──────────────────────────────────────────────┐
│              UpgradeManager                   │
│  (Singleton)                                  │
│──────────────────────────────────────────────│
│  - _upgrades : Dict<EUpgradeType, Upgrade>    │
│  - _repository : IUpgradeRepository           │
│  - _specTable : UpgradeSpecTableSO            │
│──────────────────────────────────────────────│
│  + TryLevelUp(type) : bool                    │
│  + CanLevelUp(type) : bool                    │
│  + Get(type) : Upgrade                        │
│  + static event OnDataChanged                 │
└───────┬───────────────┬──────────────────────┘
        │ uses          │ uses
        ▼               ▼
┌───────────────┐  ┌────────────────────┐
│   Upgrade     │  │IUpgradeRepository  │◁── FirebaseUpgradeRepo
│  (Domain)     │  └────────────────────┘◁── LocalUpgradeRepo
│───────────────│         ▲
│ + Level       │         │ serializes
│ + Cost        │  ┌──────┴─────────────┐
│ + Value       │  │  UpgradeSaveData   │
│ + TryLevelUp()│  │  [FirestoreData]   │
│ + IsMaxLevel  │  │  + Levels: int[]   │
└───────┬───────┘  └────────────────────┘
        │ configured by
        ▼
┌───────────────────┐     ┌──────────────────────┐
│  UpgradeSpecData  │◁────│ UpgradeSpecTableSO   │
│───────────────────│     │  (ScriptableObject)  │
│ + Type            │     │  Inspector에서 설정     │
│ + MaxLevel        │     └──────────────────────┘
│ + BaseCost        │
│ + CostMultiplier  │     Cost = BaseCost + CostMultiplier^Level
│ + BaseValue       │     Value = BaseValue + Level * ValuePerLevel
│ + ValuePerLevel   │
└───────────────────┘
```

---

## 4. Manager 간 의존성 맵

```
┌─────────────┐
│   Account   │    ※ 다른 Manager가 AccountManager.Email을 참조
│   Manager   │    ※ 로그인 성공 후 GameScene 전환
└──────┬──────┘
       │ Email (userId)
       ▼
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│  Currency   │◁────│   Monster   │     │   Upgrade   │
│  Manager    │     │  Outgame    │     │   Manager   │
│             │◁────│  Manager    │     │             │
│  Add/Spend  │     │             │     │             │
│  CanAfford  │     │ TrySpawn()  │     │ TryLevelUp()│
└──────┬──────┘     │ TryMerge()  │     └──────┬──────┘
       │            └──────┬──────┘            │
       │                   │                    │
       │     event         │ event              │ event
       │  OnCurrency       │ OnData             │ OnData
       │  Changed          │ Changed            │ Changed
       ▼                   ▼                    ▼
┌──────────────────────────────────────────────────────┐
│                    Ingame Layer                        │
│                                                        │
│  MonsterInGameManager ◁── OnDataChanged (초기 로드)      │
│  Clicker              ◁── OnDataChanged (데미지 갱신)    │
│  ResourceSpawner      ◁── OnDataChanged (리소스 리밸런스) │
│  UI_Currency          ◁── OnCurrencyChanged (표시 갱신)  │
│  UpgradeButton        ◁── OnDataChanged (버튼 갱신)      │
│  MonsterSpawnButton   ◁── OnDataChanged (버튼 갱신)      │
└──────────────────────────────────────────────────────┘

의존성 방향:
  Monster/Upgrade Manager ──> CurrencyManager (Spend/CanAfford)
  Ingame ──> Outgame Manager (이벤트 구독)
  UI ──> Manager (이벤트 구독)

  ※ CurrencyManager는 다른 Manager를 모름 (가장 독립적)
  ※ Outgame Manager는 Ingame을 모름 (이벤트로만 통신)
```

---

## 5. 데이터 흐름 시퀀스 다이어그램

### 5-1. 로그인 ~ 게임 시작

```
LoginScene          AccountManager       Firebase        GameScene Managers
    │                    │                  │                  │
    │  TryLogin(email,pw)│                  │                  │
    │───────────────────>│                  │                  │
    │                    │  new Account()   │                  │
    │                    │  (유효성 검증)     │                  │
    │                    │                  │                  │
    │                    │  Login(email,pw) │                  │
    │                    │────────────────> │                  │
    │                    │                  │  Firebase Auth   │
    │                    │  AccountResult   │                  │
    │                    │<────────────────│                  │
    │                    │                  │                  │
    │  result.Success    │                  │                  │
    │<───────────────────│                  │                  │
    │                    │                  │                  │
    │  LoadScene("Game") │                  │                  │
    │═══════════════════════════════════════│═════════════════>│
    │                    │                  │                  │
    │                    │                  │  Awake()         │
    │                    │                  │  - Repository 생성│
    │                    │                  │  - Domain 초기화   │
    │                    │                  │                  │
    │                    │                  │  Start()         │
    │                    │                  │  - Load() async  │
    │                    │                  │<─────────────────│
    │                    │                  │  Firestore Read  │
    │                    │                  │─────────────────>│
    │                    │                  │                  │
    │                    │                  │  OnDataChanged   │
    │                    │                  │  (로드 완료 알림)  │
```

### 5-2. 몬스터 소환

```
User        SpawnButton     InGameMgr      OutgameMgr     CurrencyMgr    Firebase
 │              │               │              │              │             │
 │  Click       │               │              │              │             │
 │─────────────>│               │              │              │             │
 │              │ TrySpawnMonster()             │              │             │
 │              │──────────────>│              │              │             │
 │              │               │  TrySpawn()  │              │             │
 │              │               │─────────────>│              │             │
 │              │               │              │  CanAfford() │             │
 │              │               │              │─────────────>│             │
 │              │               │              │  true        │             │
 │              │               │              │<─────────────│             │
 │              │               │              │  Spend()     │             │
 │              │               │              │─────────────>│             │
 │              │               │              │              │  Save()     │
 │              │               │              │              │────────────>│
 │              │               │              │              │             │
 │              │               │              │ collection   │             │
 │              │               │              │ .AddMonster()│             │
 │              │               │              │ Save()       │             │
 │              │               │              │─────────────────────────── >│
 │              │               │              │              │             │
 │              │               │  true        │              │             │
 │              │               │<─────────────│              │             │
 │              │               │              │              │             │
 │              │               │ CreateMonster(tier=0, pos)  │             │
 │              │               │ Instantiate GameObject      │             │
 │              │               │ Monster.Initialize()        │             │
 │              │               │              │              │             │
 │  몬스터 등장  │               │              │              │             │
 │<═════════════════════════════│              │              │             │
```

### 5-3. 클릭 공격 (플레이어 + 몬스터 공통)

```
Clicker/Monster      Resource         Feedbacks        CurrencyMgr
     │                  │                │                 │
     │  OnClick(info)   │                │                 │
     │─────────────────>│                │                 │
     │                  │  HP -= Damage  │                 │
     │                  │                │                 │
     │                  │  Play(info)    │                 │
     │                  │───────────────>│                 │
     │                  │               DamageFloater     │
     │                  │               ColorFlash        │
     │                  │               Sound             │
     │                  │                │                 │
     │                  │  reward = BaseReward + Damage    │
     │                  │  Add(Gold, reward)               │
     │                  │────────────────────────────────>│
     │                  │                │                 │
     │                  │  if HP <= 0    │                 │
     │                  │  ─> Destroy    │                 │
     │                  │  ─> Respawn    │                 │
```

### 5-4. 업그레이드 레벨업

```
UpgradeButton     UpgradeMgr      CurrencyMgr     Firebase     Ingame Systems
     │                │               │              │              │
     │  TryLevelUp()  │               │              │              │
     │───────────────>│               │              │              │
     │                │  Spend(cost)  │              │              │
     │                │──────────────>│              │              │
     │                │  true         │              │              │
     │                │<──────────────│              │              │
     │                │               │              │              │
     │                │  upgrade      │              │              │
     │                │  .TryLevelUp()│              │              │
     │                │  Level++      │              │              │
     │                │               │              │              │
     │                │  Save()       │              │              │
     │                │──────────────────────────── >│              │
     │                │               │              │              │
     │                │  OnDataChanged│              │              │
     │                │═══════════════════════════════════════════ >│
     │                │               │              │   Clicker:   │
     │                │               │              │   데미지 갱신  │
     │                │               │              │   Resource:  │
     │                │               │              │   리밸런스     │
```

---

## 6. Repository 패턴 상세

### 인터페이스 / 구현체 매핑

```
┌─────────────────────┐
│    <<interface>>     │
│  IXxxRepository      │
│─────────────────────│
│ + Save(SaveData)     │   ───── 동일한 계약
│ + Load() : SaveData  │
└──────────┬──────────┘
           │
     ┌─────┴─────┐
     ▼           ▼
┌──────────┐ ┌──────────┐
│ Firebase │ │  Local   │
│   Repo   │ │  Repo    │
│──────────│ │──────────│
│ Firestore│ │PlayerPrefs│
│ SetAsync │ │ SetInt   │
│ GetSnap  │ │ GetInt   │
└──────────┘ └──────────┘
```

### Firestore 문서 구조

```
Firestore DB
└── users (Collection)
    └── {email} (Document)
        ├── currency (Collection)
        │   └── data (Document)
        │       └── { Currencies: [1500.0] }
        │
        ├── monster (Collection)
        │   └── data (Document)
        │       └── { TierCounts: [3, 1, 0, 0, 0] }
        │
        └── upgrade (Collection)
            └── data (Document)
                └── { Levels: [2, 5, 1] }
```

### SaveData 직렬화 방식

```
[FirestoreData] 어트리뷰트 활용
  → SetAsync(saveData)     : 객체를 자동 직렬화하여 저장
  → ConvertTo<SaveData>()  : 문서를 자동 역직렬화하여 반환
  → 수동 Dictionary 변환 불필요
```

---

## 7. 이벤트 시스템 맵

```
┌──────────────────────────────────────────────────────────────┐
│                       이벤트 발행자                             │
├──────────────────────┬───────────────────────────────────────┤
│  CurrencyManager     │  OnCurrencyChanged(type, amount)      │
│  (instance event)    │                                       │
├──────────────────────┼───────────────────────────────────────┤
│  UpgradeManager      │  OnDataChanged()                      │
│  (static event)      │                                       │
├──────────────────────┼───────────────────────────────────────┤
│  MonsterOutgameManager│ OnDataChanged()                      │
│  (static event)      │                                       │
├──────────────────────┼───────────────────────────────────────┤
│  MonsterInGameManager│  OnMonsterChanged()                   │
│  (instance event)    │                                       │
└──────────────────────┴───────────────────────────────────────┘
                          │
                          ▼  구독자
┌──────────────────────────────────────────────────────────────┐
│  OnCurrencyChanged ───> UI_Currency (표시 갱신)               │
│                    ───> UpgradeButtonBase (버튼 활성화 갱신)    │
│                    ───> MonsterSpawnButton (버튼 활성화 갱신)   │
│                                                               │
│  UpgradeManager        Clicker (데미지 값 갱신)                │
│  .OnDataChanged   ───> ResourceSpawner (리소스 리밸런스)        │
│                   ───> MonsterInGameManager (ToolLevel 갱신)   │
│                   ───> UpgradeButton (레벨/비용 표시 갱신)       │
│                                                               │
│  MonsterOutgame        MonsterInGameManager                   │
│  .OnDataChanged   ───>   (최초 1회 LoadMonsters 실행)          │
│                   ───> MonsterSpawnButton (버튼 갱신)           │
│                                                               │
│  OnMonsterChanged ───> MonsterSpawnButton (가용 수량 갱신)      │
└──────────────────────────────────────────────────────────────┘
```

---

## 8. 씬 라이프사이클 & 객체 생존 범위

```
  LoginScene                              GameScene
 ┌──────────────────┐                   ┌──────────────────────────────┐
 │                  │   LoadScene()     │                              │
 │  LoginScene UI   │ ═══════════════>  │  Ingame 오브젝트들             │
 │                  │                   │  ├── MonsterInGameManager    │
 └──────────────────┘                   │  ├── ResourceSpawner        │
                                        │  ├── Clicker                │
                                        │  ├── Monster (GameObjects)  │
                                        │  ├── Resource (GameObjects) │
                                        │  └── UI (Canvas)            │
                                        └──────────────────────────────┘

 ─ ─ ─ ─ ─ ─ ─  DontDestroyOnLoad (씬 전환에도 유지)  ─ ─ ─ ─ ─ ─ ─ ─

 ┌────────────────────────────────────────────────────────────────────┐
 │  FirebaseInitializer                                               │
 │  AccountManager      ← LoginScene에서 생성, Email 보유              │
 │  CurrencyManager     ← GameScene Awake에서 생성                     │
 │  UpgradeManager      ← GameScene Awake에서 생성                     │
 │  MonsterOutgameManager ← GameScene Awake에서 생성                   │
 │  UISoundManager                                                    │
 └────────────────────────────────────────────────────────────────────┘
```

---

## 9. 비동기(Async) 처리 패턴

```
┌─────────────────────────────────────────────────────────┐
│                    Save (Fire-and-Forget)                │
│                                                         │
│  Manager.Save()                                         │
│    └─> _repository.Save(saveData).Forget();             │
│           └─> await doc.SetAsync(saveData).AsUniTask(); │
│                                                         │
│  ※ 저장 실패해도 게임 진행에 영향 없음                       │
│  ※ Debug.LogError로 실패 로깅                             │
├─────────────────────────────────────────────────────────┤
│                    Load (Await 필수)                      │
│                                                         │
│  async void Load()                                      │
│    var saveData = await _repository.Load();              │
│    ─> Manager가 SaveData → Domain 변환                    │
│    ─> OnDataChanged?.Invoke();   ← 로드 완료 알림          │
│                                                         │
│  예: _collection.SetTierCounts(saveData.TierCounts)      │
│  예: _currency[i] = new Currency(saveData.Currencies[i]) │
│                                                         │
│  ※ await 없으면 UniTask 객체 자체가 반환 (버그)              │
│  ※ 로드 완료 후 이벤트로 다른 시스템에 알림                    │
│  ※ Domain은 SaveData를 모름 — Manager가 변환 책임            │
├─────────────────────────────────────────────────────────┤
│                Firebase Task → UniTask 변환              │
│                                                         │
│  Firebase SDK: Task<T>                                  │
│      └─> .AsUniTask()  ─> UniTask<T>                    │
│                                                         │
│  UniTaskVoid (fire-and-forget):                         │
│      └─> .Forget()  ─> 예외 무시하지 않고 로그 출력          │
└─────────────────────────────────────────────────────────┘
```

---

## 10. Ingame 시스템 구조

### 클릭 시스템

```
┌────────────┐     ClickInfo         ┌────────────────┐
│  Clicker   │ ─────────────────────>│   <<IClickable>>│
│ (Raycast)  │  { Damage, ToolLevel, │                 │
└────────────┘    Position, Type }   │   Resource      │
                                     │   ├── HP        │
┌────────────┐     ClickInfo         │   ├── Reward    │
│  Monster   │ ─────────────────────>│   └── Feedbacks │
│  Attack    │  { MonsterDamage,     │                 │
└────────────┘    ToolLevel }        └────────┬────────┘
                                              │
                                              │ implements
                                              ▼
                                     ┌────────────────┐
                                     │  <<IFeedback>>  │
                                     │  ├── Damage     │
                                     │  │   Floater    │
                                     │  ├── ColorFlash │
                                     │  ├── Scale      │
                                     │  │   Tweening   │
                                     │  ├── Sound      │
                                     │  └── Warning    │
                                     │      Floater    │
                                     └────────────────┘
```

### Monster 상태 머신

```
   ┌──────────┐
   │ Spawning │  Scale-In 애니메이션
   └────┬─────┘
        │ 완료
        ▼
   ┌──────────┐  랜덤 Resource 탐색
   │   Idle   │◁─────────────────────────────┐
   └────┬─────┘                              │
        │ 타겟 발견                            │
        ▼                                     │
   ┌──────────┐  타겟까지 이동                 │
   │  Moving  │                              │
   └────┬─────┘                              │
        │ 사거리 도달                          │
        ▼                                     │
   ┌──────────┐  ClickInfo로 Resource 공격     │
   │Attacking │───────────────────────────────┘
   └──────────┘  타겟 파괴 시 Idle로 복귀
```

---

## 11. 네이밍 컨벤션

```
┌───────────────┬──────────────────┬─────────────────────────┐
│    구분         │    접두/접미사     │    예시                   │
├───────────────┼──────────────────┼─────────────────────────┤
│  Interface    │  I 접두           │  IClickable             │
│               │                  │  IFeedback              │
│               │                  │  ICurrencyRepository    │
├───────────────┼──────────────────┼─────────────────────────┤
│  Enum         │  E 접두           │  ECurrencyType          │
│               │                  │  EUpgradeType           │
├───────────────┼──────────────────┼─────────────────────────┤
│  Manager      │  Manager 접미     │  CurrencyManager        │
│  (Singleton)  │                  │  UpgradeManager         │
├───────────────┼──────────────────┼─────────────────────────┤
│  SaveData     │  SaveData 접미    │  CurrencySaveData       │
│  (DTO)        │                  │  MonsterSaveData        │
├───────────────┼──────────────────┼─────────────────────────┤
│  Repository   │  Repository 접미  │  FirebaseCurrencyRepo   │
│               │  Firebase/Local  │  LocalCurrencyRepo      │
│               │  접두             │                         │
├───────────────┼──────────────────┼─────────────────────────┤
│  Scriptable   │  SO 접미          │  UpgradeSpecTableSO     │
│  Object       │                  │                         │
├───────────────┼──────────────────┼─────────────────────────┤
│  UI           │  UI_ 접두 또는     │  UI_Currency            │
│               │  Button 접미      │  UpgradeButton          │
├───────────────┼──────────────────┼─────────────────────────┤
│  private 필드  │  _ 접두           │  _repository            │
│               │                  │  _currentAccount        │
├───────────────┼──────────────────┼─────────────────────────┤
│  Domain Model │  접미 없음         │  Account, Currency      │
│  (순수 클래스)  │  (명사 그대로)     │  Upgrade, Monster       │
└───────────────┴──────────────────┴─────────────────────────┘
```

---

## 12. 설계 원칙 요약

```
┌─────────────────────────────────────────────────────────────────┐
│  1. 단일 책임 (SRP)                                              │
│     - Manager: 흐름 조율만                                        │
│     - Domain: 비즈니스 규칙만                                      │
│     - Repository: 저장/로드만                                     │
│     - UI: 표시만                                                  │
├─────────────────────────────────────────────────────────────────┤
│  2. 개방-폐쇄 (OCP)                                              │
│     - IRepository 인터페이스로 저장소 교체 가능                       │
│       (Firebase ↔ Local, 서버 추가 시 새 구현체만 작성)               │
│     - IFeedback으로 피드백 종류 확장 가능                            │
├─────────────────────────────────────────────────────────────────┤
│  3. 의존성 역전 (DIP)                                             │
│     - Manager는 IRepository(추상)에 의존                           │
│     - Firebase/Local(구체)은 IRepository를 구현                    │
│     - Domain은 외부 의존성 없음 (가장 안쪽 레이어)                    │
│     - Domain ↔ SaveData 변환은 Manager가 담당                      │
│       (Domain이 저장 방식을 모르도록 보장)                            │
├─────────────────────────────────────────────────────────────────┤
│  4. 이벤트 기반 느슨한 결합                                         │
│     - Outgame Manager는 Ingame을 직접 참조하지 않음                  │
│     - OnDataChanged 이벤트로만 통신                                 │
│     - UI도 이벤트 구독으로 갱신 (Manager가 UI를 모름)                  │
├─────────────────────────────────────────────────────────────────┤
│  5. 응집도                                                        │
│     - Feature 폴더 단위로 관련 코드가 모여 있음                       │
│     - Currency 수정 시 Currency 폴더만 보면 됨                      │
│     - Ingame/Outgame 분리로 게임 로직과 상태 관리 분리                │
└─────────────────────────────────────────────────────────────────┘
```
