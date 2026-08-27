"use client";

import { useEffect, useRef, useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { clearTokens, currentUser } from "@/lib/api";
import Icon from "./ui/Icon";

export default function Nav() {
  const router = useRouter();
  const user = typeof window !== "undefined" ? currentUser() : null;
  const isAdmin = user?.role === "Admin";
  const [openMenu, setOpenMenu] = useState<string | null>(null);
  const menuRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    function onClick(e: MouseEvent) {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) {
        setOpenMenu(null);
      }
    }
    document.addEventListener("mousedown", onClick);
    return () => document.removeEventListener("mousedown", onClick);
  }, []);

  function toggle(name: string) {
    setOpenMenu((cur) => (cur === name ? null : name));
  }

  function logout() {
    clearTokens();
    router.push("/login");
  }

  return (
    <header className="glass sticky top-0 z-40 mx-3 mt-3 flex items-center gap-4 px-4 py-2.5">
      <Link href="/" className="flex items-center gap-2">
        <span
          className="flex h-9 w-9 items-center justify-center rounded-full bg-indigo-500 text-sm font-bold text-white shadow-lg"
          title="SmartLMS"
        >
          S
        </span>
        <span className="text-sm font-semibold uppercase tracking-wide text-white/90">
          SmartLMS
        </span>
      </Link>

      <nav className="flex flex-1 items-center gap-1 text-sm">
        <Link href="/" className="rounded px-2 py-1.5 text-white/70 hover:bg-white/5 hover:text-white">
          Início
        </Link>

        {isAdmin && (
          <div ref={menuRef} className="relative">
            <button
              onClick={() => toggle("management")}
              className="flex items-center gap-1 rounded px-2 py-1.5 text-white/70 hover:bg-white/5 hover:text-white"
            >
              <Icon name="settings" className="text-base" /> Gerenciamento
              <Icon name="arrow_drop_down" className="text-base" />
            </button>
            {openMenu === "management" && (
              <div className="glass absolute left-0 top-full mt-1 w-64 p-2 text-white/80">
                <p className="px-3 pt-1 pb-1 text-[11px] font-medium uppercase tracking-wide text-white/40">
                  Pessoas
                </p>
                <DropdownLink href="/admin/students" onClick={() => setOpenMenu(null)}>Alunos</DropdownLink>
                <DropdownLink href="/admin/teachers" onClick={() => setOpenMenu(null)}>Professores</DropdownLink>
                <DropdownLink href="/admin/classrooms" onClick={() => setOpenMenu(null)}>Turmas</DropdownLink>
                <div className="my-1 mx-1 border-t border-white/10" />
                <p className="px-3 pt-1 pb-1 text-[11px] font-medium uppercase tracking-wide text-white/40">
                  Conteúdo
                </p>
                <DropdownLink href="/admin/knowledge-areas" onClick={() => setOpenMenu(null)}>Áreas de conhecimento</DropdownLink>
                <DropdownLink href="/admin/subjects" onClick={() => setOpenMenu(null)}>Assuntos</DropdownLink>
                <DropdownLink href="/admin/courses" onClick={() => setOpenMenu(null)}>Cursos</DropdownLink>
                <DropdownLink href="/admin/classes" onClick={() => setOpenMenu(null)}>Aulas</DropdownLink>
                <div className="my-1 mx-1 border-t border-white/10" />
                <p className="px-3 pt-1 pb-1 text-[11px] font-medium uppercase tracking-wide text-white/40">
                  Entrega
                </p>
                <DropdownLink href="/admin/delivery-plans" onClick={() => setOpenMenu(null)}>Plano de entrega</DropdownLink>
              </div>
            )}
          </div>
        )}

        {isAdmin && (
          <Link href="/admin/notices" className="rounded px-2 py-1.5 text-white/70 hover:bg-white/5 hover:text-white">
            Enviar aviso
          </Link>
        )}
      </nav>

      <div className="relative" ref={menuRef}>
        {user ? (
          <>
            <button
              onClick={() => toggle("user")}
              className="flex items-center gap-2 rounded px-2 py-1.5 text-white/80 hover:bg-white/5"
            >
              <span className="material-icon rounded-full bg-white/10 p-1.5 text-base leading-none">person</span>
              <span className="max-w-[160px] truncate text-sm">{user.email}</span>
              <span className="material-icon text-base">arrow_drop_down</span>
            </button>
            {openMenu === "user" && (
              <div className="glass absolute right-0 top-full mt-1 w-56 p-2 text-white/80">
                <DropdownLink href="/me" onClick={() => setOpenMenu(null)}>Meu progresso</DropdownLink>
                <DropdownLink href="/notices" onClick={() => setOpenMenu(null)}>Avisos</DropdownLink>
                <DropdownLink href="/change-password" onClick={() => setOpenMenu(null)}>Trocar senha</DropdownLink>
                <div className="my-1 mx-1 border-t border-white/10" />
                <button
                  onClick={logout}
                  className="flex w-full items-center gap-2 rounded px-3 py-2 text-left text-sm hover:bg-white/5"
                >
                  <span className="material-icon text-base">logout</span> Sair
                </button>
              </div>
            )}
          </>
        ) : (
          <Link href="/login" className="btn btn-primary text-sm">
            <span className="material-icon text-base">login</span> Entrar
          </Link>
        )}
      </div>
    </header>
  );
}

function DropdownLink({
  href,
  onClick,
  children,
}: {
  href: string;
  onClick?: () => void;
  children: React.ReactNode;
}) {
  return (
    <Link
      href={href}
      onClick={onClick}
      className="flex items-center gap-2 rounded px-3 py-2 text-sm hover:bg-white/5"
    >
      {children}
    </Link>
  );
}
