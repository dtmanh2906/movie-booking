import { Result } from 'antd';
import { Link } from 'react-router-dom';
import './Pages.css';

/**
 * Trang 404 hiển thị khi người dùng truy cập đường dẫn không tồn tại.
 */
export function NotFoundPage() {
  return (
    <div className="mb-page mb-surface mb-not-found">
      <Result
        status="404"
        title="404 — Không tìm thấy trang"
        subTitle="Đường dẫn bạn truy cập không tồn tại hoặc đã được thay đổi."
        extra={
          <Link to="/" className="mb-btn mb-btn--primary mb-btn--lg">
            Về trang chủ
          </Link>
        }
      />
    </div>
  );
}
