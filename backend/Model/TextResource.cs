using System.ComponentModel.DataAnnotations;

namespace translation_app.Model
{
    public sealed class TextResource
    {
        [Key]
        public string Sid { get; set; } = string.Empty; // Primary Key

        // Navigation property: One SID has many translations
        public List<Translation> Translations { get; set; } = new();
    }

}
