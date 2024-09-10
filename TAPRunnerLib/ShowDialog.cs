using Keysight.Ccl.Wsl.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace TAPRunnerLib
{
    public static class ShowDialog
    {
        public static void CustomShowDialog(string title, object content, string closeDialogButtonText)
        {
            try
            {
                WslDialog window = new WslDialog();
                Grid grid = new Grid()
                {
                    RowDefinitions =
                                {
                                        new RowDefinition() { Height = new GridLength(100,GridUnitType.Star) },
                                        new RowDefinition() { Height = GridLength.Auto },
                                }
                };

                ContentPresenter contentPresenter = new ContentPresenter() { Content = content };
                grid.Children.Add(contentPresenter);
                Grid.SetRow(contentPresenter, 0);

                Button closeDialogButton = new Button();
                closeDialogButton.Margin = new Thickness(10);
                closeDialogButton.Content = closeDialogButtonText;
                closeDialogButton.IsCancel = true;
                grid.Children.Add(closeDialogButton);
                Grid.SetRow(closeDialogButton, 1);

                window.Content = grid;
                window.Title = title;
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                window.Owner = App.Current.MainWindow;
                window.ShowDialog();
                // window.Show();
            }
            catch (System.Exception ex)
            {
                throw;
            }
        }
    }
}