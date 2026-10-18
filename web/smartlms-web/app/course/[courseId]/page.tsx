"use client";

import { useCallback, useEffect, useState } from "react";
import Image from "next/image";
import { useParams } from "next/navigation";
import Link from "next/link";
import { api, apiList, currentUser, errorMessage } from "@/lib/api";
import { usePagedList } from "@/lib/usePagedList";
import AppShell from "@/components/AppShell";
import Panel from "@/components/ui/Panel";
import Icon from "@/components/ui/Icon";
import Pagination from "@/components/ui/Pagination";
import { useToast } from "@/components/ui/Toast";

type CourseDetail = {
  id: string;
  name: string;
  order: number;
  image?: string | null;
  subjectId: string;
  subjectName: string;
  teacherInChargeId?: string | null;
  classes: { id: string; name: string; order: number; deliveryDays: number; contentType: string }[];
};

type AssessmentItem = { id: string; courseName: string; minimumScore: number; questionCount: number };

type ProgressItem = { classId: string; className: string; courseName: string; percentual: number; watchedSeconds: number; lastAccessAtUtc: string };

export default function CoursePage() {
  const params = useParams<{ courseId: string }>();
  const courseId = params.courseId;
  const { toast } = useToast();
  const user = currentUser();
  const [course, setCourse] = useState<CourseDetail | null>(null);
  const { items: assessments, total: aTotal, page, setPage } = usePagedList<AssessmentItem>(
    `/api/courses/${courseId}/assessments`,
    { active: true },
  );
  const [progressMap, setProgressMap] = useState<Record<string, number>>({});

  const load = useCallback(async () => {
    try {
      setCourse(await api<CourseDetail>(`/api/courses/${courseId}`));
    } catch (e) {
      toast("error", errorMessage(e, "Curso não encontrado."));
    }
  }, [courseId, toast]);

  useEffect(() => {
    // This effect fetches remote course data; state updates follow the response.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    load();
  }, [load]);

  useEffect(() => {
    if (user?.role === "Admin") {
      // Admins do not request student progress; clear any stale role-specific state.
      // eslint-disable-next-line react-hooks/set-state-in-effect
      setProgressMap({});
      return;
    }
    apiList<ProgressItem>("/api/me/progress")
      .then((progress) => {
        const map: Record<string, number> = {};
        for (const item of progress) map[item.classId] = item.percentual;
        setProgressMap(map);
      })
      .catch(() => undefined);
  }, [user?.role]);

  if (!course) {
    return (
      <AppShell>
        <p className="text-sm text-white/50">Carregando curso...</p>
      </AppShell>
    );
  }

  return (
    <AppShell>
      <div className="mb-6 overflow-hidden rounded-2xl border border-white/10">
        {course.image ? (
          <Image src={course.image} alt="" width={1200} height={176} unoptimized className="h-44 w-full object-cover" />
        ) : (
          <div className="flex h-32 items-center justify-center bg-gradient-to-r from-indigo-900/40 to-sky-900/40">
            <Icon name="school" className="text-5xl text-white/30" />
          </div>
        )}
        <div className="glass rounded-none border-0 border-t border-white/10 p-5">
          <p className="text-xs uppercase tracking-wide text-white/40">{course.subjectName}</p>
          <h1 className="text-2xl font-semibold">{course.name}</h1>
        </div>
      </div>

      <div className="grid gap-6 lg:grid-cols-3">
        <Panel icon="play_circle" title={`Aulas (${course.classes.length})`} accent="#f59e0b" className="lg:col-span-2">
          {course.classes.length === 0 ? (
            <p className="text-sm text-white/50">Nenhuma aula neste curso.</p>
          ) : (
            <ol className="space-y-2">
              {course.classes.map((c) => {
                const pct = progressMap[c.id] ?? 0;
                const locked = user?.role !== "Admin" && pct < 100;
                return (
                  <li key={c.id}>
                    <Link
                      href={`/class/${c.id}`}
                      className="flex items-center gap-3 rounded-lg border border-white/10 bg-white/5 px-4 py-3 transition hover:border-indigo-500/50 hover:bg-white/10"
                    >
                      <span className="material-icon rounded-full bg-white/10 p-2 text-base leading-none">{c.contentType === "Vimeo" ? "videocam" : "play_circle"}</span>
                      <div className="flex-1">
                        <div className="font-medium">{c.name}</div>
                        <div className="text-xs text-white/40">
                          {c.contentType} • liberada em {c.deliveryDays} dia(s) •{" "}
                          {locked ? (
                            <span className="inline-flex items-center gap-1 text-amber-400">
                              <Icon name="lock" className="text-sm" /> {pct}% concluída
                            </span>
                          ) : pct >= 100 ? (
                            <span className="inline-flex items-center gap-1 text-emerald-400">
                              <Icon name="check_circle" className="text-sm" /> Concluída
                            </span>
                          ) : (
                            <span>{pct}% assistida</span>
                          )}
                        </div>
                      </div>
                      <span className="material-icon text-white/30">{locked ? "lock" : "chevron_right"}</span>
                    </Link>
                  </li>
                );
              })}
            </ol>
          )}
        </Panel>

        <Panel icon="assignment" title="Avaliações" accent="#ec4899">
          {assessments.length === 0 ? (
            <p className="text-sm text-white/50">Nenhuma avaliação.</p>
          ) : (
            <ul className="space-y-2">
              {assessments.map((a) => (
                <li key={a.id}>
                  <Link
                    href={`/assessment/${a.id}`}
                    className="flex items-center gap-3 rounded-lg border border-white/10 bg-white/5 px-4 py-3 transition hover:border-pink-500/50 hover:bg-white/10"
                  >
                    <Icon name="quiz" className="text-pink-400" />
                    <div className="flex-1">
                      <div className="font-medium">{a.questionCount} questões</div>
                      <div className="text-xs text-white/40">Nota mínima: {a.minimumScore}</div>
                    </div>
                    <span className="material-icon text-white/30">chevron_right</span>
                  </Link>
                </li>
              ))}
            </ul>
          )}
          <Pagination page={page} total={aTotal} pageSize={25} onChange={setPage} />
          {user?.role === "Admin" && (
            <Link href={`/admin/assessments?course=${courseId}`} className="btn btn-ghost mt-4 w-full justify-center text-sm">
              <Icon name="add" /> Administrar avaliações
            </Link>
          )}
        </Panel>
      </div>
    </AppShell>
  );
}
