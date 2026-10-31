using Microsoft.EntityFrameworkCore;
using VaricoseSocks.Application.DTOs;
using VaricoseSocks.Application.Interfaces;
using VaricoseSocks.Domain.Entities;
using VaricoseSocks.Infrastructure.Data;

namespace VaricoseSocks.Infrastructure.Services;

public class HealthRecordService : IHealthRecordService
{
    private readonly ApplicationDbContext _context;

    public HealthRecordService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PatientHealthRecordDto>> GetRecordsByUserIdAsync(int userId)
    {
        var records = await _context.PatientHealthRecords
            .AsNoTracking()
            .Include(r => r.User)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.MeasurementDate)
            .ToListAsync();

        return records.Select(MapToDto);
    }

    public async Task<PatientHealthRecordDto?> GetByIdAsync(int id)
    {
        var record = await _context.PatientHealthRecords
            .AsNoTracking()
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);

        return record == null ? null : MapToDto(record);
    }

    public async Task<(bool Success, string Message, PatientHealthRecordDto? Record)> CreateRecordAsync(int userId, CreateHealthRecordDto dto)
    {
        if (dto.AnkleCircumferenceCm <= 10 || dto.AnkleCircumferenceCm > 60)
        {
            return (false, "Số đo vòng cổ chân cB không hợp lý (chuẩn người lớn từ 15cm - 50cm).", null);
        }

        var measurementDate = DateTime.UtcNow;
        var replacementDueDate = measurementDate.AddMonths(6); // Chu kỳ thay vớ y tế chuẩn 6 tháng

        var entity = new PatientHealthRecord
        {
            UserId = userId,
            AnkleCircumferenceCm = dto.AnkleCircumferenceCm,
            CalfCircumferenceCm = dto.CalfCircumferenceCm,
            ThighCircumferenceCm = dto.ThighCircumferenceCm,
            LegLengthCm = dto.LegLengthCm,
            CeapStage = dto.CeapStage,
            Symptoms = dto.Symptoms,
            DiagnosisNote = dto.DiagnosisNote,
            DoctorName = dto.DoctorName,
            PrescribedCompression = dto.PrescribedCompression,
            RecommendedSize = dto.RecommendedSize,
            MeasurementDate = measurementDate,
            ReplacementDueDate = replacementDueDate,
            ReminderSent = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.PatientHealthRecords.Add(entity);
        await _context.SaveChangesAsync();

        return (true, "Đã lưu hồ sơ theo dõi tĩnh mạch thành công.", MapToDto(entity));
    }

    public async Task<(bool Success, string Message)> DeleteRecordAsync(int id, int userId, bool isAdmin = false)
    {
        var record = await _context.PatientHealthRecords.FindAsync(id);
        if (record == null)
        {
            return (false, "Không tìm thấy hồ sơ theo dõi.");
        }

        if (!isAdmin && record.UserId != userId)
        {
            return (false, "Bạn không có quyền xóa hồ sơ của người khác.");
        }

        _context.PatientHealthRecords.Remove(record);
        await _context.SaveChangesAsync();
        return (true, "Đã xóa hồ sơ thành công.");
    }

    public CeapAssessmentResultDto AssessCeap(int stage, bool hasUlcer, bool hasSkinChanges, bool hasEdema, bool hasVaricose)
    {
        // Phân tầng CEAP lâm sàng quốc tế
        int detectedStage = stage;
        if (hasUlcer) detectedStage = 6;
        else if (hasSkinChanges && detectedStage < 4) detectedStage = 4;
        else if (hasEdema && detectedStage < 3) detectedStage = 3;
        else if (hasVaricose && detectedStage < 2) detectedStage = 2;

        return detectedStage switch
        {
            0 => new CeapAssessmentResultDto
            {
                Stage = 0,
                StageCode = "C0",
                StageName = "Giai đoạn C0 - Chưa có tổn thương thực thể",
                Description = "Chỉ có cảm giác nặng mỏi chân, tức bắp chân khi đứng hoặc ngồi lâu. Chưa thấy búi gân hay mạng nhện.",
                RecommendedCompression = "CCL1 (15 - 20 mmHg)",
                RecommendedStyle = "Vớ gối y khoa phòng ngừa (AD)",
                TreatmentAdvice = "Nên mang vớ CCL1 khi làm việc văn phòng, đi máy bay hoặc đứng nhiều. Nâng cao chân 15 phút khi nghỉ ngơi.",
                RequiresDoctorPrescription = false
            },
            1 => new CeapAssessmentResultDto
            {
                Stage = 1,
                StageCode = "C1",
                StageName = "Giai đoạn C1 - Giãn mao mạch mạng nhện / Tĩnh mạch dạng lưới",
                Description = "Xuất hiện các mạch máu li ti màu xanh đỏ hình mạng nhện dưới da (< 3mm), thường ở vùng mắt cá hoặc đùi.",
                RecommendedCompression = "CCL1 (18 - 21 mmHg)",
                RecommendedStyle = "Vớ gối hoặc Vớ đùi y khoa",
                TreatmentAdvice = "Sử dụng vớ y khoa CCL1 để ngăn dòng máu ứ trệ lan rộng, kết hợp kem bôi mát dịu tĩnh mạch và đi bộ nhẹ nhàng.",
                RequiresDoctorPrescription = false
            },
            2 => new CeapAssessmentResultDto
            {
                Stage = 2,
                StageCode = "C2",
                StageName = "Giai đoạn C2 - Giãn tĩnh mạch nông nổi ngoằn ngoèo",
                Description = "Tĩnh mạch nổi phồng ngoằn ngoèo trên bề mặt da (> 3mm), nhìn thấy rõ ở bắp chân hoặc mặt trong đùi.",
                RecommendedCompression = "CCL2 (23 - 32 mmHg)",
                RecommendedStyle = "Vớ gối y khoa (AD) hoặc Vớ đùi (AG)",
                TreatmentAdvice = "Cần dùng vớ điều trị chuẩn CCL2 hàng ngày vào ban ngày. Khép các van tĩnh mạch bị hở, tránh mang vác nặng.",
                RequiresDoctorPrescription = false
            },
            3 => new CeapAssessmentResultDto
            {
                Stage = 3,
                StageCode = "C3",
                StageName = "Giai đoạn C3 - Phù chi dưới do ứ đọng tĩnh mạch",
                Description = "Chân sưng phù, ấn vào thấy lõm nhẹ ở mu chân hoặc cổ chân, phù tăng nặng về chiều tối.",
                RecommendedCompression = "CCL2 (23 - 32 mmHg) hoặc CCL3",
                RecommendedStyle = "Vớ gối y khoa hở ngón (Open Toe)",
                TreatmentAdvice = "Mang vớ y khoa ngay từ sáng sớm khi chân chưa kịp phù. Hạn chế ăn mặn, tập co duỗi cổ chân thường xuyên.",
                RequiresDoctorPrescription = false
            },
            4 => new CeapAssessmentResultDto
            {
                Stage = 4,
                StageCode = "C4",
                StageName = "Giai đoạn C4 - Biến đổi dinh dưỡng da & Xơ mỡ mô",
                Description = "Vùng da quanh cổ chân bị sạm màu nâu/đen (nhiễm sắc tố sắc), xơ cứng mô mỡ hoặc chàm tĩnh mạch gây ngứa rát.",
                RecommendedCompression = "CCL2 hoặc CCL3 (34 - 46 mmHg)",
                RecommendedStyle = "Vớ y khoa điều trị chuyên sâu",
                TreatmentAdvice = "Tình trạng tiến triển nặng, nguy cơ loét cao. Cần khám chuyên khoa mạch máu để siêu âm Doppler và tuân thủ phác đồ.",
                RequiresDoctorPrescription = true
            },
            5 => new CeapAssessmentResultDto
            {
                Stage = 5,
                StageCode = "C5",
                StageName = "Giai đoạn C5 - Biến đổi da có vết loét tĩnh mạch đã lành",
                Description = "Vết loét trước đây đã kéo da non và lành sẹo nhưng nền da mỏng yếu, dễ tái phát bất cứ lúc nào.",
                RecommendedCompression = "CCL2 hoặc CCL3 duy trì",
                RecommendedStyle = "Vớ điều trị ngừa tái phát loét",
                TreatmentAdvice = "Bắt buộc duy trì mang vớ y khoa suốt đời để phòng ngừa loét tái phát. Dưỡng ẩm nhẹ vùng da sẹo.",
                RequiresDoctorPrescription = true
            },
            _ => new CeapAssessmentResultDto
            {
                Stage = 6,
                StageCode = "C6",
                StageName = "Giai đoạn C6 - Vết loét tĩnh mạch đang hở miệng tiến triển",
                Description = "Có ổ loét hở, rỉ dịch hoặc đau nhức ở vùng mắt cá trong chân, khó lành nếu không có băng ép y tế.",
                RecommendedCompression = "Hệ thống vớ 2 lớp điều trị loét (Ulcer Kit - 40 mmHg)",
                RecommendedStyle = "Vớ lót + Vớ ngoài chuyên dụng trị loét",
                TreatmentAdvice = "Cần chăm sóc vết thương vô khuẩn tại cơ sở y tế kết hợp mang bộ vớ Ulcer kit đặc trị theo chỉ định của bác sĩ.",
                RequiresDoctorPrescription = true
            }
        };
    }

    private static PatientHealthRecordDto MapToDto(PatientHealthRecord r)
    {
        return new PatientHealthRecordDto
        {
            Id = r.Id,
            UserId = r.UserId,
            PatientName = r.User?.FullName,
            AnkleCircumferenceCm = r.AnkleCircumferenceCm,
            CalfCircumferenceCm = r.CalfCircumferenceCm,
            ThighCircumferenceCm = r.ThighCircumferenceCm,
            LegLengthCm = r.LegLengthCm,
            CeapStage = r.CeapStage,
            Symptoms = r.Symptoms,
            DiagnosisNote = r.DiagnosisNote,
            DoctorName = r.DoctorName,
            PrescribedCompression = r.PrescribedCompression,
            RecommendedSize = r.RecommendedSize,
            MeasurementDate = r.MeasurementDate,
            ReplacementDueDate = r.ReplacementDueDate,
            ReminderSent = r.ReminderSent
        };
    }
}
