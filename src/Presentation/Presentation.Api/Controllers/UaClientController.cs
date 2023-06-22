using DataCollectors.OPCUA.Core.Application.UaClient.Queries;
using DataCollectors.OPCUA.Core.Application.UaClient.Responses;
using Microsoft.AspNetCore.Mvc;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;

namespace DataCollectors.OPCUA.Presentation.Api.Controllers;

[Route("api/[controller]")]
public class UaClientController : BaseController
{
    /// <summary>
    /// Gets the server info.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The server info.</returns>
    [HttpGet("Server")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ServerInfoDto>> GetServerInfo(CancellationToken cancellationToken)
    {
        var response = await Mediator.Send(new GetServerInfoQuery(), cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Gets a list of nodes from the UA Client.
    /// </summary>
    /// <param name="query">The query containing the node and tag name.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A list of Nodes.</returns>
    [HttpGet("BrowseNode")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IList<NodeDto>>> GetNodes([FromQuery] BrowseNodeQuery query, CancellationToken cancellationToken)
    {
        var response = await Mediator.Send(query, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Gets node value.
    /// </summary>
    /// <param name="query">The query containing the node and tag name.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A list of Nodes.</returns>
    [HttpGet("Node")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<NodeValueDto>> GetNode([FromQuery] GetNodeValueQuery query, CancellationToken cancellationToken)
    {
        var response = await Mediator.Send(query, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Gets fields for a node.
    /// </summary>
    /// <param name="query">The query containing the NodeId.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A list of Fields.</returns>
    [HttpGet("Fields")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IList<InstanceDeclarationDto>>> GetNode([FromQuery] GetFieldsQuery query, CancellationToken cancellationToken)
    {
        var response = await Mediator.Send(query, cancellationToken);
        return Ok(response);
    }
}