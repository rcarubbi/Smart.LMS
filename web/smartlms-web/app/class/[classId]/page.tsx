"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import { useParams } from "next/navigation";
import Link from "next/link";
import { api, currentUser, errorMessage } from "@/lib/api";
import AppShell from "@/components/AppShell";
import Panel from "@/components/ui/Panel";
import Icon from "@/components/ui/Icon";
import IconButton from "@/components/ui/IconButton";
import { useToast } from "@/components/ui/Toast";

type ClassDetail = {
  id: string;
  name: string;
  content: string;
  order: number;
  deliveryDays: number;
  contentType: string;
  courseId: string;
  courseName: string;
  teacherId: string;
  files: { id: string; name: string; storagePath: string }[];
};

type CommentItem = { id: string; commentText: string; username: string; userId: string; dateTime: string };

function youtubeId(url: string): string {
  const m = url.match(/(?:youtu\.be\/|v=|\/embed\/)([\w-]{11})/);
  return m ? m[1] : url;
}

function vimeoId(url: string): string {
  const m = url.match(/vimeo\.com\/(\d+)/);
  return m ? m[1] : "";
}

export default function ClassPage() {
  const params = useParams<{ classId: string }>();
  const classId = params.classId;
  const { toast } = useToast();
  const user = currentUser();
  const [detail, setDetail] = useState<ClassDetail | null>(null);
  const [comments, setComments] = useState<CommentItem[]>([]);
  const [newComment, setNewComment] = useState("");
  const [elapsed, setElapsed] = useState(0);
  const timerRef = useRef<ReturnType<typeof setInterval> | null>(null);

  const load = useCallback(async () => {
    try {
      const d = await api<ClassDetail>(`/api/classes/${classId}`);
      setDetail(d);
    } catch (e) {
      toast("error", errorMessage(e, "Aula não encontrada."));
    }
  }, [classId, toast]);

  const loadComments = useCallback(async () => {
    try {
      setComments(await api<CommentItem[]>(`/api/classes/${classId}/comments`));
    } catch (e) {
      toast("error", errorMessage(e));
    }
  }, [classId, toast]);

  useEffect(() => {
    load();
    loadComments();
  }, [load, loadComments]);

  // Track watch time and periodically record progress.
  useEffect(() => {
    timerRef.current = setInterval(() => setElapsed((s) => s + 1), 1000);
    return () => {
      if (timerRef.current) clearInterval(timerRef.current);
    };
  }, []);

  async function recordProgress(percentual: number) {
    try {
      await api("/api/me/class-access", {
        method: "POST",
        body: JSON.stringify({ classId, percentual, watchedSeconds: elapsed }),
      });
    } catch (e) {
      toast("error", errorMessage(e));
    }
  }

  async function addComment(e: React.FormEvent) {
    e.preventDefault();
    if (!newComment.trim()) return;
    try {
      await api(`/api/classes/${classId}/comments`, {
        method: "POST",
        body: JSON.stringify({ commentText: newComment }),
      });
      setNewComment("");
      loadComments();
    } catch (err) {
      toast("error", errorMessage(err));
    }
  }

  async function deleteComment(id: string) {
    try {
      await api(`/api/classes/${classId}/comments/${id}`, { method: "DELETE" });
      loadComments();
    } catch (e) {
      toast("error", errorMessage(e));
    }
  }

  function embed() {
    if (!detail) return null;
    if (detail.contentType === "Vimeo") {
      const id = vimeoId(detail.content);
      if (!id) return <a href={detail.content} target="_blank" rel="noreferrer" className="text-indigo-300 underline">{detail.content}</a>;
      return (
        <iframe
          src={`https://player.vimeo.com/video/${id}`}
          className="aspect-video w-full"
          allow="autoplay; fullscreen; picture-in-picture"
          allowFullScreen
          frameBorder={0}
          title={detail.name}
        />
      );
    }
    const id = youtubeId(detail.content);
    const src = id === detail.content ? detail.content : `https://www.youtube.com/embed/${id}`;
    return (
      <iframe
        src={src}
        className="aspect-video w-full"
        allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
        allowFullScreen
        frameBorder={0}
        title={detail.name}
      />
    );
  }

  if (!detail) {
    return (
      <AppShell>
        <p className="text-sm text-white/50">Carregando aula...</p>
      </AppShell>
    );
  }

  return (
    <AppShell>
      <Link href={`/course/${detail.courseId}`} className="mb-3 inline-flex items-center gap-1 text-sm text-indigo-300 hover:underline">
        <Icon name="arrow_back" className="text-base" /> {detail.courseName}
      </Link>

      <div className="grid gap-6 lg:grid-cols-3">
        <div className="space-y-4 lg:col-span-2">
          <Panel icon="play_circle" title={detail.name} accent="#f59e0b">
            <div className="overflow-hidden rounded-xl border border-white/10">{embed()}</div>
            <div className="mt-4 flex items-center justify-between">
              <span className="text-xs text-white/40">Tempo de exibição: {Math.floor(elapsed / 60)}min {elapsed % 60}s</span>
              <div className="flex gap-2">
                <button className="btn btn-ghost text-sm" onClick={() => recordProgress(50)}>
                  <Icon name="bookmark_border" /> Marcar 50%
                </button>
                <button className="btn btn-primary text-sm" onClick={() => recordProgress(100)}>
                  <Icon name="check" /> Concluir aula
                </button>
              </div>
            </div>
          </Panel>

          <Panel icon="attach_file" title="Arquivos de apoio" accent="#3b82f6">
            {detail.files.length === 0 ? (
              <p className="text-sm text-white/50">Nenhum arquivo anexado.</p>
            ) : (
              <ul className="space-y-2">
                {detail.files.map((f) => (
                  <li key={f.id} className="flex items-center gap-3 rounded-lg border border-white/10 bg-white/5 p-2.5 text-sm">
                    <Icon name="description" className="text-sky-400" />
                    <span className="flex-1">{f.name}</span>
                    {f.storagePath && (
                      <a href={f.storagePath} target="_blank" rel="noreferrer" className="text-indigo-300 hover:underline">
                        Abrir
                      </a>
                    )}
                  </li>
                ))}
              </ul>
            )}
          </Panel>
        </div>

        <Panel icon="forum" title="Comentários" accent="#10b981">
          <form onSubmit={addComment} className="mb-4 flex gap-2">
            <input
              value={newComment}
              onChange={(e) => setNewComment(e.target.value)}
              placeholder="Escreva um comentário..."
              className="field"
            />
            <button type="submit" className="btn btn-primary">
              <Icon name="send" />
            </button>
          </form>
          {comments.length === 0 ? (
            <p className="text-sm text-white/50">Seja o primeiro a comentar.</p>
          ) : (
            <ul className="max-h-96 space-y-3 overflow-y-auto">
              {comments.map((c) => (
                <li key={c.id} className="rounded-lg border border-white/10 bg-white/5 p-3 text-sm">
                  <div className="mb-1 flex items-center justify-between">
                    <span className="font-medium text-white/80">{c.username}</span>
                    <div className="flex items-center gap-2">
                      <span className="text-xs text-white/40">{new Date(c.dateTime).toLocaleString("pt-BR")}</span>
                      {(user?.sub === c.userId || user?.role === "Admin") && (
                        <IconButton name="delete" title="Excluir" danger onClick={() => deleteComment(c.id)} />
                      )}
                    </div>
                  </div>
                  <p className="text-white/80">{c.commentText}</p>
                </li>
              ))}
            </ul>
          )}
        </Panel>
      </div>
    </AppShell>
  );
}
