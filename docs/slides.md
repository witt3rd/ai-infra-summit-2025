---
marp: true
theme: uncover
class: invert
size: 16:9
style: |
  section {
    font-size: 1.8rem;
  }
  h1 {
    font-size: 2.5rem;
    font-weight: 600;
  }
  h2 {
    font-size: 2rem;
    font-weight: 500;
    margin-bottom: 1.5rem;
  }
  h3 {
    font-size: 1.5rem;
    font-weight: 500;
  }
  .small-text {
    font-size: 0.9rem;
    color: #888;
    margin-top: 2rem;
  }
  .subtitle {
    font-size: 1.2rem;
    color: #aaa;
    font-weight: 300;
  }
  ul {
    text-align: left;
  }
  li {
    font-size: 1.3rem;
    line-height: 1.8;
  }
  strong {
    color: #4fc3f7;
  }
---

# Designing Multi-Agent Systems

<p class="subtitle">Challenges and Pitfalls</p>

Donald Thompson  
Distinguished Engineer  
Microsoft

---

## What is an agent

An AI agent is an **autonomous** system powered by (language) models that **achieves goals** by:

- **Reasoning**: Using LLM capabilities for logical thinking
- **Planning**: Creating multi-step strategies  
- **Acting**: Executing tools/APIs to effect change
- **Observing**: Monitoring outcomes and environment
- **Self-Correcting**: Adapting strategy based on feedback

<p class="small-text">Key differentiator: <strong>autonomy</strong> - agents maintain self-directed control over their process, dynamically adjusting their approach rather than following scripted paths.</p>

---

## What is a multi-agent system

A multi-agent system consists of two or more autonomous agents that **collaborate** (cooperative or adversarial) to achieve goals through:

- **message passing** (distributed)
- **direct invocation** (local)
- **shared memory** (blackboard)
- **environment modification** (state)

---

## Why not a single agent?

Almost everything marketed as "multi-agent" could be done with a single agent. Multi-agent is primarily a **software engineering** choice, not an architectural requirement.

### What Single Agents Can Do:

- Switch between different prompts/modes (adversarial validation, different expertise)
- Call different models via tool use (GPT-4 for reasoning, Claude for coding)
- Manage complex workflows with state machines
- Handle "parallel" tasks through async operations

---

## When to use multi-agent systems?

### Hard Requirements for Multi-Agent

- **Physical parallelism with latency constraints** - When you MUST process in multiple locations simultaneously (edge computing, geographic distribution)
- **Regulatory/security boundaries** - When different parts of the system need different data access, compliance rules, or security clearances
- **Organizational boundaries** - When different teams/companies need to deploy and manage their agents independently

---

## Why We Choose Multi-Agent Anyway 
### (Engineering Benefits)

- **Team scalability** - Different teams own different agents
- **Deployment flexibility** - Update one agent without touching others
- **Prompt maintainability** - Focused prompts are easier than mega-prompts
- **Testing isolation** - Test each agent independently
- **Failure boundaries** - Bugs in one agent don't break everything
- **Mental model clarity** - Easier to reason about specialized components

---
