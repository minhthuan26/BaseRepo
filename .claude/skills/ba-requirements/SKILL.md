---
name: ba-requirements
description: Chế độ BA. Trao đổi nhiều vòng với người dùng để làm rõ yêu cầu, chốt yêu cầu, rồi tách thành task kèm coding-spec để giao cho coder. Dùng khi người dùng có yêu cầu/tính năng mới hoặc muốn đổi yêu cầu, trước khi bất kỳ task nào được code.
---

# BA — Trao đổi, chốt yêu cầu, tách task

Chạy **trong hội thoại chính** (không dùng subagent), vì cần hỏi–đáp nhiều vòng với người dùng. Khi cần khảo sát code, giao cho subagent `ba-analyst` (chỉ đọc) rồi dùng kết quả của nó.

## Đầu vào
Yêu cầu thô của người dùng (`$ARGUMENTS` nếu gọi qua `/ba`), `.claude/docs/roadmap.md`, `CLAUDE.md`, `.claude/rules/convention-*.md`.

## Quy trình

### 1. Nắm bối cảnh
- Đọc roadmap để biết yêu cầu này nằm ở module nào, đã có gì, trùng hoặc phụ thuộc gì.
- Khảo sát hiện trạng code liên quan (nhờ `ba-analyst`; dùng CodeGraph nếu có). **Không hỏi người dùng những điều đọc được từ code.**
- Tóm tắt hiểu biết trong 3–5 dòng và xin xác nhận.

### 2. Hỏi theo vòng
Dùng AskUserQuestion, tối đa 4 câu mỗi vòng, tối đa 3 vòng trước khi nháp. Mỗi vòng chỉ hỏi những gì còn thiếu, thuộc các nhóm:
1. Mục tiêu và người dùng: ai dùng, giải quyết vấn đề gì, thế nào là thành công.
2. Luồng chính: các bước từ đầu đến cuối.
3. Quy tắc nghiệp vụ: điều kiện, tính toán, trạng thái, ràng buộc.
4. Trường hợp biên và lỗi: dữ liệu trống, trùng, sai, đồng thời, quyền.
5. Dữ liệu và bảo mật: trường nhạy cảm (y tế, cá nhân), ai được xem/sửa, cần ghi log/kiểm toán không.
6. Phụ thuộc và tích hợp: module khác, hệ thống ngoài, thứ cần 🔑.
7. Phạm vi: cái gì **không** làm lần này.

### 3. Nháp yêu cầu
Viết `.claude/docs/tasks/<ID>-<slug>/01-requirement.md` theo `.claude/docs/templates/01-requirement.md`. Câu chưa rõ ghi ở mục "Câu hỏi còn mở", không tự đoán. Trình bày bản nháp, sửa theo phản hồi, lặp lại bước 2–3 nếu cần.

### 4. Cổng chốt 1 — chốt yêu cầu
Hỏi rõ: "Bạn chốt yêu cầu này chưa?". **Chưa có chữ "chốt" của người dùng thì không tách task.** Mục "Câu hỏi còn mở" phải trống hoặc người dùng chấp nhận để lại.

### 5. Tách task
Quy tắc tách:
- Mỗi task là một **lát cắt dọc** làm xong chạy được và test được độc lập (DB + API + UI nếu cần), không tách ngang theo tầng.
- Cỡ vừa cho một lượt code: thường sửa/tạo không quá ~8 file; lớn hơn thì tách tiếp.
- Mỗi task có tiêu chí nghiệm thu kiểm tra được (có thể viết thành test).
- Khai báo phụ thuộc giữa các task; không phụ thuộc vòng. Sắp thứ tự làm.
- Việc cần 🔑 hoặc hạ tầng ngoài tách thành task riêng.
- Đặt mã: nếu chỉ một task thì dùng mã roadmap (`M03`/`N12`); nếu tách nhiều thì `M03-1`, `M03-2`… Dòng cha trong roadmap giữ 🟡 đến khi mọi task con xong.

Với mỗi task: tạo `.claude/docs/tasks/<ID>-<slug>/` và viết `02-coding-spec.md` theo template (phạm vi, ngoài phạm vi, **danh sách file được sửa**, thay đổi DB, API, tên theo convention, tiêu chí nghiệm thu, lệnh kiểm tra). Cập nhật roadmap theo `.claude/rules/roadmap-sync.md` Trigger 2 (thêm mã mới, vị trí ưu tiên).

### 6. Cổng chốt 2 — duyệt spec
Trình bày danh sách task (ID, tên, phụ thuộc, thứ tự) và tóm tắt từng spec. Khi người dùng duyệt: ghi "✅ đã duyệt bởi <người dùng> ngày <ngày>" vào đầu mỗi `02-coding-spec.md`, rồi **dừng**. Không tự gọi coder; người dùng chạy `/task-run <ID>` hoặc `/sprint-plan`.

## Chất lượng yêu cầu — tự kiểm trước khi trình bày
- [ ] Mỗi tiêu chí nghiệm thu đo được, không dùng từ mơ hồ ("nhanh", "thân thiện")
- [ ] Đã nêu trường hợp biên và lỗi
- [ ] Đã nêu dữ liệu nhạy cảm và quyền truy cập
- [ ] Đã nêu rõ ngoài phạm vi
- [ ] Danh sách file được sửa đầy đủ và khớp với thiết kế
- [ ] Tên đối tượng đúng `.claude/rules/convention-*.md`

## Không làm
- Không viết code sản phẩm.
- Không mở rộng phạm vi; ý tưởng phát sinh ghi thành đề xuất mã `N` mới.
- Không giao việc cho coder trước khi người dùng duyệt spec.
- Không đoán khi thiếu thông tin: hỏi hoặc ghi vào câu hỏi còn mở.
