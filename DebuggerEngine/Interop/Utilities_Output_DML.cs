using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Remoting.Channels;
using System.Text;

namespace Microsoft.Mex.DotNetDbg
{
    public partial class DebugUtilities
    {
        public int OutputLineEx(string format, params object[] parameters)
        {
            format = EncodeTextForXml(format);
            var formattedString = parameters != null && parameters.Length != 0 ? string.Format(new DMLInterceptProvider(this), format, parameters) : format;
            return OutputDMLPreformattedLine(formattedString);
        }

        public int OutputVerboseLineEx(string format, params object[] parameters)
        {
            format = EncodeTextForXml(format);
            var formattedString = parameters != null && parameters.Length != 0 ? string.Format(new DMLInterceptProvider(this), format, parameters) : format;
            return OutputVerboseDMLPreformattedLine(formattedString);
        }

        public int OutputEx(string format, params object[] parameters)
        {
            format = EncodeTextForXml(format);
            var formattedString = parameters != null && parameters.Length != 0 ? string.Format(new DMLInterceptProvider(this), format, parameters) : format;
            return OutputDMLPreformatted(formattedString);
        }

        public interface IDMLAble
        {
            DML ToDML();
        }

        public class DMLInterceptProvider : IFormatProvider, ICustomFormatter
        {
            private readonly DebugUtilities _d;

            public DMLInterceptProvider() {}

            public DMLInterceptProvider(DebugUtilities d)
            {
                _d = d;
            }

            public string Format(string format, object obj, IFormatProvider provider)
            {
                if (format != null)
                {
                    if ((format.StartsWith("p") || format.StartsWith("q")) && _d != null)
                    {
                        string output;
                        if (DecodePointerArguments(_d, format, obj, out output))
                        {
                            return output;
                        }
                    }
                }

                if (obj is IDMLAble)
                {
                    if (format == "G")
                    {
                        return ((IDMLAble)obj).ToDML().ToString();
                    }
                    return ((IDMLAble)obj).ToDML().ToString(format, CultureInfo.InvariantCulture);
                }

                // Use default for all other formatting. 
                if (obj is IFormattable)
                {
                    return ((IFormattable)obj).ToString(format, CultureInfo.InvariantCulture);
                }
                if (obj != null)
                {
                    return obj.ToString().ToDML().ToString(format, CultureInfo.InvariantCulture);
                }
                return format;
            }

            public object GetFormat(Type formatType)
            {
                if (formatType == typeof(ICustomFormatter))
                {
                    return this;
                }
                return null;
            }


            private static bool DecodePointerArguments(DebugUtilities d, string format, object param, out string outputString)
            {
                // fast exit code
                outputString = null;
                if (format[0] != 'p' && format[0] != 'P' && format[0] != 'q' && format[0] != 'Q')
                {
                    return false;
                }

                switch (format[0])
                {
                    case 'p':
                    {
                        if (param == null)
                        {
                            outputString = "(null)";
                            return true;
                        }
                        try
                        {
                            outputString = d.P2S((ulong)param);
                            break;
                        }
                        catch
                        {
                            if (format.Length == 1)
                            {
                                throw new Exception("Could not convert " + param + " to UInt64 for {x:p}");
                            }
                            return false;
                        }
                    }
                    case 'P':
                    {
                        if (param == null)
                        {
                            outputString = "(NULL)";
                            return true;
                        }
                        try
                        {
                            outputString = d.P2SUC((ulong)param);
                            break;
                        }
                        catch
                        {
                            if (format.Length == 1)
                            {
                                throw new Exception("Could not convert " + param + " to UInt64 for {x:P}");
                            }
                            return false;
                        }
                    }
                    case 'q':
                    {
                        if (param == null)
                        {
                            outputString = "(null)";
                            return true;
                        }
                        try
                        {
                            outputString = d.P2S((ulong)param, true);
                            break;
                        }
                        catch
                        {
                            if (format.Length == 1)
                            {
                                throw new Exception("Could not convert " + param + " to UInt64 for {x:q}");
                            }
                            return false;
                        }
                    }
                    case 'Q':
                    {
                        if (param == null)
                        {
                            outputString = "(NULL)";
                            return true;
                        }
                        try
                        {
                            outputString = d.P2SUC((ulong)param, true);
                            break;
                        }
                        catch
                        {
                            if (format.Length == 1)
                            {
                                throw new Exception("Could not convert " + param + " to UInt64 for {x:Q}");
                            }
                            return false;
                        }
                    }
                }

                if (format.Length == 1)
                {
                    return true;
                }

                if (outputString == null)
                {
                    return false;
                }

                var sb = new StringBuilder(16);

                bool leftAlign;
                int desiredWidth;
                if (!int.TryParse(format.Substring(1), out desiredWidth))
                {
                    outputString = null;
                    return false;
                }

                if (desiredWidth < 0)
                {
                    desiredWidth = -desiredWidth;
                    leftAlign = true;
                }
                else
                {
                    leftAlign = false;
                }

                var spacesToAdd = desiredWidth - outputString.Length;
                if (spacesToAdd > 0)
                {
                    if (leftAlign)
                    {
                        sb.Append(outputString);
                        sb.Append(' ', spacesToAdd);
                    }
                    else
                    {
                        sb.Append(' ', spacesToAdd);
                        sb.Append(outputString);
                    }
                }
                else
                {
                    sb.Append(outputString);
                }


                outputString = sb.ToString();

                return true;
            }
        }

