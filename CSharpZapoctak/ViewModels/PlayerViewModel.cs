﻿using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Others;
using CSharpZapoctak.Stores;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using static CSharpZapoctak.ViewModels.MatchesSelectionViewModel;

namespace CSharpZapoctak.ViewModels
{
    class PlayerInMatchStats : NotifyPropertyChanged
    {
        #region Properties
        private DateTime datetime;
        public DateTime Datetime
        {
            get { return datetime; }
            set
            {
                datetime = value;
                OnPropertyChanged();
            }
        }

        private int goals = 0;
        public int Goals
        {
            get { return goals; }
            set
            {
                goals = value;
                OnPropertyChanged();
            }
        }

        private int assists = 0;
        public int Assists
        {
            get { return assists; }
            set
            {
                assists = value;
                OnPropertyChanged();
            }
        }

        private int points = 0;
        public int Points
        {
            get { return points; }
            set
            {
                points = value;
                OnPropertyChanged();
            }
        }

        private int penaltyMinutes = 0;
        public int PenaltyMinutes
        {
            get { return penaltyMinutes; }
            set
            {
                penaltyMinutes = value;
                OnPropertyChanged();
            }
        }
        #endregion
    }

    class PlayerStats : NotifyPropertyChanged, IStats
    {
        #region Properties
        private int gamesPlayed = 0;
        public int GamesPlayed
        {
            get { return gamesPlayed; }
            set
            {
                gamesPlayed = value;
                OnPropertyChanged();
            }
        }

        private int goals = 0;
        public int Goals
        {
            get { return goals; }
            set
            {
                goals = value;
                OnPropertyChanged();
            }
        }

        private int assists = 0;
        public int Assists
        {
            get { return assists; }
            set
            {
                assists = value;
                OnPropertyChanged();
            }
        }

        private int points = 0;
        public int Points
        {
            get { return points; }
            set
            {
                points = value;
                OnPropertyChanged();
            }
        }

        private int penaltyMinutes = 0;
        public int PenaltyMinutes
        {
            get { return penaltyMinutes; }
            set
            {
                penaltyMinutes = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public PlayerStats(int gamesPlayed, int goals, int assists, int penaltyMinutes)
        {
            this.gamesPlayed = gamesPlayed;
            this.goals = goals;
            this.assists = assists;
            this.penaltyMinutes = penaltyMinutes;
            this.points = goals + assists;
        }

        public PlayerStats(Player player, int seasonID, int competitionID)
        {
            CalculateStats(player.id, seasonID, competitionID).Await();
        }

        public async Task CalculateStats(int playerID, int seasonID, int competitionID)
        {
            List<Task> tasks = new List<Task>();
            tasks.Add(Task.Run(() => CountGamesPlayed(playerID, seasonID, competitionID)));
            tasks.Add(Task.Run(() => CountGoals(playerID, seasonID, competitionID)));
            tasks.Add(Task.Run(() => CountAssists(playerID, seasonID, competitionID)));
            tasks.Add(Task.Run(() => CountPenaltyMinutes(playerID, seasonID, competitionID)));
            await Task.WhenAll(tasks);
            Points = Goals + Assists;
        }

        public async Task CountGamesPlayed(int playerID, int seasonID, int competitionID)
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) " +
                                                "FROM player_matches " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "INNER JOIN seasons AS s ON s.id = m.season_id " +
                                                "WHERE player_id = " + playerID, connection);
            if (seasonID > 0) { cmd.CommandText += " AND m.season_id = " + seasonID; }
            if (competitionID > 0) { cmd.CommandText += " AND s.competition_id = " + competitionID; }
            try
            {
                connection.Open();
                GamesPlayed = (int)(long)await cmd.ExecuteScalarAsync();
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                connection.Close();
            }
        }

