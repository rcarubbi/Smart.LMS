"use client";

import { useCallback, useEffect, useState } from "react";
import { useParams } from "next/navigation";
import { api, errorMessage } from "@/lib/api";
import AppShell from "@/components/AppShell";
import Panel from "@/components/ui/Panel";
import Icon from "@/components/ui/Icon";
import { useToast } from "@/components/ui/Toast";

type OptionDetail = { id: string; text: string; order: number };
type QuestionDetail = { id: string; type: string; description: string; score: number; options: OptionDetail[] };
type AssessmentDetail = { id: string; courseId: string; minimumScore: number; questions: QuestionDetail[] };

type Result = {
  assessmentId: string;
  courseName: string;
  score: number;
  minimumScore: number;
  passed: boolean;
  questions: { questionId: string; correct: boolean; scoreAwarded: number }[];
};

export default function AssessmentPage() {
  const params = useParams<{ assessmentId: string }>();
  const assessmentId = params.assessmentId;
  const { toast } = useToast();
  const [assessment, setAssessment] = useState<AssessmentDetail | null>(null);
  const [answers, setAnswers] = useState<Record<string, string[]>>({});
  const [result, setResult] = useState<Result | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const load = useCallback(async () => {
    try {
      const a = await api<AssessmentDetail>(`/api/assessments/${assessmentId}`);
      setAssessment(a);
      // Seed the in-progress answer state with the original option order so
      // the reorder arrows operate on a real (non-empty) list.
      const seeded: Record<string, string[]> = {};
      for (const q of a.questions) {
        if (q.type === "Reordering") seeded[q.id] = q.options.map((o) => o.id);
      }
      setAnswers((prev) => ({ ...seeded, ...prev }));
    } catch (e) {
      toast("error", errorMessage(e, "Avaliação não encontrada."));
    }
  }, [assessmentId, toast]);

  useEffect(() => {
    load();
  }, [load]);

  function setAnswer(questionId: string, optionId: string, single: boolean) {
    setAnswers((a) => {
      if (single) return { ...a, [questionId]: [optionId] };
      const cur = a[questionId] ?? [];
      const next = cur.includes(optionId) ? cur.filter((x) => x !== optionId) : [...cur, optionId];
      return { ...a, [questionId]: next };
    });
  }

  function move(questionId: string, index: number, dir: -1 | 1) {
    setAnswers((a) => {
      const cur = a[questionId] ?? [];
      const target = index + dir;
      if (target < 0 || target >= cur.length) return a;
      const next = [...cur];
      [next[index], next[target]] = [next[target], next[index]];
      return { ...a, [questionId]: next };
    });
  }

  // For reordering, default the answer to the original option order.
  function reorderOrder(q: QuestionDetail): string[] {
    const custom = answers[q.id];
    if (custom && custom.length > 0) return custom;
    return q.options.map((o) => o.id);
  }

  async function submit() {
    if (!assessment) return;
    const questions = assessment.questions.map((q) => {
      const isReorder = q.type === "Reordering";
      const optionIds = isReorder
        ? reorderOrder(q)
        : (answers[q.id] ?? []);
      return { questionId: q.id, optionIds };
    });
    setSubmitting(true);
    try {
      setResult(await api<Result>(`/api/assessments/${assessment.id}/submit`, {
        method: "POST",
        body: JSON.stringify({ questions }),
      }));
    } catch (e) {
      toast("error", errorMessage(e));
    } finally {
      setSubmitting(false);
    }
  }

  if (result) {
    const pct = result.minimumScore ? Math.round((result.score / result.minimumScore) * 100) : 0;
    return (
      <AppShell>
        <Panel
          icon={result.passed ? "emoji_events" : "sentiment_dissatisfied"}
          title={result.passed ? "Aprovado!" : "Não atingiu a nota mínima"}
          accent={result.passed ? "#10b981" : "#ef4444"}
        >
          <div className="flex flex-col items-center gap-2 py-4 text-center">
            <span className={`material-icon text-5xl ${result.passed ? "text-emerald-400" : "text-red-400"}`}>
              {result.passed ? "check_circle" : "cancel"}
            </span>
            <p className="text-2xl font-semibold">
              {result.score} / {result.minimumScore}
            </p>
            <p className="text-sm text-white/50">Nota mínima: {result.minimumScore} • Progresso: {pct}%</p>
            <a href={`/course/${assessment?.courseId}`} className="btn btn-primary mt-4">
              <Icon name="arrow_back" /> Voltar ao curso
            </a>
          </div>
        </Panel>
      </AppShell>
    );
  }

  if (!assessment) {
    return (
      <AppShell>
        <p className="text-sm text-white/50">Carregando avaliação...</p>
      </AppShell>
    );
  }

  return (
    <AppShell>
      <Panel icon="quiz" title="Avaliação" accent="#ec4899">
        <p className="mb-4 text-sm text-white/50">Nota mínima para aprovação: {assessment.minimumScore}</p>

        <div className="space-y-6">
          {assessment.questions.map((q, qi) => (
            <div key={q.id} className="rounded-xl border border-white/10 bg-white/5 p-4">
              <div className="mb-3 flex items-start justify-between gap-3">
                <p className="font-medium">
                  <span className="mr-2 text-white/40">{qi + 1}.</span>
                  {q.description}
                </p>
                <span className="shrink-0 rounded bg-white/10 px-2 py-0.5 text-xs text-white/60">{q.score} pts</span>
              </div>

              {q.type === "Reordering" ? (
                <div className="space-y-1.5">
                  <p className="text-xs text-white/40">Ordene as opções (do topo para baixo).</p>
                  {reorderOrder(q).map((optId, oi) => {
                    const opt = q.options.find((o) => o.id === optId);
                    return (
                      <div key={optId} className="flex items-center gap-2 rounded-lg border border-white/10 bg-white/5 px-3 py-2 text-sm">
                        <span className="w-6 text-white/40">{oi + 1}.</span>
                        <span className="flex-1">{opt?.text}</span>
                        <div className="flex flex-col">
                          <button className="text-white/50 hover:text-white" onClick={() => move(q.id, oi, -1)}><Icon name="arrow_drop_up" /></button>
                          <button className="text-white/50 hover:text-white" onClick={() => move(q.id, oi, 1)}><Icon name="arrow_drop_down" /></button>
                        </div>
                      </div>
                    );
                  })}
                </div>
              ) : (
                <div className="space-y-1.5">
                  {q.options.map((opt) => {
                    const checked = (answers[q.id] ?? []).includes(opt.id);
                    return (
                      <label
                        key={opt.id}
                        className="flex cursor-pointer items-center gap-3 rounded-lg border border-white/10 bg-white/5 px-3 py-2 text-sm transition hover:border-indigo-500/50"
                      >
                        <input
                          type={q.type === "SingleChoice" ? "radio" : "checkbox"}
                          name={q.id}
                          checked={checked}
                          onChange={() => setAnswer(q.id, opt.id, q.type === "SingleChoice")}
                          className="accent-indigo-500"
                        />
                        {opt.text}
                      </label>
                    );
                  })}
                </div>
              )}
            </div>
          ))}
        </div>

        <button className="btn btn-primary mt-6 w-full justify-center" onClick={submit} disabled={submitting}>
          <Icon name="send" /> {submitting ? "Enviando..." : "Enviar respostas"}
        </button>
      </Panel>
    </AppShell>
  );
}
