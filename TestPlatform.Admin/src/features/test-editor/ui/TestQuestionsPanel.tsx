import { Plus } from "lucide-react";
import { closestCenter, DndContext, type DragEndEvent, KeyboardSensor, PointerSensor, useSensor, useSensors } from "@dnd-kit/core";
import { SortableContext, sortableKeyboardCoordinates, verticalListSortingStrategy } from "@dnd-kit/sortable";
import type { Question, QuestionEditor } from '@/entities/question'
import type { TestDetails } from '@/entities/test'
import { TestQuestionListItem } from "@/features/test-editor/ui/TestQuestionListItem";

type Props = { test: TestDetails; questions: Question[]; availableQuestions: Question[]; selectedQuestionId: string; setSelectedQuestionId: (value: string) => void; editable: boolean; busy: boolean; expandedQuestionId?: string; loadingQuestionId?: string; questionDetails: Record<string, QuestionEditor>; questionImageUrls: Record<string, string>; optionImageUrls: Record<string, string>; addQuestion: () => Promise<void>; removeQuestion: (id: string) => Promise<void>; toggleQuestion: (id: string) => Promise<void>; moveQuestion: (event: DragEndEvent) => Promise<void> };
export function TestQuestionsPanel({ test, questions, availableQuestions, selectedQuestionId, setSelectedQuestionId, editable, busy, expandedQuestionId, loadingQuestionId, questionDetails, questionImageUrls, optionImageUrls, addQuestion, removeQuestion, toggleQuestion, moveQuestion }: Props) { const sensors = useSensors(useSensor(PointerSensor, { activationConstraint: { distance: 6 } }), useSensor(KeyboardSensor, { coordinateGetter: sortableKeyboardCoordinates })); return (
              <div className="card">
                <div className="flex items-center justify-between">
                  <div>
                    <h2 className="text-lg font-semibold">Вопросы</h2>
                    <p className="mt-1 text-sm text-slate-500">
                      Нажмите на вопрос, чтобы раскрыть его. Перетаскивайте за
                      ручку, чтобы изменить порядок.
                    </p>
                  </div>
                  <span className="text-sm text-slate-500">
                    {test.questions.length}
                  </span>
                </div>
                {editable && (
                  <div className="mt-5 flex gap-2">
                    <select
                      className="input"
                      onChange={(event) =>
                        setSelectedQuestionId(event.target.value)
                      }
                      value={selectedQuestionId}
                    >
                      <option value="">Выберите опубликованный вопрос</option>
                      {availableQuestions.map((question) => (
                        <option key={question.id} value={question.id}>
                          {question.text}
                        </option>
                      ))}
                    </select>
                    <button
                      className="button-primary shrink-0"
                      disabled={!selectedQuestionId || busy}
                      onClick={() => void addQuestion()}
                      type="button"
                    >
                      <Plus size={16} /> Добавить
                    </button>
                  </div>
                )}
                <div className="mt-4 overflow-hidden rounded-lg border border-slate-200">
                  {test.questions.length === 0 ? (
                    <p className="p-6 text-center text-sm text-slate-500">
                      Вопросы ещё не добавлены
                    </p>
                  ) : (
                    <DndContext
                      collisionDetection={closestCenter}
                      onDragEnd={(event) => void moveQuestion(event)}
                      sensors={sensors}
                    >
                      <SortableContext
                        items={test.questions.map((item) => item.questionId)}
                        strategy={verticalListSortingStrategy}
                      >
                        {test.questions.map((item, index) => {
                          const question = questions.find(
                            (candidate) => candidate.id === item.questionId,
                          );
                          const details = questionDetails[item.questionId];
                          return (
                            <TestQuestionListItem
                              key={item.questionId}
                              id={item.questionId}
                              index={index}
                              text={
                                question?.text ??
                                details?.text ??
                                "Вопрос недоступен"
                              }
                              kind={question?.kind ?? details?.kind}
                              editable={editable && !busy}
                              expanded={expandedQuestionId === item.questionId}
                              loading={loadingQuestionId === item.questionId}
                              details={details}
                              imageUrl={questionImageUrls[item.questionId]}
                              optionImageUrls={optionImageUrls}
                              onToggle={() =>
                                void toggleQuestion(item.questionId)
                              }
                              onRemove={() =>
                                void removeQuestion(item.questionId)
                              }
                            />
                          );
                        })}
                      </SortableContext>
                    </DndContext>
                  )}
                </div>
              </div>

); }


