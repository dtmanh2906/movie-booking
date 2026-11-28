import { useEffect, useState } from 'react';
import { Alert, Button, Empty, Input, Pagination, Select, Skeleton } from 'antd';
import { Link } from 'react-router-dom';
import { getGenres, getMovies } from '../api/catalogApi';
import { MovieCard } from '../components/MovieCard';
import { MoviePoster } from '../components/MoviePoster';
import { formatDateOnlyVi } from '../utils/dateTime';
import {
  getAgeRatingMeta,
  isRequestCanceled,
  translateGenreName,
} from '../utils/movieHelpers';
import './Pages.css';

const PAGE_SIZE = 12;
const SKELETON_COUNT = 8;

const EMPTY_PAGED_RESULT = {
  items: [],
  page: 1,
  pageSize: PAGE_SIZE,
  totalCount: 0,
  totalPages: 0,
};

function SearchIcon() {
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
      <circle cx="11" cy="11" r="8" />
      <path d="m21 21-4.3-4.3" />
    </svg>
  );
}

/**
 * Trang chủ danh sách phim đang chiếu với Hero Spotlight, tìm kiếm, lọc thể loại và phân trang.
 */
export function HomePage() {
  const [searchInput, setSearchInput] = useState('');

  // Dùng revision tăng đơn điệu để mỗi truy vấn mới (kể cả quay lại A -> B -> A) luôn tách biệt hoàn toàn
  const [moviesQuery, setMoviesQuery] = useState({
    search: '',
    genreId: null,
    page: 1,
    revision: 1,
  });

  const [moviesState, setMoviesState] = useState({
    settledRevision: 0,
    result: EMPTY_PAGED_RESULT,
    error: null,
  });

  const [featuredMovie, setFeaturedMovie] = useState(null);

  const [genresRevision, setGenresRevision] = useState(1);
  const [genresState, setGenresState] = useState({
    settledRevision: 0,
    items: [],
    error: null,
  });

  const moviesLoading = moviesState.settledRevision !== moviesQuery.revision;
  const moviesError =
    !moviesLoading && moviesState.settledRevision === moviesQuery.revision
      ? moviesState.error
      : null;
  const moviesResult =
    !moviesLoading && !moviesError ? moviesState.result : EMPTY_PAGED_RESULT;

  const genresLoading = genresState.settledRevision !== genresRevision;
  const genres = genresState.items;
  const genresError =
    !genresLoading && genresState.settledRevision === genresRevision
      ? genresState.error
      : null;

  // Debounce từ khóa tìm kiếm (350ms) và tăng revision khi từ khóa thật sự thay đổi
  useEffect(() => {
    const trimmed = searchInput.trim();
    if (trimmed === moviesQuery.search) {
      return undefined;
    }

    const timerId = setTimeout(() => {
      setMoviesQuery((prev) =>
        prev.search === trimmed
          ? prev
          : {
              ...prev,
              search: trimmed,
              page: 1,
              revision: prev.revision + 1,
            }
      );
    }, 350);

    return () => {
      clearTimeout(timerId);
    };
  }, [searchInput, moviesQuery.search]);

  // Tải danh sách thể loại từ GET /api/genres
  useEffect(() => {
    const activeRevision = genresRevision;
    const controller = new AbortController();
    let isActive = true;

    getGenres({ signal: controller.signal })
      .then((data) => {
        if (!isActive || controller.signal.aborted) {
          return;
        }
        setGenresState((prev) =>
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
        setGenresState((prev) =>
          activeRevision < prev.settledRevision
            ? prev
            : {
                ...prev,
                settledRevision: activeRevision,
                error: 'Không thể tải danh sách thể loại.',
              }
        );
      });

    return () => {
      isActive = false;
      controller.abort();
    };
  }, [genresRevision]);

  // Tải danh sách phim từ GET /api/movies, tự hủy request cũ và kiểm tra revision
  useEffect(() => {
    const { search, genreId, page, revision: activeRevision } = moviesQuery;
    const controller = new AbortController();
    let isActive = true;

    getMovies(
      {
        search,
        genreId,
        page,
        pageSize: PAGE_SIZE,
      },
      { signal: controller.signal }
    )
      .then((data) => {
        if (!isActive || controller.signal.aborted) {
          return;
        }

        const normalized = {
          items: Array.isArray(data?.items) ? data.items : [],
          page: typeof data?.page === 'number' ? data.page : page,
          pageSize: typeof data?.pageSize === 'number' ? data.pageSize : PAGE_SIZE,
          totalCount: typeof data?.totalCount === 'number' ? data.totalCount : 0,
          totalPages: typeof data?.totalPages === 'number' ? data.totalPages : 0,
        };

        setMoviesState((prev) =>
          activeRevision < prev.settledRevision
            ? prev
            : {
                settledRevision: activeRevision,
                result: normalized,
                error: null,
              }
        );

        // Giữ phim nổi bật từ danh sách gốc chưa lọc để Hero ổn định khi tìm kiếm/lọc
        if (!search && genreId === null && page === 1) {
          setFeaturedMovie(normalized.items[0] || null);
        }
      })
      .catch((err) => {
        if (isRequestCanceled(err, controller.signal) || !isActive) {
          return;
        }
        setMoviesState((prev) =>
          activeRevision < prev.settledRevision
            ? prev
            : {
                settledRevision: activeRevision,
                result: EMPTY_PAGED_RESULT,
                error: 'Không thể tải danh sách phim. Vui lòng thử lại.',
              }
        );
      });

    return () => {
      isActive = false;
      controller.abort();
    };
  }, [moviesQuery]);

  const handleSearchSubmit = (event) => {
    event.preventDefault();
    const trimmed = searchInput.trim();
    setMoviesQuery((prev) =>
      prev.search === trimmed
        ? prev
        : {
            ...prev,
            search: trimmed,
            page: 1,
            revision: prev.revision + 1,
          }
    );
  };

  const handleGenreChange = (value) => {
    const nextGenreId = value === undefined || value === null ? null : value;
    setMoviesQuery((prev) =>
      prev.genreId === nextGenreId
        ? prev
        : {
            ...prev,
            genreId: nextGenreId,
            page: 1,
            revision: prev.revision + 1,
          }
    );
  };

  const handlePageChange = (nextPage) => {
    setMoviesQuery((prev) =>
      prev.page === nextPage
        ? prev
        : {
            ...prev,
            page: nextPage,
            revision: prev.revision + 1,
          }
    );
  };

  const handleResetFilters = () => {
    setSearchInput('');
    setMoviesQuery((prev) => ({
      search: '',
      genreId: null,
      page: 1,
      revision: prev.revision + 1,
    }));
  };

  const handleRetryMovies = () => {
    setMoviesQuery((prev) => ({
      ...prev,
      revision: prev.revision + 1,
    }));
  };

  const hasActiveFilters =
    Boolean(moviesQuery.search) || moviesQuery.genreId !== null;
  const genreOptions = [
    { label: 'Tất cả thể loại', value: '' },
    ...genres.map((genre) => ({
      label: translateGenreName(genre.name),
      value: genre.id,
    })),
  ];

  const featuredAgeMeta = featuredMovie
    ? getAgeRatingMeta(featuredMovie.ageRating)
    : null;

  let catalogSubtitle;
  if (moviesLoading) {
    catalogSubtitle = 'Đang cập nhật danh sách phim...';
  } else if (moviesError) {
    catalogSubtitle = 'Không thể tải số lượng phim';
  } else if (moviesResult.totalCount === 0) {
    catalogSubtitle = 'Không có phim phù hợp';
  } else if (moviesResult.items.length === 0) {
    catalogSubtitle = `Trang ${moviesResult.page} không có phim (${moviesResult.totalCount} phim hiện có)`;
  } else {
    catalogSubtitle = `${moviesResult.totalCount} phim phù hợp`;
  }

  return (
    <div className="mb-page">
      {/* Hero Spotlight dựa trên dữ liệu phim thật từ API */}
      {moviesLoading && !featuredMovie ? (
        <section className="mb-hero mb-surface" aria-label="Đang tải phim nổi bật">
          <div className="mb-hero__grid">
            <div className="mb-hero__content">
              <Skeleton active paragraph={{ rows: 3 }} />
            </div>
            <div className="mb-hero__poster-wrap">
              <div className="mb-skeleton-poster" />
            </div>
          </div>
        </section>
      ) : featuredMovie ? (
        <section className="mb-hero mb-surface" aria-labelledby="hero-movie-title">
          <div className="mb-hero__grid">
            <div className="mb-hero__content">
              <div className="mb-hero__eyebrow-row">
                <span className="mb-hero__spotlight-pill">Phim nổi bật</span>
                <span
                  className={`mb-age-badge mb-age-badge--${featuredAgeMeta.tone}`}
                  title={featuredAgeMeta.description}
                >
                  {featuredAgeMeta.badgeText}
                </span>
              </div>

              <h1 id="hero-movie-title" className="mb-hero__title">
                {featuredMovie.title}
              </h1>

              <dl className="mb-hero__facts">
                <div className="mb-hero__fact">
                  <dt>Thời lượng</dt>
                  <dd>{featuredMovie.duration} phút</dd>
                </div>
                <div className="mb-hero__fact">
                  <dt>Khởi chiếu</dt>
                  <dd>{formatDateOnlyVi(featuredMovie.releaseDate)}</dd>
                </div>
                {Array.isArray(featuredMovie.genres) &&
                  featuredMovie.genres.length > 0 && (
                    <div className="mb-hero__fact">
                      <dt>Thể loại</dt>
                      <dd>
                        {featuredMovie.genres
                          .map(translateGenreName)
                          .join(', ')}
                      </dd>
                    </div>
                  )}
              </dl>

              <div className="mb-hero__actions">
                <Link
                  to={`/movies/${encodeURIComponent(featuredMovie.slug)}`}
                  className="mb-btn mb-btn--primary mb-btn--lg"
                >
                  Xem chi tiết & lịch chiếu
                </Link>
              </div>
            </div>

            <div className="mb-hero__poster-wrap">
              <MoviePoster
                src={featuredMovie.posterUrl}
                title={featuredMovie.title}
                ageRating={featuredMovie.ageRating}
                loading="eager"
              />
            </div>
          </div>
        </section>
      ) : null}

      {/* Danh sách phim đang chiếu + Bộ lọc */}
      <section className="mb-section" aria-labelledby="catalog-heading">
        <div className="mb-section__header">
          <div>
            <h2 id="catalog-heading" className="mb-section__title">
              Phim đang chiếu
            </h2>
            <p className="mb-section__subtitle">{catalogSubtitle}</p>
          </div>
        </div>

        <form
          className="mb-filter-bar mb-surface"
          onSubmit={handleSearchSubmit}
          role="search"
          aria-label="Tìm kiếm và lọc phim"
        >
          <div className="mb-filter-bar__search">
            <label htmlFor="movie-search-input" className="mb-filter-bar__label">
              Tìm theo tên phim
            </label>
            <div className="mb-filter-bar__search-row">
              <Input
                id="movie-search-input"
                value={searchInput}
                onChange={(e) => setSearchInput(e.target.value)}
                placeholder="Nhập tên phim (ví dụ: Hành trình, Đêm...)"
                allowClear
                prefix={<SearchIcon />}
                maxLength={120}
              />
              <Button type="primary" htmlType="submit">
                Tìm kiếm
              </Button>
            </div>
          </div>

          <div className="mb-filter-bar__genre">
            <label htmlFor="movie-genre-select" className="mb-filter-bar__label">
              Thể loại
            </label>
            <Select
              id="movie-genre-select"
              value={moviesQuery.genreId === null ? '' : moviesQuery.genreId}
              onChange={(val) =>
                handleGenreChange(val === '' ? null : Number(val))
              }
              options={genreOptions}
              loading={genresLoading}
              className="mb-filter-bar__select"
              aria-label="Lọc theo thể loại phim"
            />
          </div>

          {hasActiveFilters && (
            <div className="mb-filter-bar__reset">
              <Button type="default" onClick={handleResetFilters}>
                Xóa bộ lọc
              </Button>
            </div>
          )}
        </form>

        {genresError && (
          <Alert
            type="warning"
            showIcon
            title={genresError}
            action={
              <Button
                size="small"
                onClick={() => setGenresRevision((r) => r + 1)}
              >
                Tải lại thể loại
              </Button>
            }
          />
        )}

        {moviesLoading ? (
          <div className="mb-movie-grid" aria-busy="true" aria-label="Đang tải danh sách phim">
            {Array.from({ length: SKELETON_COUNT }).map((_, index) => (
              <div key={index} className="mb-movie-skeleton mb-surface">
                <div className="mb-skeleton-poster" />
                <div className="mb-movie-skeleton__body">
                  <Skeleton active title={{ width: '80%' }} paragraph={{ rows: 2 }} />
                </div>
              </div>
            ))}
          </div>
        ) : moviesError ? (
          <div className="mb-state-box mb-surface" role="alert">
            <Alert
              type="error"
              showIcon
              title="Không thể tải danh sách phim"
              description={moviesError}
              action={
                <Button type="primary" onClick={handleRetryMovies}>
                  Thử lại
                </Button>
              }
            />
          </div>
        ) : moviesResult.totalCount === 0 ? (
          <div className="mb-state-box mb-surface">
            <Empty
              description="Không tìm thấy phim phù hợp."
              image={Empty.PRESENTED_IMAGE_SIMPLE}
            >
              {hasActiveFilters && (
                <Button type="primary" onClick={handleResetFilters}>
                  Hiển thị tất cả phim
                </Button>
              )}
            </Empty>
          </div>
        ) : (
          <>
            {moviesResult.items.length === 0 ? (
              <div className="mb-state-box mb-surface">
                <Empty
                  description={`Trang ${moviesResult.page} không có phim. Tổng cộng hiện có ${moviesResult.totalCount} phim.`}
                  image={Empty.PRESENTED_IMAGE_SIMPLE}
                >
                  <Button type="primary" onClick={() => handlePageChange(1)}>
                    Quay về trang 1
                  </Button>
                </Empty>
              </div>
            ) : (
              <div className="mb-movie-grid">
                {moviesResult.items.map((movie) => (
                  <MovieCard key={movie.id} movie={movie} />
                ))}
              </div>
            )}

            <div className="mb-pagination">
              <Pagination
                current={moviesResult.page}
                pageSize={moviesResult.pageSize}
                total={moviesResult.totalCount}
                responsive
                showSizeChanger={false}
                onChange={handlePageChange}
                showTotal={(total, range) =>
                  range[0] <= range[1]
                    ? `Hiển thị ${range[0]}–${range[1]} trên ${total} phim`
                    : `Tổng ${total} phim`
                }
              />
            </div>
          </>
        )}
      </section>
    </div>
  );
}
