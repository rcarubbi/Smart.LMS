"use client";

import { Fragment, useCallback, useEffect, useState } from "react";
import { api, apiList, errorMessage } from "@/lib/api";
import { usePagedList } from "@/lib/usePagedList";
import AppShell from "@/components/AppShell";
import AuthGuard from "@/components/AuthGuard";
import Panel from "@/components/ui/Panel";
import Icon from "@/components/ui/Icon";
import Pagination from "@/components/ui/Pagination";
import { useToast } from "@/components/ui/Toast";

type Classroom = { id: string; name: string };
type Student = { id: string; name: string; email: string; active: boolean };

type PlanListItem = {
  id: string;
  classroomId: string;
  classroomName: string;
  startDate: string;
  studentCount: number;
  concluded: boolean;
  deliveredClassCount: number;
};

type PlanDetail = {
  id: string;
  classroomName: string;
  startDate: string;
  studentCount: number;
  concluded: boolean;
  students: { id: string; name: string; email: string; active: boolean }[];
  classes: {
    classId: string;
    courseName: string;
    courseOrder: number;
    className: string;
    classOrder: number;
    deliveryDays: number;
    deliveryDate: string;
    delivered: boolean;
  }[];
};

export default function DeliveryPlansAdmin() {
  const { toast } = useToast();
  const [classrooms, setClassrooms] = useState<Classroom[]>([]);
  const [students, setStudents] = useState<Student[]>([]);
  const { items: plans, total, page, setPage, reload } = usePagedList<PlanListItem>("/api/delivery-plans", {});
  const [details, setDetails] = useState<Record<string, PlanDetail>>({});
  const [expandedId, setExpandedId] = useState<string | null>(null);
  const [classroomId, setClassroomId] = useState("");
  const [startDate, setStartDate] = useState("");
  const [selected, setSelected] = useState<string[]>([]);
  const [planStudents, setPlanStudents] = useState<Record<string, string[]>>({});
  const [saving, setSaving] = useState(false);

  const load = useCallback(async () => {
    const [cl, st] = await Promise.all([
      apiList<Classroom>("/api/classrooms"),
      apiList<Student>("/api/users?role=Student&active=true"),
    ]);
    setClassrooms(cl);
    setStudents(st);
    if (cl.length > 0) setClassroomId((cur) => cur || cl[0].id);
  }, []);

  useEffect(() => {
    // This effect fetches remote options; state updates happen after the async response.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    load().catch((e) => toast("error", errorMessage(e)));
  }, [load, toast]);

  async function toggleDetail(id: string) {
    if (expandedId === id) {
      setExpandedId(null);
      return;
    }
    setExpandedId(id);
    if (!details[id]) {
      try {
        const detail = await api<PlanDetail>(`/api/delivery-plans/${id}`);
        setDetails((cur) => ({ ...cur, [id]: detail }));
        setPlanStudents((cur) => ({ ...cur, [id]: detail.students.map((s) => s.id) }));
      } catch (e) {
        toast("error", errorMessage(e));
      }
    }
  }

  async function savePlanStudents(id: string) {
    try {
      const studentIds = planStudents[id] ?? [];
      await api(`/api/delivery-plans/${id}/students`, {
        method: "PUT",
        body: JSON.stringify({ studentIds }),
      });
      const detail = await api<PlanDetail>(`/api/delivery-plans/${id}`);
      setDetails((current) => ({ ...current, [id]: detail }));
      setPlanStudents((current) => ({ ...current, [id]: detail.students.map((s) => s.id) }));
      reload();
      toast("success", "Inscrições atualizadas; aulas já liberadas continuam registradas.");
    } catch (e) {
      toast("error", errorMessage(e));
    }
  }

  function toggleStudent(id: string) {
    setSelected((cur) =>
      cur.includes(id) ? cur.filter((x) => x !== id) : [...cur, id],
    );
  }

  function selectAll() {
    setSelected(students.map((s) => s.id));
  }

  function clearAll() {
    setSelected([]);
  }

  async function submit() {
    if (!classroomId || !startDate) {
      toast("error", "Informe a turma e a data inicial.");
      return;
    }
    if (selected.length === 0) {
      toast("error", "Selecione ao menos um aluno.");
      return;
    }
    setSaving(true);
    try {
      await api("/api/delivery-plans", {
        method: "POST",
        body: JSON.stringify({ classroomId, startDate, studentIds: selected }),
      });
      toast("success", "Plano de entrega criado. As aulas serão liberadas automaticamente conforme a data.");
      setSelected([]);
      setStartDate("");
      reload();
    } catch (e) {
      toast("error", errorMessage(e));
    } finally {
      setSaving(false);
    }
  }

  return (
    <AuthGuard roles={["Admin"]}>
      <AppShell>
        <Panel
          icon="event_available"
          title="Plano de entrega"
          accent="#10b981"
          action={
            <button className="btn btn-primary text-sm" onClick={submit} disabled={saving}>
              <Icon name="rocket_launch" /> {saving ? "Criando..." : "Criar plano"}
            </button>
          }
        >
          <div className="grid max-w-3xl gap-4 md:grid-cols-2">
            <label className="block space-y-1">
              <span className="text-sm text-white/60">Turma</span>
              <select className="field" value={classroomId} onChange={(e) => setClassroomId(e.target.value)}>
                <option value="">Selecione...</option>
                {classrooms.map((c) => (
                  <option key={c.id} value={c.id} className="bg-gray-900">
                    {c.name}
                  </option>
                ))}
              </select>
            </label>
            <label className="block space-y-1">
              <span className="text-sm text-white/60">Data de início</span>
              <input type="date" className="field" value={startDate} onChange={(e) => setStartDate(e.target.value)} />
            </label>
          </div>

          <div className="mt-6">
            <div className="mb-2 flex items-center justify-between">
              <span className="text-sm text-white/60">Alunos ({selected.length} selecionados)</span>
              <div className="flex gap-2">
                <button className="btn btn-ghost text-xs" onClick={selectAll}>Selecionar todos</button>
                <button className="btn btn-ghost text-xs" onClick={clearAll}>Limpar</button>
              </div>
            </div>
            {students.length === 0 ? (
              <p className="text-sm text-white/50">Nenhum aluno cadastrado.</p>
            ) : (
              <ul className="max-h-80 space-y-1 overflow-y-auto rounded-lg border border-white/10 bg-white/5 p-2">
                {students.map((s) => {
                  const checked = selected.includes(s.id);
                  return (
                    <li key={s.id}>
                      <label className="flex cursor-pointer items-center gap-2 rounded px-2 py-1.5 hover:bg-white/5">
                        <input
                          type="checkbox"
                          checked={checked}
                          onChange={() => toggleStudent(s.id)}
                          className="accent-indigo-500"
                        />
                        <span className="text-sm">{s.name}</span>
                        <span className="ml-auto text-xs text-white/40">{s.email}</span>
                      </label>
                    </li>
                  );
                })}
              </ul>
            )}
          </div>
        </Panel>

        <div className="mt-6">
          <Panel icon="view_list" title="Planos criados" accent="#10b981">            {plans.length === 0 ? (
              <p className="text-sm text-white/50">Nenhum plano de entrega criado ainda.</p>
            ) : (
              <table className="w-full text-left text-sm">
                <thead>
                  <tr className="border-b border-white/10 text-xs uppercase tracking-wide text-white/40">
                    <th className="py-2 pr-2 font-medium">Turma</th>
                    <th className="py-2 pr-2 font-medium">Início</th>
                    <th className="py-2 pr-2 font-medium">Alunos</th>
                    <th className="py-2 pr-2 font-medium">Entregas</th>
                    <th className="py-2 pr-2 font-medium">Status</th>
                    <th className="py-2 text-right font-medium">Cronograma</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-white/5">
                  {plans.map((p) => (
                    <Fragment key={p.id}>
                      <tr className="hover:bg-white/5">
                        <td className="py-2.5 pr-2 font-medium">{p.classroomName}</td>
                        <td className="py-2.5 pr-2 text-white/70">{p.startDate}</td>
                        <td className="py-2.5 pr-2 text-white/60">{p.studentCount}</td>
                        <td className="py-2.5 pr-2 text-white/60">
                          {p.deliveredClassCount}
                          <span className="text-white/30"> / {details[p.id]?.classes.length ?? "—"}</span>
                        </td>
                        <td className="py-2.5 pr-2">
                          {p.concluded ? (
                            <span className="rounded bg-emerald-500/20 px-1.5 py-0.5 text-xs text-emerald-300">
                              Concluído
                            </span>
                          ) : (
                            <span className="rounded bg-sky-500/20 px-1.5 py-0.5 text-xs text-sky-300">
                              Em andamento
                            </span>
                          )}
                        </td>
                        <td className="py-2.5 text-right">
                          <button
                            className="btn btn-ghost text-xs"
                            onClick={() => toggleDetail(p.id)}
                          >
                            {expandedId === p.id ? "Recolher" : "Ver"}
                          </button>
                        </td>
                      </tr>
                      {expandedId === p.id && details[p.id] && (
                        <tr>
                          <td colSpan={6} className="bg-white/5 px-4 py-4">
                            <div className="mb-2 flex items-center justify-between">
                              <span className="text-xs uppercase tracking-wide text-white/40">
                                Escala de liberação das aulas
                              </span>
                              <span className="text-xs text-white/40">
                                Início: {details[p.id].startDate}
                              </span>
                            </div>
                            <section className="mb-4 rounded-lg border border-white/10 bg-black/10 p-3">
                              <div className="mb-2 flex items-center justify-between">
                                <span className="text-sm font-medium">Alunos inscritos ({details[p.id].students.length})</span>
                                <button className="btn btn-primary text-xs" onClick={() => savePlanStudents(p.id)}>
                                  Salvar inscrições
                                </button>
                              </div>
                              <div className="flex flex-wrap gap-x-4 gap-y-2">
                                {[...students, ...details[p.id].students.filter((student) => !student.active)].map((student) => {
                                  const checked = (planStudents[p.id] ?? []).includes(student.id);
                                  return (
                                    <label key={student.id} className="flex items-center gap-2 text-xs text-white/70">
                                      <input
                                        type="checkbox"
                                        checked={checked}
                                        onChange={() => setPlanStudents((current) => {
                                          const selectedIds = current[p.id] ?? [];
                                          return { ...current, [p.id]: checked
                                            ? selectedIds.filter((id) => id !== student.id)
                                            : [...selectedIds, student.id] };
                                        })}
                                        className="accent-indigo-500"
                                      />
                                      {student.name}{!student.active && " (inativo)"}
                                    </label>
                                  );
                                })}
                              </div>
                            </section>
                            <div className="grid gap-4 lg:grid-cols-2">
                              <table className="w-full text-left text-xs">
                                <thead>
                                  <tr className="border-b border-white/10 text-white/40">
                                    <th className="py-0.5 pr-2 font-medium">Nº</th>
                                    <th className="py-0.5 pr-2 font-medium">Curso</th>
                                    <th className="py-0.5 pr-2 font-medium">Aula</th>
                                    <th className="py-0.5 pr-2 font-medium">Dias p/ entrega</th>
                                    <th className="py-0.5 pr-2 font-medium">Liberação</th>
                                    <th className="py-0.5 text-right font-medium">Situação</th>
                                  </tr>
                                </thead>
                                <tbody className="divide-y divide-white/5">
                                  {details[p.id].classes.map((c) => (
                                    <tr key={c.classId}>
                                      <td className="py-0.5 pr-2 text-white/50">{c.classOrder}</td>
                                      <td className="py-0.5 pr-2">{c.courseName}</td>
                                      <td className="py-0.5 pr-2">{c.className}</td>
                                      <td className="py-0.5 pr-2 text-white/60">{c.deliveryDays}</td>
                                      <td className="py-0.5 pr-2 text-white/70">{c.deliveryDate}</td>
                                      <td className="py-0.5 text-right">
                                        {c.delivered ? (
                                          <span className="rounded bg-emerald-500/20 px-1.5 py-0.5 text-emerald-300">
                                            Entregue
                                          </span>
                                        ) : (
                                          <span className="rounded bg-white/10 px-1.5 py-0.5 text-white/50">
                                            Pendente
                                          </span>
                                        )}
                                      </td>
                                    </tr>
                                  ))}
                                </tbody>
                              </table>
                              <PlanCalendar
                                startDate={details[p.id].startDate}
                                classes={details[p.id].classes}
                              />
                            </div>
                          </td>
                        </tr>
                      )}
                    </Fragment>
                  ))}
                </tbody>
              </table>
            )}
            <Pagination page={page} total={total} pageSize={25} onChange={setPage} />
          </Panel>
        </div>
      </AppShell>
    </AuthGuard>
  );
}

