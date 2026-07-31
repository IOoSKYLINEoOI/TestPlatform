import type { AttemptListItem } from '@/entities/attempt'

const headers = ['Номер попытки', 'Табельный номер', 'ID пользователя', 'Статус', 'Вопросов', 'Дано ответов', 'Правильных ответов', 'Баллы', 'Максимум баллов', 'Результат, %', 'Сдан', 'Начало', 'Завершение']

function rows(items: AttemptListItem[]) {
  return items.map((item) => [
    item.attemptNumber, item.employeeNumber, item.userId, item.status,
    item.answeredQuestions, item.totalQuestions, item.correctAnswers ?? '',
    item.earnedPoints ?? '', item.maxPoints ?? '', item.percentage ?? '',
    item.passed == null ? '' : item.passed ? 'Да' : 'Нет',
    item.startedAt ? new Date(item.startedAt).toLocaleString('ru-RU') : '',
    item.finishedAt ? new Date(item.finishedAt).toLocaleString('ru-RU') : '',
  ])
}

export function exportAttemptsCsv(items: AttemptListItem[], filename: string) {
  const escape = (value: unknown) => `"${String(value).replaceAll('"', '""')}"`
  const content = '\uFEFF' + [headers, ...rows(items)].map((row) => row.map(escape).join(';')).join('\r\n')
  download(new Blob([content], { type: 'text/csv;charset=utf-8' }), `${filename}.csv`)
}

export async function exportAttemptsXlsx(items: AttemptListItem[], filename: string) {
  const { strToU8, zipSync } = await import('fflate')
  const table = [headers, ...rows(items)]
  const files: Record<string, Uint8Array> = {
    '[Content_Types].xml': xml(`<?xml version="1.0" encoding="UTF-8" standalone="yes"?><Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types"><Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/><Default Extension="xml" ContentType="application/xml"/><Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/><Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/><Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/></Types>`),
    '_rels/.rels': xml(`<?xml version="1.0" encoding="UTF-8" standalone="yes"?><Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/></Relationships>`),
    'xl/workbook.xml': xml(`<?xml version="1.0" encoding="UTF-8" standalone="yes"?><workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"><sheets><sheet name="Попытки" sheetId="1" r:id="rId1"/></sheets></workbook>`),
    'xl/_rels/workbook.xml.rels': xml(`<?xml version="1.0" encoding="UTF-8" standalone="yes"?><Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/><Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/></Relationships>`),
    'xl/styles.xml': xml(`<?xml version="1.0" encoding="UTF-8" standalone="yes"?><styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><fonts count="2"><font><sz val="11"/><name val="Calibri"/></font><font><b/><sz val="11"/><name val="Calibri"/></font></fonts><fills count="1"><fill><patternFill patternType="none"/></fill></fills><borders count="1"><border/></borders><cellStyleXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0"/></cellStyleXfs><cellXfs count="2"><xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0"/><xf numFmtId="0" fontId="1" fillId="0" borderId="0" xfId="0" applyFont="1"/></cellXfs></styleSheet>`),
    'xl/worksheets/sheet1.xml': xml(buildWorksheet(table)),
  }
  const content = zipSync(files, { level: 6 })
  download(new Blob([Uint8Array.from(content).buffer], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' }), `${filename}.xlsx`)

  function xml(content: string) { return strToU8(content) }
}

function buildWorksheet(table: Array<Array<unknown>>) {
  const columns = headers.map((header, index) => {
    const width = Math.max(14, header.length + 2)
    return `<col min="${index + 1}" max="${index + 1}" width="${width}" customWidth="1"/>`
  }).join('')
  const sheetRows = table.map((row, rowIndex) => `<row r="${rowIndex + 1}">${row.map((value, columnIndex) => {
    const reference = `${columnName(columnIndex)}${rowIndex + 1}`
    const style = rowIndex === 0 ? ' s="1"' : ''
    return typeof value === 'number'
      ? `<c r="${reference}"${style}><v>${value}</v></c>`
      : `<c r="${reference}" t="inlineStr"${style}><is><t>${escapeXml(String(value))}</t></is></c>`
  }).join('')}</row>`).join('')
  return `<?xml version="1.0" encoding="UTF-8" standalone="yes"?><worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><sheetViews><sheetView workbookViewId="0"><pane ySplit="1" topLeftCell="A2" activePane="bottomLeft" state="frozen"/></sheetView></sheetViews><cols>${columns}</cols><sheetData>${sheetRows}</sheetData></worksheet>`
}

function columnName(index: number) {
  let value = index + 1
  let result = ''
  while (value > 0) { value -= 1; result = String.fromCharCode(65 + value % 26) + result; value = Math.floor(value / 26) }
  return result
}

function escapeXml(value: string) { return value.replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;').replaceAll('"', '&quot;').replaceAll("'", '&apos;') }

function download(blob: Blob, filename: string) {
  const url = URL.createObjectURL(blob)
  const anchor = document.createElement('a')
  anchor.href = url; anchor.download = filename; anchor.click()
  URL.revokeObjectURL(url)
}
