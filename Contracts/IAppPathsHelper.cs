using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocCreator01.Contracts
{
    /// <summary>
    /// Interface for managing application paths
    /// </summary>
    public interface IAppPathsHelper
    {
        /// <summary>
        /// Gets the application data directory
        /// </summary>
        string AppDataDirectory { get; }
        
        /// <summary>
        /// Gets the directory where Python scripts are stored
        /// </summary>
        string ScriptsDirectory { get; }
        
        /// <summary>
        /// Gets the path to the folder where generated documents are stored
        /// </summary>
        string DocumentsOutputDirectory { get; }
        
        /// <summary>
        /// Gets the path to the application settings file
        /// </summary>
        string SettingsFilePath { get; }
        
        /// <summary>
        /// Ensures all necessary application directories exist
        /// </summary>
        void EnsureDirectoriesExist();
    }
}
