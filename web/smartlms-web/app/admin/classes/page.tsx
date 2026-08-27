"use client";

import { Suspense, useCallback, useEffect, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
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

type ClassItem = { id: string; name: string; order: number; deliveryDays: number; contentType: string };
type Course = ListItem & { id: string };
type Teacher = { id: string; name: string };

export default function ClassesAdminPage() {
  return (
    <Suspense fallback={null}>
      <ClassesInner />
    </Suspense>
  );
}

function ClassesInner() {
  const router = useRouter();
  const search = useSearchParams();
  const courseParam = search.get("course") ?? "";

  const { toast } = useToast();
  const [courses, setCourses] = useState<Course[]>([]);
  const [teachers, setTeachers] = useState<Teacher[]>([]);
  const [courseId, setCourseId] = useState(courseParam);
  const [classes, setClasses] = useState<ClassItem[]>([]);
  const [loading, setLoading] = useState(false);

  const [modal, setModal] = useState<{ item?: ClassItem; name: string; content: string; order: string; deliveryDays: string; contentType: string; teacherId: string } | null>(null);
  const [deleting, setDeleting] = useState<ClassItem | null>(null);

  const loadOptions = useCallback(async () => {
    const [cs, t] = await Promise.all([
      api<Course[]>("/api/courses"),
      api<Teacher[]>("/api/users?role=Teacher"),
    ]);
    setCourses(cs);
    setTeachers(t);
    if (!courseId && cs.length > 0) setCourseId(cs[0].id);
  }, [courseId]);

  const load = useCallback(async () => {
    if (!courseId) return;
    setLoading(true);
    try {
      setClasses(await api<ClassItem[]>(`/api/classes?courseId=${courseId}`));
    } catch (e) {
      toast("error", errorMessage(e));
    } finally {
      setLoading(false);
    }
  }, [courseId, toast]);

  useEffect(() => {
    loadOptions();
  }, [loadOptions]);

  useEffect(() => {
    load();
  }, [load]);

  useEffect(() => {
    if (courseParam && courseParam !== courseId) setCourseId(courseParam);
  }, [courseParam, courseId]);

  function openNew() {
    setModal({ name: "", content: "", order: "0", deliveryDays: "0", contentType: "YouTube", teacherId: "" });
  }

  function openEdit(item: ClassItem) {
    setModal({
      item,
      name: item.name,
      content: "",
      order: String(item.order),
      deliveryDays: String(item.deliveryDays),
      contentType: item.contentType,
      teacherId: "",
    });
    api<{ content: string; teacherId: string }>(`/api/classes/${item.id}`)
      .then((detail) =>
        setModal((m) =>
          m && m.item?.id === item.id
            ? { ...m, content: detail.content, teacherId: detail.teacherId }
            : m,
        ),
      )
      .catch(() => {});
  }

  async function submit() {
    if (!modal) return;
    const payload = {
      name: modal.name,
      content: modal.content,
      order: Number(modal.order),
      deliveryDays: Number(modal.deliveryDays),
      contentType: modal.contentType,
      courseId,
      teacherId: modal.teacherId,
    };
    try {
      if (modal.item) {
        await api(`/api/classes/${modal.item.id}`, {
          method: "PUT",
          body: JSON.stringify({
            name: modal.name,
            content: modal.content,
            order: Number(modal.order),
            deliveryDays: Number(modal.deliveryDays),
            contentType: modal.contentType,
            teacherId: modal.teacherId,
          }),
        });
        toast("success", "Aula atualizada.");
      } else {
        await api("/api/classes", { method: "POST", body: JSON.stringify(payload) });
        toast("success", "Aula criada.");
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
      await api(`/api/classes/${deleting.id}`, { method: "DELETE" });
      toast("success", "Aula excluída.");
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
          icon="play_circle"
          title={`Aulas — ${courses.find((c) => c.id === courseId)?.name ?? "Curso"}`}
          accent="#f59e0b"
          action={
            <button className="btn btn-primary text-sm" onClick={openNew}>
              <Icon name="add" /> Nova aula
            </button>
          }
        >
          <label className="mb-4 inline-flex items-center gap-2 text-sm">
            <span className="text-white/60">Curso:</span>
            <select
              className="field w-auto"
              value={courseId}
              onChange={(e) => {
                const v = e.target.value;
                setCourseId(v);
                router.replace(`/admin/classes?course=${v}`);
              }}
            >
              <option value="">Selecione...</option>
              {courses.map((c) => (
                <option key={c.id} value={c.id} className="bg-gray-900">
                  {c.name}
                </option>
              ))}
            </select>
          </label>

          {loading ? (
            <p className="text-sm text-white/50">Carregando...</p>
          ) : !courseId ? (
            <p className="text-sm text-white/50">Selecione um curso para listar as aulas.</p>
          ) : classes.length === 0 ? (
            <p className="text-sm text-white/50">Nenhuma aula neste curso.</p>
          ) : (
            <table className="w-full text-left text-sm">
              <thead>
                <tr className="border-b border-white/10 text-xs uppercase tracking-wide text-white/40">
                  <th className="py-2 pr-2 font-medium">Ordem</th>
                  <th className="py-2 pr-2 font-medium">Nome</th>
                  <th className="py-2 pr-2 font-medium">Tipo</th>
                  <th className="py-2 pr-2 font-medium">Entrega (dias)</th>
                  <th className="py-2 text-right font-medium">Ações</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-white/5">
                {classes.map((item) => (
                  <tr key={item.id} className="hover:bg-white/5">
                    <td className="py-2.5 pr-2 text-white/60">{item.order}</td>
                    <td className="py-2.5 pr-2 font-medium">{item.name}</td>
                    <td className="py-2.5 pr-2 text-white/70">{item.contentType}</td>
                    <td className="py-2.5 pr-2 text-white/60">{item.deliveryDays}</td>
                    <td className="py-2.5 text-right">
                      <IconButton name="edit" title="Editar" onClick={() => openEdit(item)} />
                      <IconButton name="delete" title="Excluir" danger onClick={() => setDeleting(item)} />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </Panel>

        <FormModal
          open={!!modal}
          title={modal?.item ? "Editar aula" : "Nova aula"}
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
          <label className="block space-y-1">
            <span className="text-sm text-white/60">Conteúdo (URL do vídeo)</span>
            <input
              value={modal?.content ?? ""}
              onChange={(e) => setModal((m) => (m ? { ...m, content: e.target.value } : m))}
              className="field"
              placeholder="https://www.youtube.com/watch?v=..."
            />
          </label>
          <div className="grid grid-cols-3 gap-3">
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
              <span className="text-sm text-white/60">Dias p/ entrega</span>
              <input
                type="number"
                value={modal?.deliveryDays ?? ""}
                onChange={(e) => setModal((m) => (m ? { ...m, deliveryDays: e.target.value } : m))}
                className="field"
              />
            </label>
            <label className="block space-y-1">
              <span className="text-sm text-white/60">Tipo</span>
              <select
                className="field"
                value={modal?.contentType ?? "YouTube"}
                onChange={(e) => setModal((m) => (m ? { ...m, contentType: e.target.value } : m))}
              >
                <option value="YouTube" className="bg-gray-900">YouTube</option>
                <option value="Vimeo" className="bg-gray-900">Vimeo</option>
              </select>
            </label>
          </div>
          <label className="block space-y-1">
            <span className="text-sm text-white/60">Professor</span>
            <select
              className="field"
              value={modal?.teacherId ?? ""}
              onChange={(e) => setModal((m) => (m ? { ...m, teacherId: e.target.value } : m))}
            >
              <option value="">Selecione...</option>
              {teachers.map((t) => (
                <option key={t.id} value={t.id} className="bg-gray-900">
                  {t.name}
                </option>
              ))}
            </select>
          </label>
        </FormModal>

        <ConfirmModal
          open={!!deleting}
          title="Excluir aula"
          message={`Excluir "${deleting?.name}"?`}
          onConfirm={confirmDelete}
          onCancel={() => setDeleting(null)}
        />
      </AppShell>
    </AuthGuard>
  );
}
