"use client";

import { useState } from "react";
import Link from "next/link";
import { usePagedList } from "@/lib/usePagedList";
import AppShell from "@/components/AppShell";
import AuthGuard from "@/components/AuthGuard";
import Panel from "@/components/ui/Panel";
import Pagination from "@/components/ui/Pagination";

type TeachingClass = { id: string; name: string; order: number; active: boolean };
type TeachingCourse = {
  id: string;
  name: string;
  order: number;
  teacherInChargeId: string | null;
  classes: TeachingClass[];
};
type StudentProgress = {
  studentId: string;
  studentName: string;
  studentEmail: string;
  classId: string;
  className: string;
  courseName: string;
  percentual: number;
  watchedSeconds: number;
  lastAccessAtUtc: string;
};
type TeachingStudent = { id: string; name: string; email: string; classroomName: string };
type TeachingDelivery = {
  classId: string;
  className: string;
  courseName: string;
  classroomName: string;
  releasedAtUtc: string;
  studentCount: number;
};

export default function TeachingPage() {
  const [courseId, setCourseId] = useState("");
  const [classId, setClassId] = useState("");
  const { items: courses } = usePagedList<TeachingCourse>("/api/me/teaching/courses", {}, 100);
  const selectedCourse = courses.find((course) => course.id === courseId) ?? courses[0];
  const classes = selectedCourse?.classes.filter((item) => item.active) ?? [];
  const selectedClassId = classes.some((item) => item.id === classId) ? classId : "";
  const { items: progress, total, page, setPage, loading } = usePagedList<StudentProgress>(
    "/api/me/teaching/progress",
    { courseId: selectedCourse?.id, classId: selectedClassId || undefined },
  );
  const { items: students, total: studentTotal, page: studentPage, setPage: setStudentPage, loading: studentsLoading } = usePagedList<TeachingStudent>(
    "/api/me/teaching/students",
    { courseId: selectedCourse?.id },
  );
  const { items: deliveries, total: deliveryTotal, page: deliveryPage, setPage: setDeliveryPage } = usePagedList<TeachingDelivery>(
    "/api/me/teaching/delivery-history",
    { courseId: selectedCourse?.id },
  );

  return (
    <AuthGuard roles={["Admin", "Teacher"]}>
      <AppShell>
        <Panel icon="school" title="Área do professor" accent="#14b8a6">
          {courses.length === 0 ? (
            <p className="text-sm text-white/50">Nenhum curso atribuído.</p>
          ) : (
            <div className="grid gap-4 md:grid-cols-2">
              <label className="space-y-1">
                <span className="text-sm text-white/60">Curso</span>
                <select className="field" value={selectedCourse?.id ?? ""} onChange={(e) => { setCourseId(e.target.value); setClassId(""); }}>
                  {courses.map((course) => <option key={course.id} value={course.id} className="bg-gray-900">{course.name}</option>)}
                </select>
              </label>
              <label className="space-y-1">
                <span className="text-sm text-white/60">Aula</span>
                <select className="field" value={selectedClassId} onChange={(e) => setClassId(e.target.value)}>
                  <option value="" className="bg-gray-900">Todas as aulas</option>
                  {classes.map((item) => <option key={item.id} value={item.id} className="bg-gray-900">{item.name}</option>)}
                </select>
              </label>
            </div>
          )}
          {selectedCourse && (
            <div className="mt-4 flex flex-wrap gap-2">
              <Link href={`/course/${selectedCourse.id}`} className="btn btn-ghost text-sm">Abrir curso</Link>
              <Link href={`/admin/classes?course=${selectedCourse.id}`} className="btn btn-ghost text-sm">Gerenciar minhas aulas</Link>
            </div>
          )}
        </Panel>

        {selectedCourse && (
          <div className="mt-6">
            <Panel icon="groups" title="Alunos inscritos" accent="#ec4899">
              {studentsLoading ? <p className="text-sm text-white/50">Carregando...</p> : students.length === 0 ? (
                <p className="text-sm text-white/50">Nenhum aluno inscrito nas turmas deste curso.</p>
              ) : (
                <ul className="grid gap-2 sm:grid-cols-2">{students.map((student) => (
                  <li key={`${student.id}-${student.classroomName}`} className="rounded-lg border border-white/10 bg-white/5 p-3 text-sm">
                    <div className="font-medium">{student.name}</div>
                    <div className="text-xs text-white/50">{student.email} · {student.classroomName}</div>
                  </li>
                ))}</ul>
              )}
              <Pagination page={studentPage} total={studentTotal} pageSize={25} onChange={setStudentPage} />
            </Panel>
          </div>
        )}

        {selectedCourse && (
          <div className="mt-6">
            <Panel icon="event_available" title="Histórico de liberações" accent="#10b981">
              {deliveries.length === 0 ? <p className="text-sm text-white/50">Nenhuma aula liberada.</p> : (
                <ul className="space-y-2">{deliveries.map((delivery) => (
                  <li key={`${delivery.classId}-${delivery.classroomName}`} className="flex flex-wrap justify-between gap-2 rounded-lg border border-white/10 bg-white/5 p-3 text-sm">
                    <span>{delivery.classroomName} · {delivery.className}</span>
                    <span className="text-white/50">{new Date(delivery.releasedAtUtc).toLocaleString("pt-BR")} · {delivery.studentCount} alunos</span>
                  </li>
                ))}</ul>
              )}
              <Pagination page={deliveryPage} total={deliveryTotal} pageSize={25} onChange={setDeliveryPage} />
            </Panel>
          </div>
        )}

        {selectedCourse && (
          <div className="mt-6">
            <Panel icon="insights" title="Progresso recente dos alunos" accent="#6366f1">
              {loading ? <p className="text-sm text-white/50">Carregando...</p> : progress.length === 0 ? (
                <p className="text-sm text-white/50">Nenhum acesso registrado para este filtro.</p>
              ) : (
                <div className="overflow-x-auto">
                  <table className="w-full text-left text-sm">
                    <thead><tr className="border-b border-white/10 text-xs uppercase text-white/40">
                      <th className="py-2 pr-3">Aluno</th><th className="py-2 pr-3">Aula</th><th className="py-2 pr-3">Progresso</th><th className="py-2">Último acesso</th>
                    </tr></thead>
                    <tbody className="divide-y divide-white/5">{progress.map((item) => (
                      <tr key={`${item.studentId}-${item.classId}`}>
                        <td className="py-2 pr-3"><div>{item.studentName}</div><div className="text-xs text-white/40">{item.studentEmail}</div></td>
                        <td className="py-2 pr-3">{item.className}</td>
                        <td className="py-2 pr-3">{item.percentual}%</td>
                        <td className="py-2 text-white/50">{new Date(item.lastAccessAtUtc).toLocaleString("pt-BR")}</td>
                      </tr>
                    ))}</tbody>
                  </table>
                </div>
              )}
              <Pagination page={page} total={total} pageSize={25} onChange={setPage} />
            </Panel>
          </div>
        )}
      </AppShell>
    </AuthGuard>
  );
}
