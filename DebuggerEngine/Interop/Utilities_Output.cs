using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Mex.Framework;

namespace Microsoft.Mex.DotNetDbg
{
    public unsafe partial class DebugUtilities
    {
        private static readonly Regex PointerArgumentRegex = new MexRegex(@"{(\d+):([pPqQ])(,-?\d+)?}", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
        private DEBUG_OUTPUT _currentMask = (DEBUG_OUTPUT)0x3F7;
        public DEBUG_OUTCTL _DML = DEBUG_OUTCTL.DML;
        public DEBUG_OUTCTL _NODML = 0;
        private DEBUG_OUTPUT _savedMask = (DEBUG_OUTPUT)0x3F7;
        private int _threadIDofSavedMask = 0;
        public void OutputMaskSave()
        {
            DEBUG_OUTPUT maskToSave;
            if (SUCCEEDED(DebugClient.GetOutputMask(out maskToSave)))
            {
                _threadIDofSavedMask = Thread.CurrentThread.ManagedThreadId;

                if (maskToSave != DEBUG_OUTPUT.NONE)
                {
                    _savedMask = maskToSave;
                    _currentMask = maskToSave;
                }
            }
        }

        public void OutputMaskRestore()
        {
            if (_threadIDofSavedMask == Thread.CurrentThread.ManagedThreadId || _threadIDofSavedMask == 0)
            {
                if (SUCCEEDED(DebugClient.SetOutputMask(_savedMask)))
                {
                    _currentMask = _savedMask;
                }
            }
        }
        public void OutputMaskDisableAll()
        {
            //if (SUCCEEDED(DebugClient.SetOutputMask(0)))
            //{
            //    _currentMask = 0;
            //}
            if (SUCCEEDED(DebugClient.SetOutputMask(DEBUG_OUTPUT.NONE)))
            {
                _currentMask = DEBUG_OUTPUT.NONE;
            }
        }

        public void OutputMaskDisable(DEBUG_OUTPUT Disable)
        {
            DEBUG_OUTPUT currentMask;
            if (SUCCEEDED(DebugClient.GetOutputMask(out currentMask)))
            {
                if (SUCCEEDED(DebugClient.SetOutputMask(currentMask & ~Disable)))
                {
                    _currentMask = currentMask & ~Disable;
                }
            }
        }

        private int OutputHelper(string formattedString, DEBUG_OUTPUT outputType)
        {
            //formattedString = EscapePercents(formattedString);
            return ControlledOutputWide(_outCtl | _NODML, outputType, formattedString);
        }

        private int OutputLineHelper(string formattedString, DEBUG_OUTPUT outputType)
        {
            //formattedString = EscapePercents(formattedString);
            //int hr = ControlledOutputWide(_outCtl, outputType, formattedString);
            //return FAILED(hr) ? hr : ControlledOutputWide(_outCtl, outputType, "\n");
            return ControlledOutputWide(_outCtl | _NODML, outputType, formattedString + '\n');
        }

        /// <summary>
        ///     Outputs a string that contains one or more DML block
        /// </summary>
        /// <param name="formattedDmlString">String containing DML</param>
        /// <returns>HRESULT of the IDebugControl4::ControlledOutputWide or IDebugControl::ControlledOutput call</returns>
        public int OutputDMLPreformatted(string formattedDmlString)
        {
            //formattedDmlString = EscapePercents(formattedDmlString);
            return ControlledOutputWide(_outCtl | _DML, DEBUG_OUTPUT.NORMAL, formattedDmlString);
        }

        /// <summary>
        ///     Outputs a string that contains one or more DML block
        /// </summary>
        /// <param name="formattedDmlString">String containing DML</param>
        /// <returns>HRESULT of the IDebugControl4::ControlledOutputWide or IDebugControl::ControlledOutput call</returns>
        public int OutputErrorDMLPreformatted(string formattedDmlString)
        {
            //formattedDmlString = EscapePercents(formattedDmlString);
            return ControlledOutputWide(_outCtl | _DML, DEBUG_OUTPUT.ERROR, formattedDmlString);
        }

        /// <summary>
        ///     Outputs a string that contains one or more DML block
        /// </summary>
        /// <param name="formattedDmlString">String containing DML</param>
        /// <returns>HRESULT of the IDebugControl4::ControlledOutputWide or IDebugControl::ControlledOutput call</returns>
        public int OutputWarningDMLPreformatted(string formattedDmlString)
        {
            //formattedDmlString = EscapePercents(formattedDmlString);
            return ControlledOutputWide(_outCtl | _DML, DEBUG_OUTPUT.WARNING, formattedDmlString);
        }

        /// <summary>
        ///     Outputs a string that contains one or more DML block
        /// </summary>
        /// <param name="formattedDmlString">String containing DML</param>
        /// <returns>HRESULT of the IDebugControl4::ControlledOutputWide or IDebugControl::ControlledOutput call</returns>
        public int OutputVerboseDMLPreformatted(string formattedDmlString)
        {
            //formattedDmlString = EscapePercents(formattedDmlString);
            return ControlledOutputWide(_outCtl | _DML, DEBUG_OUTPUT.VERBOSE, formattedDmlString);
        }

        /// <summary>
        ///     Outputs a string that contains one or more DML block
        /// </summary>
        /// <param name="formattedDmlString">String containing DML</param>
        /// <returns>HRESULT of the IDebugControl4::ControlledOutputWide or IDebugControl::ControlledOutput call</returns>
        public int OutputDMLPreformattedLine(string formattedDmlString)
        {
            //formattedDmlString = EscapePercents(formattedDmlString);
            //int hr = ControlledOutputWide(_outCtl | DEBUG_OUTCTL.DML, DEBUG_OUTPUT.NORMAL, formattedDmlString);
            //return FAILED(hr) ? hr : ControlledOutputWide(_outCtl, DEBUG_OUTPUT.NORMAL, "\n");
            return ControlledOutputWide(_outCtl | _DML, DEBUG_OUTPUT.NORMAL, formattedDmlString + '\n');
        }

        /// <summary>
        ///     Outputs a string that contains one or more DML block
        /// </summary>
        /// <param name="formattedDmlString">String containing DML</param>
        /// <returns>HRESULT of the IDebugControl4::ControlledOutputWide or IDebugControl::ControlledOutput call</returns>
        public int OutputErrorDMLPreformattedLine(string formattedDmlString)
        {
            //formattedDmlString = EscapePercents(formattedDmlString);
            //int hr = ControlledOutputWide(_outCtl | DEBUG_OUTCTL.DML, DEBUG_OUTPUT.ERROR, formattedDmlString);
            //return FAILED(hr) ? hr : ControlledOutputWide(_outCtl, DEBUG_OUTPUT.ERROR, "\n");
            return ControlledOutputWide(_outCtl | _DML, DEBUG_OUTPUT.ERROR, formattedDmlString + '\n');
        }

        /// <summary>
        ///     Outputs a string that contains one or more DML block
        /// </summary>
        /// <param name="formattedDmlString">String containing DML</param>
        /// <returns>HRESULT of the IDebugControl4::ControlledOutputWide or IDebugControl::ControlledOutput call</returns>
        public int OutputVerboseDMLPreformattedLine(string formattedDmlString)
        {
            //formattedDmlString = EscapePercents(formattedDmlString);
            //int hr = ControlledOutputWide(_outCtl | DEBUG_OUTCTL.DML, DEBUG_OUTPUT.VERBOSE, formattedDmlString);
            //return FAILED(hr) ? hr : ControlledOutputWide(_outCtl, DEBUG_OUTPUT.VERBOSE, "\n");
            return ControlledOutputWide(_outCtl | _DML, DEBUG_OUTPUT.VERBOSE, formattedDmlString + '\n');
        }

        /// <summary>
        ///     Outputs a string that contains one or more DML block
        /// </summary>
        /// <param name="formattedDmlString">String containing DML</param>
        /// <returns>HRESULT of the IDebugControl4::ControlledOutputWide or IDebugControl::ControlledOutput call</returns>
        public int OutputWarningDMLPreformattedLine(string formattedDmlString)
        {
            //formattedDmlString = EscapePercents(formattedDmlString);
            //int hr = ControlledOutputWide(_outCtl | DEBUG_OUTCTL.DML, DEBUG_OUTPUT.WARNING, formattedDmlString);
            //return FAILED(hr) ? hr : ControlledOutputWide(_outCtl, DEBUG_OUTPUT.WARNING, "\n");
            return ControlledOutputWide(_outCtl | _DML, DEBUG_OUTPUT.WARNING, formattedDmlString + '\n');
        }

        /// <summary>
        ///     Builds a formatted string and outputs the result as normal text
        /// </summary>
        /// <param name="format">Format string using C# string formatting syntax</param>
        /// <param name="parameters">Optional parameters for the string format</param>
        /// <returns>HRESULT of the IDebugControl4::OutputWide or IDebugControl::Output call</returns>
        public int Output(string format, params object[] parameters)
        {
            ScanForPointerArguments(format, parameters, out format);
            var formattedString = ((parameters != null) && (parameters.Length != 0)) ? string.Format(CultureInfo.InvariantCulture, format, parameters) : format;
            return OutputHelper(formattedString, DEBUG_OUTPUT.NORMAL);
        }

        /// <summary>
        ///     Builds a formatted string and outputs the result as normal text
        /// </summary>
        /// <param name="format">Format string using C# string formatting syntax</param>
        /// <param name="parameters">Optional parameters for the string format</param>
        /// <returns>HRESULT of the IDebugControl4::OutputWide or IDebugControl::Output call</returns>
        public int OutputLine(string format, params object[] parameters)
        {
            ScanForPointerArguments(format, parameters, out format);
            var formattedString = ((parameters != null) && (parameters.Length != 0)) ? string.Format(CultureInfo.InvariantCulture, format, parameters) : format;
            return OutputLineHelper(formattedString, DEBUG_OUTPUT.NORMAL);
        }

        /// <summary>
        ///     Output a CR/LF pair
        /// </summary>
        /// <returns>HRESULT of the IDebugControl4::OutputWide or IDebugControl::Output call</returns>
        public int OutputLine()
        {
            return OutputHelper("\n", DEBUG_OUTPUT.NORMAL);
        }

        public int OutputTabs(ulong tabLevel, bool bBar = false)
        {
            if (tabLevel == 0) return S_OK;
            var sb = new StringBuilder();

            for (ulong i = 0; i < tabLevel; i++)
            {
                if (ShouldBreak())
                {
                    OutputLine("OutputTabs cancelled");
                    break;
                }

                sb.Append(!bBar ? "   " : "|  ");
            }

            return OutputHelper(sb.ToString(), DEBUG_OUTPUT.NORMAL);
        }

        /// <summary>
        ///     Builds a formatted string and outputs the result as verbose text
        /// </summary>
        /// <param name="format">Format string using C# string formatting syntax</param>
        /// <param name="parameters">Optional parameters for the string format</param>
        /// <returns>HRESULT of the IDebugControl4::OutputWide or IDebugControl::Output call</returns>
        public int OutputVerbose(string format, params object[] parameters)
        {
            ScanForPointerArguments(format, parameters, out format);
            var formattedString = ((parameters != null) && (parameters.Length != 0)) ? string.Format(CultureInfo.InvariantCulture, format, parameters) : format;
            if (!string.IsNullOrEmpty(formattedString))
            {
#if INTERNAL
                SetStatusBar("Mex: " + formattedString);
#endif
            }
            return OutputHelper(formattedString, DEBUG_OUTPUT.VERBOSE);
        }

        /// <summary>
        ///     Builds a formatted string and outputs the result as verbose text
        /// </summary>
        /// <param name="format">Format string using C# string formatting syntax</param>
        /// <param name="parameters">Optional parameters for the string format</param>
        /// <returns>HRESULT of the IDebugControl4::OutputWide or IDebugControl::Output call</returns>
        public int OutputVerboseLine(string format, params object[] parameters)
        {
            ScanForPointerArguments(format, parameters, out format);
            var formattedString = ((parameters != null) && (parameters.Length != 0)) ? string.Format(CultureInfo.InvariantCulture, format, parameters) : format;

#if INTERNAL
            SetStatusBar("Mex: " + formattedString);
#endif

            return OutputLineHelper(formattedString, DEBUG_OUTPUT.VERBOSE);
        }

        /// <summary>
        ///     Builds a formatted string and outputs the result as verbose text, only called in debug builds
        /// </summary>
        /// <param name="format">Format string using C# string formatting syntax</param>
        /// <param name="parameters">Optional parameters for the string format</param>
        /// <returns>HRESULT of the IDebugControl4::OutputWide or IDebugControl::Output call</returns>
        [Conditional("DEBUG")]
        public void OutputDebugLine(string format, params object[] parameters)
        {
#if DEBUG

            ScanForPointerArguments(format, parameters, out format);
            var formattedString = ((parameters != null) && (parameters.Length != 0)) ? string.Format(CultureInfo.InvariantCulture, format, parameters) : format;

            OutputLineHelper("[DEBUG] " + formattedString, DEBUG_OUTPUT.VERBOSE);
#endif
        }

        /// <summary>
        ///     Builds a formatted string and outputs the result as verbose text, only called in debug builds
        /// </summary>
        /// <param name="format">Format string using C# string formatting syntax</param>
        /// <param name="parameters">Optional parameters for the string format</param>
        /// <returns>HRESULT of the IDebugControl4::OutputWide or IDebugControl::Output call</returns>
        [Conditional("DEBUG")]
        public void OutputDebug(string format, params object[] parameters)
        {
#if DEBUG

            ScanForPointerArguments(format, parameters, out format);
            var formattedString = ((parameters != null) && (parameters.Length != 0)) ? string.Format(CultureInfo.InvariantCulture, format, parameters) : format;

            OutputHelper(formattedString, DEBUG_OUTPUT.VERBOSE);
#endif
        }

        /// <summary>
        ///     Output a CR/LF pair
        /// </summary>
        /// <returns>HRESULT of the IDebugControl4::OutputWide or IDebugControl::Output call</returns>
        public int OutputVerboseLine()
        {
            return OutputHelper("\n", DEBUG_OUTPUT.VERBOSE);
        }

        /// <summary>
        ///     Builds a formatted string and outputs the result as warning text
        /// </summary>
        /// <param name="format">Format string using C# string formatting syntax</param>
        /// <param name="parameters">Optional parameters for the string format</param>
        /// <returns>HRESULT of the IDebugControl4::OutputWide or IDebugControl::Output call</returns>
        public int OutputWarning(string format, params object[] parameters)
        {
            ScanForPointerArguments(format, parameters, out format);
            var formattedString = ((parameters != null) && (parameters.Length != 0)) ? string.Format(CultureInfo.InvariantCulture, format, parameters) : format;
            return OutputHelper(formattedString, DEBUG_OUTPUT.WARNING);
        }

        /// <summary>
        ///     Builds a formatted string and outputs the result as warning text
        /// </summary>
        /// <returns>HRESULT of the IDebugControl4::OutputWide or IDebugControl::Output call</returns>
        public int OutputWarningLine()
        {
            return OutputLineHelper("\n", DEBUG_OUTPUT.WARNING);
        }

        /// <summary>
        ///     Builds a formatted string and outputs the result as warning text
        /// </summary>
        /// <param name="format">Format string using C# string formatting syntax</param>
        /// <param name="parameters">Optional parameters for the string format</param>
        /// <returns>HRESULT of the IDebugControl4::OutputWide or IDebugControl::Output call</returns>
        public int OutputWarningLine(string format, params object[] parameters)
        {
            ScanForPointerArguments(format, parameters, out format);
            var formattedString = ((parameters != null) && (parameters.Length != 0)) ? string.Format(CultureInfo.InvariantCulture, format, parameters) : format;
            return OutputLineHelper(formattedString, DEBUG_OUTPUT.WARNING);
        }

        /// <summary>
        ///     Builds a formatted string and outputs the result as error text
        /// </summary>
        /// <param name="format">Format string using C# string formatting syntax</param>
        /// <param name="parameters">Optional parameters for the string format</param>
        /// <returns>HRESULT of the IDebugControl4::OutputWide or IDebugControl::Output call</returns>
        public int OutputError(string format, params object[] parameters)
        {
            ScanForPointerArguments(format, parameters, out format);
            var formattedString = ((parameters != null) && (parameters.Length != 0)) ? string.Format(CultureInfo.InvariantCulture, format, parameters) : format;
            return OutputHelper(formattedString, DEBUG_OUTPUT.ERROR);
        }

        /// <summary>
        ///     Builds a formatted string and outputs the result as error text
        /// </summary>
        /// <param name="format">Format string using C# string formatting syntax</param>
        /// <param name="parameters">Optional parameters for the string format</param>
        /// <returns>HRESULT of the IDebugControl4::OutputWide or IDebugControl::Output call</returns>
        public int OutputErrorLine(string format, params object[] parameters)
        {
            ScanForPointerArguments(format, parameters, out format);
            var formattedString = ((parameters != null) && (parameters.Length != 0)) ? string.Format(CultureInfo.InvariantCulture, format, parameters) : format;
            return OutputLineHelper(formattedString, DEBUG_OUTPUT.ERROR);
        }

        /// <summary>
        ///     Outputs a formatted DML command block using the desired text and command
        /// </summary>
        /// <param name="text">Text which will appear as a clickable link</param>
        /// <param name="command">The command to run when the link is clicked</param>
        /// <returns>HRESULT of the IDebugControl4::ControlledOutputWide or IDebugControl::ControlledOutput call</returns>
        public int OutputDML(string text, string command)
        {
            return OutputDMLPreformatted(FormatDML(text, command));
        }

        /// <summary>
        ///     Outputs a formatted DML command block using the desired text and command
        /// </summary>
        /// <param name="text">Text which will appear as a clickable link</param>
        /// <param name="command">The command to run when the link is clicked</param>
        /// <returns>HRESULT of the IDebugControl4::ControlledOutputWide or IDebugControl::ControlledOutput call</returns>
        public int OutputDMLLine(string text, string command)
        {
            return OutputDMLPreformattedLine(FormatDML(text, command));
        }

        /// <summary>
        ///     Outputs a formatted DML command block using the desired text and command
        /// </summary>
        /// <param name="textFormatString">Format string which will appear as a clickable link</param>
        /// <param name="commandFormatString">Format string for the command to run when the link is clicked</param>
        /// <param name="parameters">Parameters to use when formatting the clickable text and the command string</param>
        /// <returns>HRESULT of the IDebugControl4::ControlledOutputWide or IDebugControl::ControlledOutput call</returns>
        public int OutputDML(string textFormatString, string commandFormatString, params object[] parameters)
        {
            return OutputDMLPreformatted(FormatDML(textFormatString, commandFormatString, parameters));
        }

        /// <summary>
        ///     Outputs a formatted DML command block using the desired text and command
        /// </summary>
        /// <param name="textFormatString">Format string which will appear as a clickable link</param>
        /// <param name="commandFormatString">Format string for the command to run when the link is clicked</param>
        /// <param name="parameters">Parameters to use when formatting the clickable text and the command string</param>
        /// <returns>HRESULT of the IDebugControl4::ControlledOutputWide or IDebugControl::ControlledOutput call</returns>
        public int OutputDMLLine(string textFormatString, string commandFormatString, params object[] parameters)
        {
            return OutputDMLPreformattedLine(FormatDML(textFormatString, commandFormatString, parameters));
        }

        /// <summary>
        ///     Outputs a formatted DML command block using the desired text and command
        /// </summary>
        /// <param name="textFormatString">Format string which will appear as a clickable link</param>
        /// <param name="commandFormatString">Format string for the command to run when the link is clicked</param>
        /// <param name="parameters">Parameters to use when formatting the clickable text and the command string</param>
        /// <returns>HRESULT of the IDebugControl4::ControlledOutputWide or IDebugControl::ControlledOutput call</returns>
        public int OutputDMLLineWithCommandIfDMLNotPossible(string textFormatString, string commandFormatString, params object[] parameters)
        {
            return OutputDMLPreformattedLine(FormatDMLWithDMLCheck(textFormatString, commandFormatString, parameters));
        }

        /// <summary>
        ///     Outputs a formatted DML command block using the desired text and command
        /// </summary>
        /// <param name="text">Text which will appear as a clickable link</param>
        /// <param name="command">The command to run when the link is clicked</param>
        /// <returns>HRESULT of the IDebugControl4::ControlledOutputWide or IDebugControl::ControlledOutput call</returns>
        public int OutputErrorDML(string text, string command)
        {
            return OutputErrorDMLPreformatted(FormatDML(text, command));
        }

        /// <summary>
        ///     Outputs a formatted DML command block using the desired text and command
        /// </summary>
        /// <param name="text">Text which will appear as a clickable link</param>
        /// <param name="command">The command to run when the link is clicked</param>
        /// <returns>HRESULT of the IDebugControl4::ControlledOutputWide or IDebugControl::ControlledOutput call</returns>
        public int OutputErrorDMLLine(string text, string command)
        {
            return OutputErrorDMLPreformattedLine(FormatDML(text, command));
        }

        /// <summary>
        ///     Outputs a formatted DML command block using the desired text and command
        /// </summary>
        /// <param name="textFormatString">Format string which will appear as a clickable link</param>
        /// <param name="commandFormatString">Format string for the command to run when the link is clicked</param>
        /// <param name="parameters">Parameters to use when formatting the clickable text and the command string</param>
        /// <returns>HRESULT of the IDebugControl4::ControlledOutputWide or IDebugControl::ControlledOutput call</returns>
        public int OutputErrorDML(string textFormatString, string commandFormatString, params object[] parameters)
        {
            return OutputErrorDMLPreformatted(FormatDML(textFormatString, commandFormatString, parameters));
        }

        /// <summary>
        ///     Outputs a formatted DML command block using the desired text and command
        /// </summary>
        /// <param name="textFormatString">Format string which will appear as a clickable link</param>
        /// <param name="commandFormatString">Format string for the command to run when the link is clicked</param>
        /// <param name="parameters">Parameters to use when formatting the clickable text and the command string</param>
        /// <returns>HRESULT of the IDebugControl4::ControlledOutputWide or IDebugControl::ControlledOutput call</returns>
        public int OutputErrorDMLLine(string textFormatString, string commandFormatString, params object[] parameters)
        {
            return OutputErrorDMLPreformattedLine(FormatDML(textFormatString, commandFormatString, parameters));
        }

        // ===
        /// <summary>
        ///     Outputs a formatted DML command block using the desired text and command
        /// </summary>
        /// <param name="text">Text which will appear as a clickable link</param>
        /// <param name="command">The command to run when the link is clicked</param>
        /// <returns>HRESULT of the IDebugControl4::ControlledOutputWide or IDebugControl::ControlledOutput call</returns>
        public int OutputWarningDML(string text, string command)
        {
            return OutputWarningDMLPreformatted(FormatDML(text, command));
        }

        /// <summary>
        ///     Outputs a formatted DML command block using the desired text and command
        /// </summary>
        /// <param name="text">Text which will appear as a clickable link</param>
        /// <param name="command">The command to run when the link is clicked</param>
        /// <returns>HRESULT of the IDebugControl4::ControlledOutputWide or IDebugControl::ControlledOutput call</returns>
        public int OutputWarningDMLLine(string text, string command)
        {
            return OutputWarningDMLPreformattedLine(FormatDML(text, command));
        }

        /// <summary>
        ///     Outputs a formatted DML command block using the desired text and command
        /// </summary>
        /// <param name="textFormatString">Format string which will appear as a clickable link</param>
        /// <param name="commandFormatString">Format string for the command to run when the link is clicked</param>
        /// <param name="parameters">Parameters to use when formatting the clickable text and the command string</param>
        /// <returns>HRESULT of the IDebugControl4::ControlledOutputWide or IDebugControl::ControlledOutput call</returns>
        public int OutputWarningDML(string textFormatString, string commandFormatString, params object[] parameters)
        {
            return OutputWarningDMLPreformatted(FormatDML(textFormatString, commandFormatString, parameters));
        }

        /// <summary>
        ///     Outputs a formatted DML command block using the desired text and command
        /// </summary>
        /// <param name="textFormatString">Format string which will appear as a clickable link</param>
        /// <param name="commandFormatString">Format string for the command to run when the link is clicked</param>
        /// <param name="parameters">Parameters to use when formatting the clickable text and the command string</param>
        /// <returns>HRESULT of the IDebugControl4::ControlledOutputWide or IDebugControl::ControlledOutput call</returns>
        public int OutputWarningDMLLine(string textFormatString, string commandFormatString, params object[] parameters)
        {
            return OutputWarningDMLPreformattedLine(FormatDML(textFormatString, commandFormatString, parameters));
        }

        // ===

        /// <summary>
        ///     Outputs a formatted DML command block using the desired text and command
        /// </summary>
        /// <param name="text">Text which will appear as a clickable link</param>
        /// <param name="command">The command to run when the link is clicked</param>
        /// <returns>HRESULT of the IDebugControl4::ControlledOutputWide or IDebugControl::ControlledOutput call</returns>
        public int OutputVerboseDML(string text, string command)
        {
            return OutputVerboseDMLPreformatted(FormatDML(text, command));
        }

        /// <summary>
        ///     Outputs a formatted DML command block using the desired text and command
        /// </summary>
        /// <param name="text">Text which will appear as a clickable link</param>
        /// <param name="command">The command to run when the link is clicked</param>
        /// <returns>HRESULT of the IDebugControl4::ControlledOutputWide or IDebugControl::ControlledOutput call</returns>
        public int OutputVerboseDMLLine(string text, string command)
        {
            return OutputVerboseDMLPreformattedLine(FormatDML(text, command));
        }

        /// <summary>
        ///     Outputs a formatted DML command block using the desired text and command
        /// </summary>
        /// <param name="textFormatString">Format string which will appear as a clickable link</param>
        /// <param name="commandFormatString">Format string for the command to run when the link is clicked</param>
        /// <param name="parameters">Parameters to use when formatting the clickable text and the command string</param>
        /// <returns>HRESULT of the IDebugControl4::ControlledOutputWide or IDebugControl::ControlledOutput call</returns>
        public int OutputVerboseDML(string textFormatString, string commandFormatString, params object[] parameters)
        {
            return OutputVerboseDMLPreformatted(FormatDML(textFormatString, commandFormatString, parameters));
        }

        /// <summary>
        ///     Outputs a formatted DML command block using the desired text and command
        /// </summary>
        /// <param name="textFormatString">Format string which will appear as a clickable link</param>
        /// <param name="commandFormatString">Format string for the command to run when the link is clicked</param>
        /// <param name="parameters">Parameters to use when formatting the clickable text and the command string</param>
        /// <returns>HRESULT of the IDebugControl4::ControlledOutputWide or IDebugControl::ControlledOutput call</returns>
        public int OutputVerboseDMLLine(string textFormatString, string commandFormatString, params object[] parameters)
        {
            return OutputVerboseDMLPreformattedLine(FormatDML(textFormatString, commandFormatString, parameters));
        }

        /// <summary>
        ///     Returns a formatted DML command block
        /// </summary>
        /// <param name="text">The text that should appear as clickable when displayed</param>
        /// <param name="command">The command that should execute when the DML link is clicked</param>
        /// <returns>The formatted DML command block</returns>
        public string FormatDML(string text, string command)
        {
            return string.Format(CultureInfo.InvariantCulture, "<link cmd=\"{0}\">{1}</link>", EncodeTextForXml(command), EncodeTextForXml(text));
        }

        /// <summary>
        ///     Returns a formatted DML command block
        /// </summary>
        /// <param name="textFormatString">Format string which will appear as a clickable link</param>
        /// <param name="commandFormatString">Format string for the command to run when the link is clicked</param>
        /// <param name="parameters">Parameters to use when formatting the clickable text and the command string</param>
        /// <returns>The formatted DML command block</returns>
        public string FormatDML(string textFormatString, string commandFormatString, params object[] parameters)
        {
            ScanForPointerArguments(commandFormatString, parameters, out commandFormatString);
            var formattedCommand = FormatString(commandFormatString, parameters);
            return string.Format(CultureInfo.InvariantCulture, "<link cmd=\"{0}\">{1}</link>", EncodeTextForXml(formattedCommand), EncodeTextForXml(FormatString(textFormatString, parameters)));
        }

        /// <summary>
        ///     Returns a formatted DML command block
        /// </summary>
        /// <param name="textFormatString">Format string which will appear as a clickable link</param>
        /// <param name="commandFormatString">Format string for the command to run when the link is clicked</param>
        /// <param name="parameters">Parameters to use when formatting the clickable text and the command string</param>
        /// <returns>The formatted DML command block</returns>
        public string FormatDMLWithDMLCheck(string textFormatString, string commandFormatString, params object[] parameters)
        {
            if (IsDebuggerDMLCapable())
            {
                return FormatDML(textFormatString, commandFormatString, parameters);
            }
            ScanForPointerArguments(commandFormatString, parameters, out commandFormatString);
            var formattedCommand = FormatString(commandFormatString, parameters);
            return string.Format(CultureInfo.InvariantCulture, "<link cmd=\"{0}\">{1}[{0}]</link>", EncodeTextForXml(formattedCommand), EncodeTextForXml(FormatString(textFormatString, parameters)));
        }

        /// <summary>
        ///     Formats a DML command block using the desired text and command, then appends it to a DbgStringBuilder
        /// </summary>
        /// <param name="sb">StringBuilder to which the DML command block is appended</param>
        /// <param name="text">Text which will appear as a clickable link</param>
        /// <param name="command">The command to run when the link is clicked</param>
        public void AppendDML(DbgStringBuilder sb, string text, string command)
        {
            sb.Append("<link cmd=\"");
            sb.Append(EncodeTextForXml(command));
            sb.Append("\">");
            sb.Append(EncodeTextForXml(text));
            sb.Append("</link>");
        }

        /// <summary>
        ///     Formats a DML command block using the desired text and command, then appends it to a DbgStringBuilder
        /// </summary>
        /// <param name="sb">DbgStringBuilder to which the DML command block is appended</param>
        /// <param name="textFormatString">Format string which will appear as a clickable link</param>
        /// <param name="commandFormatString">Format string for the command to run when the link is clicked</param>
        /// <param name="parameters">Parameters to use when formatting the clickable text and the command string</param>
        public void AppendDML(DbgStringBuilder sb, string textFormatString, string commandFormatString, params object[] parameters)
        {
            ScanForPointerArguments(commandFormatString, parameters, out commandFormatString);
            sb.Append("<link cmd=\"");
            sb.Append(EncodeTextForXml(FormatString(commandFormatString, parameters)));
            sb.Append("\">");
            sb.Append(EncodeTextForXml(FormatString(textFormatString, parameters)));
            sb.Append("</link>");
        }

        /// <summary>
        ///     Encodes a text string and replaces any special XML/DML characters with the appropriate replacement.
        /// </summary>
        /// <param name="input">String to modify</param>
        /// <returns>Encoded string</returns>
        public static string EncodeTextForXml(string input)
        {
            return s_EncodeTextForXml(input);
        }

        /// <summary>
        ///     Decodes a text string and replaces any special XML/DML escape sequences with the appropriate character.
        /// </summary>
        /// <param name="input">String to modify</param>
        /// <returns>Decoded string</returns>
        public static string DecodeTextFromXml(string input)
        {
            return s_DecodeTextFromXml(input);
        }

        /// <summary>
        ///     Encodes a text string and replaces any special XML/DML characters with the appropriate replacement.
        /// </summary>
        /// <param name="input">String to modify</param>
        /// <returns>Encoded string</returns>
        public static string s_EncodeTextForXml(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }
            //NOTE: Apparently DML doesn't want escaped apostrophes, because it isn't following the XML spec. Stupid.
            //return System.Security.SecurityElement.Escape(input);
            return input.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }

