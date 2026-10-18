"use client";

import { useRef, useState } from "react";
import { api, apiList, errorMessage } from "@/lib/api";
import { usePagedList } from "@/lib/usePagedList";
import { useTrashConfirmations } from "@/lib/useTrashConfirmations";
import AppShell from "@/components/AppShell";
import AuthGuard from "@/components/AuthGuard";
import Panel from "@/components/ui/Panel";
import FormModal from "@/components/ui/FormModal";
import InfoModal from "@/components/ui/InfoModal";
import Icon from "@/components/ui/Icon";
import IconButton from "@/components/ui/IconButton";
import Pagination from "@/components/ui/Pagination";
import ViewTabs, { type ViewFilter } from "@/components/ui/ViewTabs";
import { useToast } from "@/components/ui/Toast";

type Classroom = { id: string; name: string; courseCount: number; planCount: number; courseIds: string[]; active: boolean };
type Course = { id: string; name: string; order: number };

export default function ClassroomsAdmin() {
  const { toast } = useToast();
  const [courses, setCourses] = useState<Course[]>([]);
  const [view, setView] = useState<ViewFilter>("active");
  const { items: classrooms, total, loading, page, setPage, reload } = usePagedList<Classroom>(
    "/api/classrooms",
    { active: view === "all" ? undefined : view === "active" },
  );

  const [nameModal, setNameModal] = useState<{ item?: Classroom; name: string } | null>(null);
  const [enrollModal, setEnrollModal] = useState<{ classroom?: Classroom; selected: string[] } | null>(null);
  const [showTip, setShowTip] = useState(false);
  const dragIndex = useRef<number | null>(null);
  const { setDeleting, setPurging, restore, modals } = useTrashConfirmations<Classroom>({
    reload,
    basePath: "/api/classrooms",
    labels: {
      trash: "Turma movida para a lixeira.",
      restore: "Turma restaurada.",
      purge: "Turma excluída definitivamente.",
    },
    messages: {
      trash: (i) => `Excluir "${i.name}"? Ela ficará na lixeira e pode ser restaurada.`,
      purge: (i) => `Esta ação não pode ser desfeita. Excluir "${i.name}" para sempre?`,
    },
  });

  function openEnroll(c: Classroom) {
    const ids = c.courseIds ?? [];
    setEnrollModal({ classroom: c, selected: ids });
    apiList<Course>("/api/courses")
      .then((cs) => {
        setCourses(cs);
        setEnrollModal({ classroom: c, selected: ids });
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

  function moveCourse(from: number, to: number) {
    if (from === to) return;
    setEnrollModal((m) => {
      if (!m) return m;
      const next = [...m.selected];
      const [moved] = next.splice(from, 1);
      next.splice(to, 0, moved);
      return { ...m, selected: next };
    });
    dragIndex.current = null;
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
      reload();
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
      reload();
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
              <button className="btn btn-ghost mr-2 text-sm" onClick={() => setShowTip(true)}>
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
          ) : (
            <>
              <ViewTabs view={view} onChange={setView} total={total} labels={{ active: "Ativas" }} />
              {classrooms.length === 0 ? (
                <p className="text-sm text-white/50">
                  {view === "trash" ? "A lixeira está vazia." : "Nenhuma turma cadastrada."}
                </p>
              ) : (
                <table className="w-full text-left text-sm">
                  <thead>
                    <tr className="border-b border-white/10 text-xs uppercase tracking-wide text-white/40">
                      <th className="py-2 pr-2 font-medium">Nome</th>
                      <th className="py-2 pr-2 font-medium">Cursos</th>
                      <th className="py-2 pr-2 font-medium">Planos</th>
                      <th className="py-2 pr-2 font-medium">Status</th>
                      <th className="py-2 text-right font-medium">Ações</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-white/5">
                    {classrooms.map((c) => (
                      <tr key={c.id} className="hover:bg-white/5">
                        <td className="py-2.5 pr-2 font-medium">{c.name}</td>
                        <td className="py-2.5 pr-2 text-white/60">{c.courseCount}</td>
                        <td className="py-2.5 pr-2 text-white/60">{c.planCount}</td>
                        <td className="py-2.5 pr-2">
                          {c.active ? (
                            <span className="inline-flex items-center gap-1 rounded-full bg-emerald-500/10 px-2 py-0.5 text-xs text-emerald-400">
                              <Icon name="check_circle" className="text-sm" /> Ativa
                            </span>
                          ) : (
                            <span className="inline-flex items-center gap-1 rounded-full bg-red-500/10 px-2 py-0.5 text-xs text-red-400">
                              <Icon name="delete" className="text-sm" /> Na lixeira
                            </span>
                          )}
                        </td>
                        <td className="py-2.5 text-right">
                          {c.active ? (
                            <>
                              <button
                                className="mr-1 inline-flex items-center gap-1 rounded p-1.5 text-xs text-pink-400 hover:bg-white/10"
                                onClick={() => openEnroll(c)}
                                title="Gerenciar cursos"
                              >
                                <Icon name="library_books" className="text-base" /> Cursos
                              </button>
                              <IconButton name="edit" title="Editar" onClick={() => setNameModal({ item: c, name: c.name })} />
                              <IconButton name="delete" title="Mover para a lixeira" variant="danger" onClick={() => setDeleting(c)} />
                            </>
                          ) : (
                            <>
                              <IconButton name="restore" title="Restaurar" onClick={() => restore(c)} />
                              <IconButton name="delete_forever" title="Excluir definitivamente" variant="danger" onClick={() => setPurging(c)} />
                            </>
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </>
          )}
          <Pagination page={page} total={total} pageSize={25} onChange={setPage} />
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
          <p className="text-xs text-white/50">
            Selecione os cursos desta turma e arraste na lista &quot;Selecionados&quot; para definir a ordem.
          </p>
          {(() => {
            const sel = (enrollModal?.selected ?? [])
              .map((id) => courses.find((x) => x.id === id))
              .filter((x): x is Course => !!x);
            return sel.length === 0 && courses.length > 0 ? (
              <p className="text-xs text-white/40">Nenhum curso selecionado.</p>
            ) : sel.length === 0 ? null : (
              <div>
                <p className="mb-1 text-xs uppercase tracking-wide text-white/40">Selecionados</p>
                <ul className="space-y-1">
                  {sel.map((c, i) => (
                    <li
                      key={c.id}
                      draggable
                      onDragStart={() => (dragIndex.current = i)}
                      onDragOver={(e) => e.preventDefault()}
                      onDrop={() => moveCourse(dragIndex.current ?? i, i)}
                      onDragEnd={() => (dragIndex.current = null)}
                      className="flex cursor-grab items-center gap-2 rounded border border-white/10 bg-white/5 px-2 py-1.5 text-sm active:cursor-grabbing"
                    >
                      <Icon name="drag_indicator" className="text-white/40" />
                      <span className="truncate">{c.name}</span>
                      <button
                        type="button"
                        onClick={() => toggleCourse(c.id)}
                        className="ml-auto text-white/40 hover:text-white"
                        title="Remover"
                      >
                        <Icon name="close" />
                      </button>
                    </li>
                  ))}
                </ul>
              </div>
            );
          })()}
          <p className="mb-1 text-xs uppercase tracking-wide text-white/40">Todos os cursos</p>
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

        {modals}

        <InfoModal
          open={showTip}
          title="Dica"
          confirmLabel="Entendi"
          message={
            <>
              <p>
                Em <strong>Cursos</strong>, marque os cursos que integram a turma. Na lista{" "}
                <strong>Selecionados</strong>, arraste os itens para definir a ordem em que os cursos
                aparecerão e salve.
              </p>
              <p className="mt-2">
                Crie um <strong>plano de entrega</strong> para liberar as aulas automaticamente aos
                alunos da turma.
              </p>
            </>
          }
          onConfirm={() => setShowTip(false)}
          onClose={() => setShowTip(false)}
        />
      </AppShell>
    </AuthGuard>
  );
}
