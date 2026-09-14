"use client";

import { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { api, apiList, errorMessage } from "@/lib/api";
import type { ListItem } from "@/lib/api";
import { usePagedList } from "@/lib/usePagedList";
import { useTrashConfirmations } from "@/lib/useTrashConfirmations";
import AppShell from "@/components/AppShell";
import AuthGuard from "@/components/AuthGuard";
import Panel from "@/components/ui/Panel";
import FormModal from "@/components/ui/FormModal";
import Icon from "@/components/ui/Icon";
import IconButton from "@/components/ui/IconButton";
import Pagination from "@/components/ui/Pagination";
import ViewTabs, { type ViewFilter } from "@/components/ui/ViewTabs";
import { useToast } from "@/components/ui/Toast";

type Course = {
  id: string;
  name: string;
  order: number;
  subjectId: string;
  image?: string | null;
  teacherInChargeName?: string | null;
  classCount: number;
  active: boolean;
};
type Teacher = { id: string; name: string };

export default function CoursesAdmin() {
  const { toast } = useToast();
  const [subjects, setSubjects] = useState<ListItem[]>([]);
  const [teachers, setTeachers] = useState<Teacher[]>([]);
  const [subjectFilter, setSubjectFilter] = useState<string>("");
  const [view, setView] = useState<ViewFilter>("active");
  const { items: courses, total, loading, page, setPage, reload } = usePagedList<Course>(
    "/api/courses",
    {
      subjectId: subjectFilter,
      active: view === "all" ? undefined : view === "active",
    },
  );

  const [modal, setModal] = useState<{ item?: Course; name: string; order: string; subjectId: string; teacherId: string; image: string | null } | null>(null);
  const [uploading, setUploading] = useState(false);
  const { setDeleting, setPurging, restore, modals } = useTrashConfirmations<Course>({
    reload,
    basePath: "/api/courses",
    labels: {
      trash: "Curso movido para a lixeira.",
      restore: "Curso restaurado.",
      purge: "Curso excluído definitivamente.",
    },
    messages: {
      trash: (i) => `Mover "${i.name}" para a lixeira? Você poderá restaurá-lo depois.`,
      purge: (i) =>
        `Excluir "${i.name}" definitivamente? Essa ação não pode ser desfeita e remove as aulas vinculadas.`,
    },
  });

  const loadOptions = useCallback(async () => {
    const [subs, t] = await Promise.all([
      apiList<ListItem>("/api/subjects?active=true"),
      apiList<Teacher>("/api/users?role=Teacher"),
    ]);
    setSubjects(subs);
    setTeachers(t);
  }, []);

  useEffect(() => {
    loadOptions().catch((e) => toast("error", errorMessage(e)));
  }, [loadOptions, toast]);

  function openNew() {
    setModal({ name: "", order: "0", subjectId: subjectFilter || "", teacherId: "", image: null });
  }

  function openEdit(item: Course) {
    setModal({
      item,
      name: item.name,
      order: String(item.order),
      subjectId: item.subjectId,
      teacherId: "",
      image: item.image ?? null,
    });
    api<{ teacherInChargeId: string | null }>(`/api/courses/${item.id}`)
      .then((detail) =>
        setModal((m) =>
          m && m.item?.id === item.id ? { ...m, teacherId: detail.teacherInChargeId ?? "" } : m,
        ),
      )
      .catch(() => {});
  }

  async function uploadImage(file: File) {
    const fd = new FormData();
    fd.append("file", file);
    setUploading(true);
    try {
      const { url } = await api<{ url: string }>("/api/uploads/course-images", { method: "POST", body: fd });
      setModal((m) => (m ? { ...m, image: url } : m));
      toast("success", "Imagem enviada.");
    } catch (e) {
      toast("error", errorMessage(e));
    } finally {
      setUploading(false);
    }
  }

  async function submit() {
    if (!modal) return;
    const payload = {
      name: modal.name,
      order: Number(modal.order),
      subjectId: modal.subjectId,
      teacherInChargeId: modal.teacherId || null,
      image: modal.image || null,
    };
    try {
      if (modal.item) {
        await api(`/api/courses/${modal.item.id}`, { method: "PUT", body: JSON.stringify(payload) });
        toast("success", "Curso atualizado.");
      } else {
        await api("/api/courses", { method: "POST", body: JSON.stringify(payload) });
        toast("success", "Curso criado.");
      }
      setModal(null);
      reload();
    } catch (e) {
      toast("error", errorMessage(e));
    }
  }

  return (
    <AuthGuard roles={["Admin"]}>
      <AppShell>
        <Panel
          icon="school"
          title="Cursos"
          accent="#3b82f6"
          action={
            <button className="btn btn-primary text-sm" onClick={openNew}>
              <Icon name="add" /> Novo curso
            </button>
          }
        >
          <label className="mb-2 inline-flex items-center gap-2 text-sm">
            <span className="text-white/60">Filtrar por assunto:</span>
            <select className="field w-auto" value={subjectFilter} onChange={(e) => setSubjectFilter(e.target.value)}>
              <option value="">Todos</option>
              {subjects.map((s) => (
                <option key={s.id} value={s.id} className="bg-gray-900">
                  {s.name}
                </option>
              ))}
            </select>
          </label>

          <ViewTabs view={view} onChange={setView} total={total} />

          {loading ? (
            <p className="text-sm text-white/50">Carregando...</p>
          ) : courses.length === 0 ? (
            <p className="text-sm text-white/50">
              {view === "trash" ? "A lixeira está vazia." : "Nenhum curso cadastrado."}
            </p>
          ) : (
            <table className="w-full text-left text-sm">
              <thead>
                <tr className="border-b border-white/10 text-xs uppercase tracking-wide text-white/40">
                  <th className="py-2 pr-2 font-medium">Ordem</th>
                  <th className="py-2 pr-2 font-medium">Curso</th>
                  <th className="py-2 pr-2 font-medium">Responsável</th>
                  <th className="py-2 pr-2 font-medium">Aulas</th>
                  <th className="py-2 text-right font-medium">Ações</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-white/5">
                {courses.map((c) => (
                  <tr key={c.id} className="hover:bg-white/5">
                    <td className="py-2.5 pr-2 text-white/60">{c.order}</td>
                    <td className="py-2.5 pr-2">
                      <div className="flex items-center gap-3">
                        {c.image ? (
                          <img src={c.image} alt="" className="h-9 w-14 rounded object-cover" loading="lazy" />
                        ) : (
                          <span className="flex h-9 w-14 items-center justify-center rounded bg-white/10 text-white/40">
                            <Icon name="image" className="text-base" />
                          </span>
                        )}
                        <div>
                          <div className="font-medium">{c.name}</div>
                          <div className="text-xs text-white/40">
                            {subjects.find((s) => s.id === c.subjectId)?.name ?? "—"}
                            {!c.active && (
                              <span className="ml-2 rounded bg-amber-500/20 px-1.5 py-0.5 text-amber-300">
                                Na lixeira
                              </span>
                            )}
                          </div>
                        </div>
                      </div>
                    </td>
                    <td className="py-2.5 pr-2 text-white/70">{c.teacherInChargeName ?? "—"}</td>
                    <td className="py-2.5 pr-2 text-white/60">{c.classCount}</td>
                    <td className="py-2.5 text-right">
                      {c.active ? (
                        <>
                          <Link
                            href={`/admin/classes?course=${c.id}`}
                            className="mr-1 inline-flex items-center gap-1 rounded p-1.5 text-xs text-sky-400 hover:bg-white/10"
                            title="Gerenciar aulas"
                          >
                            <Icon name="play_circle" className="text-base" /> Aulas
                          </Link>
                          <Link
                            href={`/admin/assessments?course=${c.id}`}
                            className="mr-1 inline-flex items-center gap-1 rounded p-1.5 text-xs text-pink-400 hover:bg-white/10"
                            title="Gerenciar avaliações"
                          >
                            <Icon name="assignment" className="text-base" /> Avaliações
                          </Link>
                          <IconButton name="edit" title="Editar" onClick={() => openEdit(c)} />
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
          <Pagination page={page} total={total} pageSize={25} onChange={setPage} />
        </Panel>

        <FormModal
          open={!!modal}
          title={modal?.item ? "Editar curso" : "Novo curso"}
          onClose={() => setModal(null)}
          onSubmit={submit}
        >
          <label className="block space-y-1">
            <span className="text-sm text-white/60">Nome</span>
            <input
              value={modal?.name ?? ""}
              onChange={(e) => setModal((m) => (m ? { ...m, name: e.target.value } : m))}
              className="field"
              autoFocus
            />
          </label>
          <div className="grid grid-cols-2 gap-3">
            <label className="block space-y-1">
              <span className="text-sm text-white/60">Ordem</span>
              <input
                type="number"
                value={modal?.order ?? ""}
                onChange={(e) => setModal((m) => (m ? { ...m, order: e.target.value } : m))}
                className="field"
              />
            </label>
            <label className="block space-y-1">
              <span className="text-sm text-white/60">Assunto</span>
              <select
                className="field"
                value={modal?.subjectId ?? ""}
                onChange={(e) => setModal((m) => (m ? { ...m, subjectId: e.target.value } : m))}
              >
                <option value="">Selecione...</option>
                {subjects.map((s) => (
                  <option key={s.id} value={s.id} className="bg-gray-900">
                    {s.name}
                  </option>
                ))}
              </select>
            </label>
          </div>
          <label className="block space-y-1">
            <span className="text-sm text-white/60">Professor responsável (opcional)</span>
            <select
              className="field"
              value={modal?.teacherId ?? ""}
              onChange={(e) => setModal((m) => (m ? { ...m, teacherId: e.target.value } : m))}
            >
              <option value="">Nenhum</option>
              {teachers.map((t) => (
                <option key={t.id} value={t.id} className="bg-gray-900">
                  {t.name}
                </option>
              ))}
            </select>
          </label>
          <label className="block space-y-1">
            <span className="text-sm text-white/60">Imagem do curso</span>
            <div
              className="flex items-center justify-center gap-2 rounded-lg border-2 border-dashed border-white/20 bg-white/5 px-3 py-6 text-sm text-white/50 transition hover:border-white/40 hover:bg-white/10"
              onDragOver={(e) => e.preventDefault()}
              onDrop={(e) => {
                e.preventDefault();
                const f = e.dataTransfer.files?.[0];
                if (f) uploadImage(f);
              }}
            >
              {modal?.image ? (
                <>
                  <img src={modal.image} alt="" className="h-16 w-28 rounded object-cover" />
                  <span className="ml-2">Arraste para substituir ou clique para escolher</span>
                </>
              ) : (
                <>
                  <Icon name="cloud_upload" className="text-2xl" />
                  {uploading ? "Enviando..." : "Arraste a imagem aqui ou clique para escolher"}
                </>
              )}
              <input
                type="file"
                accept="image/*"
                className="hidden"
                onChange={(e) => {
                  const f = e.target.files?.[0];
                  if (f) uploadImage(f);
                  e.target.value = "";
                }}
              />
            </div>
            {modal?.image ? (
              <button
                type="button"
                className="text-xs text-red-300 hover:text-red-200"
                onClick={(e) => {
                  e.preventDefault();
                  e.stopPropagation();
                  setModal((m) => (m ? { ...m, image: null } : m));
                }}
              >
                Remover imagem
              </button>
            ) : null}
          </label>
        </FormModal>

        {modals}
      </AppShell>
    </AuthGuard>
  );
}
