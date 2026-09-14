"use client";

import { useEffect, useState } from "react";
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

type SubjectItem = {
  id: string;
  name: string;
  order: number;
  knowledgeAreaId: string;
  knowledgeAreaName: string;
  active: boolean;
};

export default function SubjectsAdmin() {
  const { toast } = useToast();
  const [areas, setAreas] = useState<ListItem[]>([]);
  const [areaFilter, setAreaFilter] = useState<string>("");
  const [view, setView] = useState<ViewFilter>("active");
  const { items, total, loading, page, setPage, reload } = usePagedList<SubjectItem>(
    "/api/subjects",
    {
      knowledgeAreaId: areaFilter,
      active: view === "all" ? undefined : view === "active",
    },
  );

  const [modal, setModal] = useState<{ item?: SubjectItem; name: string; order: string; areaId: string } | null>(null);
  const { setDeleting, setPurging, restore, modals } = useTrashConfirmations<SubjectItem>({
    reload,
    basePath: "/api/subjects",
    labels: {
      trash: "Assunto movido para a lixeira.",
      restore: "Assunto restaurado.",
      purge: "Assunto excluído definitivamente.",
    },
    messages: {
      trash: (i) => `Mover "${i.name}" para a lixeira? Você poderá restaurá-lo depois.`,
      purge: (i) =>
        `Excluir "${i.name}" definitivamente? Essa ação não pode ser desfeita e remove os cursos e aulas vinculados.`,
    },
  });

  useEffect(() => {
    (async () => {
      try {
        setAreas(await apiList<ListItem>("/api/knowledge-areas?active=true"));
      } catch (e) {
        toast("error", errorMessage(e));
      }
    })();
  }, [toast]);

  function openNew() {
    setModal({ name: "", order: "0", areaId: areaFilter });
  }

  function openEdit(item: SubjectItem) {
    setModal({ item, name: item.name, order: String(item.order), areaId: item.knowledgeAreaId });
  }

  async function submit() {
    if (!modal) return;
    if (!modal.areaId) {
      toast("error", "Selecione a área de conhecimento.");
      return;
    }
    try {
      if (modal.item) {
        await api(`/api/subjects/${modal.item.id}`, {
          method: "PUT",
          body: JSON.stringify({ name: modal.name, order: Number(modal.order), knowledgeAreaId: modal.areaId }),
        });
        toast("success", "Assunto atualizado.");
      } else {
        await api(`/api/subjects?knowledgeAreaId=${modal.areaId}`, {
          method: "POST",
          body: JSON.stringify({ name: modal.name, order: Number(modal.order) }),
        });
        toast("success", "Assunto criado.");
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
          icon="menu_book"
          title="Assuntos"
          accent="#8b5cf6"
          action={
            <button className="btn btn-primary text-sm" onClick={openNew}>
              <Icon name="add" /> Novo assunto
            </button>
          }
        >
          <label className="mb-2 inline-flex items-center gap-2 text-sm">
            <span className="text-white/60">Filtrar por área:</span>
            <select className="field w-auto" value={areaFilter} onChange={(e) => setAreaFilter(e.target.value)}>
              <option value="">Todas</option>
              {areas.map((a) => (
                <option key={a.id} value={a.id} className="bg-gray-900">
                  {a.name}
                </option>
              ))}
            </select>
          </label>

          <ViewTabs view={view} onChange={setView} total={total} />

          {loading ? (
            <p className="text-sm text-white/50">Carregando...</p>
          ) : items.length === 0 ? (
            <p className="text-sm text-white/50">
              {view === "trash" ? "A lixeira está vazia." : "Nenhum assunto cadastrado."}
            </p>
          ) : (
            <table className="w-full text-left text-sm">
              <thead>
                <tr className="border-b border-white/10 text-xs uppercase tracking-wide text-white/40">
                  <th className="py-2 pr-2 font-medium">Ordem</th>
                  <th className="py-2 pr-2 font-medium">Nome</th>
                  <th className="py-2 pr-2 font-medium">Área</th>
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
                    <td className="py-2.5 pr-2 text-white/60">{item.knowledgeAreaName || "—"}</td>
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
          title={modal?.item ? "Editar assunto" : "Novo assunto"}
          onClose={() => setModal(null)}
          onSubmit={submit}
        >
          <label className="block space-y-1">
            <span className="text-sm text-white/60">Área de conhecimento</span>
            <select
              className="field"
              value={modal?.areaId ?? ""}
              onChange={(e) => setModal((m) => (m ? { ...m, areaId: e.target.value } : m))}
            >
              <option value="">Selecione...</option>
              {areas.map((a) => (
                <option key={a.id} value={a.id} className="bg-gray-900">
                  {a.name}
                </option>
              ))}
            </select>
          </label>
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
