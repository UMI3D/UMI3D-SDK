///*
//Copyright 2019 - 2023 Inetum

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//    http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.
//*/
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Globalization;
//using System.Threading;
//using UnityEngine;

//namespace umi3d.debug
//{
//    /// <summary>
//    /// Enumeration flags for log type.
//    /// </summary>
//    [Flags]
//    public enum UMI3DLogType
//    {
//        /// <summary>
//        /// No logs.
//        /// </summary>
//        None = 0,

//        /// <summary>
//        /// TODO Logs that are only visible if UMI3D_DEBUG is defined.
//        /// 
//        /// <para>
//        /// They look like todo logs.
//        /// </para>
//        /// </summary>
//        DebugTodo = 1 << 0,
//        /// <summary>
//        /// HACK Logs that are only visible if UMI3D_DEBUG is defined.
//        /// 
//        /// <para>
//        /// They look like hack logs.
//        /// </para>
//        /// </summary>
//        DebugHack = 1 << 1,
//        /// <summary>
//        /// Table Logs that are only visible if UMI3D_DEBUG is defined.
//        /// 
//        /// <para>
//        /// They look like a table of logs.
//        /// </para>
//        /// </summary>
//        DebugTab = 1 << 2,
//        /// <summary>
//        /// Logs that are only visible if UMI3D_DEBUG is defined.
//        /// 
//        /// <para>
//        /// They look like default logs.
//        /// </para>
//        /// </summary>
//        Debug = 1 << 3,

//        /// <summary>
//        /// Default logs. You can used them in build.
//        /// </summary>
//        Default = 1 << 4,
//        /// <summary>
//        /// Warning logs. You can used them in build.
//        /// </summary>
//        Warning = 1 << 5,
//        /// <summary>
//        /// Assert Logs. You can used them in build.
//        /// </summary>
//        Assert = 1 << 6,
//        /// <summary>
//        /// Error Logs. You can used them in build.
//        /// </summary>
//        Error = 1 << 7,
//        /// <summary>
//        /// Exception Logs. You can used them in build.
//        /// </summary>
//        Exception = 1 << 8
//    }

//    /// <summary>
//    /// The color of the log when displayed in the Unity Console.
//    /// </summary>
//    public enum UMI3DLogColor
//    {
//        White,
//        Red,
//        Green,
//        Blue,
//        Magenta,
//        Cyan,
//        Yellow,
//        Orange,
//        Grey,
//        Black
//    }

//    /// <summary>
//    /// Alignment of a message in a formated string.
//    /// </summary>
//    public enum UMI3DStringAlignment
//    {
//        Left,
//        Center,
//        Right
//    }

//    /// <summary>
//    /// Represent a cell in a tab log.
//    /// </summary>
//    public sealed class UMI3DLogCell
//    {
//        /// <summary>
//        /// Title of the column.
//        /// </summary>
//        public string Header;
//        /// <summary>
//        /// The message inside the cell.
//        /// </summary>
//        public object Message;
//        /// <summary>
//        /// The size of the cell.
//        /// </summary>
//        public int StringFormatSize;
//        /// <summary>
//        /// The alignment of the message.
//        /// </summary>
//        public UMI3DStringAlignment StringFormatAlignment;

//        /// <summary>
//        /// Create a cell.
//        /// </summary>
//        /// <param name="header"></param>
//        /// <param name="message"></param>
//        /// <param name="stringFormatSize"></param>
//        /// <param name="alignment"></param>
//        public UMI3DLogCell(string header, object message, int stringFormatSize, UMI3DStringAlignment alignment = UMI3DStringAlignment.Left)
//        {
//            this.Header = header;
//            this.Message = message;
//            this.StringFormatSize = stringFormatSize;
//            this.StringFormatAlignment = alignment;
//        }
//    }

//    /// <summary>
//    /// A call trace is composed of the essential information to identify a calling method.
//    /// </summary>
//    public sealed class UMI3DCallTrace
//    {
//        const string Assets = "Assets";

//        /// <summary>
//        /// Name of the caller.
//        /// </summary>
//        public string MemberName = "";
//        /// <summary>
//        /// File path of the caller.
//        /// </summary>
//        public string SourceFilePath = "";
//        /// <summary>
//        /// Line number of the caller inside the file.
//        /// </summary>
//        public int SourceLineNumber = 0;

//        /// <summary>
//        /// Get a string representation of a call trace.
//        /// </summary>
//        public string Trace
//        {
//            get
//            {
//                string result = "at   ";

//                result += $"{MemberName}()   ";

//                result += PathAndNumber;

//                return result;
//            }
//        }

//        /// <summary>
//        /// A string representation of the path and line number of the call trace.
//        /// </summary>
//        public string PathAndNumber
//        {
//            get
//            {
//                return $"{Path}:{SourceLineNumber}";
//            }
//        }

//        /// <summary>
//        /// Path of the call trace begining with Asset.
//        /// </summary>
//        public string Path
//        {
//            get
//            {
//                var result = SourceFilePath;

//                var split = SourceFilePath.Split(new string[] { Assets }, StringSplitOptions.RemoveEmptyEntries);
//                if (split.Length > 0)
//                {
//                    result = split[1];
//                }
//                else
//                {
//                    result = split[0];
//                }

//                return $"{Assets}{result}";

//            }
//        }

//        /// <summary>
//        /// A href that can be clicked in the unity console.
//        /// </summary>
//        public string ClickableTrace
//        {
//            get
//            {
//                var result = $"<a href=\"{Path}\" line=\"{SourceLineNumber}\">{Trace}</a>";

//                return result;
//            }
//        }

//        /// <summary>
//        /// Create a call trace.
//        /// </summary>
//        /// <param name="memberName"></param>
//        /// <param name="sourceFilePath"></param>
//        /// <param name="sourceLineNumber"></param>
//        public UMI3DCallTrace(string memberName, string sourceFilePath, int sourceLineNumber)
//        {
//            MemberName = memberName;
//            SourceFilePath = sourceFilePath;
//            SourceLineNumber = sourceLineNumber;
//        }
//    }

