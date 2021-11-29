using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class League {
    public string leagueName;
    public int leagueLevel;
    public List<Team> teams = new List<Team>();
    public List<Match> schedule;

    public void InitLeague() {
        NewSeason();

    }
    public void EndSeason() {
        //save stats
        //reset currentStats
        //relegate and promote
    }
    public void NewSeason() {
        teams = new List<Team>();
        if (Game.instance.playerStable.leagueLevel == leagueLevel) {
            teams.Add(new Team() { stable = Game.instance.playerStable, isPlayer = true });
        }
        foreach (var s in Game.instance.otherStables) {
            if (s.leagueLevel == leagueLevel) {
                teams.Add(new Team() { stable = s });
            }
        }
        schedule = new List<Match>();
        for (int i = 1; i <= 12; i++) {
            schedule.Add(new Match() { home = teams[0], away = teams[UnityEngine.Random.Range(1, teams.Count - 1)], date = new Game.GameDate() { day = 5, month = i, year = Game.instance.gameDate.year } });
        }
        CreateSchedule();
    }
    public List<Team> temp;
    public List<Team> thisteams;
    public void CreateSchedule() {
        string results = null;
        List<Match> matchList = new List<Match>();
        temp = new List<Team>();
        thisteams = new List<Team>();
        thisteams.AddRange(teams);
        temp.AddRange(teams);
        thisteams.RemoveAt(0);

        int teamSize = thisteams.Count;
        int numDays = teams.Count - 1;
        int halfsize = teams.Count / 2;
        Game.GameDate matchDay = new Game.GameDate() { year = Helper.Today().year, day = 5, month = 1 };
        for (int day = 0; day < numDays * 2; day++) {
            if (day % 2 == 0) {
                results += String.Format("\n\nDay {0}\n", (day + 1));
                int teamIdx = day % teamSize;

                results += String.Format("{0} vs {1}\n", thisteams[teamIdx].stable.stableName, temp[0].stable.stableName);
                matchList.Add(new Match() { away = GetTeam(thisteams[teamIdx].stable), home = GetTeam(temp[0].stable), date = matchDay.Add((day * 10)) });
                for (int idx = 0; idx < halfsize; idx++) {
                    int firstTeam = (day + idx) % teamSize;
                    int secondTeam = ((day + teamSize) - idx) % teamSize;

                    if (firstTeam != secondTeam) {
                        results += String.Format("{0} vs {1}\n", thisteams[firstTeam].stable.stableName, thisteams[secondTeam].stable.stableName);
                        matchList.Add(new Match() { away = GetTeam(thisteams[firstTeam].stable), home = GetTeam(thisteams[secondTeam].stable), date = matchDay.Add((day * 10)) });
                    }
                }
            }

            if (day % 2 != 0) {
                int teamIdx = day % teamSize;

                results += String.Format("\n\nDay {0}\n", (day + 1));

                results += String.Format("{0} vs {1}\n", temp[0].stable.stableName, thisteams[teamIdx].stable.stableName);
                matchList.Add(new Match() { home = GetTeam(thisteams[teamIdx].stable), away = GetTeam(temp[0].stable), date = matchDay.Add((day * 10)) });
                for (int idx = 0; idx < halfsize; idx++) {
                    int firstTeam = (day + idx) % teamSize;
                    int secondTeam = ((day + teamSize) - idx) % teamSize;

                    if (firstTeam != secondTeam) {
                        results += String.Format("{0} vs {1}\n", thisteams[secondTeam].stable.stableName, thisteams[firstTeam].stable.stableName);
                        matchList.Add(new Match() { home = GetTeam(thisteams[firstTeam].stable), away = GetTeam(thisteams[secondTeam].stable), date = matchDay.Add((day * 10)) });
                    }
                }
            }
        }
        schedule = matchList;
        Debug.Log(results);
    }
    Team GetTeam(Stable stable) {
        foreach (Team t in teams) {
            if (t.stable == stable) {
                return t;
            }
        }
        return null;
    }

    public List<string> GetTable() {
        var thisTable = new List<string>();
        var tempTable = new List<Team>();
        foreach (Team t in teams) {
            tempTable.Add(t);
        }
        tempTable.Sort((a, b) => a.points.CompareTo(b.points));
        tempTable.Reverse();
        thisTable.Add($"{"Stable Name____________________",8}" + $"{"Wins",10}" +$"{"Losses",10}"+ $"{"Draws",10}" + $"{"Points",10}" + $"{"Goals",10}" + $"{"GAllowed",10}");
        foreach (Team y in tempTable) {
            thisTable.Add(y.GetTableInfo());
        }
        return thisTable;
    }

    [System.Serializable]
    public class Match {
        public Game.GameDate date;
        public Team home;
        public Team away;
        public int homeGoals;
        public int awayGoals;
        public bool final;
        public void ProcessResult() {
            if (homeGoals == awayGoals) {
                home.draws++;
                away.draws++;
            }
            else if (homeGoals > awayGoals) {
                home.wins++;
                away.losses++;
            }
            else {
                home.losses++;
                away.wins++;
            }
            home.goals += homeGoals;
            away.goals += awayGoals;
            home.goalsAllowed += awayGoals;
            away.goalsAllowed += homeGoals;
        }
        public bool IsPlayerMatch() {
            return (away.isPlayer || home.isPlayer);
        }
    }
    [System.Serializable]
    public class Team {
        public Stable stable;
        public int wins, losses, draws;
        public int points { get { return (wins * 3) + (draws * 1); } }
        public int goals, goalsAllowed;
        public bool isPlayer;

        public string GetTableInfo() {
            string stableNameMod = stable.stableName;
            for (int i=0; i<30-stable.stableName.Length; i++) {
                stableNameMod += "_";
            }
            return $"{stableNameMod,8}" + $"{wins,12}" +$"{losses,12}"+ $"{draws,12}" + $"{points,10}" + $"{goals,15}" + $"{goalsAllowed,12}";
        }
    }
    
}
