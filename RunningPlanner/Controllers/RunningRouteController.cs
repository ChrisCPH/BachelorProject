using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RunningPlanner.Models;
using RunningPlanner.Services;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class RunningRouteController : ControllerBase
{
    private readonly IRunningRouteService _runningRouteService;

    public RunningRouteController(IRunningRouteService runningRouteService)
    {
        _runningRouteService = runningRouteService;
    }

    [HttpGet("getAll")]
    public async Task<ActionResult<List<RunningRoute>>> GetAll()
    {
        var routes = await _runningRouteService.GetAllAsync();
        return Ok(routes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RunningRoute>> GetById(string id)
    {
        try
        {
            var route = await _runningRouteService.GetByIdAsync(id);
            return Ok(route);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("add")]
    public async Task<ActionResult> Add([FromBody] RunningRoute route)
    {
        if (route == null)
        {
            return BadRequest("Route data is required.");
        }

        var userIdClaim = User.FindFirst("UserID");
        if (userIdClaim == null)
        {
            return Unauthorized("User ID not found in token.");
        }

        int userId = int.Parse(userIdClaim.Value);

        var routeWithUserID = new RunningRoute
        {
            ID = route.ID,
            UserID = userId,
            Name = route.Name,
            Geometry = route.Geometry,
            CreatedAt = route.CreatedAt,
            DistanceKm = route.DistanceKm
        };

        await _runningRouteService.AddAsync(routeWithUserID);
        return CreatedAtAction(nameof(GetById), new { id = routeWithUserID.ID }, routeWithUserID);
    }

    [HttpPut("update")]
    public async Task<ActionResult> Update([FromBody] RunningRoute route)
    {
        if (route == null)
        {
            return BadRequest("Route data is required.");
        }

        var userIdClaim = User.FindFirst("UserID");
        if (userIdClaim == null)
        {
            return Unauthorized("User ID not found in token.");
        }

        int userId = int.Parse(userIdClaim.Value);

        var routeWithUserID = new RunningRoute
        {
            ID = route.ID,
            UserID = userId,
            Name = route.Name,
            Geometry = route.Geometry,
            CreatedAt = route.CreatedAt,
            DistanceKm = route.DistanceKm
        };

        try
        {
            await _runningRouteService.UpdateAsync(routeWithUserID.ID, routeWithUserID);
            return CreatedAtAction(nameof(GetById), new { id = routeWithUserID.ID }, routeWithUserID);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("delete/{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        try
        {
            await _runningRouteService.DeleteAsync(id);
            return Ok("Running Route with id: " + id + " deleted");
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("nearby")]
    public async Task<ActionResult<List<RunningRoute>>> GetRoutesNearPoint([FromBody] GeoPoint point)
    {
        if (point == null)
            return BadRequest("Point data is required.");

        var routes = await _runningRouteService.GetRoutesNearPointAsync(point.Longitude, point.Latitude, point.MaxDistanceMeters);

        return Ok(routes);
    }

    [HttpPost("within")]
    public async Task<ActionResult<List<RunningRoute>>> GetRoutesWithinPolygon([FromBody] Polygon polygon)
    {
        if (polygon == null || polygon.Coordinates.Count < 3)
        {
            return BadRequest("A valid polygon with at least 3 coordinates is required.");
        }

        var routes = await _runningRouteService.GetRoutesWithinPolygonAsync(polygon);
        return Ok(routes);
    }

    [HttpPost("intersect")]
    public async Task<ActionResult<List<RunningRoute>>> GetRoutesIntersectingPolygon([FromBody] Polygon polygon)
    {
        if (polygon?.Coordinates == null || polygon.Coordinates.Count < 4)
        {
            return BadRequest("Polygon coordinates must have at least 4 points (including closing point).");
        }

        var routes = await _runningRouteService.GetRoutesIntersectingPolygonAsync(polygon.Coordinates);
        return Ok(routes);
    }
}
