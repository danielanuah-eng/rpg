"""Renderiza o manual indicado pelo autor em uma pasta isolada de leitura."""
import hashlib
import json
from pathlib import Path
import pypdfium2 as pdfium
from pypdf import PdfReader

ROOT = Path(__file__).resolve().parents[4]
SOURCE = ROOT / "krosmaster_arena_2_0_krosmaster_arena_2_0_manual_ga_76609.pdf"
OUT = ROOT / "tmp/auditoria-manual-013"
OUT.mkdir(parents=True,exist_ok=True)
doc=pdfium.PdfDocument(str(SOURCE))
reader=PdfReader(str(SOURCE))
text=[]
for i in range(len(doc)):
    page=doc[i]
    scale=1400/page.get_width()
    image=page.render(scale=scale).to_pil()
    image.save(OUT/f"pagina-{i+1:02}.png")
    t=reader.pages[i].extract_text() or ""
    text.append({"paginaPdf":i+1,"texto":t})
    page.close()
(OUT/"texto.json").write_text(json.dumps(text,ensure_ascii=False,indent=2),encoding="utf-8")
(OUT/"fonte.json").write_text(json.dumps({"arquivo":str(SOURCE),"sha256":hashlib.sha256(SOURCE.read_bytes()).hexdigest(),"paginas":len(doc),"convencao":"paginaPdf 1 = primeira folha do arquivo; confirmar numero impresso separado"},ensure_ascii=False,indent=2),encoding="utf-8")
print(json.dumps({"pages":len(doc),"out":str(OUT),"textChars":sum(len(t["texto"]) for t in text)},ensure_ascii=False))
