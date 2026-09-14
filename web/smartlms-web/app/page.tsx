"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { api, apiList } from "@/lib/api";
import type { ListItem } from "@/lib/api";
import AppShell from "@/components/AppShell";
import Panel from "@/components/ui/Panel";
import Icon from "@/components/ui/Icon";

type SearchResult = { id: string; description: string; resultType: string; percentual: number; link?: string | null };
type NoticeItem = { id: string; text: string; dateTime: string; seen: boolean };

const typeLabels: Record<string, string> = {
  KnowledgeArea: "Área",
  Subject: "Assunto",
  Course: "Curso",
  Class: "Aula",
  File: "Arquivo",
};

export default function HomePage() {
  const [term, setTerm] = useState("");
  const [results, setResults] = useState<SearchResult[] | null>(null);
  const [notices, setNotices] = useState<NoticeItem[]>([]);
  const [areas, setAreas] = useState<ListItem[]>([]);

  useEffect(() => {
    apiList<NoticeItem>("/api/notices")
      .then(setNotices)
      .catch(() => setNotices([]));
    apiList<ListItem>("/api/knowledge-areas?active=true")
      .then(setAreas)
      .catch(() => setAreas([]));
  }, []);

  async function search(event: React.FormEvent) {
    event.preventDefault();
    if (!term.trim()) return;
    setResults(await apiList<SearchResult>(`/api/search?q=${encodeURIComponent(term)}`));
  }

  async function markSeen(id: string) {
    await api(`/api/notices/${id}/seen`, { method: "POST" });
    setNotices((current) => current.map((n) => (n.id === id ? { ...n, seen: true } : n)));
  }

  return (
    <AppShell>
      <Panel
        title={
          <span className="flex items-center gap-2">
            <span className="material-icon" style={{ color: "#6366f1" }}>search</span>
            Busca contextual
          </span>
        }
      >
        <form onSubmit={search} className="flex gap-2">
          <input
            value={term}
            onChange={(e) => setTerm(e.target.value)}
            placeholder="Cursos, aulas, assuntos..."
            className="field"
          />
          <button type="submit" className="btn btn-primary">
            <Icon name="search" /> Buscar
          </button>
        </form>

        {results && (
          <ul className="mt-4 divide-y divide-white/10">
            {results.length === 0 && <li className="py-3 text-sm text-white/50">Nenhum resultado.</li>}
            {results.map((r) => (
              <li key={`${r.resultType}-${r.id}`} className="flex items-center justify-between gap-3 py-3 text-sm">
                <span className="flex min-w-0 items-center gap-2">
                  <span className="mr-1 shrink-0 rounded bg-white/10 px-1.5 py-0.5 text-xs text-white/70">
                    {typeLabels[r.resultType] ?? r.resultType}
                  </span>
                  {r.resultType === "File" && r.link ? (
                    <a href={r.link} target="_blank" rel="noreferrer" className="truncate text-indigo-300 hover:underline">
                      {r.description}
                    </a>
                  ) : r.link ? (
                    <a href={r.link} className="truncate text-indigo-300 hover:underline">
                      {r.description}
                    </a>
                  ) : (
                    <span className="truncate">{r.description}</span>
                  )}
                </span>
                {r.percentual > 0 && <span className="shrink-0 text-xs text-emerald-400">{r.percentual}%</span>}
              </li>
            ))}
          </ul>
        )}
      </Panel>

      <div className="mt-6">
        <Panel
          title={
            <span className="flex items-center gap-2">
              <span className="material-icon" style={{ color: "#6366f1" }}>school</span>
              Áreas de conhecimento
            </span>
          }
        >
          {areas.length === 0 ? (
            <p className="text-sm text-white/50">Nenhuma área cadastrada.</p>
          ) : (
            <div className="grid gap-2 sm:grid-cols-2 lg:grid-cols-3">
              {areas.map((a) => (
                <Link
                  key={a.id}
                  href={`/areas?area=${a.id}`}
                  className="group flex items-center justify-between rounded-lg border border-white/10 bg-white/5 px-4 py-3 text-sm text-white/80 transition hover:border-white/20 hover:bg-white/10 hover:text-white"
                >
                  <span>{a.name}</span>
                  <Icon name="chevron_right" />
                </Link>
              ))}
            </div>
          )}
        </Panel>
      </div>

      <div className="mt-6">
        <Panel
          title={
            <span className="flex items-center gap-2">
              <span className="material-icon" style={{ color: "#f59e0b" }}>notifications</span>
              Avisos recentes
            </span>
          }
          accent="#f59e0b"
        >
          {notices.filter((n) => !n.seen).length === 0 ? (
            <p className="text-sm text-white/50">Nenhum aviso por enquanto.</p>
          ) : (
            <ul className="space-y-2">
              {notices
                .filter((n) => !n.seen)
                .map((n) => (
                <li key={n.id} className="flex items-start justify-between gap-4 rounded-lg border border-white/10 bg-white/5 p-3 text-sm">
                  <div className="[&_a]:text-indigo-300" dangerouslySetInnerHTML={{ __html: n.text }} />
                  {!n.seen && (
                    <button
                      onClick={() => markSeen(n.id)}
                      className="btn btn-ghost shrink-0 text-xs"
                    >
                      Marcar como lido
                    </button>
                  )}
                </li>
              ))}
            </ul>
          )}
        </Panel>
      </div>
    </AppShell>
  );
}
