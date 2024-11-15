﻿namespace Cli
{
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.IO;
	using System.Reflection;
	using System.Text;
	using System;
	using System.Data.SqlTypes;

	/// <summary>
	/// Represents an exception in the argument processesing
	/// </summary>
#if CALIB
	public
#endif
	class CmdException : Exception
	{
		/// <summary>
		/// The name of the named argument, if applicable
		/// </summary>
		public string Name { get; private set; }
		/// <summary>
		/// The ordinal of the ordinal argument, if applicable
		/// </summary>
		public int Ordinal { get; private set; }
		/// <summary>
		/// Constructs a new instance
		/// </summary>
		/// <param name="message">The message</param>
		/// <param name="ordinal">The ordinal</param>
		public CmdException(string message, int ordinal) : base(message)
		{
			Name = null;
			Ordinal = ordinal;
		}
		/// <summary>
		/// Constructs a new instance
		/// </summary>
		/// <param name="message">The message</param>
		/// <param name="name">The name</param>
		public CmdException(string message, string name) : base(message)
		{
			Name = name;
			Ordinal = -1;
		}
		/// <summary>
		/// Constructs a new instance
		/// </summary>
		/// <param name="message">The message</param>
		/// <param name="ordinal">The ordinal</param>
		/// <param name="innerException">The inner exception</param>
		public CmdException(string message, int ordinal, Exception innerException) : base(message,innerException)
		{

		}
		/// <summary>
		/// Constructs a new instance
		/// </summary>
		/// <param name="message">The message</param>
		/// <param name="name">The name</param>
		/// <param name="innerException">The inner exception</param>
		public CmdException(string message, string name, Exception innerException) : base(message,innerException) 
		{

		}
	}
	/// <summary>
	/// The type of switch
	/// </summary>
#if CLILIB
	public
#endif
	enum CmdSwitchType
	{
		/// <summary>
		/// Just the switch itself
		/// </summary>
		Simple,
		/// <summary>
		/// Switch with single argument
		/// </summary>
		OneArg,
		/// <summary>
		/// Switch with a list of args
		/// </summary>
		List
	}
	/// <summary>
	/// Represents the result of parsing the command line arguments
	/// </summary>
#if CLILIB
	public
#endif
	class CmdParseResult : IDisposable
	{
		/// <summary>
		/// The ordinal arguments
		/// </summary>
		public List<object> OrdinalArguments;
		/// <summary>
		/// The named arguments
		/// </summary>
		public Dictionary<string, object> NamedArguments;
		/// <summary>
		/// Disposes any disposable arguments
		/// </summary>
		public void Dispose()
		{
			for (int i = 0; i < OrdinalArguments.Count; i++)
			{
				_DisposeArg(OrdinalArguments[i]);
			}
			foreach (var de in NamedArguments)
			{
				_DisposeArg(de.Value);
			}
		}

		private static void _DisposeArg(object arg)
		{
			if (arg != null && arg.GetType().IsArray)
			{
				var arr = (Array)arg;
				if (arr.Rank == 1)
				{
					for (int j = 0; j < arr.Length; ++j)
					{
						var v = arr.GetValue(j);
						if (v == Console.In || v == Console.Out || v == Console.Error)
						{
							continue;
						}
						var d = v as IDisposable;
						if (d != null)
						{
							d.Dispose();
						}
					}
				}
			}
			else if (arg is IDisposable)
			{
				if (arg != Console.Out && arg != Console.Error && arg != Console.In)
				{
					((IDisposable)arg).Dispose();
				}
			}
		}
	}
	/// <summary>
	/// Represents a command argument switch value
	/// </summary>
#if CLILIB
	public
#endif
	struct CmdSwitch
	{
		/// <summary>
		/// The name of the argument, if named
		/// </summary>
		public string Name;
		/// <summary>
		/// The ordinal of the argument, if ordinal, otherwise -1
		/// </summary>
		public int Ordinal;
		/// <summary>
		/// Indicates if the argument is optional
		/// </summary>
		public bool Optional;
		/// <summary>
		/// Indicates the default value
		/// </summary>
		public object Default;
		/// <summary>
		/// Indicates the type of switch
		/// </summary>
		public CmdSwitchType Type;
		/// <summary>
		/// Indicates the name of the element associated with the switch argument
		/// </summary>
		public string ElementName;
		/// <summary>
		/// Indicates the type of the element associated with the switch argument
		/// </summary>
		public Type ElementType;
		/// <summary>
		/// Indicates the <see cref="TypeConverter"/> to use to convert to and from an invariant string
		/// </summary>
		public TypeConverter ElementConverter;
		/// <summary>
		/// Indicates a description for the argument
		/// </summary>
		public string Description;
		/// <summary>
		/// Constructs a new instance
		/// </summary>
		/// <param name="name"><see cref="Name"/></param>
		/// <param name="ordinal"><see cref="Ordinal"/></param>
		/// <param name="optional"><see cref="Optional"/></param>
		/// <param name="default"><see cref="Default"/></param>
		/// <param name="type"><see cref="Type"/></param>
		/// <param name="elementName"><see cref="ElementName"/></param>
		/// <param name="elementType"><see cref="ElementType"/></param>
		/// <param name="elementConverter"><see cref="ElementConverter"/></param>
		/// <param name="description"><see cref="Description"/></param>
		public CmdSwitch(string name, int ordinal, bool optional, object @default, CmdSwitchType type, string elementName, Type elementType, TypeConverter elementConverter, string description)
		{
			Name = name;
			Ordinal = ordinal;
			Optional = optional;
			Default = @default;
			Type = type;
			ElementName = elementName;
			ElementType = elementType;
			ElementConverter = elementConverter;
			Description = description;
		}
		/// <summary>
		/// Returns a new empty instance
		/// </summary>
		public static CmdSwitch Empty { get => new CmdSwitch(null, -1, false, null, CmdSwitchType.OneArg, null, typeof(string), null, null); }
	}
	/// <summary>
	/// Indicates an attribute used to mark up static fields and properties to use as command line arguments
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
#if CLILIB
	public
#endif
	class CmdArgAttribute : Attribute
	{
		/// <summary>
		/// The name of the switch. Defaults to the member name
		/// </summary>
		public string Name { get; set; } = null;
		/// <summary>
		/// The ordinal. Default is named.
		/// </summary>
		public int Ordinal { get; set; } = -1;
		/// <summary>
		/// Indicates if the argument is optional
		/// </summary>
		public bool Optional { get; set; } = false;
		/// <summary>
		/// Indicates the name of the element associated with the argument
		/// </summary>
		public string ElementName { get; set; } = null;
		/// <summary>
		/// Indicates the <see cref="TypeConverter"/> used to convert elements to and from an invariant string
		/// </summary>
		public string ElementConverter { get; set; } = null;
		/// <summary>
		/// Indicates the description of the argument
		/// </summary>
		public string Description { get; set; } = null;
		/// <summary>
		/// Constructs a new instance
		/// </summary>
		/// <param name="name"><see cref="Name"/></param>
		/// <param name="ordinal"><see cref="Ordinal"/></param>
		/// <param name="optional"><see cref="Optional"/></param>
		/// <param name="elementName"><see cref="ElementName"/></param>
		/// <param name="elementConverter"><see cref="ElementConverter"/></param>
		/// <param name="description"><see cref="Description"/></param>
		public CmdArgAttribute(string name = null, int ordinal = -1, bool optional = false, string elementName = null, string elementConverter = null, string description = null)
		{
			Name = name;
			Ordinal = ordinal;
			Optional = optional;
			ElementName = elementName;
			Description = description;
		}
	}
	/// <summary>
	/// Provides command line argument parsing, stale file checking, usage screen generation and word wrapping facilities useful for CLI applications
	/// </summary>
#if CLILIB
	public
#endif
	static class CliUtility
	{
		#region _DeferredTextWriter
		private sealed class _DeferredTextWriter : TextWriter
		{
			readonly string _name;
			StreamWriter _writer = null;
			void EnsureWriter()
			{
				if (_writer == null)
				{
					_writer = new StreamWriter(_name, false, Encoding.UTF8);
				}
			}
			public override Encoding Encoding
			{
				get
				{
					if (_writer == null)
					{
						return Encoding.UTF8;
					}
					return _writer.Encoding;
				}
			}
			public _DeferredTextWriter(string path)
			{
				_name = path;
			}
			public string Name
			{
				get
				{
					return _name;
				}
			}
			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					if (_writer != null)
					{
						_writer.Close();
						_writer = null;
					}
				}
				base.Dispose(disposing);
			}
			public override void Close()
			{
				if (_writer != null)
				{
					_writer.Close();
				}
				base.Close();
			}
			public override void Write(string value)
			{
				EnsureWriter();
				_writer.Write(value);
			}
			public override void WriteLine(string value)
			{
				EnsureWriter();
				_writer.WriteLine(value);
			}
		}
		#endregion // _DeferredTextWriter
		#region _StringCursor
		private class _StringCursor
		{
			public IEnumerator<char> Input = null;
			public long Position = -1;
			public int State = -2;
			public string Current = null;
			public void EnsureStarted()
			{
				if (State == -2)
				{
					Advance();
				}
			}
			public string Advance()
			{
				if (State == -1)
				{
					return null;
				}
				++Position;
				if (!Input.MoveNext())
				{
					State = -1;
					Current = null;
					return null;
				}
				char c = Input.Current;
				if (char.IsSurrogate(c))
				{
					if (!Input.MoveNext())
					{
						throw new Exception("Incomplete Unicode surrogate found. Unexpected end of string.");
					}
					++Position;
					char c2 = Input.Current;
					Current = new string(new char[] { c, c2 });
				}
				else
				{
					Current = c.ToString();
				}
				State = 0;
				return Current;
			}
		}
		#endregion
		#region _ArrayCursor
		private class _ArrayCursor
		{
			public IEnumerator<string> Input = null;
			public int State = -2;
			public string Current = null;
			public void EnsureStarted()
			{
				if (State == -2)
				{
					Advance();
				}
			}
			public string Advance()
			{
				if (State == -1)
				{
					return null;
				}
				if (!Input.MoveNext())
				{
					State = -1;
					Current = null;
					return null;
				}
				Current = Input.Current;
				State = 0;
				return Current;
			}
		}
		#endregion
		static (string Value, bool Quoted) _ParseWithQuoted(_StringCursor cur)
		{
			var sb = new StringBuilder();
			cur.EnsureStarted();
			var inQuotes = false;
			while (cur.Current != null && char.IsWhiteSpace(cur.Current, 0))
			{
				cur.Advance();
			}
			if (cur.Current == "\"")
			{
				inQuotes = true;
				cur.Advance();
			}
			var moved = false;
			while (cur.State != -1)
			{
				if (!inQuotes)
				{
					if (cur.Current == "\"")
					{
						return (sb.ToString(), false);
					}
					if (char.IsWhiteSpace(cur.Current, 0))
					{
						cur.Advance();
						return (sb.ToString(), false);
					}
					sb.Append(cur.Current);
					cur.Advance();
					moved = true;
				}
				else
				{
					if (cur.Current == "\"")
					{
						cur.Advance();
						if (cur.Current != "\"")
						{
							return (sb.ToString(), true);
						}
					}
					sb.Append(cur.Current);
					cur.Advance();
					moved = true;
				}
			}
			if (inQuotes)
			{
				throw new Exception("Unterminated quote");
			}
			return (moved ? sb.ToString() : null, false);
		}
		static object _ValueFromString(string value, Type type, TypeConverter converter)
		{
			if (converter != null)
			{
				try
				{
					return converter.ConvertFromInvariantString(value);
				}
				catch { }
			}
			if (type == null || type == typeof(object) || type == typeof(string))
			{
				return value;
			}
			if (type == typeof(TextReader))
			{
				return new StreamReader(value);
			}
			else if (type == typeof(TextWriter))
			{
				return new _DeferredTextWriter(value);
			}
			if (type == typeof(Uri))
			{
				return new Uri(value);
			}
			if (type == typeof(DateTime))
			{
				return DateTime.Parse(value);
			}
			if (type == typeof(Guid))
			{
				return Guid.Parse(value);
			}
			if (type == typeof(TimeSpan))
			{
				return TimeSpan.Parse(value);
			}
			if (type == typeof(FileInfo))
			{
				return new FileInfo(value);
			}
			if (type == typeof(DirectoryInfo))
			{
				return new DirectoryInfo(value);
			}
			return Convert.ChangeType(value, type);
		}
		static string _ValueToString(object value, Type type, TypeConverter converter)
		{
			if (converter != null)
			{
				return converter.ConvertToInvariantString(value);
			}
			if (type == null || type == typeof(string))
			{
				return value as string;
			}
			if (type == typeof(TextReader))
			{
				if (value == Console.In)
				{
					return "<stdin>";
				}
				return "<#file>";
			}
			else if (type == typeof(TextWriter))
			{
				if (value == Console.Out)
				{
					return "<stdout>";
				}
				if (value == Console.Error)
				{
					return "<stderr>";
				}
				return "<#file>";
			}
			if (type == typeof(Uri))
			{
				return ((uint)value).ToString();
			}
			if (type == typeof(DateTime))
			{
				return ((DateTime)value).ToString();
			}
			if (type == typeof(Guid))
			{
				return ((Guid)value).ToString();
			}
			if (type == typeof(TimeSpan))
			{
				return ((TimeSpan)value).ToString();
			}
			if (type == typeof(FileInfo))
			{
				return ((FileInfo)value).FullName;
			}
			if (type == typeof(DirectoryInfo))
			{
				return ((DirectoryInfo)value).FullName;
			}
			return value.ToString();
		}
		/// <summary>
		/// Normalizes and validates a list of <see cref="CmdSwitch"/> instances
		/// </summary>
		/// <param name="switches">The switches to normalize</param>
		/// <exception cref="ArgumentException">Validation failed for one or more arguments</exception>
		/// <remarks>This is called by the framework automatically, but can be used by the user to perform validation earlier.</remarks>
		public static void NormalizeAndValidateSwitches(List<CmdSwitch> switches)
		{
			for (var i = 0; i < switches.Count; i++)
			{
				var sw = switches[i];
				if (sw.Ordinal > -1 && !string.IsNullOrEmpty(sw.Name))
				{
					throw new ArgumentException("Both ordinal and name cannot be specified.", sw.Name);
				}
				if (sw.Type == CmdSwitchType.Simple)
				{
					if (sw.Ordinal != -1)
					{
						throw new ArgumentException("Switch type of Simple must be named.");
					}
					sw.Optional = true;
				}
			}
			switches.Sort((lhs, rhs) =>
			{
				if (lhs.Ordinal < 0)
				{
					return rhs.Ordinal < 0 ? 0 : 1;
				}
				else
				{
					if (rhs.Ordinal < 0)
					{
						return -1;
					}
					if (lhs.Ordinal < rhs.Ordinal)
					{
						return -1;
					}
					else if (lhs.Ordinal > rhs.Ordinal)
					{
						return 1;
					}
				}
				return 0;
			});
			for (var i = 0; i < switches.Count; ++i)
			{
				var sw = switches[i];
				if (sw.Ordinal < 0) break;
				sw.Ordinal = i;
			}
			int ordCount = 0;
			for (int i = 0; i < switches.Count; ++i)
			{
				var sw = switches[i];
				if (sw.Ordinal < 0) break;
				ordCount++;
			}
			for (int i = 0; i < switches.Count; ++i)
			{
				var sw = switches[i];
				if (sw.Ordinal < 0) break;
				if (sw.Type == CmdSwitchType.List)
				{
					if (i < ordCount - 1)
					{
						throw new ArgumentException("Ordinal position list must be last among unnamed arguments");
					}
				}
				if (sw.Optional)
				{
					if (i < ordCount - 1)
					{
						throw new ArgumentException("Ordinal position optional value must be last among unnamed arguments");
					}
				}
			}
			for (int i = 0; i < switches.Count; ++i)
			{
				var sw = switches[i];
				if (string.IsNullOrEmpty(sw.ElementName) && sw.Type == CmdSwitchType.Simple)
				{
					sw.ElementName = sw.Name;
					sw.Optional = true;
				}
				if (string.IsNullOrEmpty(sw.ElementName))
				{
					if (sw.ElementType == typeof(TextReader))
					{
						sw.ElementName = "inputfile";
					}
					else if (sw.ElementType == typeof(TextWriter))
					{
						sw.ElementName = "outputfile";
					}
					else
					{
						sw.ElementName = "item";
					}
				}
				if (string.IsNullOrEmpty(sw.Description))
				{
					if (sw.ElementType == typeof(TextReader))
					{
						sw.Description = "The input file";
					}
					else if (sw.ElementType == typeof(TextWriter))
					{
						sw.Description = "The output file";
					}
					else
					{
						sw.Description = "";
					}
				}
				switches[i] = sw;
			}
		}
		static object _ParseArgValue(CmdSwitch sw, _StringCursor cur)
		{
			var v = _ParseWithQuoted(cur);
			if (v.Value == null)
			{
				throw new ArgumentException("No value found");
			}
			return _ValueFromString(v.Value, sw.ElementType, sw.ElementConverter);

		}
		static object _ParseArgValue(CmdSwitch sw, _ArrayCursor cur)
		{
			cur.EnsureStarted();
			var v = cur.Current;
			if (v== null)
			{
				throw new ArgumentException("No value found");
			}
			var result = _ValueFromString(v, sw.ElementType, sw.ElementConverter);
			cur.Advance();
			return result;

		}
		static Array _ParseArgValues(CmdSwitch sw, _StringCursor cur, string switchPrefix)
		{
			var result = new List<object>();
			while (true)
			{
				while (char.IsWhiteSpace(cur.Current, 0))
				{
					cur.Advance();
				}
				if (cur.Current != null && cur.Current.StartsWith(switchPrefix))
				{
					break;
				}
				var v = _ParseWithQuoted(cur);
				if (v.Value == null)
				{
					break;
				}
				object o = _ValueFromString(v.Value, sw.ElementType, sw.ElementConverter);


				result.Add(o);
			}
			Type t = sw.ElementType;
			if (t == null) t = typeof(string);
			var arr = Array.CreateInstance(t, result.Count);
			for(int i = 0;i<arr.Length;++i)
			{
				arr.SetValue(result[i], i);
			}
			return arr;
		}
		static Array _ParseArgValues(CmdSwitch sw, _ArrayCursor cur, string switchPrefix)
		{
			cur.EnsureStarted();
			var result = new List<object>();
			while (true)
			{
				if (cur.Current != null && cur.Current.StartsWith(switchPrefix))
				{
					break;
				}
				var v = cur.Current;
				if (v == null)
				{
					break;
				}
				object o = _ValueFromString(v, sw.ElementType, sw.ElementConverter);
				cur.Advance();


				result.Add(o);
			}
			Type t = sw.ElementType;
			if (t == null) t = typeof(string);
			var arr = Array.CreateInstance(t, result.Count);
			for (int i = 0; i < arr.Length; ++i)
			{
				arr.SetValue(result[i], i);
			}
			return arr;
		}
		/// <summary>
		/// Parses the executable path from the command line
		/// </summary>
		/// <param name="commandLine">The command line string</param>
		/// <returns>The executable path</returns>
		public static string ParseExePath(string commandLine)
		{
			_StringCursor cur = new _StringCursor() { Input = commandLine.GetEnumerator() };
			cur.EnsureStarted();
			return _ParseWithQuoted(cur).Value;

		}
		/// <summary>
		/// Parses command line arguments
		/// </summary>
		/// <param name="switches">A list of <see cref="CmdSwitch"/> instances describing the switches and arguments</param>
		/// <param name="commandLine">The command line to parse</param>
		/// <param name="switchPrefix">The prefix for switches</param>
		/// <returns>A <see cref="CmdParseResult"/> instance containing the parsed arguments</returns>
		public static CmdParseResult ParseArguments(List<CmdSwitch> switches, IEnumerable<string> commandLine, string switchPrefix = null)
		{
			if (string.IsNullOrEmpty(switchPrefix))
			{
				switchPrefix = SwitchPrefix;
			}
			_ArrayCursor cur = new _ArrayCursor () { Input = commandLine.GetEnumerator() };
			cur.EnsureStarted();
			var ords = new List<object>();
			var named = new Dictionary<string, object>();
			NormalizeAndValidateSwitches(switches);
			var i = 0;
			try
			{
				// process ordinal args
				for (; i < switches.Count; i++)
				{
					var c = cur.Current;
					var sw = switches[i];
					if (sw.Ordinal < 0)
					{
						break;
					}
					if (c == null || c.StartsWith(switchPrefix))
					{
						if (!sw.Optional)
						{
							throw new CmdException("At ordinal " + i.ToString() + ": Required argument missing", i);
						}
						else
						{
							ords.Add(sw.Default);
						}
						break;
					}
					switch (sw.Type)
					{
						case CmdSwitchType.OneArg:
							ords.Add(_ParseArgValue(sw, cur));
							break;
						case CmdSwitchType.List:
							ords.Add(_ParseArgValues(sw, cur, switchPrefix));
							break;

					}
				}
			}
			catch (CmdException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new CmdException("At ordinal " + i.ToString() + ": " + ex.Message, i, ex);
			}
			var argt = cur.Current;
			while (argt!= null)
			{
				if (!argt.StartsWith(switchPrefix))
				{
					throw new ArgumentException("Unexpected value when looking for a switch");
				}
				var name = argt.Substring(switchPrefix.Length);
				try
				{
					if (named.ContainsKey(name))
					{
						throw new CmdException("At switch " + name + ": Duplicate switch", name);
					}
					CmdSwitch sw = CmdSwitch.Empty;
					for (int j = 0; j < switches.Count; ++j)
					{
						sw = switches[j];
						if (sw.Name == name)
						{
							break;
						}

					}
					if (sw.Name == name)
					{
						cur.Advance();
						switch (sw.Type)
						{
							case CmdSwitchType.Simple:
								named.Add(name, true);
								break;
							case CmdSwitchType.OneArg:
								named.Add(name, _ParseArgValue(sw, cur));
								break;
							case CmdSwitchType.List:
								var v = _ParseArgValues(sw, cur, switchPrefix);
								if (v.Length == 0 && sw.Optional == false)
								{
									throw new CmdException("At switch " + sw.Name + ": Required argument not specified", sw.Name);
								}
								named.Add(name, v);
								break;

						}
					}
					else
					{
						throw new CmdException("At switch " + name + ": Unknown switch", sw.Name);
					}
					argt = cur.Current;
				}
				catch (CmdException)
				{
					throw;
				}
				catch (Exception ex)
				{
					throw new CmdException("At switch " + name + ": " + ex.Message, name, ex);
				}
			}
			for (i = 0; i < switches.Count; ++i)
			{
				var sw = switches[i];

				if (!string.IsNullOrEmpty(sw.Name))
				{
					if (sw.Optional == false && !named.ContainsKey(sw.Name))
					{
						throw new CmdException("At switch " + sw.Name + ": Required argument not specified", sw.Name);
					}
				}
			}
			return new CmdParseResult() { OrdinalArguments = ords, NamedArguments = named };
		}
		/// <summary>
		/// Parses command line arguments
		/// </summary>
		/// <param name="switches">A list of <see cref="CmdSwitch"/> instances describing the switches and arguments</param>
		/// <param name="commandLine">The command line to parse</param>
		/// <param name="switchPrefix">The prefix for switches</param>
		/// <returns>A <see cref="CmdParseResult"/> instance containing the parsed arguments</returns>
		/// <exception cref="ArgumentException">One of the arguments or the switch configuration is invalid</exception>
		public static CmdParseResult ParseArguments(List<CmdSwitch> switches, string commandLine = null, string switchPrefix = null)
		{
			if (string.IsNullOrEmpty(commandLine))
			{
				commandLine = Environment.CommandLine;
			}

			if (string.IsNullOrEmpty(switchPrefix))
			{
				switchPrefix = SwitchPrefix;
			}
			_StringCursor cur = new _StringCursor() { Input = commandLine.GetEnumerator() };
			cur.EnsureStarted();
			_ParseWithQuoted(cur);
			var ords = new List<object>();
			var named = new Dictionary<string, object>();
			NormalizeAndValidateSwitches(switches);
			var i = 0;
			try
			{
				// process ordinal args
				for (; i < switches.Count; i++)
				{
					var c = cur.Current;
					var sw = switches[i];
					if (sw.Ordinal < 0)
					{
						break;
					}
					if (c == null || c.StartsWith(switchPrefix))
					{
						if (!sw.Optional)
						{
							throw new CmdException("At ordinal "+i.ToString()+": Required argument missing",i);
						}
						else
						{
							ords.Add(sw.Default);
						}
						break;
					}
					switch (sw.Type)
					{
						case CmdSwitchType.OneArg:
							ords.Add(_ParseArgValue(sw, cur));
							break;
						case CmdSwitchType.List:
							ords.Add(_ParseArgValues(sw, cur, switchPrefix));
							break;

					}
				}
			}
			catch (CmdException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new CmdException("At ordinal " + i.ToString()+": "+ex.Message, i, ex);
			}
			var argt = _ParseWithQuoted(cur);
			while (argt.Value != null)
			{
				if (argt.Quoted || !argt.Value.StartsWith(switchPrefix))
				{
					throw new ArgumentException("Unexpected value when looking for a switch");
				}
				var name = argt.Value.Substring(switchPrefix.Length);
				try
				{


					if (named.ContainsKey(name))
					{
						throw new CmdException("At switch "+name+": Duplicate switch", name);
					}
					CmdSwitch sw = CmdSwitch.Empty;
					for (int j = 0; j < switches.Count; ++j)
					{
						sw = switches[j];
						if (sw.Name == name)
						{
							break;
						}

					}
					if (sw.Name == name)
					{
						switch (sw.Type)
						{
							case CmdSwitchType.Simple:
								named.Add(name, true);
								break;
							case CmdSwitchType.OneArg:
								named.Add(name, _ParseArgValue(sw, cur));
								break;
							case CmdSwitchType.List:
								var v = _ParseArgValues(sw, cur, switchPrefix);
								if (v.Length == 0 && sw.Optional == false)
								{
									throw new CmdException("At switch " + sw.Name + ": Required argument not specified", sw.Name);
								}
								named.Add(name, v);
								break;

						}
					}
					else
					{
						throw new CmdException("At switch " + name + ": Unknown switch", sw.Name);
					}
					argt = _ParseWithQuoted(cur);
				}
				catch(CmdException)
				{
					throw;
				}
				catch(Exception ex)
				{
					throw new CmdException("At switch "+name+": "+ex.Message,name,ex);
				}
			}
			for (i = 0; i < switches.Count; ++i)
			{
				var sw = switches[i];

				if (!string.IsNullOrEmpty(sw.Name))
				{
					if (sw.Optional == false && !named.ContainsKey(sw.Name))
					{
						throw new CmdException("At switch " + sw.Name + ": Required argument not specified", sw.Name);
					}
				}
			}
			return new CmdParseResult() { OrdinalArguments = ords, NamedArguments = named };
		}
		#region WordWrap
		/// <summary>
		/// Performs word wrapping
		/// </summary>
		/// <param name="text">The text to wrap</param>
		/// <param name="width">The width of the display. Tries to approximate if zero</param>
		/// <param name="indent">The indent for successive lines, in number of spaces</param>
		/// <param name="startOffset">The starting offset of the first line where the text begins</param>
		/// <returns>Wrapped text</returns>
		public static string WordWrap(string text, int width = 0, int indent = 0, int startOffset = 0)
		{
			if (width == 0)
			{
				width = Console.WindowWidth;
			}
			if (indent < 0) throw new ArgumentOutOfRangeException(nameof(indent));
			if (width < 0) throw new ArgumentOutOfRangeException(nameof(width));
			if (width > 0 && width < indent)
			{
				throw new ArgumentOutOfRangeException(nameof(width));
			}
			string[] words = text.Split(new string[] { " " },
				StringSplitOptions.None);

			StringBuilder result = new StringBuilder();
			double actualWidth = startOffset;
			for (int i = 0; i < words.Length; i++)
			{
				var word = words[i];
				if (i > 0)
				{
					if (actualWidth + word.Length >= width)
					{
						result.Append(Environment.NewLine);
						if (indent > 0)
						{
							result.Append(new string(' ', indent));
						}
						actualWidth = indent;
					}
					else
					{
						result.Append(' ');
						++actualWidth;
					}
				}
				result.Append(word.Replace('\u00A0',' '));
				actualWidth += word.Length;
			}
			return result.ToString();
		}

		#endregion // WordWrap
		/// <summary>
		/// Gets the command line argument portion of the usage information
		/// </summary>
		/// <param name="switches">A list of <see cref="CmdSwitch"/> instances</param>
		/// <param name="switchPrefix">The switch prefix to use</param>
		/// <param name="width">The width in characters</param>
		/// <param name="startOffset">The starting column where the arguments will be printed</param>
		/// <param name="nonBreaking">Returns with non-breaking spaces</param>
		/// <returns>A string indicating the usage arguments</returns>
		public static string GetUsageArguments(List<CmdSwitch> switches, string switchPrefix = null, int width = 0, int startOffset = 0, bool nonBreaking = false)
		{
			const int indent = 4;
			if (string.IsNullOrEmpty(switchPrefix))
			{
				switchPrefix = SwitchPrefix;
			}
			NormalizeAndValidateSwitches(switches);
			var sb = new StringBuilder();
			for (int i = 0; i < switches.Count; ++i)
			{
				if (i > 0)
				{
					sb.Append(" ");
				}
				var sw = switches[i];
				if (sw.Optional)
				{
					if (nonBreaking)
					{
						sb.Append("[\u00A0");
					}
					else
					{
						sb.Append("[ ");
					}
				}
				if (!string.IsNullOrEmpty(sw.Name))
				{
					sb.Append(switchPrefix);
					sb.Append(sw.Name);
					if (sw.Type != CmdSwitchType.Simple)
					{
						if (nonBreaking)
						{
							sb.Append('\u00A0');
						}
						else
						{
							sb.Append(' ');
						}
					}
				}
				switch (sw.Type)
				{
					case CmdSwitchType.OneArg:
						sb.Append("<");
						sb.Append(sw.ElementName);
						sb.Append(">");
						break;
					case CmdSwitchType.List:
						if (nonBreaking)
						{
							sb.Append("{\u00A0<");
						}
						else
						{
							sb.Append("{ <");
						}
						sb.Append(sw.ElementName);
						sb.Append("1>, ");
						sb.Append(" <");
						sb.Append(sw.ElementName);
						if (nonBreaking)
						{
							sb.Append("2>, ...\u00A0}");
						}
						else
						{
							sb.Append("2>, ... }");
						}
						break;
				}
				if (sw.Optional)
				{
					if (nonBreaking)
					{
						sb.Append("\u00A0]");
					}
					else
					{
						sb.Append(" ]");
					}
				}
			}
			return WordWrap(sb.ToString(), width, indent, startOffset);
		}
		/// <summary>
		/// Returns the assembly title as set by the <see cref="AssemblyTitleAttribute">
		/// </summary>
		public static string AssemblyTitle
		{
			get
			{
				var attr = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyTitleAttribute>();
				if (attr != null)
				{
					return attr.Title;
				}
				return null;
			}
		}
		/// <summary>
		/// Returns the assembly description as set by the <see cref="AssemblyDescriptionAttribute">
		/// </summary>
		public static string AssemblyDescription
		{
			get
			{
				var attr = Assembly	.GetEntryAssembly().GetCustomAttribute<AssemblyDescriptionAttribute>();
				if(attr != null)
				{
					return attr.Description;
				}
				return null;
			}
		}
		/// <summary>
		/// Indicates whether or not the operating system is Microsoft Windows based
		/// </summary>
		public static bool IsWindows
		{
			get
			{
				return (Environment.OSVersion.Platform == PlatformID.Win32NT || Environment.OSVersion.Platform == PlatformID.Win32Windows || Environment.OSVersion.Platform == PlatformID.Win32S);
			}
		}
		/// <summary>
		/// Gets the platform specific switch prefix
		/// </summary>
		public static string SwitchPrefix
		{
			get
			{
				try
				{
					return IsWindows ? "/" : "--";
				}
				catch
				{
					return "--";
				}
			}
		}
		/// <summary>
		/// Prints the usage screen
		/// </summary>
		/// <param name="switches">The list of switches</param>
		/// <param name="width">The width in characters to wrap to (defaults to console width)</param>
		/// <param name="writer">The writer to write to (defaults to stderr)</param>
		public static void PrintUsage(List<CmdSwitch> switches, int width = 0, TextWriter writer = null, string switchPrefix = null)
		{
			if (string.IsNullOrEmpty(switchPrefix))
			{
				switchPrefix = SwitchPrefix;
			}
			const int indent = 4;
			if (writer == null)
			{
				writer = Console.Error;
			}
			var asm = Assembly.GetEntryAssembly();
			string desc = null;
			string ver = null;
			string name = asm.GetName().Name;
			var asmVer = asm.GetName().Version;
			ver = asmVer.ToString();
			var asmTitle = AssemblyTitle;
			var asmDesc = AssemblyDescription;
			desc = string.IsNullOrEmpty(asmDesc) ? null : asmDesc;
			
			if (!string.IsNullOrEmpty(asmTitle))
			{
				name = asmTitle;
			}
			
			writer.WriteLine(WordWrap(name + " v" + ver, width, indent));
			writer.WriteLine();
			if (!string.IsNullOrEmpty(desc))
			{
				writer.WriteLine(WordWrap(desc, width, indent));
				writer.WriteLine();
			}
			var path = CliUtility.ParseExePath(Environment.CommandLine);
			var str = "Usage: " + Path.GetFileNameWithoutExtension(path) + " ";
			writer.Write(str);
			writer.WriteLine(CliUtility.GetUsageArguments(switches, switchPrefix, width, str.Length,true));
			writer.WriteLine();
			writer.WriteLine(CliUtility.GetUsageCommandDescription(switches, switchPrefix, width));
		}
		/// <summary>
		/// Retrieves the description portion of the usage information
		/// </summary>
		/// <param name="switches">The list if <see cref="CmdSwitch"/> instances</param>
		/// <param name="switchPrefix">The switch prefix to use</param>
		/// <param name="width">The width in characters</param>
		/// <returns>A string wrapped to the width containing a description for each switch</returns>
		public static string GetUsageCommandDescription(List<CmdSwitch> switches, string switchPrefix, int width = 0)
		{
			const int indent = 4;
			NormalizeAndValidateSwitches(switches);
			var left = new string(' ', indent);
			var max_len = 0;
			for (var i = 0; i < switches.Count; ++i)
			{
				var sw = switches[i];
				var len = sw.ElementName.Length;
				if (len > max_len)
				{
					max_len = len;
				}
			}
			var sb = new StringBuilder();
			var sbLine = new StringBuilder();
			for (var i = 0; i < switches.Count; ++i)
			{
				var sw = switches[i];
				sbLine.Clear();
				sbLine.Append(left);
				sbLine.Append('<');
				sbLine.Append(sw.ElementName);
				sbLine.Append('>');
				sbLine.Append(new string(' ', max_len - sw.ElementName.Length + 1));
				sbLine.Append(sw.Description);
				if (sw.Type != CmdSwitchType.Simple && sw.Default != null)
				{
					object val = sw.Default;
					if (sw.Type == CmdSwitchType.List)
					{
						Array arr = val as Array;
						if (arr != null && arr.Rank == 1 && arr.Length == 1)
						{
							val = arr.GetValue(0);
						}
					}
					if (string.IsNullOrEmpty(sw.Description) || sw.Description.IndexOf("default", StringComparison.InvariantCultureIgnoreCase) < 0)
					{
						string str = _ValueToString(val, sw.ElementType, sw.ElementConverter);
						if (!string.IsNullOrEmpty(sw.Description) && !sw.Description.TrimEnd().EndsWith("."))
						{
							sbLine.Append('.');
						}
						if (!string.IsNullOrEmpty(sw.Description))
						{
							sbLine.Append(' ');
						}
						sbLine.Append("Defaults to ");
						sbLine.Append(str);
					}
				}
				sb.AppendLine(WordWrap(sbLine.ToString(), width, indent * 2));
			}
			return sb.ToString();
		}
		private static object _ReflGetValue(MemberInfo m)
		{
			if (m is PropertyInfo)
			{
				return ((PropertyInfo)m).GetValue(null);
			}
			else if (m is FieldInfo)
			{
				return ((FieldInfo)m).GetValue(null);
			}
			return null;
		}
		private static void _ReflSetValue(MemberInfo m, object value)
		{
			if (m is PropertyInfo)
			{
				((PropertyInfo)m).SetValue(null, value);
			}
			else if (m is FieldInfo)
			{
				((FieldInfo)m).SetValue(null, value);
			}
		}
		/// <summary>
		/// Sets the values from a parse result to the command arg static fields on the specified type
		/// </summary>
		/// <param name="switches">The list of <see cref="CmdSwitch"/> instances</param>
		/// <param name="result">The parse result</param>
		/// <param name="type">The type to set the fields on</param>
		public static void SetValues(List<CmdSwitch> switches, CmdParseResult result, Type type)
		{
			var members = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			foreach (var member in members)
			{
				var cmdArg = member.GetCustomAttribute<CmdArgAttribute>();
				var found = false;
				object value = null;
				var cmdSw = CmdSwitch.Empty;
				Type mt = null;
				if (cmdArg != null)
				{
					if (member is PropertyInfo)
					{
						mt = ((PropertyInfo)member).PropertyType;
					}
					else
					{
						mt = ((FieldInfo)member).FieldType;
					}
					if (cmdArg.Ordinal > -1)
					{
						for (var i = 0; i < switches.Count; ++i)
						{
							var sw = switches[i];
							if (cmdArg.Ordinal == sw.Ordinal)
							{
								cmdSw = sw;
								found = true; break;
							}
						}
					}
					else
					{
						var n = member.Name;
						if (!string.IsNullOrEmpty(cmdArg.Name))
						{
							n = cmdArg.Name;
						}
						for (int i = 0; i < switches.Count; ++i)
						{
							var sw = switches[i];

							if (n == sw.Name)
							{
								cmdSw = sw;
								found = true; break;
							}
						}
					}
				}
				if (found)
				{
					value = cmdSw.Default;
					if (cmdSw.Ordinal > -1)
					{
						value = result.OrdinalArguments[cmdSw.Ordinal];
					}
					else
					{
						value = result.NamedArguments[cmdSw.Name];
					}
					if (cmdSw.Type == CmdSwitchType.List)
					{
						var newArr = Array.CreateInstance(mt.GetElementType(), ((Array)value).Length);
						for (var i = 0; i < ((Array)value).Length; ++i)
						{
							newArr.SetValue(((Array)value).GetValue(i), i);
						}
						_ReflSetValue(member, newArr);
					}
					else if (cmdSw.Type == CmdSwitchType.Simple)
					{
						_ReflSetValue(member, true);
					}
					else
					{
						_ReflSetValue(member, value);
					}
				}
			}
		}
		/// <summary>
		/// Retrieves all the switches defined as static fields or properties on a type
		/// </summary>
		/// <param name="type">The type to reflect</param>
		/// <returns>A list of <see cref="CmdSwitch"> instances based on the reflected members</returns>
		/// <exception cref="Exception">The attribute was on something other than a field or property</exception>
		public static List<CmdSwitch> GetSwitches(Type type)
		{
			var result = new List<CmdSwitch>();
			var members = type.GetMembers(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (var member in members)
			{
				var cmdArg = member.GetCustomAttribute<CmdArgAttribute>();
				if (cmdArg != null)
				{
					CmdSwitch cmdSwitch = CmdSwitch.Empty;
					if (cmdArg.Ordinal > -1)
					{
						cmdSwitch.Ordinal = cmdArg.Ordinal;
					}
					else
					{
						cmdSwitch.Name = cmdArg.Name;
						if (string.IsNullOrEmpty(cmdSwitch.Name))
						{
							cmdSwitch.Name = member.Name;
						}
					}
					cmdSwitch.Description = cmdArg.Description;
					cmdSwitch.Optional = cmdArg.Optional;
					cmdSwitch.ElementName = cmdArg.ElementName;

					if (!string.IsNullOrEmpty(cmdArg.ElementConverter))
					{
						
						var t = Type.GetType(cmdArg.ElementConverter,false,false);
						if (t == null)
						{
							t = Assembly.GetCallingAssembly().GetType(cmdArg.ElementConverter, true, false);
						}
						cmdSwitch.ElementConverter = Activator.CreateInstance(t) as TypeConverter;
						
					}

					if (member is PropertyInfo)
					{
						var pi = (PropertyInfo)member;
						if (pi.PropertyType.IsArray)
						{
							cmdSwitch.Type = CmdSwitchType.List;
							cmdSwitch.ElementType = pi.PropertyType.GetElementType();
						}
						else if (pi.PropertyType == typeof(bool))
						{
							cmdSwitch.Type = CmdSwitchType.Simple;
							cmdSwitch.ElementType = typeof(bool);
						}
						else
						{
							cmdSwitch.ElementType = pi.PropertyType;
						}
						try
						{
							cmdSwitch.Default = pi.GetValue(null);
						}
						catch { }
					}
					else if (member is FieldInfo)
					{
						var fi = (FieldInfo)member;
						if (fi.FieldType.IsArray)
						{
							cmdSwitch.Type = CmdSwitchType.List;
							cmdSwitch.ElementType = fi.FieldType.GetElementType();
						}
						else if (fi.FieldType == typeof(bool))
						{
							cmdSwitch.Type = CmdSwitchType.Simple;
							cmdSwitch.ElementType = typeof(bool);
						}
						else
						{
							cmdSwitch.ElementType = fi.FieldType;
						}
						try
						{
							cmdSwitch.Default = fi.GetValue(null);
						}
						catch { }
					}
					else
					{
						throw new Exception("Invalid attribute target");
					}
					result.Add(cmdSwitch);
				}

			}
			return result;
		}
		/// <summary>
		/// Parses, validates and sets fields and properties with the command line and target type
		/// </summary>
		/// <param name="targetType">The type with the static fields and/or properties to set</param>
		/// <param name="commandLine">The command line, or null to use the environment's current command line</param>
		/// <param name="width">The width in characters, or 0 to use the console window width</param>
		/// <param name="writer">The writer to write the help screen to or null to use stderr</param>
		/// <returns>The result of the parse</returns>
		public static CmdParseResult ParseValidateAndSet(Type targetType, string commandLine = null, int width = 0, TextWriter writer = null, string switchPrefix = null)
		{
			List<CmdSwitch> switches = null;
			CmdParseResult result = null;
			try
			{
				switches = GetSwitches(targetType);
				result = ParseArguments(switches,commandLine,switchPrefix);
				SetValues(switches, result, targetType);
				return result;
			}
			catch
			{
				if (switches != null) 
				{ 
					PrintUsage(switches, width, writer, switchPrefix);
				}
				throw;
			}
		}
		/// <summary>
		/// Parses, validates and sets fields and properties with the command line and target type
		/// </summary>
		/// <param name="targetType">The type with the static fields and/or properties to set</param>
		/// <param name="commandLine">The command line arguments</param>
		/// <param name="width">The width in characters, or 0 to use the console window width</param>
		/// <param name="writer">The writer to write the help screen to or null to use stderr</param>
		/// <returns>The result of the parse</returns>
		public static CmdParseResult ParseValidateAndSet(Type targetType, IEnumerable<string> commandLine, int width = 0, TextWriter writer = null, string switchPrefix = null)
		{
			List<CmdSwitch> switches = null;
			CmdParseResult result = null;
			try
			{
				switches = GetSwitches(targetType);
				result = ParseArguments(switches, commandLine, switchPrefix);
				SetValues(switches, result, targetType);
				return result;
			}
			catch
			{
				if (switches != null)
				{
					PrintUsage(switches, width, writer, switchPrefix);
				}
				throw;
			}
		}
		#region IsStale
		/// <summary>
		/// Indicates whether outputfile doesn't exist or is old
		/// </summary>
		/// <param name="inputfile">The master file to check the date of</param>
		/// <param name="outputfile">The output file which is compared against <paramref name="inputfile"/></param>
		/// <returns>True if <paramref name="outputfile"/> doesn't exist or is older than <paramref name="inputfile"/></returns>
		public static bool IsStale(string inputfile, string outputfile)
		{
			var result = true;
			// File.Exists doesn't always work right
			try
			{
				if (File.GetLastWriteTimeUtc(outputfile) >= File.GetLastWriteTimeUtc(inputfile))
					result = false;
			}
			catch { }
			return result;
		}
		/// <summary>
		/// Indicates whether outputfile doesn't exist or is old
		/// </summary>
		/// <param name="inputfile">The master file to check the date of</param>
		/// <param name="outputfile">The output file which is compared against <paramref name="inputfile"/></param>
		/// <returns>True if <paramref name="outputfile"/> doesn't exist or is older than <paramref name="inputfile"/></returns>
		public static bool IsStale(FileSystemInfo inputfile, FileSystemInfo outputfile)
		{
			var result = true;
			// File.Exists doesn't always work right
			try
			{
				if (File.GetLastWriteTimeUtc(outputfile.FullName) >= File.GetLastWriteTimeUtc(inputfile.FullName))
					result = false;
			}
			catch { }
			return result;
		}
		/// <summary>
		/// Indicates whether <paramref name="outputfile"/>'s file doesn't exist or is old
		/// </summary>
		/// <param name="inputfiles">The master files to check the date of</param>
		/// <param name="outputfile">The output file which is compared against each of the <paramref name="inputfiles"/></param>
		/// <returns>True if <paramref name="outputfile"/> doesn't exist or is older than <paramref name="inputfiles"/> or if any don't refer to a file</returns>
		public static bool IsStale(IEnumerable<FileSystemInfo> inputfiles, FileSystemInfo outputfile)
		{
			var result = true;
			foreach (var input in inputfiles)
			{
				result = false;
				if (IsStale(input, outputfile))
				{
					result = true;
					break;
				}
			}
			return result;
		}
		/// <summary>
		/// Indicates whether <paramref name="outputfile"/>'s file doesn't exist or is old
		/// </summary>
		/// <param name="inputfiles">The master files to check the date of</param>
		/// <param name="outputfile">The output file which is compared against each of the <paramref name="inputfiles"/></param>
		/// <returns>True if <paramref name="outputfile"/> doesn't exist or is older than <paramref name="inputfiles"/> or if any don't refer to a file</returns>
		public static bool IsStale(IEnumerable<FileInfo> inputfiles, FileInfo outputfile)
		{
			var result = true;
			foreach (var input in inputfiles)
			{
				result = false;
				if (IsStale(input, outputfile))
				{
					result = true;
					break;
				}
			}
			return result;
		}
		/// <summary>
		/// Indicates whether outputfile doesn't exist or is old
		/// </summary>
		/// <param name="input">The input reader to check the date of</param>
		/// <param name="output">The output writer which is compared against <paramref name="input"/></param>
		/// <returns>True if the file behind <paramref name="output"/> doesn't exist or is older than the file behind <paramref name="input"/> or if any are not files.</returns>
		public static bool IsStale(TextReader input, TextWriter output)
		{
			var result = true;
			var inputfile = GetFilename(input);
			if (inputfile == null)
			{
				return result;
			}
			var outputfile = GetFilename(output);
			if (outputfile == null)
			{
				return result;
			}
			// File.Exists doesn't always work right
			try
			{
				if (File.GetLastWriteTimeUtc(outputfile) >= File.GetLastWriteTimeUtc(inputfile))
					result = false;
			}
			catch { }
			return result;
		}
		/// <summary>
		/// Indicates whether <paramref name="output"/>'s file doesn't exist or is old
		/// </summary>
		/// <param name="inputs">The master files to check the date of</param>
		/// <param name="output">The output file which is compared against each of the <paramref name="inputs"/></param>
		/// <returns>True if <paramref name="output"/> doesn't exist or is older than <paramref name="inputs"/> or if any don't refer to a file</returns>
		public static bool IsStale(IEnumerable<TextReader> inputs, TextWriter output)
		{
			var result = true;
			foreach (var input in inputs)
			{
				result = false;
				if (IsStale(input, output))
				{
					result = true;
					break;
				}
			}
			return result;
		}
		#endregion // IsStale
		#region GetFilename
		/// <summary>
		/// Gets the filename for a <see cref="TextReader"/>if available
		/// </summary>
		/// <param name="t">The <see cref="TextReader"/> to examine</param>
		/// <returns>The filename, if available, or null</returns>
		public static string GetFilename(TextReader t)
		{
			var sr = t as StreamReader;
			string result = null;
			if (sr != null)
			{
				FileStream fstm = sr.BaseStream as FileStream;
				if (fstm != null)
				{
					result = fstm.Name;
				}
			}
			if (!string.IsNullOrEmpty(result))
			{
				return result;
			}
			return null;
		}
		/// <summary>
		/// Gets the filename for a <see cref="TextWriter"/>if available
		/// </summary>
		/// <param name="t">The <see cref="TextWriter"/> to examine</param>
		/// <returns>The filename, if available, or null</returns>
		public static string GetFilename(TextWriter t)
		{
			var dtw = t as _DeferredTextWriter;
			if (dtw != null)
			{
				return dtw.Name;
			}
			var sw = t as StreamWriter;
			string result = null;
			if (sw != null)
			{
				FileStream fstm = sw.BaseStream as FileStream;
				if (fstm != null)
				{
					result = fstm.Name;
				}
			}
			if (!string.IsNullOrEmpty(result))
			{
				return result;
			}
			return null;
		}
		#endregion GetFilename
	}
}