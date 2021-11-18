using System;
using System.Collections.Generic;

namespace HardCodeLab.TutorialMaster
{
    /// <summary>
    /// Used to validate a data type to specify whether it's valid for runtime usage.
    /// </summary>
    public abstract class DataValidator
    {
        public List<string> Issues;
        protected Type DataType;

        protected DataValidator(Type dataType)
        {
            DataType = dataType;
        }

        protected void AddIssue(string issue, string prefix = "")
        {
            Issues.Add(string.Format("{0}> {1}", prefix, issue));
        }

        /// <summary>
        /// Adds new issues with a prefix.
        /// </summary>
        /// <param name="issues">The issue.</param>
        /// <param name="prefix">Prefix of those issues.</param>
        protected void AddIssues(List<string> issues, string prefix = "")
        {
            foreach (var issue in issues)
            {
                AddIssue(issue, prefix);
            }
        }

        /// <summary>
        /// Checks if a given data structure has valid fields
        /// </summary>
        /// <param name="data">The data that will be validated.</param>
        /// <returns>Returns true if validation checks passed.</returns>
        public abstract bool Validate(object data);
    }

    public abstract class DataValidator<TData> : DataValidator
        where TData : class
    {
        protected DataValidator(Type dataType) : base(dataType)
        {
        }

        public override bool Validate(object data)
        {
            Issues = new List<string>();
            var validateTarget = data as TData;

            if (validateTarget == null)
                return true;

            OnValidate(validateTarget);

            return Issues.Count == 0;
        }

        protected abstract void OnValidate(TData data);
    }
}