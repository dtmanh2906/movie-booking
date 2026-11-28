import axiosClient from './axiosClient';

/**
 * Loại bỏ tham số rỗng (undefined, null, chuỗi chỉ có khoảng trắng),
 * nhưng vẫn giữ lại giá trị hợp lệ như số 0 hoặc false.
 */
export function cleanParams(params = {}) {
  const result = {};

  Object.entries(params).forEach(([key, value]) => {
    if (value === undefined || value === null) {
      return;
    }

    if (typeof value === 'string') {
      const trimmed = value.trim();
      if (trimmed === '') {
        return;
      }
      result[key] = trimmed;
      return;
    }

    result[key] = value;
  });

  return result;
}

/**
 * Lấy danh sách phim đang chiếu có hỗ trợ tìm kiếm, lọc thể loại và phân trang.
 * Khớp hợp đồng Backend: GET /api/movies -> PagedResult<MovieListItemDto>
 * @param {object} [params] - { search, genreId, page, pageSize }
 * @param {object} [options] - Tùy chọn request (hỗ trợ { signal } từ AbortController)
 */
export async function getMovies(params = {}, options = {}) {
  const response = await axiosClient.get('/api/movies', {
    params: cleanParams(params),
    signal: options.signal,
  });
  return response.data;
}

/**
 * Lấy thông tin chi tiết một bộ phim theo slug.
 * Khớp hợp đồng Backend: GET /api/movies/{slug} -> MovieDetailDto
 * @param {string} slug
 * @param {object} [options] - Tùy chọn request (hỗ trợ { signal } từ AbortController)
 */
export async function getMovieBySlug(slug, options = {}) {
  const response = await axiosClient.get(
    `/api/movies/${encodeURIComponent(slug)}`,
    {
      signal: options.signal,
    }
  );
  return response.data;
}

/**
 * Lấy danh sách suất chiếu của một phim theo slug (lọc tùy chọn theo ngày và rạp).
 * Khớp hợp đồng Backend: GET /api/movies/{slug}/showtimes -> List<ShowtimeDto>
 * @param {string} slug
 * @param {object} [params] - { date (YYYY-MM-DD), cinemaId }
 * @param {object} [options] - Tùy chọn request (hỗ trợ { signal } từ AbortController)
 */
export async function getShowtimes(slug, params = {}, options = {}) {
  const response = await axiosClient.get(
    `/api/movies/${encodeURIComponent(slug)}/showtimes`,
    {
      params: cleanParams(params),
      signal: options.signal,
    }
  );
  return response.data;
}

/**
 * Lấy danh sách tất cả thể loại phim.
 * Khớp hợp đồng Backend: GET /api/genres -> IReadOnlyList<GenreDto>
 * @param {object} [options] - Tùy chọn request (hỗ trợ { signal } từ AbortController)
 */
export async function getGenres(options = {}) {
  const response = await axiosClient.get('/api/genres', {
    signal: options.signal,
  });
  return response.data;
}

/**
 * Lấy danh sách rạp đang hoạt động (lọc tùy chọn theo thành phố).
 * Khớp hợp đồng Backend: GET /api/cinemas -> List<PublicCinemaDto>
 * @param {object} [params] - { city }
 * @param {object} [options] - Tùy chọn request (hỗ trợ { signal } từ AbortController)
 */
export async function getCinemas(params = {}, options = {}) {
  const response = await axiosClient.get('/api/cinemas', {
    params: cleanParams(params),
    signal: options.signal,
  });
  return response.data;
}
