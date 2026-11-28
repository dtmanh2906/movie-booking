import { Button, Tooltip } from 'antd';
import { useTheme } from '../styles/ThemeContext';

function SunIcon() {
  return (
    <svg
      width="16"
      height="16"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      <circle cx="12" cy="12" r="4" />
      <path d="M12 2v2" />
      <path d="M12 20v2" />
      <path d="m4.93 4.93 1.41 1.41" />
      <path d="m17.66 17.66 1.41 1.41" />
      <path d="M2 12h2" />
      <path d="M20 12h2" />
      <path d="m6.34 17.66-1.41 1.41" />
      <path d="m19.07 4.93-1.41 1.41" />
    </svg>
  );
}

function MoonIcon() {
  return (
    <svg
      width="16"
      height="16"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      <path d="M12 3a6 6 0 0 0 9 9 9 9 0 1 1-9-9Z" />
    </svg>
  );
}

/**
 * Nút chuyển đổi giao diện Sáng / Tối với ngữ nghĩa aria-label và aria-pressed nhất quán.
 */
export function ThemeToggle() {
  const { themeMode, toggleTheme } = useTheme();
  const isDark = themeMode === 'dark';
  const tooltipText = isDark
    ? 'Đang bật chế độ tối — nhấn để chuyển sang chế độ sáng'
    : 'Đang bật chế độ sáng — nhấn để chuyển sang chế độ tối';

  return (
    <Tooltip title={tooltipText}>
      <Button
        type="default"
        onClick={toggleTheme}
        aria-label="Chế độ tối"
        aria-pressed={isDark}
        className="mb-theme-toggle"
        icon={isDark ? <MoonIcon /> : <SunIcon />}
      >
        <span className="mb-theme-toggle__text">
          {isDark ? 'Chế độ tối' : 'Chế độ sáng'}
        </span>
      </Button>
    </Tooltip>
  );
}
