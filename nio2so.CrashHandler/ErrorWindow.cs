using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace nio2so.CrashHandler
{
    public partial class ErrorWindow : Form
    {
        /// <summary>
        /// When the window is dismissed, this is the action the user took.
        /// <para/>Null indicates the window was closed without taking any actions.
        /// </summary>
        public string? Result { get; private set; }
        public bool ErrorShown { get; private set; } = false;

        public ErrorWindow()
        {
            InitializeComponent();
            Load += ErrorWindow_Load;
        }        

        /// <summary>
        /// <inheritdoc cref="SetupWindow(Exception, ErrorHandlerArgs)"/>
        /// </summary>
        /// <param name="Details"></param>
        /// <param name="Args"></param>
        public ErrorWindow(Exception Details, ErrorHandlerArgs Args, DateTime Time) : this()
        {
            SetupWindow(Details, Args, Time);
        }

        private void ErrorWindow_Load(object? sender, EventArgs e)
        {
            if (!ErrorShown)
                CheckForErrorDetail();
            if (!ErrorShown)
            {
                //ShowPrompt("No error details were found. Closing.");
                Close();
            }
        }

        private bool CheckForErrorDetail()
        {
            var path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            path = Path.Combine(path, ErrorDetail.FNAME);
            if (!File.Exists(path)) // if an error detail file exists here
                return false;
            try
            {
                using (var fstream = File.OpenRead(path)) {                    
                    ErrorDetail detail = JsonSerializer.Deserialize<ErrorDetail>(fstream);
                    SetupWindow(detail.ExceptionMessage, detail.ExceptionStackTrace, detail.ProgramName, detail.Arguments, detail.ErrorTime);
                }
                return true;
            }
            catch (Exception ex)
            {
                ShowPrompt("Saved error details could not be loaded. The window will now show details of why these details could not be loaded.");
                SetupWindow(ex, ErrorHandlerArgs.Default, DateTime.Now);
            }
            return false;
        }

        private void ShowPrompt(string Message, string Caption = default)
        {
            if (Caption == null) Caption = "Error";
            MessageBox.Show(Message, Caption);
        }

        public void SetupWindow(Exception Details, ErrorHandlerArgs Args, DateTime Time) => SetupWindow(Details.Message, Details.ToString(), Details.Source, Args,Time);

        /// <summary>
        /// Sets up the <see cref="ErrorWindow"/> with the given exception details and arguments.
        /// </summary>
        /// <param name="Details"></param>
        /// <param name="Args"></param>
        public void SetupWindow(string ExceptionMessage, string ExceptionStackTrace, string ProgramName, ErrorHandlerArgs Args, DateTime Time)
        {
            this.Text = ProgramName;

            ErrorTime.Text = Time.ToString();
            StackTraceText.Text = ExceptionStackTrace;
            ErrorMessageBox.Text = ExceptionMessage;

            ButtonsTray.Controls.Clear();
            foreach(var item in Args.Buttons)
            {
                Button action = new Button()
                {
                    Text = item.ToString(),
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowOnly
                };
                action.Click += (s, e) =>
                {
                    Result = item;
                    Close();
                };
                ButtonsTray.Controls.Add(action);
            }

            ErrorShown = true;
        }


        private static ErrorWindow DefaultCreateWindow(Exception Details, ErrorHandlerArgs? Arguments = default)
        {
            if (Arguments == null) Arguments = ErrorHandlerArgs.Default;
            ErrorWindow wnd = new ErrorWindow(Details, Arguments, DateTime.Now);
            return wnd;
        }

        /// <summary>
        /// Shows <see cref="Exception"/> details in a window blocking the calling thread until the window is dismissed.
        /// <para/>Returns which one of the <see cref="ErrorHandlerArgs.Buttons"/> actions were selected. Null if no action was taken (window was closed).
        /// </summary>
        /// <param name="Details"></param>
        public static string? ShowError(Exception Details, ErrorHandlerArgs? Arguments = default)
        {
            var wnd = DefaultCreateWindow(Details, ErrorHandlerArgs.Default);
            wnd.ShowDialog();
            return wnd.Result;
        }

        /// <summary>
        /// <inheritdoc cref="ShowError(Exception)"/>
        /// </summary>
        /// <param name="Details"></param>
        /// <returns></returns>
        public static async Task<string?> ShowErrorAsync(Exception Details, ErrorHandlerArgs? Arguments = default)
        {
            var wnd = DefaultCreateWindow(Details, ErrorHandlerArgs.Default);
            ManualResetEvent evt = new ManualResetEvent(false);
            await Task.Run(() =>
            {
                evt.WaitOne();
            });
            wnd.FormClosed += (s, e) => { evt.Set(); };
            wnd.Show();
            return wnd.Result;
        }

        public static void SaveErrorDetailsToFile(string EnclosingDirectory, Exception Details, ErrorHandlerArgs Arguments, DateTime Time = default)
        {
            if (string.IsNullOrWhiteSpace(EnclosingDirectory)) EnclosingDirectory = 
                    Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            if (Time == default) Time = DateTime.Now;
            ErrorDetail detail = new ErrorDetail()
            {
                ErrorTime = Time,
                ExceptionMessage = Details.Message,
                ExceptionStackTrace = Details.StackTrace,
                ProgramName = Arguments.SourceProgramName ?? Details.Source ?? "nio2so Crash Handler",
                Arguments = Arguments
            };
            string serializedObject = JsonSerializer.Serialize(detail);
            EnclosingDirectory = Path.Combine(EnclosingDirectory, ErrorDetail.FNAME);
            File.WriteAllTextAsync(EnclosingDirectory, serializedObject);
        }

        public static void InvokeErrorHandler(Exception Details, ErrorHandlerArgs Arguments, DateTime Time = default)
        {
            string EnclosingDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            SaveErrorDetailsToFile(EnclosingDirectory, Details, Arguments, Time);
            string crashHandlerPath = Path.Combine(EnclosingDirectory, typeof(ErrorWindow).Assembly.GetName().Name) + ".exe";
            Process.Start(crashHandlerPath);
        }
    }
}
