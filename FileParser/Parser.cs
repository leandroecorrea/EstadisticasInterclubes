using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Tabula.Extractors;
using Tabula;
using UglyToad.PdfPig;
using System.IO;
using Core.Entities;

namespace FileParser
{
    public class Parser
    {
        public IEnumerable<PlayersMatch> Parse(byte[] fileBytes)
        {
            List<PlayersMatch> matches = new();
            List<Player> players = new();
            using (PdfDocument document = PdfDocument.Open(fileBytes, new ParsingOptions() { ClipPaths = true }))
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
            return matches;
        }
        private Result CreateResult(IReadOnlyList<Cell> row, PlayersMatch playersMatch)
        {
            Result result = new();
            (int p1Result, int p2Result) = ParseHolesWon(row);
            result.HolesWon = Math.Max(p2Result, p1Result);
            result.HolesRemaining = Math.Min(p2Result, p1Result);
            result.WinnerId = p1Result > p2Result ?
                              playersMatch.PlayerOne.Id :
                              p2Result > p1Result ?
                              playersMatch.PlayerTwo.Id :
                              Result.TIE_RESULT;
            return result;
        }

        private (int, int) ParseHolesWon(IReadOnlyList<Cell> row)
        {
            int p1Result = 0;
            int p2Result = 0;
            if (!string.IsNullOrEmpty(row[1].GetText()))
            {
                if (!row[1].GetText().ToLowerInvariant().Contains("up"))
                {
                    int.TryParse(row[1].GetText().Split('/')[0].ToString(), out p1Result);
                    int.TryParse(row[1].GetText().Split('/')[1].ToString(), out p2Result);
                }
                else
                {
                    int.TryParse(row[1].GetText()[0].ToString(), out p1Result);
                }
            }
            else if (!string.IsNullOrEmpty(row[3].GetText()))
            {
                if (!row[3].GetText().ToLowerInvariant().Contains("up"))
                {
                    int.TryParse(row[3].GetText().Split('/')[0].ToString(), out p2Result);
                    int.TryParse(row[3].GetText().Split('/')[1].ToString(), out p1Result);
                }
                else
                {
                    int.TryParse(row[3].GetText()[0].ToString(), out p2Result);
                }
            }
            return (p1Result, p2Result);
        }

        
    }
}
