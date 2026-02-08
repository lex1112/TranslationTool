namespace translation_app.Models
{
    namespace TranslationTool.Model
    {
        public class CreateRequest
        {
            // These property names must match the JSON keys
            public string Sid { get; set; } = string.Empty;
            public string DefaultText { get; set; } = string.Empty;
        }
    }

}
