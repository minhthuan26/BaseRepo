---
name: review-checklist
description: Bộ tiêu chí và định dạng review code cho một task (đúng spec, đúng convention, hiệu quả, an toàn). Dùng ở bước Review, cho cả reviewer chạy bằng model ngoài lẫn tự review.
---

# Review checklist

Dự án: .NET 10 / ASP.NET Core Web API (Clean Architecture 5-layer + CQRS (MediatR)), FE: không áp dụng, DB: SQL Server (Dapper qua BaseDbContext).

## Nguyên tắc
- Chỉ đọc, không sửa code sản phẩm.
- Mỗi phát hiện phải **kiểm chứng bằng cách đọc code**, nêu `file:dòng`. Không phát hiện chung chung, không đoán.
- Đối chiếu với `02-coding-spec.md` và `.claude/rules/convention-*.md`, không áp quy ước cá nhân.
- Không bình luận lỗi định dạng đã có formatter/linter lo.

## 1. Đúng spec
- [ ] Làm đủ từng tiêu chí nghiệm thu AC1…ACn
- [ ] Không làm thêm ngoài phạm vi; mọi file đổi nằm trong danh sách được phép
- [ ] API/giao diện đúng đặc tả (đường dẫn, đầu vào, đầu ra, mã lỗi)

## 2. Đúng convention
- [ ] Đặt tên đúng `naming.md` (class, method, bảng, cột, component, file)
- [ ] Phân tầng đúng Clean Architecture 5-layer + CQRS (MediatR); controller mỏng, nghiệp vụ nằm ở tầng đúng
- [ ] DTO ở ranh giới API, không trả entity ra ngoài

## 3. Hiệu quả
- [ ] DB: không N+1, không `SELECT *`, có phân trang xác định, index phù hợp truy vấn mới, transaction ngắn
- [ ] Không gọi DB/API trong vòng lặp khi có thể gom lô
- [ ] async/await xuyên suốt, không `.Result`/`.Wait()`

## 4. An toàn
- [ ] Truy vấn tham số hóa, không nối chuỗi từ đầu vào
- [ ] Validate đầu vào ở ranh giới; kiểm tra quyền truy cập
- [ ] Không log hay trả ra dữ liệu nhạy cảm (thông tin bệnh nhân/người hiến)
- [ ] Không hardcode secret, chuỗi kết nối, URL môi trường

## 5. Bảo trì và test
- [ ] Mỗi tiêu chí nghiệm thu có test; test kiểm hành vi, không phụ thuộc DB thật
- [ ] Không xóa/làm yếu test hiện có
- [ ] Xử lý lỗi rõ ràng, không nuốt exception
- [ ] Không trùng lặp đáng kể, không code chết, không TODO không mã task
- [ ] Không sửa script SQL/migration đã chạy

## Mức độ
- **blocker:** sai chức năng, mất/lộ dữ liệu, lỗ hổng bảo mật, vỡ build/test, sửa script đã chạy.
- **major:** sai spec hoặc convention đáng kể, vấn đề hiệu năng thực sự, thiếu test cho tiêu chí nghiệm thu.
- **minor:** cải thiện nhỏ, gợi ý đặt tên/đơn giản hóa.

## Định dạng kết quả (`05-review.md` theo template)
Bảng phát hiện: mức độ, `file:dòng`, vấn đề, gợi ý sửa. Đánh giá ngắn theo 5 nhóm. Mục "Phạm vi ảnh hưởng" (nơi gọi tới code đã đổi có bị vỡ không). Dòng cuối:
- `VERDICT: REQUEST_CHANGES` nếu còn blocker hoặc major.
- `VERDICT: APPROVE` nếu chỉ còn minor (ghi vào "Đề xuất sau").

Không có phát hiện thật thì nói thẳng, không bịa phát hiện cho đủ số lượng. Chỉ review phần diff của task, không review cả codebase.

Tối đa 2 vòng review cho mỗi task.