        public class DML : IFormattable, IDMLAble
        {
            public enum Colors
            {
                None,
                DefaultWindow,
                CurrentLine,
                Changed,
                Emphasized,
                Subdued,
                Normal,
                Warning,
                Error,
                Verbose,
                BreakPoint,
                Prompt,
                PromptRegisters,
                EnabledBreakPoint,
                DisabledBreakPoint,
                SecondaryLine,
                UserSelectedLine,
                ExtensionWarning,
                DebuggeeLevel,
                DebuggeePrompt,
                SymbolMessage,
                InternalEvent,
                InternalBreakpoint,
                InternalRemoting,
                InternalKD,
                UserAdded,
                SourceNumber,
                SourceChar,
                SourceString,
                SourceIdentifier,
                SourceKeyword,
                SourceBraceorPair,
                SourceComment,
                SourceDirective,
                SourceSpecialIdentifier,
                SourceAnnotation
            }

            private readonly Dictionary<string, string> _contextMenu = new Dictionary<string, string>();

            private string _altTextIfNoDML;
            private DebugUtilities _utilities = null;

            public DML(string text, string command)
            {
                Text = text;
                Command = command;
                DMLCapable = DebuggerDMLCapable();
                DMLPreferred = PreferDMLSet();
            }

            public DML(string text, bool textIsPreFormattedDML = false)
            {
                if (textIsPreFormattedDML)
                {
                    PreFormattedDMLInsteadOfText = text;
                }
                else

                {
                    Text = text;
                }

                DMLCapable = DebuggerDMLCapable();
                DMLPreferred = PreferDMLSet();
            }

            public DML(string text, string command, params object[] paramObjects)
            {
                Text = FormatString(text, paramObjects);
                Command = FormatString(command, paramObjects);
                DMLCapable = DebuggerDMLCapable();
                DMLPreferred = PreferDMLSet();
            }

            public DML(DebugUtilities d, string text, string command, params object[] paramObjects)
            {

                _utilities = d;
                Text = FormatString(text, paramObjects);
                Command = FormatString(command, paramObjects);
                DMLCapable = DebuggerDMLCapable();
                DMLPreferred = PreferDMLSet();
            }

            public DML()
            {
                DMLCapable = DebuggerDMLCapable();
                DMLPreferred = PreferDMLSet();
            }

            private string FormatString(string text, params object[] param)
            {
                if (text == null)
                {
                    return string.Empty;
                }

                if (param == null)
                {
                    return text;
                }

                if (_utilities == null)
                {
                    return string.Format(text, param);
                }
                return _utilities.FormatStringEx(text, param);
            }

            public bool DMLCapable {get;}
            public bool DMLPreferred {get;}

            public string ToolTipText {get; set;} = string.Empty;

            public string Text {get; set;}

            public string PreFormattedDMLInsteadOfText { get; set; }

            public string Command {get; set;}

