import { useState } from 'react';
import { getAgeRatingMeta, normalizeSafeImageUrl } from '../utils/movieHelpers';
import './MoviePoster.css';

function FilmFrameIcon() {
  return (
    <svg
      width="36"
      height="36"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.6"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      <rect width="18" height="18" x="3" y="3" rx="2" />
      <path d="M7 3v18" />
      <path d="M3 7.5h4" />
      <path d="M3 12h18" />
      <path d="M3 16.5h4" />
      <path d="M17 3v18" />
      <path d="M17 7.5h4" />
      <path d="M17 16.5h4" />
    </svg>
  );
}

/**
 * Hiển thị poster phim theo đúng tỷ lệ 2:3.
 * Nếu posterUrl null, không hợp lệ hoặc tải ảnh thất bại sẽ chuyển sang khung trung tính
 * một lần duy nhất (không gây vòng lặp onError).
 */
export function MoviePoster({
  src,
  title,
  ageRating,
  className = '',
  loading = 'lazy',
}) {
  const safeUrl = normalizeSafeImageUrl(src);
  const [failedSrc, setFailedSrc] = useState(null);
  const ageMeta = getAgeRatingMeta(ageRating);

  const shouldShowFallback = !safeUrl || failedSrc === safeUrl;

  return (
    <div className={`mb-poster ${className}`.trim()}>
      {shouldShowFallback ? (
        <div className="mb-poster__fallback">
          <div className="mb-poster__fallback-icon">
            <FilmFrameIcon />
          </div>
          <p className="mb-poster__fallback-title">{title || 'Phim đang chiếu'}</p>
          <span className="mb-poster__fallback-caption">
            {ageMeta.isClassified
              ? `Phân loại ${ageMeta.code}`
              : 'Chưa phân loại'}
          </span>
        </div>
      ) : (
        <img
          src={safeUrl}
          alt={`Poster phim ${title || ''}`.trim()}
          loading={loading}
          className="mb-poster__img"
          onError={() => setFailedSrc(safeUrl)}
        />
      )}
    </div>
  );
}