//    /// <summary>
//    /// The data display in the log.
//    /// </summary>
//    public struct UMI3DLog
//    {
//        /// <summary>
//        /// Type of log.
//        /// </summary>
//        public UMI3DLogType Type;
//        /// <summary>
//        /// The main tag.
//        /// 
//        /// <para>
//        /// Usualy the type name of a logger's master.
//        /// </para>
//        /// </summary>
//        public string MainTag;
//        /// <summary>
//        /// The tag.
//        /// 
//        /// <para>
//        /// Used to identify the source of a log message. It usually identifies the method where the log call occurs.
//        /// </para>
//        /// </summary>
//        public string Tag;
//        /// <summary>
//        /// Color of the log in the Unity console.
//        /// </summary>
//        public UMI3DLogColor Color;
//        /// <summary>
//        /// The message.
//        /// </summary>
//        public object Message;
//        /// <summary>
//        /// The call trace.
//        /// </summary>
//        public UMI3DCallTrace Trace;

//        /// <summary>
//        /// Get a formated log.
//        /// </summary>
//        public string FormatedLog
//        {
//            get
//            {
//                return Message.FormatLog(Type, MainTag, Tag, Color, false, trace: Trace);
//            }
//        }

//        /// <summary>
//        /// Create a <see cref="UMI3DLog"/>.
//        /// </summary>
//        /// <param name="type"></param>
//        /// <param name="mainTag"></param>
//        /// <param name="tag"></param>
//        /// <param name="color"></param>
//        /// <param name="message"></param>
//        /// <param name="trace"></param>
//        public UMI3DLog(
//            UMI3DLogType type,
//            string mainTag,
//            string tag,
//            UMI3DLogColor color,
//            object message,
//            UMI3DCallTrace trace
//        )
//        {
//            this.Type = type;
//            this.MainTag = mainTag;
//            this.Tag = tag;
//            this.Color = color;
//            this.Message = message;
//            this.Trace = trace;
//        }
//    }

//    /// <summary>
//    /// A log reporter.
//    /// 
//    /// <para>
//    /// It is <see cref="UMI3DLog"/> queue that will display its logs when ask.
//    /// </para>
//    /// </summary>
//    public class UMI3DLogReport : Queue<UMI3DLog>
//    {
//        /// <summary>
//        /// The Id of this log report.
//        /// </summary>
//        public string ID;
//        /// <summary>
//        /// Maximum number of logs in this report.
//        /// </summary>
//        public int MaxNumberOfLogs;
//        /// <summary>
//        /// The main context of this logger.
//        /// 
//        /// <para>
//        /// Usualy the instance of the logger's master if it is a <see cref="UnityEngine.Object"/>.
//        /// </para>
//        /// </summary>
//        public UnityEngine.Object MainContext;
//        /// <summary>
//        /// The logger.
//        /// </summary>
//        public UMI3DLogger Logger;

//        /// <summary>
//        /// Create a log reporter.
//        /// </summary>
//        /// <param name="id"></param>
//        /// <param name="maxNumberOfLogs"></param>
//        /// <param name="mainContext"></param>
//        /// <param name="logger"></param>
//        public UMI3DLogReport(string id, int maxNumberOfLogs = 10, UnityEngine.Object mainContext = null, UMI3DLogger logger = null)
//        {
//            this.ID = id;
//            MaxNumberOfLogs = maxNumberOfLogs;
//            MainContext = mainContext;
//            Logger = logger;
//        }

//        /// <summary>
//        /// Add a report at the end of the report.
//        /// </summary>
//        /// <param name="log"></param>
//        public new void Enqueue(UMI3DLog log)
//        {
//            if (this.Count >= MaxNumberOfLogs)
//            {
//                Report();
//            }

//            base.Enqueue(log);
//        }

//        /// <summary>
//        /// Report the stored logs.
//        /// </summary>
//        public void Report()
//        {
//            if (this.Count == 0)
//            {
//                return;
//            }

//            string message = "";

//            message += $"Report [{ID}] start: {'-'.Repeat(10)}";

//#if DEBUG
//            message += $"\n\n";
//#else
//            message += $"\n";
//#endif

//            while (this.Count > 0)
//            {
//                var log = Dequeue();
//                message += log.FormatedLog;
//            }

//            message += $"Report [{ID}] end. {'-'.Repeat(10)}";
//            message += "\n";

//            var stackTraceType = Application.GetStackTraceLogType(UnityEngine.LogType.Log);
//            Application.SetStackTraceLogType(UnityEngine.LogType.Log, StackTraceLogType.None);

//            Logger.LogHandler.LogFormat(UnityEngine.LogType.Log, MainContext, message);

//            Application.SetStackTraceLogType(UnityEngine.LogType.Log, stackTraceType);
//        }
//    }


//    /// <summary>
//    /// A Logger for UMI3D.
//    /// 
//    /// <para>
//    /// To take advantage of this logger you can fallow this simple guide:
//    /// <list type="bullet">
//    /// <item>Right click on the console tab header and selection "Use Monospace font"</item>
//    /// </list>
//    /// </para>
//    /// </summary>
//    [Serializable]
//    public class UMI3DLogger
//    {
//        /// <summary>
//        /// All <see cref="UMI3DLogType"/> cases.
//        /// </summary>
//        public const UMI3DLogType LogTypeAllCases =
//            UMI3DLogType.DebugTodo
//            | UMI3DLogType.DebugHack
//            | UMI3DLogType.DebugTab
//            | UMI3DLogType.Debug
//            | UMI3DLogType.Default
//            | UMI3DLogType.Warning
//            | UMI3DLogType.Assert
//            | UMI3DLogType.Error
//            | UMI3DLogType.Exception;

//        /// <summary>
//        /// The static log type filter.
//        /// </summary>
//        public static UMI3DLogType GeneralLogType = LogTypeAllCases;

//        /// <summary>
//        /// The log handler that will handle logs.
//        /// 
//        /// <para>
//        /// By default <see cref="UnityEngine.Debug.unityLogger"/>.logHandler.
//        /// </para>
//        /// </summary>
//        public ILogHandler LogHandler;

