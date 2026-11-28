# MovieBooking Frontend

Giao diện web khách hàng cho hệ thống tra cứu phim, lịch chiếu đa chi nhánh và đặt vé xem phim MovieBooking.

## Công nghệ sử dụng

- React 19 (JavaScript) + Vite
- React Router DOM
- Axios
- Ant Design 6 (`ConfigProvider` hỗ trợ Light/Dark Mode)
- CSS thuần + CSS Variables (`src/styles/tokens.css`)

## Yêu cầu môi trường

- Node.js (khuyến nghị bản LTS)
- Backend `CinemaBooking.Api` đang chạy tại `http://localhost:5080` (đã bật CORS cho `http://localhost:5173`)

## Cấu hình biến môi trường

Sao chép `.env.example` thành `.env` trong thư mục `frontend/`:

```powershell
Copy-Item .env.example .env
```

Biến môi trường mặc định:

```env
VITE_API_URL=http://localhost:5080
```

> Lưu ý: Không đặt bất kỳ khóa bí mật nào vào biến `VITE_*` vì giá trị sẽ lộ ra trình duyệt.

## Lệnh phát triển và kiểm tra

Chạy từ thư mục `frontend/`:

```powershell
# Cài đặt thư viện
npm install

# Chạy máy chủ phát triển tại http://localhost:5173
npm run dev

# Kiểm tra lint
npm run lint

# Build production
npm run build
```

## Cấu trúc thư mục chính

- `src/api/` — Axios client và các hàm gọi Public Catalog API (`movies`, `genres`, `cinemas`).
- `src/components/` — Các component dùng chung (`BrandLogo`, `ThemeToggle`, `MoviePoster`, `MovieCard`).
- `src/layouts/` — Khung trang công khai (`MainLayout` với Header, Mobile Navigation và Footer).
- `src/pages/` — Các trang (`HomePage`, `MovieDetailPage`, `NotFoundPage`).
- `src/styles/` — Design tokens (`tokens.css`), cấu hình Ant Design (`themeConfig.js`) và `ThemeContext.jsx`.
- `src/utils/` — Tiện ích xử lý ngày/giờ theo múi giờ Việt Nam (`Asia/Ho_Chi_Minh`), tiền tệ VND và kiểm tra URL an toàn.
