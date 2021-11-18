using System;
using HardCodeLab.TutorialMaster.Runtime.Metadata;
using UnityEngine;

namespace HardCodeLab.TutorialMaster
{
    /// <summary>
    /// Logs Tutorial Master related errors
    /// </summary>
    public static class TMLogger
    {
        /// <summary>
        /// Gets or sets the logging level of the Tutorial Master
        /// </summary>
        /// <value>
        /// The logging level.
        /// </value>
        public static LoggingLevel LoggingLevel
        {
            get
            {
                if (!PlayerPrefs.HasKey("tm_loggingLevel"))
                {
                    PlayerPrefs.SetInt("tm_loggingLevel", (int)LoggingLevel.WarningsAndErrors);
                }

                return (LoggingLevel)PlayerPrefs.GetInt("tm_loggingLevel");
            }
            set
            {
                PlayerPrefs.SetInt("tm_loggingLevel", (int)value);
            }
        }

        private static TutorialMetadata GetMetadata(TutorialMasterManager manager)
        {
            return TutorialMetadata.Create(manager);
        }

        /// <summary>
        /// Prepended to the beginning for every log message. Contains the caller name and type.
        /// </summary>
        private const string LogHeaderWithCaller = "<b>[Tutorial Master]</b> [{0}] [Caller \"{1}\" of type \"{2}\"] {3}";

        /// <summary>
        /// Prepended to the beginning for every log message.
        /// </summary>
        private const string LogHeader = "<b>[Tutorial Master]</b> {0}";

        /// <summary>
        /// Logs the specified Message to Unity console unless user has disabled them
        /// </summary>
        /// <param name="message">String or object to be converted to string representation for display.</param>
        /// <param name="caller">MonoBehaviour from which message has been logged</param>
        public static void LogInfo(string message, TutorialMasterManager caller = null)
        {
            if (LoggingLevel != LoggingLevel.Full)
                return;

            if (caller != null)
            {
                Debug.Log(string.Format(LogHeaderWithCaller, GetMetadata(caller), caller.name, caller.GetType().Name, message), caller);
            }
            else
            {
                Debug.Log(string.Format(LogHeader, message));
            }
        }

        /// <summary>
        /// Logs the exception then returns it
        /// </summary>
        /// <param name="exception">The exception that will be logged.</param>
        /// <returns>Exception. Usually used to throw it by the caller function.</returns>
        public static Exception LogException(Exception exception)
        {
            Debug.LogError(exception.Message);
            return exception;
        }

        /// <summary>
        /// Logs all issues the validator has picked up.
        /// </summary>
        /// <param name="validator">The validator which issues will be logged.</param>
        /// <param name="caller">MonoBehaviour from which message has been logged</param>
        public static void LogValidationErrors(DataValidator validator, TutorialMasterManager caller = null)
        {
            foreach (var issue in validator.Issues)
            {
                LogError(issue, caller);
            }
        }

        /// <summary>
        /// Logs the specified Error Message to Unity console unless user has disabled them.
        /// </summary>
        /// <param name="message">String or object to be converted to string representation for display.</param>
        /// <param name="caller">MonoBehaviour from which message has been logged.</param>
        public static void LogError(string message, TutorialMasterManager caller = null)
        {
            if (LoggingLevel != LoggingLevel.WarningsAndErrors
                && LoggingLevel != LoggingLevel.ErrorsOnly
                && LoggingLevel != LoggingLevel.Full)
                return;


            if (caller != null)
            {
                Debug.LogError(string.Format(LogHeaderWithCaller,
                        GetMetadata(caller),
                        caller.name,
                        caller.GetType().Name,
                        message),
                    caller);
            }
            else
            {
                Debug.LogError(string.Format(LogHeader, message));
            }
        }

        /// <summary>
        /// Logs the specified Warning Message to Unity console unless user has disabled them
        /// </summary>
        /// <param name="message">String or object to be converted to string representation for display.</param>
        /// <param name="caller">MonoBehaviour from which message has been logged</param>
        public static void LogWarning(string message, TutorialMasterManager caller = null)
        {
            if (LoggingLevel != LoggingLevel.WarningsAndErrors
                && LoggingLevel != LoggingLevel.WarningsOnly
                && LoggingLevel != LoggingLevel.Full)
                return;

            if (caller != null)
            {
                Debug.LogWarning(string.Format(LogHeaderWithCaller, GetMetadata(caller), caller.name, caller.GetType().Name, message), caller);
            }
            else
            {
                Debug.LogWarning(string.Format(LogHeader, message));
            }
        }
    }
}