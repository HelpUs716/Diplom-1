using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Учет.Data.Interfaces;
using Учет.Enums;
using Учет.Models;

namespace Учет.Views
{
    public partial class AuthWindow : Window
    {
        public AuthWindow()
        {
            InitializeComponent();
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LoginBox.Text))
            {
                ErrorText.Text = "Введите логин!";
                ErrorText.Visibility = Visibility.Visible;
                return;
            }

            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                ErrorText.Text = "Введите пароль!";
                ErrorText.Visibility = Visibility.Visible;
                return;
            }

            try
            {
                using var scope = App.ServiceProvider.CreateScope();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var users = await uow.Users.GetAllAsync();
                var user = users.FirstOrDefault(u => u.Login == LoginBox.Text);

                if (user != null && user.PasswordHash == PasswordBox.Password)
                {
                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                    Close();
                }
                else
                {
                    ErrorText.Text = "Неверный логин или пароль!";
                    ErrorText.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                ErrorText.Text = $"Ошибка: {ex.Message}";
                ErrorText.Visibility = Visibility.Visible;
            }
        }
    }
}