        /// <summary>
        ///     Decodes a text string and replaces any special XML/DML escape sequences with the appropriate character.
        /// </summary>
        /// <param name="input">String to modify</param>
        /// <returns>Decoded string</returns>
        public static string s_DecodeTextFromXml(string input)
        {
            return input.Replace("&gt;", ">").Replace("&lt;", "<").Replace("&quot;", "\"").Replace("&apos;", "'").Replace("&amp;", "&");
        }

        private void ScanForPointerArguments(string inputString, object[] parameters, out string outputString)
        {
            if ((parameters == null) || (parameters.Length == 0))
            {
                outputString = inputString;
                return;
            }

            var mc = PointerArgumentRegex.Matches(inputString);
            if ((mc.Count == 0))
            {
                outputString = inputString;
                return;
            }

            var sb = new StringBuilder(inputString.Length << 1);

            var endOfLastMatch = 0;
            for (var i = 0; i < mc.Count; ++i)
            {
                var m = mc[i];
                if (m.Index != endOfLastMatch)
                {
                    sb.Append(inputString.Substring(endOfLastMatch, m.Index - endOfLastMatch));
                }
                endOfLastMatch = m.Index + m.Length;

                var parameterIndex = int.Parse(m.Groups[1].Value);

                var parameter = parameters[parameterIndex];
                if (parameter is ulong) {}
                else
                {
                    try {}
                    catch
                    {
                        throw;
                    }
                    //catch { pointer = 0xdeadbeef; }
                }

                var pointerAsString = "(null)";
                if (m.Groups[2].Value == "p")
                {
                    if (parameters[parameterIndex] == null)
                    {
                        pointerAsString = "(null)";
                    }
                    else
                    {
                        try
                        {
                            pointerAsString = P2S((ulong)parameters[parameterIndex]);
                        }
                        catch
                        {
                            throw new Exception("Could not convert " + parameters[parameterIndex] + " to UInt64 for {x:p}");
                        }
                    }
                }
                if (m.Groups[2].Value == "P")
                {
                    if (parameters[parameterIndex] == null)
                    {
                        pointerAsString = "(NULL)";
                    }
                    else
                    {
                        try
                        {
                            pointerAsString = P2SUC((ulong)parameters[parameterIndex]);
                        }
                        catch
                        {
                            throw new Exception("Could not convert " + parameters[parameterIndex] + " to UInt64 for {x:P}");
                        }
                    }
                }

                if (m.Groups[2].Value == "q")
                {
                    if (parameters[parameterIndex] == null)
                    {
                        pointerAsString = "(null)";
                    }
                    else
                    {
                        try
                        {
                            pointerAsString = P2S((ulong)parameters[parameterIndex], true);
                        }
                        catch
                        {
                            throw new Exception("Could not convert " + parameters[parameterIndex] + " to UInt64 for {x:p}");
                        }
                    }
                }
                if (m.Groups[2].Value == "Q")
                {
                    if (parameters[parameterIndex] == null)
                    {
                        pointerAsString = "(NULL)";
                    }
                    else
                    {
                        try
                        {
                            pointerAsString = P2SUC((ulong)parameters[parameterIndex], true);
                        }
                        catch
                        {
                            throw new Exception("Could not convert " + parameters[parameterIndex] + " to UInt64 for {x:P}");
                        }
                    }
                }

                if ((m.Groups.Count >= 4) && (!string.IsNullOrEmpty(m.Groups[3].Value)))
                {
                    bool leftAlign;
                    var desiredWidth = int.Parse(m.Groups[3].Value.Substring(1));
                    if (desiredWidth < 0)
                    {
                        desiredWidth = -desiredWidth;
                        leftAlign = true;
                    }
                    else
                    {
                        leftAlign = false;
                    }

                    var spacesToAdd = desiredWidth - pointerAsString.Length;
                    if (spacesToAdd > 0)
                    {
                        if (leftAlign)
                        {
                            sb.Append(pointerAsString);
                            sb.Append(' ', spacesToAdd);
                        }
                        else
                        {
                            sb.Append(' ', spacesToAdd);
                            sb.Append(pointerAsString);
                        }
                    }
                    else
                    {
                        sb.Append(pointerAsString);
                    }
                }
                else
                {
                    sb.Append(pointerAsString);
                }
            }
            if (endOfLastMatch < inputString.Length)
            {
                sb.Append(inputString.Substring(endOfLastMatch, inputString.Length - endOfLastMatch));
            }

            outputString = sb.ToString();
        }

