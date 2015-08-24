using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Jil;

namespace ApiMediaFormatters {
    /// <summary>
    ///     JIL media type formatter
    /// </summary>
    public class JilMediaTypeFormatter : MediaTypeFormatter {
        private static readonly MediaTypeHeaderValue ApplicationJsonMediaType = new MediaTypeHeaderValue("application/json");
        private static readonly MediaTypeHeaderValue TextJsonMediaType = new MediaTypeHeaderValue("text/json");
        private static readonly Task<bool> Done = Task.FromResult(true);

        private readonly Options _options;

        public JilMediaTypeFormatter(Options options) {
            _options = options;
            SupportedMediaTypes.Add(ApplicationJsonMediaType);
            SupportedMediaTypes.Add(TextJsonMediaType);

            SupportedEncodings.Add(new UTF8Encoding(false, true));
            SupportedEncodings.Add(new UnicodeEncoding(false, true, true));
        }

        public JilMediaTypeFormatter() : this(GetDefaultOptions()) { }

        private static Options GetDefaultOptions() {
            var opt = new Options(excludeNulls: true, dateFormat: DateTimeFormat.ISO8601);
            return opt;
        }

        public override bool CanReadType(Type type) { return true; }

        public override bool CanWriteType(Type type) { return true; }

        public override Task<object> ReadFromStreamAsync(Type type, Stream input, HttpContent content, IFormatterLogger formatterLogger) {
            var reader = new StreamReader(input);
            Func<TextReader, Options, object> deserialize = TypedDeserializers.GetTyped(type);
            object result = deserialize(reader, _options);
            return Task.FromResult(result);
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream output, HttpContent content, TransportContext transportContext) {
            var writer = new StreamWriter(output);
            JSON.Serialize(value, writer, _options);
            writer.Flush();
            return Done;
        }

        private static class TypedDeserializers {
            private static readonly ConcurrentDictionary<Type, Func<TextReader, Options, object>> _methods;
            private static readonly MethodInfo _method = typeof (JSON).GetMethod("Deserialize", new[] {typeof (TextReader), typeof (Options)});

            static TypedDeserializers() { _methods = new ConcurrentDictionary<Type, Func<TextReader, Options, object>>(); }

            public static Func<TextReader, Options, object> GetTyped(Type type) { return _methods.GetOrAdd(type, CreateDelegate); }

            private static Func<TextReader, Options, object> CreateDelegate(Type type) {
                return (Func<TextReader, Options, object>) _method.MakeGenericMethod(type).CreateDelegate(typeof (Func<TextReader, Options, object>));
            }
        }
    }
}