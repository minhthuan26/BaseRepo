// PreToolUse hook: chặn Read/Edit/Write/Grep/Bash chạm vào file chứa secret.
// Nhận JSON từ stdin; exit 2 = chặn và gửi stderr cho Claude.
// Chạy bằng Node nên dùng được trên Windows/macOS/Linux.
// Không phải ranh giới bảo mật tuyệt đối (lệnh shell có thể lách bằng nhiều cách);
// đây là lớp chặn thêm bên cạnh `deny` trong settings.json. Với dữ liệu thật nhạy cảm, vẫn dùng DB dev/test.
import { readFileSync } from "node:fs";

let input = {};
try {
  input = JSON.parse(readFileSync(0, "utf8") || "{}");
} catch {
  process.exit(0); // không đọc được input thì không chặn nhầm
}

const ti = input?.tool_input ?? {};
const norm = (s) => String(s ?? "").replace(/\\/g, "/");
const isTemplate = (s) => /\.(example|sample|template)$/i.test(s);

// Một "đường dẫn/token" có phải secret không
function secretToken(tok) {
  if (!tok || isTemplate(tok)) return false;
  return (
    /(^|\/)\.env(\.[\w-]+)?$/i.test(tok) ||
    /appsettings\.(Production|[\w.-]*\.local)\.json$/i.test(tok) ||
    /(^|\/)secrets\.json$/i.test(tok) ||
    /(^|\/)\.(aws|ssh)(\/|$)/i.test(tok) ||
    /(^|\/)id_(rsa|ed25519)(\.pub)?$/i.test(tok) ||
    /\.pem$/i.test(tok)
  );
}

const targets = [];
// Read/Edit/Write/Grep: kiểm tra các trường đường dẫn
for (const k of ["file_path", "path", "glob"]) if (ti[k]) targets.push(norm(ti[k]));
// Bash: tách lệnh thành token, kiểm tra từng token
if (ti.command) {
  for (const raw of norm(ti.command).split(/[\s"'`=:<>|&;()]+/)) {
    if (raw) targets.push(raw.replace(/^~\//, "/"));
  }
}

const hit = targets.find(secretToken);
if (hit) {
  console.error(
    `Bị chặn: "${hit}" có thể là file secret. Dùng file .example hoặc hỏi người dùng; không đọc/ghi trực tiếp.`
  );
  process.exit(2);
}
process.exit(0);
