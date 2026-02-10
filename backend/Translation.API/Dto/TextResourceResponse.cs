namespace translation_app.Dto
{
    public record TextResourceResponse(
        string Sid,
        IEnumerable<TranslationResponse> Translations
    );


}