            public string AltTextIfNoDML
            {
                get
                {
                    return EncodeTextForXml(string.IsNullOrEmpty(_altTextIfNoDML) ? Text : _altTextIfNoDML);
                }
                set
                {
                    _altTextIfNoDML = value;
                }
            }

            public Colors Color {get; set;} = Colors.None;

            public bool IsOutputDML
            {
                get
                {
                    if (!DMLCapable)
                    {
                        return false;
                    }
                    return !HonorPreferDML || DMLPreferred != false;
                }
            }

            public bool NoHistory {get; set;}
            public bool Bold {get; set;}
            public bool Underline {get; set;}
            public bool Italic {get; set;}

            public bool HonorPreferDML {get; set;}

            public bool DisableLinkAsTextIfNoDML {get; set;}

            public DML ToDML()
            {
                return this;
            }


            public string ToString(string format, IFormatProvider provider)
            {
                bool bold = false, italic = false, underline = false, normal = false;
                var color = Colors.None;
                if (!string.IsNullOrEmpty(format))
                {
                    format = format.ToLowerInvariant();

                    var formats = format.Split('.');

                    foreach (var form in formats)
                    {
                        if (form == "i" || form == "italic")
                        {
                            italic = true;
                        }
                        if (form == "b" || form == "bold")
                        {
                            bold = true;
                        }
                        if (form == "u" || form == "underline")
                        {
                            underline = true;
                        }
                        if (form == "n" || form == "normal")
                        {
                            normal = true;
                        }
                        foreach (var c in Enum.GetNames(typeof(Colors)))
                        {
                            if (c.ToLowerInvariant() == form)
                            {
                                if (!Enum.TryParse(c, true, out color))
                                {
                                    color = Colors.None;
                                }
                            }
                        }
                    }
                }
                return ToString(bold, italic, underline, color, normal);
            }

            public void ClearContextMenu()
            {
                _contextMenu.Clear();
            }

            public void AddContextMenuItem(string text, string command)
            {
                _contextMenu.Add(text, command);
            }

            public void AddContextMenuItem(string text, string command, params object[] paramObjects)
            {
                _contextMenu.Add(FormatString(text, paramObjects), FormatString(command, paramObjects));
            }

            public string ToPreformatedDML()
            {
                return ToString(false, false, false, Colors.None, false);
            }

            public override string ToString()
            {
                if (!string.IsNullOrEmpty(PreFormattedDMLInsteadOfText))
                {
                   return DebugUtilities.DmlToText(PreFormattedDMLInsteadOfText);
                }
                return Text;
            }

            public string ToString(bool bold, bool italic, bool underline, Colors color, bool normal)
            {
                string outputData;
                if (!IsOutputDML)
                {
                    outputData = AltTextIfNoDML;
                    if (!DisableLinkAsTextIfNoDML && !string.IsNullOrEmpty(Command))
                    {
                        if (outputData != Command)
                        {
                            outputData = outputData + "[" +Command + "]";
                        }
                    }
                    return outputData;
                }

                if (!string.IsNullOrEmpty(PreFormattedDMLInsteadOfText))
                {
                    outputData = PreFormattedDMLInsteadOfText;
                }
                else
                {
                    outputData = EncodeTextForXml(Text);
                }

                if ((Bold || bold) && !normal)
                {
                    outputData = "<b>" + outputData + "</b>";
                }

                if ((Italic || italic) && !normal)
                {
                    outputData = "<i>" + outputData + "</i>";
                }

                if (Underline || underline && !normal || !string.IsNullOrEmpty(Command) && (Color != Colors.None || color != Colors.None && !normal))
                {
                    outputData = "<u>" + outputData + "</u>";
                }

                if (color != Colors.None)
                {
                    outputData = GetColorString(color, outputData);
                }
                else
                {
                    outputData = GetColorString(Color, outputData);
                }

                if (!string.IsNullOrEmpty(Command) || !string.IsNullOrEmpty(ToolTipText))
                {
                  

                    var cmdLine = string.Empty;
                    if (!string.IsNullOrEmpty(ToolTipText))
                    {
                        cmdLine = "alt=\"" + EncodeTextForXml(ToolTipText) + "\"";
                    }

                    if (!string.IsNullOrEmpty(Command))
                    {
                        if (cmdLine != string.Empty)
                        {
                            cmdLine += " ";
                        }
                        cmdLine += "cmd=\"" + EncodeTextForXml(Command) + "\"";
                    }

                    if (NoHistory)
                    {
                        outputData = string.Format(CultureInfo.InvariantCulture, "<exec {0}>{1}", cmdLine, outputData);
                    }
                    else
                    {
                        outputData = string.Format(CultureInfo.InvariantCulture, "<link {0}>{1}", cmdLine, outputData);
                    }

                    foreach (var menuItem in _contextMenu)
                    {
                        outputData = outputData + $"<altlink name=\"{EncodeTextForXml(menuItem.Key)}\" cmd=\"{EncodeTextForXml(menuItem.Value)}\">";
                    }

                    if (NoHistory)
                    {
                        outputData = outputData + "</exec>";
                    }
                    else
                    {
                        outputData = outputData + "</link>";
                    }
                }

                return outputData;
            }

