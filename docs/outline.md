# 30‑minute presentation outline: Local agentic AI on Windows 11 (GPU/NPU), no cloud required

> Core idea: Just as high‑end gaming pulled GPUs into every home, local AI will pull accelerators (GPU/NPU) onto every desk. We’ll show why the economics and experience make this inevitable—and prove it live on Windows 11.

---

## 0. Title slide (0:00–0:30)

- Local agentic AI: From cloud dependence to client excellence
- Tagline: Gaming made GPUs mainstream. Personal AI will do it again.

---

## 1. Set the stage: Why now (0:30–4:00)

- The bottleneck isn’t intelligence; it’s delivery. Cloud adds latency, cost, and privacy friction.
- Small Language Models (SLMs) are “good enough” for most agentic tasks, especially with tools[^slm-future].
- Windows 11 + modern accelerators = low‑latency, private, resilient agents on your machine.

---

## 2. The gaming parallel: A market we’ve seen before (4:00–7:00)

- Killer app → hardware demand → ecosystem flywheel.
- Gaming did it for GPUs; personal AI (copilots, creators, automators) will do it for GPU/NPU.
- What changes:
  - Continuous, everyday usage (not just leisure windows).
  - Value accrues to privacy, latency, and offline reliability.
  - Modular SLM stacks unlock rapid specialization at the edge.

---

## 3. Economics that move budgets (7:00–12:00)

### A. Latency, privacy, reliability
- Local: sub‑second prompt‑to‑first‑token, zero egress, works on airplanes.
- Cloud: network variance, compliance review, rate limits.

### B. Cost structure (simple model you can plug in)
- Cloud opex for generation: \( \text{Monthly} = U \times Q \times T \times c \)
  - \(U\): users, \(Q\): queries/user/month, \(T\): tokens/query, \(c\): $/token.
- Local capex amortized: \( \text{Monthly} = \frac{H}{L} + P \)
  - \(H\): hardware spend, \(L\): lifetime (months), \(P\): power/month.
- Hybrid: escalate only on “hard” cases; target <10% cloud calls.

### C. Organizational unlocks
- Vendor independence, predictable spend, easier data governance.
- Edge fine‑tuning/specialization for workflows without central bottlenecks.

---

## 4. Architecture: A pragmatic local agent on Windows 11 (12:00–15:00)

- **Controller SLM**: 3–8B instruction model (quantized) for tool use and formatting.
- **Toolbelt**:
  - Local RAG over a folder (PDFs/Markdown).
  - File I/O (JSON, CSV).
  - Shell or PowerShell tasks.
  - Optional: simple UI automation (Notepad, Explorer) for visual “wow”.
- **Accelerators**:
  - GPU path: NVIDIA (CUDA/cuBLAS) or cross‑vendor via DirectML.
  - NPU path (Copilot‑class PCs): ONNX‑Runtime pipelines with NPU EP where available.
- **Safety/guardrails**: schema‑constrained outputs, strict tool contracts.
- **Observability**: local logging of prompts, tokens/s, tool calls, and latency.

---

## 5. Live demo: “Agent on a laptop” (15:00–23:00)

Goal: Prove compelling, private, real‑time agentic behavior using only local compute.

### Demo flow (scripted, 8 minutes)

1) Show it’s local
- Open Task Manager → Performance: watch GPU/NPU utilization spike, Network minimal.
- Start model server; show tokens streaming.

2) Tool‑use loop (3 quick tasks)
- Local RAG: “Summarize the included brief and extract 3 actions by role.”
- File action: “Write a prioritized to‑do list to todos.json; add deadlines.”
- UI automation: “Open Notepad and draft a 3‑paragraph email from the summary.”

3) Stress test
- Run the same queries twice: show stable latency, no rate limits, no egress.

4) Close with toggles
- Switch acceleration off → demonstrate latency penalty.
- Switch model sizes (3B vs 7–8B) → show quality/latency trade‑off.

