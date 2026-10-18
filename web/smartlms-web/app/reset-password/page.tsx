"use client";

import { Suspense, useState } from "react";
import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { api, errorMessage } from "@/lib/api";
import AppShell from "@/components/AppShell";
import Panel from "@/components/ui/Panel";
import { useToast } from "@/components/ui/Toast";

export default function ResetPasswordPage() {
  return <Suspense fallback={null}><ResetPasswordForm /></Suspense>;
}

function ResetPasswordForm() {
  const token = useSearchParams().get("token") ?? "";
  const { toast } = useToast();
  const [password, setPassword] = useState("");
  const [done, setDone] = useState(false);

  async function submit(event: React.FormEvent) {
    event.preventDefault();
    try {
      await api("/api/auth/reset-password", {
        method: "POST",
        body: JSON.stringify({ token, newPassword: password }),
      });
      setDone(true);
    } catch (error) {
      toast("error", errorMessage(error, "Link inválido ou expirado."));
    }
  }

  return (
    <AppShell>
      <div className="mx-auto max-w-md">
        <Panel icon="lock_reset" title="Definir nova senha">
          {done ? (
            <p className="text-sm text-white/70">Senha atualizada. <Link href="/login" className="text-indigo-300 hover:underline">Entrar</Link></p>
          ) : (
            <form onSubmit={submit} className="space-y-4">
              <label className="block space-y-1">
                <span className="text-sm text-white/60">Nova senha</span>
                <input className="field" type="password" minLength={8} required value={password} onChange={(e) => setPassword(e.target.value)} />
              </label>
              <button className="btn btn-primary w-full justify-center" disabled={!token}>Redefinir senha</button>
              {!token && <p className="text-sm text-red-300">Link sem token. Solicite uma nova redefinição.</p>}
            </form>
          )}
        </Panel>
      </div>
    </AppShell>
  );
}
