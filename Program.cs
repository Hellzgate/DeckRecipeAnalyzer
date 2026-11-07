using System.Text;

namespace DeckRecipeAnalyzer;

/// <summary>
/// EDOPro 덱(.ydk) 분석기 — 뼈대.
/// 현재: 섹션 파싱 + 장수/중복/규정 검사.
/// 다음: cards.cdb 로 이름/테마 매핑, 초동 확률 계산 (AGENTS.md 참고).
/// </summary>
internal static class Program
{
    private const string DefaultDeckDir = @"D:\edopro\WindBot\Decks";

    private static int Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        if (args.Length == 0)
        {
            ListDecks(DefaultDeckDir);
            return 0;
        }

        if (args[0] is "-h" or "--help" or "/?")
        {
            PrintHelp();
            return 0;
        }

        var path = args[0];
        if (Directory.Exists(path))
        {
            ListDecks(path);
            return 0;
        }

        if (!File.Exists(path))
        {
            Console.Error.WriteLine($"파일 없음: {path}");
            return 2;
        }

        return AnalyzeDeck(path);
    }

    private static void PrintHelp()
    {
        Console.WriteLine("""
            DeckRecipeAnalyzer — EDOPro .ydk 분석기 (뼈대)

            사용법:
              DeckRecipeAnalyzer                     덱 폴더 요약
              DeckRecipeAnalyzer <덱.ydk>            덱 분석
              DeckRecipeAnalyzer <폴더>              폴더 내 덱 요약
              DeckRecipeAnalyzer --help              도움말

            구현 예정:
              --names    카드 이름 표시 (cards.cdb)
              --theme    테마 분포 (!setname)
              --odds     초동 확률
              --json     JSON 출력
            """);
    }

    private static void ListDecks(string dir)
    {
        if (!Directory.Exists(dir))
        {
            Console.WriteLine($"덱 폴더 없음: {dir}");
            return;
        }

        var decks = new DirectoryInfo(dir).GetFiles("*.ydk").OrderBy(f => f.Name).ToList();
        Console.WriteLine($"덱 폴더: {dir}");
        Console.WriteLine($"덱 수: {decks.Count}");
        Console.WriteLine();
        Console.WriteLine($"{"파일",-40} {"메인",5} {"엑스트라",8} {"사이드",6} {"수정",-17}");

        foreach (var d in decks)
        {
            var deck = YdkParser.Parse(d.FullName);
            var flag = deck.Valid ? "" : "  ⚠";
            Console.WriteLine($"{Trim(d.Name, 40),-40} {deck.Main.Count,5} {deck.Extra.Count,8} {deck.Side.Count,6} {d.LastWriteTime:yyyy-MM-dd HH:mm} {flag}");
        }
    }

    private static int AnalyzeDeck(string path)
    {
        var deck = YdkParser.Parse(path);
        var file = new FileInfo(path);

        Console.WriteLine($"덱     : {file.FullName}");
        Console.WriteLine($"수정   : {file.LastWriteTime:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"구성   : 메인 {deck.Main.Count} / 엑스트라 {deck.Extra.Count} / 사이드 {deck.Side.Count} (합계 {deck.Total})");
        Console.WriteLine($"고유   : 메인 {deck.Main.Distinct().Count()} / 엑스트라 {deck.Extra.Distinct().Count()} / 사이드 {deck.Side.Distinct().Count()}");
        Console.WriteLine();

        PrintDuplicates("메인", deck.Main);
        PrintDuplicates("엑스트라", deck.Extra);
        PrintDuplicates("사이드", deck.Side);

        Console.WriteLine();
        Console.WriteLine("규정 검사");
        Console.WriteLine($"  메인 40~60장 : {(deck.Main.Count is >= 40 and <= 60 ? "OK" : "위반")} ({deck.Main.Count})");
        Console.WriteLine($"  엑스트라 ≤15 : {(deck.Extra.Count <= 15 ? "OK" : "위반")} ({deck.Extra.Count})");
        Console.WriteLine($"  사이드  ≤15  : {(deck.Side.Count <= 15 ? "OK" : "위반")} ({deck.Side.Count})");
        Console.WriteLine($"  이름→ID 변환 필요 행: {deck.UnparsedLines}");

        return deck.Valid ? 0 : 1;
    }

    private static void PrintDuplicates(string label, List<int> cards)
    {
        var dup = cards.GroupBy(c => c)
                       .Where(g => g.Count() > 1)
                       .OrderByDescending(g => g.Count())
                       .ThenBy(g => g.Key)
                       .Take(8)
                       .ToList();

        if (dup.Count == 0)
        {
            Console.WriteLine($"{label} 중복: 없음");
            return;
        }

        Console.WriteLine($"{label} 중복: " + string.Join(", ", dup.Select(g => $"{g.Key}×{g.Count()}")));
    }

    private static string Trim(string s, int max) => s.Length <= max ? s : s[..(max - 1)] + "…";

    private sealed class Deck
    {
        public List<int> Main { get; } = new();
        public List<int> Extra { get; } = new();
        public List<int> Side { get; } = new();
        public int UnparsedLines { get; set; }
        public int Total => Main.Count + Extra.Count + Side.Count;

        public bool Valid =>
            Main.Count is >= 40 and <= 60 &&
            Extra.Count <= 15 &&
            Side.Count <= 15;
    }

    private static class YdkParser
    {
        public static Deck Parse(string path)
        {
            var deck = new Deck();
            var section = 0; // 0=main, 1=extra, 2=side

            foreach (var raw in File.ReadLines(path))
            {
                var line = raw.Trim();
                if (line.Length == 0) continue;

                if (line.StartsWith('#'))
                {
                    var name = line.TrimStart('#').Trim().ToLowerInvariant();
                    if (name is "main" or "created by" or "") { if (name == "main") section = 0; }
                    if (name == "extra") section = 1;
                    if (name is "side" or "!side") section = 2;
                    continue;
                }

                if (line.StartsWith('!'))
                {
                    var name = line.TrimStart('!').Trim().ToLowerInvariant();
                    if (name == "side") section = 2;
                    continue;
                }

                if (!int.TryParse(line, out var id) || id <= 0)
                {
                    deck.UnparsedLines++;
                    continue;
                }

                var target = section switch { 1 => deck.Extra, 2 => deck.Side, _ => deck.Main };
                target.Add(id);
            }

            return deck;
        }
    }
}
