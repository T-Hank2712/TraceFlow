# Yêu cầu hệ thống

## 1. Bài toán
Các hệ thống phân tán thường tạo log ở nhiều service, container và môi trường khác nhau. Khi không có một pipeline tập trung để thu thập, xử lý và tìm kiếm log, developer phải kiểm tra từng serivce riêng lẻ khi kiểm tra lỗi. Điều này khiến quá trình debug chậm, rời rạc và khó truy vết theo ngữ cảnh vận hành.

TraceFlow giải quyết bài toán này bằng cách cung cấp một backend observability platform tập trùng, nơi application có thể gửi structured logs qua API key, log được xử lý bất đồng bộ qua Kafka, index vào OpenSearch và được tìm kiếm thông qua Control API có kiểm soát quyền truy cập.

## 2. Mục tiêu
- Cung cấp một hệ thống tập trung để tiếp nhận structured logs từ nhiều application.
- Tách log ingestion khỏi log indexing thông qua Kafka để hệ thống chịu tải tốt hơn
- Cho phép user tìm kiếm log theo workspace, project, application, environment, service, level, trace ID, correlation ID và khoảng thời gian.
- Đảm bảo tenant isolation theo workspace, project, application và environment.
- Không tin tenant context do client gửi lên khi submit log.
- Cung cấp API key lifecycle an toàn cho ingestion path.
- Xử lý log đáng tin cậy bằng batching, retry và Dead Letter Queue.
- Cho phép hệ thống có thể benchmark throughput và latency cho mục tiêu portfolio/backend engineering.

## 3. Ngoài phạm vi
- TraceFlow không phải metrics platform như Prometheus.
- TraceFlow không phải distributed tracing platform đầy đủ.
- TraceFlow không expose OpenSearch trực tiếp cho end user.
- TraceFlow không cung cấp query language phức tạp cho end user.
- TraceFlow không làm billing, subscription hoặc payment.
- TraceFlow không yêu cầu Kubernetes hoặc cloud production deployment trong phạm vi ban đầu.
- TraceFlow không xây SDK ecosystem đầy đủ cho nhiều ngôn ngữ.
- TraceFlow không làm archive storage hoặc restore log đã hết hạn trong phạm vi ban đầu.
- TraceFlow không xây frontend dashboard lớn vượt quá nhu cầu demo và sử dụng luồng chính.

## 4. Người Dùng Mục Tiêu

### Developer / Engineer
Developer sử dụng TraceFlow để tìm kiếm log, điều tra lỗi, kiểm tra hành vi application và truy vết request hoặc correlation giữa nhiều service.

### Workspace / Project Admin
Admin quản lý workspace, project, trace application, API key, member và quyền truy cập. Nhóm này chịu trách nhiệm cấu hình resource và kiểm soát ai được xem hoặc quản lý log.

### Client Application
Client Application là service bên ngoài gửi log vào TraceFlow bằng API key. Đây không phải là user đăng nhập, mà là workload sử dụng ingestion endpoint để đưa log vào hệ thống.

## 5. Luồng Sử Dụng Chính

| Nhóm luồng | Mô tả | Kết quả |
| :--- | :--- | :--- |
| Account Flow | User đăng ký, đăng nhập, refresh session, logout và quản lý thông tin tài khoản cơ bản. | User có JWT access token hợp lệ để gọi các API quản trị và tìm kiếm log. |
| Workspace Flow | User tạo workspace, xem workspace được tham gia, quản lý thông tin workspace, member và invitation theo quyền được cấp. | Hệ thống có ranh giới workspace để tổ chức tenant, member và quyền truy cập cấp cao. |
| Project Flow | User tạo project trong workspace, quản lý project member, invitation và quyền truy cập project. | Hệ thống có project scope để phân quyền, quản lý application và giới hạn phạm vi search log. |
| Trace Application Flow | User tạo trace application trong project và cấu hình environment, trạng thái, retention hoặc các thiết lập liên quan. | Hệ thống có application context để gắn log với workload cụ thể trong project. |
| API Key Flow | Project Manager tạo, xem danh sách, revoke và cấu hình expiration cho API key của trace application. | Client application có credential riêng để gửi log, còn secret được kiểm soát theo lifecycle an toàn. |
| Ingestion Flow | Client application gửi single log hoặc batch logs bằng API key tới Ingestion API. | Log request được xác thực, validate, giới hạn kích thước và chuẩn bị đưa vào pipeline. |
| Tenant Enrichment Flow | Ingestion API validate API key với Control API, nhận tenant context hợp lệ và enrich log bằng workspace, project, application, environment. | Tenant context được xác định server-side, tránh việc client spoof workspace/project/application. |
| Kafka Publish Flow | Ingestion API publish enriched log event vào Kafka sau khi validate và enrich thành công. | Log ingestion được tách khỏi indexing, giúp giảm độ trễ response và tăng khả năng chịu tải. |
| Processing Flow | Log Processor consume Kafka event, validate internal log event, normalize, gom batch và index vào OpenSearch. | Log hợp lệ được lưu vào search backend và có thể được truy vấn sau đó. |
| Failure Handling Flow | Log Processor đưa malformed, invalid hoặc failed event vào Dead Letter Queue kèm failure reason và original payload. | Event lỗi không làm nghẽn pipeline và vẫn còn dữ liệu để debug hoặc replay thủ công trong tương lai. |
| Search Flow | User search log thông qua Control API với JWT authentication và project access check. | User chỉ nhận log trong workspace/project được cấp quyền; OpenSearch không bị expose trực tiếp. |
| Operations Flow | Hệ thống cung cấp health check, local stack configuration và dependency readiness cho các service chính. | Developer có thể chạy, kiểm tra và debug toàn bộ stack trong môi trường local development. |

