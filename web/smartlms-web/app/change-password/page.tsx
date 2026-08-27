"use client";

import { useState } from "react";
import { api, errorMessage } from "@/lib/api";
import AppShell from "@/components/AppShell";
import AuthGuard from "@/components/AuthGuard";
import Panel from "@/components/ui/Panel";
import Icon from "@/components/ui/Icon";
import { useToast } from "@/components/ui/Toast";

export default function ChangePasswordPage() {
  const { toast } = useToast();
  const [current, setCurrent] = useState("");
  const [next, setNext] = useState("");
  const [confirm, setConfirm] = useState("");
  const [saving, setSaving] = useState(false);

  async function submit(e: React.FormEvent) {
    e.preventDefault();
    if (next !== confirm) {
      toast("error", "As senhas não conferem.");
      return;
    }
    setSaving(true);
    try {
      await api("/api/auth/change-password", {
        method: "POST",
        body: JSON.stringify({ currentPassword: current, newPassword: next }),
      });
      toast("success", "Senha alterada com sucesso.");
      setCurrent("");
      setNext("");
      setConfirm("");
    } catch (err) {
      toast("error", errorMessage(err));
    } finally {
      setSaving(false);
    }
  }

  return (
    <AuthGuard>
      <AppShell>
        <Panel icon="lock" title="Trocar senha" accent="#6366f1">
          <form onSubmit={submit} className="mx-auto max-w-sm space-y-3">
            <label className="block space-y-1">
              <span className="text-sm text-white/60">Senha atual</span>
              <input type="password" className="field" value={current} onChange={(e) => setCurrent(e.target.value)} required />
            </label>
            <label className="block space-y-1">
              <span className="text-sm text-white/60">Nova senha</span>
              <input type="password" className="field" value={next} onChange={(e) => setNext(e.target.value)} required />
            </label>
            <label className="block space-y-1">
              <span className="text-sm text-white/60">Confirmar nova senha</span>
              <input type="password" className="field" value={confirm} onChange={(e) => setConfirm(e.target.value)} required />
            </label>
            <button type="submit" className="btn btn-primary w-full justify-center" disabled={saving}>
              <Icon name="lock_reset" /> {saving ? "Salvando..." : "Alterar senha"}
            </button>
          </form>
        </Panel>
      </AppShell>
    </AuthGuard>
  );
}
