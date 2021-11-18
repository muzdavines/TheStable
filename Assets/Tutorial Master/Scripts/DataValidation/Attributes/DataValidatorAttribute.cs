using System;

namespace HardCodeLab.TutorialMaster
{
    /// <inheritdoc />
    /// <summary>
    /// Specifies that this data structure is possible to validate.
    /// </summary>
    /// <seealso cref="T:System.Attribute" />
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class DataValidatorAttribute : Attribute
    {
        public readonly Type DataValidatorType;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataValidatorAttribute"/> class.
        /// </summary>
        /// <param name="dataValidatorType">Class type of data validator which will be used to validate it.</param>
        public DataValidatorAttribute(Type dataValidatorType)
        {
            DataValidatorType = dataValidatorType;
        }
    }
}