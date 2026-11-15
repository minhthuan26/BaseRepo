# N4-2 — ConfigEnv: giao diện hai lưới System / User · Test Report

- **Ngày:** 2026-11-16 · **Vòng:** 1
- Không có test project trong repo (`dotnet test` không áp dụng). Kiểm tra tự động = build + quét diff + harness WinForms tạm ngoài repo (STA, tham chiếu `SystemManagement.dll` vừa build, dùng reflection cho type internal; chỉ dùng file backup giả trong thư mục `Backups` cạnh harness, không đụng registry, không ghi HKLM/HKCU; đã dọn sạch sau khi chạy).

## 1. Đối chiếu tiêu chí nghiệm thu
| Tiêu chí | Test | Kết quả |
|---|---|---|
| AC1 | `dotnet build BaseRepo.slnx -m:1`: 0 Error, không `WFO1000`; `grep dgv_Envs` trong `SystemManagement` = 0; harness: UC không còn field/control `dgv_Envs`, dựng được UC (Designer chạy `InitializeComponent`) | ✅ |
| AC2 | Cấu trúc (harness): có `dgv_System`, `dgv_User`, `lbl_SystemTitle`, `lbl_UserTitle`, `spc_Containers` (SplitContainer ngang, mỗi lưới một panel); nhãn User chứa `UserDomainName\UserName`; cột đúng tên `col_SysKey/Value/Type`, `col_UserKey/Value/Type`; Type "BaseRepo" cho key `BaseRepo__*`. Nội dung thật của hai lưới từ registry: chưa chạy | ✅ cấu trúc / ⬜ thủ công |
| AC3 | Harness (nạp danh sách giả qua reflection + `ApplyFilter`): Tìm kiếm lọc cả hai lưới; ToggleMask đổi cả hai lưới (mặc định che). Làm mới thật: chưa chạy | ✅ logic / ⬜ thủ công |
| AC4 | Thêm vào User/System, chặn trùng: chưa chạy (cần UI + registry) | ⬜ chưa chạy |
| AC5 | Sửa/Xóa/Khôi phục + file backup: chưa chạy (dịch vụ đã verify ở N4-1) | ⬜ chưa chạy |
| AC6 | Biến cốt lõi, Path, không elevated: chưa chạy | ⬜ chưa chạy |
| AC7 | `BaseRepo__*` ở System (Administrator): chưa chạy | ⬜ chưa chạy |
| AC8 | `_activeGrid` (harness, reflection): mặc định System; `dgv_User_Enter` → User, nhãn User có ▶, System không; `dgv_System_Enter` → trả về System. Việc Add/Edit/Delete/Restore thực sự dùng lưới này: chưa chạy | ✅ theo dõi lưới / ⬜ thao tác |
| AC9 | Hồi quy `NeedsExtraWarning`, tab Mặc định: chưa chạy | ⬜ chưa chạy |
| AC10 | `git diff --name-only`: đúng 6 file code (`UC_ConfigEnvCustom(.Designer).cs`, `FrmCustomEnvEditor.cs`, `FrmEnvListEditor.cs`, `FrmEnvRestorePicker(.Designer).cs`) + spec; không `Console.`/`Log.`/`Debug.` trong dòng thêm; không `.resx` mới (git status không có) | ✅ |

Kiểm tra form (harness): `FrmEnvRestorePicker(User)` kế thừa `Frm_Base`, có cột `col_Key`, nạp `ListBackups(User)` từ 3 file giả (2 User, 1 Machine) → chỉ 2 dòng User, mới nhất trước; `Picker(Machine)` → 1 dòng. `FrmCustomEnvEditor` (thêm User / sửa Machine) và `FrmEnvListEditor` (User, Machine) khởi tạo được; cả hai kế thừa `Frm_Base`.

## 2. Kết quả chạy
- Build: `dotnet build BaseRepo.slnx -m:1` → 0 Error.
- Harness: 29 kiểm tra · Đạt: 29 · Lỗi: 0 (1 lần chạy đầu lỗi do lỗi của harness: `CustomEnvItem` là record không có ctor mặc định; đã sửa harness, không liên quan code sản phẩm).
- Harness (ngoài repo): `...\scratchpad\n42h\Program.cs`.

## 3. Test mới đã thêm
- Harness tạm ngoài repo, không thêm test vào repo.

## 4. Lỗi (nếu có)
Không có lỗi ở code sản phẩm.

## 5. Checklist thủ công cho người dùng (AC2–AC9, chạy bằng Administrator)
1. Trước tiên: `reg export "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Environment" env_machine.reg` và `reg export "HKCU\Environment" env_user.reg`. Chỉ thử bằng `N4_TEST_*`, hoặc biến an toàn (`NUMBER_OF_PROCESSORS`).
2. AC2: mở tab Tùy chỉnh, lưới System có đủ biến Machine (kể cả `BaseRepo__*`), lưới User có biến tài khoản hiện tại, tiêu đề User ghi tên tài khoản.
3. AC3: gõ tìm kiếm, lọc cả hai lưới; Hiện/Ẩn đổi cả hai; Làm mới tải lại cả hai.
4. AC4: Thêm `N4_TEST_UI` vào User, chỉ ở lưới User; vào System, chỉ ở System; trùng tên cùng lưới bị chặn, khác lưới thì được.
5. AC5: Sửa `N4_TEST_UI` (User): có `Backups/Env/User_N4_TEST_UI_*.txt` Reason=Edit giá trị cũ; Xóa: file Reason=Delete, biến mất; Khôi phục bản Delete: biến trở lại đúng giá trị và kiểu.
6. AC6: User `TEMP`: xóa bị chặn, sửa đòi gõ lại tên key; `Path` User mở `FrmEnvListEditor`; không elevated, Machine sửa/xóa/khôi phục báo cần Administrator, không tạo backup.
7. AC7: sửa/xóa một key `BaseRepo__*` ở System, rồi khôi phục lại ngay bằng nút Khôi phục.
8. AC8: bấm lưới User rồi Thêm/Sửa/Xóa/Khôi phục tác động User; bấm lưới System thì tác động System; lưới chọn không có dòng báo "Vui lòng chọn một biến".
9. AC9: sửa biến thường Machine vẫn hỏi `NeedsExtraWarning`; tab Mặc định hoạt động như cũ.
10. Cuối cùng: đối chiếu với hai file `.reg`, xóa mọi `N4_TEST_*`.

PASS nghĩa là phần tự động đạt; AC4–AC7, AC9 và phần thao tác của AC2/AC3/AC8 vẫn cần người dùng chạy thủ công.

VERDICT: PASS
