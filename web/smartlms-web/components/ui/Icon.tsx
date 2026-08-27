export default function Icon({
  name,
  className = "",
}: {
  name: string;
  className?: string;
}) {
  return (
    <span className={`material-icon ${className}`} aria-hidden="true">
      {name}
    </span>
  );
}