## 6. Yêu Cầu Chức Năng
### Identity Và Session

| Mã FR | Tên Chức Năng | Mô Tả Yêu Cầu & Quy Tắc Nghiệp Vụ | Độ Ưu Tiên | Giai Đoạn |
| :--- | :--- | :--- | :---: | :---: |
| **FR-001** | Đăng ký tài khoản | Hệ thống cho phép người dùng khởi tạo tài khoản mới bằng email và mật khẩu. | High | Phase 1 |
| **FR-002** | Đăng nhập hệ thống | Cho phép người dùng đăng nhập vào hệ thống bằng credential (email + mật khẩu) hợp lệ. | High | Phase 1 |
| **FR-003** | Cấp Access Token | Hệ thống tự động cấp Access Token (JWT) ngắn hạn sau khi người dùng đăng nhập thành công. | High | Phase 1 |
| **FR-004** | Refresh Session | Hỗ trợ duy trì/làm mới phiên làm việc bằng Refresh Token mà không yêu cầu đăng nhập lại. | High | Phase 1 |
| **FR-005** | Đăng xuất (Logout) | Cho phép người dùng hủy phiên làm việc hiện tại trên thiết bị đang sử dụng. | High | Phase 1 |
| **FR-006** | Đổi mật khẩu | Cho phép người dùng đã xác thực tự thay đổi mật khẩu khi cung cấp đúng mật khẩu cũ. | Medium | Phase 1 |
| **FR-007** | Xem & Cập nhật Profile | Cho phép người dùng xem và cập nhật thông tin cá nhân cơ bản (Họ tên, Avatar, ...). | Low | Phase 2 |
| **FR-008** | Khôi phục mật khẩu | Hỗ trợ quy trình quên mật khẩu (gửi email reset link/OTP để tạo mật khẩu mới). | Medium | Phase 1 |
| **FR-009** | Xác thực Email | Bắt buộc gửi link/OTP xác thực qua email để kích hoạt tài khoản trước khi cho phép đăng nhập. | High | Phase 1 |
| **FR-010** | Khóa tài khoản tạm thời | Tự động khóa tài khoản trong $X$ phút hoặc yêu cầu CAPTCHA sau $N$ lần đăng nhập thất bại liên tiếp (chống Brute-force). | High | Phase 1 |
| **FR-011** | Quản lý Active Sessions | Cho phép người dùng xem danh sách thiết bị/trình duyệt đang đăng nhập và chủ động thu hồi (Revoke) phiên từ xa. | Medium | Phase 2 |
| **FR-012** | Thu hồi Refresh Token | Tự động thêm Refresh Token vào danh sách đen (Blacklist/Revoke) ngay khi người dùng Logout hoặc Đổi mật khẩu thành công. | High | Phase 1 |

### Workspace Management

| Mã FR | Tên Chức Năng | Mô Tả Yêu Cầu & Quy Tắc Nghiệp Vụ | Độ Ưu Tiên | Giai Đoạn |
| :--- | :--- | :--- | :---: | :---: |
| **FR-013** | Khởi tạo Workspace | Cho phép người dùng đã xác thực tạo Workspace mới và tự động trở thành Workspace Owner. | High | Phase 1 |
| **FR-014** | Xem danh sách Workspace | Cho phép người dùng xem danh sách tất cả các Workspace mà họ đang tham gia với tư cách thành viên. | High | Phase 1 |
| **FR-015** | Xem chi tiết Workspace | Cho phép người dùng xem thông tin chi tiết và cấu hình của Workspace khi có quyền truy cập hợp lệ. | High | Phase 1 |
| **FR-016** | Cập nhật thông tin Workspace | Cho phép quản trị viên (Manager/Owner/Admin) chỉnh sửa tên, mô tả và cấu hình của Workspace. | Medium | Phase 1 |
| **FR-017** | Quản lý thành viên Workspace | Cho phép Owner hoặc Admin thay đổi vai trò (Role), phân quyền hoặc xoá thành viên ra khỏi Workspace. | High | Phase 1 |
| **FR-018** | Mời thành viên mới | Cho phép gửi lời mời tham gia Workspace tới người dùng khác qua địa chỉ Email. | High | Phase 1 |
| **FR-019** | Quản lý lời mời Workspace | Hỗ trợ người được mời Chấp nhận (Accept) / Từ chối (Decline), và cho phép bên mời Hủy (Cancel) lời mời chưa phản hồi. | High | Phase 1 |
| **FR-020** | Chuyển quyền sở hữu | Cho phép Workspace Owner chuyển giao toàn bộ quyền sở hữu (Owner) cho một thành viên khác trong Workspace. | High | Phase 1 |
| **FR-021** | Rời khỏi Workspace | Cho phép thành viên tự rời khỏi Workspace (ngoại trừ Owner duy nhất chưa chuyển quyền sở hữu). | Medium | Phase 1 |
| **FR-022** | Xóa cứng & Xóa mềm Workspace | Hỗ trợ ẩn tạm thời (Soft Delete/Archive) có thể khôi phục, hoặc xóa vĩnh viễn (Hard Delete) toàn bộ dữ liệu Workspace. | High | Phase 1 |
| **FR-023** | Định danh Workspace Slug | Hỗ trợ tạo và quản lý chuỗi định danh (Slug) duy nhất cho từng Workspace để phục vụ định tuyến URL và nhận diện hệ thống. | High | Phase 1 |
| **FR-024** | Nhật ký hoạt động (Audit Log) | Tự động ghi lại và cho phép Owner/Admin tra cứu nhật ký thao tác quản trị (mời/xóa thành viên, đổi quyền, chỉnh sửa/xóa tài nguyên). | High | Phase 2 |

### Project Management

