using System.Text.Json;


namespace TSL.Base.Platform.JsonConverters
{
    /// <summary>
    /// JsonSerializerOptionsSetting
    /// </summary>
    public static class JsonSerializerOptionsSetting
    {
        private static JsonSerializerOptions _option;

        public static JsonSerializerOptions Options => _option;

        /// <summary>
        /// 設定 SerializerOptions
        /// </summary>
        public static void SetOption()
        {
            _option = new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            _option.Converters.Add(new DictionaryTKeyNonStringConverter());
        }
    }
}
