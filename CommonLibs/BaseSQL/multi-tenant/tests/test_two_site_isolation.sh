#!/usr/bin/env bash
#
# Test cách ly 2 Site (README mục "Không nằm trong các script này" -> "Viết test cách ly 2
# Site (UserId/RoleId trùng tên nhau giữa 2 Site, xác nhận không có API nào trả chéo dữ liệu)").
#
# Kịch bản: dùng fixture do seed_two_site_isolation.sql tạo - 2 Site (ISOTEST_A, ISOTEST_B),
# mỗi Site có 1 User "isotest_user" và 1 Role "ISOTEST_ROLE" TRÙNG TÊN nhưng là 2 bản ghi hoàn
# toàn độc lập (khác AutoId/SiteId), Role của Site A được cấp thêm Function "ToggleSiteModule"
# mà Role của Site B thì KHÔNG. Test này gọi thẳng API (giống quy ước manual/HTTP-based trong
# .claude/rules/06-testing.md - repo không có xUnit/NUnit project) để xác nhận không endpoint
# nào vô tình trả/ghi dữ liệu chéo Site chỉ vì UserId/RoleId trùng chuỗi.
#
# ⚠️ YÊU CẦU TRƯỚC KHI CHẠY:
#   1. Gateway đang chạy (mặc định http://localhost:80, theo Gateway/Properties/launchSettings.json)
#      và Site service đang chạy trực tiếp ở cổng nội bộ (mặc định http://localhost:8001, theo
#      Site/Properties/launchSettings.json) - 2 action ProvisionSiteModule/GetSiteModulesBySite
#      KHÔNG có route YARP nên phải gọi thẳng cổng Site, xem SiteModuleController.cs.
#   2. Đã chạy xong toàn bộ migration 00-08 trong CommonLibs/BaseSQL/multi-tenant/ theo đúng
#      thứ tự ghi trong README.md - ĐẶC BIỆT 08_fix_roleid_unique_scope.sql, nếu chưa chạy thì
#      RoleId vẫn unique toàn cục và toàn bộ tiền đề "RoleId trùng tên giữa 2 Site" của test này
#      không thể xảy ra ở tầng DB.
#   3. Đã chạy seed_two_site_isolation.sql (cùng thư mục) để tạo fixture. Chạy xong test muốn dọn
#      dẹp thì chạy cleanup_two_site_isolation.sql.
#   4. Cần curl, jq, base64 trên PATH.
#
# Cách chạy:
#   GATEWAY_URL=http://localhost:80 SITE_URL=http://localhost:8001 bash test_two_site_isolation.sh
# (2 biến môi trường trên là optional - mặc định đúng theo launchSettings.json của repo.)

set -uo pipefail

GATEWAY_URL="${GATEWAY_URL:-http://localhost:80}"
SITE_URL="${SITE_URL:-http://localhost:8001}"
ISOTEST_PASSWORD="${ISOTEST_PASSWORD:-admin2604}"

TMP_DIR="$(mktemp -d)"
trap 'rm -rf "$TMP_DIR"' EXIT

PASS_COUNT=0
FAIL_COUNT=0
SKIP_COUNT=0

# ---------- Helpers in/output ----------
section() { printf '\n\033[1;34m== %s ==\033[0m\n' "$1"; }
pass() { PASS_COUNT=$((PASS_COUNT + 1)); printf '\033[0;32m[PASS]\033[0m %s\n' "$1"; }
fail() { FAIL_COUNT=$((FAIL_COUNT + 1)); printf '\033[0;31m[FAIL]\033[0m %s\n' "$1"; }
skip() { SKIP_COUNT=$((SKIP_COUNT + 1)); printf '\033[0;33m[SKIP]\033[0m %s\n' "$1"; }

require_tool() {
    if ! command -v "$1" >/dev/null 2>&1; then
        echo "Thiếu công cụ bắt buộc: $1. Cài đặt rồi chạy lại." >&2
        exit 1
    fi
}
require_tool curl
require_tool jq
require_tool base64

