namespace DocCreator01.Contracts
{
    /// <summary>
    /// Interface for browser operations
    /// </summary>
    public interface IBrowserService
    {
        /// <summary>
        /// Opens a file or URL in Opera browser
        /// </summary>
        /// <param name="path">Path to the file or URL to open</param>
        /// <returns>True if successfully opened, false otherwise</returns>
        bool OpenInOpera(string path);

        /// <summary>
        /// Opens a file in Notepad++ editor
        /// </summary>
        /// <param name="path">Path to the file to open</param>
        /// <returns>True if successfully opened, false otherwise</returns>
        bool OpenInNotepadPlusPlus(string path);

        /// <summary>
        /// Opens a file in default browser
        /// </summary>
        /// <param name="path">Path to the file to open</param>
        /// <returns>True if successfully opened, false otherwise</returns>
        bool OpenInDefaultBrowser(string path);
        
        /// <summary>
        /// Opens a file in MS Word
        /// </summary>
        /// <param name="path">Path to the file to open</param>
        /// <returns>True if successfully opened, false otherwise</returns>
        bool OpenInWord(string path);
    }
}
