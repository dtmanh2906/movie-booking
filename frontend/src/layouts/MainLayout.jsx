import { useEffect, useRef, useState, useSyncExternalStore } from 'react';
import { Link, NavLink, Outlet, useLocation } from 'react-router-dom';
import { BrandLogo } from '../components/BrandLogo';
import { ThemeToggle } from '../components/ThemeToggle';
import './MainLayout.css';

const DESKTOP_MEDIA_QUERY = '(min-width: 768px)';

function subscribeDesktopMedia(callback) {
  if (typeof window === 'undefined' || typeof window.matchMedia !== 'function') {
    return () => {};
  }
  const mediaQuery = window.matchMedia(DESKTOP_MEDIA_QUERY);
  mediaQuery.addEventListener('change', callback);
  return () => mediaQuery.removeEventListener('change', callback);
}

function getIsDesktopSnapshot() {
  if (typeof window === 'undefined' || typeof window.matchMedia !== 'function') {
    return true;
  }
  return window.matchMedia(DESKTOP_MEDIA_QUERY).matches;
}

function MenuIcon({ isOpen }) {
  return (
    <svg
      width="20"
      height="20"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      {isOpen ? (
        <>
          <path d="M18 6 6 18" />
          <path d="m6 6 12 12" />
        </>
      ) : (
        <>
          <path d="M4 6h16" />
          <path d="M4 12h16" />
          <path d="M4 18h16" />
        </>
      )}
    </svg>
  );
}

/**
 * Khung giao diện chính cho toàn bộ trang công khai:
 * Header cố định phía trên, vùng nội dung qua Outlet, và Footer ở cuối trang.
 */
export function MainLayout() {
  const location = useLocation();
  const currentNavId = `${location.key}|${location.pathname}${location.search}`;
  const [menuOpen, setMenuOpen] = useState(false);
  const [trackedNavId, setTrackedNavId] = useState(currentNavId);
  const menuButtonRef = useRef(null);

  const isDesktop = useSyncExternalStore(
    subscribeDesktopMedia,
    getIsDesktopSnapshot,
    () => true
  );

  // Khi điều hướng (bao gồm Browser Back/Forward), reset menu về đóng để quay lại route cũ không tự mở lại
  if (trackedNavId !== currentNavId) {
    setTrackedNavId(currentNavId);
    if (menuOpen) {
      setMenuOpen(false);
    }
  }

  const mobileMenuOpen = !isDesktop && menuOpen;

  // Khi mở rộng màn hình sang Tablet/Desktop (>= 768px), xóa trạng thái mở
  useEffect(() => {
    if (typeof window === 'undefined' || typeof window.matchMedia !== 'function') {
      return undefined;
    }

    const mediaQuery = window.matchMedia(DESKTOP_MEDIA_QUERY);
    const handleViewportChange = (event) => {
      if (event.matches) {
        setMenuOpen(false);
      }
    };

    mediaQuery.addEventListener('change', handleViewportChange);
    return () => {
      mediaQuery.removeEventListener('change', handleViewportChange);
    };
  }, []);

  // Hỗ trợ phím Escape để đóng Mobile Menu và trả focus về nút mở menu
  useEffect(() => {
    if (!mobileMenuOpen) {
      return undefined;
    }

    const handleKeyDown = (event) => {
      if (event.key === 'Escape') {
        setMenuOpen(false);
        menuButtonRef.current?.focus();
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => {
      window.removeEventListener('keydown', handleKeyDown);
    };
  }, [mobileMenuOpen]);

  const toggleMobileMenu = () => setMenuOpen((prev) => !prev);
  const closeMobileMenu = () => setMenuOpen(false);

  const getNavLinkClass = ({ isActive }) =>
    isActive ? 'mb-nav__link mb-nav__link--active' : 'mb-nav__link';

  return (
    <div className="mb-layout">
      <a href="#main-content" className="skip-link">
        Chuyển tới nội dung chính
      </a>

      <header className="mb-header">
        <div className="mb-container mb-header__inner">
          <Link
            to="/"
            className="mb-header__brand-link"
            aria-label="Trang chủ MovieBooking"
            onClick={closeMobileMenu}
          >
            <BrandLogo />
          </Link>

          <div className="mb-header__actions">
            <nav className="mb-nav mb-nav--desktop" aria-label="Điều hướng chính">
              <NavLink to="/" end className={getNavLinkClass}>
                Phim đang chiếu
              </NavLink>
            </nav>

            <ThemeToggle />

            <button
              ref={menuButtonRef}
              type="button"
              className="mb-mobile-menu-btn"
              aria-label={mobileMenuOpen ? 'Đóng menu điều hướng' : 'Mở menu điều hướng'}
              aria-expanded={mobileMenuOpen}
              aria-controls="mobile-navigation"
              onClick={toggleMobileMenu}
            >
              <MenuIcon isOpen={mobileMenuOpen} />
            </button>
          </div>
        </div>
      </header>

      <div
        id="mobile-navigation"
        hidden={!mobileMenuOpen}
        className={
          mobileMenuOpen
            ? 'mb-mobile-drawer mb-mobile-drawer--open'
            : 'mb-mobile-drawer'
        }
      >
        <div className="mb-container">
          <nav className="mb-mobile-drawer__nav" aria-label="Điều hướng di động">
            <NavLink
              to="/"
              end
              className={getNavLinkClass}
              onClick={closeMobileMenu}
            >
              Phim đang chiếu
            </NavLink>
          </nav>
        </div>
      </div>

      <main id="main-content" className="mb-main" tabIndex={-1}>
        <div className="mb-container">
          <Outlet />
        </div>
      </main>

      <footer className="mb-footer">
        <div className="mb-container mb-footer__inner">
          <div className="mb-footer__brand">
            <BrandLogo compact />
            <p className="mb-footer__desc">
              Hệ thống tra cứu suất chiếu và đặt vé xem phim trực tuyến tại các cụm rạp MovieBooking.
            </p>
          </div>

          <div className="mb-footer__meta">
            <p className="mb-footer__copyright">
              © {new Date().getFullYear()} MovieBooking. Bảo lưu mọi quyền.
            </p>
            <p className="mb-footer__note">
              Giờ chiếu hiển thị theo múi giờ Việt Nam (GMT+7)
            </p>
          </div>
        </div>
      </footer>
    </div>
  );
}