        /// <summary>
        ///     Like String.Format except it understands the special pointer identifier
        /// </summary>
        /// <param name="format">Format string</param>
        /// <param name="parameters">Parameters to use as data when formatting</param>
        /// <returns>Formatted string</returns>
        public string FormatString(string format, params object[] parameters)
        {
            if (parameters == null)
            {
                return format;
            }
            ScanForPointerArguments(format, parameters, out format);
            return string.Format(CultureInfo.InvariantCulture, format, parameters);
        }

        /// <summary>
        ///     Like String.Format except it understands the special pointer identifier
        /// </summary>
        /// <param name="format">Format string</param>
        /// <param name="parameters">Parameters to use as data when formatting</param>
        /// <returns>Formatted string</returns>
        public string FormatStringEx(string format, params object[] parameters)
        {
            return string.Format(new DMLInterceptProvider(this), format, parameters);
        }

        /// <summary>
        ///     Like String.Format except it understands the special pointer identifier
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="format">Format string</param>
        /// <param name="parameters">Parameters to use as data when formatting</param>
        /// <returns>Formatted string</returns>
        public string FormatString(IFormatProvider provider, string format, params object[] parameters)
        {
            ScanForPointerArguments(format, parameters, out format);
            return string.Format(provider, format, parameters);
        }

