"use client";

import Link from "next/link";
import { usePagedList } from "@/lib/usePagedList";
import AppShell from "@/components/AppShell";
import Panel from "@/components/ui/Panel";
import Pagination from "@/components/ui/Pagination";

type MyPlan = { classroomId: string; name: string; deliveredCount: number; totalCount: number };
type Progress = { classId: string; className: string; courseName: string; percentual: number; watchedSeconds: number; lastAccessAtUtc: string };
type MyCourse = { courseId: string; courseName: string; deliveredCount: number; totalCount: number; percentual: number };
type ReleasedClass = { classId: string; className: string; courseId: string; courseName: string; releasedAtUtc: string };

export default function MePage() {
  const { items: plans, total: plansTotal, page: plansPage, setPage: setPlansPage } = usePagedList<MyPlan>("/api/me/classrooms", {});
  const { items: progress, total: progressTotal, page: progressPage, setPage: setProgressPage } = usePagedList<Progress>("/api/me/progress", {});
  const { items: courses, total: coursesTotal, page: coursesPage, setPage: setCoursesPage } = usePagedList<MyCourse>("/api/me/courses", {});
  const { items: released, total: releasedTotal, page: releasedPage, setPage: setReleasedPage } = usePagedList<ReleasedClass>("/api/me/released-classes", {});

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
        <Panel icon="menu_book" title="Meus cursos" accent="#0ea5e9">
          {courses.length === 0 ? <p className="text-sm text-white/50">Nenhum curso atribuído.</p> : (
            <ul className="space-y-3">{courses.map((course) => (
              <li key={course.courseId}>
                <Link href={`/course/${course.courseId}`} className="block rounded-lg border border-white/10 bg-white/5 p-3 hover:bg-white/10">
                  <div className="flex justify-between gap-3 text-sm"><span className="font-medium">{course.courseName}</span><span className="text-white/50">{course.deliveredCount}/{course.totalCount}</span></div>
                  {bar(course.percentual)}
                </Link>
              </li>
            ))}</ul>
          )}
          <Pagination page={coursesPage} total={coursesTotal} pageSize={25} onChange={setCoursesPage} />
        </Panel>

        <Panel icon="new_releases" title="Aulas liberadas recentemente" accent="#14b8a6">
          {released.length === 0 ? <p className="text-sm text-white/50">Nenhuma aula liberada.</p> : (
            <ul className="space-y-2">{released.map((item) => (
              <li key={item.classId}>
                <Link href={`/class/${item.classId}`} className="block rounded-lg border border-white/10 bg-white/5 p-3 hover:bg-white/10">
                  <span className="block text-sm font-medium">{item.className}</span>
                  <span className="text-xs text-white/40">{item.courseName} · {new Date(item.releasedAtUtc).toLocaleDateString("pt-BR")}</span>
                </Link>
              </li>
            ))}</ul>
          )}
          <Pagination page={releasedPage} total={releasedTotal} pageSize={25} onChange={setReleasedPage} />
        </Panel>

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
