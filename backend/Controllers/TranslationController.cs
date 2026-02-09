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
        private readonly ITranslationRepository _repository;
        private readonly ILogger<TranslationController> _logger;

        public TranslationController(ITranslationRepository repository, ILogger<TranslationController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        // 1. List all SIDs (Returns a simple string list)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> List()
        {
            var sids = await _repository.GetAllSidsAsync();
            return Ok(sids);
        }

        // Get Resource (Mapped to DTO)
        [HttpGet("{sid}")]
        public async Task<ActionResult<TextResourceResponse>> Get(string sid)
        {
            var resource = await _repository.GetBySidAsync(sid);
            if (resource == null) return NotFound();

            // Manual mapping from Entity to DTO
            var response = MapToDto(resource);

            return Ok(response);
        }

        // Create Resource
        [HttpPost]
        public async Task<ActionResult<TextResourceResponse>> Create([FromBody] CreateTranslationRequest req)
        {
            var existing = await _repository.GetBySidAsync(req.Sid);
            if (existing != null) return Conflict(new { message = "SID already exists" });

            // Domain Logic: Create via Constructor
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

            _repository.DeleteBySidAsync(resource.Sid);
            await _repository.SaveChangesAsync();

            return NoContent();
        }

        // --- Private Helper for Mapping ---
        private static TextResourceResponse MapToDto(TextResourceEntity entity)
        {
            return new TextResourceResponse(
                entity.Sid,
                entity.Translations.Select(t => new TranslationResponse(t.LangId, t.Text))
            );
        }
    }


}
