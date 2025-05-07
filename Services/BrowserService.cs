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

        /// <summary>
        /// Opens a file in Notepad++ editor
        /// </summary>
        /// <param name="path">Path to the file to open</param>
        /// <returns>True if successfully opened, false otherwise</returns>
        public bool OpenInNotepadPlusPlus(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    Debug.WriteLine($"File not found: {path}");
                    return false;
                }

                // Common Notepad++ installation paths
                string[] notepadPlusPlusPaths = new[]
                {
                    @"C:\Program Files\Notepad++\notepad++.exe",
                    @"C:\Program Files (x86)\Notepad++\notepad++.exe"
                };

                // Try to find Notepad++
                string notepadPlusPlusPath = Array.Find(notepadPlusPlusPaths, File.Exists);

                if (!string.IsNullOrEmpty(notepadPlusPlusPath))
                {
                    // Start Notepad++ with the document
                    using var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = notepadPlusPlusPath,
                        Arguments = $"\"{path}\"", // Quote the path to handle spaces
                        UseShellExecute = false
                    });
                    return true;
                }
                else
                {
                    // If Notepad++ not found, try using default text editor
                    Debug.WriteLine("Notepad++ not found in standard locations, using default application");
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
                Debug.WriteLine($"Error opening document in Notepad++: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Opens a file in default browser
        /// </summary>
        /// <param name="path">Path to the file to open</param>
        /// <returns>True if successfully opened, false otherwise</returns>
        public bool OpenInDefaultBrowser(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    Debug.WriteLine($"File not found: {path}");
                    return false;
                }

                // Convert to file URI if it's a file path
                var fileUri = new Uri(path).AbsoluteUri;

                // Start default browser with the document
                using var process = Process.Start(new ProcessStartInfo
                {
                    FileName = fileUri,
                    UseShellExecute = true
                });
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error opening document in default browser: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Opens a file in MS Word
        /// </summary>
        /// <param name="path">Path to the file to open</param>
        /// <returns>True if successfully opened, false otherwise</returns>
        public bool OpenInWord(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    Debug.WriteLine($"File not found: {path}");
                    return false;
                }

                // Common Word installation paths
                string[] wordPaths = new[]
                {
                    @"C:\Program Files\Microsoft Office\root\Office16\WINWORD.EXE",
                    @"C:\Program Files (x86)\Microsoft Office\root\Office16\WINWORD.EXE",
                    @"C:\Program Files\Microsoft Office\Office16\WINWORD.EXE",
                    @"C:\Program Files (x86)\Microsoft Office\Office16\WINWORD.EXE",
                    @"C:\Program Files\Microsoft Office\Office15\WINWORD.EXE",
                    @"C:\Program Files (x86)\Microsoft Office\Office15\WINWORD.EXE"
                };

                // Try to find Word
                string wordPath = Array.Find(wordPaths, File.Exists);

                if (!string.IsNullOrEmpty(wordPath))
                {
                    // Start Word with the document
                    using var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = wordPath,
                        Arguments = $"\"{path}\"", // Quote the path to handle spaces
                        UseShellExecute = false
                    });
                    return true;
                }
                else
                {
                    // If Word not found in standard locations, try using shell association
                    Debug.WriteLine("MS Word not found in standard locations, using default application");
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
                Debug.WriteLine($"Error opening document in Word: {ex.Message}");
                return false;
            }
        }
    }
}
