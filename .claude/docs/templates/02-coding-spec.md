# <ID> — <Tên tính năng> · Coding Spec

- **Trạng thái duyệt:** ⬜ chờ duyệt / ✅ đã duyệt bởi <tên> ngày YYYY-MM-DD

## 1. Phạm vi
_Làm gì, trong một đoạn ngắn._

## 2. Ngoài phạm vi
- ...

<!-- TOOLING:BEGIN -->
## 2b. Phân tích ảnh hưởng (CodeGraph)
_Hàm/class/bảng sẽ đổi → nơi gọi và phụ thuộc bị ảnh hưởng (kết quả `codegraph_impact`/`codegraph_callers`). Ghi "không dùng CodeGraph" nếu không có._

| Đối tượng đổi | Bị ảnh hưởng | Cần cập nhật? |
|---|---|---|
| ... | ... | ... |
<!-- TOOLING:END -->

## 3. File được phép tạo/sửa
| Đường dẫn | Tạo/Sửa | Nội dung thay đổi |
|---|---|---|
| ... | Tạo | ... |

> Coder không được sửa file ngoài danh sách này nếu chưa báo lại.

## 4. Thay đổi cơ sở dữ liệu
_Script mới, bảng/cột/index, procedure. Không sửa script đã chạy._

## 5. API / giao diện
| Phương thức | Đường dẫn | Đầu vào | Đầu ra | Lỗi |
|---|---|---|---|---|
| ... | ... | ... | ... | ... |

## 6. Tên theo convention
_Liệt kê class, method, bảng, component cần tạo, đã đặt tên đúng `.claude/rules/convention-*.md`._

## 7. Luồng xử lý dự kiến
_Từ UI/API → service → DB, từng bước._

## 8. Tiêu chí nghiệm thu
| # | Tiêu chí (kiểm tra được) | Loại test |
|---|---|---|
| AC1 | ... | Unit |
| AC2 | ... | Unit |

## 9. Lệnh kiểm tra
- Build: `dotnet build BaseRepo.slnx` · Test: `dotnet test BaseRepo.slnx`
