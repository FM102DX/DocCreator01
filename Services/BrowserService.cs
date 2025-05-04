using DocCreator01.Contracts;
using System;
using System.Diagnostics;
using System.IO;

namespace DocCreator01.Services
{
    public class BrowserService : IBrowserService
    {
        /// <summary>
        /// Opens a file or URL in Opera browser
        /// </summary>
        /// <param name="path">Path to the file or URL to open</param>
        /// <returns>True if successfully opened, false otherwise</returns>
        public bool OpenInOpera(string path)
        {
            try
            {
                // Convert to file URI if it's a file path
                if (File.Exists(path))
                {
                    // Convert local file path to URI format
                    var fileUri = new Uri(path).AbsoluteUri;
                    path = fileUri;
                }

                // Common Opera installation paths
                string[] operaPaths = new[]
                {
                    @"C:\Program Files\Opera\launcher.exe",
                    @"C:\Program Files (x86)\Opera\launcher.exe",
                    @"C:\Program Files\Opera GX\launcher.exe",
                    @"C:\Program Files (x86)\Opera GX\launcher.exe"
                };

                // Try to find Opera browser
                string operaPath = Array.Find(operaPaths, File.Exists);

                if (!string.IsNullOrEmpty(operaPath))
                {
                    // Start Opera with the document
                    using var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = operaPath,
                        Arguments = path,
                        UseShellExecute = false
                    });
                    return true;
                }
                else
                {
                    // If Opera not found in standard locations, try using default browser
                    using var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = path,
                        UseShellExecute = true
                    });
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error opening document in Opera: {ex.Message}");
                return false;
            }
        }
    }
}
