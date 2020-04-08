using PremierLeague.Core;
using PremierLeague.Core.Contracts;
using PremierLeague.Core.Entities;
using PremierLeague.Persistence;
using Serilog;
using System;
using System.Linq;
using ConsoleTableExt;
using System.Collections.Generic;
using PremierLeague.Core.DataTransferObjects;

namespace PremierLeague.ImportConsole
{
    class Program
    {
        static void Main()
        {
            PrintHeader();
            InitData();
            AnalyzeData();

            Console.Write("Beenden mit Eingabetaste ...");
            Console.ReadLine();
        }

        private static void PrintHeader()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(new String('-', 60));

            Console.WriteLine(
                  @"
            _,...,_
          .'@/~~~\@'.          
         //~~\___/~~\\        P R E M I E R  L E A G U E 
        |@\__/@@@\__/@|             
        |@/  \@@@/  \@|            (inkl. Statistik)
         \\__/~~~\__//
          '.@\___/@.'
            `""""""
                ");

            Console.WriteLine(new String('-', 60));
            Console.WriteLine();
            Console.ResetColor();
        }

        /// <summary>
        /// Importiert die Ergebnisse (csv-Datei >> Datenbank).
        /// </summary>
        private static void InitData()
        {
            using (IUnitOfWork unitOfWork = new UnitOfWork())
            {
                Log.Information("Import der Spiele und Teams in die Datenbank");

                Log.Information("Datenbank löschen");
                unitOfWork.DeleteDatabase();
                Log.Information("Datenbank migrieren");
                unitOfWork.MigrateDatabase();
                Log.Information("Spiele werden von premierleague.csv eingelesen");
                var games = ImportController.ReadFromCsv().ToArray();
                if (games.Length == 0)
                {
                    Log.Warning("!!! Es wurden keine Spiele eingelesen");
                }
                else
                {
                    Log.Debug($"  Es wurden {games.Count()} Spiele eingelesen!");

                    // TODO: Teams aus den Games ermitteln
                    var teams = games
                        .Select(g => g.HomeTeam)
                        .Distinct()
                        .OrderBy(t => t.Name);

                    Log.Debug($"  Es wurden {teams.Count()} Teams eingelesen!");

                    Log.Information("Daten werden in Datenbank gespeichert (in Context übertragen)");

                    unitOfWork.Games.AddRange(games);
                    // TODO: Teams/Games in der Datenbank speichern
                    Log.Information("Daten wurden in DB gespeichert!");

                    unitOfWork.SaveChanges();
                }
            }
        }

        private static void AnalyzeData()
        {
            using (IUnitOfWork uow = new UnitOfWork())
            {
                (Team, int) maxGoalSum = uow.Teams.GetMaxGoalSum();
                PrintResult("Das Team mit den meisten Toren", $"{maxGoalSum.Item1.ToString()} : {maxGoalSum.Item2}");

                (Team, int) maxGuestGoals = uow.Teams.GetMaxGuestGoals();
                PrintResult("Das Team mit den mesiten Auswärtstoren", $"{maxGuestGoals.Item1.ToString()} : {maxGuestGoals.Item2}");

                (Team, int) maxHomeGoals = uow.Teams.GetMaxHomeGoals();
                PrintResult("Das Team mit den mesiten Auswärtstoren", $"{maxHomeGoals.Item1.ToString()} : {maxHomeGoals.Item2}");

                (Team, int) bestGoalRelation = uow.Teams.GetBestGoalRelation();
                PrintResult("Das Team mit den mesiten Auswärtstoren", $"{bestGoalRelation.Item1.ToString()} : {bestGoalRelation.Item2}");

                List<(Team, double, double, double, double, double, double)> teamPerformance = uow.Teams.GetTeamPerformance();
                ConsoleTableBuilder.From(teamPerformance
                .Select(o => new object[]
                {
                        o.Item1.Name,
                        o.Item2,
                        o.Item3,
                        o.Item4,
                        o.Item5,
                        o.Item6,
                        o.Item7
                })
                .ToList())
                .WithColumn(
                    "Name",
                    "AVG1",
                    "AVG2",
                    "AVG3",
                    "AVG4",
                    "AVG5",
                    "AVG6"
                )
                .WithFormat(ConsoleTableBuilderFormat.MarkDown)
                .WithOptions(new ConsoleTableBuilderOption { DividerString = "" })
                .ExportAndWrite();

                Console.WriteLine();
                Console.WriteLine();

                List<TeamTableRowDto> teamTable = uow.Teams.GetStandingsTable();

                for(int i = 0; i < teamTable.Count; i++)
                {
                    teamTable[i].Rank = i + 1;
                }

                ConsoleTableBuilder
                    .From(teamTable
                    .Select(o => new object[]
                    {
                        o.Id,
                        o.Rank,
                        o.Name,
                        o.Points,
                        o.Matches,
                        o.Won,
                        o.Lost,
                        o.Drawn,
                        o.GoalsFor,
                        o.GoalsAgainst,
                        o.GoalDifference
                    })
                    .ToList())
                    .WithColumn(
                        "ID",
                        "RANK",
                        "NAME",
                        "PUNKTE",
                        "MATCHES",
                        "GEWONNEN",
                        "VERLOREN",
                        "UNENTSCHIEDEN",
                        "TORE GESCHOSSEN",
                        "TORE BEKOMMEN",
                        "TOR DIFFERENZ"
                    )
                    .WithFormat(ConsoleTableBuilderFormat.MarkDown)
                    .WithOptions(new ConsoleTableBuilderOption { DividerString = "" })
                    .ExportAndWrite();
            }
        }

        /// <summary>
        /// Erstellt eine Konsolenausgabe
        /// </summary>
        /// <param name="caption">Enthält die Überschrift</param>
        /// <param name="result">Enthält das ermittelte Ergebnise</param>
        private static void PrintResult(string caption, string result)
        {
            Console.WriteLine();

            if (!string.IsNullOrEmpty(caption))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(new String('=', caption.Length));
                Console.WriteLine(caption);
                Console.WriteLine(new String('=', caption.Length));
                Console.ResetColor();
                Console.WriteLine();
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(result);
            Console.ResetColor();
            Console.WriteLine();
        }


    }
}
