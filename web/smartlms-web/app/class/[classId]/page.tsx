"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import { useParams } from "next/navigation";
import Link from "next/link";
import { api, apiList, currentUser, errorMessage } from "@/lib/api";
import { usePagedList } from "@/lib/usePagedList";
import AppShell from "@/components/AppShell";
import Panel from "@/components/ui/Panel";
import Icon from "@/components/ui/Icon";
import IconButton from "@/components/ui/IconButton";
import Pagination from "@/components/ui/Pagination";
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

type ProgressItem = { classId: string; className: string; courseName: string; percentual: number; watchedSeconds: number; lastAccessAtUtc: string };

function youtubeId(url: string): string {
  const m = url.match(/(?:youtu\.be\/|v=|\/embed\/)([\w-]{11})/);
  return m ? m[1] : "";
}

function vimeoId(url: string): string {
  const m = url.match(/vimeo\.com\/(\d+)/);
  return m ? m[1] : "";
}

function loadScript(src: string): Promise<void> {
  return new Promise((resolve, reject) => {
    if (document.querySelector(`script[src="${src}"]`)) {
      resolve();
      return;
    }
    const script = document.createElement("script");
    script.src = src;
    script.async = true;
    script.onload = () => resolve();
    script.onerror = () => reject(new Error(`Falha ao carregar o player (${src}).`));
    document.head.appendChild(script);
  });
}

function waitForYouTube(): Promise<any> {
  return new Promise((resolve, reject) => {
    const started = Date.now();
    const poll = () => {
      const yt = (window as any).YT;
      if (yt?.Player) {
        resolve(yt);
        return;
      }
      if (Date.now() - started > 15000) {
        reject(new Error("Tempo esgotado ao carregar o YouTube."));
        return;
      }
      setTimeout(poll, 200);
    };
    poll();
  });
}

