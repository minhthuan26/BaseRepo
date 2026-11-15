# N4-1 — ConfigEnv: dịch vụ biến môi trường theo phạm vi User/Machine + backup mọi thao tác · Code Review

- **Reviewer:** Claude (chế độ B, tự review) · **Ngày:** 2026-11-16 · **Vòng:** 2

## 1. Phát hiện
| Mức độ | File:dòng | Vấn đề | Gợi ý sửa |
|---|---|---|---|
| (không) | — | Không có blocker/major mới. Major vòng 1 đã được xử lý đúng (xem mục 2). | — |

Các minor vòng 1 còn lại (chấp nhận, không chặn): (1) `EnvRegistryService.ListAll` có race giữa `GetValueNames()` và `GetValueKind()`; (2) `RestoreVariable` validate key chưa đầy đủ như `CreateVariable`; (3) validate key trùng lặp giữa `CreateVariable`/`ValidateKey`. Xem đánh giá: không cái nào là blocker (nguồn dữ liệu là file backup do app sinh, race xác suất thấp, chỉ ảnh hưởng độ chặt).

## 2. Xác minh sửa major vòng 1
- `EnvBackupService.ListBackups(string key)` (dòng ~227-243): điều kiện nay là `entry != null && entry.Scope == EnvScope.Machine && key khớp (OrdinalIgnoreCase)`. Backup `Scope=User` không còn lọt vào `FrmEnvRestorePicker`/`UC_ConfigEnvCustom` (2 nơi duy nhất gọi). File cũ không có dòng `Scope=` được coi là Machine nên hành vi N1/N2 giữ nguyên.
- `CustomEnvService.RestoreCoreValue` (dòng 149-151): guard `entry.Scope != Machine` ném `InvalidOperationException` trước mọi thao tác registry, nên không thể ghi giá trị từ HKCU vào HKLM dù gọi trực tiếp. Phòng thủ chiều sâu hợp lý.
- Không phát sinh lỗi mới: thứ tự của `UpdateCoreValue`/`RestoreCoreValue` vẫn là kiểm quyền → validate → ReadRaw → kiểm kind → CreateBackup (lỗi thì dừng) → WriteRaw → Broadcast. `CreateBackup(key, ...)` cũ ủy quyền sang overload Machine, chữ ký `EnvBackupEntry` mở rộng bằng tham số mặc định nên nơi gọi cũ vẫn biên dịch. `ListBackups(EnvScope)` mới tách biệt, không đụng hàm cũ.
- Test vòng 2: 46/46 PASS (theo `04-test-report.md`), gồm ca bổ sung cho lỗi này.

## 3. Đánh giá theo tiêu chí
- **Đúng spec:** đạt AC9 (hàm cũ giữ hành vi Machine). Không làm thừa.
- **Convention / An toàn / Hiệu quả:** không có vấn đề mới; không log giá trị, backup đã nằm trong `.gitignore`.

<!-- TOOLING:BEGIN -->
## 3b. Phạm vi ảnh hưởng
Kiểm tra bằng Grep (không dùng CodeGraph): `ListBackups(string)` chỉ gọi ở `FrmEnvRestorePicker.cs:36` và `UC_ConfigEnvCustom.cs:115`; `RestoreCoreValue` chỉ gọi ở `UC_ConfigEnvCustom.cs:145` với entry lấy từ danh sách Machine. Không vỡ.
<!-- TOOLING:END -->

## 4. Đề xuất sau (minor, không chặn) / Việc tồn đọng
- Bọc `GetValueKind` trong `ListAll` bằng try/catch từng biến (race xóa biến đồng thời).
- Dùng chung hàm validate key cho `CreateVariable`/`RestoreVariable` (có thể làm ở N4-2).

VERDICT: APPROVE
