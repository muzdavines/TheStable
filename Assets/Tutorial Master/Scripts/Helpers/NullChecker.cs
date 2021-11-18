namespace HardCodeLab.TutorialMaster
{
    /// <summary>
    /// A helper function to help quickly evaluate the state of a said object
    /// </summary>
    public static class NullChecker
    {
        /// <summary>
        /// Checks whether a specified target is null. Logs a warning message if it fails
        /// </summary>
        /// <typeparam name="T">Type of the target</typeparam>
        /// <param name="target">The object that will be evaluated.</param>
        /// <param name="failMessage">The fail message that will be logged if the target is null.</param>
        /// <param name="caller">Caller object that s</param>
        /// <returns>Returns true if target is null, returns false if target isn't null.</returns>
        public static bool IsNull<T>(T target, string failMessage, TutorialMasterManager caller = null)
        {
            if (target != null && !target.Equals(null)) 
                return false;

            TMLogger.LogWarning(failMessage, caller);

            return true;

        }
    }
}