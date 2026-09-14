"use client";

import Link from "next/link";
import { usePagedList } from "@/lib/usePagedList";
import AppShell from "@/components/AppShell";
import Panel from "@/components/ui/Panel";
import Pagination from "@/components/ui/Pagination";

type MyPlan = { classroomId: string; name: string; deliveredCount: number; totalCount: number };
type Progress = { classId: string; className: string; courseName: string; percentual: number; watchedSeconds: number; lastAccessAtUtc: string };

export default function MePage() {
  const { items: plans, total: plansTotal, page: plansPage, setPage: setPlansPage } = usePagedList<MyPlan>("/api/me/classrooms", {});
  const { items: progress, total: progressTotal, page: progressPage, setPage: setProgressPage } = usePagedList<Progress>("/api/me/progress", {});

  function bar(v: number) {
    const w = Math.max(0, Math.min(100, v));
    return (
      <div className="h-2 overflow-hidden rounded-full bg-white/10">
        <div className="h-2 rounded-full bg-gradient-to-r from-indigo-500 to-sky-400" style={{ width: `${w}%` }} />
      </div>
    );
  }

  return (
    <AppShell>
      <div className="grid gap-6 lg:grid-cols-2">
        <Panel icon="groups" title="Minhas turmas" accent="#ec4899">
          {plans.length === 0 ? (
            <p className="text-sm text-white/50">Você não está em nenhuma turma.</p>
          ) : (
            <ul className="space-y-4">
              {plans.map((p) => (
                <li key={p.classroomId} className="space-y-1.5">
                  <div className="flex items-center justify-between text-sm">
                    <span className="font-medium">{p.name}</span>
                    <span className="text-xs text-white/50">{p.deliveredCount}/{p.totalCount} aulas liberadas</span>
                  </div>
                  {bar(p.totalCount ? (p.deliveredCount / p.totalCount) * 100 : 0)}
                </li>
              ))}
            </ul>
          )}
          <Pagination page={plansPage} total={plansTotal} pageSize={25} onChange={setPlansPage} />
        </Panel>

        <Panel icon="insights" title="Meu progresso" accent="#6366f1">
          {progress.length === 0 ? (
            <p className="text-sm text-white/50">Nenhum acesso registrado.</p>
          ) : (
            <ul className="space-y-3">
              {progress.map((p) => (
                <li key={p.classId}>
                  <Link href={`/class/${p.classId}`} className="block rounded-lg border border-white/10 bg-white/5 p-3 transition hover:bg-white/10">
                    <div className="mb-1 flex items-center justify-between">
                      <span className="text-sm font-medium">{p.className}</span>
                      <span className={`text-xs font-semibold ${p.percentual >= 100 ? "text-emerald-400" : "text-indigo-300"}`}>{p.percentual}%</span>
                    </div>
                    <p className="mb-1.5 text-xs text-white/40">{p.courseName}</p>
                    {bar(p.percentual)}
                  </Link>
                </li>
              ))}
            </ul>
          )}
          <Pagination page={progressPage} total={progressTotal} pageSize={25} onChange={setProgressPage} />
        </Panel>
      </div>
    </AppShell>
  );
}