            public static DML GetDML(string text, string command)
            {
                return new DML {Text = text, Command = command};
            }

            public static DML GetDML(string text, string command, Colors color)
            {
                return new DML {Text = text, Command = command, Color = color};
            }

            public static DML GetDML(string text)
            {
                return new DML {Text = text};
            }

            public static DML GetDML(string text, Colors color)
            {
                return new DML {Text = text, Color = color};
            }

            public static DML GetDML(string format, string command, params object[] parameters)
            {
                var formattedString = parameters != null && parameters.Length != 0 ? string.Format(CultureInfo.InvariantCulture, format, parameters) : format;
                var formattedStringCommand = parameters != null && parameters.Length != 0 ? string.Format(CultureInfo.InvariantCulture, command, parameters) : command;
                return GetDML(formattedString, formattedStringCommand);
            }

            public DML SetColor(Colors color)
            {
                Color = color;
                return this;
            }

            public DML SetCommand(string command)
            {
                Command = command;
                return this;
            }

            private string GetColorString(Colors color, string text)
            {
                switch (color)
                {
                    case Colors.None:
                        return text;
                    case Colors.DefaultWindow:
                        return "<col fg=\"wfg\" bg=\"wbg\">" + text + "</col>";
                    case Colors.CurrentLine:
                        return "<col fg=\"clfg\" bg=\"clbg\">" + text + "</col>";
                    case Colors.Changed:
                        return "<col fg=\"changed\">" + text + "</col>";
                    case Colors.Emphasized:
                        return "<col fg=\"emphfg\" bg=\"empbg\">" + text + "</col>";
                    case Colors.Subdued:
                        return "<col fg=\"subfg\" bg=\"subbg\">" + text + "</col>";
                    case Colors.Normal:
                        return "<col fg=\"normfg\" bg=\"normbg\">" + text + "</col>";
                    case Colors.Warning:
                        return "<col fg=\"warnfg\" bg=\"warnbg\">" + text + "</col>";
                    case Colors.Error:
                        return "<col fg=\"errfg\" bg=\"errbg\">" + text + "</col>";
                    case Colors.Verbose:
                        return "<col fg=\"verbfg\" bg=\"verbbg\">" + text + "</col>";
                    case Colors.SourceNumber:
                        return "<col fg=\"srcnum\">" + text + "</col>";
                    case Colors.SourceChar:
                        return "<col fg=\"srcchar\">" + text + "</col>";
                    case Colors.SourceString:
                        return "<col fg=\"srcstr\">" + text + "</col>";
                    case Colors.SourceIdentifier:
                        return "<col fg=\"srcid\">" + text + "</col>";
                    case Colors.SourceKeyword:
                        return "<col fg=\"srckw\">" + text + "</col>";
                    case Colors.SourceBraceorPair:
                        return "<col fg=\"srcpair\">" + text + "</col>";
                    case Colors.SourceComment:
                        return "<col fg=\"srccmnt\">" + text + "</col>";
                    case Colors.SourceDirective:
                        return "<col fg=\"srcdrct\">" + text + "</col>";
                    case Colors.SourceSpecialIdentifier:
                        return "<col fg=\"srcspid\">" + text + "</col>";
                    case Colors.SourceAnnotation:
                        return "<col fg=\"srcannot\">" + text + "</col>";
                    case Colors.BreakPoint:
                        return "<col fg=\"bpfg\" bg=\"bpbg\">" + text + "</col>";
                    case Colors.EnabledBreakPoint:
                        return "<col fg=\"ebpfg\" bg=\"ebpbg\">" + text + "</col>";
                    case Colors.DisabledBreakPoint:
                        return "<col fg=\"dbpfg\" bg=\"dbpbg\">" + text + "</col>";
                    case Colors.SecondaryLine:
                        return "<col fg=\"slfg\" bg=\"slbg\">" + text + "</col>";
                    case Colors.UserSelectedLine:
                        return "<col fg=\"uslfg\" bg=\"uslbg\">" + text + "</col>";
                    case Colors.Prompt:
                        return "<col fg=\"promptfg\" bg=\"promptbg\">" + text + "</col>";
                    case Colors.PromptRegisters:
                        return "<col fg=\"promptregfg\" bg=\"promptregbg\">" + text + "</col>";
                    case Colors.ExtensionWarning:
                        return "<col fg=\"extfg\" bg=\"extbg\">" + text + "</col>";
                    case Colors.DebuggeeLevel:
                        return "<col fg=\"dbgfg\" bg=\"dbgbg\">" + text + "</col>";
                    case Colors.DebuggeePrompt:
                        return "<col fg=\"dbgpfg\" bg=\"dbgpbg\">" + text + "</col>";
                    case Colors.SymbolMessage:
                        return "<col fg=\"symfg\" bg=\"symbg\">" + text + "</col>";
                    case Colors.InternalEvent:
                        return "<col fg=\"ievtfg\" bg=\"ievtbg\">" + text + "</col>";
                    case Colors.InternalBreakpoint:
                        return "<col fg=\"ibpfg\" bg=\"ibpbg\">" + text + "</col>";
                    case Colors.InternalRemoting:
                        return "<col fg=\"iremfg\" bg=\"irembg\">" + text + "</col>";
                    case Colors.InternalKD:
                        return "<col fg=\"ikdfg\" bg=\"ikdbg\">" + text + "</col>";
                    case Colors.UserAdded:
                        return "<col fg=\"uwfg\" bg=\"uwbg\">" + text + "</col>";
                    default:
                        throw new ArgumentOutOfRangeException(nameof(color), color, null);
                }
            }
        }
    }

