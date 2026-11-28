import axios from 'axios';

// Đọc URL API từ biến môi trường Vite (không viết cứng địa chỉ máy chủ trong code)
const apiBaseUrl = import.meta.env.VITE_API_URL || '';

if (import.meta.env.DEV && !apiBaseUrl) {
  console.warn(
    'Thiếu biến môi trường VITE_API_URL. Hãy sao chép .env.example thành .env.'
  );
}

// Axios instance dùng chung cho toàn bộ ứng dụng.
// Lưu ý: Chưa gắn JWT interceptor trong task khung nền tảng; sẽ bổ sung ở task xác thực.
const axiosClient = axios.create({
  baseURL: apiBaseUrl,
  timeout: 15000,
  headers: {
    Accept: 'application/json',
  },
});

export default axiosClient;