        public int ControlledOutputWide(DEBUG_OUTCTL OutputControl, DEBUG_OUTPUT Mask, string Data)
        {

            int maxDataLength = 4096;
            if (_refreshStatusText == true || StatusBarStopWatch.ElapsedMilliseconds > 1000)
            {
#if INTERNAL
                RefreshStatusBar();
#endif
            }

            if (Settings.Get("EnableFastOutput") == true)
            {
                maxDataLength = 16384;
                if (Data.Length < maxDataLength)
                {
                    return ControlledOutputWideImpl(OutputControl, Mask, Data);
                }
            }

            if (Data.Length <= 4096 || Data.Contains("<"))
            {
                return ControlledOutputWideImpl(OutputControl, Mask, Data);
            }

            var length = Data.Length;
            var currentPos = 0;
            var recentPos = 0;
            var flush = true;
            while (currentPos < length)
            {
                if (ShouldBreak())
                {
                    return E_FAIL;
                }
                var nextBreak = Data.IndexOf('\n', Math.Min(currentPos + (maxDataLength/2), length));
                if (nextBreak == -1)
                {
                    nextBreak = length;
                }
                else
                {
                    flush = true;
                }


                var nextOutputLength = Math.Min(nextBreak - currentPos, maxDataLength/2);
                var currentOutput = Data.Substring(currentPos, nextOutputLength);
                ControlledOutputWideImpl(OutputControl, Mask, currentOutput);
                if (currentOutput.Contains("<"))
                {
                    flush = false;
                }

                currentPos += nextOutputLength;
                recentPos += nextOutputLength;
                if (recentPos >= maxDataLength && flush == true)
                {
                    DebugClient.FlushCallbacks();
                    recentPos = 0;
                    flush = false;
                }
            }

            DebugClient.FlushCallbacks();
            return S_OK;
        }