| Mã FR | Tên Chức Năng | Mô Tả Yêu Cầu & Quy Tắc Nghiệp Vụ | Độ Ưu Tiên | Giai Đoạn |
| :--- | :--- | :--- | :---: | :---: |
| **FR-025** | Khởi tạo Project | Cho phép Workspace Owner hoặc Admin tạo Project mới trong phạm vi Workspace. | High | Phase 1 |
| **FR-026** | Xem danh sách Project | Cho phép người dùng xem danh sách tất cả các Project trong Workspace mà họ được phân quyền truy cập. | High | Phase 1 |
| **FR-027** | Xem chi tiết Project | Cho phép người dùng xem thông tin chi tiết và cấu hình của Project khi có quyền tương ứng. | High | Phase 1 |
| **FR-028** | Cập nhật thông tin Project | Cho phép Project Manager chỉnh sửa tên, mô tả và cấu hình chung của Project. | Medium | Phase 1 |
| **FR-029** | Quản lý thành viên Project | Cho phép Project Manager thay đổi vai trò, phân quyền hoặc xóa thành viên ra khỏi Project. | High | Phase 1 |
| **FR-030** | Mời thành viên vào Project | Hỗ trợ gửi lời mời thành viên trong Workspace tham gia vào một Project cụ thể. | High | Phase 1 |
| **FR-031** | Quản lý lời mời Project | Hỗ trợ người được mời Chấp nhận (Accept) / Từ chối (Decline), và cho phép bên mời Hủy (Cancel) lời mời. | High | Phase 1 |
| **FR-032** | Rời khỏi Project | Cho phép thành viên tự rời khỏi Project theo quy tắc hợp lệ (ngoại trừ Manager duy nhất chưa bàn giao). | Medium | Phase 1 |
| **FR-033** | Xóa cứng & Xóa mềm Project | Hỗ trợ ẩn tạm thời (Soft Delete/Archive) có thể khôi phục, hoặc xóa vĩnh viễn (Hard Delete) Project và dữ liệu con. | High | Phase 1 |
| **FR-034** | Gán quyền Khởi tạo mặc định | Tự động gán vai trò Project Manager cho người dùng trực tiếp khởi tạo Project đó. | High | Phase 1 |
| **FR-035** | Định danh Project Slug | Yêu cầu và tạo chuỗi định danh (Slug/Key) ngắn gọn, duy nhất trong cùng Workspace để định tuyến và nhận diện. | High | Phase 1 |
| **FR-036** | Cấu hình Retention Policy cấp Project | Cho phép Project Manager thiết lập hoặc kiểm tra thời gian lưu trữ log ở cấp Project (ví dụ: 7 ngày, 14 ngày, 30 ngày, 90 ngày) để làm ranh giới lifecycle mặc định cho log trong phạm vi Project. | High | Phase 1 |

### Trace Application Management

| Mã FR | Tên Chức Năng | Mô Tả Yêu Cầu & Quy Tắc Nghiệp Vụ | Độ Ưu Tiên | Giai Đoạn |
| :--- | :--- | :--- | :---: | :---: |
| **FR-037** | Khởi tạo Trace Application | Cho phép Project Manager tạo mới ứng dụng cần theo dõi log (Trace Application) trong phạm vi Project. | High | Phase 1 |
| **FR-038** | Xem danh sách Trace Application | Cho phép người dùng có quyền xem danh sách toàn bộ các Trace Application thuộc Project. | High | Phase 1 |
| **FR-039** | Xem chi tiết Trace Application | Cho phép người dùng có quyền xem thông tin chi tiết, cấu hình và trạng thái của Trace Application. | High | Phase 1 |
| **FR-040** | Cập nhật Trace Application | Cho phép Project Manager chỉnh sửa thông tin tên, mô tả và cài đặt của Trace Application. | Medium | Phase 1 |
| **FR-041** | Xóa cứng & Xóa mềm Trace App | Hỗ trợ ẩn tạm thời (Soft Delete/Archive) hoặc xóa vĩnh viễn (Hard Delete) Trace Application cùng cấu hình liên quan. | High | Phase 1 |
| **FR-042** | Cấu hình Môi trường Vận hành | Cho phép gán và thiết lập môi trường hoạt động (`Development`, `Staging`, `Production`, ...) cho từng Trace Application. | High | Phase 1 |
| **FR-043** | Cấu hình Retention Policy | Cho phép Project Manager thiết lập hoặc kiểm tra thời gian lưu trữ log (ví dụ: 7 ngày, 30 ngày) riêng biệt cho từng Trace Application. | High | Phase 1 |

### API Key Lifecycle

