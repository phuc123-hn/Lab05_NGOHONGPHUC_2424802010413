# Bài thực hành số 5 - Microservices .NET Core (Đại lý bảo hiểm)

- Họ và tên: Ngô Hồng Phúc
- MSSV: 2424802010413
- Lớp: D24CNTT05
- GitHub username: phuc123-hn

## Nội dung Lab 5

- Task 1: bổ sung code cho PricingService (PricingService.Api, PricingService.Test, PricingService.IntegrationTest).
- Task 2: cài đặt Eureka Server (thư mục eureka-server, chạy bằng Docker, cổng 8761) và đăng ký PricingService làm Eureka client (Steeltoe).

## Ghi chú

- PricingService chạy ở cổng 5050 (không dùng 5040 như đề vì cổng 5040 bị tiến trình hệ thống svchost của Windows chiếm). Cấu hình tại PricingService/appsettings.json.
- Chạy Eureka: docker build -t eureka-server ./eureka-server rồi docker run -d --name eureka -p 8761:8761 eureka-server
- Chạy PricingService: dotnet run --project ./PricingService --urls http://localhost:5050
- BlazorWasmClient tạm gỡ khỏi .sln vì cần các DTO/command trong các project .Api, sẽ bổ sung ở bài sau.
- CSDL: lab_netmicro_payments, lab_netmicro_jobs, lab_netmicro_policy, lab_netmicro_pricing (PostgreSQL chạy bằng Docker, xem docker-compose.infra.yml).