        private int ControlledOutputWideImpl(DEBUG_OUTCTL OutputControl, DEBUG_OUTPUT Mask, string Data)
        {

            if (Settings.Get("EnableFastOutput") == true)
            {
                return DebugControl.ControlledOutputWide(OutputControl, Mask, @"%s", Data);
            }

            //http://bugcheck/bugs/WindowsBlueBugs/358786          
            return DebugControl.ControlledOutputWide(OutputControl, Mask, Data.Replace("%", "%%"), null);
            //return DebugControl.ControlledOutputWide(OutputControl, Mask, @"%s", Data);
        }

   
        private string EscapePercents(string input)
        {
            var index = input.IndexOf('%');
            if (index == -1)
            {
                return input;
            }

            var segmentStart = 0;
            var sb = new StringBuilder(input.Length << 1);

            do
            {
                var prefixLength = index - segmentStart;
                if (prefixLength > 0)
                {
                    sb.Append(input.Substring(segmentStart, prefixLength));
                }
                var percentCount = 1;

                while ((++index < input.Length) && ((input[index]) == '%'))
                {
                    ++percentCount;
                }

                sb.Append('%', ((percentCount & 1) == 0) ? percentCount : percentCount + 1);

                segmentStart = index;

                index = input.IndexOf('%', segmentStart);
                if (index == -1)
                {
                    sb.Append(input.Substring(segmentStart));
                    break;
                }
            } while (index < input.Length);

            return sb.ToString();
        }

