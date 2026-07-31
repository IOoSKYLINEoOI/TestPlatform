import { Save } from "lucide-react";
import type { FormEvent } from "react";
type Props = { title: string; description: string; editable: boolean; busy: boolean; showValidation: boolean; setTitle: (value: string) => void; setDescription: (value: string) => void; saveDetails: (event: FormEvent) => Promise<void> };
export function ExamDetailsForm({ title, description, editable, busy, showValidation, setTitle, setDescription, saveDetails }: Props) { return (
              <form className="card" onSubmit={saveDetails}>
                <h2 className="text-lg font-semibold">Основная информация</h2>
                <label className="label mt-5">
                  Название
                  <input
                    className={`input mt-2 ${showValidation && !title.trim() ? "border-rose-400 ring-2 ring-rose-100" : ""}`}
                    disabled={!editable}
                    maxLength={200}
                    onChange={(e) => setTitle(e.target.value)}
                    required
                    value={title}
                  />
                </label>
                <label className="label mt-5">
                  Описание
                  <textarea
                    className={`input mt-2 min-h-28 ${showValidation && !description.trim() ? "border-rose-400 ring-2 ring-rose-100" : ""}`}
                    disabled={!editable}
                    maxLength={2000}
                    onChange={(e) => setDescription(e.target.value)}
                    required
                    value={description}
                  />
                </label>
                {editable && (
                  <div className="mt-5 flex justify-end">
                    <button className="button-primary" disabled={busy}>
                      <Save size={16} /> Сохранить
                    </button>
                  </div>
                )}
              </form>

); }