# Giải mã 1 segment base64url của JWT (không cần verify chữ ký - chỉ đọc claim để so sánh, đúng
# mức cần thiết cho việc xác nhận cách ly, không phải xác thực bảo mật token).
b64url_decode() {
    local input="$1"
    input="${input//-/+}"
    input="${input//_/\/}"
    local mod=$(( ${#input} % 4 ))
    if [ "$mod" -eq 2 ]; then input="${input}=="; elif [ "$mod" -eq 3 ]; then input="${input}="; fi
    printf '%s' "$input" | base64 -d 2>/dev/null || printf '%s' "$input" | base64 --decode 2>/dev/null
}

jwt_claim() {
    local token="$1" claim="$2"
    local payload
    payload="$(echo "$token" | cut -d '.' -f2)"
    b64url_decode "$payload" | jq -r --arg c "$claim" '.[$c] // empty' 2>/dev/null
}

# call METHOD PATH BASE_URL BODY_JSON_OR_EMPTY TOKEN_OR_EMPTY -> ghi status vào $STATUS, body vào $BODY_FILE
call() {
    local method="$1" path="$2" base="$3" body="$4" token="$5"
    BODY_FILE="$TMP_DIR/resp_$$_$RANDOM.json"
    local args=(-s -o "$BODY_FILE" -w '%{http_code}' -X "$method" "${base}${path}")
    args+=(-H "Content-Type: application/json")
    if [ -n "$token" ]; then args+=(-H "Authorization: Bearer $token"); fi
    if [ -n "$body" ]; then args+=(-d "$body"); fi
    STATUS="$(curl "${args[@]}")"
}

assert_status() {
    local expected="$1" desc="$2"
    if [ "$STATUS" = "$expected" ]; then
        pass "$desc (HTTP $STATUS)"
        return 0
    else
        fail "$desc (mong đợi HTTP $expected, nhận HTTP $STATUS) - body: $(cat "$BODY_FILE" 2>/dev/null | head -c 500)"
        return 1
    fi
}

assert_eq() {
    local actual="$1" expected="$2" desc="$3"
    if [ "$actual" = "$expected" ]; then
        pass "$desc (= '$expected')"
    else
        fail "$desc (mong đợi '$expected', nhận '$actual')"
    fi
}

assert_ne() {
    local actual="$1" not_expected="$2" desc="$3"
    if [ "$actual" != "$not_expected" ]; then
        pass "$desc (khác '$not_expected', nhận '$actual')"
    else
        fail "$desc (KHÔNG được bằng '$not_expected' nhưng lại bằng)"
    fi
}

assert_contains() {
    local haystack="$1" needle="$2" desc="$3"
    if printf '%s' "$haystack" | grep -qF "$needle"; then
        pass "$desc (có chứa '$needle')"
    else
        fail "$desc (THIẾU '$needle' - nhận: $haystack)"
    fi
}

assert_not_contains() {
    local haystack="$1" needle="$2" desc="$3"
    if printf '%s' "$haystack" | grep -qF "$needle"; then
        fail "$desc (KHÔNG được chứa '$needle' nhưng lại có - đây là rò rỉ chéo Site - nhận: $haystack)"
    else
        pass "$desc (đúng - không thấy '$needle')"
    fi
}

# ================================================================================================
section "0) Đăng nhập isotest_user ở CẢ 2 Site (UserId trùng chuỗi, khác Site)"
# ================================================================================================

call POST "/api/authorization/login" "$GATEWAY_URL" \
    "{\"userId\":\"isotest_user\",\"password\":\"$ISOTEST_PASSWORD\",\"siteId\":\"ISOTEST_A\"}" ""
if ! assert_status 200 "Login isotest_user @ Site A"; then
    echo "" >&2
    echo "Không đăng nhập được isotest_user ở ISOTEST_A. Kiểm tra lại:" >&2
    echo "  - Gateway có đang chạy ở $GATEWAY_URL không (đổi qua biến môi trường GATEWAY_URL nếu khác)." >&2
    echo "  - Đã chạy seed_two_site_isolation.sql chưa (cùng thư mục)." >&2
    echo "  - Đã chạy đủ migration 00-08 theo README.md chưa." >&2
    exit 1
fi
TOKEN_A="$(jq -r '.data.accessToken // .Data.AccessToken // empty' "$BODY_FILE")"
if [ -z "$TOKEN_A" ]; then
    # Fallback: 1 số cấu hình serialize camelCase khác - thử đọc case-insensitive qua jq generic.
    TOKEN_A="$(jq -r '.. | .accessToken? // empty' "$BODY_FILE" | head -n1)"
fi
[ -n "$TOKEN_A" ] && pass "Lấy được AccessToken cho Site A" || { fail "Không tìm thấy AccessToken trong response login Site A"; exit 1; }

call POST "/api/authorization/login" "$GATEWAY_URL" \
    "{\"userId\":\"isotest_user\",\"password\":\"$ISOTEST_PASSWORD\",\"siteId\":\"ISOTEST_B\"}" ""
assert_status 200 "Login isotest_user @ Site B"
TOKEN_B="$(jq -r '.data.accessToken // .Data.AccessToken // empty' "$BODY_FILE")"
[ -z "$TOKEN_B" ] && TOKEN_B="$(jq -r '.. | .accessToken? // empty' "$BODY_FILE" | head -n1)"
[ -n "$TOKEN_B" ] && pass "Lấy được AccessToken cho Site B" || { fail "Không tìm thấy AccessToken trong response login Site B"; exit 1; }

section "1) Xác nhận claim trong token: cùng UserId text nhưng KHÁC AutoId/SiteId"

SITE_ID_CLAIM_A="$(jwt_claim "$TOKEN_A" site_id)"
SITE_ID_CLAIM_B="$(jwt_claim "$TOKEN_B" site_id)"
AUTO_ID_A="$(jwt_claim "$TOKEN_A" auto_id)"
AUTO_ID_B="$(jwt_claim "$TOKEN_B" auto_id)"

assert_eq "$SITE_ID_CLAIM_A" "ISOTEST_A" "Claim site_id trong token Site A"
assert_eq "$SITE_ID_CLAIM_B" "ISOTEST_B" "Claim site_id trong token Site B"
assert_ne "$AUTO_ID_A" "$AUTO_ID_B" "auto_id (khoá cache Redis) của 2 User trùng UserId phải khác nhau"
assert_ne "$AUTO_ID_A" "" "auto_id claim Site A không rỗng"
assert_ne "$AUTO_ID_B" "" "auto_id claim Site B không rỗng"

# ================================================================================================
section "2) GetRolesByUser - User trùng UserId chỉ thấy Role ĐÚNG Site của mình"
# ================================================================================================
# GET /api/user-role/{userId} - SiteId luôn lấy từ CurrentSiteId (không nhận qua tham số), xem
# UserRoleController.GetRolesByUser.

call GET "/api/user-role/isotest_user" "$GATEWAY_URL" "" "$TOKEN_A"
assert_status 200 "GetRolesByUser(isotest_user) với token Site A"
ROLES_A="$(cat "$BODY_FILE")"
assert_contains "$(echo "$ROLES_A" | jq -c '.data // .Data')" '"ISOTEST_A"' "Role trả về cho Site A có SiteId=ISOTEST_A"
assert_not_contains "$(echo "$ROLES_A" | jq -c '.data // .Data')" '"ISOTEST_B"' "Role trả về cho Site A KHÔNG được lẫn SiteId=ISOTEST_B"

call GET "/api/user-role/isotest_user" "$GATEWAY_URL" "" "$TOKEN_B"
assert_status 200 "GetRolesByUser(isotest_user) với token Site B"
ROLES_B="$(cat "$BODY_FILE")"
assert_contains "$(echo "$ROLES_B" | jq -c '.data // .Data')" '"ISOTEST_B"' "Role trả về cho Site B có SiteId=ISOTEST_B"
assert_not_contains "$(echo "$ROLES_B" | jq -c '.data // .Data')" '"ISOTEST_A"' "Role trả về cho Site B KHÔNG được lẫn SiteId=ISOTEST_A"

# ================================================================================================
section "3) GetRole - RoleId trùng tên phải resolve về ĐÚNG bản ghi của Site người gọi"
# ================================================================================================
# GET /api/role/{roleId} - roleId="ISOTEST_ROLE" tồn tại độc lập ở CẢ 2 Site. Nếu Repository chỉ
# lọc theo RoleId (không kèm SiteId của người gọi) thì có thể trả nhầm RoleName của Site kia.

call GET "/api/role/ISOTEST_ROLE" "$GATEWAY_URL" "" "$TOKEN_A"
assert_status 200 "GetRole(ISOTEST_ROLE) với token Site A"
ROLE_NAME_A="$(jq -r '.data.roleName // .Data.RoleName // empty' "$BODY_FILE")"
ROLE_SITE_A="$(jq -r '.data.siteId // .Data.SiteId // empty' "$BODY_FILE")"
assert_eq "$ROLE_SITE_A" "ISOTEST_A" "GetRole trả đúng SiteId=ISOTEST_A cho token Site A"
assert_contains "$ROLE_NAME_A" "Role A" "GetRole trả đúng RoleName của Site A (không lẫn Role B)"

call GET "/api/role/ISOTEST_ROLE" "$GATEWAY_URL" "" "$TOKEN_B"
assert_status 200 "GetRole(ISOTEST_ROLE) với token Site B"
ROLE_NAME_B="$(jq -r '.data.roleName // .Data.RoleName // empty' "$BODY_FILE")"
ROLE_SITE_B="$(jq -r '.data.siteId // .Data.SiteId // empty' "$BODY_FILE")"
assert_eq "$ROLE_SITE_B" "ISOTEST_B" "GetRole trả đúng SiteId=ISOTEST_B cho token Site B"
assert_contains "$ROLE_NAME_B" "Role B" "GetRole trả đúng RoleName của Site B (không lẫn Role A)"

# ================================================================================================
section "4) GetAllRoleFunctions - Function cấp LỆCH giữa 2 Site dù RoleId trùng tên"
# ================================================================================================
# Seed: Role A (ISOTEST_ROLE @ ISOTEST_A) có ToggleSiteModule + CreateUserRole.
#       Role B (ISOTEST_ROLE @ ISOTEST_B) chỉ có CreateUserRole.

call GET "/api/role-function?roleId=ISOTEST_ROLE&pageSize=50" "$GATEWAY_URL" "" "$TOKEN_A"
assert_status 200 "GetAllRoleFunctions(ISOTEST_ROLE) với token Site A"
RF_A="$(jq -c '[(.data // .Data).records[]? // (.data // .Data).Records[]? | (.functionId // .FunctionId)]' "$BODY_FILE")"
assert_contains "$RF_A" "ToggleSiteModule" "Role A có Function ToggleSiteModule"
assert_contains "$RF_A" "CreateUserRole" "Role A có Function CreateUserRole"

call GET "/api/role-function?roleId=ISOTEST_ROLE&pageSize=50" "$GATEWAY_URL" "" "$TOKEN_B"
assert_status 200 "GetAllRoleFunctions(ISOTEST_ROLE) với token Site B"
RF_B="$(jq -c '[(.data // .Data).records[]? // (.data // .Data).Records[]? | (.functionId // .FunctionId)]' "$BODY_FILE")"
assert_contains "$RF_B" "CreateUserRole" "Role B có Function CreateUserRole"
assert_not_contains "$RF_B" "ToggleSiteModule" "Role B KHÔNG được có Function ToggleSiteModule (không rò quyền từ Role A chỉ vì trùng RoleId)"

# ================================================================================================
section "5) PermissionController.GetPermissionTree - route param siteId bị bỏ qua, luôn dùng claim"
# ================================================================================================
# GET /api/permission/{siteId} - PermissionController cố tình BỎ QUA siteId trên route, luôn dùng
# CurrentSiteId. Gọi với token Site A nhưng cố tình truyền route siteId=ISOTEST_B để xác nhận
# response vẫn là cây quyền của Site A (claim), không phải Site B (route).

call GET "/api/permission/ISOTEST_B?roleId=ISOTEST_ROLE" "$GATEWAY_URL" "" "$TOKEN_A"
assert_status 200 "GetPermissionTree(route siteId=ISOTEST_B, token Site A)"
TREE_SITE="$(jq -r '.data.siteId // .Data.SiteId // empty' "$BODY_FILE")"
assert_eq "$TREE_SITE" "ISOTEST_A" "GetPermissionTree bỏ qua route siteId, trả đúng SiteId=ISOTEST_A theo token (không rò cây quyền Site B qua URL)"

# ================================================================================================
section "6) CreateUserRole - chặn gán Role của Site khác dù biết đúng RoleId"
# ================================================================================================
# Site A gọi CreateUserRole với đúng RoleId "ISOTEST_ROLE" (literal tồn tại ở cả 2 Site) cho 1
# UserId không tồn tại ở Site A - Repository phải resolve Role theo ĐÚNG Site của người gọi
# (input.SiteId = CurrentSiteId, không phải ambiguous theo RoleId một mình), nên phải trả lỗi rõ
# ràng (400/404) vì "khong_ton_tai_user_nay" không thuộc Site A - KHÔNG được vô tình tạo liên kết
# chéo sang bản ghi Role của Site B.

call POST "/api/user-role" "$GATEWAY_URL" \
    '{"userId":"khong_ton_tai_user_nay","roleId":"ISOTEST_ROLE"}' "$TOKEN_A"
if [ "$STATUS" = "400" ] || [ "$STATUS" = "404" ]; then
    pass "CreateUserRole với UserId không thuộc Site A bị từ chối đúng cách (HTTP $STATUS)"
else
    fail "CreateUserRole với UserId không thuộc Site A: mong đợi HTTP 400/404, nhận HTTP $STATUS - body: $(cat "$BODY_FILE" | head -c 500)"
fi

# ================================================================================================
section "7) SiteModuleController - RequireFunction('ToggleSiteModule') cách ly đúng theo Site"
# ================================================================================================
# Role A có ToggleSiteModule, Role B thì KHÔNG (dù cùng RoleId literal) - AuthorizationMiddleware
# phải chặn Site B ở bước RequireFunction, không liên quan gì tới entitlement tbl_Site_Module.
# ProvisionSiteModule/GetSiteModulesBySite không có route YARP - gọi thẳng $SITE_URL (cổng nội bộ).

call GET "/api/site-module/site/ISOTEST_A" "$SITE_URL" "" "$TOKEN_A"
if [ "$STATUS" != "200" ]; then
    skip "Không gọi được Site service trực tiếp ở $SITE_URL (HTTP $STATUS) - bỏ qua nhóm test SiteModuleController. Đặt biến môi trường SITE_URL nếu Site service chạy ở cổng khác."
else
    MODULE_ID="$(jq -r '(.data // .Data)[0].moduleId // (.data // .Data)[0].ModuleId // empty' "$BODY_FILE")"
    if [ -z "$MODULE_ID" ]; then
        skip "Site A chưa có entitlement tbl_Site_Module nào (DB có thể chưa có Module nào active khi chạy seed) - bỏ qua nhóm test toggle. Xem ghi chú đầu seed_two_site_isolation.sql."
    else
        pass "Lấy được ModuleId đã cấp cho Site A để test toggle: $MODULE_ID"

        # Site B: có RequireFunction("ToggleSiteModule") nhưng Role B KHÔNG được cấp Function này -
        # AuthorizationMiddleware phải chặn ở bước RequireFunction (403) TRƯỚC KHI chạm tới entitlement,
        # bất kể moduleId có tồn tại trong tbl_Site_Module của Site B hay không.
        call PUT "/api/site-module/toggle" "$GATEWAY_URL" \
            "{\"moduleId\":\"$MODULE_ID\",\"enable\":true}" "$TOKEN_B"
        assert_status 403 "ToggleSiteModule với token Site B (Role B không có Function này) phải bị chặn 403"

        # Site A: Role A có Function này VÀ đã có entitlement -> phải thành công.
        call PUT "/api/site-module/toggle" "$GATEWAY_URL" \
            "{\"moduleId\":\"$MODULE_ID\",\"enable\":true}" "$TOKEN_A"
        assert_status 200 "ToggleSiteModule với token Site A (đúng quyền + đúng entitlement) phải thành công"
    fi
fi

# ================================================================================================
section "8) GetAllUserRoles/GetAllUsers - danh sách không trả chéo dữ liệu Site khác"
# ================================================================================================
# Không thể assert "chỉ đúng 1 dòng" vì DB có thể có dữ liệu thật khác - assert bằng cách lọc
# roleId=ISOTEST_ROLE (chỉ do fixture 2 Site này tạo ra) và xác nhận số dòng đúng bằng 1 mỗi bên,
# không phải 2 (nếu leak sẽ thấy cả bản ghi của Site kia).

call GET "/api/user-role?roleId=ISOTEST_ROLE&pageSize=50" "$GATEWAY_URL" "" "$TOKEN_A"
assert_status 200 "GetAllUserRoles(roleId=ISOTEST_ROLE) với token Site A"
COUNT_A="$(jq -r '((.data // .Data).records // (.data // .Data).Records) | length' "$BODY_FILE")"
assert_eq "$COUNT_A" "1" "GetAllUserRoles trả đúng 1 dòng cho Site A (không lẫn dòng của Site B)"

call GET "/api/user-role?roleId=ISOTEST_ROLE&pageSize=50" "$GATEWAY_URL" "" "$TOKEN_B"
assert_status 200 "GetAllUserRoles(roleId=ISOTEST_ROLE) với token Site B"
COUNT_B="$(jq -r '((.data // .Data).records // (.data // .Data).Records) | length' "$BODY_FILE")"
assert_eq "$COUNT_B" "1" "GetAllUserRoles trả đúng 1 dòng cho Site B (không lẫn dòng của Site A)"

# UserFilterDTO KHÔNG có tham số lọc theo UserId (chỉ SiteId/Priority/UserName/Email/QuickCode) - lấy
# nguyên trang đầu (pageSize lớn) rồi tự đếm số dòng "isotest_user" xuất hiện, thay vì lọc qua query string.
call GET "/api/user?pageSize=200" "$GATEWAY_URL" "" "$TOKEN_A"
assert_status 200 "GetAllUsers() với token Site A"
USERS_A="$(jq -c '(.data // .Data).records // (.data // .Data).Records' "$BODY_FILE")"
ISO_COUNT_A="$(echo "$USERS_A" | jq '[.[] | select((.userId // .UserId) == "isotest_user")] | length')"
assert_eq "$ISO_COUNT_A" "1" "GetAllUsers trả đúng 1 User 'isotest_user' cho Site A (không lẫn User trùng UserId của Site B)"

call GET "/api/user?pageSize=200" "$GATEWAY_URL" "" "$TOKEN_B"
assert_status 200 "GetAllUsers() với token Site B"
USERS_B="$(jq -c '(.data // .Data).records // (.data // .Data).Records' "$BODY_FILE")"
ISO_COUNT_B="$(echo "$USERS_B" | jq '[.[] | select((.userId // .UserId) == "isotest_user")] | length')"
assert_eq "$ISO_COUNT_B" "1" "GetAllUsers trả đúng 1 User 'isotest_user' cho Site B (không lẫn User trùng UserId của Site A)"

# ================================================================================================
section "Tổng kết"
# ================================================================================================
printf '\nPASS: %d  FAIL: %d  SKIP: %d\n' "$PASS_COUNT" "$FAIL_COUNT" "$SKIP_COUNT"
if [ "$FAIL_COUNT" -gt 0 ]; then
    echo "=> CÓ TEST THẤT BẠI - rà lại các dòng [FAIL] phía trên trước khi coi cách ly 2 Site là an toàn."
    exit 1
else
    echo "=> Toàn bộ test cách ly 2 Site PASS (hoặc SKIP do thiếu tiền đề dữ liệu - xem log)."
    exit 0
fi