        /// <summary>
        ///     Replaces all occurrences of a string in the debugger output with the contents of another
        /// </summary>
        /// <param name="variable">String to replace. Normally something like @#VariableName</param>
        /// <param name="replacement">Text that should be used in place of variable</param>
        /// <returns>HRESULT</returns>
        public int AliasSet(string variable, string replacement)
        {
            OutputVerboseLine("ALIAS: {0} = {1}", variable, replacement);

            DebugControl.SetTextReplacementWide(variable, null);
            return DebugControl.SetTextReplacementWide(variable, replacement);
        }

        public bool IsDebuggerDMLCapable()
        {

            if (_dmlCapable != null && _dmlCapable.Value == true)
            {
                return true;
            }

            if (IsWindbg)
            {
                return true;
            }           
            try
            {
                int hr = DebugAdvanced.Request(DEBUG_REQUEST.CURRENT_OUTPUT_CALLBACKS_ARE_DML_AWARE, null, 0, null, 0, null);
                bool dmlAware = hr == S_OK;
                return dmlAware;
            }
            catch
            {
                return false;
            }
        }

        public static bool DebuggerDMLCapable()
        {
            return _dmlCapable != null && _dmlCapable.Value;
        }

        public bool PreferDML()
        {
            try
            {
                if (IsDebuggerDMLCapable() == false)
                {
                    return false;
                }
                DEBUG_ENGOPT options;
                if (FAILED(DebugControl.GetEngineOptions(out options)))
                {
                    return false;
                }

                if (options.HasFlag(DEBUG_ENGOPT.PREFER_DML))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        public static bool PreferDMLSet()
        {
            return _preferDML != null && _preferDML.Value;
        }

        /// <summary>
        ///     Stops replacing text for a specific string.
        /// </summary>
        /// <param name="variable">String to reset. Normally something like @#VariableName</param>
        /// <returns>HRESULT</returns>
        public int AliasClear(string variable)
        {
            return DebugControl.SetTextReplacementWide(variable, null);
        }

        /// <summary>
        ///     Changes a DML block to plain text
        /// </summary>
        public static string DmlToText(string dml, bool Unescape = false)
        {
            var openBracket = dml.IndexOf('<');
            if (openBracket < 0)
            {
                goto UnescapeAnyway;
            }

            var closeBracket = dml.IndexOf('>', openBracket);
            if (closeBracket < 0)
            {
                goto UnescapeAnyway;
            }

            var sb = new StringBuilder();
            if (openBracket != 0)
            {
                sb.Append(dml, 0, openBracket);
            }

            for (;;)
            {
                openBracket = dml.IndexOf('<', closeBracket);
                if (openBracket < 0)
                {
                    sb.Append(dml, closeBracket + 1, dml.Length - closeBracket - 1);
                    break;
                }

                var previousCloseBracket = closeBracket;
                closeBracket = dml.IndexOf('>', openBracket);
                if (closeBracket < 0)
                {
                    sb.Append(dml, openBracket, dml.Length - openBracket);
                    break;
                }

                sb.Append(dml, previousCloseBracket + 1, openBracket - previousCloseBracket - 1);
            }
            return Unescape ? sb.Replace("&quot;", "\"").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&").Replace("&apos;", "'").ToString() : sb.ToString();

            UnescapeAnyway:

            return Unescape ? dml.Replace("&quot;", "\"").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&").Replace("&apos;", "'") : dml;
        }

        public string FormatToHex(uint addr)
        {
            return FormatToHex((ulong)addr);
        }

        public string FormatToHex(ulong addr)
        {
            var output = string.Format("0x{0:X}", addr).ToLower();

            // If we have more than 8 hex characters, insert a backtick
            if (output.Length > 10)
            {
                output = output.Insert(output.Length - 8, "`");
            }

            return output;
        }

        /// <summary>
        ///     Determines if your struct type (in terms of module!_STRUCT) matches variants of the target type.  E.g. does _IRP
        ///     match _IRP, IRP, irp, etc.
        /// </summary>
        /// <param name="StructTypeInQuestion">
        ///     Whatever your input type is.  Include "module!" prefix or simply "!" prefix to match
        ///     only the struct type.
        /// </param>
        /// <param name="UnderscoredRefStructType"></param>
        /// <returns></returns>
        public bool MatchStructTypeVariants(string StructTypeInQuestion, string UnderscoredRefStructType)
        {
            // Fix this up if there isn't an underscore already
            if (!UnderscoredRefStructType.Contains("!_"))
            {
                UnderscoredRefStructType.Insert(UnderscoredRefStructType.IndexOf("!", StringComparison.Ordinal) + 1, "_");
            }

            //
            // Check for both mod!TYPE and mod!_TYPE (with and without underscore)
            //            
            string with_;

            if (StructTypeInQuestion.Contains("!_"))
            {
                with_ = StructTypeInQuestion;
            }
            else
            {
                with_ = StructTypeInQuestion.Replace("!", "!_");
            }

            if (UnderscoredRefStructType.EndsWith(with_, StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
            return false;
        }
    }
}