"use client";

import { useCallback, useEffect, useState } from "react";
import { api, errorMessage } from "@/lib/api";
import AppShell from "@/components/AppShell";
import AuthGuard from "@/components/AuthGuard";
import Panel from "@/components/ui/Panel";
import Icon from "@/components/ui/Icon";
import { useToast } from "@/components/ui/Toast";

type Classroom = { id: string; name: string };
type Student = { id: string; name: string; email: string; active: boolean };

export default function DeliveryPlansAdmin() {
  const { toast } = useToast();
  const [classrooms, setClassrooms] = useState<Classroom[]>([]);
  const [students, setStudents] = useState<Student[]>([]);
  const [classroomId, setClassroomId] = useState("");
  const [startDate, setStartDate] = useState("");
  const [selected, setSelected] = useState<string[]>([]);
  const [saving, setSaving] = useState(false);

  const load = useCallback(async () => {
    const [cl, st] = await Promise.all([
      api<Classroom[]>("/api/classrooms"),
      api<Student[]>("/api/users?role=Student"),
    ]);
    setClassrooms(cl);
    setStudents(st);
    if (cl.length > 0) setClassroomId((cur) => cur || cl[0].id);
  }, []);

  useEffect(() => {
    load().catch((e) => toast("error", errorMessage(e)));
  }, [load, toast]);

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
      </AppShell>
    </AuthGuard>
  );
}
