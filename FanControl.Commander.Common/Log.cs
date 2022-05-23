using System;

namespace FanControl.Commander.Common
{
    public class Log
    {
        #region Public methods

        public static void WriteToLog(String fileName, String data)
        {
            if (!String.IsNullOrWhiteSpace(fileName) && !String.IsNullOrWhiteSpace(data))
            {
                try
                {
                    System.IO.File.AppendAllText(fileName, $"{DateTime.UtcNow:R} {data}{Environment.NewLine}");
                }
                catch (System.Security.SecurityException)
                {

                }
                catch (System.IO.IOException)
                {

                }
                catch (NotSupportedException)
                {

                }
                catch (UnauthorizedAccessException)
                {

                }
                catch (ArgumentException)
                {

                }
            }
        }

        #endregion
    }
}