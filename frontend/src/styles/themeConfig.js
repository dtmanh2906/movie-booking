import { theme as antdTheme } from 'antd';

/**
 * Ánh xạ Design System sang ThemeConfig của Ant Design 6.
 * Đồng bộ bảng màu, màu ngữ nghĩa, font và bo góc với CSS variables trong tokens.css.
 */
export function getAntdTheme(mode) {
  const isDark = mode === 'dark';

  return {
    algorithm: isDark ? antdTheme.darkAlgorithm : antdTheme.defaultAlgorithm,
    token: {
      colorPrimary: isDark ? '#E50914' : '#D90813',
      colorPrimaryHover: isDark ? '#C80812' : '#B80710',
      colorPrimaryActive: isDark ? '#A8060F' : '#99050D',
      colorLink: isDark ? '#FF6B73' : '#B80710',
      colorLinkHover: isDark ? '#FF949A' : '#D90813',
      colorBgBase: isDark ? '#0F1014' : '#F6F7FB',
      colorBgLayout: isDark ? '#0F1014' : '#F6F7FB',
      colorBgContainer: isDark ? '#1C1E26' : '#FFFFFF',
      colorBgElevated: isDark ? '#252833' : '#EEF1F7',
      colorText: isDark ? '#F5F5F5' : '#111318',
      colorTextSecondary: isDark ? '#D1D5DB' : '#374151',
      colorTextTertiary: isDark ? '#9CA3AF' : '#4B5563',
      colorTextPlaceholder: isDark ? '#9CA3AF' : '#5B6472',
      colorBorder: isDark ? '#2A2D3A' : '#E2E5EE',
      colorBorderSecondary: isDark ? '#3B4052' : '#C8CEDD',
      colorSuccess: isDark ? '#22C55E' : '#15803D',
      colorWarning: isDark ? '#F59E0B' : '#B45309',
      colorError: isDark ? '#EF4444' : '#DC2626',
      colorInfo: isDark ? '#38BDF8' : '#0369A1',
      fontFamily:
        "'Segoe UI', system-ui, -apple-system, BlinkMacSystemFont, 'Roboto', 'Helvetica Neue', Arial, sans-serif",
      fontSize: 14,
      borderRadius: 10,
      controlHeight: 40,
    },
    components: {
      Button: {
        borderRadius: 10,
        fontWeight: 600,
        primaryShadow: 'none',
      },
      Card: {
        borderRadiusLG: 14,
      },
      Input: {
        activeBorderColor: isDark ? '#FF4D58' : '#B80710',
        activeShadow: isDark
          ? '0 0 0 2px rgba(255, 77, 88, 0.32)'
          : '0 0 0 2px rgba(184, 7, 16, 0.22)',
      },
      Select: {
        activeBorderColor: isDark ? '#FF4D58' : '#B80710',
        activeOutlineColor: isDark
          ? 'rgba(255, 77, 88, 0.32)'
          : 'rgba(184, 7, 16, 0.22)',
        optionSelectedBg: isDark
          ? 'rgba(229, 9, 20, 0.18)'
          : 'rgba(217, 8, 19, 0.1)',
      },
      Layout: {
        headerBg: 'transparent',
        bodyBg: 'transparent',
        footerBg: 'transparent',
      },
      Tag: {
        borderRadiusSM: 6,
      },
    },
  };
}
