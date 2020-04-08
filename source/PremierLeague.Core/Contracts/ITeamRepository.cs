using PremierLeague.Core.DataTransferObjects;
using PremierLeague.Core.Entities;
using System.Collections.Generic;

namespace PremierLeague.Core.Contracts
{
    public interface ITeamRepository
    {
        IEnumerable<Team> GetAllWithGames();
        IEnumerable<Team> GetAll();
        void AddRange(IEnumerable<Team> teams);
        Team Get(int teamId);
        void Add(Team team);
        (Team, int) GetMaxGoalSum();
        (Team, int) GetMaxGuestGoals();
        (Team, int) GetMaxHomeGoals();
        (Team, int) GetBestGoalRelation();
        List<(Team, double, double, double, double, double, double)> GetTeamPerformance();
        List<TeamTableRowDto> GetStandingsTable();
    }
}