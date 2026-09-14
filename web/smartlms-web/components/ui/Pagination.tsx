"use client";

import Icon from "./Icon";

function pageNumbers(current: number, totalPages: number): (number | "…")[] {
  if (totalPages <= 7) {
    return Array.from({ length: totalPages }, (_, i) => i + 1);
  }

  const first = 1;
  const last = totalPages;
  const windowStart = Math.max(first + 1, current - 1);
  const windowEnd = Math.min(last - 1, current + 1);
  const pages: (number | "…")[] = [first];

  if (windowStart > first + 1) {
    pages.push("…");
  }

  for (let i = windowStart; i <= windowEnd; i++) {
    pages.push(i);
  }

  if (windowEnd < last - 1) {
    pages.push("…");
  }

  pages.push(last);
  return pages;
}

export default function Pagination({
  page,
  total,
  pageSize,
  onChange,
}: {
  page: number;
  total: number;
  pageSize: number;
  onChange: (page: number) => void;
}) {
  const totalPages = Math.max(1, Math.ceil(total / Math.max(1, pageSize)));
  if (totalPages <= 1) return null;

  const pages = pageNumbers(page, totalPages);

  const buttonClass = (active: boolean) =>
    `min-w-8 rounded px-2 py-1 text-sm transition ${
      active ? "bg-white/20 text-white" : "text-white/50 hover:bg-white/10"
    }`;

  return (
    <nav className="mt-4 flex flex-wrap items-center justify-center gap-1 text-sm" aria-label="Paginação">
      <button type="button" disabled={page <= 1} onClick={() => onChange(page - 1)} className={buttonClass(false)} aria-label="Anterior">
        <Icon name="chevron_left" />
      </button>
      {pages.map((p, idx) =>
        p === "…" ? (
          <span key={`gap-${idx}`} className="px-1 text-white/40">
            …
          </span>
        ) : (
          <button
            key={p}
            type="button"
            onClick={() => onChange(p)}
            className={buttonClass(p === page)}
            aria-current={p === page ? "page" : undefined}
          >
            {p}
          </button>
        ),
      )}
      <button type="button" disabled={page >= totalPages} onClick={() => onChange(page + 1)} className={buttonClass(false)} aria-label="Próxima">
        <Icon name="chevron_right" />
      </button>
    </nav>
  );
}