"use client";

import { Suspense, useEffect, useState } from "react";
import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { apiList, errorMessage } from "@/lib/api";
import type { ListItem } from "@/lib/api";
import { usePagedList } from "@/lib/usePagedList";
import AppShell from "@/components/AppShell";
import Panel from "@/components/ui/Panel";
import Icon from "@/components/ui/Icon";
import Pagination from "@/components/ui/Pagination";

export default function AreasIndex() {
  return (
    <Suspense fallback={null}>
      <AreasInner />
    </Suspense>
  );
}

function AreasInner() {
  const search = useSearchParams();
  const areaId = search.get("area");

  const [areas, setAreas] = useState<ListItem[]>([]);
  const [error, setError] = useState<string | null>(null);
  const { items: subjects, total, page, setPage } = usePagedList<ListItem>(
    areaId ? "/api/subjects" : "",
    { knowledgeAreaId: areaId ?? "", active: true },
  );

  useEffect(() => {
    apiList<ListItem>("/api/knowledge-areas?active=true")
      .then(setAreas)
      .catch((e) => setError(errorMessage(e)));
  }, []);

  const current = areas.find((a) => a.id === areaId) ?? null;
  const others = areas.filter((a) => a.id !== areaId);

  return (
    <AppShell>
      <Panel
        icon="school"
        title={current ? `Assuntos — ${current.name}` : "Assuntos por área"}
        accent="#6366f1"
      >
        {error && <p className="mb-3 rounded border border-red-800 bg-red-950 p-2 text-sm text-red-400">{error}</p>}

        {current && (
          <p className="mb-4 text-sm text-white/50">
            Escolha um assunto para ver os cursos disponíveis.
          </p>
        )}

        <div className="grid gap-6 lg:grid-cols-[1fr_280px]">
          <div>
            {!current ? (
              <>
                <p className="mb-4 text-sm text-white/50">
                  Escolha uma área de conhecimento para ver os assuntos.
                </p>
                <ul className="divide-y divide-white/5 overflow-hidden rounded-lg border border-white/10 bg-white/5">
                  {areas.map((a) => (
                    <li key={a.id} className="hover:bg-white/5">
                      <Link
                        href={`/areas?area=${a.id}`}
                        className="flex items-center justify-between gap-3 px-4 py-3 text-sm text-white/80 hover:text-white"
                      >
                        <span>{a.name}</span>
                        <Icon name="chevron_right" />
                      </Link>
                    </li>
                  ))}
                </ul>
              </>
            ) : subjects.length === 0 ? (
              <p className="text-sm text-white/50">Nenhum assunto nesta área ainda.</p>
            ) : (
              <ul className="divide-y divide-white/5 overflow-hidden rounded-lg border border-white/10 bg-white/5">
                {subjects.map((s) => (
                  <li key={s.id} className="hover:bg-white/5">
                    <Link
                      href={`/subjects?subject=${s.id}&area=${areaId}`}
                      className="flex items-center justify-between gap-3 px-4 py-3 text-sm text-white/80 hover:text-white"
                    >
                      <span>{s.name}</span>
                      <Icon name="chevron_right" />
                    </Link>
                  </li>
                ))}
              </ul>
            )}
            <Pagination page={page} total={total} pageSize={25} onChange={setPage} />
          </div>

          <aside className="lg:border-l lg:border-white/10 lg:pl-4">
            <h4 className="mb-3 text-xs uppercase tracking-wide text-white/40">
              Veja também...
            </h4>
            {others.length === 0 ? (
              <p className="text-sm text-white/40">Nenhuma outra área cadastrada.</p>
            ) : (
              <ul className="space-y-2">
                {others.map((a) => (
                  <li key={a.id}>
                    <Link
                      href={`/areas?area=${a.id}`}
                      className="text-sm text-white/60 hover:text-white"
                    >
                      {a.name}
                    </Link>
                  </li>
                ))}
              </ul>
            )}
          </aside>
        </div>
      </Panel>
    </AppShell>
  );
}