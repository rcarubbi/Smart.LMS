"use client";

import { Suspense, useEffect, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { api, apiList, errorMessage } from "@/lib/api";
import { usePagedList } from "@/lib/usePagedList";
import { useTrashConfirmations } from "@/lib/useTrashConfirmations";
import AppShell from "@/components/AppShell";
import AuthGuard from "@/components/AuthGuard";
import Panel from "@/components/ui/Panel";
import FormModal from "@/components/ui/FormModal";
import Icon from "@/components/ui/Icon";
import IconButton from "@/components/ui/IconButton";
import Pagination from "@/components/ui/Pagination";
import ViewTabs, { type ViewFilter } from "@/components/ui/ViewTabs";
import { useToast } from "@/components/ui/Toast";

type Course = { id: string; name: string };
type AssessmentItem = { id: string; courseName: string; minimumScore: number; questionCount: number; active: boolean };

type EditOption = { id: string; text: string; order: number };
type EditQuestion = {
  id: string;
  type: "SingleChoice" | "MultipleChoice" | "Reordering";
  description: string;
  score: number;
  options: EditOption[];
  singleChoiceRightOrder?: number;
  multipleChoiceRightOrders: number[];
};
type EditDetail = { id: string; courseId: string; minimumScore: number; questions: EditQuestion[] };

type QDraft = {
  type: "SingleChoice" | "MultipleChoice" | "Reordering";
  description: string;
  score: string;
  options: string[];
  singleRight?: number;
  multipleRight: number[];
};

export default function AssessmentsAdminPage() {
  return (
    <Suspense fallback={null}>
      <AssessmentsInner />
    </Suspense>
  );
}

function AssessmentsInner() {
  const router = useRouter();
  const search = useSearchParams();
  const courseParam = search.get("course") ?? "";
  const { toast } = useToast();

  const [courses, setCourses] = useState<Course[]>([]);
  const [courseId, setCourseId] = useState(courseParam);
  const [view, setView] = useState<ViewFilter>("active");
  const { items: assessments, total, loading, page, setPage, reload } = usePagedList<AssessmentItem>(
    courseId ? `/api/courses/${courseId}/assessments` : "",
    { active: view === "all" ? undefined : view === "active" },
  );

  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<AssessmentItem | null>(null);
  const [minimumScore, setMinimumScore] = useState("0");
  const [questions, setQuestions] = useState<QDraft[]>([]);
  const [saving, setSaving] = useState(false);
  const { setDeleting, setPurging, restore, modals } = useTrashConfirmations<AssessmentItem>({
    reload,
    basePath: "/api/assessments",
    labels: {
      trash: "Avaliação movida para a lixeira.",
      restore: "Avaliação restaurada.",
      purge: "Avaliação excluída definitivamente.",
    },
    messages: {
      trash: () => "Excluir esta avaliação? Ela ficará na lixeira e pode ser restaurada.",
      purge: () => "Esta ação não pode ser desfeita. Excluir esta avaliação para sempre?",
    },
  });

  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const cs = await apiList<Course>("/api/courses");
        if (cancelled) return;
        setCourses(cs);
        if (cs.length > 0) setCourseId((cur) => cur || cs[0].id);
      } catch (e) {
        toast("error", errorMessage(e));
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [toast]);

  function openNew() {
    setEditing(null);
    setMinimumScore("0");
    setQuestions([]);
    setModalOpen(true);
  }

  async function openEdit(item: AssessmentItem) {
    try {
      const detail = await api<EditDetail>(`/api/assessments/${item.id}/edit`);
      setEditing(item);
      setMinimumScore(String(detail.minimumScore));
      setQuestions(
        detail.questions.map((q) => ({
          type: q.type,
          description: q.description,
          score: String(q.score),
          options: [...q.options].sort((a, b) => a.order - b.order).map((o) => o.text),
          singleRight: q.singleChoiceRightOrder,
          multipleRight: [...q.multipleChoiceRightOrders],
        })),
      );
      setModalOpen(true);
    } catch (e) {
      toast("error", errorMessage(e));
    }
  }

  function addQuestion() {
    setQuestions((q) => [...q, { type: "SingleChoice", description: "", score: "1", options: ["", ""], multipleRight: [] }]);
  }

  function updateQuestion(i: number, patch: Partial<QDraft>) {
    setQuestions((qs) => qs.map((q, idx) => (idx === i ? { ...q, ...patch } : q)));
  }

  function removeQuestion(i: number) {
    setQuestions((qs) => qs.filter((_, idx) => idx !== i));
  }

  async function submit() {
    if (questions.length === 0) {
      toast("error", "Adicione ao menos uma questão.");
      return;
    }
    const payload = questions.map((q) => ({
      type: q.type,
      description: q.description,
      score: Number(q.score) || 1,
      options: q.options.map((text, idx) => ({ text, order: idx })),
      singleChoiceRightOrder: q.type === "SingleChoice" ? q.singleRight : undefined,
      multipleChoiceRightOrders:
        q.type === "MultipleChoice" ? q.multipleRight.sort((a, b) => a - b) : undefined,
    }));
    setSaving(true);
    try {
      const body = JSON.stringify({ minimumScore: Number(minimumScore) || 0, questions: payload });
      if (editing) {
        await api(`/api/assessments/${editing.id}`, { method: "PUT", body });
        toast("success", "Avaliação atualizada.");
      } else {
        await api("/api/assessments", {
          method: "POST",
          body: JSON.stringify({ courseId, minimumScore: Number(minimumScore) || 0, questions: payload }),
        });
        toast("success", "Avaliação criada.");
      }
      setEditing(null);
      setModalOpen(false);
      reload();
    } catch (e) {
      toast("error", errorMessage(e));
    } finally {
      setSaving(false);
    }
  }

  return (
    <AuthGuard roles={["Admin"]}>
      <AppShell>
        <Panel
          icon="assignment"
          title={`Avaliações — ${courses.find((c) => c.id === courseId)?.name ?? "Curso"}`}
          accent="#ec4899"
          action={
            <button className="btn btn-primary text-sm" onClick={openNew}>
              <Icon name="add" /> Nova avaliação
            </button>
          }
        >
          <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
            <label className="inline-flex items-center gap-2 text-sm">
              <span className="text-white/60">Curso:</span>
              <select
                className="field w-auto"
                value={courseId}
                onChange={(e) => {
                  setCourseId(e.target.value);
                  router.replace(`/admin/assessments?course=${e.target.value}`);
                }}
              >
                <option value="">Selecione...</option>
                {courses.map((c) => (
                  <option key={c.id} value={c.id} className="bg-gray-900">
                    {c.name}
                  </option>
                ))}
              </select>
            </label>
            <ViewTabs view={view} onChange={setView} total={total} labels={{ active: "Ativas" }} />
          </div>

          {loading ? (
            <p className="text-sm text-white/50">Carregando...</p>
          ) : !courseId ? (
            <p className="text-sm text-white/50">Selecione um curso.</p>
          ) : assessments.length === 0 ? (
            <p className="text-sm text-white/50">{view === "trash" ? "A lixeira está vazia." : view === "all" ? "Nenhuma avaliação cadastrada." : "Nenhuma avaliação neste curso."}</p>
          ) : (
            <table className="w-full text-left text-sm">
              <thead>
                <tr className="border-b border-white/10 text-xs uppercase tracking-wide text-white/40">
                  <th className="py-2 pr-2 font-medium">Questões</th>
                  <th className="py-2 pr-2 font-medium">Nota mínima</th>
                  <th className="py-2 pr-2 font-medium">Status</th>
                  <th className="py-2 text-right font-medium">Ações</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-white/5">
                {assessments.map((a) => (
                  <tr key={a.id} className="hover:bg-white/5">
                    <td className="py-2.5 pr-2">{a.questionCount}</td>
                    <td className="py-2.5 pr-2 text-white/70">{a.minimumScore}</td>
                    <td className="py-2.5 pr-2">
                      {a.active ? (
                        <span className="inline-flex items-center gap-1 rounded-full bg-emerald-500/10 px-2 py-0.5 text-xs text-emerald-400">
                          <Icon name="check_circle" className="text-sm" /> Ativa
                        </span>
                      ) : (
                        <span className="inline-flex items-center gap-1 rounded-full bg-red-500/10 px-2 py-0.5 text-xs text-red-400">
                          <Icon name="delete" className="text-sm" /> Lixeira
                        </span>
                      )}
                    </td>
                    <td className="py-2.5 text-right">
                      {a.active ? (
                        <>
                          <IconButton name="edit" title="Editar" onClick={() => openEdit(a)} />
                          <IconButton name="delete" title="Mover para a lixeira" variant="danger" onClick={() => setDeleting(a)} />
                        </>
                      ) : (
                        <>
                          <IconButton name="restore" title="Restaurar" onClick={() => restore(a)} />
                          <IconButton name="delete_forever" title="Excluir definitivamente" variant="danger" onClick={() => setPurging(a)} />
                        </>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
          <Pagination page={page} total={total} pageSize={25} onChange={setPage} />
        </Panel>

        <FormModal
          open={modalOpen}
          title={editing ? "Editar avaliação" : "Nova avaliação"}
          onClose={() => {
            setEditing(null);
            setModalOpen(false);
          }}
          onSubmit={submit}
          submitLabel={saving ? "Salvando..." : editing ? "Salvar alterações" : "Salvar avaliação"}
        >
          <label className="block space-y-1">
            <span className="text-sm text-white/60">Nota mínima para aprovação</span>
            <input type="number" className="field" value={minimumScore} onChange={(e) => setMinimumScore(e.target.value)} />
          </label>

          {questions.map((q, qi) => (
            <div key={qi} className="rounded-lg border border-white/10 bg-white/5 p-3 space-y-2">
              <div className="flex items-center gap-2">
                <select
                  className="field w-auto"
                  value={q.type}
                  onChange={(e) => updateQuestion(qi, { type: e.target.value as QDraft["type"] })}
                >
                  <option value="SingleChoice" className="bg-gray-900">Escolha única</option>
                  <option value="MultipleChoice" className="bg-gray-900">Múltipla escolha</option>
                  <option value="Reordering" className="bg-gray-900">Ordenação</option>
                </select>
                <input
                  type="number"
                  className="field w-20"
                  value={q.score}
                  onChange={(e) => updateQuestion(qi, { score: e.target.value })}
                  title="Pontos"
                />
                <IconButton name="delete" title="Remover questão" variant="danger" onClick={() => removeQuestion(qi)} />
              </div>
              <input
                className="field"
                placeholder="Enunciado da questão"
                value={q.description}
                onChange={(e) => updateQuestion(qi, { description: e.target.value })}
              />
              <div className="space-y-1.5">
                <span className="text-xs text-white/50">Opções</span>
                {q.options.map((opt, oi) => (
                  <div key={oi} className="flex items-center gap-2">
                    <input
                      className="field"
                      value={opt}
                      placeholder={`Opção ${oi + 1}`}
                      onChange={(e) =>
                        updateQuestion(qi, {
                          options: q.options.map((o, idx) => (idx === oi ? e.target.value : o)),
                        })
                      }
                    />
                    {q.type === "SingleChoice" && (
                      <input
                        type="radio"
                        name={`single-${qi}`}
                        checked={q.singleRight === oi}
                        onChange={() => updateQuestion(qi, { singleRight: oi })}
                        title="Marcar como correta"
                      />
                    )}
                    {q.type === "MultipleChoice" && (
                      <input
                        type="checkbox"
                        checked={q.multipleRight.includes(oi)}
                        onChange={() =>
                          updateQuestion(qi, {
                            multipleRight: q.multipleRight.includes(oi)
                              ? q.multipleRight.filter((x) => x !== oi)
                              : [...q.multipleRight, oi],
                          })
                        }
                        title="Marcar como correta"
                      />
                    )}
                    <IconButton
                      name="remove_circle"
                      title="Remover opção"
                      variant="danger"
                      onClick={() =>
                        updateQuestion(qi, { options: q.options.filter((_, idx) => idx !== oi) })
                      }
                    />
                  </div>
                ))}
                <button
                  type="button"
                  className="btn btn-ghost text-xs"
                  onClick={() => updateQuestion(qi, { options: [...q.options, ""] })}
                >
                  <Icon name="add" /> Adicionar opção
                </button>
                {q.type === "Reordering" && (
                  <p className="text-xs text-white/40">A resposta correta é a sequência por ordem de posição (arraste/ordene as opções).</p>
                )}
              </div>
            </div>
          ))}

          <button type="button" className="btn btn-ghost w-full justify-center" onClick={addQuestion}>
            <Icon name="add" /> Adicionar questão
          </button>
        </FormModal>

        {modals}
      </AppShell>
    </AuthGuard>
  );
}
