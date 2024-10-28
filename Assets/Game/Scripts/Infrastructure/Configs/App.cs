using Newtonsoft.Json;

namespace Game.Scripts.Infrastructure
{
    public static partial class App
    {
        public static readonly JsonSerializerSettings JsonSettings = new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,

            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,

            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
        };

        public static readonly string[] DateTimeParseExactFormats = 
        {
            "yyyy/MM/dd HH:mm:ss",
            "yyyy/MM/d HH:mm:ss",
            "yyyy/M/dd HH:mm:ss",
            "yyyy/M/d HH:mm:ss",
            "yyyy/MM/dd HH:mm",
            "yyyy/MM/d HH:mm",
            "yyyy/M/dd HH:mm",
            "yyyy/M/d HH:mm"
        };
        public const char ListDivider = ';';
    }
}