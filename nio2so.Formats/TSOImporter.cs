using nio2so.Formats.TSOData;
using System.Text;

namespace nio2so.Formats
{
    public abstract class TSOFileImporterBase<T> : TSOImporter<T> where T : ITSOImportable
    {
        /// <summary>
        /// Allows inheritors to easily append strings to the <see cref="ErrorOut"/> property
        /// </summary>
        protected StringBuilder DefaultErrorOutBuilder { get; } = new StringBuilder();
        /// <summary>
        /// Allows inheritors to easily append strings to the <see cref="ErrorOut"/> property
        /// </summary>
        protected StringBuilder DefaultWarningOutBuilder { get; } = new StringBuilder();
        /// <summary>
        /// Allows inheritors to easily append strings to the <see cref="ErrorOut"/> property
        /// </summary>
        protected StringBuilder DefaultMessageOutBuilder { get; } = new StringBuilder();
        public string ErrorOut => DefaultErrorOutBuilder.ToString();
        public string WarningOut => DefaultWarningOutBuilder.ToString();
        public string MessageOut => DefaultMessageOutBuilder.ToString();

        protected enum TSOImporterBaseChannel
        {
            Error,
            Warning, 
            Message
        }
        /// <summary>
        /// Writes a line to the <see cref="ErrorOut"/> or <see cref="WarningOut"/> or <see cref="MessageOut"/>. 
        /// <para>Format: <c>[<paramref name="Caption"/>] <paramref name="Message"/></c></para>
        /// </summary>
        /// <param name="Channel"></param>
        /// <param name="Caption"></param>
        /// <param name="Message"></param>
        protected void DefaultAppendLine(TSOImporterBaseChannel Channel, string Caption, string Message) => (Channel switch
        {
            TSOImporterBaseChannel.Error => DefaultErrorOutBuilder,
            TSOImporterBaseChannel.Warning => DefaultWarningOutBuilder,
            TSOImporterBaseChannel.Message => DefaultMessageOutBuilder,
        }).AppendLine($"[{Caption}] {Message}");
        /// <summary>
        /// Writes a line to the <see cref="ErrorOut"/> or <see cref="WarningOut"/> or <see cref="MessageOut"/>. 
        /// <para>Format: <c>[<paramref name="Caption"/>] <paramref name="Message"/></c></para>
        /// </summary>
        /// <param name="Channel"></param>
        /// <param name="Message"></param>
        protected void DefaultAppendLine(TSOImporterBaseChannel Channel, string Message) => DefaultAppendLine(Channel, GetType().Name, Message);

        /// <summary>
        /// Imports a <see cref="TSODataFile"/> such as TSODataDefinition.dat in the root directory of The Sims Online.
        /// </summary>
        /// <param name="FilePath"></param>
        /// <returns></returns>
        public T ImportFromFile(string FilePath)
        {
            if (!File.Exists(FilePath)) return default;
            using (FileStream fs = File.OpenRead(FilePath))
                return Import(fs);
        }
        public abstract T Import(Stream stream);
    }

    public interface TSOImporter<T> where T : ITSOImportable
    {
        /// <summary>
        /// Errors (typically meaning importing cannot continue or there is noticable lost info) are listed here.
        /// <para>They should be formatted such that each error is on its own line and has the problem component clearly listed in []
        /// followed by a descriptive message of what the error was.</para>
        /// <para>Example: <c>[cTSOUIScriptImporter] Tried to read a Token past the end of the file, the file may be corrupt.
        /// Current token: Begin</c></para>
        /// </summary>
        public string ErrorOut { get; }
        /// <summary>
        /// Warnings (typically meaning importing can continue, but there may be some noticable lost info) are listed here.
        /// <para>They should be formatted such that each error is on its own line and has the problem component clearly listed in []
        /// followed by a descriptive message of what the warning was.</para>
        /// <para>Example: <c>[cTSOUIScriptImporter] An unknown token was found on Line: 14 at Character: 3 '!'.</c></para>
        /// </summary>
        public string WarningOut { get; }
        /// <summary>
        /// Messages (typically just helpful info) are listed here.
        /// <para>They should be formatted such that each error is on its own line and has the problem component clearly listed in []
        /// followed by a descriptive message.</para>
        /// <para>Example: <c>[cTSOUIScriptImporter] Added a Button to group: 1 with the propertie(s): size=(100,150)</c></para>
        /// </summary>
        public string MessageOut { get; }
        public T Import(Stream stream);
    }
}