| Mã FR | Tên Chức Năng | Mô Tả Yêu Cầu & Quy Tắc Nghiệp Vụ | Độ Ưu Tiên | Giai Đoạn |
| :--- | :--- | :--- | :---: | :---: |
| **FR-044** | Khởi tạo API Key | Cho phép Project Manager tạo API Key mới dành riêng cho một Trace Application cụ thể. | High | Phase 1 |
| **FR-045** | Hiển thị Secret một lần | Chuỗi Secret của API Key chỉ được hiển thị duy nhất một lần ngay sau khi khởi tạo thành công. | High | Phase 1 |
| **FR-046** | Bảo mật lưu trữ Secret | Hệ thống tuyệt đối không lưu trữ Secret của API Key ở dạng văn bản thuần (Plaintext) trong cơ sở dữ liệu. | High | Phase 1 |
| **FR-047** | Băm Secret với Pepper | Secret của API Key phải được băm (Hash) kèm theo chuỗi bí mật phía Server (Server-side Pepper/Secret) trước khi lưu DB. | High | Phase 1 |
| **FR-048** | Định danh Prefix Key | API Key phải bao gồm đoạn Prefix/Identifier cố định hỗ trợ tra cứu (Lookup) và ghi vết (Audit) mà không cần băm. | High | Phase 1 |
| **FR-049** | Xem danh sách API Key | Cho phép Project Manager xem danh sách các Key (chỉ hiển thị Masked Secret, Prefix, ngày tạo, trạng thái) mà không lộ Full Secret. | High | Phase 1 |
| **FR-050** | Thu hồi API Key (Revoke) | Cho phép Project Manager chủ động vô hiệu hóa (Revoke) API Key lập tức khi có sự cố lộ khóa hoặc thay khóa mới. | High | Phase 1 |
| **FR-051** | Thời hạn API Key (Expiration) | Hỗ trợ thiết lập ngày hết hạn cho API Key; Key sẽ tự động vô hiệu hóa sau thời gian này. | High | Phase 1 |
| **FR-052** | Cập nhật thời gian sử dụng | Tự động cập nhật mốc thời gian sử dụng gần nhất (`last_used_at`) mỗi khi API Key xác thực thành công trên Ingestion Path. | Medium | Phase 1 |
| **FR-053** | Chặn API Key không hợp lệ | Từ chối toàn bộ request nạp log sử dụng Key đã bị Revoke, Expired, hoặc thuộc Resource (Workspace/Project/App) bị Delete/Archive. | High | Phase 1 |
| **FR-054** | Giới hạn Scope Nạp log | Đảm bảo API Key chỉ có duy nhất quyền Nạp log (`ingest:write`), không thể dùng để gọi API Quản trị hay Truy vấn/Tìm kiếm log. | High | Phase 1 |
| **FR-055** | Xóa Cache tức thì khi Vô hiệu hóa | Lập tức xóa/vô hiệu hóa API Key trên bộ nhớ đệm (Redis Cache) của Ingestion Gateway ngay khi Key bị Revoke/Expire hoặc Resource bị xóa. | High | Phase 1 |

### Internal API Key Validation

| Mã FR | Tên Chức Năng | Mô Tả Yêu Cầu & Quy Tắc Nghiệp Vụ | Độ Ưu Tiên | Giai Đoạn |
| :--- | :--- | :--- | :---: | :---: |
| **FR-056** | Xác thực API Key nội bộ | Control API phải cung cấp Internal Endpoint để Ingestion API kiểm tra API Key có hợp lệ hay không. | High | Phase 1 |
| **FR-057** | Xác thực Service-to-Service | Internal Endpoint phải yêu cầu Internal Service Secret hoặc cơ chế Service-to-Service Authentication tương đương để chỉ các service được phép mới có thể gọi endpoint. | High | Phase 1 |
| **FR-058** | Trả về Tenant Context | Nếu API Key hợp lệ, endpoint phải trả về Tenant Context gồm các thông tin cần thiết để Ingestion API xác định Workspace, Project và Application tương ứng. | High | Phase 1 |
| **FR-059** | Bảo vệ API Key Secret | Endpoint không được trả về API Key Secret hoặc Secret Hash trong response. | High | Phase 1 |
| **FR-060** | Từ chối API Key không hợp lệ | Endpoint phải trả về trạng thái không hợp lệ nếu API Key sai format, không tồn tại, đã bị Revoke, đã Expire hoặc không còn Usable. | High | Phase 1 |
| **FR-061** | Hỗ trợ Redis Cache | Endpoint phải trả về thông tin Cache như `ttl` hoặc `cache_duration` để Ingestion Gateway có thể lưu kết quả xác thực vào Redis Cache và giảm số lần gọi đến Control API. | High | Phase 1 |
| **FR-062** | Bảo vệ khỏi quá tải | Control API phải áp dụng Rate Limiting hoặc Circuit Breaker cho Internal Endpoint để hạn chế quá tải khi Redis Cache của Ingestion Gateway bị lỗi hoặc xảy ra Cache Stampede. | High | Phase 1 |

### Log Ingestion

| Mã FR | Tên Chức Năng | Mô Tả Yêu Cầu & Quy Tắc Nghiệp Vụ | Độ Ưu Tiên | Giai Đoạn |
| :--- | :--- | :--- | :---: | :---: |
| **FR-063** | Tiếp nhận Single Log | Ingestion API phải cho phép Client Application gửi một log event trong mỗi request. | High | Phase 1 |
| **FR-064** | Tiếp nhận Batch Logs | Ingestion API phải cho phép Client Application gửi nhiều log event trong một request. | High | Phase 1 |
| **FR-065** | Đọc API Key | Ingestion API phải đọc API Key từ HTTP Header theo format `Authorization: ApiKey {secret}`. | High | Phase 1 |
| **FR-066** | Kiểm tra API Key Request | Ingestion API phải từ chối request nếu thiếu API Key hoặc API Key không đúng format. | High | Phase 1 |
| **FR-067** | Xác thực API Key | Ingestion API phải xác thực API Key thông qua Control API trước khi tiếp nhận và xử lý log. | High | Phase 1 |
| **FR-068** | Validate Log Payload | Ingestion API phải validate log payload trước khi publish event vào Kafka. | High | Phase 1 |
| **FR-069** | Giới hạn Request Body | Ingestion API phải giới hạn kích thước HTTP request body để tránh request quá lớn gây ảnh hưởng đến hệ thống. | High | Phase 1 |
| **FR-070** | Giới hạn Batch Size | Ingestion API phải giới hạn số lượng log event tối đa trong một batch request. | High | Phase 1 |
| **FR-071** | Kiểm tra Required Fields | Ingestion API phải từ chối log nếu thiếu các field bắt buộc như `service`, `level` hoặc `message`. | High | Phase 1 |
| **FR-072** | Chuẩn hóa Log Fields | Ingestion API phải normalize các field cơ bản như `level`, `service` và `timestamp` khi cần thiết để đảm bảo dữ liệu có format nhất quán. | Medium | Phase 1 |
| **FR-073** | Validate Timestamp | Nếu Client Application gửi `timestamp`, Ingestion API phải validate giá trị này theo format RFC3339; nếu không gửi `timestamp`, Ingestion API sử dụng thời điểm nhận request làm `timestamp`. | High | Phase 1 |
| **FR-074** | Giới hạn Metadata | Ingestion API phải giới hạn metadata theo tổng kích thước, độ sâu nested object và số lượng field để tránh payload quá lớn hoặc OpenSearch mapping explosion. Kiểu dữ liệu metadata chỉ nên dùng các kiểu JSON cơ bản. | High | Phase 1 |
| **FR-075** | Tin cậy Tenant Context | Ingestion API không được sử dụng `workspace`, `project`, `application` hoặc `environment` do Client Application tự khai báo để xác định tenant context. | High | Phase 1 |
| **FR-076** | Enrich Tenant Context | Ingestion API phải enrich log bằng Tenant Context được lấy từ API Key đã xác thực, thay vì tin tưởng thông tin tenant do client cung cấp. | High | Phase 1 |
| **FR-077** | Publish Log Event | Ingestion API phải publish log event đã được validate và enrich vào Kafka để xử lý bất đồng bộ. | High | Phase 1 |
| **FR-078** | Xác nhận Log Ingestion | Ingestion API chỉ được trả về response thành công sau khi log event đã được publish thành công vào Kafka. | High | Phase 1 |
| **FR-079** | Không Index Trực tiếp | Ingestion API không được index log trực tiếp vào OpenSearch trong quá trình xử lý HTTP request. | High | Phase 1 |
| **FR-080** | Rate Limiting | Ingestion API phải giới hạn số lượng request hoặc số lượng log/giây theo từng API Key (ví dụ: 1,000 req/min) | High | Phase 1 |

