using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Storage;
using System.IO.IsolatedStorage;
using System.Diagnostics;
using System.IO;
using System.Collections.Concurrent;
using System.Threading;

namespace ERAUtils.Logger
{
    /// <summary>
    /// This class is used to create log files while running the program. This class is threadsafe.
    /// </summary>
    public static class Logger
    {
        
        #region Options
        private static readonly Severity __DEBUGSEVERITY = Severity.Debug;

        private static readonly String PATH = @"Logs\";
        private static readonly String PREFIX = "Log_";
        private static readonly DateTime DATE = DateTime.Now;

        private static Severity __CONSOLESEVERITY = Severity.None;
        #endregion

        private static String _fullPath = new StringBuilder(PATH).Append(PREFIX).Append(DATE.ToString("MM-dd-yyyy")).Append(".txt").ToString();
        private static Object _lock = new Object();
        private static ConcurrentQueue<String> _messageQueue = new ConcurrentQueue<String>();

        private static Severity LogSeverity { get; set; }

        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="message">The message to be logged</param>
        /// <param name="severity">The Severity of the log message</param>
        public static void LogMessage(String message, Severity severity)
        {
            StringBuilder intermediate = new StringBuilder("[").Append(DateTime.Now.ToShortTimeString()).Append(":").Append(DateTime.Now.Second.ToString()).Append("] ").Append(severity.ToString());

#if DEBUG && !NOTRACE
            StackTrace t = new StackTrace(true);
            StackFrame f = t.GetFrame(1);

            var methods = System.Enum.GetNames(typeof(Severity)).ToList();
            if (methods.Contains(f.GetMethod().Name))
                f = t.GetFrame(2);

            intermediate = intermediate.Append(@"<").Append(f.GetMethod().DeclaringType.Name).Append(":").Append(f.GetFileLineNumber()).Append(", ").Append(f.GetMethod().Name).Append(@">");
#endif

            message = intermediate.Append(": ").AppendLine(message).ToString();
            //System.Diagnostics.Debug.WriteIf(severity >= __DEBUGSEVERITY, message, System.Enum.GetName(typeof(Severity), severity));
            
            if(severity >= __CONSOLESEVERITY)
                Console.Write(message);

#if !DEBUG
            _messageQueue.Enqueue(message);

            // Thread safe logging
            if (Monitor.TryEnter(_lock))
            {
                using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForDomain())
                {
                    String writeString;
                    while (_messageQueue.TryDequeue(out writeString))
                    {
                        try
                        {
                            using (IsolatedStorageFileStream stream = isf.OpenFile(_fullPath, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write))
                            {
                                Int64 eof = stream.Length;
                                Byte[] bytes = UTF8Encoding.ASCII.GetBytes(writeString);
                                //stream.Lock(eof, bytes.Length);
                                stream.Position = eof;
                                stream.Write(bytes, 0, bytes.Length);
                                //stream.Unlock(eof, bytes.Length);
                            }

                            break;
                        }
                        catch (IOException)
                        {
                            _messageQueue.Enqueue(writeString);
                        }
                    }
                }

                Monitor.Exit(_lock);
            }
#endif
        }

        /// <summary>
        /// Logs a message with a fatal severity
        /// </summary>
        /// <param name="message">The message to be logged</param>
        public static void Fatal(String message)
        {
            LogMessage(message, Severity.Fatal);
        }

        /// <summary>
        /// Logs a message with an error severity
        /// </summary>
        /// <param name="message">The message to be logged</param>
        public static void Error(String message)
        {
            // Do not write to file if not severe enough
            if (Severity.Error < LogSeverity)
                return;

            LogMessage(message, Severity.Error);
        }

        /// <summary>
        /// Logs a message with an error severity
        /// </summary>
        /// <param name="message_parts">The message to be logged</param>
        public static void Error(String[] message_parts)
        {
            Error(String.Join("", message_parts));
        }

        /// <summary>
        /// Logs a message with a warning severity
        /// </summary>
        /// <param name="message">The message to be logged</param>
        public static void Warning(String message)
        {
            // Do not write to file if not severe enough
            if (Severity.Warning < LogSeverity)
                return;

            LogMessage(message, Severity.Warning);
        }

