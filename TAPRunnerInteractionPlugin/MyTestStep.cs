using OpenTap;
using System.ComponentModel;
using System;
using System.Security.Policy;
using System.Collections.Generic;
using TAPRunnerInteractionPlugin.Misc;
using System.Xml.Linq;

namespace TAPRunnerInteractionPlugin
{
    public enum TestEnum
    {
        Choice1,
        Choice2,
    }

    [Display("UserInteractionStep", Description: "Insert a description here", Group: "TAPRunnerInteractionPlugin")]
    public class MyTestStep : TestStep
    {
        #region Settings

        [Display("Use Timeout", "Enabling this will make the dialog close after an amount of time.", Group: "Timeout", Order: 1)]
        public bool UseTimeout { get; set; }

        [EnabledIf("UseTimeout", true)]
        [Unit("s")]
        [Display("Timeout", "The dialog closes after this time.", Group: "Timeout", Order: 2)]
        public double Timeout { get; set; }

        public TestEnum EnumExample { get; set; }
        public string StringExample { get; set; }

        public int A { get; set; }
        public int B { get; set; }

        // ToDo: Add property here for each parameter the end user should be able to change

        #endregion Settings

        public MyTestStep()
        {
            UseTimeout = true;
            Timeout = 60;
            Rules.Add(() => Timeout > 0, "Timeout must > 0s.", "Timeout");
            // ToDo: Set default values for properties / settings.
        }

        public override void PrePlanRun()
        {
            base.PrePlanRun();
            // ToDo: Optionally add any setup code this step needs to run before the testplan starts
        }

        public override void Run()
        {
            int result = A + B;
            Log.Info("Result A+B: " + result);
            Log.Info("Enum: " + EnumExample.ToString());
            Log.Info("Bool :" + UseTimeout.ToString());
            Log.Info("String : " + StringExample);
            Log.Info("double :  " + Timeout.ToString());

            try
            {
                var timeout = UseTimeout ? TimeSpan.FromSeconds(Timeout) : TimeSpan.MaxValue;

                ShowDialog("Calibration", "Short", timeout);
                ShowDialog("Calibration", "Open", timeout);
                ShowDialog("Calibration", "Load", timeout);
                ShowDialog("Calibration", "Through", timeout);
            }
            catch (TimeoutException)
            {
                Log.Info("User did not click. Are they sleeping?");
            }

            List<double> X = GenerateRandomDoubleList(20, 0.0, 10.0);
            List<double> Y = GenerateRandomDoubleList(20, 0.0, 10.0);

            ResultColumn xColumn = new ResultColumn("XColumn", X.ToArray());
            ResultColumn yColumn = new ResultColumn("YColumn", Y.ToArray());

            ResultTable resultTable = new ResultTable("ExampleResultTable", new ResultColumn[] { xColumn, yColumn });

            Results.Publish(resultTable);

            Log.Info("Publish done!");

            if (result > 100)
                UpgradeVerdict(Verdict.Fail);
            else
                UpgradeVerdict(Verdict.Pass);
        }

        public override void PostPlanRun()
        {
            // ToDo: Optionally add any cleanup code this step needs to run after the entire testplan has finished
            base.PostPlanRun();
        }

        private static List<double> GenerateRandomDoubleList(int count, double minValue, double maxValue)
        {
            List<double> randomDoubles = new List<double>();
            for (int i = 0; i < count; i++)
            {
                randomDoubles.Add(RandomExtensions.NextDouble(minValue, maxValue));
            }
            return randomDoubles;
        }

        private void ShowDialog(string title, string description, TimeSpan timeout)
        {
            var dialog = new SimpleDialog
            {
                Message = description,
                Name = title
            };
            UserInput.Request(dialog, timeout);

            // Response from the user.
            if (dialog.Response == WaitForInputResult.Cancel)
            {
                Log.Info("User clicked Cancel.");
                return;
            }
            if (dialog.Response == WaitForInputResult.Ok)
            {
                Log.Info("User clicked OK.");
            }
        }
    }

    public enum WaitForInputResult
    {
        // The number assigned, determines the order in which the buttons are shown in the dialog.
        Cancel = 2, Ok = 1
    }

    // This describes a dialog that asks the user to reset the DUT.
    internal class SimpleDialog : IDisplayAnnotation
    {
        public SimpleDialog()
        {
        }

        [Layout(LayoutMode.FullRow)] // Set the layout of the property to fill the entire row.
        [Browsable(true)] // Show it event though it is read-only.
        public string Message { get; set; }

        [Layout(LayoutMode.FloatBottom | LayoutMode.FullRow)] // Show the button selection at the bottom of the window.
        [Submit] // When the button is clicked the result is 'submitted', so the dialog is closed.
        public WaitForInputResult Response { get; set; }

        public string Name { get; set; }

        string IDisplayAnnotation.Description => string.Empty;

        string[] IDisplayAnnotation.Group => Array.Empty<string>();

        double IDisplayAnnotation.Order => -10000;

        bool IDisplayAnnotation.Collapsed => false;
    }

    // This describes a dialog that asks the user to enter the serial number.
    [Display("Please enter serial number")]
    internal class EnterSNDialog
    {
        // Serial number to be entered by the user.
        [Display("Serial Number")]
        public string SerialNumber { get; set; }
    }
}