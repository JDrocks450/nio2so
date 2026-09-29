namespace nio2so.CrashHandler
{
    /// <summary>
    /// Arguments to change the behavior of the <see cref="ErrorWindow"/>
    /// </summary>
    public class ErrorHandlerArgs
    {
        /// <summary>
        /// These are the buttons shown at the bottom of the crash handler. They can be used to lead the user towards a solution to their problem.
        /// </summary>
        public string[] Buttons { get; set; } =
        {
            "OK"
        };
        /// <summary>
        /// The name of the program that caused the error. This can be set to whatever best represents the source of the problem.
        /// </summary>
        public string SourceProgramName { get; set; }

        public static readonly ErrorHandlerArgs Default = new();

        public ErrorHandlerArgs(params string[] buttons)
        {
            Buttons = buttons;
        }
    }
}
