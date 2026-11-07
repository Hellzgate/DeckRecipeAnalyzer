# AGENTS.md — 덱레시피 분석기 (DeckRecipeAnalyzer)

EDOPro 덱(`.ydk`)을 읽어 **구성/분포/확률**을 분석하는 CLI 도구.
`WindBotAIEditor`(봇 편집기)와는 **별개 프로젝트**다.

## 경로

| 항목 | 경로 |
|---|---|
| 소스 | `C:\Users\Main PC\Documents\기본 프로젝트\DeckRecipeAnalyzer` |
| 덱 | `D:\edopro\WindBot\Decks\*.ydk`, `D:\edopro\deck\*.ydk` (읽기 전용) |
| 카드 DB | `D:\edopro\expansions\cards.cdb` (SQLite) |
| 테마 이름 | `D:\edopro\repositories\languages\한국어\strings.conf` (`!setname`) |
| 공통 문서 | `C:\Users\Main PC\Documents\기본 프로젝트\_shared\EDOPRO-COMMON.md` |

## 기술 스택 (확정)

- **C# / .NET 8 / 콘솔** (`net8.0`)
- 카드 이름/종족/테마: `Microsoft.Data.Sqlite` 로 `cards.cdb` 조회 (다음 단계)
- 기존 `.ydk` 파싱 규칙 재사용: **사이드 마커는 `!side`** (`#!side` 아님)

## 빌드 / 실행

```powershell
Set-Location "C:\Users\Main PC\Documents\기본 프로젝트\DeckRecipeAnalyzer"
& "C:\Program Files\dotnet\dotnet.exe" build -c Release
& ".\bin\Release\net8.0\DeckRecipeAnalyzer.exe" "D:\edopro\WindBot\Decks\AI_raisemoon.ydk"
```

## 현재 상태 (뼈대)

- [x] `.ydk` 파싱(main/extra/side) + 장수/종류/중복 요약 + 규정 검사(40~60 / ≤15 / ≤15)
- [x] 인자 없이 실행 → 덱 폴더 요약
- [ ] 카드 이름/종족/레벨/테마 매핑 (`cards.cdb`)
- [ ] 테마(`!setname`) 분포
- [ ] 초동(1장 스타트) 확률 계산: `1 - C(N-k, n) / C(N, n)`
- [ ] 두 덱 비교(차이 JSON), CSV/JSON 출력

## 설계 원칙

1. **읽기 전용**: `D:\edopro` 를 수정하지 않는다.
2. 규정 위반은 **경고**로 표시(강제 중단 금지).
3. 출력은 표 + `--json`.

## 검증 규칙

- 기준 덱: `D:\edopro\WindBot\Decks\AI_raisemoon.ydk`
- `!side` 가 **엑스트라로 집계되면 버그** (자주 틀리는 것)

## 작업 방식 (공통)

- 창을 띄우지 않는다(콘솔 CLI만). 실행 중 프로세스 강제 종료 금지.
- 사용자에게는 **원인 → 수정 → 검증 결과 → 경로** 순서로 짧게 보고.
