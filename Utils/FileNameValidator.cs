using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DocCreator01.Utils
{
    /// <summary>
    /// Utility class for validating file names according to Windows file naming rules
    /// </summary>
    public static class FileNameValidator
    {
        // Reserved Windows filenames (without extensions)
        private static readonly string[] ReservedNames = {
            "CON", "PRN", "AUX", "NUL",
            "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
            "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
        };

        /// <summary>
        /// Validates a filename against Windows naming rules
        /// </summary>
        /// <param name="fileName">The filename to validate (without path)</param>
        /// <returns>A tuple containing (isValid, errorMessage)</returns>
        public static (bool isValid, string errorMessage) Validate(string fileName)
        {
            // Check for null or empty
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return (false, "Имя файла не может быть пустым.");
            }

            // Check for invalid characters
            var invalidChars = Path.GetInvalidFileNameChars();
            var invalidChar = fileName.FirstOrDefault(c => invalidChars.Contains(c));
            if (invalidChar != '\0')
            {
                return (false, $"Имя файла содержит недопустимый символ: '{invalidChar}'");
            }

            // Check for reserved names (ignoring extensions)
            string filenameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            if (ReservedNames.Contains(filenameWithoutExtension, StringComparer.OrdinalIgnoreCase))
            {
                return (false, $"'{filenameWithoutExtension}' является зарезервированным именем файла Windows.");
            }

            // Check if filename starts or ends with spaces or periods
            if (fileName.StartsWith(" ") || fileName.StartsWith("."))
            {
                return (false, "Имя файла не может начинаться с пробела или точки.");
            }

            if (fileName.EndsWith(" ") || fileName.EndsWith("."))
            {
                return (false, "Имя файла не может заканчиваться пробелом или точкой.");
            }

            // Check for maximum length (255 is usually the limit for NTFS)
            if (fileName.Length > 255)
            {
                return (false, "Имя файла слишком длинное. Максимальная длина - 255 символов.");
            }

            return (true, string.Empty);
        }
    }
}
