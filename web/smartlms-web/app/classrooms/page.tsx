"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";
import Nav from "@/components/Nav";

type ClassroomItem = { id: string; name: string; courseCount: number; planCount: number };

export default function ClassroomsPage() {
  const [classrooms, setClassrooms] = useState<ClassroomItem[]>([]);
  const [name, setName] = useState("");
  const [error, setError] = useState<string | null>(null);

  async function reload() {
    setClassrooms(await api<ClassroomItem[]>("/api/classrooms"));
  }

  useEffect(() => {
    reload().catch((e) => setError(String(e)));
  }, []);

  async function create(event: React.FormEvent) {
    event.preventDefault();
    setError(null);
    try {
      await api("/api/classrooms", { method: "POST", body: JSON.stringify({ name }) });
      setName("");
      await reload();
    } catch {
      setError("Não foi possível criar a turma.");
    }
  }

  return (
    <main className="min-h-dvh bg-gray-50">
      <Nav />
      <div className="mx-auto max-w-3xl space-y-6 p-6">
        <h1 className="text-lg font-semibold">Turmas</h1>
        {error && <p className="rounded border border-red-200 bg-red-50 p-2 text-sm text-red-700">{error}</p>}

        <form onSubmit={create} className="flex gap-2">
          <input
            value={name}
            onChange={(e) => setName(e.target.value)}
            required
            placeholder="Nome da turma..."
            className="flex-1 rounded border bg-white px-3 py-2"
          />
          <button className="rounded bg-indigo-600 px-4 text-white hover:bg-indigo-700">Criar</button>
        </form>

        <ul className="divide-y rounded border bg-white">
          {classrooms.map((c) => (
            <li key={c.id} className="flex items-center justify-between p-3 text-sm">
              <span>{c.name}</span>
              <span className="text-xs text-gray-500">
                {c.courseCount} cursos · {c.planCount} planos
              </span>
            </li>
          ))}
          {classrooms.length === 0 && <li className="p-3 text-sm text-gray-500">Nenhuma turma.</li>}
        </ul>
      </div>
    </main>
  );
}
