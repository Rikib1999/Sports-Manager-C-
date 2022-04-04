﻿using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Stores;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace CSharpZapoctak.ViewModels
{
    class GroupsScheduleViewModel : NotifyPropertyChanged
    {
        public class MatchStats : IStats
        {
            public string Overview { get; set; }
            public string HomeScore { get; set; }
            public string AwayScore { get; set; }
            public string Datetime { get; set; }
        }

        private readonly NavigationStore ns;

        private readonly object roundsLock = new object();

        private ObservableCollection<Round> rounds;
        public ObservableCollection<Round> Rounds
        {
            get { return rounds; }
            set
            {
                rounds = value;
                OnPropertyChanged();
            }
        }

        private ICommand deleteRoundCommand;
        public ICommand DeleteRoundCommand
        {
            get
            {
                if (deleteRoundCommand == null)
                {
                    deleteRoundCommand = new RelayCommand(param => DeleteRound((Round)param));
                }
                return deleteRoundCommand;
            }
        }

        private ICommand matchDetailCommand;
        public ICommand MatchDetailCommand
        {
            get
            {
                if (matchDetailCommand == null)
                {
                    matchDetailCommand = new RelayCommand(param => MatchDetail((Match)param));
                }
                return matchDetailCommand;
            }
        }

        private ICommand addMatchCommand;
        public ICommand AddMatchCommand
        {
            get
            {
                if (addMatchCommand == null)
                {
                    addMatchCommand = new RelayCommand(param => AddMatch(((Round)param).id));
                }
                return addMatchCommand;
            }
        }

        private ICommand addRoundCommand;
        public ICommand AddRoundCommand
        {
            get
            {
                if (addRoundCommand == null)
                {
                    addRoundCommand = new RelayCommand(param => AddRound());
                }
                return addRoundCommand;
            }
        }

        public bool IsEnabled { get; private set; } = true;

        public GroupsScheduleViewModel(NavigationStore navigationStore)
        {
            ns = navigationStore;
            if (SportsData.SEASON.PlayOffStarted || SportsData.SEASON.WinnerID != SportsData.NO_ID) { IsEnabled = false; }
            LoadRounds();
        }

        private void LoadRounds()
        {
            Rounds = new ObservableCollection<Round>();

            lock (roundsLock)
            {
                string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.SPORT.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
                MySqlConnection connection = new MySqlConnection(connectionString);
                MySqlCommand cmd = new MySqlCommand("SELECT id, season_id, name FROM rounds WHERE season_id = " + SportsData.SEASON.id, connection);

                try
                {
                    connection.Open();
                    DataTable dataTable = new DataTable();
                    dataTable.Load(cmd.ExecuteReader());

                    foreach (DataRow row in dataTable.Rows)
                    {
                        Round r = new Round
                        {
                            id = int.Parse(row["id"].ToString()),
                            Name = row["name"].ToString(),
                            SeasonID = int.Parse(row["season_id"].ToString()),
                            Matches = new ObservableCollection<Match>()
                        };

                        cmd = new MySqlCommand("SELECT h.name AS home_name, a.name AS away_name, " +
                                                "matches.id, played, datetime, home_score, away_score, overtime, shootout, forfeit " +
                                                "FROM matches", connection);
                        if (SportsData.SPORT.name == "tennis")
                        {
                            cmd.CommandText += " INNER JOIN player AS h ON h.id = matches.home_competitor";
                            cmd.CommandText += " INNER JOIN player AS a ON a.id = matches.away_competitor";
                        }
                        else
                        {
                            cmd.CommandText += " INNER JOIN team AS h ON h.id = matches.home_competitor";
                            cmd.CommandText += " INNER JOIN team AS a ON a.id = matches.away_competitor";
                        }
                        cmd.CommandText += " WHERE round = " + r.id + " AND serie_match_number < 1";

                        DataTable dt = new DataTable();
                        dt.Load(cmd.ExecuteReader());

                        //load matches
                        foreach (DataRow dtRow in dt.Rows)
                        {
                            Team home = new Team();
                            home.Name = dtRow["home_name"].ToString();
                            Team away = new Team();
                            away.Name = dtRow["away_name"].ToString();

                            Match m = new Match
                            {
                                id = int.Parse(dtRow["id"].ToString()),
                                Datetime = DateTime.Parse(dtRow["datetime"].ToString()),
                                Played = Convert.ToBoolean(int.Parse(dtRow["played"].ToString())),
                                HomeTeam = home,
                                AwayTeam = away,
                                HomeScore = int.Parse(dtRow["home_score"].ToString()),
                                AwayScore = int.Parse(dtRow["away_score"].ToString()),
                                Overtime = Convert.ToBoolean(int.Parse(dtRow["overtime"].ToString())),
                                Shootout = Convert.ToBoolean(int.Parse(dtRow["shootout"].ToString())),
                                Forfeit = Convert.ToBoolean(int.Parse(dtRow["forfeit"].ToString()))
                            };

                            MatchStats mStats = new MatchStats
                            {
                                Overview = m.Overview(),
                                Datetime = m.Datetime.ToString("g"),
                                HomeScore = m.HomeTeam.Name + "   " + m.HomeScore,
                                AwayScore = m.AwayScore + m.ResultType() + "   " + m.AwayTeam.Name
                            };
                            if (!m.Played)
                            {
                                mStats.HomeScore = m.HomeTeam.Name + "   -";
                                mStats.AwayScore = "-   " + m.AwayTeam.Name;
                            }
                            m.Stats = mStats;

                            r.Matches.Add(m);
                        }
                        Rounds.Add(r);
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
            }
        }

        private void DeleteRound(Round r)
        {
            MessageBoxResult msgResult = MessageBox.Show("Do you really want to delete " + r.Name + "?.", "Delete round", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (msgResult == MessageBoxResult.Yes)
            {
                //delete round from DB
                string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.SPORT.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
                MySqlConnection connection = new MySqlConnection(connectionString);
                MySqlTransaction transaction = null;
                MySqlCommand cmd = null;
                string roundDeletionQuerry = "DELETE FROM rounds WHERE id = " + r.id;

                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();

                    cmd = new MySqlCommand(roundDeletionQuerry, connection);
                    cmd.Transaction = transaction;
                    cmd.ExecuteNonQuery();

                    //get all match ids of round r
                    string querry = "SELECT id FROM matches WHERE serie_match_number < 1 AND round = " + r.id;
                    cmd = new MySqlCommand(querry, connection);
                    cmd.Transaction = transaction;
                    DataTable dataTable = new DataTable();
                    dataTable.Load(cmd.ExecuteReader());

                    StringBuilder sb = new StringBuilder();
                    foreach (DataRow row in dataTable.Rows)
                    {
                        sb.Append("'" + row["id"].ToString() + "',");
                    }
                    if (sb.Length > 0) { sb.Remove(sb.Length - 1, 1); }

                    if (sb.Length > 0)
                    {
                        //delete matches from DB
                        querry = "DELETE FROM matches WHERE bracket_index < 1 AND round = " + r.id;
                        cmd = new MySqlCommand(querry, connection);
                        cmd.Transaction = transaction;
                        cmd.ExecuteNonQuery();

                        //delete all player/goalie match enlistments and all stats from all matches
                        List<string> databases = new List<string> { "player_matches", "goalie_matches", "penalties", "goals", "penalty_shots", "shutouts", "shifts", "shootout_shots", "time_outs", "period_score", "game_state" };
                        foreach (string db in databases)
                        {
                            querry = "DELETE FROM " + db + " WHERE match_id IN (" + sb + ")";
                            cmd = new MySqlCommand(querry, connection);
                            cmd.Transaction = transaction;
                            cmd.ExecuteNonQuery();
                        }
                    }

                    //rename all rounds after round r
                    int roundNumber = int.Parse(r.Name.Split(' ')[1]);
                    for (int i = Rounds.IndexOf(r) + 1; i < Rounds.Count; i++)
                    {
                        querry = "UPDATE rounds SET name = 'Round " + roundNumber  + "' WHERE id = " + Rounds[i].id;
                        cmd = new MySqlCommand(querry, connection);
                        cmd.Transaction = transaction;
                        cmd.ExecuteNonQuery();

                        Rounds[i].Name = "Round " + roundNumber;
                        roundNumber++;
                    }

                    Rounds.Remove(r);

                    transaction.Commit();
                    connection.Close();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    MessageBox.Show("Unable to connect to databse.", "Database error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
            }
        }

        private void MatchDetail(Match m)
        {
            new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, new MatchViewModel(ns, m, new GroupsScheduleViewModel(ns)))).Execute(null);
        }

        private void AddMatch(int id)
        {
            new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, new AddMatchViewModel(ns, id))).Execute(null);
        }

        private void AddRound()
        {
            Round r = new Round();
            r.SeasonID = SportsData.SEASON.id;
            r.Name = "Round " + (Rounds.Count + 1);
            r.Matches = new ObservableCollection<Match>();

            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.SPORT.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("INSERT INTO rounds(season_id, name) VALUES ('" + r.SeasonID + "', '" + r.Name + "')", connection);

            try
            {
                connection.Open();
                cmd.ExecuteNonQuery();
                r.id = (int)cmd.LastInsertedId;

                Rounds.Add(r);
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
}