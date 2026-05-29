# PTEducation

## EF Core migrations workflow

Từ giờ chỉ cần chạy API là EF sẽ tự áp migration pending vào DB. Không cần script khi khởi động ứng dụng.

```powershell
# Chạy API, EF sẽ tự áp migration khi startup
dotnet run --project .\PTEducation.API\PTEducation.API.csproj

# Nếu thay đổi entity, vẫn cần tạo migration một lần ở design-time
dotnet ef migrations add Add_Attendance_Date --project .\PTEducation.Data\PTEducation.Data.csproj --startup-project .\PTEducation.API\PTEducation.API.csproj
```

Quy trình đúng là: sửa entity -> tạo migration bằng EF -> chạy `dotnet run` để ứng dụng tự apply migration vào DB.
