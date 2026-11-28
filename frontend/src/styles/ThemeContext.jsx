import { createContext, useContext, useEffect, useState } from 'react';
import { ConfigProvider } from 'antd';
import { getAntdTheme } from './themeConfig';

const THEME_STORAGE_KEY = 'moviebooking-theme';

const ThemeContext = createContext(null);

function detectSystemTheme() {
  if (typeof window !== 'undefined' && typeof window.matchMedia === 'function') {
    if (window.matchMedia('(prefers-color-scheme: light)').matches) {
      return 'light';
    }
    if (window.matchMedia('(prefers-color-scheme: dark)').matches) {
      return 'dark';
    }
  }
  return 'dark';
}

function getInitialTheme() {
  // Ưu tiên: lựa chọn hợp lệ trong localStorage -> prefers-color-scheme -> dark
  // Không tự ghi prefers-color-scheme vào localStorage khi người dùng chưa chọn.
  try {
    const saved = localStorage.getItem(THEME_STORAGE_KEY);
    if (saved === 'light' || saved === 'dark') {
      return saved;
    }
  } catch {
    // Bỏ qua lỗi đọc localStorage ở trình duyệt chế độ riêng tư
  }

  return detectSystemTheme();
}

function persistUserTheme(mode) {
  try {
    localStorage.setItem(THEME_STORAGE_KEY, mode);
  } catch {
    // Không làm gián đoạn giao diện nếu trình duyệt chặn ghi localStorage
  }
}

export function ThemeProvider({ children }) {
  const [themeMode, setThemeModeState] = useState(getInitialTheme);

  // Chỉ đồng bộ DOM theo state, không tự ý ghi System Theme vào localStorage
  useEffect(() => {
    document.documentElement.setAttribute('data-theme', themeMode);
    document.documentElement.style.colorScheme = themeMode;
  }, [themeMode]);

  const setThemeMode = (nextMode) => {
    if (nextMode !== 'light' && nextMode !== 'dark') {
      return;
    }
    persistUserTheme(nextMode);
    setThemeModeState(nextMode);
  };

  const toggleTheme = () => {
    const nextMode = themeMode === 'dark' ? 'light' : 'dark';
    persistUserTheme(nextMode);
    setThemeModeState(nextMode);
  };

  return (
    <ThemeContext.Provider value={{ themeMode, setThemeMode, toggleTheme }}>
      <ConfigProvider theme={getAntdTheme(themeMode)}>{children}</ConfigProvider>
    </ThemeContext.Provider>
  );
}

// eslint-disable-next-line react-refresh/only-export-components
export function useTheme() {
  const context = useContext(ThemeContext);
  if (!context) {
    throw new Error('useTheme phải được sử dụng bên trong ThemeProvider.');
  }
  return context;
}
