using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using Oqtane.Modules;
using System.Threading.Tasks;
using Trifoia.Module.AIVideoCoach.Models;
using System;

namespace CodeChops.Module.AIVideoCoach.Repository
{
    public class AIVideoCoachRepository : ITransientService
    {
        private readonly IDbContextFactory<AIVideoCoachContext> _factory;

        public AIVideoCoachRepository(IDbContextFactory<AIVideoCoachContext> factory)
        {
            _factory = factory;
        }

        // Save an embedding to the database
        public async Task<bool> AddEmbedding(TextEmbedding embedding)
        {
            using var db = _factory.CreateDbContext();
            db.TextEmbedding.Add(embedding);
            var numAdded = await db.SaveChangesAsync();
            if (numAdded > 0)
            {
                return true;
            }
            return false;
        }

        // Retrieve all embeddings from the database
        public async Task<List<TextEmbedding>> GetEmbeddedChunksAsync()
        {
            using var db = _factory.CreateDbContext();
            return await db.TextEmbedding.ToListAsync();
        }

        public async Task<bool> DeleteEmbeddingsAsync(string Title)
        {
            using var db = _factory.CreateDbContext();
            var embeddings = await db.TextEmbedding.Where(item => item.Title == Title).ToListAsync();
            db.TextEmbedding.RemoveRange(embeddings);
            var numDeleted = await db.SaveChangesAsync();
            if (numDeleted > 0)
            {
                return true;
            }
            return false;
        }
    }
}
