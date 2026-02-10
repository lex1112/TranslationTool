using NUnit.Framework;
using Translation.Domain.Entities;
namespace Translation.Domain.Tests
{

    [TestFixture]
    public class TextResourceTests
    {
        private const string ValidSid = "GREETING_MSG";
        private const string ValidLang = "en-US";
        private const string ValidText = "Hello";

        [Test]
        public void Constructor_WithValidSid_ShouldInitializeCorrectly()
        {
            // Act
            var resource = new TextResourceEntity(ValidSid);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(resource.Sid, Is.EqualTo(ValidSid));
                Assert.That(resource.Id, Is.Not.EqualTo(Guid.Empty));
                Assert.That(resource.Translations, Is.Empty);
            });
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Constructor_WithInvalidSid_ShouldThrowArgumentException(string? invalidSid)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new TextResourceEntity(invalidSid!));
        }

        [Test]
        public void AddOrUpdateTranslation_WhenNew_ShouldAddToCollection()
        {
            // Arrange
            var resource = new TextResourceEntity(ValidSid);

            // Act
            resource.AddOrUpdateTranslation(ValidLang, ValidText);

            // Assert
            Assert.That(resource.Translations, Has.Count.EqualTo(1));
            var translation = resource.Translations.First();
            Assert.Multiple(() =>
            {
                Assert.That(translation.LangId, Is.EqualTo(ValidLang));
                Assert.That(translation.Text, Is.EqualTo(ValidText));
                Assert.That(translation.Sid, Is.EqualTo(ValidSid));
            });
        }

        [Test]
        public void AddOrUpdateTranslation_WhenExisting_ShouldUpdateTextAndNotAddDuplicate()
        {
            // Arrange
            var resource = new TextResourceEntity(ValidSid);
            resource.AddOrUpdateTranslation(ValidLang, "Old Text");

            // Act
            resource.AddOrUpdateTranslation(ValidLang, "New Text");

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(resource.Translations, Has.Count.EqualTo(1), "Should not create a second entry for the same language");
                Assert.That(resource.Translations.First().Text, Is.EqualTo("New Text"));
            });
        }

        [Test]
        public void AddOrUpdateTranslation_ShouldBeCaseInsensitiveForLanguageId()
        {
            // Arrange
            var resource = new TextResourceEntity(ValidSid);
            resource.AddOrUpdateTranslation("en-US", "Original");

            // Act - Adding with different casing
            resource.AddOrUpdateTranslation("EN-us", "Updated");

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(resource.Translations, Has.Count.EqualTo(1), "Should match language IDs regardless of case");
                Assert.That(resource.Translations.First().Text, Is.EqualTo("Updated"));
            });
        }

        [Test]
        public void AddOrUpdateTranslation_WithMultipleLanguages_ShouldMaintainSeparateEntries()
        {
            // Arrange
            var resource = new TextResourceEntity(ValidSid);

            // Act
            resource.AddOrUpdateTranslation("en-US", "Hello");
            resource.AddOrUpdateTranslation("de-DE", "Hallo");

            // Assert
            Assert.That(resource.Translations, Has.Count.EqualTo(2));
            Assert.That(resource.Translations.Any(t => t.LangId == "en-US"), Is.True);
            Assert.That(resource.Translations.Any(t => t.LangId == "de-DE"), Is.True);
        }

        [Test]
        public void TranslationsCollection_ShouldBeReadOnly()
        {
            // Arrange
            var resource = new TextResourceEntity(ValidSid);

            // Act & Assert
            // This ensures the property cannot be cast back to List<T> to bypass AddOrUpdateTranslation
            Assert.That(resource.Translations, Is.Not.InstanceOf<List<TranslationEntity>>());
        }

    }
}
