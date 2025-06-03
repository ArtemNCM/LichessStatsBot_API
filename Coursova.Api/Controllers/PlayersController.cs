using Coursova.Core.Models.DTOs;
using Coursova.Core.Models.Entities;
using Coursova.Core.Models.Requests;
using Coursova.Core;
using Microsoft.AspNetCore.Mvc;

namespace Coursova_Proga.Controllers
{
    [ApiController]
    [Route("api/players")]
    
    public class PlayersController : ControllerBase
    {
        private readonly ILichessService _lichessService;


        public PlayersController(ILichessService lichessService)
        {
            _lichessService = lichessService;
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<PlayerInfoDto>> GetPlayerInfo(string username)
        {
            var dto = await _lichessService.GetPlayerInfoAsync(username);
            if (dto is null)
                return NotFound(new { Message = $"Користувача '{username}' не знайдено" });

            return Ok(dto);
        }
        
        [HttpGet("{username}/games/pgn")]
        [Produces("text/plain")]
        public async Task<IActionResult> GetPlayerGamesPgn(
            string username,
            [FromQuery] int count = 5)
        {
            var pgns = await _lichessService.GetPlayerGamesPgnAsync(username, count);
            if (pgns == null || !pgns.Any())
                return NotFound(new { Message = $"PGN-партій для '{username}' не знайдено" });

            var allPgn = string.Join("\n\n", pgns);
            return Content(allPgn, "text/plain");
        }

        [HttpGet("{username}/openings")]
        public async Task<ActionResult<IEnumerable<OpeningDto>>> GetPlayerOpenings(
        string username,
        [FromQuery] string? color = null,  
        [FromQuery] int top = 3,
        [FromQuery] int fetch = 50)
        {
            var list = await _lichessService
                .GetPlayerOpeningsAsync(username, color, fetch, top);

            if (!list.Any())
                return NotFound(new { message = "Немає даних по дебютах" });

            return Ok(list);
        }

        [HttpGet("{username}/favorite-control")]
        public async Task<ActionResult<FavoriteControlDto>> GetPlayerFavoriteControl(string username)
        {
            var fav = await _lichessService.GetPlayerFavoriteControlAsync(username);
            if (fav.GamesCount == 0)
                return NotFound(new { Message = $"Немає даних для '{username}'" });

            return Ok(fav);
        }
        [HttpPost("compare")]
        public async Task<ActionResult<CompareStatsDto>> ComparePlayers([FromBody] ComparePlayersRequest req)
        {
            var result = await _lichessService.ComparePlayersAsync(req);
            if (result is null)
                return NotFound(new { Message = "Один із гравців не знайдений" });
            return Ok(result);
        }
        [HttpGet("{username}/performance")]
        public async Task<ActionResult<PerformanceDto>> GetPlayerPerformance(
        string username,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
        {
            if (to < from)
                return BadRequest(new { Message = "'to' має бути пізніше за 'from'" });

            var perf = await _lichessService.GetPlayerPerformanceAsync(username, from, to);
            return Ok(perf);
        }
        [HttpGet("{username}/rating-history")]
        public async Task<ActionResult<IEnumerable<ChartPointDto>>> GetRatingHistory(
        string username,
        [FromQuery(Name = "control")] string timeControl,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
        {
            if (to < from)
                return BadRequest(new { Message = "'to' має бути не раніше за 'from'" });

            var data = await _lichessService.GetRatingHistoryAsync(username, timeControl, from, to);
            if (!data.Any())
                return NotFound(new { Message = "Дані історії рейтингу не знайдені" });

            return Ok(data);
        }

        [HttpGet("{username}/rating-chart")]
        public async Task<IActionResult> GetRatingChart(
        string username,
        [FromQuery] string control,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
        {
            var img = await _lichessService.GetRatingHistoryChartAsync(username, control, from, to);
            if (img.Length == 0) return NotFound();

            return File(img, "image/png");
        }

    }

}
