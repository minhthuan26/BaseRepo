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
- 2026-11-16 — N4 (ConfigEnv hai lưới System/User, backup mọi thao tác): yêu cầu chốt, tách N4-1/N4-2, spec đã duyệt, chưa code. Docs ở commit `d4268cf`.
- 2026-11-15 — Khởi tạo roadmap (roadmap-init): 7 module, việc tồn đọng M1–M21 rút từ code + Progress Log `claude.md`; mã N kế tiếp: N1.