        /// <summary>
        /// Logs a message with a warning severity
        /// </summary>
        /// <param name="message_parts">The message to be logged</param>
        public static void Warning(String[] message_parts)
        {
            Warning(String.Join("", message_parts));
        }

        /// <summary>
        /// Logs a message with a notice severity
        /// </summary>
        /// <param name="message">The message to be logged</param>
        public static void Notice(String message)
        {
            // Do not write to file if not severe enough
            if (Severity.Notice < LogSeverity)
                return;

            LogMessage(message, Severity.Notice);
        }

        /// <summary>
        /// Logs a message with a notice severity
        /// </summary>
        /// <param name="message_parts">The message to be logged</param>
        public static void Notice(String[] message_parts)
        {
            Notice(String.Join("", message_parts));
        }


        /// <summary>
        /// Logs a message with an info severity
        /// </summary>
        /// <param name="message">The message to be logged</param>
        public static void Info(String message)
        {
            // Do not write to file if not severe enough
            if (Severity.Info < LogSeverity)
                return;

            LogMessage(message, Severity.Info);
        }

        /// <summary>
        /// Logs a message with an info severity
        /// </summary>
        /// <param name="message_parts">The message to be logged</param>
        public static void Info(String[] message_parts)
        {
            Info(String.Join("", message_parts));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public static void Info(String format, params Object[] args)
        {
            Info(String.Format(format, args));
        }

        /// <summary>
        /// Logs a message with a debug severity
        /// </summary>
        /// <param name="message">The message to be logged</param>
        [Conditional("DEBUG")]
        public static void Debug(String message)
        {
            // Do not write to file if not severe enough
            if (Severity.Debug < LogSeverity)
                return;

            LogMessage(message, Severity.Debug);
        }

        /// <summary>
        /// Logs a message with a debug severity
        /// </summary>
        /// <param name="message_parts">The message to be logged</param>
        [Conditional("DEBUG")]
        public static void Debug(String[] message_parts)
        {
            Debug(String.Join("", message_parts));
        }

        /// <summary>
        /// Logs a message with a verbose severity
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        [Conditional("DEBUG")]
        public static void Debug(String format, params String[] args)
        {
            Debug(String.Format(format, args));
        }

        /// <summary>
        /// Logs a message with a verbose severity
        /// </summary>
        /// <param name="message">The message to be logged</param>
        [Conditional("DEBUG")]
        public static void Verbose(String message)
        {
            // Do not write to file if not severe enough
            if (Severity.Verbose < LogSeverity)
                return;

            LogMessage(message, Severity.Verbose);
        }

        /// <summary>
        /// Logs a message with a verbose severity
        /// </summary>
        /// <param name="message_parts">The message to be logged</param>
        [Conditional("DEBUG")]
        public static void Verbose(String[] message_parts)
        {
            Verbose(String.Join("", message_parts));
        }

        /// <summary>
        /// Logs a message with a verbose severity
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        [Conditional("DEBUG")]
        public static void Verbose(String format, params String[] args)
        {
            Verbose(String.Format(format, args));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message_parts"></param>
        /// <param name="severity"></param>
        public static void LogMessage(Severity severity, params String[] message_parts)
        {
            LogMessage(String.Join("", message_parts), severity);
        }

        /// <summary>
        /// 
        /// </summary>
        public static void Initialize(Severity logSeverity)
        {
            IsolatedStorageFile.GetUserStoreForDomain().CreateDirectory(PATH);

            LogSeverity = logSeverity;

            Verbose("Logger created path ::USER::" + PATH);
            Verbose("Logger initialized");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consoleSeverity"></param>
        public static void Initialize(Severity logSeverity, Severity consoleSeverity)
        {
            Initialize(logSeverity);

            __CONSOLESEVERITY = consoleSeverity;

            if (logSeverity > consoleSeverity)
                Warning("logSeverity > consoleSeverity");
        }


    }
}