//        /// <summary>
//        /// The log type filter of this logger.
//        /// </summary>
//        public UMI3DLogType LogType;
//        /// <summary>
//        /// The main tag of this logger.
//        /// 
//        /// <para>
//        /// Usualy the type name of the logger's master.
//        /// </para>
//        /// </summary>
//        public string MainTag;
//        /// <summary>
//        /// The main context of this logger.
//        /// 
//        /// <para>
//        /// Usualy the instance of the logger's master if it is a <see cref="UnityEngine.Object"/>.
//        /// </para>
//        /// </summary>
//        public UnityEngine.Object MainContext;
//        /// <summary>
//        /// Wherther or not the thread id has to be displayed.
//        /// 
//        /// <para>
//        /// It is helpfull when debuging multi-threaded feature.
//        /// </para>
//        /// </summary>
//        public bool IsThreadDisplayed;

//        /// <summary>
//        /// The list of table headers.
//        /// </summary>
//        Dictionary<string, (string[] headers, int cellIndex)> tabHeaders;

//        Dictionary<string, UMI3DLogReport> reporter;

//        /// <summary>
//        /// Create a logger by defining all of its properties.
//        /// </summary>
//        /// <param name="logHandler"></param>
//        /// <param name="logType"></param>
//        /// <param name="mainTag"></param>
//        /// <param name="mainContext"></param>
//        /// <param name="isThreadDisplayed"></param>
//        public UMI3DLogger(ILogHandler logHandler, UMI3DLogType logType, string mainTag, UnityEngine.Object mainContext, bool isThreadDisplayed)
//        {
//            this.LogHandler = logHandler;
//            this.LogType = logType;
//            this.MainTag = mainTag;
//            this.MainContext = mainContext;
//            this.IsThreadDisplayed = isThreadDisplayed;
//        }

//        /// <summary>
//        /// Create a logger by defining some of its properties.
//        /// </summary>
//        /// <param name="logType"></param>
//        /// <param name="mainTag"></param>
//        /// <param name="mainContext"></param>
//        /// <param name="isThreadDisplayed"></param>
//        public UMI3DLogger(UMI3DLogType logType = LogTypeAllCases, string mainTag = null, UnityEngine.Object mainContext = null, bool isThreadDisplayed = false) : this(
//            UnityEngine.Debug.unityLogger.logHandler,
//            logType,
//            mainTag,
//            mainContext,
//            isThreadDisplayed
//        )
//        {

//        }

//        /// <summary>
//        /// Get a log reporter.
//        /// </summary>
//        /// <param name="reportId"></param>
//        /// <param name="maxNumberOfLogs"></param>
//        /// <returns></returns>
//        public UMI3DLogReport GetReporter(string reportId, int maxNumberOfLogs = 10)
//        {
//            if (reporter == null)
//            {
//                reporter = new Dictionary<string, UMI3DLogReport>();
//            }

//            if (reporter.ContainsKey(reportId))
//            {
//                return reporter[reportId];
//            }
//            else
//            {
//                var report = new UMI3DLogReport(reportId, maxNumberOfLogs, MainContext, this);
//                reporter.Add(reportId, report);
//                return report;
//            }
//        }

//        /// <summary>
//        /// A variant of Log that logs an DebugTodo message.
//        /// 
//        /// <para>
//        /// DebugTodo logs:
//        /// <list type="bullet">
//        /// <item>are not displayed in build.</item>
//        /// <item>Have no stack trace.</item>
//        /// </list>
//        /// </para>
//        /// </summary>
//        /// <param name="tag">Used to identify the source of a log message. It usually identifies the method where the log call occurs.</param>
//        /// <param name="message">String or object to be converted to string representation for display.</param>
//        /// <param name="context">Object to which the message applies.</param>
//        /// <param name="color">The color of the log in the Unity console.</param>
//        /// <param name="memberName">The name of the caller.</param>
//        /// <param name="sourceFilePath">File path of the caller.</param>
//        /// <param name="sourceLineNumber">Line number in the file where of the caller.</param>
//        [Conditional("UNITY_EDITOR")]
//        public void DebugTodo(
//            string tag, object message,
//            UnityEngine.Object context = null, UMI3DLogColor color = UMI3DLogColor.Cyan,
//            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
//            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
//            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
//        )
//        {
//            if (!GeneralLogType.HasFlag(UMI3DLogType.DebugTodo) || !LogType.HasFlag(UMI3DLogType.DebugTodo))
//            {
//                return;
//            }

//            var stackTraceType = Application.GetStackTraceLogType(UnityEngine.LogType.Log);
//            Application.SetStackTraceLogType(UnityEngine.LogType.Log, StackTraceLogType.None);

//            LogHandler.LogFormat(UnityEngine.LogType.Log, context ?? MainContext, message.FormatLog(UMI3DLogType.DebugTodo, MainTag, tag, color, IsThreadDisplayed, "TODO:", new UMI3DCallTrace(memberName, sourceFilePath, sourceLineNumber)));

//            Application.SetStackTraceLogType(UnityEngine.LogType.Log, stackTraceType);
//        }

//        /// <summary>
//        /// A variant of Log that logs an DebugHack message.
//        /// 
//        /// <para>
//        /// DebugHack logs:
//        /// <list type="bullet">
//        /// <item>are not displayed in build.</item>
//        /// <item>Have no stack trace.</item>
//        /// </list>
//        /// </para>
//        /// </summary>
//        /// <param name="tag">Used to identify the source of a log message. It usually identifies the method where the log call occurs.</param>
//        /// <param name="message">String or object to be converted to string representation for display.</param>
//        /// <param name="context">Object to which the message applies.</param>
//        /// <param name="color">The color of the log in the Unity console.</param>
//        /// <param name="memberName">The name of the caller.</param>
//        /// <param name="sourceFilePath">File path of the caller.</param>
//        /// <param name="sourceLineNumber">Line number in the file where of the caller.</param>
//        [Conditional("UNITY_EDITOR")]
//        public void DebugHack(
//            string tag, object message,
//            UnityEngine.Object context = null, UMI3DLogColor color = UMI3DLogColor.Orange,
//            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
//            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
//            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
//        )
//        {
//            if (!GeneralLogType.HasFlag(UMI3DLogType.DebugHack) || !LogType.HasFlag(UMI3DLogType.DebugHack))
//            {
//                return;
//            }

//            var stackTraceType = Application.GetStackTraceLogType(UnityEngine.LogType.Log);
//            Application.SetStackTraceLogType(UnityEngine.LogType.Log, StackTraceLogType.None);

