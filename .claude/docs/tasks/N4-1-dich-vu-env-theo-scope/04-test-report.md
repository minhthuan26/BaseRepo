# N4-1 — ConfigEnv: dịch vụ biến môi trường theo phạm vi User/Machine + backup mọi thao tác · Test Report

- **Ngày:** 2026-11-16 · **Vòng:** 2 (chạy lại sau vòng sửa 1: coder sửa lỗi major của review - `ListBackups(string key)` không lọc Scope, `RestoreCoreValue` không kiểm Scope)

## 1. Đối chiếu tiêu chí nghiệm thu
Repo không có test project (ngoài phạm vi spec). Dùng harness console tạm **ngoài repo** (scratchpad của phiên, `n4h/`, cập nhật thêm ca hồi quy), compile trực tiếp 3 file service. Registry chỉ key `N4_TEST_*` ở HKCU; backup của harness ghi cạnh exe của harness.

| Tiêu chí | Test (harness) | Kết quả |
|---|---|---|
| AC1 | `TryRead`: file cũ không `Scope` -> Machine; `Scope=User` -> User; `Scope=Foo` -> null; `Scope=7` (số không định nghĩa) -> null | ✅ (4/4) |
| AC2 | `CreateBackup(User, ...)` tên `User_N4_TEST_A_<ts>.txt`, có dòng `Scope=User`; gọi 2 lần cùng giây ra `_1` không ghi đè; hàm 4 tham số cũ tạo `Machine_...` | ✅ (4/4) |
| AC3 | `ListBackups(User)` chỉ User, mới nhất trước, bỏ file hỏng; `ListBackups(Machine)` gồm file cũ không `Scope`, không lẫn User | ✅ (4/4) |
| AC4 | `CreateVariable(User)` -> `GetVariables(User)` thấy key, kind/scope đúng, sắp xếp theo Key; trùng cùng scope bị chặn; `%` -> ExpandString giữ chuỗi thô; key chứa `=`/khoảng trắng đầu và value có `\n` bị từ chối, registry không đổi | ✅ (7/7) |
| AC5 | `UpdateVariable(User)`: backup `Edit` chứa giá trị cũ + kind cũ, giá trị mới ghi đúng, ExpandString giữ nguyên và `%...%` không bị mở rộng; key không tồn tại -> `KeyNotFoundException` | ✅ (3/3) |
| AC6 | `DeleteVariable(User)`: backup `Delete`, `ReadRaw` sau đó null; không tồn tại -> `KeyNotFoundException`; `Path` -> `InvalidOperationException`, không tạo file backup, không chạm registry | ✅ (4/4) |
| AC7 | `RestoreVariable`: backup `Delete` tạo lại đúng giá trị + kind, không sinh `BeforeRestore` khi biến vắng; khi biến còn thì có backup `BeforeRestore` chứa giá trị hiện tại rồi ghi đè; key có khoảng trắng đầu và value có `\n` bị từ chối, registry không đổi | ✅ (6/6) |
| AC8 | Không elevated (`elevated=False`): `Create/Update/Delete/RestoreVariable(Machine)` đều ném `UnauthorizedAccessException`; số file backup không đổi, HKLM `N4_TEST_M` không xuất hiện; `RequiresElevation` đúng. Không ghi HKLM | ✅ (7/7) |
| AC9 | `dotnet build BaseRepo.slnx -m:1`: Build succeeded, 0 Error (913 Warning có sẵn); hàm cũ `ReadRaw(string)`, `WriteRaw(string,...)`, `DeleteRaw(string)`, `CreateBackup` 4 tham số còn nguyên (diff chỉ thêm overload); không có lỗi `WFO1000` | ✅ |
| HỒI QUY (a) | Tạo backup `User` + `Machine` + file cũ không Scope cho key `N4_TEST_R`; `ListBackups(string key)` không trả backup User, trả đúng backup Machine và file cũ (Scope=Machine); tổng đúng 2 | ✅ (4/4) |
| HỒI QUY (b) | `RestoreCoreValue` với entry `Scope=User`, Key=`Path` -> `InvalidOperationException` (ném trước mọi ghi; phiên không elevated nên không thể ghi HKLM); HKLM `Path` đọc trước/sau giống nhau; số file backup không đổi | ✅ (3/3) |
| AC10 | `git diff --name-only` đúng 4 file (`.gitignore` + 3 service); `git check-ignore -v Backups/Env/x.txt` -> `.gitignore:175:Backups/`; grep diff không có `Console.`/`Log.`/`Debug.` ở dòng thêm | ✅ |

## 2. Kết quả chạy
- Lệnh: harness `dotnet run -c Release` (scratchpad) + `dotnet build BaseRepo.slnx -m:1` + lệnh git.
- Tổng: 46 kiểm tra harness (39 cũ + 7 hồi quy) · Đạt: 46 · Lỗi: 0 · Bỏ qua: 0. Build 0 Error.
- Dọn dẹp: `cleanup leftover keys=0`, thư mục `Backups` của harness đã xóa. Không chạm biến thật nào (Path chỉ được đọc ở HKLM để so sánh trước/sau, không ghi; Path chỉ dùng để kiểm tra bị chặn trước khi chạm registry).

## 3. Test mới đã thêm
- Harness tạm ngoài repo (không thuộc diff): `...\scratchpad\n4h\Program.cs`, `n4h.csproj`.

## 4. Lỗi (nếu có)
Không có.

## 5. Ghi chú
- Machine elevated không được test (phiên không elevated); AC8 xác nhận bằng nhánh không elevated đúng theo spec.
- Không sửa code sản phẩm.

VERDICT: PASS
