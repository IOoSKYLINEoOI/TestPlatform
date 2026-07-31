import type { FormEvent } from "react";
import { Save } from "lucide-react";

type Props = { title: string; description: string; editable: boolean; busy: boolean; setTitle: (value: string) => void; setDescription: (value: string) => void; saveDetails: (event: FormEvent) => Promise<void> };
export function TestDetailsForm({ title, description, editable, busy, setTitle, setDescription, saveDetails }: Props) { return (
              <form className="card" onSubmit={saveDetails}>
                <h2 className="text-lg font-semibold">Основная информация</h2>
                <label className="label mt-5">
                  Название
                  <input
                    className="input mt-2"
                    disabled={!editable}
                    maxLength={200}
                    onChange={(event) => setTitle(event.target.value)}
                    required
                    value={title}
                  />
                </label>
                <label className="label mt-5">
                  Описание
                  <textarea
                    className="input mt-2 min-h-32 resize-y"
                    disabled={!editable}
                    maxLength={2000}
                    onChange={(event) => setDescription(event.target.value)}
                    required
                    value={description}
                  />
                </label>
                {editable && (
                  <div className="mt-5 flex justify-end">
                    <button
                      className="button-primary"
                      disabled={busy}
                      type="submit"
                    >
                      <Save size={16} /> Сохранить
                    </button>
                  </div>
                )}
              </form>

); }
