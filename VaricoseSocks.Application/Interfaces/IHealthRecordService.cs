using VaricoseSocks.Application.DTOs;

namespace VaricoseSocks.Application.Interfaces;

public interface IHealthRecordService
{
    Task<IEnumerable<PatientHealthRecordDto>> GetRecordsByUserIdAsync(int userId);
    Task<PatientHealthRecordDto?> GetByIdAsync(int id);
    Task<(bool Success, string Message, PatientHealthRecordDto? Record)> CreateRecordAsync(int userId, CreateHealthRecordDto dto);
    Task<(bool Success, string Message)> DeleteRecordAsync(int id, int userId, bool isAdmin = false);
    CeapAssessmentResultDto AssessCeap(int stage, bool hasUlcer, bool hasSkinChanges, bool hasEdema, bool hasVaricose);
}
