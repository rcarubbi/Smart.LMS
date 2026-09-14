"use client";

export type ViewFilter = "active" | "trash" | "all";

export default function ViewTabs({
  view,
  onChange,
  total,
  labels,
}: {
  view: ViewFilter;
  onChange: (view: ViewFilter) => void;
  total: number;
  labels?: { active?: string; trash?: string; all?: string };
}) {
  const active = labels?.active ?? "Ativos";
  const trash = labels?.trash ?? "Lixeira";
  const all = labels?.all ?? "Todos";

  return (
    <div className="mb-4 flex flex-wrap items-center gap-1 text-sm">
      {(
        [
          ["active", `${active}${view === "active" ? ` (${total})` : ""}`],
          ["trash", `${trash}${view === "trash" ? ` (${total})` : ""}`],
          ["all", all],
        ] as const
      ).map(([value, label]) => (
        <button
          key={value}
          onClick={() => onChange(value)}
          className={`rounded-full px-3 py-1 transition ${
            view === value ? "bg-white/20 text-white" : "text-white/50 hover:bg-white/10"
          }`}
        >
          {label}
        </button>
      ))}
    </div>
  );
}