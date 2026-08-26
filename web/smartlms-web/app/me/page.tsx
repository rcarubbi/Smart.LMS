"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";
import Nav from "@/components/Nav";

type MyPlan = { classroomId: string; name: string; deliveredCount: number; totalCount: number };
type Progress = { classId: string; className: string; courseName: string; percentual: number; lastAccessAtUtc: string };
type NoticeItem = { id: string; text: string; seen: boolean };

export default function MePage() {
  const [plans, setPlans] = useState<MyPlan[]>([]);
  const [progress, setProgress] = useState<Progress[]>([]);
  const [notices, setNotices] = useState<NoticeItem[]>([]);

  useEffect(() => {
    api<MyPlan[]>("/api/me/classrooms").then(setPlans).catch(() => setPlans([]));
    api<Progress[]>("/api/me/progress").then(setProgress).catch(() => setProgress([]));
    api<NoticeItem[]>("/api/notices").then(setNotices).catch(() => setNotices([]));
  }, []);

  return (
    <main className="min-h-dvh bg-gray-50">
      <Nav />
      <div className="mx-auto max-w-3xl space-y-8 p-6">
        <section className="space-y-3">
          <h1 className="text-lg font-semibold">Minhas turmas</h1>
          <ul className="divide-y rounded border bg-white">
            {plans.map((p) => (
              <li key={p.classroomId} className="space-y-1 p-3 text-sm">
                <div className="flex items-center justify-between">
                  <span className="font-medium">{p.name}</span>
                  <span className="text-xs text-gray-500">
                    {p.deliveredCount}/{p.totalCount} aulas liberadas
                  </span>
                </div>
                <div className="h-2 rounded bg-gray-100">
                  <div
                    className="h-2 rounded bg-indigo-500"
                    style={{ width: `${p.totalCount ? (p.deliveredCount / p.totalCount) * 100 : 0}%` }}
                  />
                </div>
              </li>
            ))}
            {plans.length === 0 && <li className="p-3 text-sm text-gray-500">Você não está em nenhuma turma.</li>}
          </ul>
        </section>

        <section className="space-y-3">
          <h2 className="text-lg font-semibold">Meu progresso</h2>
          <ul className="divide-y rounded border bg-white">
            {progress.map((p) => (
              <li key={p.classId} className="flex items-center justify-between p-3 text-sm">
                <span>
                  {p.className} <span className="text-xs text-gray-500">· {p.courseName}</span>
                </span>
                <span className="text-xs font-medium text-green-600">{p.percentual}%</span>
              </li>
            ))}
            {progress.length === 0 && <li className="p-3 text-sm text-gray-500">Nenhum acesso registrado.</li>}
          </ul>
        </section>

        <section className="space-y-3">
          <h2 className="text-lg font-semibold">Avisos</h2>
          <ul className="divide-y rounded border bg-white">
            {notices.map((n) => (
              <li key={n.id} className="flex items-center justify-between p-3 text-sm">
                <div dangerouslySetInnerHTML={{ __html: n.text }} className="[&_a]:text-indigo-600" />
                <span className={`text-xs ${n.seen ? "text-gray-400" : "font-medium text-indigo-600"}`}>
                  {n.seen ? "lido" : "novo"}
                </span>
              </li>
            ))}
            {notices.length === 0 && <li className="p-3 text-sm text-gray-500">Sem avisos.</li>}
          </ul>
        </section>
      </div>
    </main>
  );
}
