"use client";

import { api, errorMessage } from "@/lib/api";
import { usePagedList } from "@/lib/usePagedList";
import AppShell from "@/components/AppShell";
import AuthGuard from "@/components/AuthGuard";
import Panel from "@/components/ui/Panel";
import Icon from "@/components/ui/Icon";
import Pagination from "@/components/ui/Pagination";
import { useToast } from "@/components/ui/Toast";

type DeadLetter = { id: number; type: string; createdAt: string; attempts: number; error: string | null };

export default function OutboxAdminPage() {
  const { toast } = useToast();
  const { items, total, loading, page, setPage, reload } = usePagedList<DeadLetter>("/api/outbox/dead-letters", {});

  async function retry(id: number) {
    try {
      await api(`/api/outbox/dead-letters/${id}/retry`, { method: "POST" });
      toast("success", "Mensagem adicionada novamente à fila.");
      reload();
    } catch (error) {
      toast("error", errorMessage(error));
    }
  }

  return (
    <AuthGuard roles={["Admin"]}>
      <AppShell>
        <Panel icon="mark_email_unread" title="Falhas de e-mail" accent="#f97316">
          <p className="mb-4 text-sm text-white/50">Falhas tentam novamente com espera crescente. Após cinco tentativas, revise e reenvie manualmente.</p>
          {loading ? <p className="text-sm text-white/50">Carregando...</p> : items.length === 0 ? (
            <p className="text-sm text-white/50">Nenhuma mensagem aguardando análise.</p>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-left text-sm">
                <thead><tr className="border-b border-white/10 text-xs uppercase text-white/40">
                  <th className="py-2 pr-3">Tipo</th><th className="py-2 pr-3">Criada</th><th className="py-2 pr-3">Tentativas</th><th className="py-2 pr-3">Erro</th><th className="py-2 text-right">Ação</th>
                </tr></thead>
                <tbody className="divide-y divide-white/5">{items.map((item) => (
                  <tr key={item.id}>
                    <td className="py-2 pr-3">{item.type}</td>
                    <td className="py-2 pr-3 text-white/50">{new Date(item.createdAt).toLocaleString("pt-BR")}</td>
                    <td className="py-2 pr-3">{item.attempts}</td>
                    <td className="max-w-md truncate py-2 pr-3 text-amber-200/80" title={item.error ?? ""}>{item.error}</td>
                    <td className="py-2 text-right"><button className="btn btn-ghost text-xs" onClick={() => retry(item.id)}>
                      <Icon name="refresh" /> Reenviar
                    </button></td>
                  </tr>
                ))}</tbody>
              </table>
            </div>
          )}
          <Pagination page={page} total={total} pageSize={25} onChange={setPage} />
        </Panel>
      </AppShell>
    </AuthGuard>
  );
}
