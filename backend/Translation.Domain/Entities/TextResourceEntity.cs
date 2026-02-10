using System;
using System.Collections.Generic;
using System.Text;

namespace Translation.Domain.Entities
{
    public sealed class TextResourceEntity
    {
        public Guid Id { get; private set; } // Technical ID for DB
        public string Sid { get; private set; } // Business Key (e.g., "GREETING_MSG")

        // Encapsulated collection of localized versions
        private readonly List<TranslationEntity> _translations = new();
        public IReadOnlyCollection<TranslationEntity> Translations => _translations.AsReadOnly();

        public TextResourceEntity(string sid)
        {
            if (string.IsNullOrWhiteSpace(sid)) throw new ArgumentException("SID is required.");
            Id = Guid.NewGuid();
            Sid = sid;
        }

        public void AddOrUpdateTranslation(string langId, string text)
        {
            var existing = _translations.FirstOrDefault(t =>
                string.Equals(t.LangId, langId, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                existing.UpdateText(text);
            }
            else
            {
                _translations.Add(new TranslationEntity(Sid, langId, text));
            }
        }
    }
}
