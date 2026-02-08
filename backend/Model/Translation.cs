using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace translation_app.Model
{

    public sealed class Translation
    {
        public int Id { get; set; } // Internal Primary Key

        [Required]
        public string Sid { get; set; } = string.Empty; // Foreign Key

        [Required]
        public string LangId { get; set; } = string.Empty; // e.g., "en", "de"

        [Required]
        public string Text { get; set; } = string.Empty;

        // Optional: Navigation property back to parent
        [JsonIgnore]
        public TextResource? TextResource { get; set; }
    }

}
