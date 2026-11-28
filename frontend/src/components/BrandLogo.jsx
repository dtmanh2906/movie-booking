import cinemaEmblem from '../assets/cinema-emblem.svg';

/**
 * Biểu tượng nhận diện thương hiệu MovieBooking dùng ở Header và Footer.
 */
export function BrandLogo({ compact = false }) {
  return (
    <span className="mb-brand">
      <img
        src={cinemaEmblem}
        alt=""
        width={compact ? 28 : 34}
        height={compact ? 28 : 34}
        className="mb-brand__icon"
      />
      <span className="mb-brand__text">
        <span className="mb-brand__name">Movie</span>
        <span className="mb-brand__accent">Booking</span>
      </span>
    </span>
  );
}
