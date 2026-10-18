using VaricoseSocks.Domain.Entities;

namespace VaricoseSocks.Application.Interfaces;

public interface IChatbotRepository
{
    Task<ChatbotConsultation> SaveConsultationAsync(ChatbotConsultation consultation);
    Task<IEnumerable<ChatbotConsultation>> GetHistoryBySessionAsync(string sessionId);
}
