import axios from 'axios';

// Dùng Map để không bao giờ truy xuất nhầm thuộc tính kế thừa của Object.prototype (như toString, constructor)
const AGE_RATING_MAP = new Map([
  [
    'P',
    {
      code: 'P',
      badgeText: 'P',
      label: 'P — Mọi lứa tuổi',
      description: 'Phim phổ biến cho khán giả ở mọi độ tuổi',
      tone: 'safe',
      isClassified: true,
    },
  ],
  [
    'K',
    {
      code: 'K',
      badgeText: 'K',
      label: 'K — Dưới 13 tuổi có người bảo hộ',
      description: 'Khán giả dưới 13 tuổi cần đi cùng cha mẹ hoặc người giám hộ',
      tone: 'info',
      isClassified: true,
    },
  ],
  [
    'T13',
    {
      code: 'T13',
      badgeText: 'T13',
      label: 'T13 — Từ 13 tuổi trở lên',
      description: 'Phim dành cho khán giả từ đủ 13 tuổi trở lên',
      tone: 'caution',
      isClassified: true,
    },
  ],
  [
    'T16',
    {
      code: 'T16',
      badgeText: 'T16',
      label: 'T16 — Từ 16 tuổi trở lên',
      description: 'Phim dành cho khán giả từ đủ 16 tuổi trở lên',
      tone: 'warning',
      isClassified: true,
    },
  ],
  [
    'T18',
    {
      code: 'T18',
      badgeText: 'T18',
      label: 'T18 — Từ 18 tuổi trở lên',
      description: 'Phim dành cho khán giả từ đủ 18 tuổi trở lên',
      tone: 'restricted',
      isClassified: true,
    },
  ],
]);

const UNCLASSIFIED_AGE_RATING = {
  code: null,
  badgeText: 'Chưa phân loại',
  label: 'Chưa phân loại',
  description: 'Phim chưa có thông tin phân loại độ tuổi',
  tone: 'neutral',
  isClassified: false,
};

const LOCAL_DEV_HOSTNAMES = new Set(['localhost', '127.0.0.1', '[::1]']);

const YOUTUBE_ALLOWED_HOSTNAMES = new Set([
  'youtu.be',
  'youtube.com',
  'www.youtube.com',
  'm.youtube.com',
  'youtube-nocookie.com',
  'www.youtube-nocookie.com',
]);

function hasControlCharsOrBackslash(value) {
  for (let i = 0; i < value.length; i += 1) {
    const code = value.charCodeAt(i);
    if (code <= 0x1f || code === 0x7f || code === 0x5c) {
      return true;
    }
  }
  return false;
}

/**
 * Kiểm tra URL bên ngoài (ưu tiên HTTPS) hoặc đường dẫn nội bộ hợp lệ.
 * Cho phép HTTP chỉ với host phát triển cục bộ (localhost / 127.0.0.1 / [::1]).
 * Kiểm tra ký tự điều khiển trên rawUrl gốc trước khi trim.
 */
function parseSafeHttpUrl(rawUrl) {
  if (typeof rawUrl !== 'string' || hasControlCharsOrBackslash(rawUrl)) {
    return null;
  }

  const trimmed = rawUrl.trim();
  if (!trimmed) {
    return null;
  }

  // Từ chối protocol-relative URL (//evil.com)
  if (trimmed.startsWith('//')) {
    return null;
  }

  // Cho phép đường dẫn nội bộ hợp lệ bắt đầu bằng dấu "/"
  if (trimmed.startsWith('/')) {
    return {
      isRelative: true,
      href: trimmed,
      parsed: null,
    };
  }

  let parsed;
  try {
    parsed = new URL(trimmed);
  } catch {
    return null;
  }

  // Từ chối URL chứa thông tin đăng nhập gây hiểu nhầm (user:pass@host)
  if (parsed.username !== '' || parsed.password !== '') {
    return null;
  }

  const protocol = parsed.protocol.toLowerCase();
  const hostname = parsed.hostname.toLowerCase();

  if (protocol === 'https:') {
    return {
      isRelative: false,
      href: parsed.toString(),
      parsed,
    };
  }

  if (protocol === 'http:' && LOCAL_DEV_HOSTNAMES.has(hostname)) {
    return {
      isRelative: false,
      href: parsed.toString(),
      parsed,
    };
  }

  return null;
}

/**
 * Trả về thông tin nhãn phân loại độ tuổi để hiển thị rõ cả bằng chữ lẫn kiểu viền.
 * Giá trị rỗng hoặc không hợp lệ luôn trả về trạng thái trung tính "Chưa phân loại".
 */
