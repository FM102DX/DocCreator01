using DocCreator01.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocCreator01.Services
{
    public class AppPathsHelper : IAppPathsHelper
    {
        private const string CompanyName = "RICompany";
        private const string ProductName = "DockPartApp";
        private const string SettingsFileName = "appdata.docpartsettings";
        
        /// <summary>
        /// Тут хранятся настройки приложения
        /// </summary>
        public string AppDataDirectory { get; }

        /// <summary>
        /// Тут хранятся настройки приложения
        /// </summary>
        public string ExeFileDirectory { get; }

        /// <summary>
        /// Тут лежат питон скрипты
        /// </summary>
        public string ScriptsDirectory { get; }

        /// <summary>
        /// Тут лежат иконки файлов
        /// </summary>
        public string IconsDirectory { get; }

        /// <summary>
        /// Gets the path to the folder where generated documents are stored
        /// </summary>
        public string DocumentsOutputDirectory { get; }
        
        /// <summary>
        /// Gets the path to the application settings file
        /// </summary>
        public string SettingsFilePath => Path.Combine(AppDataDirectory, SettingsFileName);
        
        public AppPathsHelper()
        {
            // Initialize paths
            ExeFileDirectory = AppContext.BaseDirectory; // Set to the application's executable directory

            AppDataDirectory = GetAppDataPath();

            ScriptsDirectory = Path.Combine(ExeFileDirectory, "Scripts");
            IconsDirectory = Path.Combine(ExeFileDirectory, "Icons");
            DocumentsOutputDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "DocCreator");

            // Ensure directories exist
            EnsureDirectoriesExist();
        }
        
        /// <summary>
        /// Ensures all necessary application directories exist
        /// </summary>
        public void EnsureDirectoriesExist()
        {
            Directory.CreateDirectory(AppDataDirectory);
            Directory.CreateDirectory(ScriptsDirectory);
            Directory.CreateDirectory(DocumentsOutputDirectory);
            Directory.CreateDirectory(IconsDirectory); // Ensure icons directory exists
        }
        
        /// <summary>
        /// Gets the path to the application data directory
        /// </summary>
        private string GetAppDataPath()
        {
            string docsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                CompanyName, 
                ProductName);
            return docsDir;
        }
    }
}
