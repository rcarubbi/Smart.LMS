"use client";

import { useState } from "react";
import { api, errorMessage } from "@/lib/api";
import ConfirmModal from "@/components/ui/ConfirmModal";
import { useToast } from "@/components/ui/Toast";

type Trashable = { id: string };

/**
 * Shared trash-cycle for admin list pages: delete (soft), purge and restore actions
 * plus the two confirm modals. `basePath` is the REST prefix; the hook appends
 * /{id}, /{id}/permanent and /{id}/restore.
 */
export function useTrashConfirmations<T extends Trashable>({
  reload,
  basePath,
  labels,
  messages,
}: {
  reload: () => Promise<void> | void;
  basePath: string;
  labels: { trash: string; restore: string; purge: string };
  messages: { trash: (item: T) => React.ReactNode; purge: (item: T) => React.ReactNode };
}) {
  const { toast } = useToast();
  const [deleting, setDeleting] = useState<T | null>(null);
  const [purging, setPurging] = useState<T | null>(null);

  async function confirmDelete() {
    if (!deleting) return;
    try {
      await api(`${basePath}/${deleting.id}`, { method: "DELETE" });
      toast("success", labels.trash);
      setDeleting(null);
      reload();
    } catch (e) {
      toast("error", errorMessage(e));
    }
  }

  async function confirmPurge() {
    if (!purging) return;
    try {
      await api(`${basePath}/${purging.id}/permanent`, { method: "DELETE" });
      toast("success", labels.purge);
      setPurging(null);
      reload();
    } catch (e) {
      toast("error", errorMessage(e));
    }
  }

  async function restore(item: T) {
    try {
      await api(`${basePath}/${item.id}/restore`, { method: "POST" });
      toast("success", labels.restore);
      reload();
    } catch (e) {
      toast("error", errorMessage(e));
    }
  }

  const modals = (
    <>
      <ConfirmModal
        open={!!deleting}
        title="Mover para a lixeira"
        message={deleting ? messages.trash(deleting) : null}
        onConfirm={confirmDelete}
        onCancel={() => setDeleting(null)}
      />
      <ConfirmModal
        open={!!purging}
        title="Excluir definitivamente"
        message={purging ? messages.purge(purging) : null}
        onConfirm={confirmPurge}
        onCancel={() => setPurging(null)}
      />
    </>
  );

  return { deleting, setDeleting, purging, setPurging, restore, modals };
}