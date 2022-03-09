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

        public void Create(string path)
        {
            try
            {
                _fileStream = File.Create(path);
            }
            catch (Exception ex)
            {
                ConsoleDisplay.Error($"Cannot write to log file ({path})", ex);
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

        public static string GenerateLogFileName()
        {
            return "test.txt";
        }
    }
}