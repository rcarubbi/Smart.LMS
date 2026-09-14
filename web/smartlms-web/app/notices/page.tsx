"use client";

import { api } from "@/lib/api";
import { usePagedList } from "@/lib/usePagedList";
import AppShell from "@/components/AppShell";
import Panel from "@/components/ui/Panel";
import Icon from "@/components/ui/Icon";
import Pagination from "@/components/ui/Pagination";

type NoticeItem = { id: string; text: string; dateTime: string; seen: boolean };

export default function NoticesPage() {
  const { items: notices, loading, page, total, setPage, setItems } = usePagedList<NoticeItem>("/api/notices", {});

  async function markSeen(id: string) {
    await api(`/api/notices/${id}/seen`, { method: "POST" });
    setItems((current) => current.map((n) => (n.id === id ? { ...n, seen: true } : n)));
  }

  async function markAllSeen() {
    const unseen = notices.filter((n) => !n.seen);
    if (unseen.length === 0) return;
    await Promise.all(unseen.map((n) => api(`/api/notices/${n.id}/seen`, { method: "POST" })));
    setItems((current) => current.map((n) => (n.seen ? n : { ...n, seen: true })));
  }

  return (
    <AppShell>
      <Panel
        icon="notifications"
        title="Avisos"
        accent="#f59e0b"
        action={
          notices.some((n) => !n.seen) ? (
            <button className="btn btn-ghost text-sm" onClick={markAllSeen}>
              <Icon name="done_all" /> Marcar todos como lido
            </button>
          ) : null
        }
      >
        {loading ? (
          <p className="text-sm text-white/50">Carregando...</p>
        ) : notices.length === 0 ? (
          <p className="text-sm text-white/50">Nenhum aviso por enquanto.</p>
        ) : (
          <ul className="space-y-2">
            {notices.map((n) => (
              <li
                key={n.id}
                className={`flex items-start justify-between gap-4 rounded-lg border p-3 text-sm ${
                  n.seen ? "border-white/10 bg-white/5" : "border-amber-500/30 bg-amber-500/5"
                }`}
              >
                <div>
                  <div
                    className="[&_a]:text-indigo-300"
                    dangerouslySetInnerHTML={{ __html: n.text }}
                  />
                  <div className="mt-1 text-xs text-white/40">
                    {new Date(n.dateTime).toLocaleString("pt-BR")}
                  </div>
                </div>
                {!n.seen && (
                  <button onClick={() => markSeen(n.id)} className="btn btn-ghost shrink-0 text-xs">
                    <Icon name="done" /> Marcar como lido
                  </button>
                )}
              </li>
            ))}
          </ul>
        )}
        <Pagination page={page} total={total} pageSize={25} onChange={setPage} />
      </Panel>
    </AppShell>
  );
}
