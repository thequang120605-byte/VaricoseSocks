# =========================================================================
# SCRIPT KHỞI CHẠY TỰ ĐỘNG CẢ BACKEND API VÀ FRONTEND WEB UI
# DỰ ÁN: VARICOSE SOCKS MEDICAL (.NET 9 CORE)
# =========================================================================

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "  DỰ ÁN VỚ Y KHOA ĐIỀU TRỊ SUY GIÃN TĨNH MẠCH" -ForegroundColor Yellow
Write-Host "  - Backend Lead: Nguyễn Thế Quang (ASP.NET Core REST API)" -ForegroundColor Green
Write-Host "  - Frontend Lead: Chu Văn Quốc Trung (Web UI / Mobile)" -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Cyan

# 1. Khởi chạy Backend API (Cổng 5046)
Write-Host "[1/2] Đang khởi động Backend REST API tại http://localhost:5046 ..." -ForegroundColor Yellow
$apiProcess = Start-Process -FilePath "dotnet" -ArgumentList "run --project VaricoseSocks.API" -PassThru -NoNewWindow

Start-Sleep -Seconds 3

# 2. Khởi chạy Frontend Web UI (Cổng 5118)
Write-Host "[2/2] Đang khởi động Frontend Web UI tại http://localhost:5118 ..." -ForegroundColor Yellow
$webProcess = Start-Process -FilePath "dotnet" -ArgumentList "run --project VaricoseSocks.Web" -PassThru -NoNewWindow

Start-Sleep -Seconds 3

# 3. Mở trình duyệt xem giao diện Web UI
Write-Host "Đang mở trình duyệt vào Giao diện Web: http://localhost:5118 ..." -ForegroundColor Cyan
Start-Process "http://localhost:5118"

Write-Host ""
Write-Host "----------------------------------------------------------" -ForegroundColor Gray
Write-Host "  ĐỊA CHỈ TRUY CẬP HỆ THỐNG:" -ForegroundColor White
Write-Host "  1. Giao diện Mua sắm (Web UI):   http://localhost:5118" -ForegroundColor Green
Write-Host "  2. Backend REST API Portal:       http://localhost:5046" -ForegroundColor Green
Write-Host "  3. Quản trị viên (Admin UI):      http://localhost:5118/Admin" -ForegroundColor Green
Write-Host "  4. Tra cứu đơn hàng (Tracking):   http://localhost:5118/Order/Track" -ForegroundColor Green
Write-Host "----------------------------------------------------------" -ForegroundColor Gray
Write-Host "Nhấn phím bất kỳ hoặc tắt cửa sổ để dừng máy chủ..." -ForegroundColor Yellow

$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

Stop-Process -Id $apiProcess.Id -Force -ErrorAction SilentlyContinue
Stop-Process -Id $webProcess.Id -Force -ErrorAction SilentlyContinue
Write-Host "Đã dừng các dịch vụ." -ForegroundColor Red
