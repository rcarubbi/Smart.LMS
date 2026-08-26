"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";
import Nav from "@/components/Nav";

type ListItem = { id: string; name: string; order: number };
type CourseItem = { id: string; name: string; order: number; subjectId: string; classCount: number };

export default function CatalogPage() {
  const [areas, setAreas] = useState<ListItem[]>([]);
  const [subjects, setSubjects] = useState<ListItem[]>([]);
  const [courses, setCourses] = useState<CourseItem[]>([]);
  const [selectedArea, setSelectedArea] = useState<string | null>(null);
  const [newAreaName, setNewAreaName] = useState("");
  const [error, setError] = useState<string | null>(null);

  async function reload(areaId: string | null) {
    setAreas(await api<ListItem[]>("/api/knowledge-areas"));
    setSubjects(
      await api<ListItem[]>(`/api/subjects${areaId ? `?knowledgeAreaId=${areaId}` : ""}`),
    );
    setCourses(await api<CourseItem[]>("/api/courses"));
  }

  useEffect(() => {
    reload(null).catch((e) => setError(String(e)));
  }, []);

  async function createArea(event: React.FormEvent) {
    event.preventDefault();
    setError(null);
    try {
      await api("/api/knowledge-areas", {
        method: "POST",
        body: JSON.stringify({ name: newAreaName, order: areas.length + 1 }),
      });
      setNewAreaName("");
      await reload(selectedArea);
    } catch {
      setError("Não foi possível criar a área (nome duplicado?).");
    }
  }

  async function deleteArea(id: string) {
    try {
      await api(`/api/knowledge-areas/${id}`, { method: "DELETE" });
      await reload(selectedArea === id ? null : selectedArea);
    } catch {
      setError("Remova os assuntos antes de excluir a área.");
    }
  }

  return (
    <main className="min-h-dvh bg-gray-50">
      <Nav />
      <div className="mx-auto max-w-4xl space-y-8 p-6">
        <h1 className="text-lg font-semibold">Catálogo</h1>
        {error && <p className="rounded border border-red-200 bg-red-50 p-2 text-sm text-red-700">{error}</p>}

        <section className="space-y-3">
          <h2 className="font-medium">Áreas de conhecimento</h2>
          <form onSubmit={createArea} className="flex gap-2">
            <input
              value={newAreaName}
              onChange={(e) => setNewAreaName(e.target.value)}
              required
              placeholder="Nova área..."
              className="flex-1 rounded border bg-white px-3 py-2"
            />
            <button className="rounded bg-indigo-600 px-4 text-white hover:bg-indigo-700">Criar</button>
          </form>
          <ul className="divide-y rounded border bg-white">
            {areas.map((a) => (
              <li
                key={a.id}
                className={`flex items-center justify-between p-3 text-sm ${selectedArea === a.id ? "bg-indigo-50" : ""}`}
              >
                <button
                  onClick={() => {
                    const next = selectedArea === a.id ? null : a.id;
                    setSelectedArea(next);
                    reload(next);
                  }}
                  className="text-left"
                >
                  {a.name}
                </button>
                <button onClick={() => deleteArea(a.id)} className="text-xs text-red-600">
                  Excluir
                </button>
              </li>
            ))}
          </ul>
        </section>

        <section className="space-y-3">
          <h2 className="font-medium">Assuntos{selectedArea ? " (da área selecionada)" : ""}</h2>
          <ul className="divide-y rounded border bg-white">
            {subjects.map((s) => (
              <li key={s.id} className="p-3 text-sm">
                {s.name}
              </li>
            ))}
            {subjects.length === 0 && <li className="p-3 text-sm text-gray-500">Nenhum assunto.</li>}
          </ul>
        </section>

        <section className="space-y-3">
          <h2 className="font-medium">Cursos</h2>
          <ul className="divide-y rounded border bg-white">
            {courses.map((c) => (
              <li key={c.id} className="flex items-center justify-between p-3 text-sm">
                <span>{c.name}</span>
                <span className="text-xs text-gray-500">{c.classCount} aulas</span>
              </li>
            ))}
            {courses.length === 0 && <li className="p-3 text-sm text-gray-500">Nenhum curso.</li>}
          </ul>
        </section>
      </div>
    </main>
  );
}
