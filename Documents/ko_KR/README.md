# NoMoreMissing
 
NoMoreMissing 은 유니티 프로젝트내 Missing Script 문제를 해결해 줍니다.

## 주의

* 플러그인은 모든 프리팹내 Behaviour 명칭을 기록하므로 적용하기 전에 프로젝트를 백업하시기 바랍니다. 


## 예시

### Managed Behaviour

* ManagedBehaviour 를 사용할 게임 오브젝트들을 프리팹으로 만듭니다.

* Behaviour 소스를 컴파일해서 라이브러리로 만듭니다.

    iTween.cs -> iTween.dll

* **"Help/Prepare for Missing scripts** 메뉴를 클릭합니다.

* Behaviour 소스를 제거하고 라이브러리를 복사합니다.

    $ rm iTween.cs 
    $ copy iTween.dll Assets/iTween.dll

* **Help/Fix Missing scripts** 메뉴를 클릭합니다.

* 유니티를 재시작합니다.

### Behaviour 메타 손실

* 게임 오브젝트를 만듭니다

    Hierarchy:

        example

* 테스트용 Behaviour 를 만듭니다.

    Project:
        
        Assets/
            TestBehaviour.cs

* 게임 오브젝트내 테스트용 Behaviour 를 추가합니다.

    Inspector:

        Transform:
            ...

        TestBehaviour:
            Name: john
            Score: 75 

* 게임 오브젝트를 프리팹으로 저장합니다.

    Project:
        
        Assets/
            example.prefab

* **Help/Prepare for Missing scripts** 메뉴를 클릭합니다

* 씬을 저장하고 유니티를 종료합니다.

* 깨진 프리팹(Missing Script가 포함된)을 만들기 위해 meta 파일과 라이브러리 폴더를 삭제합니다.

        $ rm Assets/TestBehaviour.cs
        $ rm Library

* 씬을 열어 **MISSING SCRIPT** 를 확인합니다.

* **Help/Fix Missing scripts** 메뉴를 클릭합니다.

* 씬을 다시 열어 수정 여부를 확인합니다.