### Kafka Log Event

| Mã FR | Tên Chức Năng | Mô Tả Yêu Cầu & Quy Tắc Nghiệp Vụ | Độ Ưu Tiên | Giai Đoạn |
| :--- | :--- | :--- | :---: | :---: |
| **FR-081** | Định danh Log Event | Mỗi Enriched Log Event được publish vào Kafka phải có một `eventId` duy nhất để định danh và truy vết event. | High | Phase 1 |
| **FR-082** | Sinh Event ID tại Ingestion API | Ingestion API phải là thành phần sinh `eventId` cho mỗi Log Event sau khi request được validate và API Key hợp lệ; Client Application không được tự quyết định `eventId` trong phạm vi ban đầu. | High | Phase 1 |
| **FR-083** | Timestamp của Log Event | Mỗi Enriched Log Event phải có `timestamp` của log và `receivedAt` là thời điểm Ingestion API tiếp nhận log. | High | Phase 1 |
| **FR-084** | Tenant Context | Mỗi Enriched Log Event phải chứa `workspaceId`, `projectId`, `applicationId` và `environment` được xác định từ API Key hợp lệ. | High | Phase 1 |
| **FR-085** | Required Log Fields | Mỗi Enriched Log Event phải chứa các field bắt buộc gồm `service`, `level` và `message`. | High | Phase 1 |
| **FR-086** | Trace và Metadata | Enriched Log Event có thể chứa `traceId`, `correlationId` và `metadata` để hỗ trợ tracing, correlation và lưu trữ thông tin bổ sung. | Medium | Phase 1 |
| **FR-087** | Tách Kafka Topic và DLQ | Kafka phải sử dụng topic riêng cho Log Event chính và topic riêng cho Dead Letter Queue (DLQ), không được sử dụng chung một topic. | High | Phase 1 |

### Log Processing

| Mã FR | Tên Chức Năng | Mô Tả Yêu Cầu & Quy Tắc Nghiệp Vụ | Độ Ưu Tiên | Giai Đoạn |
| :--- | :--- | :--- | :---: | :---: |
| **FR-088** | Consume Log Events | Log Processor phải consume Log Event từ Kafka Main Topic để thực hiện xử lý bất đồng bộ. | High | Phase 1 |
| **FR-089** | Validate Log Event | Log Processor phải validate Enriched Log Event trước khi index vào OpenSearch. | High | Phase 1 |
| **FR-090** | Normalize Log Event | Log Processor phải normalize Log Event trước khi index để đảm bảo dữ liệu có cấu trúc và format nhất quán. | High | Phase 1 |
| **FR-091** | Batch Processing | Log Processor phải hỗ trợ xử lý log theo batch với `batchSize` có thể cấu hình. | High | Phase 1 |
| **FR-092** | Batch Flush Interval | Log Processor phải hỗ trợ flush batch theo khoảng thời gian (`flushInterval`) có thể cấu hình. | High | Phase 1 |
| **FR-093** | Index Log Events | Log Processor phải index các Log Event hợp lệ vào OpenSearch. | High | Phase 1 |
| **FR-094** | OpenSearch Bulk Indexing | Log Processor phải sử dụng OpenSearch Bulk API để index nhiều Log Event trong một request nhằm giảm số lượng request đến OpenSearch. | High | Phase 1 |
| **FR-095** | Retry Bulk Indexing | Log Processor phải retry toàn bộ Bulk Request khi request index thất bại hoàn toàn. | High | Phase 1 |
| **FR-096** | Full Bulk Failure to DLQ | Nếu toàn bộ Bulk Request vẫn thất bại sau khi retry, Log Processor phải đưa toàn bộ batch tương ứng vào DLQ. | High | Phase 1 |
| **FR-097** | Phát hiện Partial Bulk Failure | Log Processor phải kiểm tra kết quả của từng item trong Bulk Response để phát hiện các Log Event index thất bại một phần. | High | Phase 1 |
| **FR-098** | Partial Failure to DLQ | Khi xảy ra Partial Bulk Failure, Log Processor chỉ được đưa các item index thất bại vào DLQ. | High | Phase 1 |
| **FR-099** | Không đưa Successful Item vào DLQ | Log Processor không được đưa các item đã index thành công vào DLQ. | High | Phase 1 |
| **FR-100** | Processing Logging | Log Processor phải ghi log rõ ràng các thông tin như `batchSize`, `indexedCount`, `failedCount`, `retryAttempt` và `dlqResult` để hỗ trợ monitoring và troubleshooting. | Medium | Phase 1 |