### What you’ll need (prep checklist)

- Windows 11, 16+ GB RAM; one of:
  - NVIDIA RTX (8+ GB VRAM) for comfortable 7–8B int4.
  - Copilot‑class NPU laptop for 3–4B ONNX int4 pipelines.
- Install:
  - A quantized SLM: e.g., Phi‑3‑mini‑Instruct (int4) or Llama‑3‑8B (Q4).
  - Runtime: one of
    - llama.cpp (cuBLAS or DirectML)
    - ONNX Runtime (DirectML/NPU EP)
    - TensorRT‑LLM (RTX path)
  - A minimal agent runner with tools (Python or Node).
- Test corpus: a small local folder (PDF/MD) for RAG.

---

## 6. Proof points and measurements (23:00–26:00)

- Show tokens/s under acceleration vs CPU.
- End‑to‑end task times (RAG + tool call + file write).
- Stability across repeated runs.
- Privacy: no network calls; logs stored locally.

Table: Cloud LLM vs Local SLM for agentic tasks

| Attribute        | Cloud LLM                             | Local SLM (GPU/NPU)                  |
|------------------|---------------------------------------|--------------------------------------|
| Latency          | Network‑bound; variable               | Consistent; prompt‑to‑first‑token fast |
| Cost             | Opex per token                        | Amortized capex + small power        |
| Privacy          | Data leaves device                    | Stays on device                      |
| Reliability      | Rate limits/outages                   | Works offline                        |
| Customization    | Centralized, slower cycles            | Rapid, per‑machine specialization    |

---

## 7. When to escalate to the cloud (26:00–27:30)

- Edge does 80–90%: routine reasoning, tool orchestration, structured outputs.
- Escalate for:
  - Long‑context synthesis across large corpora.
  - Novel, cross‑domain reasoning or complex code refactors.
  - Heavy multimodal generation at high fidelity.
- Policy: confidence gating + budget thresholds to trigger cloud calls.

---

## 8. Call to action and next steps (27:30–30:00)

- Pilot plan:
  - Week 1: Select 2–3 agent workflows; gather local corpora.
  - Week 2: Stand up local stack; benchmark latency and quality.
  - Week 3: Rollout to 10 users; measure savings and satisfaction.
- Hardware roadmap:
  - Short term: ensure 8–12 GB VRAM or Copilot‑class NPU on new laptops.
  - Medium term: standardize on an accelerator tier per persona.
- Success metrics:
  - % tasks completed locally, median latency, cost per task, user NPS.

---

## Optional appendix: Minimal local agent scaffold

Use a small controller with strict function schemas. Example tool contracts:

```json
{
  "tools": [
    {
      "name": "search_docs",
      "description": "Semantic search over ./corpus",
      "parameters": {"query": "string", "k": "integer"}
    },
    {
      "name": "write_file",
      "description": "Write text to a local file",
      "parameters": {"path": "string", "content": "string"}
    },
    {
      "name": "run_powershell",
      "description": "Execute a safe PowerShell command",
      "parameters": {"command": "string"}
    }
  ],
  "response_format": {"type": "json", "strict": true}
}
```

Implementation tips:
- Keep prompts short; enforce JSON with a grammar or JSON schema.
- Cache embeddings locally for instant RAG.
- Start with int4 quantization; tune context window to fit VRAM/NPU limits.
- Log tokens/s and tool latency; display a tiny HUD during the demo.

---

Want me to tailor this to your exact hardware and pick a concrete model/runtime combo for your Windows 11 machine? I can also script the demo so it’s one command to run.

[^slm-future]: P. Belcak, G. Heinrich, S. Diao, Y. Fu, X. Dong, S. Muralidharan, Y. C. Lin, P. Molchanov. "Small Language Models are the Future of Agentic AI." arXiv:2506.02153 [cs.AI], 2025. <https://doi.org/10.48550/arXiv.2506.02153>
