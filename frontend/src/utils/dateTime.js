const VIETNAM_TIME_ZONE = 'Asia/Ho_Chi_Minh';

const vnDatePartsFormatter = new Intl.DateTimeFormat('en-CA', {
  timeZone: VIETNAM_TIME_ZONE,
  year: 'numeric',
  month: '2-digit',
  day: '2-digit',
});

const vnWeekdayShortFormatter = new Intl.DateTimeFormat('vi-VN', {
  timeZone: VIETNAM_TIME_ZONE,
  weekday: 'short',
});

const vnDayMonthFormatter = new Intl.DateTimeFormat('vi-VN', {
  timeZone: VIETNAM_TIME_ZONE,
  day: '2-digit',
  month: '2-digit',
});

const vnTimeFormatter = new Intl.DateTimeFormat('vi-VN', {
  timeZone: VIETNAM_TIME_ZONE,
  hour: '2-digit',
  minute: '2-digit',
  hour12: false,
});

const vndFormatter = new Intl.NumberFormat('vi-VN', {
  style: 'currency',
  currency: 'VND',
  maximumFractionDigits: 0,
});

/**
 * Lấy chuỗi YYYY-MM-DD hiện tại theo múi giờ Asia/Ho_Chi_Minh (không dùng toISOString).
 */
export function getTodayVietnamDateString(now = new Date()) {
  const parts = vnDatePartsFormatter.formatToParts(now);
  const year = parts.find((p) => p.type === 'year')?.value;
  const month = parts.find((p) => p.type === 'month')?.value;
  const day = parts.find((p) => p.type === 'day')?.value;

  if (!year || !month || !day) {
    throw new Error('Không thể xác định ngày hiện tại theo múi giờ Việt Nam.');
  }

  return `${year}-${month}-${day}`;
}

/**
 * Sinh danh sách `count` ngày liên tiếp (mặc định 7 ngày) bắt đầu từ hôm nay
 * theo múi giờ Asia/Ho_Chi_Minh.
 */
export function getNextVietnamDays(count = 7, now = new Date()) {
  const todayStr = getTodayVietnamDateString(now);
  const [year, month, day] = todayStr.split('-').map(Number);
  const result = [];

  for (let offset = 0; offset < count; offset += 1) {
    // Dùng Date.UTC ở 12:00 trưa để tránh dịch ngày khi đổi sang giờ Việt Nam (+07:00)
    const utcNoon = new Date(Date.UTC(year, month - 1, day + offset, 12, 0, 0));
    const dateValue = getTodayVietnamDateString(utcNoon);

    result.push({
      value: dateValue,
      weekdayLabel: offset === 0 ? 'Hôm nay' : vnWeekdayShortFormatter.format(utcNoon),
      dayMonthLabel: vnDayMonthFormatter.format(utcNoon),
      isToday: offset === 0,
    });
  }

  return result;
}

/**
 * Định dạng thời điểm UTC từ Backend sang giờ Việt Nam (HH:mm).
 * Không tự cộng thêm 7 giờ thủ công vì Intl.DateTimeFormat đã xử lý múi giờ.
 */
export function formatVietnamTime(utcDateString) {
  if (!utcDateString) {
    return '--:--';
  }

  const parsed = new Date(utcDateString);
  if (Number.isNaN(parsed.getTime())) {
    return '--:--';
  }

  return vnTimeFormatter.format(parsed);
}

/**
 * Định dạng chuỗi DateOnly (YYYY-MM-DD) sang dd/MM/yyyy mà không bị lệch múi giờ.
 */
export function formatDateOnlyVi(dateOnlyString) {
  if (!dateOnlyString || typeof dateOnlyString !== 'string') {
    return 'Chưa cập nhật';
  }

  const match = /^(\d{4})-(\d{2})-(\d{2})$/.exec(dateOnlyString.trim());
  if (!match) {
    return dateOnlyString;
  }

  const [, year, month, day] = match;
  return `${day}/${month}/${year}`;
}

/**
 * Định dạng giá vé sang tiền tệ VND (ví dụ: 90.000 ₫).
 * Dùng Number.isFinite để từ chối NaN, Infinity và -Infinity.
 */
export function formatVndCurrency(amount) {
  if (typeof amount !== 'number' || !Number.isFinite(amount)) {
    return 'Liên hệ';
  }

  return vndFormatter.format(amount);
}
