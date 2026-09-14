"use client";

import { Suspense, useEffect, useState } from "react";
import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { apiList, errorMessage } from "@/lib/api";
import { usePagedList } from "@/lib/usePagedList";
import AppShell from "@/components/AppShell";
import Panel from "@/components/ui/Panel";
import Icon from "@/components/ui/Icon";
import Pagination from "@/components/ui/Pagination";

type Subject = { id: string; name: string; order: number; knowledgeAreaId: string; knowledgeAreaName: string };
type Course = { id: string; name: string; order: number; subjectId: string; classCount: number };

export default function SubjectsIndex() {
  return (
    <Suspense fallback={null}>
      <SubjectsInner />
    </Suspense>
  );
}

function SubjectsInner() {
  const search = useSearchParams();
  const subjectId = search.get("subject");
  const areaId = search.get("area");

  const [subjects, setSubjects] = useState<Subject[]>([]);
  const [error, setError] = useState<string | null>(null);
  const { items: courses, total, page, setPage } = usePagedList<Course>(
    subjectId ? "/api/courses" : "",
    { subjectId: subjectId ?? "", active: true },
  );

  useEffect(() => {
    apiList<Subject>("/api/subjects?active=true")
      .then(setSubjects)
      .catch((e) => setError(errorMessage(e)));
  }, []);

  const subject = subjects.find((s) => s.id === subjectId) ?? null;
  const area = subject ? subject.knowledgeAreaId : areaId;
  const siblings = subjects.filter((s) => s.knowledgeAreaId === area && s.id !== subjectId);

  return (
    <AppShell>
      <div className="mb-4 flex items-center gap-2 text-sm text-white/50">
        <Link href="/" className="text-indigo-300 hover:underline">
          Início
        </Link>
        {area && (
          <>
            <Icon name="chevron_right" />
            <Link href={`/areas?area=${area}`} className="text-indigo-300 hover:underline">
              {subject ? subject.knowledgeAreaName : "Área"}
            </Link>
          </>
        )}
      </div>

      <Panel
        icon="import_contacts"
        title={subject ? `Cursos — ${subject.name}` : "Cursos do assunto"}
        accent="#6366f1"
      >
        {error && <p className="mb-3 rounded border border-red-800 bg-red-950 p-2 text-sm text-red-400">{error}</p>}

        <div className="grid gap-6 lg:grid-cols-[1fr_280px]">
          <div>
            {!subject ? (
              <p className="text-sm text-white/50">Selecione um assunto para ver os cursos.</p>
            ) : courses.length === 0 ? (
              <p className="text-sm text-white/50">Nenhum curso neste assunto ainda.</p>
            ) : (
              <ul className="divide-y divide-white/5 overflow-hidden rounded-lg border border-white/10 bg-white/5">
                {courses.map((c) => (
                  <li key={c.id} className="hover:bg-white/5">
                    <Link
                      href={`/course/${c.id}`}
                      className="flex items-center justify-between gap-3 px-4 py-3 text-sm text-white/80 hover:text-white"
                    >
                      <span>{c.name}</span>
                      <span className="flex items-center gap-2">
                        <span className="text-xs text-white/40">{c.classCount} aulas</span>
                        <Icon name="chevron_right" />
                      </span>
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
            {siblings.length === 0 ? (
              <p className="text-sm text-white/40">Nenhum outro assunto nesta área.</p>
            ) : (
              <ul className="space-y-2">
                {siblings.map((s) => (
                  <li key={s.id}>
                    <Link
                      href={`/subjects?subject=${s.id}&area=${s.knowledgeAreaId}`}
                      className="text-sm text-white/60 hover:text-white"
                    >
                      {s.name}
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