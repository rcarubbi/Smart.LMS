import type { ReactNode } from "react";

export default function Panel({
  title,
  icon,
  action,
  children,
  className = "",
  accent = "#6366f1",
}: {
  title?: ReactNode;
  icon?: string;
  action?: ReactNode;
  children: ReactNode;
  className?: string;
  accent?: string;
}) {
  return (
    <section
      className={`glass glass-hover overflow-hidden ${className}`}
      style={{ borderTop: `3px solid ${accent}` }}
    >
      {title && (
        <div className="flex items-center justify-between gap-3 border-b border-white/10 px-5 py-3">
          <h3 className="flex items-center gap-2 text-sm font-medium uppercase tracking-wide text-white/80">
            {icon && <i className="material-icon" style={{ color: accent }}>{icon}</i>}
            {title}
          </h3>
          {action}
        </div>
      )}
      <div className="p-5">{children}</div>
    </section>
  );
}
