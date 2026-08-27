"use client";

import { useState } from "react";
import { api, errorMessage } from "@/lib/api";
import AppShell from "@/components/AppShell";
import AuthGuard from "@/components/AuthGuard";
import Panel from "@/components/ui/Panel";
import Icon from "@/components/ui/Icon";
import { useToast } from "@/components/ui/Toast";

export default function NoticesAdmin() {
  const { toast } = useToast();
  const [text, setText] = useState("");
  const [sending, setSending] = useState(false);

  async function submit(e: React.FormEvent) {
    e.preventDefault();
    if (!text.trim()) {
      toast("error", "Digite o texto do aviso.");
      return;
    }
    setSending(true);
    try {
      await api("/api/notices", { method: "POST", body: JSON.stringify({ text }) });
      toast("success", "Aviso enviado para todos os usuários.");
      setText("");
    } catch (err) {
      toast("error", errorMessage(err));
    } finally {
      setSending(false);
    }
  }

  return (
    <AuthGuard roles={["Admin"]}>
      <AppShell>
        <Panel
          icon="campaign"
          title="Enviar aviso"
          accent="#f59e0b"
        >
          <form onSubmit={submit} className="space-y-3">
            <label className="block space-y-1">
              <span className="text-sm text-white/60">Texto (aceita HTML básico)</span>
              <textarea
                value={text}
                onChange={(e) => setText(e.target.value)}
                rows={6}
                className="field resize-y"
                placeholder='Ex.: <a href="/class/...">Nova aula disponível</a> <br/> Confira!'
              />
            </label>
            <div className="flex items-center justify-between gap-3">
              <p className="text-xs text-white/40">
                O aviso é exibido na página inicial de todos os usuários. Links para aulas/cursos podem ser incluídos.
              </p>
              <button type="submit" className="btn btn-primary" disabled={sending}>
                <Icon name="send" /> {sending ? "Enviando..." : "Enviar"}
              </button>
            </div>
          </form>
        </Panel>
      </AppShell>
    </AuthGuard>
  );
}
