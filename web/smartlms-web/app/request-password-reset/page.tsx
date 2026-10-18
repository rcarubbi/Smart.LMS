"use client";

import { useState } from "react";
import Link from "next/link";
import { api } from "@/lib/api";
import AppShell from "@/components/AppShell";
import Panel from "@/components/ui/Panel";
import { useToast } from "@/components/ui/Toast";

export default function RequestPasswordResetPage() {
  const { toast } = useToast();
  const [email, setEmail] = useState("");
  const [sent, setSent] = useState(false);

  async function submit(event: React.FormEvent) {
    event.preventDefault();
    try {
      await api("/api/auth/request-password-reset", {
        method: "POST",
        body: JSON.stringify({ email }),
      });
      setSent(true);
    } catch {
      toast("error", "Não foi possível solicitar a redefinição agora.");
    }
  }

  return (
    <AppShell>
      <div className="mx-auto max-w-md">
        <Panel icon="lock_reset" title="Recuperar senha">
          {sent ? (
            <p className="text-sm text-white/70">Se o e-mail estiver cadastrado, enviaremos um link de redefinição.</p>
          ) : (
            <form onSubmit={submit} className="space-y-4">
              <label className="block space-y-1">
                <span className="text-sm text-white/60">E-mail cadastrado</span>
                <input className="field" type="email" required value={email} onChange={(e) => setEmail(e.target.value)} />
              </label>
              <button className="btn btn-primary w-full justify-center">Enviar link</button>
            </form>
          )}
          <Link href="/login" className="mt-4 block text-center text-sm text-indigo-300 hover:underline">Voltar ao login</Link>
        </Panel>
      </div>
    </AppShell>
  );
}
