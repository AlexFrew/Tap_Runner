using NATS.Client;
using OpenTap.Runner.Client;
using System.ComponentModel;
using System.Text;

namespace TapRunnerDll
{
    public class TAPRunnerControl
    {
        /// <summary>
        /// Event triggered to create a user interaction dialog.
        /// </summary>
        public event CreateDialogHandler? CreateUserInteractionDialog;

        /// <summary>
        /// Event triggered when the test plan is completed.
        /// </summary>
        public event EventHandler TestPlanCompleted;

        /// <summary>
        /// Event triggered when logs are received.
        /// </summary>
        public event EventHandler<LogList> LogReceived;

        /// <summary>
        /// Event triggered when results are received.
        /// </summary>
        public event EventHandler<Result> ResultReceived;

        /// <summary>
        /// Event triggered when the run status changes.
        /// </summary>
        public event EventHandler<TestRun> RunStatusChanged;

        /// <summary>
        /// Initializes a new instance of the TAPRunnerControl class.
        /// </summary>
        public TAPRunnerControl(string nATSAddress = "nats://127.0.0.1:20111")
        {
            LatestTestPlanVerdict = OpenTap.Runner.Client.Verdict.NotSet;
            NATSAddress = nATSAddress;
        }

        private delegate void TestPlanExecutionEventHandler(ExecutionState executionState);

        private event TestPlanExecutionEventHandler? RaiseTestPlanExecutionEvent;

        private Interaction? _interaction;

        public delegate void CreateDialogHandler(string title, string message, bool modal, string button1Text, string button2Text, string base64Image, Action<string> buttonClickedCallback);

        /// <summary>
        /// Gets the latest test plan verdict.
        /// </summary>
        public OpenTap.Runner.Client.Verdict LatestTestPlanVerdict { get; private set; }

        /// <summary>
        /// Gets the current run status.
        /// </summary>
        public RunStatus Status { get; private set; }

        private SessionClient CurrentSession { get; set; }

        private RunnerClient Runner { get; set; }

        /// <summary>
        /// Gets or sets the NATS server address.
        /// </summary>
        public string NATSAddress { get; set; }

        /// <summary>
        /// Connects to the runner.
        /// </summary>
        public void ConnectToRunner()
        {
            try
            {
                var options = ConnectionFactory.GetDefaultOptions();
                options.Servers = new string[] { NATSAddress };
                IConnection connection = new ConnectionFactory().CreateConnection(options);
                Runner = new RunnerClient(connection.ServerInfo.ServerName, connection);

                // Start a new session based on Runner installation
                CurrentSession = Runner.StartSession();

                // Connect to the session logs
                CurrentSession.ConnectSessionLogs(LogHandler);

                // Connect to the session results
                CurrentSession.ConnectSessionResults(ResultHandler, RunHandler);

                // Connect to the session events
                CurrentSession.ConnectSessionEvents(EventHandler);
            }
            catch (Exception ex)
            {
                throw new ConnectionException("Failed to connect to the runner: " + ex.Message);
            }
        }

        /// <summary>
        /// Disconnects from the runner.
        /// </summary>
        public void DisconnectFromRunner()
        {
            CurrentSession.DisconnectSessionEvents();
            CurrentSession.DisconnectSessionLogs();
            CurrentSession.DisconnectSessionResults();

            Runner.ShutdownSession(CurrentSession.Id);
        }

        /// <summary>
        /// Loads the test plan from the specified path.
        /// </summary>
        /// <param name="testPlanPath">The path to the test plan file.</param>
        public void LoadTestPlan(string testPlanPath)
        {
            try
            {
                CurrentSession.SetTestPlanXML(File.ReadAllText(@testPlanPath));
            }
            catch (Exception ex)
            {
                throw new TestPlanException("Failed to load the test plan: " + ex.Message);
            }
        }

