import { Link } from 'react-router-dom';
import { formatDateOnlyVi } from '../utils/dateTime';
import { getAgeRatingMeta, translateGenreName } from '../utils/movieHelpers';
import { MoviePoster } from './MoviePoster';
import './MovieCard.css';

/**
 * Thẻ hiển thị tóm tắt phim trong danh sách (theo MovieListItemDto).
 * Sử dụng phần tử Link đơn nhất cho hành động điều hướng, không lồng Button.
 */
export function MovieCard({ movie }) {
  const ageMeta = getAgeRatingMeta(movie.ageRating);
  const detailPath = `/movies/${encodeURIComponent(movie.slug)}`;
  const genres = Array.isArray(movie.genres) ? movie.genres : [];

  return (
    <article className="mb-movie-card mb-surface">
      <div className="mb-movie-card__media">
        <MoviePoster
          src={movie.posterUrl}
          title={movie.title}
          ageRating={movie.ageRating}
          className="mb-movie-card__poster"
        />
        <span
          className={`mb-age-badge mb-age-badge--${ageMeta.tone}`}
          title={ageMeta.description}
        >
          {ageMeta.badgeText}
        </span>
      </div>

      <div className="mb-movie-card__body">
        <h3 className="mb-movie-card__title">
          <Link to={detailPath} className="mb-movie-card__title-link">
            {movie.title}
          </Link>
        </h3>

        <dl className="mb-movie-card__meta">
          <div className="mb-movie-card__meta-item">
            <dt>Thời lượng</dt>
            <dd>{movie.duration} phút</dd>
          </div>
          <div className="mb-movie-card__meta-item">
            <dt>Khởi chiếu</dt>
            <dd>{formatDateOnlyVi(movie.releaseDate)}</dd>
          </div>
        </dl>

        {genres.length > 0 && (
          <ul className="mb-movie-card__genres" aria-label="Thể loại phim">
            {genres.map((genreName) => (
              <li key={genreName} className="mb-genre-chip">
                {translateGenreName(genreName)}
              </li>
            ))}
          </ul>
        )}

        <div className="mb-movie-card__footer">
          <Link
            to={detailPath}
            aria-label={`Xem chi tiết phim ${movie.title}`}
            className="mb-btn mb-btn--secondary mb-movie-card__cta"
          >
            Xem chi tiết
          </Link>
        </div>
      </div>
    </article>
  );
}
