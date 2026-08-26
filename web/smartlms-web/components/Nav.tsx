"use client";

import { useRouter } from "next/navigation";
import Link from "next/link";
import { clearTokens, currentUser } from "@/lib/api";

export default function Nav() {
  const router = useRouter();
  const user = typeof window !== "undefined" ? currentUser() : null;
  const isAdmin = user?.role === "Admin";

  function logout() {
    clearTokens();
    router.push("/login");
  }

  return (
    <header className="flex items-center gap-6 border-b bg-white px-6 py-3">
      <Link href="/" className="font-semibold text-indigo-600">
        SmartLMS
      </Link>
      <nav className="flex flex-1 items-center gap-4 text-sm text-gray-700">
        <Link href="/">Home</Link>
        {isAdmin && (
          <>
            <Link href="/catalog">Catálogo</Link>
            <Link href="/classrooms">Turmas</Link>
          </>
        )}
        {user && <Link href="/me">Meu progresso</Link>}
      </nav>
      {user ? (
        <div className="flex items-center gap-3 text-sm">
          <span className="text-gray-500">{user.email}</span>
          <button onClick={logout} className="rounded border px-2 py-1 hover:bg-gray-100">
            Sair
          </button>
        </div>
      ) : (
        <Link href="/login" className="text-sm font-medium text-indigo-600">
          Entrar
        </Link>
      )}
    </header>
  );
}
