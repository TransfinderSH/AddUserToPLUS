﻿using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AddUserToPLUS.ViewModels
{
    class MainWindowViewModel : ViewModelBase
    {
        private readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private bool isAddMultipleUsers = false;
        public ICommand AddUserCommand { get; private set; }
        public ICommand AddMultipleUsersCommand { get; set; }
        public MainWindowViewModel(
            MainWindow shell,
            IUnityContainer container, IEventAggregator eventAggregator) :
            base(shell, container, eventAggregator)
        {
            ConnectionStr = ConfigurationManager.AppSettings["ConnectionStr"];
            AddUserCommand = new DelegateCommand<object>(OnAddUserCommand);
            AddMultipleUsersCommand = new DelegateCommand<object>(OnAddMultipleUsersCommand);
        }

        private void OnAddMultipleUsersCommand(object obj)
        {
            isAddMultipleUsers = true;
            string tempUserName = UserName;
            string tempPassword = Password;
            if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConnectionStr)
                || UserName.Trim().IndexOf(' ') != -1)
            {
                MessageBox.Show("Please fill in the textboxes first before clicking the Add button. Whitespaces are not allowed for User Name.");
                return;
            }

            if (!UsersCount.HasValue && UsersCount.Value == 0) return;

            for (int i = 0; i < UsersCount.Value; i++)
            {
                UserName = tempUserName + (i + 1);
                Password = tempPassword + (i + 1);

                OnAddUserCommand(null);
            }
            isAddMultipleUsers = false;
            MessageBox.Show("Done");
        }

        protected PlusDataContext GetDataContext()
        {
            var context = new PlusDataContext();
            context.Database.Connection.ConnectionString = GetConnectionString();
            context.Configuration.LazyLoadingEnabled = false;
            return context;
        }

        private string GetConnectionString()
        {
            if (ConnectionStr[ConnectionStr.Length - 1] != ';') ConnectionStr += ";";
            string connectionString = ConnectionStr + "UID=tfuser;PWD=$transfinder2006;";
            return connectionString;
        }

        private void OnAddUserCommand(object obj)
        {
            if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConnectionStr)
                || UserName.Trim().IndexOf(' ') != -1)
            {
                MessageBox.Show("Please fill in the textboxes first before clicking the Add button. Whitespaces are not allowed for User Name.");
                return;
            }

            try
            {
                using (PlusDataContext context = GetDataContext())
                {
                    User userInDB = context.Users.FirstOrDefault(p => p.LoginID == UserName.Trim());
                    if (userInDB != null)
                    {
                        if (isAddMultipleUsers) return;
                        MessageBox.Show("User " + UserName + " already exist!");
                        return;
                    }
                    User newUser = new User();
                    newUser.LoginID = UserName.Trim();
                    using (SHA256 sha = SHA256.Create())
                    {
                        newUser.Password = sha.ComputeHash(Encoding.GetEncoding(1252).GetBytes(Password));
                    }
                    newUser.Administrator = false;
                    newUser.FirstName = UserName;
                    newUser.LastName = string.Empty;
                    newUser.CreateDateTime = DateTime.Now;
                    newUser.Deactivated = false;
                    newUser.Email = "csheil@transfinder.com";

                    context.Users.Add(newUser);

                    context.SaveChanges();
                }
                if (!isAddMultipleUsers) MessageBox.Show("Success!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Add user failed:" + ex.Message);
                logger.Debug("Add user failed:" + ex.Message);
            }
        }

        public static byte[] HexString2ByteArr(string hexString)
        {
            int NumberChars = hexString.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            return bytes;
        }

        private string connectionStr;

        public string ConnectionStr
        {
            get { return connectionStr; }
            set
            {
                connectionStr = value;
                OnPropertyChanged();
            }
        }

        private string userName;

        public string UserName
        {
            get { return userName; }
            set
            {
                userName = value;
                OnPropertyChanged();
            }
        }

        private string password;

        public string Password
        {
            get { return password; }
            set
            {
                password = value;
                OnPropertyChanged();
            }
        }

        private int? usersCount;

        public int? UsersCount
        {
            get { return usersCount; }
            set
            {
                usersCount = value;
                OnPropertyChanged();
            }
        }

    }
}