//            LogHandler.LogFormat(UnityEngine.LogType.Log, context ?? MainContext, message.FormatLog(UMI3DLogType.DebugHack, MainTag, tag, color, IsThreadDisplayed, "HACK:", new UMI3DCallTrace(memberName, sourceFilePath, sourceLineNumber)));

//            Application.SetStackTraceLogType(UnityEngine.LogType.Log, stackTraceType);
//        }

//        /// <summary>
//        /// A variant of Log that logs a DebugTab message.
//        /// 
//        /// <para>
//        /// Debug logs:
//        /// <list type="bullet">
//        /// <item>are displayed in build only if the UMI3D_DEBUG macro has been added.</item>
//        /// <item>have no stack trace.</item>
//        /// </list>
//        /// </para>
//        /// </summary>
//        /// <param name="tabName">A unique identifier for the tab.</param>
//        /// <param name="cells">The collection of cells in a line.</param>
//        /// <param name="context">Object to which the message applies.</param>
//        /// <param name="color">The color of the log in the Unity console.</param>
//        /// <param name="report">A report to wich this log will be added.</param>
//        /// <param name="memberName">The name of the caller.</param>
//        /// <param name="sourceFilePath">File path of the caller.</param>
//        /// <param name="sourceLineNumber">Line number in the file where of the caller.</param>
//        [Conditional("UMI3D_DEBUG")]
//        public void DebugTab(
//            string tabName, UMI3DLogCell[] cells,
//            UnityEngine.Object context = null, UMI3DLogColor color = UMI3DLogColor.White,
//            UMI3DLogReport report = null,
//            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
//            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
//            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
//        )
//        {
//            if (!GeneralLogType.HasFlag(UMI3DLogType.DebugTab) || !LogType.HasFlag(UMI3DLogType.DebugTab))
//            {
//                return;
//            }

//            var headers = new string[cells.Length];
//            var messages = new string[cells.Length];
//            for (int i = 0; i < cells.Length; i++)
//            {
//                var size = cells[i].StringFormatSize;
//                var alignment = cells[i].StringFormatAlignment;

//                headers[i] = cells[i].Header.FormatString(size, UMI3DStringAlignment.Center);
//                messages[i] = cells[i].Message.GetString().FormatString(size, alignment);
//            }

//            var stackTraceType = Application.GetStackTraceLogType(UnityEngine.LogType.Log);
//            Application.SetStackTraceLogType(UnityEngine.LogType.Log, StackTraceLogType.None);

//            var headerMessage = "Cell".FormatString(6) + " | " + string.Join(" | ", headers);
//            var cellIndex = 0;
//            if (tabHeaders == null)
//            {
//                tabHeaders = new Dictionary<string, (string[], int)>() { { tabName, (headers, cellIndex) } };

//                if (report != null)
//                {
//                    report.Enqueue(new UMI3DLog(
//                            UMI3DLogType.Debug,
//                            MainTag,
//                            $"Tab: {tabName}",
//                            color,
//                            headerMessage,
//                            null
//                    ));
//                }
//                else
//                {
//                    LogHandler.LogFormat(UnityEngine.LogType.Log, context ?? MainContext, headerMessage.FormatLog(UMI3DLogType.Debug, MainTag, $"Tab: {tabName}", color, false));
//                }

//            }
//            else if (!tabHeaders.ContainsKey(tabName))
//            {
//                tabHeaders.Add(tabName, (headers, cellIndex));

//                if (report != null)
//                {
//                    report.Enqueue(new UMI3DLog(
//                            UMI3DLogType.Debug,
//                            MainTag,
//                            $"Tab: {tabName}",
//                            color,
//                            headerMessage,
//                            null
//                    ));
//                }
//                else
//                {
//                    LogHandler.LogFormat(UnityEngine.LogType.Log, context ?? MainContext, headerMessage.FormatLog(UMI3DLogType.Debug, MainTag, $"Tab: {tabName}", color, false));
//                }
//            }
//            else
//            {
//                cellIndex = tabHeaders[tabName].cellIndex + 1;
//                tabHeaders[tabName] = (headers, cellIndex);
//            }

//            var message = $"{cellIndex}".FormatString(6) + " | " + string.Join(" | ", messages);
//            if (report != null)
//            {
//                report.Enqueue(new UMI3DLog(
//                        UMI3DLogType.Debug,
//                        MainTag,
//                        $"Tab: {tabName}",
//                        color,
//                        message,
//                        new UMI3DCallTrace(memberName, sourceFilePath, sourceLineNumber)
//                ));
//            }
//            else
//            {
//                LogHandler.LogFormat(UnityEngine.LogType.Log, context ?? MainContext, message.FormatLog(UMI3DLogType.Debug, MainTag, $"Tab: {tabName}", color, IsThreadDisplayed, trace: new UMI3DCallTrace(memberName, sourceFilePath, sourceLineNumber)));
//            }

//            Application.SetStackTraceLogType(UnityEngine.LogType.Log, stackTraceType);
//        }

//        /// <summary>
//        /// A variant of Log that logs an Debug message.
//        /// 
//        /// <para>
//        /// Debug logs:
//        /// <list type="bullet">
//        /// <item>are displayed in build only if the UMI3D_DEBUG macro has been added.</item>
//        /// <item>have no stack trace.</item>
//        /// </list>
//        /// </para>
//        /// </summary>
//        /// <param name="tag">Used to identify the source of a log message. It usually identifies the method where the log call occurs.</param>
//        /// <param name="message">String or object to be converted to string representation for display.</param>
//        /// <param name="context">Object to which the message applies.</param>
//        /// <param name="color">The color of the log in the Unity console.</param>
//        /// <param name="report">A report to wich this log will be added.</param>
//        /// <param name="memberName">The name of the caller.</param>
//        /// <param name="sourceFilePath">File path of the caller.</param>
//        /// <param name="sourceLineNumber">Line number in the file where of the caller.</param>
//        [Conditional("UMI3D_DEBUG")]
//        public void Debug(
//            string tag, object message,
//            UnityEngine.Object context = null, UMI3DLogColor color = UMI3DLogColor.White,
//            UMI3DLogReport report = null,
//            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
//            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
//            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
//        )
//        {
//            if (!GeneralLogType.HasFlag(UMI3DLogType.Debug) || !LogType.HasFlag(UMI3DLogType.Debug))
//            {
//                return;
//            }

