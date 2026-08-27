"use client";

import { useCallback, useEffect, useState } from "react";
import { api, errorMessage } from "@/lib/api";
import AppShell from "@/components/AppShell";
import AuthGuard from "@/components/AuthGuard";
import Panel from "@/components/ui/Panel";
import FormModal from "@/components/ui/FormModal";
import ConfirmModal from "@/components/ui/ConfirmModal";
import Icon from "@/components/ui/Icon";
import IconButton from "@/components/ui/IconButton";
import { useToast } from "@/components/ui/Toast";

type Classroom = { id: string; name: string; courseCount: number; planCount: number };
type Course = { id: string; name: string; order: number };

export default function ClassroomsAdmin() {
  const { toast } = useToast();
  const [classrooms, setClassrooms] = useState<Classroom[]>([]);
  const [courses, setCourses] = useState<Course[]>([]);
  const [loading, setLoading] = useState(true);

  const [nameModal, setNameModal] = useState<{ item?: Classroom; name: string } | null>(null);
  const [enrollModal, setEnrollModal] = useState<{ classroom?: Classroom; selected: string[] } | null>(null);
  const [deleting, setDeleting] = useState<Classroom | null>(null);

  const load = useCallback(async () => {
    setClassrooms(await api<Classroom[]>("/api/classrooms"));
  }, []);

  useEffect(() => {
    load()
      .catch((e) => toast("error", errorMessage(e)))
      .finally(() => setLoading(false));
  }, [load, toast]);

  function openEnroll(c: Classroom) {
    setEnrollModal({ classroom: c, selected: [] });
    api<Course[]>("/api/courses")
      .then((cs) => {
        setCourses(cs);
        setEnrollModal({ classroom: c, selected: [] });
      })
      .catch((e) => toast("error", errorMessage(e)));
  }

  function toggleCourse(id: string) {
    setEnrollModal((m) =>
      m
        ? {
            ...m,
            selected: m.selected.includes(id)
              ? m.selected.filter((x) => x !== id)
              : [...m.selected, id],
          }
        : m,
    );
  }

  async function submitName() {
    if (!nameModal) return;
    try {
      if (nameModal.item) {
        await api(`/api/classrooms/${nameModal.item.id}`, {
          method: "PUT",
          body: JSON.stringify({ name: nameModal.name }),
        });
        toast("success", "Turma atualizada.");
      } else {
        await api("/api/classrooms", { method: "POST", body: JSON.stringify({ name: nameModal.name }) });
        toast("success", "Turma criada.");
      }
      setNameModal(null);
      load();
    } catch (e) {
      toast("error", errorMessage(e));
    }
  }

  async function submitEnroll() {
    if (!enrollModal?.classroom) return;
    try {
      await api(`/api/classrooms/${enrollModal.classroom.id}/courses`, {
        method: "PUT",
        body: JSON.stringify({ courseIds: enrollModal.selected }),
      });
      toast("success", "Cursos da turma atualizados.");
      setEnrollModal(null);
      load();
    } catch (e) {
      toast("error", errorMessage(e));
    }
  }

  async function confirmDelete() {
    if (!deleting) return;
    try {
      await api(`/api/classrooms/${deleting.id}`, { method: "DELETE" });
      toast("success", "Turma excluída.");
      setDeleting(null);
      load();
    } catch (e) {
      toast("error", errorMessage(e));
    }
  }

  return (
    <AuthGuard roles={["Admin"]}>
      <AppShell>
        <Panel
          icon="groups"
          title="Turmas"
          accent="#ec4899"
          action={
            <>
              <button className="btn btn-ghost mr-2 text-sm" onClick={() => toast("info", "Crie um plano de entrega para liberar aulas automaticamente.")}>
                <Icon name="info" /> Dica
              </button>
              <button className="btn btn-primary text-sm" onClick={() => setNameModal({ name: "" })}>
                <Icon name="add" /> Nova turma
              </button>
            </>
          }
        >
          {loading ? (
            <p className="text-sm text-white/50">Carregando...</p>
          ) : classrooms.length === 0 ? (
            <p className="text-sm text-white/50">Nenhuma turma cadastrada.</p>
          ) : (
            <table className="w-full text-left text-sm">
              <thead>
                <tr className="border-b border-white/10 text-xs uppercase tracking-wide text-white/40">
                  <th className="py-2 pr-2 font-medium">Nome</th>
                  <th className="py-2 pr-2 font-medium">Cursos</th>
                  <th className="py-2 pr-2 font-medium">Planos</th>
                  <th className="py-2 text-right font-medium">Ações</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-white/5">
                {classrooms.map((c) => (
                  <tr key={c.id} className="hover:bg-white/5">
                    <td className="py-2.5 pr-2 font-medium">{c.name}</td>
                    <td className="py-2.5 pr-2 text-white/60">{c.courseCount}</td>
                    <td className="py-2.5 pr-2 text-white/60">{c.planCount}</td>
                    <td className="py-2.5 text-right">
                      <button
                        className="mr-1 inline-flex items-center gap-1 rounded p-1.5 text-xs text-pink-400 hover:bg-white/10"
                        onClick={() => openEnroll(c)}
                        title="Gerenciar cursos"
                      >
                        <Icon name="library_books" className="text-base" /> Cursos
                      </button>
                      <IconButton name="edit" title="Editar" onClick={() => setNameModal({ item: c, name: c.name })} />
                      <IconButton name="delete" title="Excluir" danger onClick={() => setDeleting(c)} />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </Panel>

        <FormModal
          open={!!nameModal}
          title={nameModal?.item ? "Editar turma" : "Nova turma"}
          onClose={() => setNameModal(null)}
          onSubmit={submitName}
        >
          <label className="block space-y-1">
            <span className="text-sm text-white/60">Nome</span>
            <input
              value={nameModal?.name ?? ""}
              onChange={(e) => setNameModal((m) => (m ? { ...m, name: e.target.value } : m))}
              className="field"
              autoFocus
            />
          </label>
        </FormModal>

        <FormModal
          open={!!enrollModal}
          title={`Cursos — ${enrollModal?.classroom?.name}`}
          onClose={() => setEnrollModal(null)}
          onSubmit={submitEnroll}
          submitLabel="Salvar cursos"
        >
          <p className="text-xs text-white/50">Selecione os cursos que farão parte desta turma.</p>
          {courses.length === 0 ? (
            <p className="text-sm text-white/50">Nenhum curso disponível.</p>
          ) : (
            <ul className="max-h-72 space-y-1 overflow-y-auto">
              {[...courses]
                .sort((a, b) => a.order - b.order)
                .map((c) => {
                  const checked = enrollModal?.selected.includes(c.id) ?? false;
                  return (
                    <li key={c.id}>
                      <label className="flex cursor-pointer items-center gap-2 rounded px-2 py-1.5 hover:bg-white/5">
                        <input
                          type="checkbox"
                          checked={checked}
                          onChange={() => toggleCourse(c.id)}
                          className="accent-indigo-500"
                        />
                        <span className="text-sm">{c.name}</span>
                        <span className="ml-auto text-xs text-white/40">ordem {c.order}</span>
                      </label>
                    </li>
                  );
                })}
            </ul>
          )}
        </FormModal>

        <ConfirmModal
          open={!!deleting}
          title="Excluir turma"
          message={`Excluir "${deleting?.name}"?`}
          onConfirm={confirmDelete}
          onCancel={() => setDeleting(null)}
        />
      </AppShell>
    </AuthGuard>
  );
}
