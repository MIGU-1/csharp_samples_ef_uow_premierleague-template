using System;
using System.Collections.Generic;
using System.Linq;
using PremierLeague.Core.Entities;
using Utils;

namespace PremierLeague.Core
{
    public static class ImportController
    {
        private const string FILENAME = "PremierLeague.csv";
        private const int IDX_ROUND = 0;
        private const int IDX_HOMETEAM = 1;
        private const int IDX_GUESTTEAM = 2;
        private const int IDX_HOMEGOALS = 3;
        private const int IDX_GUESTGOALS = 4;
        private static List<Team> _teams = new List<Team>();

        public static IEnumerable<Game> ReadFromCsv()
        {
            string[][] lines = MyFile.ReadStringMatrixFromCsv(FILENAME, false);
            ReadAllTeamsFromLines(lines);
            return GetAllGames(lines);
        }
        private static IEnumerable<Game> GetAllGames(string[][] lines)
        {
            List<Game> games = new List<Game>();

            foreach (string[] line in lines)
            {
                Team homeTeam = _teams.Where(t => t.Name == line[IDX_HOMETEAM]).SingleOrDefault();
                Team guestTeam = _teams.Where(t => t.Name == line[IDX_GUESTTEAM]).SingleOrDefault();

                Game newTeam = new Game()
                {
                    Round = Convert.ToInt32(line[IDX_ROUND]),
                    HomeTeam = homeTeam,
                    GuestTeam = guestTeam,
                    HomeGoals = Convert.ToInt32(line[IDX_HOMEGOALS]),
                    GuestGoals = Convert.ToInt32(line[IDX_GUESTGOALS])
                };

                homeTeam.HomeGames.Add(newTeam);
                guestTeam.AwayGames.Add(newTeam);
                games.Add(newTeam);
            }

            return games;
        }
        private static void ReadAllTeamsFromLines(string[][] lines)
        {
            foreach (string[] line in lines)
            {
                string TeamName = line[IDX_HOMETEAM];

                if (_teams.Where(t => t.Name == TeamName).SingleOrDefault() == null)
                {
                    Team newTeam = new Team()
                    {
                        Name = TeamName
                    };

                    _teams.Add(newTeam);
                }
            }
        }
    }
}
