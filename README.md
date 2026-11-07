# DeckRecipeAnalyzer

EDOPro 덱(`.ydk`)을 분석하는 CLI 도구. **뼈대 단계**.

## 지금 되는 것

- 덱 파싱(`#main` / `#extra` / `!side`)
- 장수·고유 수·중복 카드(상위 8개)
- 규정 검사: 메인 40~60 / 엑스트라 ≤15 / 사이드 ≤15
- 폴더 요약(덱별 장수)

## 실행

```powershell
& "C:\Program Files\dotnet\dotnet.exe" build -c Release

# 덱 하나
.\bin\Release\net8.0\DeckRecipeAnalyzer.exe "D:\edopro\WindBot\Decks\AI_raisemoon.ydk"

# 폴더 전체 요약
.\bin\Release\net8.0\DeckRecipeAnalyzer.exe "D:\edopro\WindBot\Decks"
```

## 다음 단계

1. `cards.cdb`로 카드 이름/종족/레벨/테마 표시
2. 테마 분포(`strings.conf !setname`)
3. 초동 확률: `1 - C(N-k, n) / C(N, n)`
4. 두 덱 비교 / `--json` / `--csv`

자세한 경로는 `../_shared/EDOPRO-COMMON.md`, 작업 규칙은 `AGENTS.md`.
