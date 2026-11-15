---
name: handover-writer
description: Viết tài liệu bàn giao (06-handover.md) giải thích luồng xử lý của task và cập nhật roadmap/changelog. Dùng ở bước cuối của /task-run, sau khi test PASS và review APPROVE.
---

# Viết bàn giao

## Điều kiện
- `04-test-report.md` kết thúc `VERDICT: PASS` và `05-review.md` kết thúc `VERDICT: APPROVE`. Chưa đạt thì **không viết bàn giao**; quay lại quy trình hoặc báo người dùng.

## Việc cần làm
1. Đọc `01`–`05` của task và `git diff` từ commit gốc (xem `03-implementation-log.md`).
2. Viết `.claude/docs/tasks/<ID>-<slug>/06-handover.md` theo `.claude/docs/templates/06-handover.md`.
3. Cập nhật roadmap và changelog theo `.claude/rules/roadmap-sync.md` Trigger 1.

## Cách giải thích luồng xử lý
- **Đi theo code thật**, không theo spec: mở các file đã đổi và lần theo đường chạy từ điểm vào (UI/API) qua từng lớp đến DB rồi quay lại.
- Mỗi bước nêu `file:dòng` hoặc tên class/method, việc nó làm, dữ liệu vào/ra.
- Nêu nơi có truy vấn hoặc thay đổi DB (bảng, cột, script), nơi validate, nơi kiểm tra quyền, nơi xử lý lỗi.
- Có sơ đồ văn bản ngắn của luồng chính.
- Người đọc là lập trình viên khác chưa biết task này: định nghĩa thuật ngữ nghiệp vụ lần đầu xuất hiện.

## Phần "Cách chạy và kiểm tra"
Ghi **lệnh chính xác** đã chạy (`dotnet test BaseRepo.slnx`), kết quả thật (số test), và các bước kiểm tra thủ công nếu có.

## Trung thực
- Chỉ ghi điều đã được kiểm chứng. Điều chưa kiểm chứng ghi rõ "chưa kiểm chứng".
- Điểm lệch so với spec, quyết định kỹ thuật đáng chú ý, rủi ro, việc minor chưa làm: ghi đủ vào "Rủi ro" và "Việc tồn đọng".
- Tính năng mới phát sinh trong lúc làm: thêm vào roadmap với mã `N`, không làm ngầm.

## Kết thúc
Nếu task là con của một tính năng lớn: cập nhật dòng cha (🟡 hoặc ✅ khi mọi task con xong). Báo người dùng đường dẫn `06-handover.md`.