//            UMI3DCallTrace trace = new UMI3DCallTrace(memberName, sourceFilePath, sourceLineNumber);

//            if (report != null)
//            {
//                report.Enqueue(new UMI3DLog(
//                        UMI3DLogType.Debug,
//                        MainTag,
//                        tag,
//                        color,
//                        message,
//                        trace
//                ));
//            }
//            else
//            {
//                var stackTraceType = Application.GetStackTraceLogType(UnityEngine.LogType.Log);
//                Application.SetStackTraceLogType(UnityEngine.LogType.Log, StackTraceLogType.None);

//                LogHandler.LogFormat(UnityEngine.LogType.Log, context ?? MainContext, message.FormatLog(UMI3DLogType.Debug, MainTag, tag, color, IsThreadDisplayed, trace: trace));

//                Application.SetStackTraceLogType(UnityEngine.LogType.Log, stackTraceType);
//            }
//        }

//        /// <summary>
//        /// A variant of Log that logs a DebugAssertion message.
//        /// 
//        /// <para>
//        /// DebugAssertion logs are displayed in build only if the UMI3D_DEBUG macro has been added.
//        /// </para>
//        /// </summary>
//        /// <param name="tag">Used to identify the source of a log message. It usually identifies the method where the log call occurs.</param>
//        /// <param name="message">String or object to be converted to string representation for display.</param>
//        /// <param name="context">Object to which the message applies.</param>
//        /// <param name="color">The color of the log in the Unity console.</param>
//        /// <param name="report">A report to wich this log will be added.</param>
//        /// <param name="memberName">The name of the caller.</param>
//        /// <param name="sourceFilePath">File path of the caller.</param>
//        /// <param name="sourceLineNumber">Line number in the file where of the caller.</param>
//        [Conditional("UMI3D_DEBUG")]
//        public void DebugAssertion(
//            string tag, object message,
//            UnityEngine.Object context = null, UMI3DLogColor color = UMI3DLogColor.Orange,
//            UMI3DLogReport report = null,
//            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
//            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
//            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
//        )
//        {
//            if (!GeneralLogType.HasFlag(UMI3DLogType.Assert) || !LogType.HasFlag(UMI3DLogType.Assert))
//            {
//                return;
//            }

//            if (report != null)
//            {
//                report.Enqueue(new UMI3DLog(
//                        UMI3DLogType.Assert,
//                        MainTag,
//                        tag,
//                        color,
//                        message,
//                        new UMI3DCallTrace(memberName, sourceFilePath, sourceLineNumber)
//                ));
//            }
//            else
//            {
//                LogHandler.LogFormat(UnityEngine.LogType.Assert, context ?? MainContext, message.FormatLog(UMI3DLogType.Assert, MainTag, tag, color, IsThreadDisplayed));
//            }
//        }

//        /// <summary>
//        /// A variant of Log that logs a DebugAssertion message if <paramref name="condition"/> is false.
//        /// 
//        /// <para>
//        /// DebugAssert logs are displayed in build only if the UMI3D_DEBUG macro has been added.
//        /// </para>
//        /// </summary>
//        /// <param name="condition">The condition has to be false to log the message.</param>
//        /// <param name="tag">Used to identify the source of a log message. It usually identifies the method where the log call occurs.</param>
//        /// <param name="message">String or object to be converted to string representation for display.</param>
//        /// <param name="context">Object to which the message applies.</param>
//        /// <param name="color">The color of the log in the Unity console.</param>
//        /// <param name="report">A report to wich this log will be added.</param>
//        /// <param name="memberName">The name of the caller.</param>
//        /// <param name="sourceFilePath">File path of the caller.</param>
//        /// <param name="sourceLineNumber">Line number in the file where of the caller.</param>
//        [Conditional("UMI3D_DEBUG")]
//        public void DebugAssert(
//            bool condition, string tag, object message = null,
//            UnityEngine.Object context = null, UMI3DLogColor color = UMI3DLogColor.Orange,
//            UMI3DLogReport report = null,
//            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
//            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
//            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
//        )
//        {
//            if (condition) return;

//            if (!GeneralLogType.HasFlag(UMI3DLogType.Assert) || !LogType.HasFlag(UMI3DLogType.Assert))
//            {
//                return;
//            }

//            if (report != null)
//            {
//                report.Enqueue(new UMI3DLog(
//                        UMI3DLogType.Assert,
//                        MainTag,
//                        tag,
//                        color,
//                        message,
//                        new UMI3DCallTrace(memberName, sourceFilePath, sourceLineNumber)
//                ));
//            }
//            else
//            {
//                LogHandler.LogFormat(UnityEngine.LogType.Assert, context ?? MainContext, message.FormatLog(UMI3DLogType.Assert, MainTag, tag, color, IsThreadDisplayed));
//            }
//        }

//        /// <summary>
//        /// A variant of Log that logs an Default message.
//        /// 
//        /// <para>
//        /// Default logs are no stack trace.
//        /// </para>
//        /// </summary>
//        /// <param name="tag">Used to identify the source of a log message. It usually identifies the method where the log call occurs.</param>
//        /// <param name="message">String or object to be converted to string representation for display.</param>
//        /// <param name="context">Object to which the message applies.</param>
//        /// <param name="color">The color of the log in the Unity console.</param>
//        /// <param name="report">A report to wich this log will be added.</param>
//        /// <param name="memberName">The name of the caller.</param>
//        /// <param name="sourceFilePath">File path of the caller.</param>
//        /// <param name="sourceLineNumber">Line number in the file where of the caller.</param>
//        public void Default(
//            string tag, object message,
//            UnityEngine.Object context = null, UMI3DLogColor color = UMI3DLogColor.White,
//            UMI3DLogReport report = null,
//            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
//            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
//            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
//        )
//        {
//            if (!GeneralLogType.HasFlag(UMI3DLogType.Default) || !LogType.HasFlag(UMI3DLogType.Default))
//            {
//                return;
//            }