        /// <summary>
        /// Loads the specified parameters into the test plan.
        /// </summary>
        /// <param name="inputParameters">The list of parameters to load.</param>
        public void LoadParameters(List<SettingsParameters> inputParameters)
        {
            try
            {
                var plan = CurrentSession.GetTestPlan(null);

                foreach (var parameter in inputParameters)
                {
                    var step = plan.ChildTestSteps.FirstOrDefault(s => s.Name == parameter.StepName);
                    if (step != null)
                    {
                        var stepSettings = CurrentSession.GetSettings(step.Id);
                        var setting = stepSettings.FirstOrDefault(s => s.PropertyName == parameter.VariableName);
                        if (setting != null)
                        {
                            switch (parameter.Value)
                            {
                                case string strValue:
                                    if (setting is TextBoxControl textBoxControl)
                                    {
                                        textBoxControl.StringValue = strValue;
                                    }
                                    break;

                                case int intValue:
                                    if (setting is TextBoxControl textBoxControlInt)
                                    {
                                        textBoxControlInt.StringValue = intValue.ToString();
                                    }
                                    break;

                                case double doubleValue:
                                    if (setting is TextBoxControl textBoxControlDouble)
                                    {
                                        textBoxControlDouble.StringValue = doubleValue.ToString();
                                    }
                                    break;

                                case bool boolValue:
                                    if (setting is CheckBoxControl checkBoxControl)
                                    {
                                        checkBoxControl.BoolValue = boolValue;
                                    }
                                    break;

                                case Enum enumValue:
                                    if (setting is DropdownControl dropdownControl)
                                    {
                                        var enumIndex = Array.IndexOf(Enum.GetValues(enumValue.GetType()), enumValue);
                                        dropdownControl.SelectedIndex = enumIndex;
                                    }
                                    break;
                                    // Add more cases as needed for other control types
                            }
                        }
                        CurrentSession.SetSettings(step.Id, stepSettings);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new TestPlanException("Failed to load parameters: " + ex.Message);
            }
        }

        /// <summary>
        /// Starts the test plan asynchronously.
        /// </summary>
        public async Task StartTestPlanAsync()
        {
            try
            {
                Status = CurrentSession.RunTestPlan(new List<Parameter>());
                while (Status.ExecutionState != ExecutionState.Idle)
                {
                    await Task.Delay(1000);
                    Status = CurrentSession.GetStatus();
                }
                LatestTestPlanVerdict = Status.Verdict;
                TestPlanCompleted?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                throw new TestPlanException("Failed to start the test plan: " + ex.Message);
            }
            finally
            {
                DisconnectFromRunner();
            }
        }

        /// <summary>
        /// Stops the test plan.
        /// </summary>
        public void StopTestPlan()
        {
            try
            {
                CurrentSession.AbortTestPlan();
            }
            catch (Exception ex)
            {
                throw new TestPlanException("Failed to stop the test plan: " + ex.Message);
            }
        }

        /// <summary>
        /// Pauses the test plan.
        /// </summary>
        public void PauseTestPlan()
        {
            try
            {
                CurrentSession.SetPauseNext();
            }
            catch (Exception ex)
            {
                throw new TestPlanException("Failed to pause the test plan: " + ex.Message);
            }
        }

        #region Internal Methods

        private void CreateUserInput(Guid requestId)
        {
            try
            {
                var interaction = CurrentSession.GetUserInput(requestId);

                string title = interaction.Title;
                string message = "";
                string base64Image = "";
                string button1Text = "";
                string button2Text = "";
                bool modal = interaction.Modal;

                foreach (var set in interaction.Settings)
                {
                    if (set is PictureControl p && p.VisualStatus.IsVisible && p.Resource != null)
                    {
                        try
                        {
                            var bytes = CurrentSession.DownloadResource(p.Resource);
                            base64Image = Convert.ToBase64String(bytes);
                        }
                        catch { }
                    }
                    if (set is TextBoxControl t && t.PropertyName == "Message")
                    {
                        message = t.StringValue;
                    }
                    else if (set is ButtonsControl b && b.VisualStatus.IsEnabled)
                    {
                        button1Text = b.AvailableValues[0].Name;
                        button2Text = b.AvailableValues[1].Name;
                    }
                }
                _interaction = interaction;
                this.CreateUserInteractionDialog(title, message, true, button1Text, button2Text, base64Image, new Action<string>(s => buttonCallback(s)));
            }
            catch (Exception ex)
            {
                throw new UserInputException("Failed to create user input: " + ex.Message);
            }
        }

        public void buttonCallback(string buttonText)
        {
            foreach (var set in _interaction.Settings)
            {
                if (set is ButtonsControl b && b.VisualStatus.IsEnabled)
                {
                    if (b.AvailableValues[0].Name == buttonText)
                    {
                        b.SelectedIndex = 0;
                    }
                    else
                    {
                        b.SelectedIndex = 1;
                    }
                    b.InvokeMethod = true;
                }
            }
            CurrentSession.SetUserInput(_interaction);
        }

        private void LogHandler(LogList list)
        {
            LogReceived?.Invoke(this, list);
        }

        private void ResultHandler(Result result)
        {
            ResultReceived?.Invoke(this, result);
        }

        private void RunHandler(TestRun run)
        {
            RunStatusChanged?.Invoke(this, run);
        }

        private void EventHandler(SessionEvent evt)
        {
            if (evt is UserInputRequestEventArgs a)
            {
                CreateUserInput(a.RequestId);
            }
            if (evt is TestPlanExecutionStateChangedEventArgs b && RaiseTestPlanExecutionEvent != null)
            {
                RaiseTestPlanExecutionEvent(b.RunStatus.ExecutionState);
            }
        }

        #endregion Internal Methods
    }

    #region CustomExceptions

    public class TAPRunnerException : Exception
    {
        public TAPRunnerException(string message) : base(message)
        {
        }
    }

    public class ConnectionException : TAPRunnerException
    {
        public ConnectionException(string message) : base(message)
        {
        }
    }

    public class TestPlanException : TAPRunnerException
    {
        public TestPlanException(string message) : base(message)
        {
        }
    }

    public class UserInputException : TAPRunnerException
    {
        public UserInputException(string message) : base(message)
        {
        }
    }

    #endregion CustomExceptions
}