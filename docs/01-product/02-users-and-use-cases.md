# Người Dùng Và Use Case

## Người dùng chính

- Backend Developer: tìm nguyên nhân lỗi trong service.
- DevOps Engineer: theo dõi tình trạng hệ thống và log theo môi trường.
- SRE: điều tra incident, error spike và service degradation.
- QA Engineer: kiểm tra lỗi trong staging hoặc test environment.
- Technical Support: tra cứu log liên quan tới yêu cầu hỗ trợ.

## Use case chính

- Tìm log lỗi trong 30 phút gần nhất.
- Truy vết một request theo `trace_id`.
- Lọc log theo `project`, `service`, `environment` và `level`.
- Xem tổng quan số lượng log, error rate và top error messages.
- Gửi log batch từ application để giảm overhead network.

## Ranh giới người dùng

Người dùng cuối của ứng dụng tích hợp không sử dụng TraceFlow trực tiếp. TraceFlow phục vụ đội kỹ thuật vận hành và phát triển hệ thống.