        public async Task CountGoals(int playerID, int seasonID, int competitionID)
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) " +
                                                "FROM goals " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "INNER JOIN seasons AS s ON s.id = m.season_id " +
                                                "WHERE player_id = " + playerID, connection);
            if (seasonID > 0) { cmd.CommandText += " AND m.season_id = " + seasonID; }
            if (competitionID > 0) { cmd.CommandText += " AND s.competition_id = " + competitionID; }
            try
            {
                connection.Open();
                Goals = (int)(long)await cmd.ExecuteScalarAsync();
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                connection.Close();
            }
        }

        public async Task CountAssists(int playerID, int seasonID, int competitionID)
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) " +
                                                "FROM goals " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "INNER JOIN seasons AS s ON s.id = m.season_id " +
                                                "WHERE assist_player_id = " + playerID, connection);
            if (seasonID > 0) { cmd.CommandText += " AND m.season_id = " + seasonID; }
            if (competitionID > 0) { cmd.CommandText += " AND s.competition_id = " + competitionID; }
            try
            {
                connection.Open();
                Assists = (int)(long)await cmd.ExecuteScalarAsync();
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                connection.Close();
            }
        }

        public async Task CountPenaltyMinutes(int playerID, int seasonID, int competitionID)
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT COALESCE(SUM(p.minutes), 0) " +
                                                "FROM penalties " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "INNER JOIN seasons AS s ON s.id = m.season_id " +
                                                "INNER JOIN penalty_type AS p ON p.code = penalty_type_id " +
                                                "WHERE player_id = " + playerID, connection);
            if (seasonID > 0) { cmd.CommandText += " AND m.season_id = " + seasonID; }
            if (competitionID > 0) { cmd.CommandText += " AND s.competition_id = " + competitionID; }
            try
            {
                connection.Open();
                PenaltyMinutes = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                connection.Close();
            }
        }
    }

    class GoalieStats : NotifyPropertyChanged, IStats
    {
        #region Properties
        private int gamesPlayed = 0;
        public int GamesPlayed
        {
            get { return gamesPlayed; }
            set
            {
                gamesPlayed = value;
                OnPropertyChanged();
            }
        }

        private int shutouts = 0;
        public int Shutouts
        {
            get { return shutouts; }
            set
            {
                shutouts = value;
                OnPropertyChanged();
            }
        }

        private int timeInGame = 0;
        public int TimeInGame
        {
            get { return timeInGame; }
            set
            {
                timeInGame = value;
                OnPropertyChanged();
            }
        }

        private string timeInGameText = "0:00";
        public string TimeInGameText
        {
            get { return timeInGameText; }
            set
            {
                timeInGameText = value;
                OnPropertyChanged();
            }
        }

        private string timeInGamePerGame = "0:00";
        public string TimeInGamePerGame
        {
            get { return timeInGamePerGame; }
            set
            {
                timeInGamePerGame = value;
                OnPropertyChanged();
            }
        }

        private int goalsAgainst = 0;
        public int GoalsAgainst
        {
            get { return goalsAgainst; }
            set
            {
                goalsAgainst = value;
                OnPropertyChanged();
            }
        }

        private float goalsAgainstPerGame = 0;
        public float GoalsAgainstPerGame
        {
            get { return goalsAgainstPerGame; }
            set
            {
                goalsAgainstPerGame = value;
                OnPropertyChanged();
            }
        }

        private string timeInGamePerGoalAgainst = "0:00";
        public string TimeInGamePerGoalAgainst
        {
            get { return timeInGamePerGoalAgainst; }
            set
            {
                timeInGamePerGoalAgainst = value;
                OnPropertyChanged();
            }
        }

        private int penaltyShotsAgainst = 0;
        public int PenaltyShotsAgainst
        {
            get { return penaltyShotsAgainst; }
            set
            {
                penaltyShotsAgainst = value;
                OnPropertyChanged();
            }
        }

        private int penaltyShotGoalsAgainst = 0;
        public int PenaltyShotGoalsAgainst
        {
            get { return penaltyShotGoalsAgainst; }
            set
            {
                penaltyShotGoalsAgainst = value;
                OnPropertyChanged();
            }
        }

        private float penaltyShotsPercentage = 0;
        public float PenaltyShotsPercentage
        {
            get { return penaltyShotsPercentage; }
            set
            {
                penaltyShotsPercentage = value;
                OnPropertyChanged();
            }
        }

        private int shootoutShotsAgainst = 0;
        public int ShootoutShotsAgainst
        {
            get { return shootoutShotsAgainst; }
            set
            {
                shootoutShotsAgainst = value;
                OnPropertyChanged();
            }
        }

        private int shootoutShotGoalsAgainst = 0;
        public int ShootoutShotGoalsAgainst
        {
            get { return shootoutShotGoalsAgainst; }
            set
            {
                shootoutShotGoalsAgainst = value;
                OnPropertyChanged();
            }
        }

        private float shootoutShotsPercentage = 0;
        public float ShootoutShotsPercentage
        {
            get { return shootoutShotsPercentage; }
            set
            {
                shootoutShotsPercentage = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public GoalieStats(Player player, int seasonID, int competitionID)
        {
            CalculateStats(player.id, seasonID, competitionID).Await();
        }

        public async Task CalculateStats(int playerID, int seasonID, int competitionID)
        {
            List<Task> tasks = new List<Task>();
            tasks.Add(Task.Run(() => CountGamesPlayed(playerID, seasonID, competitionID)));
            tasks.Add(Task.Run(() => CountGoalsAgainst(playerID, seasonID, competitionID)));
            tasks.Add(Task.Run(() => CountShutouts(playerID, seasonID, competitionID)));
            tasks.Add(Task.Run(() => CountTimeInGame(playerID, seasonID, competitionID)));
            tasks.Add(Task.Run(() => CountPenaltyShots(playerID, seasonID, competitionID)));
            tasks.Add(Task.Run(() => CountPenaltyShotGoalsAgainst(playerID, seasonID, competitionID)));
            tasks.Add(Task.Run(() => CountShootoutShots(playerID, seasonID, competitionID)));
            tasks.Add(Task.Run(() => CountShootoutShotGoalsAgainst(playerID, seasonID, competitionID)));
            await Task.WhenAll(tasks);
            TimeInGameText = (TimeInGame / 60) + ":" + (TimeInGame % 60).ToString("00");
            if (GamesPlayed != 0)
            {
                TimeInGamePerGame = (TimeInGame / GamesPlayed / 60) + ":" + (TimeInGame / GamesPlayed % 60).ToString("00");
                GoalsAgainstPerGame = (float)Math.Round((float)GoalsAgainst / (float)GamesPlayed, 2);
            }
            TimeInGamePerGoalAgainst = GoalsAgainst != 0 ? (TimeInGame / GoalsAgainst / 60) + ":" + (TimeInGame / GoalsAgainst % 60).ToString("00") : "no goal against";
            PenaltyShotsPercentage = PenaltyShotsAgainst != 0 ? (float)Math.Round((float)(PenaltyShotsAgainst - PenaltyShotGoalsAgainst) / (float)PenaltyShotsAgainst * 100, 2) : float.NaN;
            ShootoutShotsPercentage = ShootoutShotsAgainst != 0 ? (float)Math.Round((float)(ShootoutShotsAgainst - ShootoutShotGoalsAgainst) / (float)ShootoutShotsAgainst * 100, 2) : float.NaN;
        }

        public async Task CountGamesPlayed(int playerID, int seasonID, int competitionID)
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) " +
                                                "FROM goalie_matches " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "INNER JOIN seasons AS s ON s.id = m.season_id " +
                                                "WHERE player_id = " + playerID, connection);
            if (seasonID > 0) { cmd.CommandText += " AND m.season_id = " + seasonID; }
            if (competitionID > 0) { cmd.CommandText += " AND s.competition_id = " + competitionID; }
            try
            {
                connection.Open();
                GamesPlayed = (int)(long)await cmd.ExecuteScalarAsync();
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                connection.Close();
            }
        }

        public async Task CountGoalsAgainst(int playerID, int seasonID, int competitionID)
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) " +
                                                "FROM goals " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "INNER JOIN seasons AS s ON s.id = m.season_id " +
                                                "WHERE goalie_id = " + playerID, connection);
            if (seasonID > 0) { cmd.CommandText += " AND m.season_id = " + seasonID; }
            if (competitionID > 0) { cmd.CommandText += " AND s.competition_id = " + competitionID; }
            try
            {
                connection.Open();
                GoalsAgainst = (int)(long)await cmd.ExecuteScalarAsync();
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                connection.Close();
            }
        }

        public async Task CountShutouts(int playerID, int seasonID, int competitionID)
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) " +
                                                "FROM shutouts " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "INNER JOIN seasons AS s ON s.id = m.season_id " +
                                                "WHERE goalie_id = " + playerID, connection);
            if (seasonID > 0) { cmd.CommandText += " AND m.season_id = " + seasonID; }
            if (competitionID > 0) { cmd.CommandText += " AND s.competition_id = " + competitionID; }
            try
            {
                connection.Open();
                Shutouts = (int)(long)await cmd.ExecuteScalarAsync();
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                connection.Close();
            }
        }

        public async Task CountTimeInGame(int playerID, int seasonID, int competitionID)
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT COALESCE(SUM(duration), 0) " +
                                                "FROM shifts " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "INNER JOIN seasons AS s ON s.id = m.season_id " +
                                                "WHERE player_id = " + playerID, connection);
            if (seasonID > 0) { cmd.CommandText += " AND m.season_id = " + seasonID; }
            if (competitionID > 0) { cmd.CommandText += " AND s.competition_id = " + competitionID; }
            try
            {
                connection.Open();
                TimeInGame = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                connection.Close();
            }
        }

        public async Task CountPenaltyShots(int playerID, int seasonID, int competitionID)
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) " +
                                                "FROM penalty_shots " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "INNER JOIN seasons AS s ON s.id = m.season_id " +
                                                "WHERE goalie_id = " + playerID, connection);
            if (seasonID > 0) { cmd.CommandText += " AND m.season_id = " + seasonID; }
            if (competitionID > 0) { cmd.CommandText += " AND s.competition_id = " + competitionID; }
            try
            {
                connection.Open();
                PenaltyShotsAgainst = (int)(long)await cmd.ExecuteScalarAsync();
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                connection.Close();
            }
        }

        public async Task CountPenaltyShotGoalsAgainst(int playerID, int seasonID, int competitionID)
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) " +
                                                "FROM penalty_shots " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "INNER JOIN seasons AS s ON s.id = m.season_id " +
                                                "WHERE was_goal = 1 AND goalie_id = " + playerID, connection);
            if (seasonID > 0) { cmd.CommandText += " AND m.season_id = " + seasonID; }
            if (competitionID > 0) { cmd.CommandText += " AND s.competition_id = " + competitionID; }
            try
            {
                connection.Open();
                PenaltyShotGoalsAgainst = (int)(long)await cmd.ExecuteScalarAsync();
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                connection.Close();
            }
        }

        public async Task CountShootoutShots(int playerID, int seasonID, int competitionID)
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) " +
                                                "FROM shootout_shots " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "INNER JOIN seasons AS s ON s.id = m.season_id " +
                                                "WHERE goalie_id = " + playerID, connection);
            if (seasonID > 0) { cmd.CommandText += " AND m.season_id = " + seasonID; }
            if (competitionID > 0) { cmd.CommandText += " AND s.competition_id = " + competitionID; }
            try
            {
                connection.Open();
                ShootoutShotsAgainst = (int)(long)await cmd.ExecuteScalarAsync();
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                connection.Close();
            }
        }

        public async Task CountShootoutShotGoalsAgainst(int playerID, int seasonID, int competitionID)
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) " +
                                                "FROM shootout_shots " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "INNER JOIN seasons AS s ON s.id = m.season_id " +
                                                "WHERE was_goal = 1 AND goalie_id = " + playerID, connection);
            if (seasonID > 0) { cmd.CommandText += " AND m.season_id = " + seasonID; }
            if (competitionID > 0) { cmd.CommandText += " AND s.competition_id = " + competitionID; }
            try
            {
                connection.Open();
                ShootoutShotGoalsAgainst = (int)(long)await cmd.ExecuteScalarAsync();
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                connection.Close();
            }
        }
    }

    class CompetitionRecord<T> : NotifyPropertyChanged where T : IStats
    {
        private Competition competition;
        public Competition Competition
        {
            get { return competition; }
            set
            {
                competition = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<T> stats;
        public ObservableCollection<T> Stats
        {
            get { return stats; }
            set
            {
                stats = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<SeasonRecord<T>> seasons = new ObservableCollection<SeasonRecord<T>>();
        public ObservableCollection<SeasonRecord<T>> Seasons
        {
            get { return seasons; }
            set
            {
                seasons = value;
                OnPropertyChanged();
            }
        }

        private Visibility competitionVisibility = Visibility.Collapsed;
        public Visibility CompetitionVisibility
        {
            get { return competitionVisibility; }
            set
            {
                competitionVisibility = value;
                OnPropertyChanged();
            }
        }


        private ICommand setCompetitionVisibilityCommand;
        public ICommand SetCompetitionVisibilityCommand
        {
            get
            {
                if (setCompetitionVisibilityCommand == null)
                {
                    setCompetitionVisibilityCommand = new RelayCommand(param => SetCompetitionVisibility());
                }
                return setCompetitionVisibilityCommand;
            }
        }

        private void SetCompetitionVisibility()
        {
            if (CompetitionVisibility == Visibility.Visible)
            {
                CompetitionVisibility = Visibility.Collapsed;
            }
            else
            {
                CompetitionVisibility = Visibility.Visible;
            }
        }
    }

    class SeasonRecord<T> : NotifyPropertyChanged where T : IStats
    {
        private Season season;
        public Season Season
        {
            get { return season; }
            set
            {
                season = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<T> stats;
        public ObservableCollection<T> Stats
        {
            get { return stats; }
            set
            {
                stats = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Match> matches = new ObservableCollection<Match>();
        public ObservableCollection<Match> Matches
        {
            get { return matches; }
            set
            {
                matches = value;
                OnPropertyChanged();
            }
        }

        private Visibility seasonVisibility = Visibility.Collapsed;
        public Visibility SeasonVisibility
        {
            get { return seasonVisibility; }
            set
            {
                seasonVisibility = value;
                OnPropertyChanged();
            }
        }


        private ICommand setSeasonVisibilityCommand;
        public ICommand SetSeasonVisibilityCommand
        {
            get
            {
                if (setSeasonVisibilityCommand == null)
                {
                    setSeasonVisibilityCommand = new RelayCommand(param => SetSeasonVisibility());
                }
                return setSeasonVisibilityCommand;
            }
        }

        private void SetSeasonVisibility()
        {
            if (SeasonVisibility == Visibility.Visible)
            {
                SeasonVisibility = Visibility.Collapsed;
            }
            else
            {
                SeasonVisibility = Visibility.Visible;
            }
        }
    }

    class PlayerViewModel : NotifyPropertyChanged
    {
        private Player player;
        public Player Player
        {
            get { return player; }
            set
            {
                player = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<CompetitionRecord<PlayerStats>> competitions = new ObservableCollection<CompetitionRecord<PlayerStats>>();
        public ObservableCollection<CompetitionRecord<PlayerStats>> Competitions
        {
            get { return competitions; }
            set
            {
                competitions = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<CompetitionRecord<GoalieStats>> competitionsAsGoalie = new ObservableCollection<CompetitionRecord<GoalieStats>>();
        public ObservableCollection<CompetitionRecord<GoalieStats>> CompetitionsAsGoalie
        {
            get { return competitionsAsGoalie; }
            set
            {
                competitionsAsGoalie = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<PlayerInMatchStats> playerInMatchStats = new ObservableCollection<PlayerInMatchStats>();
        public ObservableCollection<PlayerInMatchStats> PlayerInMatchStats
        {
            get { return playerInMatchStats; }
            set
            {
                playerInMatchStats = value;
                OnPropertyChanged();
            }
        }

        public Func<double, string> AxisFormatterScatter { get; set; } = value => value.ToString("N2");

        public string[] DatetimeXLabels { get; set; }

        private SeriesCollection playerInMatchStatsSeries = new SeriesCollection();
        public SeriesCollection PlayerInMatchStatsSeries
        {
            get { return playerInMatchStatsSeries; }
            set
            {
                playerInMatchStatsSeries = value;
                OnPropertyChanged();
            }
        }

        private SeriesCollection playerInMatchStatsSumSeries = new SeriesCollection();
        public SeriesCollection PlayerInMatchStatsSumSeries
        {
            get { return playerInMatchStatsSumSeries; }
            set
            {
                playerInMatchStatsSumSeries = value;
                OnPropertyChanged();
            }
        }

        private SeriesCollection playerInMatchStatsAverageSeries = new SeriesCollection();
        public SeriesCollection PlayerInMatchStatsAverageSeries
        {
            get { return playerInMatchStatsAverageSeries; }
            set
            {
                playerInMatchStatsAverageSeries = value;
                OnPropertyChanged();
            }
        }

        private ICommand exportChartCommand;
        public ICommand ExportChartCommand
        {
            get
            {
                if (exportChartCommand == null)
                {
                    exportChartCommand = new RelayCommand(param => Exports.ExportControlToImage((FrameworkElement)param));
                }
                return exportChartCommand;
            }
        }

        public ICommand NavigateMatchCommand { get; set; }

        private ICommand checkNavigateMatchCommand;
        public ICommand CheckNavigateMatchCommand
        {
            get
            {
                if (checkNavigateMatchCommand == null)
                {
                    checkNavigateMatchCommand = new RelayCommand(param => CheckNavigateMatch());
                }
                return checkNavigateMatchCommand;
            }
        }

        public Match SelectedMatch { get; set; } = null;

        public ICommand NavigateEditCommand { get; }

        public PlayerViewModel(NavigationStore ns, Player p)
        {
            NavigateMatchCommand = new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, new MatchViewModel(ns, SelectedMatch, new PlayerViewModel(ns, p))));
            NavigateEditCommand = new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, new EditPlayerViewModel(ns, p)));
            Player = p;
            if (string.IsNullOrEmpty(p.PhotoPath)) { p.PhotoPath = p.Gender == "M" ? SportsData.ResourcesPath + "\\male.png" : SportsData.ResourcesPath + "\\female.png"; }
            LoadCompetitions();
            LoadCompetitionsAsGoalie();
            LoadPlayerInMatchStats();
            LoadPlayerInMatchStatsSeries();
            LoadPlayerInMatchStatsSumSeries();
            LoadPlayerInMatchStatsAverageSeries();
        }

        private void LoadCompetitions()
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT s.competition_id AS competition_id, c.name AS competition_name " +
                                                "FROM player_enlistment " +
                                                "INNER JOIN seasons AS s ON s.id = season_id " +
                                                "INNER JOIN competitions AS c ON c.id = s.competition_id " +
                                                "WHERE player_id = " + Player.id + " GROUP BY s.competition_id", connection);

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                foreach (DataRow row in dataTable.Rows)
                {
                    Competition c = new Competition { id = int.Parse(row["competition_id"].ToString()), Name = row["competition_name"].ToString() };

                    CompetitionRecord<PlayerStats> cr = new CompetitionRecord<PlayerStats>
                    {
                        Competition = c,
                        Stats = new ObservableCollection<PlayerStats> { new PlayerStats(Player, -1, c.id) }
                    };

                    //load seasons
                    cmd = new MySqlCommand("SELECT s.id AS season_id, s.name AS season_name " +
                                                "FROM player_enlistment " +
                                                "INNER JOIN seasons AS s ON s.id = season_id " +
                                                "INNER JOIN competitions AS c ON c.id = s.competition_id " +
                                                "WHERE player_id = " + Player.id + " AND s.competition_id = " + c.id, connection);

                    DataTable seasonTable = new DataTable();
                    seasonTable.Load(cmd.ExecuteReader());

                    foreach (DataRow ssn in seasonTable.Rows)
                    {
                        Season s = new Season { id = int.Parse(ssn["season_id"].ToString()), Name = ssn["season_name"].ToString() };

                        SeasonRecord<PlayerStats> sr = new SeasonRecord<PlayerStats>
                        {
                            Season = s,
                            Stats = new ObservableCollection<PlayerStats> { new PlayerStats(Player, s.id, c.id) }
                        };

                        //load matches
                        cmd = new MySqlCommand("SELECT h.name AS h_name, a.name AS a_name, " +
                                               "m.id AS match_id, m.datetime AS match_datetime, m.home_score AS h_score, m.away_score AS a_score, " +
                                               "m.overtime AS match_overtime, m.shootout AS match_shootout, m.forfeit AS match_forfeit, " +
                                               "m.qualification_id AS match_qualification_id, m.serie_match_number AS match_serie_match_number " +
                                               "FROM player_matches " +
                                               "INNER JOIN matches AS m ON m.id = match_id " +
                                               "INNER JOIN seasons AS s ON s.id = m.season_id " +
                                               "INNER JOIN competitions AS c ON c.id = s.competition_id " +
                                               "INNER JOIN team AS h ON h.id = m.home_competitor " +
                                               "INNER JOIN team AS a ON a.id = m.away_competitor " +
                                               "WHERE player_id = " + Player.id + " AND s.competition_id = " + c.id + " AND m.season_id = " + s.id + " " +
                                               "ORDER BY m.datetime DESC", connection);

                        DataTable matchTable = new DataTable();
                        matchTable.Load(cmd.ExecuteReader());

                        foreach (DataRow mtch in matchTable.Rows)
                        {
                            Team home = new Team();
                            home.Name = mtch["h_name"].ToString();
                            Team away = new Team();
                            away.Name = mtch["a_name"].ToString();

                            Match m = new Match
                            {
                                id = int.Parse(mtch["match_id"].ToString()),
                                Competition = c,
                                Season = s,
                                Datetime = DateTime.Parse(mtch["match_datetime"].ToString()),
                                HomeTeam = home,
                                AwayTeam = away,
                                HomeScore = int.Parse(mtch["h_score"].ToString()),
                                AwayScore = int.Parse(mtch["a_score"].ToString()),
                                Overtime = Convert.ToBoolean(int.Parse(mtch["match_overtime"].ToString())),
                                Shootout = Convert.ToBoolean(int.Parse(mtch["match_shootout"].ToString())),
                                Forfeit = Convert.ToBoolean(int.Parse(mtch["match_forfeit"].ToString()))
                            };

                            string partOfSeason = "Play-off";
                            if (int.Parse(mtch["match_serie_match_number"].ToString()) < 1)
                            {
                                partOfSeason = "Group";
                            }
                            else if (int.Parse(mtch["match_qualification_id"].ToString()) > 0)
                            {
                                partOfSeason = "Qualification";
                            }

                            m.Stats = new MatchStats(m);
                            ((MatchStats)m.Stats).PartOfSeason = partOfSeason;
                            ((MatchStats)m.Stats).Score = m.Score();

                            sr.Matches.Add(m);
                        }

                        cr.Seasons.Add(sr);
                    }

                    Competitions.Add(cr);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                connection.Close();
            }
        }

        private void LoadCompetitionsAsGoalie()
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT s.competition_id AS competition_id, c.name AS competition_name " +
                                                "FROM goalie_matches " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "INNER JOIN seasons AS s ON s.id = m.season_id " +
                                                "INNER JOIN competitions AS c ON c.id = s.competition_id " +
                                                "WHERE player_id = " + Player.id + " GROUP BY competition_id", connection);
            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                foreach (DataRow row in dataTable.Rows)
                {
                    Competition c = new Competition { id = int.Parse(row["competition_id"].ToString()), Name = row["competition_name"].ToString() };

                    CompetitionRecord<GoalieStats> cr = new CompetitionRecord<GoalieStats>
                    {
                        Competition = c,
                        Stats = new ObservableCollection<GoalieStats> { new GoalieStats(Player, -1, c.id) }
                    };

                    //load seasons
                    cmd = new MySqlCommand("SELECT s.id AS season_id, s.name AS season_name " +
                                                "FROM goalie_matches " +
                                                "INNER JOIN matches AS m ON m.id = match_id " +
                                                "INNER JOIN seasons AS s ON s.id = m.season_id " +
                                                "WHERE player_id = " + Player.id + " GROUP BY season_id", connection);

                    DataTable seasonTable = new DataTable();
                    seasonTable.Load(cmd.ExecuteReader());

                    foreach (DataRow ssn in seasonTable.Rows)
                    {
                        Season s = new Season { id = int.Parse(ssn["season_id"].ToString()), Name = ssn["season_name"].ToString() };

                        SeasonRecord<GoalieStats> sr = new SeasonRecord<GoalieStats>
                        {
                            Season = s,
                            Stats = new ObservableCollection<GoalieStats> { new GoalieStats(Player, s.id, c.id) }
                        };

                        //load matches
                        cmd = new MySqlCommand("SELECT h.name AS h_name, a.name AS a_name, " +
                                               "m.id AS match_id, m.datetime AS match_datetime, m.home_score AS h_score, m.away_score AS a_score, " +
                                               "m.overtime AS match_overtime, m.shootout AS match_shootout, m.forfeit AS match_forfeit, " +
                                               "m.qualification_id AS match_qualification_id, m.serie_match_number AS match_serie_match_number " +
                                               "FROM goalie_matches " +
                                               "INNER JOIN matches AS m ON m.id = match_id " +
                                               "INNER JOIN seasons AS s ON s.id = m.season_id " +
                                               "INNER JOIN competitions AS c ON c.id = s.competition_id " +
                                               "INNER JOIN team AS h ON h.id = m.home_competitor " +
                                               "INNER JOIN team AS a ON a.id = m.away_competitor " +
                                               "WHERE player_id = " + Player.id + " AND s.competition_id = " + c.id + " AND m.season_id = " + s.id + " " +
                                               "ORDER BY m.datetime DESC", connection);

                        DataTable matchTable = new DataTable();
                        matchTable.Load(cmd.ExecuteReader());

                        foreach (DataRow mtch in matchTable.Rows)
                        {
                            Team home = new Team();
                            home.Name = mtch["h_name"].ToString();
                            Team away = new Team();
                            away.Name = mtch["a_name"].ToString();

                            Match m = new Match
                            {
                                id = int.Parse(mtch["match_id"].ToString()),
                                Competition = c,
                                Season = s,
                                Datetime = DateTime.Parse(mtch["match_datetime"].ToString()),
                                HomeTeam = home,
                                AwayTeam = away,
                                HomeScore = int.Parse(mtch["h_score"].ToString()),
                                AwayScore = int.Parse(mtch["a_score"].ToString()),
                                Overtime = Convert.ToBoolean(int.Parse(mtch["match_overtime"].ToString())),
                                Shootout = Convert.ToBoolean(int.Parse(mtch["match_shootout"].ToString())),
                                Forfeit = Convert.ToBoolean(int.Parse(mtch["match_forfeit"].ToString()))
                            };

                            string partOfSeason = "Play-off";
                            if (int.Parse(mtch["match_serie_match_number"].ToString()) < 1)
                            {
                                partOfSeason = "Group";
                            }
                            else if (int.Parse(mtch["match_qualification_id"].ToString()) > 0)
                            {
                                partOfSeason = "Qualification";
                            }

                            m.Stats = new MatchStats(m);
                            ((MatchStats)m.Stats).PartOfSeason = partOfSeason;
                            ((MatchStats)m.Stats).Score = m.Score();

                            sr.Matches.Add(m);
                        }

                        cr.Seasons.Add(sr);
                    }

                    CompetitionsAsGoalie.Add(cr);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                connection.Close();
            }
        }

        private void CheckNavigateMatch()
        {
            if (SelectedMatch != null)
            {
                NavigateMatchCommand.Execute(SelectedMatch);
            }
        }

        private void LoadPlayerInMatchStats()
        {
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.sport.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("SELECT m.match_id, matches.datetime AS datetime, IFNULL(g_count, 0) AS goal_count, IFNULL(a_count, 0) AS assist_count, IFNULL(p_count, 0) AS penalty_count " +
                                                "FROM player_matches AS m " +

                                                "INNER JOIN matches ON matches.id = m.match_id " +

                                                "LEFT JOIN " +
                                                "(SELECT g.match_id AS g_match_id, COUNT(g.player_id) AS g_count FROM goals AS g " +
                                                "WHERE g.player_id = " + player.id + " " +
                                                "GROUP BY g.match_id) " +
                                                "AS g_table ON g_table.g_match_id = m.match_id " +

                                                "LEFT JOIN " +
                                                "(SELECT a.match_id AS a_match_id, COUNT(a.assist_player_id) AS a_count FROM goals AS a " +
                                                "WHERE a.assist_player_id = " + player.id + " " +
                                                "GROUP BY a.match_id) " +
                                                "AS a_table ON a_table.a_match_id = m.match_id " +

                                                "LEFT JOIN " +
                                                "(SELECT p.match_id AS p_match_id, COALESCE(SUM(p_type.minutes), 0) AS p_count FROM penalties AS p " +
                                                " INNER JOIN penalty_type AS p_type ON p_type.code = p.penalty_type_id " +
                                                "WHERE p.player_id = " + player.id + " " +
                                                "GROUP BY p.match_id) " +
                                                "AS p_table ON p_table.p_match_id = m.match_id " +

                                                "WHERE m.player_id = " + player.id + " " +
                                                "GROUP BY m.match_id", connection);

            try
            {
                connection.Open();
                DataTable dataTable = new DataTable();
                dataTable.Load(cmd.ExecuteReader());

                foreach (DataRow row in dataTable.Rows)
                {
                    PlayerInMatchStats p = new PlayerInMatchStats
                    {
                        Datetime = DateTime.Parse(row["datetime"].ToString()),
                        Goals = int.Parse(row["goal_count"].ToString()),
                        Assists = int.Parse(row["assist_count"].ToString()),
                        PenaltyMinutes = int.Parse(row["penalty_count"].ToString()),
                        Points = int.Parse(row["goal_count"].ToString()) + int.Parse(row["assist_count"].ToString()),
                    };

                    PlayerInMatchStats.Add(p);
                    DatetimeXLabels = PlayerInMatchStats.Select(x => x.Datetime.ToString("d. M. yyyy")).ToArray();
                }

                PlayerInMatchStats.OrderBy(x => x.Datetime);
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                connection.Close();
            }
        }

        private void LoadPlayerInMatchStatsSeries()
        {
            //goals per match
            ChartValues<int> goals = new ChartValues<int>();

            for (int i = 0; i < PlayerInMatchStats.Count; i++)
            {
                goals.Add(PlayerInMatchStats[i].Goals);
            }

            PlayerInMatchStatsSeries.Add(new LineSeries
            {
                Values = goals,
                Fill = Brushes.Transparent,
                Title = "Goals",
                LabelPoint = value => "Goals: " + value.Y + ", Date: " + DatetimeXLabels[value.Key]
            });

            //assists per match
            ChartValues<int> assists = new ChartValues<int>();

            for (int i = 0; i < PlayerInMatchStats.Count; i++)
            {
                assists.Add(PlayerInMatchStats[i].Assists);
            }

            PlayerInMatchStatsSeries.Add(new LineSeries
            {
                Values = assists,
                Fill = Brushes.Transparent,
                Title = "Assists",
                LabelPoint = value => "Assists: " + value.Y + ", Date: " + DatetimeXLabels[value.Key]
            });

            //penalty minutes per match
            ChartValues<int> penalties = new ChartValues<int>();

            for (int i = 0; i < PlayerInMatchStats.Count; i++)
            {
                penalties.Add(PlayerInMatchStats[i].PenaltyMinutes);
            }

            PlayerInMatchStatsSeries.Add(new LineSeries
            {
                Values = penalties,
                Fill = Brushes.Transparent,
                Title = "Penalty minutes",
                LabelPoint = value => "Penalty minutes: " + value.Y + ", Date: " + DatetimeXLabels[value.Key]
            });
        }

        private void LoadPlayerInMatchStatsSumSeries()
        {
            //goals until match
            ChartValues<int> goals = new ChartValues<int>();
            int sum = 0;

            for (int i = 0; i < PlayerInMatchStats.Count; i++)
            {
                sum += PlayerInMatchStats[i].Goals;
                goals.Add(sum);
            }

            PlayerInMatchStatsSumSeries.Add(new LineSeries
            {
                Values = goals,
                Fill = Brushes.Transparent,
                Title = "Goals",
                LabelPoint = value => "Goals: " + value.Y + ", Date: " + DatetimeXLabels[value.Key]
            });

            //assists until match
            ChartValues<int> assists = new ChartValues<int>();
            sum = 0;

            for (int i = 0; i < PlayerInMatchStats.Count; i++)
            {
                sum += PlayerInMatchStats[i].Assists;
                assists.Add(sum);
            }

            PlayerInMatchStatsSumSeries.Add(new LineSeries
            {
                Values = assists,
                Fill = Brushes.Transparent,
                Title = "Assists",
                LabelPoint = value => "Assists: " + value.Y + ", Date: " + DatetimeXLabels[value.Key]
            });

            //penalty minutes until match
            ChartValues<int> penalties = new ChartValues<int>();
            sum = 0;

            for (int i = 0; i < PlayerInMatchStats.Count; i++)
            {
                sum += PlayerInMatchStats[i].PenaltyMinutes;
                penalties.Add(sum);
            }

            PlayerInMatchStatsSumSeries.Add(new LineSeries
            {
                Values = penalties,
                Fill = Brushes.Transparent,
                Title = "Penalty minutes",
                LabelPoint = value => "Penalty minutes: " + value.Y + ", Date: " + DatetimeXLabels[value.Key]
            });
        }

        private void LoadPlayerInMatchStatsAverageSeries()
        {
            //goals average until match
            ChartValues<double> goals = new ChartValues<double>();
            int sum = 0;

            for (int i = 0; i < PlayerInMatchStats.Count; i++)
            {
                sum += PlayerInMatchStats[i].Goals;
                goals.Add(Math.Round(sum / (double)(i + 1), 2));
            }

            PlayerInMatchStatsAverageSeries.Add(new LineSeries
            {
                Values = goals,
                Fill = Brushes.Transparent,
                Title = "Goals",
                LabelPoint = value => "Goals: " + value.Y + ", Date: " + DatetimeXLabels[value.Key]
            });

            //assists average until match
            ChartValues<double> assists = new ChartValues<double>();
            sum = 0;

            for (int i = 0; i < PlayerInMatchStats.Count; i++)
            {
                sum += PlayerInMatchStats[i].Assists;
                assists.Add(Math.Round(sum / (double)(i + 1), 2));
            }

            PlayerInMatchStatsAverageSeries.Add(new LineSeries
            {
                Values = assists,
                Fill = Brushes.Transparent,
                Title = "Assists",
                LabelPoint = value => "Assists: " + value.Y + ", Date: " + DatetimeXLabels[value.Key]
            });

            //penalty minutes average until match
            ChartValues<double> penalties = new ChartValues<double>();
            sum = 0;

            for (int i = 0; i < PlayerInMatchStats.Count; i++)
            {
                sum += PlayerInMatchStats[i].PenaltyMinutes;
                penalties.Add(Math.Round(sum / (double)(i + 1), 2));
            }

            PlayerInMatchStatsAverageSeries.Add(new LineSeries
            {
                Values = penalties,
                Fill = Brushes.Transparent,
                Title = "Penalty minutes",
                LabelPoint = value => "Penalty minutes: " + value.Y + ", Date: " + DatetimeXLabels[value.Key]
            });
        }
    }
}