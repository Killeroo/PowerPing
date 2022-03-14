/**************************************************************************
 * PowerPing - Advanced command line ping tool
 * Copyright (c) 2022 Matthew Carney [matthewcarney64@gmail.com]
 * https://github.com/Killeroo/PowerPing
 *************************************************************************/

using System.Text;

namespace PowerPing
{
    internal class LogFile : IDisposable
    {
        private FileStream? _fileStream;
        private ASCIIEncoding _asciiEncoder;
        private string _filePath;

        public LogFile(string logPath)
        {
            _asciiEncoder = new ASCIIEncoding();
            _filePath = logPath;

            Create(logPath);
        }

        public void Create(string filePath)
        {
            string? path = "";
            try
            {
                if (filePath.Contains(Path.DirectorySeparatorChar))
                {
                    // Check the directory we want to write to exits
                    path = Path.GetDirectoryName(filePath);
                    if (path != null && !Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }
            }
            catch (Exception e)
            {
                ConsoleDisplay.Error($"Cannot create file at {path} changing to {Directory.GetCurrentDirectory()}", e);

                // Change file to be written to current directory
                // when we can't create our first directory choice
                filePath = Path.Combine(Directory.GetCurrentDirectory(), Path.GetFileName(filePath)); 
            }

            try
            {
                // Create the file
                _fileStream = File.Create(filePath);
            }
            catch (Exception ex)
            {
                ConsoleDisplay.Error($"Cannot write to log file ({filePath})", ex);
                _fileStream = null;
            }
        }

        public void Append(string line)
        {
            if (_fileStream != null && _fileStream.CanWrite)
            {
                Console.WriteLine(line);
                _fileStream.Write(_asciiEncoder.GetBytes(line + Environment.NewLine));

                try
                {
                    _fileStream.Flush();
                }
                catch (Exception ex)
                {
                    ConsoleDisplay.Error($"Error writing to log file ({_filePath})", ex);
                }
            }
        }

        public void Dispose()
        {
            _fileStream?.Close();
        }

        public static string SetupPath(string inputtedPath, string address)
        {
            if (string.IsNullOrEmpty(inputtedPath))
            {
                // Generate a filename if we were given nothing
                inputtedPath = LogFile.GenerateFileName(address);
            }

            if (!Path.HasExtension(inputtedPath))
            {
                // Add a file to any path that we were given
                inputtedPath = Path.Combine(inputtedPath, LogFile.GenerateFileName(address));
            }

            return Helper.CheckForDuplicateFile(inputtedPath);
        }

        public static string GenerateFileName(string address)
        {
            DateTime logCreationTime = DateTime.Now;

            return string.Format("PowerPing_{0}_{1}{2}{3}_{4}{5}.txt",
                address,
                logCreationTime.Year,
                logCreationTime.Month,
                logCreationTime.Day,
                logCreationTime.Hour,
                logCreationTime.Minute);
        }

    }
}