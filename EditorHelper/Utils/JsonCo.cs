using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace EditorHelper.Utils {
    public static class JsonCo {
        public static IEnumerator Deserialize(string json, Func<object, IEnumerator> callback) {
            if (json == null) {
                yield return callback(null);
            } else {
                yield return Parser.Parse(json, callback);
            }
        }

        public static string Serialize(object obj) => Serializer.Serialize(obj);

        private sealed class Parser : IDisposable {
            private const string WHITE_SPACE = " \t\n\r";
            private const string WORD_BREAK = " \t\n\r{}[],:\"";
            private StringReader json;

            private Parser(string jsonString) => json = new StringReader(jsonString);

            public static IEnumerator Parse(string jsonString, Func<object, IEnumerator> callback) {
                using (var parser = new Parser(jsonString))
                    yield return parser.ParseValue(callback);
            }

            public void Dispose() {
                json.Dispose();
                json = (StringReader) null;
            }

            private IEnumerator ParseObject(Func<Dictionary<string, object>, IEnumerator> callback) {
                var dictionary = new Dictionary<string, object>();
                json.Read();
                while (true) {
                    TOKEN nextToken;
                    do {
                        nextToken = NextToken;
                        if (nextToken != TOKEN.NONE) {
                            if (nextToken == TOKEN.CURLY_CLOSE)
                                goto label_5;
                        } else
                            goto label_4;
                    } while (nextToken == TOKEN.COMMA);

                    string key = ParseString();
                    if (key != null) {
                        if (NextToken == TOKEN.COLON) {
                            json.Read();
                            yield return ParseValue(o => {
                                dictionary[key] = o;
                                return null;
                            });
                        } else
                            goto label_9;
                    } else
                        goto label_7;
                }

                label_5:
                yield return callback(dictionary);
                yield break;
                
                label_4:
                label_7:
                label_9:
                yield return callback(null);
            }

            private IEnumerator ParseArray(Func<List<object>, IEnumerator> callback) {
                var objectList = new List<object>();
                json.Read();
                yield return ParseArrayCo(objectList, callback);
            }

            public IEnumerator ParseArrayCo(List<object> objectList, Func<List<object>, IEnumerator> callback) {
                var nextToken = NextToken;
                switch (nextToken) {
                    case TOKEN.NONE:
                        yield return callback(null);
                        yield break;
                    case TOKEN.SQUARED_CLOSE:
                        yield break;
                    case TOKEN.COMMA:
                        yield return ParseArrayCo(objectList, callback);
                        break;
                    default:
                        IEnumerator Callback(object byToken) {
                            objectList.Add(byToken);
                            yield return ParseArrayCo(objectList, callback);
                        }

                        yield return ParseByToken(nextToken, Callback);
                        break;
                }
            }

            private IEnumerator ParseValue(Func<object, IEnumerator> callback) {
                yield return ParseByToken(NextToken, callback);
            }

            private IEnumerator ParseByToken(TOKEN token, Func<object, IEnumerator> callback) {
                switch (token) {
                    case TOKEN.CURLY_OPEN:
                        yield return ParseObject(callback);
                        yield break;
                    case TOKEN.SQUARED_OPEN:
                        yield return ParseArray(callback);
                        yield break;
                    case TOKEN.STRING:
                        yield return callback(ParseString());
                        yield break;
                    case TOKEN.NUMBER:
                        yield return callback(ParseNumber());
                        yield break;
                    case TOKEN.TRUE:
                        yield return callback(true);
                        yield break;
                    case TOKEN.FALSE:
                        yield return callback(false);
                        yield break;
                    case TOKEN.NULL:
                        yield return callback(null);
                        yield break;
                    default:
                        yield return callback(null);
                        yield break;
                }
            }

            private string ParseString() {
                var stringBuilder1 = new StringBuilder();
                json.Read();
                bool flag = true;
                while (flag) {
                    if (json.Peek() == -1)
                        break;
                    char nextChar1 = NextChar;
                    switch (nextChar1) {
                        case '"':
                            flag = false;
                            continue;
                        case '\\':
                            if (json.Peek() == -1) {
                                flag = false;
                                continue;
                            }

                            char nextChar2 = NextChar;
                            switch (nextChar2) {
                                case '"':
                                case '/':
                                case '\\':
                                    stringBuilder1.Append(nextChar2);
                                    continue;
                                case 'b':
                                    stringBuilder1.Append('\b');
                                    continue;
                                case 'f':
                                    stringBuilder1.Append('\f');
                                    continue;
                                case 'n':
                                    stringBuilder1.Append('\n');
                                    continue;
                                case 'r':
                                    stringBuilder1.Append('\r');
                                    continue;
                                case 't':
                                    stringBuilder1.Append('\t');
                                    continue;
                                case 'u':
                                    var stringBuilder2 = new StringBuilder();
                                    for (int index = 0; index < 4; ++index)
                                        stringBuilder2.Append(NextChar);
                                    stringBuilder1.Append((char) Convert.ToInt32(stringBuilder2.ToString(), 16));
                                    continue;
                                default:
                                    continue;
                            }
                        default:
                            stringBuilder1.Append(nextChar1);
                            continue;
                    }
                }

                return stringBuilder1.ToString();
            }

            private object ParseNumber() {
                string nextWord = NextWord;
                if (nextWord.IndexOf('.') == -1) {
                    int result;
                    int.TryParse(nextWord, out result);
                    return (object) result;
                }

                float result1;
                float.TryParse(nextWord, out result1);
                return (object) result1;
            }

            private void EatWhitespace() {
                while (" \t\n\r".IndexOf(PeekChar) != -1) {
                    json.Read();
                    if (json.Peek() == -1)
                        break;
                }
            }

            private char PeekChar => Convert.ToChar(json.Peek());

            private char NextChar => Convert.ToChar(json.Read());

            private string NextWord {
                get {
                    var stringBuilder = new StringBuilder();
                    while (" \t\n\r{}[],:\"".IndexOf(PeekChar) == -1) {
                        stringBuilder.Append(NextChar);
                        if (json.Peek() == -1)
                            break;
                    }

                    return stringBuilder.ToString();
                }
            }

            private TOKEN NextToken {
                get {
                    EatWhitespace();
                    if (json.Peek() == -1)
                        return TOKEN.NONE;
                    switch (PeekChar) {
                        case '"':
                            return TOKEN.STRING;
                        case ',':
                            json.Read();
                            return TOKEN.COMMA;
                        case '-':
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            return TOKEN.NUMBER;
                        case ':':
                            return TOKEN.COLON;
                        case '[':
                            return TOKEN.SQUARED_OPEN;
                        case ']':
                            json.Read();
                            return TOKEN.SQUARED_CLOSE;
                        case '{':
                            return TOKEN.CURLY_OPEN;
                        case '}':
                            json.Read();
                            return TOKEN.CURLY_CLOSE;
                        default:
                            string nextWord = NextWord;
                            if (nextWord == "false")
                                return TOKEN.FALSE;
                            if (nextWord == "true")
                                return TOKEN.TRUE;
                            return nextWord == "null" ? TOKEN.NULL : TOKEN.NONE;
                    }
                }
            }

            private enum TOKEN {
                NONE,
                CURLY_OPEN,
                CURLY_CLOSE,
                SQUARED_OPEN,
                SQUARED_CLOSE,
                COLON,
                COMMA,
                STRING,
                NUMBER,
                TRUE,
                FALSE,
                NULL,
            }
        }

        private sealed class Serializer {
            private StringBuilder builder;

            private Serializer() => builder = new StringBuilder();

            public static string Serialize(object obj) {
                var serializer = new Serializer();
                serializer.SerializeValue(obj);
                return serializer.builder.ToString();
            }

            private void SerializeValue(object value) {
                switch (value) {
                    case null:
                        builder.Append("null");
                        break;
                    case string str:
                        SerializeString(str);
                        break;
                    case bool _:
                        builder.Append(value.ToString().ToLower());
                        break;
                    case IList anArray:
                        SerializeArray(anArray);
                        break;
                    case IDictionary dictionary:
                        SerializeObject(dictionary);
                        break;
                    case char _:
                        SerializeString(value.ToString());
                        break;
                    default:
                        SerializeOther(value);
                        break;
                }
            }

            private void SerializeObject(IDictionary obj) {
                bool flag = true;
                builder.Append("{\n");
                foreach (object key in (IEnumerable) obj.Keys) {
                    if (!flag)
                        builder.Append(",\n");
                    SerializeString(key.ToString());
                    builder.Append(':');
                    SerializeValue(obj[key]);
                    flag = false;
                }

                builder.Append("\n}");
            }

            private void SerializeArray(IList anArray) {
                builder.Append('[');
                bool flag = true;
                foreach (object an in (IEnumerable) anArray) {
                    if (!flag)
                        builder.Append(',');
                    SerializeValue(an);
                    flag = false;
                }

                builder.Append(']');
            }

            private void SerializeString(string str) {
                builder.Append('"');
                foreach (char ch in str.ToCharArray()) {
                    switch (ch) {
                        case '\b':
                            builder.Append("\\b");
                            break;
                        case '\t':
                            builder.Append("\\t");
                            break;
                        case '\n':
                            builder.Append("\\n");
                            break;
                        case '\f':
                            builder.Append("\\f");
                            break;
                        case '\r':
                            builder.Append("\\r");
                            break;
                        case '"':
                            builder.Append("\\\"");
                            break;
                        case '\\':
                            builder.Append("\\\\");
                            break;
                        default:
                            int int32 = Convert.ToInt32(ch);
                            if (int32 >= 32 && int32 <= 126) {
                                builder.Append(ch);
                                break;
                            }

                            builder.Append("\\u" + Convert.ToString(int32, 16).PadLeft(4, '0'));
                            break;
                    }
                }

                builder.Append('"');
            }

            private void SerializeOther(object value) {
                switch (value) {
                    case float _:
                    case int _:
                    case uint _:
                    case long _:
                    case double _:
                    case sbyte _:
                    case byte _:
                    case short _:
                    case ushort _:
                    case ulong _:
                    case Decimal _:
                        builder.Append(value.ToString());
                        break;
                    default:
                        SerializeString(value.ToString());
                        break;
                }
            }
        }
    }
}