"use client";

import { useState } from "react";
import { api, errorMessage } from "@/lib/api";
import { usePagedList } from "@/lib/usePagedList";
import AppShell from "@/components/AppShell";
import AuthGuard from "@/components/AuthGuard";
import Panel from "@/components/ui/Panel";
import FormModal from "@/components/ui/FormModal";
import ConfirmModal from "@/components/ui/ConfirmModal";
import Icon from "@/components/ui/Icon";
import IconButton from "@/components/ui/IconButton";
import Pagination from "@/components/ui/Pagination";
import { useToast } from "@/components/ui/Toast";

type Role = "Teacher" | "Student";
type UserItem = { id: string; name: string; email: string; role: string; active: boolean };
type ViewFilter = "active" | "trash" | "all";

export default function UserAdmin({ role, icon, title, accent }: { role: Role; icon: string; title: string; accent: string }) {
  const { toast } = useToast();
  const [view, setView] = useState<ViewFilter>("active");
  const { items, total, loading, page, setPage, reload } = usePagedList<UserItem>(
    "/api/users",
    { role, active: view === "all" ? undefined : view === "active" },
  );
  const [modal, setModal] = useState<{ name: string; email: string; password: string } | null>(null);
  const [saving, setSaving] = useState(false);
  const [deleting, setDeleting] = useState<UserItem | null>(null);
  const [purging, setPurging] = useState<UserItem | null>(null);

  const shown = items;

  async function submit() {
    if (!modal) return;
    const name = modal.name.trim();
    const email = modal.email.trim();
    if (!name) {
      toast("error", "Informe o nome.");
      return;
    }
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      toast("error", "Informe um e-mail válido.");
      return;
    }
    if (modal.password.length < 8) {
      toast("error", "A senha deve ter no mínimo 8 caracteres.");
      return;
    }
    setSaving(true);
    try {
      await api("/api/users", {
        method: "POST",
        body: JSON.stringify({ name, email, password: modal.password, role }),
      });
      toast("success", role === "Teacher" ? "Professor criado." : "Aluno criado.");
      setModal(null);
      reload();
    } catch (e) {
      toast("error", errorMessage(e));
    } finally {
      setSaving(false);
    }
  }

  async function confirmDelete() {
    if (!deleting) return;
    try {
      await api(`/api/users/${deleting.id}`, { method: "DELETE" });
      toast("success", `${deleting.name} movido(a) para a lixeira.`);
      setDeleting(null);
      reload();
    } catch (e) {
      toast("error", errorMessage(e));
    }
  }

  async function confirmPurge() {
    if (!purging) return;
    try {
      await api(`/api/users/${purging.id}/permanent`, { method: "DELETE" });
      toast("success", `${purging.name} excluído(a) definitivamente.`);
      setPurging(null);
      reload();
    } catch (e) {
      toast("error", errorMessage(e));
    }
  }

  async function restore(u: UserItem) {
    try {
      await api(`/api/users/${u.id}/restore`, { method: "POST" });
      toast("success", `${u.name} restaurado(a).`);
      reload();
    } catch (e) {
      toast("error", errorMessage(e));
    }
  }

  return (
    <AuthGuard roles={["Admin"]}>
      <AppShell>
        <Panel
          icon={icon}
          title={title}
          accent={accent}
          action={
            <button className="btn btn-primary text-sm" onClick={() => setModal({ name: "", email: "", password: "" })}>
              <Icon name="person_add" /> Novo {role === "Teacher" ? "professor" : "aluno"}
            </button>
          }
        >
          {loading ? (
            <p className="text-sm text-white/50">Carregando...</p>
          ) : (
            <>
              <div className="mb-4 flex flex-wrap items-center gap-1 text-sm">
                {(
                  [
                    ["active", `Ativos${view === "active" ? ` (${total})` : ""}`],
                    ["trash", `Lixeira${view === "trash" ? ` (${total})` : ""}`],
                    ["all", "Todos"],
                  ] as const
                ).map(([value, label]) => (
                  <button
                    key={value}
                    onClick={() => setView(value)}
                    className={`rounded-full px-3 py-1 transition ${
                      view === value ? "bg-white/20 text-white" : "text-white/50 hover:bg-white/10"
                    }`}
                  >
                    {label}
                  </button>
                ))}
              </div>
              {shown.length === 0 ? (
                <p className="text-sm text-white/50">{view === "trash" ? "A lixeira está vazia." : "Nenhum registro."}</p>
              ) : (
                <table className="w-full text-left text-sm">
                  <thead>
                    <tr className="border-b border-white/10 text-xs uppercase tracking-wide text-white/40">
                      <th className="py-2 pr-2 font-medium">Nome</th>
                      <th className="py-2 pr-2 font-medium">E-mail</th>
                      <th className="py-2 pr-2 font-medium">Status</th>
                      <th className="py-2 text-right font-medium">Ações</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-white/5">
                    {shown.map((u) => (
                      <tr key={u.id} className="hover:bg-white/5">
                        <td className="py-2.5 pr-2 font-medium">{u.name}</td>
                        <td className="py-2.5 pr-2 text-white/70">{u.email}</td>
                        <td className="py-2.5 pr-2">
                          {u.active ? (
                            <span className="inline-flex items-center gap-2 text-xs text-emerald-400">
                              <span className="h-1.5 w-1.5 rounded-full bg-emerald-400" /> Ativo
                            </span>
                          ) : (
                            <span className="inline-flex items-center gap-1 rounded-full bg-red-500/10 px-2 py-0.5 text-xs text-red-400">
                              <Icon name="delete" className="text-sm" /> Na lixeira
                            </span>
                          )}
                        </td>
                        <td className="py-2.5 text-right">
                          {u.active ? (
                            <IconButton name="delete" title="Mover para a lixeira" variant="danger" onClick={() => setDeleting(u)} />
                          ) : (
                            <>
                              <IconButton name="restore" title="Restaurar" onClick={() => restore(u)} />
                              <IconButton name="delete_forever" title="Excluir definitivamente" variant="danger" onClick={() => setPurging(u)} />
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
          open={!!modal}
          title={`Novo ${role === "Teacher" ? "professor" : "aluno"}`}
          onClose={() => setModal(null)}
          onSubmit={submit}
          submitLabel={saving ? "Salvando..." : "Salvar"}
        >
          <label className="block space-y-1">
            <span className="text-sm text-white/60">Nome</span>
            <input
              className="field"
              value={modal?.name ?? ""}
              onChange={(e) => setModal((m) => (m ? { ...m, name: e.target.value } : m))}
              autoFocus
            />
          </label>
          <label className="block space-y-1">
            <span className="text-sm text-white/60">E-mail</span>
            <input
              type="email"
              className="field"
              value={modal?.email ?? ""}
              onChange={(e) => setModal((m) => (m ? { ...m, email: e.target.value } : m))}
            />
          </label>
          <label className="block space-y-1">
            <span className="text-sm text-white/60">Senha</span>
            <input
              type="password"
              className="field"
              value={modal?.password ?? ""}
              onChange={(e) => setModal((m) => (m ? { ...m, password: e.target.value } : m))}
            />
          </label>
        </FormModal>

        <ConfirmModal
          open={!!deleting}
          title="Mover para a lixeira"
          message={`Excluir "${deleting?.name}"? Ficará na lixeira e pode ser restaurado(a).`}
          onConfirm={confirmDelete}
          onCancel={() => setDeleting(null)}
        />

        <ConfirmModal
          open={!!purging}
          title="Excluir definitivamente"
          message={`Esta ação não pode ser desfeita. Excluir "${purging?.name}" para sempre?`}
          onConfirm={confirmPurge}
          onCancel={() => setPurging(null)}
        />
      </AppShell>
    </AuthGuard>
  );
}
