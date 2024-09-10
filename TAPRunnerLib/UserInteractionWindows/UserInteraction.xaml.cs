using Keysight.Ccl.Wsl.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TAPRunnerLib.UserInteractionWindows
{
    /// <summary>
    /// Interaction logic for UserInteraction.xaml
    /// </summary>
    public partial class UserInteraction : WslDialog
    {
        private readonly Action<string> _buttonClickedCallback;

        public UserInteraction(string title, string message, string button1Text, string button2Text, string base64Image, Action<string> buttonClickedCallback)
        {
            InitializeComponent();
            TitleTextBlock.Text = title;
            MessageTextBlock.Text = message;
            Button1.Content = button1Text;
            Button2.Content = button2Text;
            _buttonClickedCallback = buttonClickedCallback;

            if (!string.IsNullOrEmpty(base64Image))
            {
                var imageBytes = Convert.FromBase64String(base64Image);
                var bitmap = new BitmapImage();
                using (var stream = new System.IO.MemoryStream(imageBytes))
                {
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                }
                DialogImage.Source = bitmap;
                DialogImage.Visibility = Visibility.Visible;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                _buttonClickedCallback?.Invoke(button.Content.ToString());
                Close();
            }
        }
    }
}