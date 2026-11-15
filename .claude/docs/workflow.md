# Workflow

## Nguyên tắc
1. **Roadmap là nguồn sự thật.** Mọi agent đọc nó trước khi làm và cập nhật sau khi làm.
2. **File là bộ nhớ chung.** Các agent/model không chia sẻ context; chúng chỉ giao tiếp qua 6 file của mỗi task.
3. **Hai cổng người ở BA**: chốt yêu cầu, rồi duyệt spec. Chưa qua hai cổng thì không code.
4. **Sprint có kiểm soát** (làm → review → xác nhận → làm tiếp). Không chạy vòng lặp tự động không điểm dừng.
5. **Chỉ ✅ khi đã verify.**

## Bức tranh tổng thể

```
/ba <yêu cầu>                        (hội thoại chính, nhiều vòng hỏi–đáp)
  BA hỏi → nháp 01 → [NGƯỜI CHỐT YÊU CẦU]
  Tách task → viết 02 cho từng task → [NGƯỜI DUYỆT SPEC]

/task-run <ID>                       (lặp cho từng task)
  Code   → Antigravity (MCP) → 03 → build xanh, diff đúng phạm vi
  Test   → 04 ── FAIL ──► quay lại Code
  Review → 05 ── REQUEST_CHANGES ──► quay lại Code → Test → Review
  Bàn giao → 06, cập nhật roadmap + changelog

/sprint-close                        (tổng kết, [NGƯỜI OK] → commit + git tag)
```

Quá 2 vòng sửa cho một task: đánh 🟡, ghi lý do, dừng và báo người dùng.

## Vai trò

| Giai đoạn | Thực hiện bởi | Đầu vào | Đầu ra | Cổng qua bước kế |
|---|---|---|---|---|
| BA (trao đổi) | skill `ba-requirements`, hội thoại chính; subagent `ba-analyst` khảo sát code | yêu cầu thô, roadmap | `01`, `02` | **Người chốt yêu cầu, người duyệt spec** |
| Code | `coder` + skill `antigravity-dispatch` → Antigravity | `02`, conventions | code + `03` | Build xanh, diff đúng phạm vi |
| Test | subagent `tester` | code, `02` | `04` | `VERDICT: PASS` |
| Review | subagent `reviewer` + skill `review-checklist` (Claude tự review) | diff, `02`, conventions | `05` | `VERDICT: APPROVE` |
| Bàn giao | skill `handover-writer` | `01`–`05` | `06` + cập nhật roadmap | — |

## Khi nào dùng agent, skill
- **Skill điểm vào** (`roadmap-init`, `ba`, `sprint-plan`, `task-run`, `sprint-close`) do người dùng gõ `/tên`; đặt `disable-model-invocation` nên Claude không tự chạy.
- **Skill hỗ trợ** (`ba-requirements`, `antigravity-dispatch`, `review-checklist`, `handover-writer`) là quy trình nạp khi cần, chạy trong hội thoại chính.
- **Rule** (`.claude/rules/`) là luật; `roadmap-sync` luôn nạp, các `convention-*` chỉ nạp khi sửa file khớp `paths`.
- **Subagent** là worker có context riêng, chạy một mạch rồi trả kết quả (khảo sát code, coder, tester, reviewer). Subagent không hỏi–đáp nhiều vòng với người dùng, nên phần trao đổi của BA nằm ở skill.
- **Hook** (`.claude/hooks/`) là script tự chạy theo sự kiện; hiện có `guard-secrets` chặn đọc/sửa file secret.

## Chạy song song (chỉ khi cần)
Mặc định các task chạy **tuần tự**. Chỉ chạy song song nhiều task/subagent khi:
- Mỗi task có phạm vi ghi file riêng, không chồng lên nhau (kiểm tra danh sách "file được phép sửa" trong `02`). Chồng lên thì chạy tuần tự.
- Tiêu chí nghiệm thu đã viết xong trước khi giao việc. Không viết được tiêu chí nghĩa là task chưa sẵn sàng.
- Mỗi worker chỉ commit đúng file của task mình.

Vai trò tương ứng: BA là điều phối (không code), `ba-analyst` là người khảo sát (chỉ đọc), `coder` là người thực thi trong phạm vi, `tester` + `reviewer` là người nghiệm thu. Một task chỉ coi là xong khi đủ: kết quả đã tích hợp, tóm tắt kiểm chứng (đã kiểm gì, chưa kiểm gì), commit id, ghi chú cập nhật, và đề xuất task kế tiếp kèm lý do (nằm trong `06-handover.md` và roadmap).

## Luồng một sprint

```
/sprint-plan [N]  → chọn N task đã có spec được duyệt, chờ OK
/task-run <ID>    → lặp cho từng task
/sprint-close     → tổng kết, chờ OK → commit + git tag sprint-<số>
```

## Bộ 6 file của mỗi task (`.claude/docs/tasks/<ID>-<slug>/`)

| File | Người viết | Nội dung |
|---|---|---|
| `01-requirement.md` | BA | Phân tích yêu cầu, luồng, quy tắc nghiệp vụ |
| `02-coding-spec.md` | BA | Tài liệu để coder làm theo (có dòng "✅ đã duyệt") |
| `03-implementation-log.md` | Coder | Đã sửa gì, quyết định kỹ thuật |
| `04-test-report.md` | Tester | Kết quả test đối chiếu tiêu chí |
| `05-review.md` | Reviewer | Phát hiện + verdict |
| `06-handover.md` | Claude chính | Bàn giao, giải thích luồng |
