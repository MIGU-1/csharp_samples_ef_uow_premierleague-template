using Microsoft.EntityFrameworkCore;
using PremierLeague.Core.Contracts;
using PremierLeague.Core.DataTransferObjects;
using PremierLeague.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PremierLeague.Persistence
{
    public class TeamRepository : ITeamRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public TeamRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public IEnumerable<Team> GetAllWithGames()
        {
            return _dbContext.Teams.Include(t => t.HomeGames).Include(t => t.AwayGames).ToList();
        }

        public IEnumerable<Team> GetAll()
        {
            return _dbContext.Teams.OrderBy(t => t.Name).ToList();
        }

        public void AddRange(IEnumerable<Team> teams)
        {
            _dbContext.Teams.AddRange(teams);
        }
        public Team Get(int teamId)
        {
            return _dbContext.Teams.Find(teamId);
        }
        public void Add(Team team)
        {
            _dbContext.Teams.Add(team);
        }
        public (Team, int) GetMaxGoalSum() => _dbContext.Teams
                    .Select(t => new
                    {
                        Team = t,
                        Goals = t.AwayGames.Sum(g => g.GuestGoals) +
                                t.HomeGames.Sum(g => g.HomeGoals)
                    })
                    .AsEnumerable()
                    .Select(t => (t.Team, t.Goals))
                    .OrderByDescending(t => t.Goals)
                    .First();
        public (Team, int) GetMaxGuestGoals() => _dbContext.Teams
            .Select(t => new
            {
                Team = t,
                Goals = t.AwayGames.Sum(g => g.GuestGoals)
            })
            .AsEnumerable()
            .Select(t => (t.Team, t.Goals))
            .OrderByDescending(t => t.Goals)
            .First();
        public (Team, int) GetMaxHomeGoals() => _dbContext.Teams
            .Select(t => new
            {
                Team = t,
                Goals = t.HomeGames.Sum(g => g.HomeGoals)
            })
            .AsEnumerable()
            .Select(t => (t.Team, t.Goals))
            .OrderByDescending(t => t.Goals)
            .First();
        public (Team, int) GetBestGoalRelation() => _dbContext.Teams
            .Select(t => new
            {
                Team = t,
                Goals = (t.HomeGames.Sum(g => g.HomeGoals) + t.AwayGames.Sum(g => g.GuestGoals)) -
                        (t.HomeGames.Sum(g => g.GuestGoals) + t.AwayGames.Sum(g => g.HomeGoals))
            })
            .AsEnumerable()
            .Select(t => (t.Team, t.Goals))
            .OrderByDescending(t => t.Goals)
            .First();
        public List<(Team, double, double, double, double, double, double)> GetTeamPerformance() => _dbContext.Teams
            .Select(t => new
            {
                Team = t,
                AvgHomeGoals = (double)t.HomeGames.Sum(g => g.HomeGoals) / t.HomeGames.Count(),
                AvgAwayGoals = (double)t.AwayGames.Sum(g => g.GuestGoals) / t.AwayGames.Count(),
                AvgShotsTotal = (double)(t.HomeGames.Sum(g => g.HomeGoals) + t.AwayGames.Sum(g => g.GuestGoals)) / (t.AwayGames.Count() + t.HomeGames.Count()),
                AvgGoalsGotAtHome = (double)t.HomeGames.Sum(g => g.GuestGoals) / t.HomeGames.Count(),
                AvgGoalsGotOutwards = (double)t.AwayGames.Sum(g => g.HomeGoals) / t.AwayGames.Count(),
                AvgGoalsGotTotal = (double)(t.HomeGames.Sum(g => g.GuestGoals) + t.AwayGames.Sum(g => g.HomeGoals)) / (t.HomeGames.Count() + t.AwayGames.Count())
            })
            .AsEnumerable()
            .Select(t => (t.Team, t.AvgHomeGoals, t.AvgAwayGoals, t.AvgShotsTotal, t.AvgGoalsGotAtHome, t.AvgGoalsGotOutwards, t.AvgGoalsGotTotal))
            .OrderByDescending(t => t.Team.Name)
            .ToList();
        public List<TeamTableRowDto> GetStandingsTable() => _dbContext.Teams
            .Select(t => new TeamTableRowDto()
            {
                Id = t.Id,
                Rank = 0,
                Name = t.Name,
                Matches = t.AwayGames.Count() + t.HomeGames.Count(),

                Won = t.AwayGames.Where(g => g.GuestGoals > g.HomeGoals).Count() +
                        t.HomeGames.Where(g => g.HomeGoals > g.GuestGoals).Count(),

                Lost = t.AwayGames.Where(g => g.HomeGoals > g.GuestGoals).Count() +
                        t.HomeGames.Where(g => g.GuestGoals > g.HomeGoals).Count(),

                GoalsFor = t.AwayGames.Sum(g => g.GuestGoals) +
                            t.HomeGames.Sum(g => g.HomeGoals),

                GoalsAgainst = t.HomeGames.Sum(g => g.GuestGoals) +
                                t.AwayGames.Sum(g => g.HomeGoals),
            })
            .AsEnumerable()
            .OrderByDescending(t => t.Points)
            .ThenByDescending(t => t.GoalDifference)
            .ToList();
    }
}
