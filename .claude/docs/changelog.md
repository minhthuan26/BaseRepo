# Changelog

Mỗi sprint khi đóng sẽ có một version và một git tag `sprint-<số>`. Rollback: quay về tag của version trước.

| Version | Ngày | Sprint | Nội dung |
|---|---|---|---|
| v0.0.0 | 2026-11-15 | — | Khởi tạo bộ workflow + khởi tạo roadmap |

## Nhật ký chi tiết

- 2026-11-15 — Khởi tạo bộ workflow (init-project).
- 2026-11-15 — N1 (ConfigEnv: env tùy chọn, SystemManagement): code xong + test tự động PASS + review APPROVE, nhưng còn chạy tay các AC cần UI/quyền Administrator nên giữ 🟡. Xem `tasks/N1-configenv-env-tuy-chinh/06-handover.md`.
- 2026-11-15 — N1 ✅: người dùng chạy tay checklist ConfigEnv (UI + quyền Administrator) và xác nhận đạt. Code ở commit `73ca84a`.
- 2026-11-15 — N2-1 (ConfigEnv: sửa biến cốt lõi + backup + khôi phục): code xong (2 vòng), test tự động PASS 54/54, review APPROVE, nhưng còn chạy tay các AC cần UI/quyền Administrator nên giữ 🟡. Xem `tasks/N2-1-backup-khoi-phuc-bien-loi/06-handover.md`.
- 2026-11-15 — N2-2 (ConfigEnv: trình sửa từng mục cho Path/PATHEXT/PSModulePath): code xong (1 vòng), test tự động PASS 37/37, review APPROVE, nhưng còn chạy tay các AC cần UI/quyền Administrator nên giữ 🟡. Xem `tasks/N2-2-trinh-sua-danh-sach/06-handover.md`.
- 2026-11-16 — N2-1, N2-2 ✅ (cha N2 ✅): người dùng chạy tay checklist ConfigEnv (sửa/khôi phục biến cốt lõi, trình sửa danh sách) và xác nhận đạt. Code ở commit `c73bdbb`, `052d90a`.
- 2026-11-16 — N5 ✅ (ConfigEnv: chuyển 4 form sang Designer, tách `FrmCustomEnvEditor` ra file riêng): code xong (1 vòng), test tự động PASS (harness so sánh cây control cũ/mới), review APPROVE, người dùng chạy tay đạt (kể cả DPI cao). Xem `tasks/N5-configenv-chuyen-form-sang-designer/06-handover.md`. Code ở commit `bc960f9`.
- 2026-11-16 — N6 (ConfigEnv: loader + chạy nền cho Lưu/Xóa/Khôi phục, sửa lỗi "trông như đơ"): code xong (1 vòng), test tự động PASS (harness 52/52), review APPROVE, nhưng còn chạy tay (loader, UI không đứng, bấm lặp) nên giữ 🟡. Xem `tasks/N6-configenv-loader-chay-nen/06-handover.md`. Code ở commit `8d9116e` (kèm `Frm_Base`), refactor tất cả form kế thừa `Frm_Base` + mô tả `FrmMain` ở `6e63d53`.
- 2026-11-16 — N4 (ConfigEnv hai lưới System/User, backup mọi thao tác): yêu cầu chốt, tách N4-1/N4-2, spec đã duyệt, chưa code. Docs ở commit `d4268cf`.
- 2026-11-16 — N4-1 (ConfigEnv: dịch vụ biến môi trường theo scope User/Machine + backup mọi thao tác, `Backups/` vào `.gitignore`): code xong (2 vòng: vòng 2 sửa `ListBackups(string)`/`RestoreCoreValue` không kiểm `Scope`), test tự động PASS 46/46, review APPROVE; giữ 🟡 vì nhánh Machine elevated chưa kiểm chứng (phiên test không có quyền Administrator). Xem `tasks/N4-1-dich-vu-env-theo-scope/06-handover.md`. Chưa commit.
- 2026-11-15 — Khởi tạo roadmap (roadmap-init): 7 module, việc tồn đọng M1–M21 rút từ code + Progress Log `claude.md`; mã N kế tiếp: N1.