export default function ClassPage() {
  const params = useParams<{ classId: string }>();
  const classId = params.classId;
  const { toast } = useToast();
  const user = currentUser();
  const [detail, setDetail] = useState<ClassDetail | null>(null);
  const { items: comments, total: commentsTotal, page: commentsPage, setPage: setCommentsPage, reload } = usePagedList<CommentItem>(
    `/api/classes/${classId}/comments`,
    {},
  );
  const [newComment, setNewComment] = useState("");
  const [watchPercent, setWatchPercent] = useState(0);
  const [playerReady, setPlayerReady] = useState(false);

  const playerRef = useRef<any>(null);
  const vimeoRef = useRef<HTMLIFrameElement | null>(null);
  const resumeSecondsRef = useRef(0);
  const heartbeatRef = useRef<ReturnType<typeof setInterval> | null>(null);
  const initedRef = useRef(false);
  const lastDurationRef = useRef(0);

  const load = useCallback(async () => {
    try {
      setDetail(await api<ClassDetail>(`/api/classes/${classId}`));
    } catch (e) {
      toast("error", errorMessage(e, "Aula não encontrada."));
    }
  }, [classId, toast]);

  useEffect(() => {
    load();
  }, [load]);

  function recordProgress(percentual: number, watchedSeconds: number) {
    api("/api/me/class-access", {
      method: "POST",
      body: JSON.stringify({ classId, percentual, watchedSeconds }),
    }).catch(() => {
      // Silently ignore: progress is best-effort and retried on the next heartbeat.
    });
  }

  useEffect(() => {
    let disposed = false;
    apiList<ProgressItem>(`/api/me/progress?classId=${classId}`)
      .then((items) => {
        if (disposed) return;
        const item = items?.[0];
        if (item) {
          setWatchPercent(item.percentual);
          if (item.percentual < 100) resumeSecondsRef.current = item.watchedSeconds;
        }
      })
      .catch(() => undefined);
    return () => {
      disposed = true;
    };
  }, [classId]);

  // Initializes the provider player (YouTube IFrame API / Vimeo Player.js).
  useEffect(() => {
    if (!detail || initedRef.current) return;
    initedRef.current = true;
    let disposed = false;
    const teardown = () => {
      disposed = true;
      if (heartbeatRef.current) clearInterval(heartbeatRef.current);
      heartbeatRef.current = null;
      if (playerRef.current) {
        try {
          if (typeof playerRef.current.destroy === "function") playerRef.current.destroy();
        } catch {
          // ignore
        }
        playerRef.current = null;
      }
      initedRef.current = false;
    };

    if (detail.contentType === "Vimeo") {
      const id = vimeoId(detail.content);
      if (!id) return teardown;
      loadScript("https://player.vimeo.com/api/player.js")
        .then(() => {
          if (disposed || !vimeoRef.current) return;
          const Vimeo = (window as any).Vimeo;
          const player = new Vimeo.Player(vimeoRef.current);
          playerRef.current = player;

          player.on("loaded", () => {
            if (disposed) return;
            setPlayerReady(true);
            if (resumeSecondsRef.current > 5) {
              player.setCurrentTime(resumeSecondsRef.current).catch(() => undefined);
            }
          });

          player.on("timeupdate", (data: { seconds: number; duration: number }) => {
            if (disposed) return;
            const duration = data.duration || 0;
            lastDurationRef.current = duration;
            if (duration <= 0) return;
            const pct = Math.min(99, Math.round((data.seconds / duration) * 100));
            setWatchPercent((prev) => Math.max(prev, pct));
            setPlayerReady(true);
            recordProgress(pct, Math.round(data.seconds));
          });

          player.on("ended", () => {
            if (disposed) return;
            setWatchPercent(100);
            recordProgress(100, Math.round(lastDurationRef.current));
          });
        })
        .catch((e) => toast("error", e.message));
      return teardown;
    }

    // YouTube
    const id = youtubeId(detail.content);
    if (!id) return teardown;

    loadScript("https://www.youtube.com/iframe_api")
      .then(waitForYouTube)
      .then((yt) => {
        if (disposed) return;
        const host = document.getElementById("smartlms-youtube-host");
        if (!host) return;

        const player = new yt.Player("smartlms-youtube-host", {
          videoId: id,
          playerVars: { rel: 0, enablejsapi: 1 },
          events: {
            onReady: () => {
              if (disposed) return;
              const iframe = host.querySelector("iframe");
              if (iframe) {
                iframe.classList.add("aspect-video", "w-full", "border-0");
              }
              setPlayerReady(true);
              if (resumeSecondsRef.current > 5) {
                player.seekTo(resumeSecondsRef.current, true);
              }
            },
            onStateChange: (e: any) => {
              if (disposed) return;
              if (e.data === yt.PlayerState.ENDED) {
                setWatchPercent(100);
                recordProgress(100, Math.round(player.getDuration?.() ?? 0));
              }
            },
          },
        });
        playerRef.current = player;

        // Heartbeat while the player is active: percent always capped at 99;
        // 100% is recorded only by the ENDED event (conclusion requires the end of the video).
        heartbeatRef.current = setInterval(() => {
          if (disposed) return;
          try {
            const current = player.getCurrentTime?.() ?? 0;
            const duration = player.getDuration?.() ?? 0;
            lastDurationRef.current = duration;
            if (duration <= 0) return;
            const pct = Math.min(99, Math.round((current / duration) * 100));
            setWatchPercent((prev) => Math.max(prev, pct));
            recordProgress(pct, Math.round(current));
          } catch {
            // ignore
          }
        }, 5000);
      })
      .catch((e) => toast("error", e.message));
    return teardown;
  }, [detail, classId, toast]);

  async function addComment(e: React.FormEvent) {
    e.preventDefault();
    if (!newComment.trim()) return;
    try {
      await api(`/api/classes/${classId}/comments`, {
        method: "POST",
        body: JSON.stringify({ commentText: newComment }),
      });
      setNewComment("");
      reload();
    } catch (err) {
      toast("error", errorMessage(err));
    }
  }

  async function deleteComment(id: string) {
    try {
      await api(`/api/classes/${classId}/comments/${id}`, { method: "DELETE" });
      reload();
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
          ref={vimeoRef}
          src={`https://player.vimeo.com/video/${id}`}
          className="aspect-video w-full"
          allow="autoplay; fullscreen; picture-in-picture"
          allowFullScreen
          frameBorder={0}
          title={detail.name}
        />
      );
    }
    if (!youtubeId(detail.content)) {
      return <a href={detail.content} target="_blank" rel="noreferrer" className="text-indigo-300 underline">{detail.content}</a>;
    }
    return <div id="smartlms-youtube-host" className="aspect-video w-full" />;
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
            <div className="mt-4">
              <div className="h-2 overflow-hidden rounded-full bg-white/10">
                <div
                  className="h-2 rounded-full bg-gradient-to-r from-indigo-500 to-sky-400 transition-[width] duration-300"
                  style={{ width: `${watchPercent}%` }}
                />
              </div>
              <div className="mt-1.5 flex items-center justify-between text-xs">
                <span className="text-white/40">{watchPercent}% assistido</span>
                {watchPercent >= 100 ? (
                  <span className="inline-flex items-center gap-1 font-medium text-emerald-400">
                    <Icon name="check_circle" className="text-sm" /> Concluída
                  </span>
                ) : playerReady ? (
                  <span className="text-white/40">Assista até o fim para concluir</span>
                ) : null}
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
                        <IconButton name="delete" title="Excluir" variant="danger" onClick={() => deleteComment(c.id)} />
                      )}
                    </div>
                  </div>
                  <p className="text-white/80">{c.commentText}</p>
                </li>
              ))}
            </ul>
          )}
          <Pagination page={commentsPage} total={commentsTotal} pageSize={25} onChange={setCommentsPage} />
        </Panel>
      </div>
    </AppShell>
  );
}