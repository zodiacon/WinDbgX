using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;

namespace Microsoft.Mex.DotNetDbg
{
    /// <summary>
    ///     A replacement for the StringBuilder class that uses \r as a newline instead of \r\n.
    /// </summary>
    [DataContract]
    public class DbgStringBuilder
    {
        [DataMember]
        private readonly StringBuilder _sb;

        private DebugUtilities _utilities;

        /// <summary>
        ///     WARNING.  For serialization/deserialization only.  Use SetDebugUtilities when deserializing to make it writable
        ///     again.
        /// </summary>
        public DbgStringBuilder()
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="debugUtilities">A DebugUtilities associated with the debugger.</param>
        public DbgStringBuilder(DebugUtilities debugUtilities)
        {
            _sb = new StringBuilder();
            _utilities = debugUtilities;
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="debugUtilities">A DebugUtilities associated with the debugger.</param>
        /// <param name="initialSize">Initial size of the buffer in characters</param>
        public DbgStringBuilder(DebugUtilities debugUtilities, int initialSize)
        {
            _sb = new StringBuilder(initialSize);
            _utilities = debugUtilities;
        }

        /// <summary>
        ///     Retreieve or set a specific character in the buffer
        /// </summary>
        public char this[int i]
        {
            get {return _sb[i];}
            set {_sb[i] = value;}
        }

        /// <summary>
        ///     Allows access to the internal StringBuild class
        /// </summary>
        public StringBuilder NativeStringBuilder
        {
            get {return _sb;}
        }

        /// <summary>
        ///     Gets or sets the underlying buffer size in characters
        /// </summary>
        public int Capacity
        {
            get {return _sb.Capacity;}
            set {_sb.Capacity = value;}
        }

        /// <summary>
        ///     Gets the maximum number of characters that the class can hold
        /// </summary>
        public int MaxCapacity
        {
            get {return _sb.MaxCapacity;}
        }

        /// <summary>
        ///     Gets the number of characters currently in the buffer, or sets the size of the buffer
        /// </summary>
        public int Length
        {
            get {return _sb.Length;}
            set {_sb.Length = value;}
        }

        public void SetDebugUtilities(DebugUtilities d)
        {
            _utilities = d;
        }

        /// <summary>
        ///     Inserts an item at a specific location
        /// </summary>
        /// <param name="index">Location to insert the new value at</param>
        /// <param name="addition">Value to insert</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Insert(int index, bool addition)
        {
            _sb.Insert(index, addition);
            return this;
        }

        /// <summary>
        ///     Inserts an item at a specific location
        /// </summary>
        /// <param name="index">Location to insert the new value at</param>
        /// <param name="addition">Value to insert</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Insert(int index, byte addition)
        {
            _sb.Insert(index, addition);
            return this;
        }

        /// <summary>
        ///     Inserts an item at a specific location
        /// </summary>
        /// <param name="index">Location to insert the new value at</param>
        /// <param name="addition">Value to insert</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Insert(int index, sbyte addition)
        {
            _sb.Insert(index, addition);
            return this;
        }

        /// <summary>
        ///     Inserts an item at a specific location
        /// </summary>
        /// <param name="index">Location to insert the new value at</param>
        /// <param name="addition">Value to insert</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Insert(int index, Int16 addition)
        {
            _sb.Insert(index, addition);
            return this;
        }

        /// <summary>
        ///     Inserts an item at a specific location
        /// </summary>
        /// <param name="index">Location to insert the new value at</param>
        /// <param name="addition">Value to insert</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Insert(int index, UInt16 addition)
        {
            _sb.Insert(index, addition);
            return this;
        }

        /// <summary>
        ///     Inserts an item at a specific location
        /// </summary>
        /// <param name="index">Location to insert the new value at</param>
        /// <param name="addition">Value to insert</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Insert(int index, Int32 addition)
        {
            _sb.Insert(index, addition);
            return this;
        }

        /// <summary>
        ///     Inserts an item at a specific location
        /// </summary>
        /// <param name="index">Location to insert the new value at</param>
        /// <param name="addition">Value to insert</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Insert(int index, UInt32 addition)
        {
            _sb.Insert(index, addition);
            return this;
        }

        /// <summary>
        ///     Inserts an item at a specific location
        /// </summary>
        /// <param name="index">Location to insert the new value at</param>
        /// <param name="addition">Value to insert</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Insert(int index, Int64 addition)
        {
            _sb.Insert(index, addition);
            return this;
        }

        /// <summary>
        ///     Inserts an item at a specific location
        /// </summary>
        /// <param name="index">Location to insert the new value at</param>
        /// <param name="addition">Value to insert</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Insert(int index, UInt64 addition)
        {
            _sb.Insert(index, addition);
            return this;
        }

        /// <summary>
        ///     Inserts an item at a specific location
        /// </summary>
        /// <param name="index">Location to insert the new value at</param>
        /// <param name="addition">Value to insert</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Insert(int index, char addition)
        {
            _sb.Insert(index, addition);
            return this;
        }

        /// <summary>
        ///     Inserts an item at a specific location
        /// </summary>
        /// <param name="index">Location to insert the new value at</param>
        /// <param name="addition">Value to insert</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Insert(int index, char[] addition)
        {
            _sb.Insert(index, addition);
            return this;
        }

        /// <summary>
        ///     Inserts an item at a specific location
        /// </summary>
        /// <param name="index">Location to insert the new value at</param>
        /// <param name="addition">Value to insert</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Insert(int index, decimal addition)
        {
            _sb.Insert(index, addition);
            return this;
        }

        /// <summary>
        ///     Inserts an item at a specific location
        /// </summary>
        /// <param name="index">Location to insert the new value at</param>
        /// <param name="addition">Value to insert</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Insert(int index, double addition)
        {
            _sb.Insert(index, addition);
            return this;
        }

        /// <summary>
        ///     Inserts an item at a specific location
        /// </summary>
        /// <param name="index">Location to insert the new value at</param>
        /// <param name="addition">Value to insert</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Insert(int index, float addition)
        {
            _sb.Insert(index, addition);
            return this;
        }

        /// <summary>
        ///     Inserts an item at a specific location
        /// </summary>
        /// <param name="index">Location to insert the new value at</param>
        /// <param name="addition">Value to insert</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Insert(int index, object addition)
        {
            _sb.Insert(index, addition);
            return this;
        }

        /// <summary>
        ///     Inserts one or more characters into the builder at a specified index
        /// </summary>
        /// <param name="index">Location to insert the new value at</param>
        /// <param name="addition">Value to insert</param>
        /// <param name="startIndex">Index into addition to start copying</param>
        /// <param name="charCount">The number of characters to copy</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Insert(int index, char[] addition, int startIndex, int charCount)
        {
            _sb.Insert(index, addition, startIndex, charCount);
            return this;
        }

        /// <summary>
        ///     Inserts one or more copies of an item at a specific location
        /// </summary>
        /// <param name="index">Location to insert the new value at</param>
        /// <param name="addition">Value to insert</param>
        /// <param name="count">The number of copies to insert</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Insert(int index, string addition, int count)
        {
            _sb.Insert(index, addition, count);
            return this;
        }

        /// <summary>
        ///     Inserts an item at a specific location
        /// </summary>
        /// <param name="index">Location to insert the new value at</param>
        /// <param name="addition">Value to insert</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Insert(int index, string addition)
        {
            _sb.Insert(index, addition);
            return this;
        }

        /// <summary>
        ///     Appends an items to the end of the buffer
        /// </summary>
        /// <param name="addition">Value to insert</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Append(bool addition)
        {
            _sb.Append(addition);
            return this;
        }

        /// <summary>
        ///     Appends an items to the end of the buffer
        /// </summary>
        /// <param name="addition">Value to insert</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Append(byte addition)
        {
            _sb.Append(addition);
            return this;
        }

        /// <summary>
        ///     Appends an items to the end of the buffer
        /// </summary>
        /// <param name="addition">Value to insert</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Append(sbyte addition)
        {
            _sb.Append(addition);
            return this;
        }

        /// <summary>
        ///     Appends an items to the end of the buffer
        /// </summary>
        /// <param name="addition">Value to insert</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Append(Int16 addition)
        {
            _sb.Append(addition);
            return this;
        }

        /// <summary>
        ///     Appends an items to the end of the buffer
        /// </summary>
        /// <param name="addition">Value to insert</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Append(UInt16 addition)
        {
            _sb.Append(addition);
            return this;
        }

        /// <summary>
        ///     Appends an items to the end of the buffer
        /// </summary>
        /// <param name="addition">Value to insert</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Append(Int32 addition)
        {
            _sb.Append(addition);
            return this;
        }

        /// <summary>
        ///     Appends an items to the end of the buffer
        /// </summary>
        /// <param name="addition">Value to insert</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Append(UInt32 addition)
        {
            _sb.Append(addition);
            return this;
        }

        /// <summary>
        ///     Appends an items to the end of the buffer
        /// </summary>
        /// <param name="addition">Value to insert</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Append(Int64 addition)
        {
            _sb.Append(addition);
            return this;
        }

        /// <summary>
        ///     Appends an items to the end of the buffer
        /// </summary>
        /// <param name="addition">Value to insert</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Append(UInt64 addition)
        {
            _sb.Append(addition);
            return this;
        }

        /// <summary>
        ///     Appends an items to the end of the buffer
        /// </summary>
        /// <param name="addition">Value to insert</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Append(char addition)
        {
            _sb.Append(addition);
            return this;
        }

        /// <summary>
        ///     Appends an items to the end of the buffer
        /// </summary>
        /// <param name="addition">Value to insert</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Append(char[] addition)
        {
            _sb.Append(addition);
            return this;
        }

        /// <summary>
        ///     Appends an items to the end of the buffer
        /// </summary>
        /// <param name="addition">Value to insert</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Append(decimal addition)
        {
            _sb.Append(addition);
            return this;
        }

        /// <summary>
        ///     Appends an items to the end of the buffer
        /// </summary>
        /// <param name="addition">Value to insert</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Append(double addition)
        {
            _sb.Append(addition);
            return this;
        }

        /// <summary>
        ///     Appends an items to the end of the buffer
        /// </summary>
        /// <param name="addition">Value to insert</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Append(float addition)
        {
            _sb.Append(addition);
            return this;
        }

        /// <summary>
        ///     Appends an items to the end of the buffer
        /// </summary>
        /// <param name="addition">Value to insert</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Append(object addition)
        {
            _sb.Append(addition);
            return this;
        }

        /// <summary>
        ///     Appends one or more copies of a character to the end of the buffer
        /// </summary>
        /// <param name="addition">Value to insert</param>
        /// <param name="repeatCount">Number of copies to insert</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Append(char addition, int repeatCount)
        {
            _sb.Append(addition, repeatCount);
            return this;
        }

        /// <summary>
        ///     Appends one or more characters out of an array to the end of the buffer
        /// </summary>
        /// <param name="addition">Value to insert</param>
        /// <param name="startIndex">Index into addition to start copying from</param>
        /// <param name="charCount">How many characters to copy</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Append(char[] addition, int startIndex, int charCount)
        {
            _sb.Append(addition, startIndex, charCount);
            return this;
        }

        /// <summary>
        ///     Appends a substring to the end of the buffer
        /// </summary>
        /// <param name="addition">Value to insert</param>
        /// <param name="startIndex">Index into addition to start copying from</param>
        /// <param name="count">How many characters to copy</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Append(string addition, int startIndex, int count)
        {
            _sb.Append(addition, startIndex, count);
            return this;
        }

        /// <summary>
        ///     Appends an items to the end of the buffer
        /// </summary>
        /// <param name="addition">Value to insert</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Append(string addition)
        {
            _sb.Append(addition);
            return this;
        }

        /// <summary>
        ///     Appends an items to the end of the buffer with XML special characters escaped
        /// </summary>
        /// <param name="addition">Value to insert</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder AppendEscaped(string addition)
        {
            _sb.Append(DebugUtilities.EncodeTextForXml(addition));
            return this;
        }

        /// <summary>
        ///     Appends a formatted string to the end of the buffer followed by a newline
        /// </summary>
        /// <param name="additionFormatted">Value to insert</param>
        /// <param name="args"></param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder AppendLineFormatted(string additionFormatted, params object[] args)
        {
            AppendFormat(additionFormatted, args);
            _sb.Append('\n');
            return this;
        }

        /// <summary>
        ///     Appends a formatted string with DML link and a newline. Requires 3 params, the string, the text display as the link
        ///     and the link itself.
        ///     Example: AppendLineWithDMLLink("String with {0}", "link to !mem", "!mex.mem")
        /// </summary>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder AppendLineWithDMLLink(string additionFormatted, string dmlText, string dmlCommand)
        {
            string dml = _utilities.FormatDML(dmlText, dmlCommand);
            AppendFormat(additionFormatted, dml);
            Append('\n');
            return this;
        }

        /// <summary>
        ///     Appends a formatted string with DML link. Requires 3 params, the string, the text display as the link and the link
        ///     itself.
        /// </summary>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder AppendWithDMLLink(string additionFormatted, string dmlText, string dmlCommand)
        {
            string str = _utilities.FormatDML(dmlText, dmlCommand);
            AppendFormat(additionFormatted, str);
            return this;
        }

        /// <summary>
        ///     Appends an items to the end of the buffer followed by a newline
        /// </summary>
        /// <param name="addition">Value to insert</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder AppendLine(string addition)
        {
            _sb.Append(addition);
            _sb.Append('\n');
            return this;
        }

        /// <summary>
        ///     Appends an items to the end of the buffer followed by a newline, with XML special characters escaped
        /// </summary>
        /// <param name="addition">Value to insert</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder AppendLineEscaped(string addition)
        {
            _sb.Append(DebugUtilities.EncodeTextForXml(addition));
            _sb.Append('\n');
            return this;
        }

        /// <summary>
        ///     Appends a newline to the end of the buffer
        /// </summary>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder AppendLine()
        {
            _sb.Append('\n');
            return this;
        }

        /// <summary>
        ///     Appends a formatted string to the end of the buffer
        /// </summary>
        /// <param name="format">Format string</param>
        /// <param name="arg0">First parameter</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder AppendFormat(string format, object arg0)
        {
            _sb.Append(_utilities.FormatString(format, arg0));
            return this;
        }

        /// <summary>
        ///     Appends a formatted string to the end of the buffer
        /// </summary>
        /// <param name="format">Format string</param>
        /// <param name="arg0">First parameter</param>
        /// <param name="arg1">Second parameter</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder AppendFormat(string format, object arg0, object arg1)
        {
            _sb.Append(_utilities.FormatString(format, arg0, arg1));
            return this;
        }

        /// <summary>
        ///     Appends a formatted string to the end of the buffer
        /// </summary>
        /// <param name="format">Format string</param>
        /// <param name="arg0">First parameter</param>
        /// <param name="arg1">Second parameter</param>
        /// <param name="arg2">Third parameter</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder AppendFormat(string format, object arg0, object arg1, object arg2)
        {
            _sb.Append(_utilities.FormatString(format, arg0, arg1, arg2));
            return this;
        }

        /// <summary>
        ///     Appends a formatted string to the end of the buffer
        /// </summary>
        /// <param name="format">Format string</param>
        /// <param name="args">Arguments</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder AppendFormat(string format, params object[] args)
        {
            _sb.Append(_utilities.FormatString(format, args));
            return this;
        }

        /// <summary>
        ///     Appends a formatted string to the end of the buffer with XML special characters escaped
        /// </summary>
        /// <param name="format">Format string</param>
        /// <param name="args">Arguments</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder AppendFormatEscaped(string format, params object[] args)
        {
            _sb.Append(DebugUtilities.EncodeTextForXml(_utilities.FormatString(format, args)));
            return this;
        }

        /// <summary>
        ///     Appends a formatted string to the end of the buffer
        /// </summary>
        /// <param name="provider">Format provider</param>
        /// <param name="format">Format string</param>
        /// <param name="args">Arguments</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder AppendFormat(IFormatProvider provider, string format, params object[] args)
        {
            _sb.Append(_utilities.FormatString(provider, format, args));
            return this;
        }

        /// <summary>
        ///     Appends a formatted string to the end of the buffer
        /// </summary>
        /// <param name="provider">Format provider</param>
        /// <param name="format">Format string</param>
        /// <param name="args">Arguments</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder AppendFormatEscaped(IFormatProvider provider, string format, params object[] args)
        {
            _sb.Append(DebugUtilities.EncodeTextForXml(_utilities.FormatString(provider, format, args)));
            return this;
        }

        /// <summary>
        ///     Resets the buffer to 0
        /// </summary>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Clear()
        {
            _sb.Length = 0;
            return this;
        }

        /// <summary>
        ///     Replaces all instances of a specified character with a second one
        /// </summary>
        /// <param name="oldChar">Value to replace</param>
        /// <param name="newChar">Value to insert</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Replace(char oldChar, char newChar)
        {
            _sb.Replace(oldChar, newChar);
            return this;
        }

        /// <summary>
        ///     Replaces all instances of a specified character with a second one
        /// </summary>
        /// <param name="oldValue">Value to replace</param>
        /// <param name="newValue">Value to insert</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Replace(string oldValue, string newValue)
        {
            _sb.Replace(oldValue, newValue);
            return this;
        }

        /// <summary>
        ///     Replaces, withing a substring of this instance, all instances of a specified character with a second one
        /// </summary>
        /// <param name="oldChar">Value to replace</param>
        /// <param name="newChar">Value to insert</param>
        /// <param name="startIndex">Start of the substring</param>
        /// <param name="count">Length of the substring</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Replace(char oldChar, char newChar, int startIndex, int count)
        {
            _sb.Replace(oldChar, newChar, startIndex, count);
            return this;
        }

        /// <summary>
        ///     Replaces, withing a substring of this instance, all instances of a specified string with a second one
        /// </summary>
        /// <param name="oldValue">Value to replace</param>
        /// <param name="newValue">Value to insert</param>
        /// <param name="startIndex">Start of the substring</param>
        /// <param name="count">Length of the substring</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Replace(string oldValue, string newValue, int startIndex, int count)
        {
            _sb.Replace(oldValue, newValue, startIndex, count);
            return this;
        }

        /// <summary>
        ///     Removes characters from the instance
        /// </summary>
        /// <param name="startIndex">Start location to remove from</param>
        /// <param name="length">Number of characters to remove</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder Remove(int startIndex, int length)
        {
            _sb.Remove(startIndex, length);
            return this;
        }

        /// <summary>
        ///     Formats a DML command block using the desired text and command, then appends it to a DbgStringBuilder
        /// </summary>
        /// <param name="text">Text which will appear as a clickable link</param>
        /// <param name="command">The command to run when the link is clicked</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder AppendDML(string text, string command)
        {
            _utilities.AppendDML(this, text, command);
            return this;
        }

        /// <summary>
        ///     Formats a DML command block using the desired text and command
        /// </summary>
        /// <param name="textFormatString">Format string which will appear as a clickable link</param>
        /// <param name="commandFormatString">Format string for the command to run when the link is clicked</param>
        /// <param name="parameters">Parameters to use when formatting the clickable text and the command string</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder AppendDML(string textFormatString, string commandFormatString, params object[] parameters)
        {
            _utilities.AppendDML(this, textFormatString, commandFormatString, parameters);
            return this;
        }

        /// <summary>
        ///     Formats a DML command block using the desired text and command
        /// </summary>
        /// <param name="dml">DebugUtilities.DML or DebugUtilities.IDMLAble object to append</param>      
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder AppendDML(DebugUtilities.IDMLAble dml)
        {
            _sb.Append(dml.ToDML().ToPreformatedDML());
            return this;
        }

        /// <summary>
        ///     Formats a DML command block using the desired text and command
        /// </summary>
        /// <param name="textFormatString">Format string which will appear as a clickable link</param>
        /// <param name="commandFormatString">Format string for the command to run when the link is clicked</param>
        /// <param name="parameters">Parameters to use when formatting the clickable text and the command string</param>
        /// <returns>The DbgStringBuilder instance</returns>
        public DbgStringBuilder AppendDMLLine(string textFormatString, string commandFormatString, params object[] parameters)
        {
            _utilities.AppendDML(this, textFormatString + "\n", commandFormatString, parameters);
            return this;
        }

        /// <summary>
        ///     Ensures the capacity is at least the specified value
        /// </summary>
        /// <param name="capacity">Minimum capacity to ensure</param>
        public int EnsureCapacity(int capacity)
        {
            return _sb.EnsureCapacity(capacity);
        }

        /// <summary>
        ///     Gets a string representation of the data in the buffer
        /// </summary>
        public override string ToString()
        {
            return _sb.ToString();
        }

        /// <summary>
        ///     Gets the hash code for the underlying StringBuilder object
        /// </summary>
        public override int GetHashCode()
        {
            Debug.Assert(_sb != null, "_sb != null");
            return _sb.GetHashCode();
        }
    }
}