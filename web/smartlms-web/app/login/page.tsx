"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { api, storeTokens } from "@/lib/api";
import Icon from "@/components/ui/Icon";

type AuthResponse = {
  accessToken: string;
  refreshToken: string;
};

export default function LoginPage() {
  const router = useRouter();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  async function submit(event: React.FormEvent) {
    event.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const tokens = await api<AuthResponse>("/api/auth/login", {
        method: "POST",
        body: JSON.stringify({ email, password }),
      });
      storeTokens(tokens);
      router.push("/");
    } catch {
      setError("Credenciais inválidas. Verifique e tente novamente.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className="flex min-h-dvh items-center justify-center p-4">
      <form onSubmit={submit} className="glass w-full max-w-sm space-y-4 p-8">
        <div className="flex items-center gap-3">
          <span className="flex h-11 w-11 items-center justify-center rounded-full bg-indigo-500 text-lg font-bold text-white shadow-lg">
            S
          </span>
          <div>
            <h1 className="text-lg font-semibold">SmartLMS</h1>
            <p className="text-xs text-white/50">Acesse sua conta</p>
          </div>
        </div>

        <label className="block space-y-1">
          <span className="text-sm text-white/60">E-mail</span>
          <input type="email" required value={email} onChange={(e) => setEmail(e.target.value)} className="field" />
        </label>

        <label className="block space-y-1">
          <span className="text-sm text-white/60">Senha</span>
          <input type="password" required value={password} onChange={(e) => setPassword(e.target.value)} className="field" />
        </label>

        {error && <p className="text-sm text-red-400">{error}</p>}

        <button type="submit" disabled={loading} className="btn btn-primary w-full justify-center">
          {loading ? "Entrando..." : (<><Icon name="login" /> Entrar</>)}
        </button>

        <Link href="/request-password-reset" className="block text-center text-sm text-indigo-300 hover:underline">
          Esqueci minha senha
        </Link>

        <p className="text-center text-xs text-white/40">
          admin padrão: administrator@yourcompany.com / smart123
        </p>
      </form>
    </main>
  );
}
