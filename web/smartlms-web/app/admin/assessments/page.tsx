"use client";

import { Suspense, useCallback, useEffect, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { api, errorMessage } from "@/lib/api";
import AppShell from "@/components/AppShell";
import AuthGuard from "@/components/AuthGuard";
import Panel from "@/components/ui/Panel";
import FormModal from "@/components/ui/FormModal";
import ConfirmModal from "@/components/ui/ConfirmModal";
import Icon from "@/components/ui/Icon";
import IconButton from "@/components/ui/IconButton";
import { useToast } from "@/components/ui/Toast";

type Course = { id: string; name: string };
type AssessmentItem = { id: string; courseName: string; minimumScore: number; questionCount: number };

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
  const [assessments, setAssessments] = useState<AssessmentItem[]>([]);
  const [loading, setLoading] = useState(false);

  const [modalOpen, setModalOpen] = useState(false);
  const [minimumScore, setMinimumScore] = useState("0");
  const [questions, setQuestions] = useState<QDraft[]>([]);
  const [saving, setSaving] = useState(false);
  const [deleting, setDeleting] = useState<AssessmentItem | null>(null);

  const loadOptions = useCallback(async () => {
    const cs = await api<Course[]>("/api/courses");
    setCourses(cs);
    if (!courseId && cs.length > 0) setCourseId(cs[0].id);
  }, [courseId]);

  const load = useCallback(async () => {
    if (!courseId) return;
    setLoading(true);
    try {
      setAssessments(await api<AssessmentItem[]>(`/api/courses/${courseId}/assessments`));
    } catch (e) {
      toast("error", errorMessage(e));
    } finally {
      setLoading(false);
    }
  }, [courseId, toast]);

  useEffect(() => {
    loadOptions();
  }, [loadOptions]);

  useEffect(() => {
    load();
  }, [load]);

  useEffect(() => {
    if (courseParam && courseParam !== courseId) setCourseId(courseParam);
  }, [courseParam, courseId]);

  function openNew() {
    setMinimumScore("0");
    setQuestions([]);
    setModalOpen(true);
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
      await api("/api/assessments", {
        method: "POST",
        body: JSON.stringify({ courseId, minimumScore: Number(minimumScore) || 0, questions: payload }),
      });
      toast("success", "Avaliação criada.");
      setModalOpen(false);
      load();
    } catch (e) {
      toast("error", errorMessage(e));
    } finally {
      setSaving(false);
    }
  }

  async function confirmDelete() {
    if (!deleting) return;
    try {
      await api(`/api/assessments/${deleting.id}`, { method: "DELETE" });
      toast("success", "Avaliação excluída.");
      setDeleting(null);
      load();
    } catch (e) {
      toast("error", errorMessage(e));
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
          <label className="mb-4 inline-flex items-center gap-2 text-sm">
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

          {loading ? (
            <p className="text-sm text-white/50">Carregando...</p>
          ) : !courseId ? (
            <p className="text-sm text-white/50">Selecione um curso.</p>
          ) : assessments.length === 0 ? (
            <p className="text-sm text-white/50">Nenhuma avaliação neste curso.</p>
          ) : (
            <table className="w-full text-left text-sm">
              <thead>
                <tr className="border-b border-white/10 text-xs uppercase tracking-wide text-white/40">
                  <th className="py-2 pr-2 font-medium">Questões</th>
                  <th className="py-2 pr-2 font-medium">Nota mínima</th>
                  <th className="py-2 text-right font-medium">Ações</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-white/5">
                {assessments.map((a) => (
                  <tr key={a.id} className="hover:bg-white/5">
                    <td className="py-2.5 pr-2">{a.questionCount}</td>
                    <td className="py-2.5 pr-2 text-white/70">{a.minimumScore}</td>
                    <td className="py-2.5 text-right">
                      <IconButton name="delete" title="Excluir" danger onClick={() => setDeleting(a)} />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </Panel>

        <FormModal
          open={modalOpen}
          title="Nova avaliação"
          onClose={() => setModalOpen(false)}
          onSubmit={submit}
          submitLabel={saving ? "Salvando..." : "Salvar avaliação"}
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
                <IconButton name="delete" title="Remover questão" danger onClick={() => removeQuestion(qi)} />
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
                      danger
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

        <ConfirmModal
          open={!!deleting}
          title="Excluir avaliação"
          message="Excluir esta avaliação?"
          onConfirm={confirmDelete}
          onCancel={() => setDeleting(null)}
        />
      </AppShell>
    </AuthGuard>
  );
}