export function getAgeRatingMeta(ageRating) {
  if (typeof ageRating !== 'string') {
    return UNCLASSIFIED_AGE_RATING;
  }

  const normalized = ageRating.trim().toUpperCase();
  if (!normalized) {
    return UNCLASSIFIED_AGE_RATING;
  }

  return AGE_RATING_MAP.get(normalized) ?? UNCLASSIFIED_AGE_RATING;
}

/**
 * Kiểm tra URL hợp lệ cho ảnh poster/banner.
 */
export function isValidImageUrl(url) {
  return parseSafeHttpUrl(url) !== null;
}

/**
 * Chuẩn hóa URL ảnh an toàn (trả về null nếu không hợp lệ).
 */
export function normalizeSafeImageUrl(url) {
  const safe = parseSafeHttpUrl(url);
  return safe ? safe.href : null;
}

/**
 * Kiểm tra và phân tích TrailerUrl an toàn (ưu tiên HTTPS, không tự phát).
 * Iframe YouTube chỉ được tạo khi URL là HTTPS với hostname và video ID hợp lệ.
 */
export function parseSafeTrailerUrl(rawUrl) {
  const safe = parseSafeHttpUrl(rawUrl);
  if (!safe || safe.isRelative || !safe.parsed) {
    return null;
  }

  const { parsed, href } = safe;
  const hostname = parsed.hostname.toLowerCase();
  const isHttps = parsed.protocol.toLowerCase() === 'https:';

  let youtubeId = null;
  if (isHttps && YOUTUBE_ALLOWED_HOSTNAMES.has(hostname)) {
    if (hostname === 'youtu.be') {
      const candidate = parsed.pathname.replace(/^\/+/, '').split('/')[0];
      if (/^[a-zA-Z0-9_-]{11}$/.test(candidate)) {
        youtubeId = candidate;
      }
    } else if (parsed.pathname === '/watch') {
      const v = parsed.searchParams.get('v');
      if (v && /^[a-zA-Z0-9_-]{11}$/.test(v)) {
        youtubeId = v;
      }
    } else if (parsed.pathname.startsWith('/embed/')) {
      const candidate = parsed.pathname.slice('/embed/'.length).split('/')[0];
      if (/^[a-zA-Z0-9_-]{11}$/.test(candidate)) {
        youtubeId = candidate;
      }
    }
  }

  return {
    externalUrl: href,
    embedUrl: youtubeId
      ? `https://www.youtube-nocookie.com/embed/${youtubeId}?rel=0`
      : null,
  };
}

const GENRE_DISPLAY_MAP = new Map([
  ['action', 'Hành động'],
  ['comedy', 'Hài'],
  ['drama', 'Tâm lý'],
  ['horror', 'Kinh dị'],
  ['sci-fi', 'Khoa học viễn tưởng'],
  ['animation', 'Hoạt hình'],
  ['thriller', 'Giật gân'],
  ['romance', 'Tình cảm'],
]);

/**
 * Việt hóa tên thể loại ở tầng hiển thị mà không làm thay đổi genreId hoặc dữ liệu API.
 */
export function translateGenreName(name) {
  if (typeof name !== 'string') {
    return '';
  }
  const trimmed = name.trim();
  if (!trimmed) {
    return '';
  }
  return GENRE_DISPLAY_MAP.get(trimmed.toLowerCase()) ?? trimmed;
}

/**
 * Nhóm danh sách suất chiếu theo CinemaId và CinemaName, sắp xếp theo StartTime tăng dần.
 */
export function groupShowtimesByCinema(showtimes = []) {
  if (!Array.isArray(showtimes) || showtimes.length === 0) {
    return [];
  }

  const sorted = [...showtimes].sort((a, b) => {
    const timeA = new Date(a.startTime).getTime();
    const timeB = new Date(b.startTime).getTime();
    return timeA - timeB;
  });

  const groupsMap = new Map();

  sorted.forEach((showtime) => {
    const key = showtime.cinemaId;
    if (!groupsMap.has(key)) {
      groupsMap.set(key, {
        cinemaId: showtime.cinemaId,
        cinemaName: showtime.cinemaName || `Rạp #${showtime.cinemaId}`,
        items: [],
      });
    }
    groupsMap.get(key).items.push(showtime);
  });

  return Array.from(groupsMap.values());
}

/**
 * Nhận diện request đã bị hủy bởi AbortController để không hiển thị lỗi giả.
 */
export function isRequestCanceled(error, signal) {
  return (
    Boolean(signal?.aborted) ||
    axios.isCancel(error) ||
    error?.code === 'ERR_CANCELED' ||
    error?.name === 'CanceledError' ||
    error?.name === 'AbortError'
  );
}