### Dead Letter Queue

| Mã FR | Tên Chức Năng | Mô Tả Yêu Cầu & Quy Tắc Nghiệp Vụ | Độ Ưu Tiên | Giai Đoạn |
| :--- | :--- | :--- | :---: | :---: |
| **FR-101** | DLQ cho JSON không hợp lệ | Message không thể parse thành JSON hợp lệ phải được đưa vào DLQ để tránh làm gián đoạn quá trình xử lý các message khác. | High | Phase 1 |
| **FR-102** | DLQ cho Event không hợp lệ | Internal Log Event thiếu các field bắt buộc hoặc không thỏa mãn validation rules phải được đưa vào DLQ. | High | Phase 1 |
| **FR-103** | DLQ cho Indexing Failure | Log Event vẫn thất bại khi index vào OpenSearch sau khi đã thực hiện retry phải được đưa vào DLQ. | High | Phase 1 |
| **FR-104** | Lưu Failure Stage | DLQ Event phải lưu `failureStage` để xác định event thất bại ở bước nào trong quá trình xử lý. | High | Phase 1 |
| **FR-105** | Lưu Failure Reason | DLQ Event phải lưu `failureReason` để mô tả nguyên nhân khiến event xử lý thất bại. | High | Phase 1 |
| **FR-106** | Lưu Original Payload | DLQ Event phải lưu `originalPayload` của message để hỗ trợ debugging và xử lý lại event. | High | Phase 1 |
| **FR-107** | Lưu Kafka Metadata | DLQ Event nên lưu các thông tin Kafka metadata như `sourceTopic`, `partition` và `offset` để hỗ trợ truy vết message gốc. | Medium | Phase 1 |
| **FR-108** | Lưu Event và Tenant Context | DLQ Event nên lưu `eventId` và Tenant Context nếu các thông tin này có thể được parse từ message gốc. | Medium | Phase 1 |
| **FR-109** | Nền tảng cho Manual Replay | TraceFlow không xây UI hoặc tool replay DLQ đầy đủ trong phạm vi ban đầu, nhưng DLQ Event phải lưu đủ dữ liệu cần thiết để hỗ trợ replay thủ công hoặc replay tool trong tương lai. | Low | Phase 2 |

### Log Search

| Mã FR | Tên Chức Năng | Mô Tả Yêu Cầu & Quy Tắc Nghiệp Vụ | Độ Ưu Tiên | Giai Đoạn |
| :--- | :--- | :--- | :---: | :---: |
| **FR-110** | Tìm kiếm Log | Control API phải cung cấp endpoint cho phép tìm kiếm log theo Workspace và Project scope. | High | Phase 1 |
| **FR-111** | JWT Authentication | Search API phải yêu cầu JWT Authentication để xác thực người dùng trước khi thực hiện tìm kiếm. | High | Phase 1 |
| **FR-112** | Kiểm tra Project Access | Search API phải kiểm tra user có quyền truy cập Project trước khi thực hiện query log. | High | Phase 1 |
| **FR-113** | Tenant Scope Filtering | Search API phải luôn filter kết quả theo `workspaceId` và `projectId` để đảm bảo user chỉ truy cập được log thuộc đúng scope. | High | Phase 1 |
| **FR-114** | Lọc Log | Search API phải hỗ trợ filter theo `application`, `environment`, `level`, `service`, `traceId`, `correlationId` và `timeRange`. | High | Phase 1 |
| **FR-115** | Phân trang kết quả | Search API phải hỗ trợ pagination để giới hạn số lượng log trả về trong mỗi request. | High | Phase 1 |
| **FR-116** | Default Time Range | Search API phải sử dụng một time range mặc định hợp lý khi request không cung cấp `from` và `to`. | Medium | Phase 1 |
| **FR-117** | Bảo vệ OpenSearch | Search API không được expose OpenSearch endpoint trực tiếp cho End User hoặc Client Application. | High | Phase 1 |
| **FR-118** | Trả về Enriched Log | Search API phải trả về đầy đủ các Enriched Log Fields cần thiết cho Client Application, bao gồm Tenant Context và các thông tin log đã được chuẩn hóa. | High | Phase 1 |

### Error Response Và API Consistency

| Mã FR | Tên Chức Năng | Mô Tả Yêu Cầu & Quy Tắc Nghiệp Vụ | Độ Ưu Tiên | Giai Đoạn |
| :--- | :--- | :--- | :---: | :---: |
| **FR-119** | Format lỗi thống nhất | Public API và Internal API phải trả lỗi theo format thống nhất để client có thể xử lý validation error, unauthorized, forbidden, not found, conflict và service unavailable một cách nhất quán. | High | Phase 1 |
| **FR-120** | Validation Error Detail | Response lỗi validation phải mô tả được field lỗi và lý do lỗi; batch validation nên có khả năng chỉ ra item index tương ứng khi phù hợp. | High | Phase 1 |
| **FR-121** | Không rò rỉ thông tin nhạy cảm qua lỗi | Error response không được trả về secret, hash, connection string, stack trace hoặc thông tin nội bộ không cần thiết cho client. | High | Phase 1 |

