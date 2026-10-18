using Microsoft.EntityFrameworkCore;
using VaricoseSocks.Application.Interfaces;
using VaricoseSocks.Domain.Entities;
using VaricoseSocks.Infrastructure.Data;

namespace VaricoseSocks.Infrastructure.Repositories;

public class ChatbotRepository : IChatbotRepository
{
    private readonly ApplicationDbContext _context;

    public ChatbotRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ChatbotConsultation> SaveConsultationAsync(ChatbotConsultation consultation)
    {
        _context.ChatbotConsultations.Add(consultation);
        await _context.SaveChangesAsync();
        return consultation;
    }

    public async Task<IEnumerable<ChatbotConsultation>> GetHistoryBySessionAsync(string sessionId)
    {
        return await _context.ChatbotConsultations
            .Where(c => c.SessionId == sessionId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }
}
