using Keysight.Ccl.Wsl.UI;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TAPRunnerLib
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : WslMainWindow
    {
        public MainWindowViewModel mainWindowVM { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            UXManager.Initialize(StandardSkinNames.Caranu);
            UXManager.ColorScheme = "Caranu Dark";
            UXManager.ToggleAmbientSkin("KioskCentered Mode", true);

            mainWindowVM = new MainWindowViewModel();
            this.DataContext = mainWindowVM;
        }
    }
}