type ScheduleClass = {
  classId: string;
  courseName: string;
  courseOrder: number;
  className: string;
  classOrder: number;
  deliveryDays: number;
  deliveryDate: string;
  delivered: boolean;
};

function parseBR(s: string): Date | null {
  const m = /^(\d{2})\/(\d{2})\/(\d{4})$/.exec(s);
  return m ? new Date(Number(m[3]), Number(m[2]) - 1, Number(m[1])) : null;
}

const CAL_DIAS = ["Dom", "Seg", "Ter", "Qua", "Qui", "Sex", "Sáb"];
const CAL_MESES = [
  "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho",
  "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro",
];

function PlanCalendar({ startDate, classes }: { startDate: string; classes: ScheduleClass[] }) {
  const start = parseBR(startDate);
  const [view, setView] = useState<Date>(() => start ?? new Date());

  const byDate = new Map<string, ScheduleClass[]>();
  for (const c of classes) {
    const d = parseBR(c.deliveryDate);
    if (!d) continue;
    const key = `${d.getFullYear()}-${d.getMonth()}-${d.getDate()}`;
    const arr = byDate.get(key) ?? [];
    arr.push(c);
    byDate.set(key, arr);
  }

  const year = view.getFullYear();
  const month = view.getMonth();
  const first = new Date(year, month, 1);
  const startPad = first.getDay();
  const total = new Date(year, month + 1, 0).getDate();
  const cells: (number | null)[] = [];
  for (let i = 0; i < startPad; i++) cells.push(null);
  for (let d = 1; d <= total; d++) cells.push(d);

  const today = new Date();
  const todayKey = `${today.getFullYear()}-${today.getMonth()}-${today.getDate()}`;

  return (
    <div>
      <div className="mb-2 flex items-center justify-between">
        <span className="text-xs uppercase tracking-wide text-white/40">Calendário</span>
        <div className="flex items-center gap-1">
          <button
            type="button"
            className="rounded p-0.5 hover:bg-white/10"
            onClick={() => setView((v) => new Date(v.getFullYear(), v.getMonth() - 1, 1))}
            title="Mês anterior"
          >
            <Icon name="chevron_left" />
          </button>
          <span className="text-xs font-semibold">
            {CAL_MESES[month]} {year}
          </span>
          <button
            type="button"
            className="rounded p-0.5 hover:bg-white/10"
            onClick={() => setView((v) => new Date(v.getFullYear(), v.getMonth() + 1, 1))}
            title="Próximo mês"
          >
            <Icon name="chevron_right" />
          </button>
        </div>
      </div>

      <div className="grid grid-cols-7 gap-1 text-center text-[10px] text-white/40">
        {CAL_DIAS.map((d) => (
          <div key={d} className="py-0.5">{d}</div>
        ))}
      </div>
      <div className="mt-1 grid grid-cols-7 gap-1">
        {cells.map((day, i) => {
          if (!day) return <div key={`e${i}`} className="aspect-square" />;
          const key = `${year}-${month}-${day}`;
          const items = byDate.get(key) ?? [];
          const isToday = key === todayKey;
          return (
            <div
              key={key}
              className={`flex min-h-[52px] flex-col rounded border p-1 ${
                isToday ? "border-sky-500/50" : "border-white/10"
              }`}
            >
              <span className={`text-[10px] ${isToday ? "font-bold text-sky-300" : "text-white/50"}`}>
                {day}
              </span>
              {items.length > 0 && (
                <div className="mt-0.5 space-y-0.5">
                  {items.map((c) => (
                    <div
                      key={c.classId}
                      title={`${c.className} — ${c.delivered ? "Entregue" : "Pendente"}`}
                      className={`truncate rounded px-0.5 py-0.5 text-[9px] leading-tight ${
                        c.delivered
                          ? "bg-emerald-500/20 text-emerald-300"
                          : "bg-amber-500/20 text-amber-300"
                      }`}
                    >
                      {c.className}
                    </div>
                  ))}
                </div>
              )}
            </div>
          );
        })}
      </div>
      <div className="mt-1.5 flex items-center gap-3 text-[10px] text-white/50">
        <span className="inline-flex items-center gap-1">
          <span className="inline-block h-2 w-2 rounded bg-emerald-500/60" /> Entregue
        </span>
        <span className="inline-flex items-center gap-1">
          <span className="inline-block h-2 w-2 rounded bg-amber-500/60" /> Pendente
        </span>
      </div>
    </div>
  );
}
