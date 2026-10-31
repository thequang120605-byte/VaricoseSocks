namespace VaricoseSocks.Application.DTOs;

public class PatientHealthRecordDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? PatientName { get; set; }
    public double AnkleCircumferenceCm { get; set; }
    public double? CalfCircumferenceCm { get; set; }
    public double? ThighCircumferenceCm { get; set; }
    public double? LegLengthCm { get; set; }
    public int CeapStage { get; set; }
    public string CeapStageText => CeapStage switch
    {
        0 => "C0 - Chưa có biểu hiện lâm sàng rõ rệt",
        1 => "C1 - Giãn mao mạch hình mạng nhện (< 3mm)",
        2 => "C2 - Giãn búi tĩnh mạch nông ngoằn ngoèo (> 3mm)",
        3 => "C3 - Phù chi dưới nhưng chưa biến đổi sắc tố",
        4 => "C4 - Loạn dưỡng sắc tố da, chàm hoặc xơ mỡ da",
        5 => "C5 - Biến đổi da có vết loét tĩnh mạch đã lành",
        6 => "C6 - Vết loét tĩnh mạch đang tiến triển / hở miệng",
        _ => "C1 - Giai đoạn nhẹ"
    };
    public string? Symptoms { get; set; }
    public string? DiagnosisNote { get; set; }
    public string? DoctorName { get; set; }
    public string? PrescribedCompression { get; set; }
    public string? RecommendedSize { get; set; }
    public DateTime MeasurementDate { get; set; }
    public DateTime? ReplacementDueDate { get; set; }
    public bool ReminderSent { get; set; }
    public int DaysUntilReplacement => ReplacementDueDate.HasValue 
        ? Math.Max(0, (int)(ReplacementDueDate.Value.Date - DateTime.UtcNow.Date).TotalDays) 
        : 0;
    public bool NeedsReplacement => ReplacementDueDate.HasValue && DateTime.UtcNow >= ReplacementDueDate.Value;
}

public class CreateHealthRecordDto
{
    public int? UserId { get; set; }
    public double AnkleCircumferenceCm { get; set; }
    public double? CalfCircumferenceCm { get; set; }
    public double? ThighCircumferenceCm { get; set; }
    public double? LegLengthCm { get; set; }
    public int CeapStage { get; set; } = 1;
    public string? Symptoms { get; set; }
    public string? DiagnosisNote { get; set; }
    public string? DoctorName { get; set; }
    public string? PrescribedCompression { get; set; }
    public string? RecommendedSize { get; set; }
}

public class CeapAssessmentResultDto
{
    public int Stage { get; set; }
    public string StageCode { get; set; } = "C1";
    public string StageName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RecommendedCompression { get; set; } = "CCL1 (15-21 mmHg)";
    public string RecommendedStyle { get; set; } = "Vớ gối y khoa (AD)";
    public string TreatmentAdvice { get; set; } = string.Empty;
    public bool RequiresDoctorPrescription { get; set; }
}
