import { Plus } from "lucide-react";
import type { Dispatch, FormEvent, SetStateAction } from "react";
import type { ExamDetails, ExamSection } from '@/entities/exam'
import type { Question } from '@/entities/question'
import type { ExamSectionInput } from "../model/useExamForm";
import { ExamSectionCard } from "@/features/exam-editor/ui/ExamSectionCard";
type Props = { exam: ExamDetails; questions: Question[]; editable: boolean; busy: boolean; newSection: ExamSectionInput; setNewSection: Dispatch<SetStateAction<ExamSectionInput>>; selectedQuestion: Record<string, string>; setSelectedQuestion: Dispatch<SetStateAction<Record<string, string>>>; addSection: (event: FormEvent) => Promise<void>; addQuestion: (section: ExamSection) => Promise<void>; onUpdateSection: (section: ExamSection, input: { name: string; questionsToSelect: number; scorePerQuestion: number }) => Promise<void>; onRemoveSection: (section: ExamSection) => void; onRemoveQuestion: (section: ExamSection, questionId: string) => void };
export function ExamSectionsPanel({ exam, questions, editable, busy, newSection, setNewSection, selectedQuestion, setSelectedQuestion, addSection, addQuestion, onUpdateSection, onRemoveSection, onRemoveQuestion }: Props) { return (
              <div className="card">
                <h2 className="text-lg font-semibold">Секции экзамена</h2>
                <p className="mt-1 text-sm text-slate-500">
                  В каждой секции задайте количество выбираемых вопросов и
                  Балл за вопрос.
                </p>
                {editable && (
                  <form
                    className="mt-5 grid gap-2 md:grid-cols-[1fr_9rem_9rem_auto]"
                    onSubmit={addSection}
                  >
                    <div className="hidden md:contents">
                      <span className="text-xs font-medium text-slate-500">
                        Название секции
                      </span>
                      <span className="text-xs font-medium text-slate-500">
                        Количество вопросов
                      </span>
                      <span className="text-xs font-medium text-slate-500">
                        Балл за вопрос
                      </span>
                      <span />
                    </div>
                    <input
                      className="input"
                      onChange={(e) =>
                        setNewSection({ ...newSection, name: e.target.value })
                      }
                      placeholder="Название секции"
                      required
                      value={newSection.name}
                    />
                    <input
                      className="input"
                      min="1"
                      onChange={(e) =>
                        setNewSection({
                          ...newSection,
                          questionsToSelect: e.target.value,
                        })
                      }
                      required
                      type="number"
                      value={newSection.questionsToSelect}
                    />
                    <input
                      className="input"
                      min="1"
                      onChange={(e) =>
                        setNewSection({
                          ...newSection,
                          scorePerQuestion: e.target.value,
                        })
                      }
                      required
                      type="number"
                      value={newSection.scorePerQuestion}
                    />
                    <button className="button-primary" disabled={busy}>
                      <Plus size={16} /> Добавить
                    </button>
                  </form>
                )}
                <div className="mt-5 space-y-4">
                  {exam.sections.length === 0 ? (
                    <p className="text-sm text-slate-500">Секций пока нет.</p>
                  ) : (
                    exam.sections.map((section) => (
                      <ExamSectionCard
                        key={section.id}
                        section={section}
                        questions={questions}
                        editable={Boolean(editable)}
                        busy={busy}
                        selectedQuestion={selectedQuestion[section.id] ?? ""}
                        onSelect={(value) =>
                          setSelectedQuestion((current) => ({
                            ...current,
                            [section.id]: value,
                          }))
                        }
                        onAdd={() => void addQuestion(section)}
                        onUpdate={(input) => onUpdateSection(section, input)}
                        onRemove={() => onRemoveSection(section)}
                        onRemoveQuestion={(questionId) => onRemoveQuestion(section, questionId)}
                      />
                    ))
                  )}
                </div>
              </div>

); }
