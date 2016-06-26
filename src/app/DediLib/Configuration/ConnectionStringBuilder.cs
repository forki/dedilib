using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DediLib.Configuration
{
    public class ConnectionStringBuilder
    {
        private readonly Dictionary<string, object> _values =
            new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

        private ConnectionStringBuilder()
        {
        }

        public static ConnectionStringBuilder New() => new ConnectionStringBuilder();

        public static ConnectionStringBuilder Parse(string connectionString)
        {
            var builder = New();
            if (string.IsNullOrEmpty(connectionString))
                return builder;

            connectionString = connectionString.Trim();

            if (connectionString.Any(c => c < 32))
                throw new ArgumentException($"{connectionString} contains invalid characters");

            string nameStr;

            var quoteChar = '\0';
            var inName = true;
            var name = new StringBuilder();
            var value = new StringBuilder();
            foreach (var ch in connectionString)
            {
                if (inName)
                {
                    if (name.Length == 0 && ch == ' ')
                        continue;

                    if (ch == '=')
                    {
                        inName = false;
                        continue;
                    }

                    if (ch == ';')
                    {
                        nameStr = name.ToString().Trim();
                        if (!string.IsNullOrEmpty(nameStr))
                            builder.With(nameStr, null);
                        name.Clear();
                        continue;
                    }

                    name.Append(ch);
                }
                else
                {
                    if (quoteChar == '\0')
                    {
                        if (value.Length == 0 && (ch == '\'' || ch == '\"'))
                        {
                            quoteChar = ch;
                            continue;
                        }

                        if (value.Length == 0 && ch == ' ')
                            continue;

                        if (ch == ';')
                        {
                            builder.With(name.ToString().Trim(), value.ToString());
                            name.Clear();
                            value.Clear();
                            inName = true;
                            continue;
                        }

                        value.Append(ch);
                    }
                    else
                    {
                        if (ch == quoteChar)
                        {
                            quoteChar = '\0';
                            continue;
                        }

                        value.Append(ch);
                    }
                }
            }

            nameStr = name.ToString().Trim();
            if (!string.IsNullOrEmpty(nameStr))
            {
                var valueStr = value.ToString();
                builder.With(nameStr, valueStr != "" ? valueStr : null);
            }

            return builder;
        }

        public object Get(string name)
        {
            object value;
            if (_values.TryGetValue(name, out value))
                return value;
            return null;
        }

        public ConnectionStringBuilder With(string name, object value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (name == "") throw new ArgumentNullException($"{nameof(name)} cannot be an empty string");

            if (name.Any(c => !char.IsLetterOrDigit(c) && c != ' '))
                throw new ArgumentException($"{nameof(name)} must not contain illegal characters");
            if (!char.IsLetter(name.First()))
                throw new ArgumentException($"{nameof(name)} must start with a letter");

            if (value != null)
            {
                var valueString = value.ToString();
                if (valueString.Any(c => c < 32 || c == '\''))
                    throw new ArgumentException($"{nameof(value)} must not contain illegal characters");
            }

            _values[name] = value;
            return this;
        }

        public string Build()
        {
            var result = new StringBuilder();
            foreach (var kp in _values)
            {
                if (result.Length > 0) result.Append(';');
                result.Append(kp.Key);
                if (kp.Value == null) continue;

                result.Append('=');
                result.Append(Escape(kp.Value));
            }
            return result.ToString();
        }

        private string Escape(object value)
        {
            if (value == null)
                return null;

            var valueString = value.ToString();
            if (valueString.Any(c => c == '=' || c == ';') || valueString.StartsWith(" ") || valueString.EndsWith(" "))
                return '\'' + valueString + '\'';

            return valueString;
        }
    }
}
