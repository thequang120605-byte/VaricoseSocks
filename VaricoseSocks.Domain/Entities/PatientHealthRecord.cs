namespace VaricoseSocks.Domain.Entities;

public class PatientHealthRecord
{
    public int Id { get; set; }
    public int UserId { get; set; }
    
    // Measurement points in centimeters
    public double AnkleCircumferenceCm { get; set; }   // cB
    public double? CalfCircumferenceCm { get; set; }    // cC
    public double? ThighCircumferenceCm { get; set; }   // cG
    public double? LegLengthCm { get; set; }            // lD hoặc lG
    
    // Medical assessment
    public int CeapStage { get; set; } = 1;             // C0 - C6
    public string? Symptoms { get; set; }               // Đau nhức, nặng chân, chuột rút ban đêm...
    public string? DiagnosisNote { get; set; }          // Ghi chú của bác sĩ hoặc chẩn đoán
    public string? DoctorName { get; set; }             // Tên bác sĩ khám/tư vấn
    public string? PrescribedCompression { get; set; }  // CCL1, CCL2, CCL3
    public string? RecommendedSize { get; set; }        // S, M, L, XL, XXL
    
    // Maintenance & replacement tracking
    public DateTime MeasurementDate { get; set; } = DateTime.UtcNow;
    public DateTime? ReplacementDueDate { get; set; }   // Thường là 6 tháng sau ngày đo / bắt đầu mang
    public bool ReminderSent { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
}
