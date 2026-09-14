"use client";

import { useState } from "react";
import { api, errorMessage } from "@/lib/api";
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

type Area = ListItem & { active: boolean };

export default function KnowledgeAreasAdmin() {
  const { toast } = useToast();
  const [view, setView] = useState<ViewFilter>("active");
  const { items, total, loading, page, setPage, reload } = usePagedList<Area>(
    "/api/knowledge-areas",
    { active: view === "all" ? undefined : view === "active" },
  );

  const [modal, setModal] = useState<{ item?: Area; name: string; order: string } | null>(null);
  const { setDeleting, setPurging, restore, modals } = useTrashConfirmations<Area>({
    reload,
    basePath: "/api/knowledge-areas",
    labels: {
      trash: "Área movida para a lixeira.",
      restore: "Área restaurada.",
      purge: "Área excluída definitivamente.",
    },
    messages: {
      trash: (i) => `Mover "${i.name}" para a lixeira? Você poderá restaurá-la depois.`,
      purge: (i) =>
        `Excluir "${i.name}" definitivamente? Essa ação não pode ser desfeita e remove os assuntos, cursos e aulas vinculados.`,
    },
  });

  function openNew() {
    setModal({ name: "", order: "0" });
  }

  function openEdit(item: Area) {
    setModal({ item, name: item.name, order: String(item.order) });
  }

  async function submit() {
    if (!modal) return;
    try {
      if (modal.item) {
        await api(`/api/knowledge-areas/${modal.item.id}`, {
          method: "PUT",
          body: JSON.stringify({ name: modal.name, order: Number(modal.order) }),
        });
        toast("success", "Área atualizada.");
      } else {
        await api(`/api/knowledge-areas`, {
          method: "POST",
          body: JSON.stringify({ name: modal.name, order: Number(modal.order) }),
        });
        toast("success", "Área criada.");
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
          icon="dns"
          title="Áreas de conhecimento"
          accent="#6366f1"
          action={
            <button className="btn btn-primary text-sm" onClick={openNew}>
              <Icon name="add" /> Nova área
            </button>
          }
        >
          <ViewTabs view={view} onChange={setView} total={total} />

          {loading ? (
            <p className="text-sm text-white/50">Carregando...</p>
          ) : items.length === 0 ? (
            <p className="text-sm text-white/50">
              {view === "trash" ? "A lixeira está vazia." : "Nenhuma área cadastrada."}
            </p>
          ) : (
            <table className="w-full text-left text-sm">
              <thead>
                <tr className="border-b border-white/10 text-xs uppercase tracking-wide text-white/40">
                  <th className="py-2 pr-2 font-medium">Ordem</th>
                  <th className="py-2 pr-2 font-medium">Nome</th>
                  <th className="py-2 text-right font-medium">Ações</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-white/5">
                {items.map((item) => (
                  <tr key={item.id} className="hover:bg-white/5">
                    <td className="py-2.5 pr-2 text-white/60">{item.order}</td>
                    <td className="py-2.5 pr-2">
                      {item.name}
                      {!item.active && (
                        <span className="ml-2 rounded bg-amber-500/20 px-1.5 py-0.5 text-xs text-amber-300">
                          Na lixeira
                        </span>
                      )}
                    </td>
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
          title={modal?.item ? "Editar área" : "Nova área"}
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
            <span className="text-sm text-white/60">Ordem</span>
            <input
              type="number"
              value={modal?.order ?? ""}
              onChange={(e) => setModal((m) => (m ? { ...m, order: e.target.value } : m))}
              className="field"
            />
          </label>
        </FormModal>

        {modals}
      </AppShell>
    </AuthGuard>
  );
}

