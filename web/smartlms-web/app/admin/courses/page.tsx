"use client";

import { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { api, errorMessage } from "@/lib/api";
import type { ListItem } from "@/lib/api";
import AppShell from "@/components/AppShell";
import AuthGuard from "@/components/AuthGuard";
import Panel from "@/components/ui/Panel";
import FormModal from "@/components/ui/FormModal";
import ConfirmModal from "@/components/ui/ConfirmModal";
import Icon from "@/components/ui/Icon";
import IconButton from "@/components/ui/IconButton";
import { useToast } from "@/components/ui/Toast";

type Course = {
  id: string;
  name: string;
  order: number;
  subjectId: string;
  image?: string | null;
  teacherInChargeName?: string | null;
  classCount: number;
};
type Teacher = { id: string; name: string };

export default function CoursesAdmin() {
  const { toast } = useToast();
  const [subjects, setSubjects] = useState<ListItem[]>([]);
  const [teachers, setTeachers] = useState<Teacher[]>([]);
  const [subjectFilter, setSubjectFilter] = useState<string>("");
  const [courses, setCourses] = useState<Course[]>([]);
  const [loading, setLoading] = useState(true);

  const [modal, setModal] = useState<{ item?: Course; name: string; order: string; subjectId: string; teacherId: string; image: string } | null>(null);
  const [deleting, setDeleting] = useState<Course | null>(null);

  const loadOptions = useCallback(async () => {
    const [subs, t] = await Promise.all([
      api<ListItem[]>("/api/subjects"),
      api<Teacher[]>("/api/users?role=Teacher"),
    ]);
    setSubjects(subs);
    setTeachers(t);
  }, []);

  const load = useCallback(async () => {
    const q = subjectFilter ? `?subjectId=${subjectFilter}` : "";
    setCourses(await api<Course[]>(`/api/courses${q}`));
  }, [subjectFilter]);

  useEffect(() => {
    loadOptions();
  }, [loadOptions]);

  useEffect(() => {
    load()
      .catch((e) => toast("error", errorMessage(e)))
      .finally(() => setLoading(false));
  }, [load, toast]);

  function openNew() {
    setModal({ name: "", order: "0", subjectId: subjectFilter || "", teacherId: "", image: "" });
  }

  function openEdit(item: Course) {
    setModal({
      item,
      name: item.name,
      order: String(item.order),
      subjectId: item.subjectId,
      teacherId: "",
      image: item.image ?? "",
    });
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
      load();
    } catch (e) {
      toast("error", errorMessage(e));
    }
  }

  async function confirmDelete() {
    if (!deleting) return;
    try {
      await api(`/api/courses/${deleting.id}`, { method: "DELETE" });
      toast("success", "Curso excluído.");
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
          icon="school"
          title="Cursos"
          accent="#3b82f6"
          action={
            <button className="btn btn-primary text-sm" onClick={openNew}>
              <Icon name="add" /> Novo curso
            </button>
          }
        >
          <label className="mb-4 inline-flex items-center gap-2 text-sm">
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

          {loading ? (
            <p className="text-sm text-white/50">Carregando...</p>
          ) : courses.length === 0 ? (
            <p className="text-sm text-white/50">Nenhum curso cadastrado.</p>
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
                          <div className="text-xs text-white/40">{subjects.find((s) => s.id === c.subjectId)?.name ?? "—"}</div>
                        </div>
                      </div>
                    </td>
                    <td className="py-2.5 pr-2 text-white/70">{c.teacherInChargeName ?? "—"}</td>
                    <td className="py-2.5 pr-2 text-white/60">{c.classCount}</td>
                    <td className="py-2.5 text-right">
                      <Link
                        href={`/admin/classes?course=${c.id}`}
                        className="mr-1 inline-flex items-center gap-1 rounded p-1.5 text-xs text-sky-400 hover:bg-white/10"
                        title="Gerenciar aulas"
                      >
                        <Icon name="play_circle" className="text-base" /> Aulas
                      </Link>
                      <IconButton name="edit" title="Editar" onClick={() => openEdit(c)} />
                      <IconButton name="delete" title="Excluir" danger onClick={() => setDeleting(c)} />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
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
            <span className="text-sm text-white/60">Imagem (URL)</span>
            <input
              value={modal?.image ?? ""}
              onChange={(e) => setModal((m) => (m ? { ...m, image: e.target.value } : m))}
              className="field"
              placeholder="https://..."
            />
          </label>
        </FormModal>

        <ConfirmModal
          open={!!deleting}
          title="Excluir curso"
          message={`Excluir "${deleting?.name}"?`}
          onConfirm={confirmDelete}
          onCancel={() => setDeleting(null)}
        />
      </AppShell>
    </AuthGuard>
  );
}