//            UMI3DCallTrace trace = new UMI3DCallTrace(memberName, sourceFilePath, sourceLineNumber);

//            if (report != null)
//            {
//                report.Enqueue(new UMI3DLog(
//                        UMI3DLogType.Default,
//                        MainTag,
//                        tag,
//                        color,
//                        message,
//                        trace
//                ));
//            }
//            else
//            {
//                var stackTraceType = Application.GetStackTraceLogType(UnityEngine.LogType.Log);
//                Application.SetStackTraceLogType(UnityEngine.LogType.Log, StackTraceLogType.None);

//                LogHandler.LogFormat(UnityEngine.LogType.Log, context ?? MainContext, message.FormatLog(UMI3DLogType.Default, MainTag, tag, color, IsThreadDisplayed, trace: trace));

//                Application.SetStackTraceLogType(UnityEngine.LogType.Log, stackTraceType);
//            }
//        }

//        /// <summary>
//        /// A variant of Log that logs an Warning message.
//        /// 
//        /// <para>
//        /// Warning have no stack trace.
//        /// </para>
//        /// </summary>
//        /// <param name="tag">Used to identify the source of a log message. It usually identifies the method where the log call occurs.</param>
//        /// <param name="message">String or object to be converted to string representation for display.</param>
//        /// <param name="context">Object to which the message applies.</param>
//        /// <param name="color">The color of the log in the Unity console.</param>
//        /// <param name="report">A report to wich this log will be added.</param>
//        /// <param name="memberName">The name of the caller.</param>
//        /// <param name="sourceFilePath">File path of the caller.</param>
//        /// <param name="sourceLineNumber">Line number in the file where of the caller.</param>
//        public void Warning(
//            string tag, object message,
//            UnityEngine.Object context = null, UMI3DLogColor color = UMI3DLogColor.Yellow,
//            UMI3DLogReport report = null,
//            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
//            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
//            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
//        )
//        {
//            if (!GeneralLogType.HasFlag(UMI3DLogType.Warning) || !LogType.HasFlag(UMI3DLogType.Warning))
//            {
//                return;
//            }

//            UMI3DCallTrace trace = new UMI3DCallTrace(memberName, sourceFilePath, sourceLineNumber);

//            if (report != null)
//            {
//                report.Enqueue(new UMI3DLog(
//                        UMI3DLogType.Warning,
//                        MainTag,
//                        tag,
//                        color,
//                        message,
//                        trace
//                ));
//            }
//            else
//            {
//                var stackTraceType = Application.GetStackTraceLogType(UnityEngine.LogType.Warning);
//                Application.SetStackTraceLogType(UnityEngine.LogType.Warning, StackTraceLogType.None);

//                LogHandler.LogFormat(UnityEngine.LogType.Warning, context ?? MainContext, message.FormatLog(UMI3DLogType.Warning, MainTag, tag, color, IsThreadDisplayed, trace: trace));

//                Application.SetStackTraceLogType(UnityEngine.LogType.Warning, stackTraceType);
//            }
//        }

//        /// <summary>
//        /// A variant of Log that logs an Assertion message.
//        /// </summary>
//        /// <param name="tag">Used to identify the source of a log message. It usually identifies the method where the log call occurs.</param>
//        /// <param name="message">String or object to be converted to string representation for display.</param>
//        /// <param name="context">Object to which the message applies.</param>
//        /// <param name="color">The color of the log in the Unity console.</param>
//        /// <param name="report">A report to wich this log will be added.</param>
//        /// <param name="memberName">The name of the caller.</param>
//        /// <param name="sourceFilePath">File path of the caller.</param>
//        /// <param name="sourceLineNumber">Line number in the file where of the caller.</param>
//        public void Assertion(
//            string tag, object message,
//            UnityEngine.Object context = null, UMI3DLogColor color = UMI3DLogColor.Orange,
//            UMI3DLogReport report = null,
//            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
//            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
//            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
//        )
//        {
//            if (!GeneralLogType.HasFlag(UMI3DLogType.Assert) || !LogType.HasFlag(UMI3DLogType.Assert))
//            {
//                return;
//            }

//            if (report != null)
//            {
//                report.Enqueue(new UMI3DLog(
//                        UMI3DLogType.Assert,
//                        MainTag,
//                        tag,
//                        color,
//                        message,
//                        new UMI3DCallTrace(memberName, sourceFilePath, sourceLineNumber)
//                ));
//            }
//            else
//            {
//                var stackTraceType = Application.GetStackTraceLogType(UnityEngine.LogType.Assert);
//                Application.SetStackTraceLogType(UnityEngine.LogType.Assert, StackTraceLogType.ScriptOnly);

//                LogHandler.LogFormat(UnityEngine.LogType.Assert, context ?? MainContext, message.FormatLog(UMI3DLogType.Assert, MainTag, tag, color, IsThreadDisplayed));

//                Application.SetStackTraceLogType(UnityEngine.LogType.Assert, stackTraceType);
//            }
//        }

//        /// <summary>
//        /// A variant of Log that logs an Assertion message if <paramref name="condition"/> is false.
//        /// </summary>
//        /// <param name="condition">The condition has to be false to log the message.</param>
//        /// <param name="tag">Used to identify the source of a log message. It usually identifies the method where the log call occurs.</param>
//        /// <param name="message">String or object to be converted to string representation for display.</param>
//        /// <param name="context">Object to which the message applies.</param>
//        /// <param name="color">The color of the log in the Unity console.</param>
//        /// <param name="report">A report to wich this log will be added.</param>
//        /// <param name="memberName">The name of the caller.</param>
//        /// <param name="sourceFilePath">File path of the caller.</param>
//        /// <param name="sourceLineNumber">Line number in the file where of the caller.</param>
//        public void Assert(
//            bool condition, string tag, object message = null,
//            UnityEngine.Object context = null, UMI3DLogColor color = UMI3DLogColor.Orange,
//            UMI3DLogReport report = null,
//            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
//            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
//            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0
//        )
//        {
//            if (condition) return;

//            if (!GeneralLogType.HasFlag(UMI3DLogType.Assert) || !LogType.HasFlag(UMI3DLogType.Assert))
//            {
//                return;
//            }

