using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using translation_app.Model;
using translation_app.Models.TranslationTool.Model;
using translation_app.Provider;

namespace translation_app.Controllers
{
    [ApiController]
    [Route("api/translations")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public class TranslationController : ControllerBase
    {
        private readonly ITranslationProvider _provider;
        private readonly ILogger<TranslationController> _logger;

        public TranslationController(ITranslationProvider provider, ILogger<TranslationController> logger)
        {
            _provider = provider;
            _logger = logger;
        }

        // 1. List all SIDs
        [HttpGet]
        public async Task<IActionResult> List()
        {
            try
            {
                var sids = await _provider.GetAllSidsAsync();
                return Ok(sids);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching SID list");
                return StatusCode(500, "Internal server error occurred while fetching SIDs.");
            }
        }

        [HttpGet("{sid}")]
        public async Task<ActionResult<TextResource>> Get(string sid) 
        {
            var resource = await _provider.GetDetailsAsync(sid);

            if (resource == null)
            {
                return NotFound();
            }

            return Ok(resource); 
        }

        // 3. Create a new SID
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRequest req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                await _provider.CreateSidAsync(req.Sid, req.DefaultText);
                return CreatedAtAction(nameof(Get), new { sid = req.Sid }, req);
            }
            catch (InvalidOperationException ex) 
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating SID: {Sid}", req.Sid);
                return StatusCode(500, "Failed to create new translation resource.");
            }
        }

        // 4. Update translation
        [HttpPut("{sid}/{langId}")]
        public async Task<IActionResult> Update(string sid, string langId, [FromBody] string text)
        {
            try
            {
                await _provider.UpdateTranslationAsync(sid, langId, text);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating {LangId} for {Sid}", langId, sid);
                return StatusCode(500, "Update failed.");
            }
        }

        // 5. Delete SID
        [HttpDelete("{sid}")]
        public async Task<IActionResult> Delete(string sid)
        {
            try
            {
                await _provider.DeleteSidAsync(sid);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting SID: {Sid}", sid);
                return StatusCode(500, "Deletion failed.");
            }
        }
    }


}
