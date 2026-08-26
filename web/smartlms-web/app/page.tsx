"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";
import Nav from "@/components/Nav";

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
    setNotices((current) =>
      current.map((n) => (n.id === id ? { ...n, seen: true } : n)),
    );
  }

  return (
    <main className="min-h-dvh bg-gray-50">
      <Nav />
      <div className="mx-auto max-w-3xl space-y-8 p-6">
        <section className="space-y-3">
          <h1 className="text-lg font-semibold">Busca contextual</h1>
          <form onSubmit={search} className="flex gap-2">
            <input
              value={term}
              onChange={(e) => setTerm(e.target.value)}
              placeholder="Cursos, aulas, assuntos..."
              className="flex-1 rounded border bg-white px-3 py-2"
            />
            <button className="rounded bg-indigo-600 px-4 py-2 text-white hover:bg-indigo-700">
              Buscar
            </button>
          </form>

          {results && (
            <ul className="divide-y rounded border bg-white">
              {results.length === 0 && (
                <li className="p-3 text-sm text-gray-500">Nenhum resultado.</li>
              )}
              {results.map((r) => (
                <li key={`${r.resultType}-${r.id}`} className="flex items-center justify-between p-3 text-sm">
                  <span>
                    <span className="mr-2 rounded bg-gray-100 px-1.5 py-0.5 text-xs text-gray-600">
                      {typeLabels[r.resultType] ?? r.resultType}
                    </span>
                    {r.description}
                  </span>
                  {r.percentual > 0 && <span className="text-xs text-green-600">{r.percentual}%</span>}
                </li>
              ))}
            </ul>
          )}
        </section>

        <section className="space-y-3">
          <h2 className="text-lg font-semibold">Avisos recentes</h2>
          {notices.length === 0 ? (
            <p className="text-sm text-gray-500">Nenhum aviso por enquanto.</p>
          ) : (
            <ul className="space-y-2">
              {notices.map((n) => (
                <li key={n.id} className="flex items-start justify-between gap-4 rounded border bg-white p-3 text-sm">
                  <div
                    className="[&_a]:text-indigo-600"
                    dangerouslySetInnerHTML={{ __html: n.text }}
                  />
                  {!n.seen && (
                    <button
                      onClick={() => markSeen(n.id)}
                      className="shrink-0 rounded border px-2 py-1 text-xs hover:bg-gray-100"
                    >
                      Marcar como lido
                    </button>
                  )}
                </li>
              ))}
            </ul>
          )}
        </section>
      </div>
    </main>
  );
}
