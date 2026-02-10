using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using Translation.Domain.Entities;
using Translation.Infrastructure.Repositories;
using translation_app.Dto;

namespace translation_app.Controllers
{

    [ApiController]
    [Route("api/translations")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public class TranslationController : ControllerBase
    {
        private readonly ITextResourceRepository _repository;

        public TranslationController(ITextResourceRepository repository)
        {
            _repository = repository;
        }

        // List all SIDs (Returns a simple string list)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TextResourceResponse>>> List()
        {
            var entities = await _repository.GetAllTextResource();

            // Map each entity to DTO
            var dtos = entities.Select(MapToDto);

            return Ok(dtos);
        }

        // Get Resource (Mapped to DTO)
        [HttpGet("{sid}")]
        public async Task<ActionResult<TextResourceResponse>> Get(string sid)
        {
            var resource = await _repository.GetBySidAsync(sid);
            if (resource == null) return NotFound();

            // Mapping from Entity to DTO
            var response = MapToDto(resource);

            return Ok(response);
        }

        // Create Resource
        [HttpPost]
        public async Task<ActionResult<TextResourceResponse>> Create([FromBody] CreateTranslationRequest req)
        {
            var existing = await _repository.GetBySidAsync(req.Sid);
            if (existing != null) return Conflict(new { message = "SID already exists" });

            var resource = new TextResourceEntity(req.Sid);
            resource.AddOrUpdateTranslation("default", req.DefaultText);

            await _repository.AddAsync(resource);
            await _repository.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { sid = resource.Sid }, MapToDto(resource));
        }

        // Update/Add Translation
        [HttpPut("{sid}/{langId}")]
        public async Task<IActionResult> Update(string sid, string langId, [FromBody] UpdateTranslationRequest req)
        {
            var resource = await _repository.GetBySidAsync(sid);
            if (resource == null) return NotFound();

            // Domain Logic: Encapsulated update
            resource.AddOrUpdateTranslation(langId, req.Text);

            await _repository.SaveChangesAsync();
            return NoContent();
        }

        // Delete
        [HttpDelete("{sid}")]
        public async Task<IActionResult> Delete(string sid)
        {
            var resource = await _repository.GetBySidAsync(sid);
            if (resource == null) return NotFound();

            await _repository.DeleteBySidAsync(resource.Sid);
            await _repository.SaveChangesAsync();

            return NoContent();
        }

        private static TextResourceResponse MapToDto(TextResourceEntity entity)
        {
            return new TextResourceResponse(
                entity.Sid,
                entity.Translations.Select(t => new TranslationResponse(t.LangId, t.Text))
            );
        }
    }
}