//            if (report != null)
//            {
//                report.Enqueue(new UMI3DLog(
//                        UMI3DLogType.Assert,
//                        MainTag,
//                        tag,
//                        color,
//                        message,
//                        new UMI3DCallTrace(memberName, sourceFilePath, sourceLineNumber)
//                ));
//            }
//            else
//            {
//                var stackTraceType = Application.GetStackTraceLogType(UnityEngine.LogType.Assert);
//                Application.SetStackTraceLogType(UnityEngine.LogType.Assert, StackTraceLogType.ScriptOnly);

//                LogHandler.LogFormat(UnityEngine.LogType.Assert, context ?? MainContext, message.FormatLog(UMI3DLogType.Assert, MainTag, tag, color, IsThreadDisplayed));

//                Application.SetStackTraceLogType(UnityEngine.LogType.Assert, stackTraceType);
//            }
//        }

//        /// <summary>
//        /// A variant of Log that logs an Error message.
//        /// </summary>
//        /// <param name="tag">Used to identify the source of a log message. It usually identifies the method where the log call occurs.</param>
//        /// <param name="message">String or object to be converted to string representation for display.</param>
//        /// <param name="context">Object to which the message applies.</param>
//        /// <param name="color">The color of the log in the Unity console.</param>
//        public void Error(string tag, object message, UnityEngine.Object context = null, UMI3DLogColor color = UMI3DLogColor.Red)
//        {
//            if (!GeneralLogType.HasFlag(UMI3DLogType.Error) || !LogType.HasFlag(UMI3DLogType.Error))
//            {
//                return;
//            }

//            var stackTraceType = Application.GetStackTraceLogType(UnityEngine.LogType.Error);
//            Application.SetStackTraceLogType(UnityEngine.LogType.Error, StackTraceLogType.ScriptOnly);

//            LogHandler.LogFormat(UnityEngine.LogType.Error, context ?? MainContext, message.FormatLog(UMI3DLogType.Error, MainTag, tag, color, IsThreadDisplayed));

//            Application.SetStackTraceLogType(UnityEngine.LogType.Error, stackTraceType);
//        }

//        /// <summary>
//        /// A variant of Log that logs an Exception message.
//        /// </summary>
//        /// <param name="tag">Used to identify the source of a log message. It usually identifies the method where the log call occurs.</param>
//        /// <param name="exception">Runtime Exception.</param>
//        /// <param name="context">Object to which the message applies.</param>
//        /// <param name="color">The color of the log in the Unity console.</param>
//        public void Exception(string tag, Exception exception, UnityEngine.Object context = null, UMI3DLogColor color = UMI3DLogColor.Red)
//        {
//            if (!GeneralLogType.HasFlag(UMI3DLogType.Exception) || !LogType.HasFlag(UMI3DLogType.Exception))
//            {
//                return;
//            }

//            string message = "";

//            message += exception.Message;

//            if (exception.InnerException != null)
//            {
//                message += "\n\n";
//                message += "Inner ex: ";
//                message += exception.InnerException.Message;
//            }

//            var stackTraceType = Application.GetStackTraceLogType(UnityEngine.LogType.Exception);
//            Application.SetStackTraceLogType(UnityEngine.LogType.Exception, StackTraceLogType.ScriptOnly);

//            LogHandler.LogFormat(
//                UnityEngine.LogType.Exception,
//                context ?? MainContext,
//                message.FormatLog(UMI3DLogType.Exception, MainTag, tag, color, IsThreadDisplayed)
//            );

//            Application.SetStackTraceLogType(UnityEngine.LogType.Exception, stackTraceType);
//        }
//    }

//    /// <summary>
//    /// Extensions toolkit.
//    /// </summary>
//    public static class LogExtensions
//    {
//        /// <summary>
//        /// Default spacing between the elements of a log.
//        /// </summary>
//        public const string spacing = "     ";

//        /// <summary>
//        /// Get a string from an object.
//        /// </summary>
//        /// <param name="message"></param>
//        /// <returns></returns>
//        public static string GetString(this object message)
//        {
//            if (message == null)
//            {
//                return "Null";
//            }

//            IFormattable formattable = message as IFormattable;
//            if (formattable != null)
//            {
//                return formattable.ToString(null, CultureInfo.InvariantCulture);
//            }

//            return message.ToString();
//        }

//        /// <summary>
//        /// Format a message to be of the size of <paramref name="size"/> and aligned by <paramref name="alignment"/>.
//        /// </summary>
//        /// <param name="message"></param>
//        /// <param name="size"></param>
//        /// <param name="alignment"></param>
//        /// <returns></returns>
//        public static string FormatString(this object message, int size, UMI3DStringAlignment alignment = UMI3DStringAlignment.Left)
//        {
//            var s = message.GetString();

//            if (s.Length > size)
//            {
//                s = s.Substring(0, size - 3) + "...";
//            }

//            switch (alignment)
//            {
//                case UMI3DStringAlignment.Left:
//                    return String.Format($"{{0, -{size}}}", s);
//                case UMI3DStringAlignment.Center:
//                    return String.Format(
//                    $"{{0,-{size}}}",
//                    String.Format($"{{0, {(size + s.Length) / 2}}}", s)
//                    );
//                case UMI3DStringAlignment.Right:
//                    return String.Format($"{{0, {size}}}", s);
//                default:
//                    return s;
//            }
//        }

//        /// <summary>
//        /// Repeat <paramref name="c"/> <paramref name="repeatCount"/>.
//        /// </summary>
//        /// <param name="c"></param>
//        /// <param name="repeatCount"></param>
//        /// <returns></returns>
//        public static string Repeat(this char c, int repeatCount)
//        {
//            string result = "";
//            for (int i = 0; i < repeatCount; i++)
//            {
//                result += c;
//            }
//            return result;
//        }