### OpenSearch Schema Và Index Mapping

| Mã FR | Tên Chức Năng | Mô Tả Yêu Cầu & Quy Tắc Nghiệp Vụ | Độ Ưu Tiên | Giai Đoạn |
| :--- | :--- | :--- | :---: | :---: |
| **FR-122** | Log Document Schema | OpenSearch Log Document phải có schema rõ ràng cho các field chính như `eventId`, `timestamp`, `receivedAt`, `workspaceId`, `projectId`, `applicationId`, `environment`, `service`, `level`, `message`, `traceId`, `correlationId` và `metadata`. | High | Phase 1 |
| **FR-123** | Tenant Field Mapping | Các tenant field như `workspaceId`, `projectId`, `applicationId` và `environment` phải được mapping phù hợp để hỗ trợ exact match filtering trong Search API. | High | Phase 1 |
| **FR-124** | Time Field Mapping | Các field thời gian như `timestamp` và `receivedAt` phải được mapping phù hợp để hỗ trợ sort, range query và default time range. | High | Phase 1 |

### Health Check Và Operations Cơ Bản
| Mã FR | Tên Chức Năng | Mô Tả Yêu Cầu & Quy Tắc Nghiệp Vụ | Độ Ưu Tiên | Giai Đoạn |
| :--- | :--- | :--- | :---: | :---: |
| **FR-125** | Service Health Check | Mỗi service chính phải cung cấp health check phù hợp để xác định service đang hoạt động và các dependency quan trọng có sẵn sàng hay không. | High | Phase 1 |
| **FR-126** | Local Development Stack | Docker Compose phải hỗ trợ khởi chạy toàn bộ local stack gồm PostgreSQL, Kafka, OpenSearch, Control API, Ingestion API và Log Processor. | High | Phase 1 |
| **FR-127** | Kafka Topic Initialization | Các Kafka Topic cần thiết cho hệ thống phải được tự động tạo hoặc khởi tạo trong local development stack. | High | Phase 1 |
| **FR-128** | Local Environment Configuration | Hệ thống phải cung cấp cấu hình môi trường rõ ràng và nhất quán cho local development, bao gồm connection string, service endpoint, credential và các tham số cần thiết. | Medium | Phase 1 |
| **FR-129** | Validate Configuration Khi Khởi Động | Các service phải fail fast hoặc log lỗi rõ ràng khi thiếu cấu hình quan trọng như Kafka broker, Kafka topic, OpenSearch URL, JWT secret, Internal Service Secret hoặc API Key Pepper. | High | Phase 1 |
| **FR-130** | Kiểm tra Dependency Health | Health check nên phân biệt trạng thái service process và trạng thái dependency quan trọng như PostgreSQL, Kafka hoặc OpenSearch để hỗ trợ troubleshooting local stack. | Medium | Phase 1 |

## 7. Yêu Cầu Phi Chức Năng

### Performance

NFR-001: Ingestion API phải trả response nhanh sau khi publish Kafka thành công, không chờ OpenSearch indexing.

NFR-002: Log Processor phải hỗ trợ batch size và flush interval có thể cấu hình để cân bằng throughput và latency.

NFR-003: OpenSearch indexing phải hỗ trợ Bulk API để giảm số request khi xử lý nhiều log.

NFR-004: Hệ thống hoàn thiện phải có benchmark thể hiện logs/s throughput.

NFR-005: Hệ thống hoàn thiện phải có benchmark thể hiện P95 latency cho ingestion path hoặc end-to-end log availability.

NFR-006: Benchmark của TraceFlow phải đo ít nhất ba nhóm số liệu: ingestion response latency, end-to-end searchable latency và processor indexing throughput.

### Reliability

NFR-007: Kafka phải đóng vai trò buffer giữa ingestion và indexing.

NFR-008: OpenSearch tạm thời lỗi không được làm Ingestion API trực tiếp thất bại nếu Kafka vẫn nhận được message.

NFR-009: Log Processor phải retry các lỗi indexing phù hợp trước khi đưa event vào DLQ.

NFR-010: Invalid message không được làm nghẽn toàn bộ consumer loop.

NFR-011: DLQ phải lưu đủ thông tin để debug hoặc replay thủ công trong tương lai.

NFR-012: Processor logs phải đủ rõ để phân biệt success, retry, full failure và partial failure.

### Security

NFR-013: User-facing API phải dùng JWT authentication.

NFR-014: Log ingestion phải dùng API key authentication.

NFR-015: API key secret không được lưu plaintext.

NFR-016: API key full secret chỉ được trả về một lần khi tạo.

NFR-017: Internal service-to-service endpoint phải được bảo vệ.

NFR-018: Tenant context do client gửi lên không được tin cậy.

NFR-019: Search log phải luôn đi qua Control API để enforce access control.

NFR-020: OpenSearch không được expose trực tiếp cho end user.

### Multi-Tenancy

NFR-021: Workspace, project, application và environment phải được dùng làm ranh giới tenant context trong log pipeline.

NFR-022: User chỉ được search log trong project mà họ có quyền.

NFR-023: API key chỉ được ingest log cho trace application và environment tương ứng.

NFR-024: Log event trong OpenSearch phải có đủ tenant fields để search API enforce scope.

### Scalability

NFR-025: Ingestion API phải có khả năng scale ngang vì không giữ state nghiệp vụ dài hạn.

NFR-026: Log Processor phải có khả năng scale thông qua Kafka consumer group.

NFR-027: Kafka partitioning strategy phải hỗ trợ tăng throughput trong tương lai.

NFR-028: OpenSearch index strategy phải phù hợp với log search và retention.

### Maintainability

NFR-029: Boundary giữa Control Plane và Data Plane phải rõ ràng.