    public static class DMLExtensions
    {
        public static DebugUtilities.DML ToDML(this string str)
        {
            return new DebugUtilities.DML {Text = str};
        }

        public static DebugUtilities.DML ToDML(this string str, bool textIsPreFormattedDML)
        {
            return new DebugUtilities.DML(str, textIsPreFormattedDML);
        }

        public static DebugUtilities.DML ToDML(this string str, string command, params object[] paramObjects)
        {
            return new DebugUtilities.DML {Text = str, Command = string.Format(command, paramObjects)};
        }

        public static DebugUtilities.DML ToDML(this string str, DebugUtilities.DML.Colors color)
        {
            return new DebugUtilities.DML {Text = str, Color = color};
        }

        public static DebugUtilities.DML ToDML(this object str)
        {
            return new DebugUtilities.DML {Text = str.ToString()};
        }

        public static DebugUtilities.DML ToDML(this object str, DebugUtilities.DML.Colors color)
        {
            return new DebugUtilities.DML {Text = str.ToString(), Color = color};
        }

        public static DebugUtilities.DML ToDML(this object str, string command, params object[] paramObjects)
        {
            return new DebugUtilities.DML {Text = str.ToString(), Command = string.Format(command, paramObjects)};
        }

        public static DebugUtilities.DML ToDML(this string str, string command, DebugUtilities.DML.Colors color)
        {
            return new DebugUtilities.DML {Text = str, Command = command, Color = color};
        }

        public static DebugUtilities.DML ToDML(this DbgStringBuilder sb, DebugUtilities.DML.Colors color)
        {
            return new DebugUtilities.DML {Text = sb.ToString(), Color = color};
        }

        public static DebugUtilities.DML ToDML(this DbgStringBuilder sb, string command, DebugUtilities.DML.Colors color)
        {
            return new DebugUtilities.DML {Text = sb.ToString(), Command = command, Color = color};
        }
    }
}