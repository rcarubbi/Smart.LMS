"use client";

import { useCallback, useEffect, useState } from "react";
import { api, errorMessage } from "@/lib/api";
import AppShell from "@/components/AppShell";
import AuthGuard from "@/components/AuthGuard";
import Panel from "@/components/ui/Panel";
import FormModal from "@/components/ui/FormModal";
import Icon from "@/components/ui/Icon";
import { useToast } from "@/components/ui/Toast";

type Role = "Teacher" | "Student";
type UserItem = { id: string; name: string; email: string; role: string; active: boolean };

export default function UserAdmin({ role, icon, title, accent }: { role: Role; icon: string; title: string; accent: string }) {
  const { toast } = useToast();
  const [items, setItems] = useState<UserItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [modal, setModal] = useState<{ name: string; email: string; password: string } | null>(null);
  const [saving, setSaving] = useState(false);

  const load = useCallback(async () => {
    try {
      setItems(await api<UserItem[]>(`/api/users?role=${role}`));
    } catch (e) {
      toast("error", errorMessage(e));
    } finally {
      setLoading(false);
    }
  }, [role, toast]);

  useEffect(() => {
    load();
  }, [load]);

  async function submit() {
    if (!modal) return;
    setSaving(true);
    try {
      await api("/api/users", {
        method: "POST",
        body: JSON.stringify({ name: modal.name, email: modal.email, password: modal.password, role }),
      });
      toast("success", role === "Teacher" ? "Professor criado." : "Aluno criado.");
      setModal(null);
      load();
    } catch (e) {
      toast("error", errorMessage(e));
    } finally {
      setSaving(false);
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
          ) : items.length === 0 ? (
            <p className="text-sm text-white/50">Nenhum registro.</p>
          ) : (
            <table className="w-full text-left text-sm">
              <thead>
                <tr className="border-b border-white/10 text-xs uppercase tracking-wide text-white/40">
                  <th className="py-2 pr-2 font-medium">Nome</th>
                  <th className="py-2 pr-2 font-medium">E-mail</th>
                  <th className="py-2 pr-2 font-medium">Status</th>
                  <th className="py-2 text-right font-medium">Papel</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-white/5">
                {items.map((u) => (
                  <tr key={u.id} className="hover:bg-white/5">
                    <td className="py-2.5 pr-2 font-medium">{u.name}</td>
                    <td className="py-2.5 pr-2 text-white/70">{u.email}</td>
                    <td className="py-2.5 pr-2">
                      <span className={`inline-flex items-center gap-1 text-xs ${u.active ? "text-emerald-400" : "text-white/40"}`}>
                        <span className={`h-1.5 w-1.5 rounded-full ${u.active ? "bg-emerald-400" : "bg-white/30"}`} />
                        {u.active ? "Ativo" : "Inativo"}
                      </span>
                    </td>
                    <td className="py-2.5 text-right text-white/70">{u.role}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
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
      </AppShell>
    </AuthGuard>
  );
}