//        /// <summary>
//        /// Add the log type after the message.
//        /// </summary>
//        /// <param name="message"></param>
//        /// <param name="logType"></param>
//        /// <returns></returns>
//        public static string AddLogType(this string message, UMI3DLogType logType)
//        {
//            string logTypeFormated = "";
//            switch (logType)
//            {
//                case UMI3DLogType.None:
//                    logTypeFormated += "";
//                    break;
//                case UMI3DLogType.DebugTodo:
//                    logTypeFormated += "TODO";
//                    break;
//                case UMI3DLogType.DebugHack:
//                    logTypeFormated += "HACK";
//                    break;
//                case UMI3DLogType.DebugTab:
//                    logTypeFormated += "TAB";
//                    break;
//                case UMI3DLogType.Debug:
//                    logTypeFormated += "DBUG";
//                    break;
//                case UMI3DLogType.Default:
//                    logTypeFormated += "DEF";
//                    break;
//                case UMI3DLogType.Warning:
//                    logTypeFormated += "W!";
//                    break;
//                case UMI3DLogType.Assert:
//                    logTypeFormated += "AT";
//                    break;
//                case UMI3DLogType.Error:
//                    logTypeFormated += "ER";
//                    break;
//                case UMI3DLogType.Exception:
//                    logTypeFormated += "EX";
//                    break;
//                default:
//                    logTypeFormated += logType;
//                    break;
//            }
//            message += logTypeFormated.FormatString(4);
//            message += $"{spacing}";

//            return message;
//        }

//        /// <summary>
//        /// Add the thread id after the message.
//        /// </summary>
//        /// <param name="message"></param>
//        /// <param name="IsThreadDisplayed"></param>
//        /// <returns></returns>
//        public static string AddThreadID(this string message, bool IsThreadDisplayed)
//        {
//#if !DEBUG
//            message += "On ThreadId: ";
//#endif

//            if (IsThreadDisplayed)
//            {
//                message += $"{Thread.CurrentThread.ManagedThreadId}".FormatString(4);
//                message += $"{spacing}";
//            }
//            else
//            {
//                message += $"".FormatString(4);
//                message += $"{spacing}";
//            }

//            return message;
//        }

//        /// <summary>
//        /// Add the tags of the log agter the message.
//        /// </summary>
//        /// <param name="message"></param>
//        /// <param name="MainTag"></param>
//        /// <param name="tag"></param>
//        /// <returns></returns>
//        public static string AddTags(this string message, string MainTag, string tag)
//        {
//            if (string.IsNullOrEmpty(MainTag) && string.IsNullOrEmpty(tag))
//            {
//                message += $"".FormatString(41);
//            }
//            else if (string.IsNullOrEmpty(MainTag) && !string.IsNullOrEmpty(tag))
//            {
//                message += $"{tag}".FormatString(41);
//            }
//            else if (!string.IsNullOrEmpty(MainTag) && string.IsNullOrEmpty(tag))
//            {
//                message += $"{MainTag}".FormatString(41);
//            }
//            else if (!string.IsNullOrEmpty(MainTag) && !string.IsNullOrEmpty(tag))
//            {
//                if (MainTag.Length > 20 && MainTag.Length + tag.Length > 40)
//                {
//                    message += $"{MainTag}".FormatString(20);
//                    message += "-";
//                    message += $"{tag}".FormatString(20);
//                }
//                else
//                {
//                    message += $"{MainTag}-{tag}".FormatString(41);
//                }
//            }

//            message += $"{spacing}";

//            return message;
//        }

//        /// <summary>
//        /// Add a color format string to highlight logs in the Unity console.
//        /// </summary>
//        /// <param name="message"></param>
//        /// <param name="color"></param>
//        /// <param name="setMessage"></param>
//        public static void AddColor(this string message, UMI3DLogColor color, Action<string> setMessage)
//        {
//            if (color == UMI3DLogColor.White)
//            {
//                return;
//            }

//            var partial = message.Split('\n');

//            for (int i = 0; i < partial.Length; i++)
//            {
//                partial[i] = $"<color={color.ToString().ToLower()}>" + partial[i] + "</color>";
//            }

//            setMessage(string.Join("\n", partial));
//        }

//        /// <summary>
//        /// Add the caller trace at the end of the message.
//        /// </summary>
//        /// <param name="message"></param>
//        /// <param name="trace"></param>
//        /// <returns></returns>
//        public static string AddTrace(this string message, UMI3DCallTrace trace)
//        {
//            if (trace != null)
//            {
//#if DEBUG
//                message += $"\n{trace.ClickableTrace}";
//#else
//                message += $"\n{spacing}{trace.Trace}";
//#endif
//            }

//            return message;
//        }

//        /// <summary>
//        /// Format a log to display.
//        /// </summary>
//        /// <param name="message"></param>
//        /// <param name="logType"></param>
//        /// <param name="MainTag"></param>
//        /// <param name="tag"></param>
//        /// <param name="color"></param>
//        /// <param name="IsThreadDisplayed"></param>
//        /// <param name="headerMessage"></param>
//        /// <param name="trace"></param>
//        /// <returns></returns>
//        public static string FormatLog(
//            this object message,
//            UMI3DLogType logType, string MainTag, string tag, UMI3DLogColor color, bool IsThreadDisplayed,
//            string headerMessage = null, UMI3DCallTrace trace = null
//        )
//        {
//            string result = "";

//#if DEBUG
//            result = result.AddLogType(logType);

//            result = result.AddThreadID(IsThreadDisplayed);

//            result = result.AddTags(MainTag, tag);

//            if (!string.IsNullOrEmpty(headerMessage))
//            {
//                result += $"{headerMessage} ";
//                result += $"{message.GetString()}";
//                result += $"{LogExtensions.spacing}";
//            }
//            else
//            {
//                result += message.GetString();
//                result += $"{LogExtensions.spacing}";
//            }

//            result.AddColor(color, _result =>
//            {
//                result = _result;
//            });

//            result = result.AddTrace(trace);
//#else
//            result += "\n";

//            result = result.AddLogType(logType);

//            if (!string.IsNullOrEmpty(headerMessage))
//            {
//                result += $"{headerMessage} ";
//                result += $"{message.GetString()}";
//                result += $"{LogExtensions.spacing}";
//            }
//            else
//            {
//                result += message.GetString();
//                result += $"{LogExtensions.spacing}";
//            }

//            result += $"\n";

//            result += $"{LogExtensions.spacing}";
//            result = result.AddTags(MainTag, tag);

//            result = result.AddThreadID(IsThreadDisplayed);

//            result = result.AddTrace(trace);
//#endif

//            result += "\n\n";

//            return result;
//        }
//    }
//}
