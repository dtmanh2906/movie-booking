import { useEffect, useMemo, useState } from 'react';
import { Alert, Button, Empty, Result, Select, Skeleton } from 'antd';
import { Link, useParams } from 'react-router-dom';
import { getCinemas, getMovieBySlug, getShowtimes } from '../api/catalogApi';
import { MoviePoster } from '../components/MoviePoster';
import {
  formatDateOnlyVi,
  formatVietnamTime,
  formatVndCurrency,
  getNextVietnamDays,
} from '../utils/dateTime';
import {
  getAgeRatingMeta,
  groupShowtimesByCinema,
  isRequestCanceled,
  normalizeSafeImageUrl,
  parseSafeTrailerUrl,
  translateGenreName,
} from '../utils/movieHelpers';
import './Pages.css';

function BackArrowIcon() {
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
      <path d="m15 18-6-6 6-6" />
    </svg>
  );
}

function MovieDetailContent({ slug }) {
  const vietnamDays = useMemo(() => getNextVietnamDays(7), []);
  const defaultDate = vietnamDays[0]?.value || '';

  const [movieRevision, setMovieRevision] = useState(1);
  const [failedBannerSrc, setFailedBannerSrc] = useState(null);

  const [cinemasRevision, setCinemasRevision] = useState(1);
  const [showtimesQuery, setShowtimesQuery] = useState({
    date: defaultDate,
    cinemaId: null,
    revision: 1,
  });

  const [movieState, setMovieState] = useState({
    settledRevision: 0,
    data: null,
    notFound: false,
    error: null,
  });

  const [cinemasState, setCinemasState] = useState({
    settledRevision: 0,
    items: [],
    error: null,
  });

  const [showtimesState, setShowtimesState] = useState({
    settledRevision: 0,
    items: [],
    error: null,
  });

  const movieLoading =
    Boolean(slug) && movieState.settledRevision !== movieRevision;
  const movie =
    !movieLoading && movieState.settledRevision === movieRevision
      ? movieState.data
      : null;
  const movieNotFound =
    !slug ||
    (!movieLoading &&
      movieState.settledRevision === movieRevision &&
      movieState.notFound);
  const movieError =
    !movieLoading && movieState.settledRevision === movieRevision
      ? movieState.error
      : null;

  const cinemasLoading = cinemasState.settledRevision !== cinemasRevision;
  const cinemas = cinemasState.items;
  const cinemasError =
    !cinemasLoading && cinemasState.settledRevision === cinemasRevision
      ? cinemasState.error
      : null;

  const showtimesLoading =
    Boolean(movie && showtimesQuery.date) &&
    showtimesState.settledRevision !== showtimesQuery.revision;
  const showtimesError =
    !showtimesLoading &&
    showtimesState.settledRevision === showtimesQuery.revision
      ? showtimesState.error
      : null;

  // Tải thông tin chi tiết phim từ GET /api/movies/{slug}
  useEffect(() => {
    if (!slug) {
      return undefined;
    }

    const activeRevision = movieRevision;
    const controller = new AbortController();
    let isActive = true;

    getMovieBySlug(slug, { signal: controller.signal })
      .then((data) => {
        if (!isActive || controller.signal.aborted) {
          return;
        }
        setMovieState((prev) =>
          activeRevision < prev.settledRevision
            ? prev
            : {
                settledRevision: activeRevision,
                data: data || null,
                notFound: !data,
                error: null,
              }
        );
      })
      .catch((err) => {
        if (isRequestCanceled(err, controller.signal) || !isActive) {
          return;
        }
        if (err?.response?.status === 404) {
          setMovieState((prev) =>
            activeRevision < prev.settledRevision
              ? prev
              : {
                  settledRevision: activeRevision,
                  data: null,
                  notFound: true,
                  error: null,
                }
          );
          return;
        }
        setMovieState((prev) =>
          activeRevision < prev.settledRevision
            ? prev
            : {
                settledRevision: activeRevision,
                data: null,
                notFound: false,
                error: 'Không thể tải thông tin phim. Vui lòng thử lại.',
              }
        );
      });

    return () => {
      isActive = false;
      controller.abort();
    };
  }, [slug, movieRevision]);

  // Tải danh sách rạp đang hoạt động từ GET /api/cinemas
  useEffect(() => {
    const activeRevision = cinemasRevision;
    const controller = new AbortController();
    let isActive = true;

    getCinemas({}, { signal: controller.signal })
      .then((data) => {
        if (!isActive || controller.signal.aborted) {
          return;
        }
        setCinemasState((prev) =>
          activeRevision < prev.settledRevision
            ? prev
            : {
                settledRevision: activeRevision,
                items: Array.isArray(data) ? data : [],
                error: null,
              }
        );
      })
      .catch((err) => {
        if (isRequestCanceled(err, controller.signal) || !isActive) {
          return;
        }
        setCinemasState((prev) =>
          activeRevision < prev.settledRevision
            ? prev
            : {
                ...prev,
                settledRevision: activeRevision,
                error: 'Không thể tải danh sách rạp.',
              }
        );
      });

    return () => {
      isActive = false;
      controller.abort();
    };
  }, [cinemasRevision]);

  // Tải lịch chiếu khi đã có phim hợp lệ; tự hủy request cũ và kiểm tra revision
  useEffect(() => {
    const { date, cinemaId, revision: activeRevision } = showtimesQuery;
    if (!movie || !slug || !date) {
      return undefined;
    }

    const controller = new AbortController();
    let isActive = true;

    getShowtimes(
      slug,
      {
        date,
        cinemaId,
      },
      { signal: controller.signal }
    )
      .then((data) => {
        if (!isActive || controller.signal.aborted) {
          return;
        }
        setShowtimesState((prev) =>
          activeRevision < prev.settledRevision
            ? prev
            : {
                settledRevision: activeRevision,
                items: Array.isArray(data) ? data : [],
                error: null,
              }
        );
      })
      .catch((err) => {
        if (isRequestCanceled(err, controller.signal) || !isActive) {
          return;
        }
        setShowtimesState((prev) =>
          activeRevision < prev.settledRevision
            ? prev
            : {
                settledRevision: activeRevision,
                items: [],
                error: 'Không thể tải lịch chiếu. Vui lòng thử lại.',
              }
        );
      });

    return () => {
      isActive = false;
      controller.abort();
    };
  }, [movie, slug, showtimesQuery]);

  const groupedShowtimes = useMemo(() => {
    const items =
      showtimesState.settledRevision === showtimesQuery.revision &&
      !showtimesState.error
        ? showtimesState.items
        : [];
    return groupShowtimesByCinema(items);
  }, [showtimesState, showtimesQuery.revision]);

  const handleDateSelect = (nextDate) => {
    setShowtimesQuery((prev) =>
      prev.date === nextDate
        ? prev
        : {
            ...prev,
            date: nextDate,
            revision: prev.revision + 1,
          }
    );
  };

  const handleCinemaChange = (value) => {
    const nextCinemaId = value === '' || value === undefined ? null : Number(value);
    setShowtimesQuery((prev) =>
      prev.cinemaId === nextCinemaId
        ? prev
        : {
            ...prev,
            cinemaId: nextCinemaId,
            revision: prev.revision + 1,
          }
    );
  };

  const handleRetryShowtimes = () => {
    setShowtimesQuery((prev) => ({
      ...prev,
      revision: prev.revision + 1,
    }));
  };

  if (movieLoading) {
    return (
      <div className="mb-page">
        <div className="mb-detail-nav">
          <Link to="/" className="mb-back-link">
            <BackArrowIcon />
            <span>Quay lại danh sách phim</span>
          </Link>
        </div>

        <section className="mb-detail-hero mb-surface" aria-busy="true">
          <div className="mb-detail-hero__grid">
            <div className="mb-skeleton-poster" />
            <Skeleton active paragraph={{ rows: 6 }} />
          </div>
        </section>
      </div>
    );
  }

  if (movieNotFound) {
    return (
      <div className="mb-page">
        <div className="mb-state-box mb-surface">
          <Result
            status="404"
            title="Không tìm thấy phim"
            subTitle="Bộ phim bạn đang tìm không tồn tại hoặc hiện đã ngừng chiếu."
            extra={
              <Link to="/" className="mb-btn mb-btn--primary mb-btn--lg">
                Về danh sách phim
              </Link>
            }
          />
        </div>
      </div>
    );
  }

  if (movieError || !movie) {
    return (
      <div className="mb-page">
        <div className="mb-detail-nav">
          <Link to="/" className="mb-back-link">
            <BackArrowIcon />
            <span>Quay lại danh sách phim</span>
          </Link>
        </div>

        <div className="mb-state-box mb-surface" role="alert">
          <Alert
            type="error"
            showIcon
            title="Không thể tải thông tin phim"
            description={movieError || 'Đã xảy ra lỗi ngoài ý muốn.'}
            action={
              <Button
                type="primary"
                onClick={() => setMovieRevision((r) => r + 1)}
              >
                Thử lại
              </Button>
            }
          />
        </div>
      </div>
    );
  }

  const ageMeta = getAgeRatingMeta(movie.ageRating);
  const safeBannerUrl = normalizeSafeImageUrl(movie.bannerUrl);
  const showBanner = Boolean(safeBannerUrl) && failedBannerSrc !== safeBannerUrl;
  const trailer = parseSafeTrailerUrl(movie.trailerUrl);
  const genres = Array.isArray(movie.genres) ? movie.genres : [];

  const cinemaOptions = [
    { label: 'Tất cả rạp', value: '' },
    ...cinemas.map((cinema) => ({
      label: `${cinema.name} (${cinema.city})`,
      value: cinema.id,
    })),
  ];

  return (
    <div className="mb-page">
      <div className="mb-detail-nav">
        <Link to="/" className="mb-back-link">
          <BackArrowIcon />
          <span>Quay lại danh sách phim</span>
        </Link>
      </div>

      {/* Khối thông tin chính của phim */}
      <section className="mb-detail-hero mb-surface" aria-labelledby="movie-detail-title">
        {showBanner && (
          <div className="mb-detail-hero__backdrop" aria-hidden="true">
            <img
              src={safeBannerUrl}
              alt=""
              className="mb-detail-hero__backdrop-img"
              onError={() => setFailedBannerSrc(safeBannerUrl)}
            />
            <div className="mb-detail-hero__backdrop-overlay" />
          </div>
        )}

        <div className="mb-detail-hero__grid">
          <div className="mb-detail-hero__poster">
            <MoviePoster
              src={movie.posterUrl}
              title={movie.title}
              ageRating={movie.ageRating}
              loading="eager"
            />
          </div>

          <div className="mb-detail-hero__info">
            <div className="mb-detail-hero__badges">
              <span
                className={`mb-age-badge mb-age-badge--${ageMeta.tone}`}
                title={ageMeta.description}
              >
                {ageMeta.label}
              </span>
              {movie.movieFormat && (
                <span className="mb-spec-pill">{movie.movieFormat}</span>
              )}
              {movie.language && (
                <span className="mb-spec-pill">{movie.language}</span>
              )}
            </div>

            <h1 id="movie-detail-title" className="mb-detail-hero__title">
              {movie.title}
            </h1>

            {genres.length > 0 && (
              <ul className="mb-detail-hero__genres" aria-label="Thể loại">
                {genres.map((genreName) => (
                  <li key={genreName} className="mb-genre-chip">
                    {translateGenreName(genreName)}
                  </li>
                ))}
              </ul>
            )}

            <p className="mb-detail-hero__desc">
              {movie.description?.trim() || 'Nội dung phim đang được cập nhật.'}
            </p>

            <dl className="mb-detail-meta">
              <div className="mb-detail-meta__row">
                <dt>Thời lượng</dt>
                <dd>{movie.duration} phút</dd>
              </div>
              <div className="mb-detail-meta__row">
                <dt>Khởi chiếu</dt>
                <dd>{formatDateOnlyVi(movie.releaseDate)}</dd>
              </div>
              <div className="mb-detail-meta__row">
                <dt>Đạo diễn</dt>
                <dd>{movie.director?.trim() || 'Đang cập nhật'}</dd>
              </div>
              <div className="mb-detail-meta__row">
                <dt>Diễn viên</dt>
                <dd>{movie.actors?.trim() || 'Đang cập nhật'}</dd>
              </div>
            </dl>
          </div>
        </div>
      </section>

      {/* Trailer (chỉ hiển thị khi URL hợp lệ, không tự động phát) */}
      {trailer && (
        <section className="mb-section mb-surface mb-trailer-box" aria-labelledby="trailer-heading">
          <div className="mb-section__header">
            <h2 id="trailer-heading" className="mb-section__title">
              Trailer chính thức
            </h2>
            <a
              href={trailer.externalUrl}
              target="_blank"
              rel="noopener noreferrer"
              className="mb-btn mb-btn--secondary"
            >
              Mở liên kết trailer
            </a>
          </div>

          {trailer.embedUrl && (
            <div className="mb-trailer-frame">
              <iframe
                src={trailer.embedUrl}
                title={`Trailer phim ${movie.title}`}
                loading="lazy"
                allow="accelerometer; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                allowFullScreen
              />
            </div>
          )}
        </section>
      )}

      {/* Lịch chiếu theo 7 ngày (Asia/Ho_Chi_Minh) và rạp */}
      <section className="mb-section mb-surface mb-showtimes-box" aria-labelledby="showtimes-heading">
        <div className="mb-showtimes-header">
          <div>
            <h2 id="showtimes-heading" className="mb-section__title">
              Lịch chiếu
            </h2>
            <p className="mb-section__subtitle">
              Chọn ngày và rạp để tra cứu suất chiếu sắp tới (giờ Việt Nam GMT+7)
            </p>
          </div>

          <div className="mb-showtimes-cinema-filter">
            <label htmlFor="cinema-filter-select" className="mb-filter-bar__label">
              Lọc theo rạp
            </label>
            <Select
              id="cinema-filter-select"
              value={
                showtimesQuery.cinemaId === null ? '' : showtimesQuery.cinemaId
              }
              onChange={handleCinemaChange}
              options={cinemaOptions}
              loading={cinemasLoading}
              className="mb-showtimes-cinema-select"
              aria-label="Lọc suất chiếu theo rạp"
            />
          </div>
        </div>

        {cinemasError && (
          <Alert
            type="warning"
            showIcon
            title={cinemasError}
            action={
              <Button
                size="small"
                onClick={() => setCinemasRevision((r) => r + 1)}
              >
                Tải lại rạp
              </Button>
            }
          />
        )}

        <div className="mb-date-tabs" role="group" aria-label="Chọn ngày chiếu">
          {vietnamDays.map((day) => {
            const isSelected = day.value === showtimesQuery.date;
            return (
              <button
                key={day.value}
                type="button"
                aria-pressed={isSelected}
                className={
                  isSelected
                    ? 'mb-date-tab mb-date-tab--active'
                    : 'mb-date-tab'
                }
                onClick={() => handleDateSelect(day.value)}
              >
                <span className="mb-date-tab__weekday">{day.weekdayLabel}</span>
                <span className="mb-date-tab__date">{day.dayMonthLabel}</span>
              </button>
            );
          })}
        </div>

        {showtimesLoading ? (
          <div className="mb-showtimes-loading" aria-busy="true">
            <Skeleton active paragraph={{ rows: 4 }} />
          </div>
        ) : showtimesError ? (
          <div role="alert">
            <Alert
              type="error"
              showIcon
              title="Không thể tải lịch chiếu"
              description={showtimesError}
              action={
                <Button type="primary" onClick={handleRetryShowtimes}>
                  Thử lại
                </Button>
              }
            />
          </div>
        ) : groupedShowtimes.length === 0 ? (
          <div className="mb-showtimes-empty">
            <Empty
              description="Chưa có suất chiếu cho ngày bạn chọn."
              image={Empty.PRESENTED_IMAGE_SIMPLE}
            />
          </div>
        ) : (
          <div className="mb-cinema-groups">
            {groupedShowtimes.map((group) => (
              <section
                key={group.cinemaId}
                className="mb-cinema-group"
                aria-label={`Suất chiếu tại ${group.cinemaName}`}
              >
                <div className="mb-cinema-group__header">
                  <h3 className="mb-cinema-group__name">{group.cinemaName}</h3>
                  <span className="mb-cinema-group__count">
                    {group.items.length} suất chiếu
                  </span>
                </div>

                <ul className="mb-showtime-list">
                  {group.items.map((showtime) => (
                    <li key={showtime.id} className="mb-showtime-item">
                      <span className="mb-showtime-item__time">
                        {formatVietnamTime(showtime.startTime)} –{' '}
                        {formatVietnamTime(showtime.endTime)}
                      </span>
                      <span className="mb-showtime-item__meta">
                        Phòng {showtime.roomNumber} ·{' '}
                        {formatVndCurrency(showtime.ticketPrice)}
                      </span>
                    </li>
                  ))}
                </ul>
              </section>
            ))}
          </div>
        )}
      </section>
    </div>
  );
}

/**
 * Trang chi tiết phim (/movies/:slug) với key={slug} để đặt lại trạng thái sạch khi đổi phim.
 */
export function MovieDetailPage() {
  const { slug } = useParams();
  return <MovieDetailContent key={slug || 'missing'} slug={slug} />;
}
