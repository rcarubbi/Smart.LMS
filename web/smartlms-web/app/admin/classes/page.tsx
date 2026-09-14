"use client";

import { Suspense, useEffect, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
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

type ClassItem = {
  id: string;
  name: string;
  order: number;
  deliveryDays: number;
  contentType: string;
  active: boolean;
};
type SupportFile = { name: string; url: string };
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
  const [view, setView] = useState<ViewFilter>("active");
  const { items: classes, total, loading, page, setPage, reload } = usePagedList<ClassItem>(
    courseId ? `/api/classes?courseId=${courseId}` : "",
    { active: view === "all" ? undefined : view === "active" },
  );

  const [modal, setModal] = useState<{
    item?: ClassItem;
    name: string;
    content: string;
    order: string;
    deliveryDays: string;
    contentType: string;
    teacherId: string;
    files: SupportFile[];
  } | null>(null);
  const { setDeleting, setPurging, restore, modals } = useTrashConfirmations<ClassItem>({
    reload,
    basePath: "/api/classes",
    labels: {
      trash: "Aula movida para a lixeira.",
      restore: "Aula restaurada.",
      purge: "Aula excluída definitivamente.",
    },
    messages: {
      trash: (i) => `Mover "${i.name}" para a lixeira? Você poderá restaurá-la depois.`,
      purge: (i) =>
        `Excluir "${i.name}" definitivamente? Essa ação não pode ser desfeita e remove o material de apoio vinculado.`,
    },
  });
  const [uploadingFiles, setUploadingFiles] = useState(false);

  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const [cs, t] = await Promise.all([
          apiList<Course>("/api/courses"),
          apiList<Teacher>("/api/users?role=Teacher"),
        ]);
        if (cancelled) return;
        setCourses(cs);
        setTeachers(t);
        if (cs.length > 0) setCourseId((cur) => cur || cs[0].id);
      } catch (e) {
        toast("error", errorMessage(e));
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [toast]);

  function openNew() {
    setModal({ name: "", content: "", order: "0", deliveryDays: "0", contentType: "YouTube", teacherId: "", files: [] });
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
      files: [],
    });
    api<{ content: string; teacherId: string; files: { name: string; storagePath: string }[] }>(`/api/classes/${item.id}`)
      .then((detail) =>
        setModal((m) =>
          m && m.item?.id === item.id
            ? {
                ...m,
                content: detail.content,
                teacherId: detail.teacherId,
                files: (detail.files ?? []).map((f) => ({ name: f.name, url: f.storagePath })),
              }
            : m,
        ),
      )
      .catch(() => {});
  }

  async function uploadMaterials(fileList: File[]) {
    if (fileList.length === 0) return;
    const fd = new FormData();
    for (const file of fileList) {
      fd.append("files", file);
    }
    setUploadingFiles(true);
    try {
      const uploaded = await api<SupportFile[]>("/api/uploads/support-files", { method: "POST", body: fd });
      setModal((m) => (m ? { ...m, files: [...m.files, ...uploaded] } : m));
      toast("success", `${uploaded.length} arquivo(s) enviado(s).`);
    } catch (e) {
      toast("error", errorMessage(e));
    } finally {
      setUploadingFiles(false);
    }
  }

  async function submit() {
    if (!modal) return;
    if (!modal.teacherId) {
      toast("error", "Selecione um professor para a aula.");
      return;
    }
    const files = modal.files.map((f) => ({ name: f.name, storagePath: f.url }));
    const payload = {
      name: modal.name,
      content: modal.content,
      order: Number(modal.order),
      deliveryDays: Number(modal.deliveryDays),
      contentType: modal.contentType,
      courseId,
      teacherId: modal.teacherId,
      files,
    };
    try {
      if (modal.item) {
        await api(`/api/classes/${modal.item.id}`, {
          method: "PUT",
          body: JSON.stringify(payload),
        });
        toast("success", "Aula atualizada.");
      } else {
        await api("/api/classes", { method: "POST", body: JSON.stringify(payload) });
        toast("success", "Aula criada.");
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

          <ViewTabs view={view} onChange={setView} total={total} />

          {loading ? (
            <p className="text-sm text-white/50">Carregando...</p>
          ) : !courseId ? (
            <p className="text-sm text-white/50">Selecione um curso para listar as aulas.</p>
          ) : classes.length === 0 ? (
            <p className="text-sm text-white/50">
              {view === "trash" ? "A lixeira está vazia." : "Nenhuma aula neste curso."}
            </p>
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
                    <td className="py-2.5 pr-2 font-medium">
                      {item.name}
                      {!item.active && (
                        <span className="ml-2 rounded bg-amber-500/20 px-1.5 py-0.5 text-xs text-amber-300">
                          Na lixeira
                        </span>
                      )}
                    </td>
                    <td className="py-2.5 pr-2 text-white/70">{item.contentType}</td>
                    <td className="py-2.5 pr-2 text-white/60">{item.deliveryDays}</td>
                    <td className="py-2.5 text-right">
                      {item.active ? (
                        <>
                          <IconButton name="edit" title="Editar" onClick={() => openEdit(item)} />
                          <IconButton name="delete" title="Mover para a lixeira" variant="danger" onClick={() => setDeleting(item)} />
                        </>
                      ) : (
                        <>
                          <IconButton name="restore" title="Restaurar" onClick={() => restore(item)} />
                          <IconButton name="delete_forever" title="Excluir definitivamente" variant="danger" onClick={() => setPurging(item)} />
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
            <span className="text-sm text-white/60">Conteúdo (ID do vídeo ou URL)</span>
            <input
              value={modal?.content ?? ""}
              onChange={(e) => setModal((m) => (m ? { ...m, content: e.target.value } : m))}
              className="field"
              placeholder="Ex.: uKH-fiYIBvU ou https://www.youtube.com/watch?v=uKH-fiYIBvU"
            />
            <span className="block text-xs text-white/40">
              Pode colar apenas o ID (YouTube ou Vimeo) — a URL completa é montada automaticamente.
            </span>
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
          <label className="block space-y-1">
            <span className="text-sm text-white/60">Material de apoio</span>
            <div
              className="rounded-lg border-2 border-dashed border-white/20 bg-white/5 px-3 py-6 text-center text-sm text-white/50 transition hover:border-white/40 hover:bg-white/10"
              onDragOver={(e) => e.preventDefault()}
              onDrop={(e) => {
                e.preventDefault();
                if (e.dataTransfer.files.length > 0) uploadMaterials(Array.from(e.dataTransfer.files));
              }}
            >
              {uploadingFiles ? (
                <span className="flex items-center justify-center gap-2">
                  <Icon name="cloud_upload" className="text-xl" /> Enviando arquivos...
                </span>
              ) : (
                <>
                  <Icon name="attach_file" className="text-2xl" />
                  <span className="ml-2">Arraste arquivos aqui ou clique para escolher</span>
                </>
              )}
              <input
                type="file"
                multiple
                className="hidden"
                accept=".pdf,.doc,.docx,.xls,.xlsx,.xlt,.xltx,.csv,.ppt,.pptx,.txt,.zip,.rtf,.odt,.ods,.odp,.md,.xml,.xsl,.xslt,.json,.jpg,.jpeg,.png,.webp,.gif"
                onChange={(e) => {
                  if (e.target.files && e.target.files.length > 0) uploadMaterials(Array.from(e.target.files));
                  e.target.value = "";
                }}
              />
            </div>
            {modal && modal.files.length > 0 && (
              <ul className="mt-2 max-h-32 space-y-1 overflow-y-auto rounded-lg border border-white/10 bg-white/5 p-2">
                {modal.files.map((f, idx) => (
                  <li key={`${f.url}-${idx}`} className="flex items-center gap-2 text-xs">
                    <Icon name="description" className="text-white/50" />
                    <span className="truncate">{f.name}</span>
                    <button
                      type="button"
                      className="ml-auto text-red-300 hover:text-red-200"
                      onClick={() => setModal((m) => (m ? { ...m, files: m.files.filter((_, i) => i !== idx) } : m))}
                    >
                      Remover
                    </button>
                  </li>
                ))}
              </ul>
            )}
          </label>
        </FormModal>

        {modals}
      </AppShell>
    </AuthGuard>
  );
}
