/*
 * MiniJSON — Lightweight JSON serializer/deserializer for Unity.
 *
 * Deserializes JSON into:
 *   Dictionary<string, object>  (object)
 *   List<object>                (array)
 *   string, bool, null
 *   long (integer numbers), double (floating-point numbers)
 *
 * Serializes Dictionary<string,object>, List<object>, string,
 * bool, null, int, long, float, double, and any IEnumerable.
 *
 * Usage:
 *   object obj   = Json.Deserialize(jsonString);
 *   string json  = Json.Serialize(obj);
 *
 * Original work Copyright (c) 2013 Calvin Rien (MIT License).
 * Modified to be fully self-contained in the UnityMCP namespace.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UnityMCP
{
    public static class Json
    {
        // ── Public API ──────────────────────────────────────────────────────────

        /// <summary>Deserialize a JSON string into .NET objects.</summary>
        public static object Deserialize(string json)
        {
            if (json == null) return null;
            return Parser.Parse(json);
        }

        /// <summary>Serialize a .NET object graph into a JSON string.</summary>
        public static string Serialize(object obj)
        {
            return Serializer.Serialize(obj);
        }

        // ── Parser ───────────────────────────────────────────────────────────────

        sealed class Parser : IDisposable
        {
            const string WordBreak = "{}[],:\"";

            StringReader _json;

            Parser(string jsonString) { _json = new StringReader(jsonString); }

            public static object Parse(string jsonString)
            {
                using (var instance = new Parser(jsonString))
                    return instance.ParseValue();
            }

            public void Dispose() { _json.Dispose(); }

            Dictionary<string, object> ParseObject()
            {
                var table = new Dictionary<string, object>();
                _json.Read(); // {
                while (true)
                {
                    switch (NextToken)
                    {
                        case Token.None: return null;
                        case Token.Comma: continue;
                        case Token.CurlyClose: return table;
                        default:
                            string key = ParseString();
                            if (key == null) return null;
                            if (NextToken != Token.Colon) return null;
                            _json.Read();
                            table[key] = ParseValue();
                            break;
                    }
                }
            }

            List<object> ParseArray()
            {
                var array = new List<object>();
                _json.Read(); // [
                bool parsing = true;
                while (parsing)
                {
                    switch (NextToken)
                    {
                        case Token.None: return null;
                        case Token.Comma: continue;
                        case Token.SquaredClose: parsing = false; break;
                        default: array.Add(ParseByToken(NextToken)); break;
                    }
                }
                return array;
            }

            object ParseValue() => ParseByToken(NextToken);

            object ParseByToken(Token token)
            {
                switch (token)
                {
                    case Token.String: return ParseString();
                    case Token.Number: return ParseNumber();
                    case Token.CurlyOpen: return ParseObject();
                    case Token.SquaredOpen: return ParseArray();
                    case Token.True: return true;
                    case Token.False: return false;
                    case Token.Null: return null;
                    default: return null;
                }
            }

            string ParseString()
            {
                var s = new StringBuilder();
                _json.Read(); // opening "
                bool parsing = true;
                while (parsing)
                {
                    if (_json.Peek() == -1) break;
                    char c = NextChar;
                    switch (c)
                    {
                        case '"': parsing = false; break;
                        case '\\':
                            if (_json.Peek() == -1) { parsing = false; break; }
                            char esc = NextChar;
                            switch (esc)
                            {
                                case '"': case '\\': case '/': s.Append(esc); break;
                                case 'b': s.Append('\b'); break;
                                case 'f': s.Append('\f'); break;
                                case 'n': s.Append('\n'); break;
                                case 'r': s.Append('\r'); break;
                                case 't': s.Append('\t'); break;
                                case 'u':
                                    var hex = new StringBuilder();
                                    for (int i = 0; i < 4; i++) hex.Append(NextChar);
                                    s.Append((char)Convert.ToInt32(hex.ToString(), 16));
                                    break;
                            }
                            break;
                        default: s.Append(c); break;
                    }
                }
                return s.ToString();
            }

            object ParseNumber()
            {
                string numStr = NextWord;
                if (numStr.IndexOf('.') == -1 && numStr.IndexOf('E') == -1 && numStr.IndexOf('e') == -1)
                {
                    long val;
                    long.TryParse(numStr, out val);
                    return val;
                }
                double dval;
                double.TryParse(numStr, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out dval);
                return dval;
            }

            void EatWhitespace() { while (char.IsWhiteSpace((char)_json.Peek())) _json.Read(); }

            char NextChar => (char)_json.Read();

            Token NextToken
            {
                get
                {
                    EatWhitespace();
                    if (_json.Peek() == -1) return Token.None;
                    switch ((char)_json.Peek())
                    {
                        case '{': return Token.CurlyOpen;
                        case '}': _json.Read(); return Token.CurlyClose;
                        case '[': return Token.SquaredOpen;
                        case ']': _json.Read(); return Token.SquaredClose;
                        case ',': _json.Read(); return Token.Comma;
                        case '"': return Token.String;
                        case ':': return Token.Colon;
                        case '0': case '1': case '2': case '3': case '4':
                        case '5': case '6': case '7': case '8': case '9':
                        case '-': return Token.Number;
                        case 't': return Token.True;
                        case 'f': return Token.False;
                        case 'n': return Token.Null;
                    }
                    _json.Read();
                    return Token.None;
                }
            }

            string NextWord
            {
                get
                {
                    var word = new StringBuilder();
                    while (WordBreak.IndexOf((char)_json.Peek()) == -1)
                    {
                        word.Append(NextChar);
                        if (_json.Peek() == -1) break;
                    }
                    return word.ToString();
                }
            }

            enum Token { None, CurlyOpen, CurlyClose, SquaredOpen, SquaredClose, Colon, Comma, String, Number, True, False, Null }
        }

        // ── Serializer ────────────────────────────────────────────────────────────

        sealed class Serializer
        {
            StringBuilder _builder;

            Serializer() { _builder = new StringBuilder(); }

            public static string Serialize(object obj)
            {
                var s = new Serializer();
                s.SerializeValue(obj);
                return s._builder.ToString();
            }

            void SerializeValue(object value)
            {
                if (value == null) { _builder.Append("null"); return; }
                if (value is string str) { SerializeString(str); return; }
                if (value is bool b) { _builder.Append(b ? "true" : "false"); return; }
                if (value is IDictionary dict) { SerializeObject(dict); return; }
                if (value is IList list) { SerializeArray(list); return; }
                if (value is IEnumerable en) { SerializeArray(en); return; }
                // Numbers
                if (value is int || value is long || value is uint || value is ulong ||
                    value is byte || value is sbyte || value is short || value is ushort)
                {
                    _builder.Append(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture));
                    return;
                }
                if (value is float || value is double || value is decimal)
                {
                    _builder.Append(Convert.ToDouble(value).ToString("R", System.Globalization.CultureInfo.InvariantCulture));
                    return;
                }
                SerializeString(value.ToString());
            }

            void SerializeObject(IDictionary obj)
            {
                _builder.Append('{');
                bool first = true;
                foreach (DictionaryEntry entry in obj)
                {
                    if (!first) _builder.Append(',');
                    SerializeString(entry.Key.ToString());
                    _builder.Append(':');
                    SerializeValue(entry.Value);
                    first = false;
                }
                _builder.Append('}');
            }

            void SerializeArray(IEnumerable arr)
            {
                _builder.Append('[');
                bool first = true;
                foreach (var item in arr)
                {
                    if (!first) _builder.Append(',');
                    SerializeValue(item);
                    first = false;
                }
                _builder.Append(']');
            }

            void SerializeString(string str)
            {
                _builder.Append('"');
                foreach (char c in str)
                {
                    switch (c)
                    {
                        case '"':  _builder.Append("\\\""); break;
                        case '\\': _builder.Append("\\\\"); break;
                        case '\b': _builder.Append("\\b");  break;
                        case '\f': _builder.Append("\\f");  break;
                        case '\n': _builder.Append("\\n");  break;
                        case '\r': _builder.Append("\\r");  break;
                        case '\t': _builder.Append("\\t");  break;
                        default:
                            if (c < 0x20)
                                _builder.AppendFormat("\\u{0:X4}", (int)c);
                            else
                                _builder.Append(c);
                            break;
                    }
                }
                _builder.Append('"');
            }
        }
    }
}