NFR-030: Control API không nhận high-throughput log ingestion trực tiếp.

NFR-031: Ingestion API không quản lý user, workspace, project hoặc application.

NFR-032: Log Processor không gọi PostgreSQL để lấy business metadata trong processing path.

NFR-033: Contract giữa các service phải được tài liệu hóa trước hoặc song song với implementation.

NFR-034: Các decision quan trọng phải được ghi lại trong system design hoặc ADR.

## 8. Ràng Buộc Hệ Thống

| Nhóm ràng buộc | Quyết định | Ý nghĩa |
| :--- | :--- | :--- |
| Control Plane | Control Plane sử dụng ASP.NET Core. | ASP.NET Core chịu trách nhiệm cho user-facing API, authentication, resource management, access control và log search. |
| Data Plane | Data Plane sử dụng Go. | Go được dùng cho các service xử lý luồng log như ingestion, Kafka producer/consumer và log processing. |
| Business Storage | PostgreSQL lưu business data và control metadata. | PostgreSQL là nguồn dữ liệu chính cho user, workspace, project, application, API key, membership và invitation. |
| Event Streaming | Kafka làm message broker và buffer cho log event. | Kafka tách ingestion khỏi processing/indexing, giúp hệ thống chịu tải tốt hơn và xử lý bất đồng bộ. |
| Search Storage | OpenSearch lưu log phục vụ search và analytics. | OpenSearch là backend chính cho log search, filtering, time range query và các truy vấn phân tích cơ bản. |
| Local Development | Docker Compose là môi trường local development chính. | Toàn bộ stack local phải có thể chạy được bằng Docker Compose để hỗ trợ phát triển, test và demo. |
| User-facing API | User-facing management/search API đi qua Control API. | User chỉ thao tác với hệ thống qua Control API, không truy cập trực tiếp database, Kafka hoặc OpenSearch. |
| Ingestion Entry Point | Client application chỉ gửi log qua Ingestion API. | Ingestion API là cổng duy nhất cho log submission từ application bên ngoài. |
| Metadata Exposure | Không service nào ngoài Control API được expose business metadata trực tiếp cho user. | Business metadata chỉ được truy cập qua lớp authorization và access control của Control API. |
| Supporting Infrastructure | Redis có thể được sử dụng cho API key validation cache, ingestion rate limiting và counter ngắn hạn. | Redis không phải source of truth; hệ thống core vẫn phải dựa trên PostgreSQL, Kafka và OpenSearch. |
| Archive Storage | Không có archive storage trong phạm vi ban đầu. | Log hết hạn không được archive sang object storage trong scope đầu tiên. |
| Deployment Scope | Không có Kubernetes deployment trong phạm vi ban đầu. | Project tập trung vào backend architecture và local/dev deployment thay vì production-grade orchestration. |

## 9. Tiêu Chí Thành Công

| Nhóm tiêu chí | Điều kiện đạt | Ý nghĩa |
| :--- | :--- | :--- |
| Identity & Session | User có thể đăng ký, đăng nhập, refresh session, logout và quản lý session cơ bản. | Người dùng có thể truy cập hệ thống bằng cơ chế xác thực rõ ràng. |
| Resource Management | User có thể tạo workspace, project, trace application và API key theo quyền được cấp. | Hệ thống có đầy đủ resource hierarchy để tổ chức tenant và log ownership. |
| API Key Security | API key được lưu an toàn, chỉ hiển thị secret một lần, có thể revoke, expire và validate. | Ingestion path có cơ chế xác thực riêng, không phụ thuộc JWT của user. |
| Single Log Ingestion | Client application có thể gửi single log hợp lệ bằng API key. | Hệ thống hỗ trợ use case gửi log cơ bản nhất. |
| Batch Log Ingestion | Client application có thể gửi batch logs hợp lệ bằng API key. | Hệ thống hỗ trợ ingestion hiệu quả hơn khi application gửi nhiều log cùng lúc. |
| Tenant Isolation | Ingestion API không tin tenant context từ client và tự enrich bằng context từ API key. | Client không thể spoof workspace, project, application hoặc environment. |
| Event Streaming | Valid log event được publish vào Kafka. | Ingestion được tách khỏi indexing, giúp pipeline xử lý bất đồng bộ. |
| Log Processing | Log Processor consume Kafka event, validate, batch và index log vào OpenSearch. | Log hợp lệ đi hết pipeline và được lưu vào search backend. |
| Bulk Indexing | Batch log processing và OpenSearch bulk indexing hoạt động đúng. | Processor có khả năng xử lý nhiều log hiệu quả hơn per-message indexing. |
| DLQ Handling | Invalid JSON, invalid internal event và indexing failure được đưa vào DLQ theo đúng failure stage. | Event lỗi không làm nghẽn pipeline và vẫn có dữ liệu để debug. |
| Bulk Failure Handling | Full bulk indexing failure được retry rồi đưa toàn batch vào DLQ nếu vẫn fail; partial failure chỉ đưa item lỗi vào DLQ. | Hệ thống xử lý được lỗi indexing mà không làm mất hoặc DLQ sai item đã thành công. |
| Log Search | User có thể search log thông qua Control API. | Người dùng có thể khai thác dữ liệu log sau khi được index. |
| Search Access Control | User chỉ search được log trong project mà họ có quyền truy cập. | Search path giữ đúng tenant isolation và resource-level RBAC. |
| Documentation | Hệ thống có tài liệu requirements, HLD, contracts, LLD, reliability, security, testing và roadmap rõ ràng. | Việc phát triển tiếp theo có thiết kế dẫn đường, không code mù. |
| Benchmark | Hệ thống có benchmark thể hiện throughput và P95 latency khi hoàn thiện. | Project có số liệu định lượng để chứng minh hiệu năng trong portfolio. |
