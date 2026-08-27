"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";
import AppShell from "@/components/AppShell";
import Panel from "@/components/ui/Panel";
import Icon from "@/components/ui/Icon";

type SearchResult = { id: string; description: string; resultType: string; percentual: number };
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

  useEffect(() => {
    api<NoticeItem[]>("/api/notices")
      .then(setNotices)
      .catch(() => setNotices([]));
  }, []);

  async function search(event: React.FormEvent) {
    event.preventDefault();
    if (!term.trim()) return;
    setResults(await api<SearchResult[]>(`/api/search?q=${encodeURIComponent(term)}`));
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
              <li key={`${r.resultType}-${r.id}`} className="flex items-center justify-between py-3 text-sm">
                <span>
                  <span className="mr-2 rounded bg-white/10 px-1.5 py-0.5 text-xs text-white/70">
                    {typeLabels[r.resultType] ?? r.resultType}
                  </span>
                  {r.description}
                </span>
                {r.percentual > 0 && <span className="text-xs text-emerald-400">{r.percentual}%</span>}
              </li>
            ))}
          </ul>
        )}
      </Panel>

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
          {notices.length === 0 ? (
            <p className="text-sm text-white/50">Nenhum aviso por enquanto.</p>
          ) : (
            <ul className="space-y-2">
              {notices.map((n) => (
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
