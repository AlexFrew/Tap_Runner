using Keysight.Aes.Asf.MVVM;
using OpenTap;
using OpenTap.Runner.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TapRunnerDll;
using TAPRunnerLib.UserInteractionWindows;

namespace TAPRunnerLib
{
    public class MainWindowViewModel : AutoPropertyChanged
    {
        public MainWindowViewModel()
        {
            StartCMD = new CommandHandler(OnStartCommand);
            TAPRunner = new TAPRunnerControl();

            TAPRunner.LogReceived += OnLogReceived;
            TAPRunner.CreateUserInteractionDialog += OnCreateDialog;
            TAPRunner.ResultReceived += OnResultReceived;

            TAPRunner.RunStatusChanged += (sender, e) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Status = e.Status.ToString();
                    Verdict = e.Verdict.ToString();
                });
            };

            EnumValues = Enum.GetValues(typeof(TAPRunnerInteractionPlugin.TestEnum)).Cast<TAPRunnerInteractionPlugin.TestEnum>().ToList();

            A = "50";
            B = "49";
            DelaySecs = "3 s";
            EnumExample = TAPRunnerInteractionPlugin.TestEnum.Choice2;
            StringExample = "Example of string";
            UseTimeout = true;
            TestPlanPath = @"C:\Users\alexfrew\source\repos\TAPRunnerControl\TAPRunnerInteractionPlugin\bin\Debug\FirstTestPlan.TapPlan";
        }

        private void OnResultReceived(object? sender, Result e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                StringResult.Add(e.ToJson());
            });
        }

        private void OnCreateDialog(string title, string message, bool modal, string button1Text, string button2Text, string base64Image, Action<string> buttonClickedCallback)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var dialog = new UserInteraction(title, message, button1Text, button2Text, base64Image, buttonClickedCallback)
                {
                    WindowStartupLocation = WindowStartupLocation.CenterScreen // Position the window in the middle of the screen
                };
                dialog.ShowDialog(); // Show the dialog to the user
            });
        }

        public TAPRunnerControl TAPRunner { get; set; }

        private void OnStartCommand()
        {
            try
            {
                StringLog.Clear();
                StringResult.Clear();

                TAPRunner.ConnectToRunner();
                TAPRunner.LoadTestPlan(TestPlanPath);
                TAPRunner.LoadParameters(new List<SettingsParameters>()
                {
                    new SettingsParameters(stepName:"Delay",               variableName:"DelaySecs",     value:DelaySecs),
                    new SettingsParameters(stepName:"UserInteractionStep", variableName:"A",             value:A),
                    new SettingsParameters(stepName:"UserInteractionStep", variableName:"B",             value:B),
                    new SettingsParameters(stepName:"UserInteractionStep", variableName:"EnumExample",   value:EnumExample),
                    new SettingsParameters(stepName:"UserInteractionStep", variableName:"StringExample", value:StringExample),
                    new SettingsParameters(stepName:"UserInteractionStep", variableName:"Timeout",       value:180),
                    new SettingsParameters(stepName:"UserInteractionStep", variableName:"UseTimeout",    value:UseTimeout),
                });

                Task Run = TAPRunner.StartTestPlanAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region Properties

        public List<TAPRunnerInteractionPlugin.TestEnum> EnumValues { get; }

        private void OnLogReceived(object sender, LogList logList)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var log in logList.Logs)
                {
                    StringLog.Add($"{log.Source,-12}: {log.Message}");
                }
            });
        }

        private ObservableCollection<string> _stringLog = new ObservableCollection<string>();

        public ObservableCollection<string> StringLog
        {
            get { return _stringLog; }
            set { SetField(ref _stringLog, value); }
        }

        private ObservableCollection<string> _stringResult = new ObservableCollection<string>();

        public ObservableCollection<string> StringResult
        {
            get { return _stringResult; }
            set { SetField(ref _stringResult, value); }
        }

        public ICommand StartCMD { get; }

        public string A
        {
            get => _a;
            set
            {
                if (_a != value)
                {
                    _a = value;
                    OnPropertyChanged("A");
                }
            }
        }

        private string _a;

        public string B
        {
            get => _b;
            set
            {
                if (_b != value)
                {
                    _b = value;
                    OnPropertyChanged("B");
                }
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged("Status");
                }
            }
        }

        private string _status;

        public string Verdict
        {
            get => _verdict;
            set
            {
                if (_verdict != value)
                {
                    _verdict = value;
                    OnPropertyChanged("Verdict");
                }
            }
        }

        private string _verdict;
        private string _b;

        public string DelaySecs
        {
            get => _delaySecs;
            set
            {
                if (_delaySecs != value)
                {
                    _delaySecs = value;
                    OnPropertyChanged("DelaySecs");
                }
            }
        }

        public string TestPlanPath
        {
            get => _testPlanPath;
            set
            {
                if (_testPlanPath != value)
                {
                    _testPlanPath = value;
                    OnPropertyChanged("TestPlanPath");
                }
            }
        }

        private string _delaySecs;

        public TAPRunnerInteractionPlugin.TestEnum EnumExample
        {
            get => _enumExample;
            set
            {
                if (_enumExample != value)
                {
                    _enumExample = value;
                    OnPropertyChanged("EnumExample");
                }
            }
        }

        private TAPRunnerInteractionPlugin.TestEnum _enumExample;

        public string StringExample
        {
            get => _stringExample;
            set
            {
                if (_stringExample != value)
                {
                    _stringExample = value;
                    OnPropertyChanged("StringExample");
                }
            }
        }

        private string _stringExample;

        public bool UseTimeout
        {
            get => _useTimeout;
            set
            {
                if (_useTimeout != value)
                {
                    _useTimeout = value;
                    OnPropertyChanged("UseTimeout");
                }
            }
        }

        private bool _useTimeout;
        private string _testPlanPath;

        #endregion Properties
    }
}
