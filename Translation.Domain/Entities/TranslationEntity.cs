using System;
using System.Collections.Generic;
using System.Text;

namespace Translation.Domain.Entities
{
    public sealed class TranslationEntity
    {
        public int Id { get; private set; } // Internal DB Primary Key
        public string Sid { get; private set; } // Reference to parent via SID
        public string LangId { get; private set; } // e.g., "en-US"
        public string Text { get; private set; } // The localized string

        private TranslationEntity() { }


        // Internal ensures only TextResource can create Translations
        internal TranslationEntity(string sid, string langId, string text)
        {
            if (string.IsNullOrWhiteSpace(sid))
                throw new Exception("SID for translation is mandatory.");

            if (string.IsNullOrWhiteSpace(langId))
                throw new Exception("Language ID is mandatory.");

            // YOU MUST ADD THESE TWO LINES:
            this.Sid = sid;
            this.LangId = langId;

            this.Text = text ?? throw new Exception("Translation text cannot be null.");
        }

        internal void UpdateText(string newText) => Text = newText;
    }

}
