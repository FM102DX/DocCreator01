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
        /// Gets the application data directory
        /// </summary>
        public string AppDataDir { get; }
        
        /// <summary>
        /// Gets the directory where Python scripts are stored
        /// </summary>
        public string ScriptsDirectory { get; }
        
        /// <summary>
        /// Gets the path to the folder where generated documents are stored
        /// </summary>
        public string OutputDirectory { get; }
        
        /// <summary>
        /// Gets the path to the application settings file
        /// </summary>
        public string SettingsFilePath => Path.Combine(AppDataDir, SettingsFileName);
        
        public AppPathsHelper()
        {
            // Initialize paths
            AppDataDir = GetProgramDataPath();
            ScriptsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts");
            OutputDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "DocCreator", "Generated");
            
            // Ensure directories exist
            EnsureDirectoriesExist();
        }
        
        /// <summary>
        /// Ensures all necessary application directories exist
        /// </summary>
        public void EnsureDirectoriesExist()
        {
            Directory.CreateDirectory(AppDataDir);
            Directory.CreateDirectory(ScriptsDirectory);
            Directory.CreateDirectory(OutputDirectory);
        }
        
        /// <summary>
        /// Gets the path to the application data directory
        /// </summary>
        private string GetProgramDataPath()
        {
            string docsDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                CompanyName, ProductName);
                
            return docsDir;
        }
    }
}
