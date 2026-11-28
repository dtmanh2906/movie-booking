import react from '@vitejs/plugin-react';
import { defineConfig } from 'vite';

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    // Không tự nhảy sang cổng khác để tránh lệch cấu hình CORS ở Backend
    strictPort: true,
  },
});
