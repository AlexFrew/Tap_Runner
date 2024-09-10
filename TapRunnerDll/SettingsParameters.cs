using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TapRunnerDll
{
    /// <summary>
    /// Represents the parameters for a specific setting in a test plan step.
    /// </summary>
    public class SettingsParameters
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsParameters"/> class.
        /// </summary>
        /// <param name="stepName">The name of the test plan step.</param>
        /// <param name="variableName">The name of the variable within the step.</param>
        /// <param name="value">The value to be assigned to the variable.</param>
        public SettingsParameters(string stepName, string variableName, object value)
        {
            StepName = stepName;
            VariableName = variableName;
            Value = value;
        }

        /// <summary>
        /// Gets or sets the name of the test plan step.
        /// </summary>
        public string StepName { get; private set; }

        /// <summary>
        /// Gets or sets the name of the variable within the step.
        /// </summary>
        public string VariableName { get; private set; }

        /// <summary>
        /// Gets or sets the value to be assigned to the variable.
        /// </summary>
        public object Value { get; private set; }
    }
}