﻿using CSharpZapoctak.Commands;
using CSharpZapoctak.Models;
using CSharpZapoctak.Stores;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace CSharpZapoctak.ViewModels
{
    class EditTeamViewModel : NotifyPropertyChanged
    {
        private Team currentTeam;
        public Team CurrentTeam
        {
            get { return currentTeam; }
            set
            {
                currentTeam = value;
                OnPropertyChanged();
            }
        }

        private BitmapImage bitmap;
        public BitmapImage Bitmap
        {
            get { return bitmap; }
            set
            {
                bitmap = value;
                OnPropertyChanged();
            }
        }

        private ICommand loadImageCommand;
        public ICommand LoadImageCommand
        {
            get
            {
                if (loadImageCommand == null)
                {
                    loadImageCommand = new RelayCommand(param => LoadImage());
                }
                return loadImageCommand;
            }
        }

        private ICommand removeImageCommand;
        public ICommand RemoveImageCommand
        {
            get
            {
                if (removeImageCommand == null)
                {
                    removeImageCommand = new RelayCommand(param => RemoveImage());
                }
                return removeImageCommand;
            }
        }

        private ObservableCollection<Country> countries;
        public ObservableCollection<Country> Countries
        {
            get { return countries; }
            set
            {
                countries = value;
                OnPropertyChanged();
            }
        }

        private ICommand saveCommand;
        public ICommand SaveCommand
        {
            get
            {
                if (saveCommand == null)
                {
                    saveCommand = new RelayCommand(param => Save());
                }
                return saveCommand;
            }
        }

        public NavigationStore ns;

        public EditTeamViewModel(NavigationStore navigationStore, Team t)
        {
            CurrentTeam = t;
            Countries = SportsData.countries;
            ns = navigationStore;

            if (!string.IsNullOrWhiteSpace(CurrentTeam.LogoPath))
            {
                MemoryStream ms = new MemoryStream();
                byte[] arrbytFileContent = File.ReadAllBytes(CurrentTeam.LogoPath);
                ms.Write(arrbytFileContent, 0, arrbytFileContent.Length);
                ms.Position = 0;

                bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = ms;
                bitmap.EndInit();
                Bitmap = bitmap;
            }
            else
            {
                CurrentTeam.LogoPath = "";
            }
        }

        private void LoadImage()
        {
            OpenFileDialog open = new OpenFileDialog();
            open.DefaultExt = ".png";
            open.Filter = "Pictures (*.jpg;*.png)|*.jpg;*.png";

            if (open.ShowDialog() == true)
            {
                CurrentTeam.LogoPath = open.FileName;

                MemoryStream ms = new MemoryStream();
                byte[] arrbytFileContent = File.ReadAllBytes(CurrentTeam.LogoPath);
                ms.Write(arrbytFileContent, 0, arrbytFileContent.Length);
                ms.Position = 0;

                bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = ms;
                bitmap.EndInit();
                Bitmap = bitmap;
                GC.Collect();
            }
        }

        private void RemoveImage()
        {
            CurrentTeam.LogoPath = "";
            Bitmap = new BitmapImage();
            GC.Collect();
        }

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(CurrentTeam.Name))
            {
                return;
            }
            //UPDATE
            string connectionString = "SERVER=" + SportsData.server + ";DATABASE=" + SportsData.SPORT.name + ";UID=" + SportsData.UID + ";PASSWORD=" + SportsData.password + ";";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand("UPDATE team SET name = '" + CurrentTeam.Name + "', info = '" + CurrentTeam.Info + "', status = '" + Convert.ToInt32(CurrentTeam.Status) + "', " +
                "country = '" + CurrentTeam.Country.CodeTwo + "', date_of_creation = '" + CurrentTeam.DateOfCreation.ToString("yyyy-MM-dd H:mm:ss") + "' " +
                "WHERE id = " + CurrentTeam.id, connection);

            try
            {   //execute querry
                connection.Open();
                cmd.ExecuteNonQuery();

                //SAVE LOGO
                //if logo is not selected return
                if (string.IsNullOrWhiteSpace(CurrentTeam.LogoPath))
                {
                    //if there is logo in the database then delete it
                    //get previous logo
                    string[] previousImgPath = Directory.GetFiles(SportsData.TeamLogosPath, SportsData.SPORT.name + CurrentTeam.id + ".*");
                    string previousFilePath = "";
                    //if it exists
                    if (previousImgPath.Length != 0)
                    {
                        previousFilePath = previousImgPath.First();
                    }
                    //delete logo
                    if (!string.IsNullOrWhiteSpace(previousFilePath))
                    {
                        GC.Collect();
                        File.Delete(previousFilePath);
                    }
                }
                else
                {
                    //get current logo
                    string[] imgPath = Directory.GetFiles(SportsData.TeamLogosPath, SportsData.SPORT.name + currentTeam.id + ".*");
                    string filePath = "";
                    //if logo existed
                    if (imgPath.Length != 0)
                    {
                        filePath = imgPath.First();
                    }
                    //if logo did not exist declare its path
                    if (string.IsNullOrWhiteSpace(filePath))
                    {
                        filePath = SportsData.TeamLogosPath + @"\" + SportsData.SPORT.name + currentTeam.id + Path.GetExtension(CurrentTeam.LogoPath);
                    }
                    //if logo had changeg
                    if (CurrentTeam.LogoPath != filePath)
                    {
                        GC.Collect();
                        File.Delete(filePath);
                        filePath = Path.ChangeExtension(filePath, Path.GetExtension(CurrentTeam.LogoPath));
                        File.Copy(CurrentTeam.LogoPath, filePath);
                        CurrentTeam.LogoPath = filePath;
                    }
                }

                new NavigateCommand<SportViewModel>(ns, () => new SportViewModel(ns, new TeamViewModel(ns, CurrentTeam))).Execute(null);
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