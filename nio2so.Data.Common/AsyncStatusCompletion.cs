namespace nio2so.Data.Common
{
    /// <summary>
    /// Represents the current completion status for an asyncronous <see cref="Task"/>
    /// </summary>
    public struct AsyncStatusCompletion
    {
        public enum CompletionProgressTypes
        {
            Value,
            Indeterminate,
        }

        public AsyncStatusCompletion(string overallTaskName, string currentTask, double overallProgress, bool Completed, double? TaskProgress = default) : this()
        {
            ArgumentException.ThrowIfNullOrEmpty(overallTaskName);
            ArgumentException.ThrowIfNullOrEmpty(currentTask);
            OverallTaskName = overallTaskName;
            CurrentTask = currentTask;
            OverallProgress = overallProgress;
            this.Completed = Completed;
            this.TaskProgress = TaskProgress;
        }
        /// <summary>
        /// The name of the overall work being completed
        /// </summary>
        public string OverallTaskName { get; set; }
        /// <summary>
        /// The current item being performed in the <see cref="OverallTaskName"/>
        /// </summary>
        public string CurrentTask { get; }
        /// <summary>
        /// The current estimated completion of the work in <see cref="CurrentTask"/>, as a percentage of the the total to be performed as a decimal number between 0 and 1
        /// <para/>Given that sometimes data isn't that granular as to break down individual statuses as a matter of percentage completed, it is acceptable for this to be null.
        /// </summary>
        public double? TaskProgress { get; }
        /// <summary>
        /// The current estimated completion of the overall work, as a percentage of the the total to be performed as a decimal number between 0 and 1
        /// <para/>If this is <see langword="null"/>, progress bars should be viewed as "Indeterminate" or some other generic loading symbol.
        /// </summary>
        public double? OverallProgress { get; }
        public CompletionProgressTypes ProgressType => OverallProgress.HasValue ? CompletionProgressTypes.Value : CompletionProgressTypes.Indeterminate;
        /// <summary>
        /// Gets a value indicating whether the operation has completed.
        /// </summary>
        public bool Completed { get; }
    }
}
