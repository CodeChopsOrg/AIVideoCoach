using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Oqtane.Shared;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Trifoia.Module.AIVideoCoach.Repository;
using Oqtane.Controllers;
using System.Net;
using System.Threading.Tasks;
using System;
using Trifoia.Module.AIVideoCoach.Models;
using Trifoia.Module.AIVideoCoach.Services;
using System.Linq;

namespace CodeChops.Module.AIVideoCoach.Controllers;

[Route(ControllerRoutes.ApiRoute)]
public class AIVideoCoachController : ModuleControllerBase
{
    public readonly AIVideoCoachRepository _AIVideoCoachRepository;
    public readonly AIVideoCoachServerService _AIVideoCoachServerService;

    public AIVideoCoachController(AIVideoCoachServerService AIVideoCoachServerService, AIVideoCoachRepository AIVideoCoachRepository, 
        ILogManager logger, IHttpContextAccessor accessor) : base(logger, accessor)
    {
        _AIVideoCoachRepository = AIVideoCoachRepository;
        _AIVideoCoachServerService = AIVideoCoachServerService;
    }

    // Saves a TextEmbedding to the database.
    [HttpPost]
    [Route("save-embedding")]
    public async Task<ActionResult> SaveEmbedding([FromBody] TextEmbedding embedding)
    {
        if (embedding is not { Text: { Length: > 0 } })
        {
            _logger.Log(LogLevel.Error, this, LogFunction.Create, "Invalid TextEmbedding object received.");
            return BadRequest("Invalid TextEmbedding object.");
        }

        try
        {
            // Call the repository to save the embedding
            var success = await _AIVideoCoachServerService.EmbedTextAsync(embedding);

            _logger.Log(LogLevel.Information, this, LogFunction.Create, "TextEmbedding saved successfully to the database.");
            return Ok("Embedding saved successfully.");
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Error, this, LogFunction.Create, "Error saving TextEmbedding to the database.", ex);
            return StatusCode(500, "An error occurred while saving the embedding.");
        }
    }

    //Look into double
    [HttpPost]
    [Route("embed-query")]
    public async Task<ActionResult<EmbedQueryRequest>> EmbedQuery([FromBody] EmbedQueryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Embedding.Text))
        {
            _logger.Log(LogLevel.Error, this, LogFunction.Create, "Empty or invalid query received.");
            return BadRequest("Query cannot be null or empty.");
        }
        try
        {
            // Call the service to embed the query
            var (similarity, textEmbedding) = await _AIVideoCoachServerService.EmbedQueryAsync(request.Embedding.Text);

            if (similarity > 0 && textEmbedding != null)
            {
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Successfully processed query embedding.");
                var embeddingResponse = new EmbedQueryRequest { Similarity = similarity, Embedding = textEmbedding };
                return Ok(embeddingResponse); // Return only the similarity score
            }

            _logger.Log(LogLevel.Error, this, LogFunction.Create, $"Failed to process query embedding.");
            return StatusCode(500, "Failed to process query embedding.");
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Error, this, LogFunction.Create, "Error occurred while processing the query embedding.", ex);
            return StatusCode(500, "An internal server error occurred.");
        }

    }

    [HttpGet]
    [Route("get-embedded-chunks")]
    public async Task<ActionResult<List<TextEmbedding>>> GetEmbeddedChunks()
    {
        try
        {
            // Call the service/repository to retrieve the embeddings
            var embeddings = await _AIVideoCoachRepository.GetEmbeddedChunksAsync();

            if (embeddings == null || !embeddings.Any())
            {
                _logger.Log(LogLevel.Warning, this, LogFunction.Read, "No embeddings found in the database.");
                return NotFound("No embeddings available.");
            }
            Console.WriteLine("Controller Hit");
            _logger.Log(LogLevel.Information, this, LogFunction.Read, $"{embeddings.Count} embeddings retrieved successfully.");
            return Ok(embeddings);
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Error, this, LogFunction.Read, "Error retrieving embeddings.", ex);
            return StatusCode(500, "An error occurred while fetching embeddings.");
        }
    }

    [HttpDelete]
    [Authorize(Policy = PolicyNames.EditModule)]
    [Route("delete-embeddings/{Title}")]
    public async Task<ActionResult> DeleteEmbeddings(string Title)
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            _logger.Log(LogLevel.Error, this, LogFunction.Delete, "Title cannot be null or empty.");
            return BadRequest("Title cannot be null or empty.");
        }
        try
        {
            // Call the service to delete embeddings by Title
            var success = await _AIVideoCoachRepository.DeleteEmbeddingsAsync(Title);

            if (success)
            {
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, $"Embeddings for video '{Title}' deleted successfully.");
                return Ok($"Embeddings for video '{Title}' deleted successfully.");
            }

            _logger.Log(LogLevel.Warning, this, LogFunction.Delete, $"No embeddings found for video '{Title}'.");
            return NotFound($"No embeddings found for video '{Title}'.");
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Error, this, LogFunction.Delete, "Error occurred while deleting embeddings.", ex);
            return StatusCode(500, "An error occurred while deleting embeddings.");
        }
    }
}