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
        /// Тут лежат питон скрипты
        /// </summary>
        public string ScriptsDirectory { get; }
        
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
            AppDataDirectory = GetAppDataPath();
            ScriptsDirectory = Path.Combine(AppDataDirectory, "Scripts");
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
