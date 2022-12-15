// See https://aka.ms/new-console-template for more information
using Tabula.Extractors;
using Tabula;
using UglyToad.PdfPig;
using System.Runtime.ExceptionServices;
using System.Numerics;

List<PlayersMatch> matches = new();
List<Player> players = new();
List<string> paths = new()
{
    "C:\\Users\\lcorrea\\Documents\\Azure\\ConsoleApp1\\ConsoleApp1\\1.pdf",
    "C:\\Users\\lcorrea\\Documents\\Azure\\ConsoleApp1\\ConsoleApp1\\2.pdf",
    "C:\\Users\\lcorrea\\Documents\\Azure\\ConsoleApp1\\ConsoleApp1\\3.pdf",
    "C:\\Users\\lcorrea\\Documents\\Azure\\ConsoleApp1\\ConsoleApp1\\4.pdf"
};
paths.ForEach(path =>
{
    using (PdfDocument document = PdfDocument.Open(path, new ParsingOptions() { ClipPaths = true }))
    {
        ObjectExtractor oe = new ObjectExtractor(document);
        PageArea page = oe.Extract(1);

        IExtractionAlgorithm ea = new SpreadsheetExtractionAlgorithm();
        List<Table> tables = ea.Extract(page);
        tables.ForEach(x =>
        {
            if (x.Rows.Count == 6)
            {
                for (int i = 1; i <= 5; i++)
                {
                    var row = x.Rows[i];
                    PlayersMatch playersMatch = new();
                    playersMatch.PlayerOne = null;
                    foreach (var player in players)
                    {
                        if (player.Name == row[0].GetText().TrimEnd(',', ' ', '.'))
                        {
                            playersMatch.PlayerOne = player;
                        }
                        else if (player.Name == row[2].GetText().TrimEnd(',', ' ', '.'))
                        {
                            playersMatch.PlayerTwo = player;
                        }
                    }
                    if (playersMatch.PlayerOne == null)
                    {
                        Player player = new()
                        {
                            Id = Guid.NewGuid(),
                            Name = row[0].GetText().TrimEnd(',', ' ', '.')

                        };
                        playersMatch.PlayerOne = player;
                        players.Add(player);
                    }
                    if (playersMatch.PlayerTwo == null)
                    {
                        Player player = new()
                        {
                            Id = Guid.NewGuid(),
                            Name = row[2].GetText().TrimEnd(',', ' ', '.')
                        };
                        playersMatch.PlayerTwo = player;
                        players.Add(player);
                    }
                    playersMatch.Result = CreateResult(row, playersMatch);
                    matches.Add(playersMatch);
                }
            }
        });
    }
});

players.Select(x => new
{
    Jugador = x.Name,
    Ganados = matches.Aggregate(0, (previous, next) => next.Result.WinnerId == x.Id ? previous + 1 : previous),
    Empatados = matches.Aggregate(0, (previous, next) => (next.PlayerOne.Id == x.Id || next.PlayerTwo.Id == x.Id) && next.Result.WinnerId == Result.TIE_RESULT ? previous + 1 : previous),
    Perdidos = matches.Aggregate(0, (previous, next) => (next.PlayerOne.Id == x.Id || next.PlayerTwo.Id == x.Id) && next.Result.WinnerId != x.Id && next.Result.WinnerId != Result.TIE_RESULT ? previous + 1 : previous),
}).OrderByDescending(x => x.Ganados)
  .ThenBy(x => x.Perdidos)
  .Take(10)
  .ToList()
  .ForEach(x => Console.WriteLine(x));

static List<PlayersMatch> ParseTable(Table table)
{
    List<PlayersMatch> matches = new();
    if (table.Rows.Count == 6)
    {
        for (int i = 1; i <= 5; i++)
        {
            var row = table.Rows[i];
            PlayersMatch playersMatch = new();
            playersMatch.PlayerOne = null;
            foreach (var match in matches)
            {
                if (match.PlayerOne.Name == row[0].GetText().TrimEnd(',', ' ', '.'))
                {
                    playersMatch.PlayerOne = match.PlayerOne;
                }
                else if (match.PlayerTwo.Name == row[2].GetText().TrimEnd(',', ' ', '.'))
                {
                    playersMatch.PlayerTwo = match.PlayerTwo;
                }
            }
            if (playersMatch.PlayerOne == null)
            {
                playersMatch.PlayerOne = new()
                {
                    Id = Guid.NewGuid(),
                    Name = row[0].GetText().TrimEnd(',', ' ', '.')

                };
            }
            if (playersMatch.PlayerTwo == null)
            {
                playersMatch.PlayerTwo = new()
                {
                    Id = Guid.NewGuid(),
                    Name = row[2].GetText().TrimEnd(',', ' ', '.')
                };
            }
            playersMatch.Result = CreateResult(row, playersMatch);
            matches.Add(playersMatch);
        }
    }
    return matches;
}

static Result CreateResult(IReadOnlyList<Cell> row, PlayersMatch playersMatch)
{
    Result result = new();
    int p1Result = 0;
    int p2Result = 0;
    if (!string.IsNullOrEmpty(row[1].GetText()))
        int.TryParse(row[1].GetText()[0].ToString(), out p1Result);
    if (!string.IsNullOrEmpty(row[3].GetText()))
        int.TryParse(row[3].GetText()[0].ToString(), out p2Result);

    result.HolesWon = Math.Max(p2Result, p1Result);
    result.HolesRemaining = Math.Min(p2Result, p1Result);
    result.WinnerId = p1Result > p2Result ?
                      playersMatch.PlayerOne.Id :
                      p2Result > p1Result ?
                      playersMatch.PlayerTwo.Id :
                      Result.TIE_RESULT;
    return result;
}

class PlayersMatch
{
    public Player PlayerOne { get; set; }
    public Player PlayerTwo { get; set; }

    public Result Result { get; set; }

}


public class Result
{
    public static readonly Guid TIE_RESULT = Guid.Empty;
    public Guid WinnerId { get; set; }
    public int HolesWon { get; set; }
    public int HolesRemaining { get; set; }
}
class Player
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}

record TeamsMatch
{
    public string TeamOne { get; set; }
    public string TeamTwo { get; set; }
    public int TeamOneScore { get; set; }
    public int TeamTwoScore { get; set; }
}
