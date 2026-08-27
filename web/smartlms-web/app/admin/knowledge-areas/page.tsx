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

export default function KnowledgeAreasAdmin() {
  const { toast } = useToast();
  const [items, setItems] = useState<ListItem[]>([]);
  const [loading, setLoading] = useState(true);

  const [modal, setModal] = useState<{ item?: ListItem; name: string; order: string } | null>(null);
  const [deleting, setDeleting] = useState<ListItem | null>(null);

  const load = useCallback(async () => {
    try {
      setItems(await api<ListItem[]>("/api/knowledge-areas"));
    } catch (e) {
      toast("error", errorMessage(e));
    } finally {
      setLoading(false);
    }
  }, [toast]);

  useEffect(() => {
    load();
  }, [load]);

  function openNew() {
    setModal({ name: "", order: "0" });
  }

  function openEdit(item: ListItem) {
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
      load();
    } catch (e) {
      toast("error", errorMessage(e));
    }
  }

  async function confirmDelete() {
    if (!deleting) return;
    try {
      await api(`/api/knowledge-areas/${deleting.id}`, { method: "DELETE" });
      toast("success", "Área excluída.");
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
          icon="dns"
          title="Áreas de conhecimento"
          accent="#6366f1"
          action={
            <button className="btn btn-primary text-sm" onClick={openNew}>
              <Icon name="add" /> Nova área
            </button>
          }
        >
          {loading ? (
            <p className="text-sm text-white/50">Carregando...</p>
          ) : items.length === 0 ? (
            <p className="text-sm text-white/50">Nenhuma área cadastrada.</p>
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
                    <td className="py-2.5 pr-2">{item.name}</td>
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

        <ConfirmModal
          open={!!deleting}
          title="Excluir área"
          message={`Excluir "${deleting?.name}"?`}
          onConfirm={confirmDelete}
          onCancel={() => setDeleting(null)}
        />
      </AppShell>
    </AuthGuard>
  );
}

