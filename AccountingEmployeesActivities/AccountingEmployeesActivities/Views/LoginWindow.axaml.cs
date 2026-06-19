
﻿using AccountingEmployeesActivities.ViewModels;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Text;

namespace AccountingEmployeesActivities.Views
{
    public partial class LoginWindow : Window
    {
        private LoginViewModel _viewModel;

        public LoginWindow()
        {
            InitializeComponent();
            DataContext = new LoginViewModel();
            _viewModel = new LoginViewModel();
            _viewModel.LoginSuccess += OnLoginSuccess;
            DataContext = _viewModel;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnLoginSuccess(object sender, Models.User user)
        {
            var mainWindow = new MainWindow(user);
            mainWindow.Show();
            Close();
        }
    }
}