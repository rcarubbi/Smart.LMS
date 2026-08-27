"use client";

import { useCallback, useEffect, useState } from "react";
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

export default function SubjectsAdmin() {
  const { toast } = useToast();
  const [areas, setAreas] = useState<ListItem[]>([]);
  const [areaFilter, setAreaFilter] = useState<string>("");
  const [items, setItems] = useState<ListItem[]>([]);
  const [loading, setLoading] = useState(true);

  const [modal, setModal] = useState<{ item?: ListItem; name: string; order: string; areaId: string } | null>(null);
  const [deleting, setDeleting] = useState<ListItem | null>(null);

  const loadAreas = useCallback(async () => {
    setAreas(await api<ListItem[]>("/api/knowledge-areas"));
  }, []);

  const load = useCallback(async () => {
    const q = areaFilter ? `?knowledgeAreaId=${areaFilter}` : "";
    setItems(await api<ListItem[]>(`/api/subjects${q}`));
  }, [areaFilter]);

  useEffect(() => {
    loadAreas();
  }, [loadAreas]);

  useEffect(() => {
    load()
      .catch((e) => toast("error", errorMessage(e)))
      .finally(() => setLoading(false));
  }, [load, toast]);

  function openNew() {
    setModal({ name: "", order: "0", areaId: areaFilter });
  }

  function openEdit(item: ListItem) {
    setModal({ item, name: item.name, order: String(item.order), areaId: areaFilter });
  }

  async function submit() {
    if (!modal) return;
    try {
      if (modal.item) {
        await api(`/api/subjects/${modal.item.id}`, {
          method: "PUT",
          body: JSON.stringify({ name: modal.name, order: Number(modal.order) }),
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
      load();
    } catch (e) {
      toast("error", errorMessage(e));
    }
  }

  async function confirmDelete() {
    if (!deleting) return;
    try {
      await api(`/api/subjects/${deleting.id}`, { method: "DELETE" });
      toast("success", "Assunto excluído.");
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
          icon="menu_book"
          title="Assuntos"
          accent="#8b5cf6"
          action={
            <button className="btn btn-primary text-sm" onClick={openNew}>
              <Icon name="add" /> Novo assunto
            </button>
          }
        >
          <label className="mb-4 inline-flex items-center gap-2 text-sm">
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

          {loading ? (
            <p className="text-sm text-white/50">Carregando...</p>
          ) : items.length === 0 ? (
            <p className="text-sm text-white/50">Nenhum assunto cadastrado.</p>
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
                {items.map((item) => {
                  const area = areas.find((a) => a.id === areaFilter) ?? areas[0];
                  return (
                    <tr key={item.id} className="hover:bg-white/5">
                      <td className="py-2.5 pr-2 text-white/60">{item.order}</td>
                      <td className="py-2.5 pr-2">{item.name}</td>
                      <td className="py-2.5 pr-2 text-white/60">{areaFilter ? area?.name : "—"}</td>
                      <td className="py-2.5 text-right">
                        <IconButton name="edit" title="Editar" onClick={() => openEdit(item)} />
                        <IconButton name="delete" title="Excluir" danger onClick={() => setDeleting(item)} />
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          )}
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

        <ConfirmModal
          open={!!deleting}
          title="Excluir assunto"
          message={`Excluir "${deleting?.name}"?`}
          onConfirm={confirmDelete}
          onCancel={() => setDeleting(null)}
        />
      </AppShell>
    </AuthGuard>
  );
}
