# Designing Multi-Agent Systems: Challenges and Pitfalls

Prompt:

I need to give a presentation on "multi-agent" ((LLM-based) AI systems.  I want to cover:

1) What is an LLM agent?

**Industry-Standard Definition:**
> An LLM agent is an autonomous system powered by a large language model that achieves goals by **reasoning**, **planning**, **executing actions**, **observing results**, and **self-correcting** until task resolution.

This definition aligns with major industry consensus (Anthropic, OpenAI, Google/Microsoft Research) and distinguishes agents from:

- Simple LLM interactions (one-shot Q&A)
- Rigid workflows (predetermined execution paths)

**Five Core Agent Capabilities:**

1. **Reasoning** - Using LLM capabilities for logical thinking and decision-making
2. **Planning** - Creating multi-step strategies to achieve goals
3. **Acting** - Executing tools/APIs to effect change in external systems
4. **Observing** - Monitoring outcomes and environmental feedback
5. **Self-Correcting** - Adapting strategy based on results until goal achievement

Key differentiator: **Autonomy** - Agents maintain self-directed control over their process, dynamically adjusting their approach rather than following scripted paths.
2) Different ways of solving this: higher orchestration (LangGraph) vs lower orchestration (Manus), declarative (e.g., N8N, Copilot Studio) where just a prompt + set of tools are provided
3) Why do we need multi-agent?

**Multi-Agent System Definition:**
> A multi-agent system consists of two or more autonomous agents that interact through message passing, shared memory, or environment modification to achieve individual or collective goals.

**The Honest Truth About Multi-Agent Systems:**

**Reality Check:** Almost everything marketed as "multi-agent" could be done with a single agent. Multi-agent is primarily a SOFTWARE ENGINEERING choice, not an architectural requirement.

**What Single Agents Can Do:**

- Switch between different prompts/modes (adversarial validation, different expertise)
- Call different models via tool use (GPT-4 for reasoning, Claude for coding)
- Manage complex workflows with state machines
- Handle "parallel" tasks through async operations

**The ONLY Hard Requirements for Multi-Agent:**

1. **Physical parallelism with latency constraints** - When you MUST process in multiple locations simultaneously (edge computing, geographic distribution)
2. **Regulatory/security boundaries** - When different parts of the system need different data access, compliance rules, or security clearances
3. **Organizational boundaries** - When different teams/companies need to deploy and manage their agents independently

**Why We Choose Multi-Agent Anyway (Engineering Benefits):**

1. **Team scalability** - Different teams own different agents
2. **Deployment flexibility** - Update one agent without touching others
3. **Prompt maintainability** - Focused prompts are easier than mega-prompts
4. **Testing isolation** - Test each agent independently
5. **Failure boundaries** - Bugs in one agent don't break everything
6. **Mental model clarity** - Easier to reason about specialized components

**Case Study: OpenAI Deep Research**

- 4 agents: Triage → Clarifier → Instruction Builder → Research
- **Could be single agent?** YES - it's just a linear pipeline
- **Why multi-agent?** Cleaner code, better metrics, modular development
- **Is this "cheating"?** No - engineering benefits are valid reasons!

**Bottom Line:** Multi-agent systems are about HUMAN factors (development, maintenance, organization) more than technical requirements. This is fine! Good engineering is about making systems maintainable, not just technically minimal.

**Local vs. Distributed Multi-Agent Architectures:**

**Local Multi-Agent** (Single Process/Machine):

- Agents share memory space and synchronous communication
- Orchestration through function calls or shared queues
- Examples: LangGraph chains, AutoGPT loops
- Latency: ~10-100ms inter-agent
- Use case: Rapid iteration, debugging, low-latency requirements

**Distributed Multi-Agent** (Multiple Processes/Machines):

- Agents communicate via network protocols (HTTP, gRPC, message queues)
- Asynchronous message passing with eventual consistency
- Examples: A2A protocol, MCP servers, microservice architectures
- Latency: ~100ms-1s inter-agent
- Use case: Scale, fault isolation, heterogeneous compute requirements

**Key Technical Distinctions:**

- **State management**: Local uses shared memory; distributed requires explicit synchronization
- **Failure modes**: Local fails atomically; distributed enables partial failure handling
- **Resource allocation**: Local constrained by single machine; distributed scales horizontally
- **Debugging complexity**: Local allows unified tracing; distributed requires distributed tracing

4) Multi-agents can be higher orchestration (LinkedIn DragonSwarn [Agents Platform] and the Hiring Assistant]) vs lower orchestration (A2A), declarative (see OpenAI Agent SDK Cookbook for Deep Researcher)
5) Cloud vs client/edge: NVIDIA labs "Future of Agents is Small Models"

What else am I missing?

---

Based on my extensive research, I can provide you with comprehensive insights on what you might be missing from your multi-agent AI systems presentation. Your outline covers the core concepts well, but there are several important areas and emerging trends you should consider adding:

Your presentation covers solid foundational elements, but here are key areas you should consider adding to make it truly comprehensive:

## Missing Technical Architecture Components

**Context Engineering and Memory Management**[1][2][3]

- **Short-term vs. Long-term memory**: How agents maintain conversation context and learn from past interactions
- **Experiential memory**: Like LinkedIn's Hiring Assistant, where agents learn individual user preferences over time[4][5]
- **File system as context**: NVIDIA's approach of using persistent storage as extended memory for complex tasks[6]

**Agent Communication Protocols**[7][8][9]

- **Agent-to-Agent (A2A) Protocol**: Google's standardization for inter-agent communication, supported by 50+ technology partners[10]
- **Model Context Protocol (MCP)**: Anthropic's standard for agent-tool interaction
- **Hybrid approaches**: How A2A and MCP work together for comprehensive multi-agent ecosystems

## Emerging Orchestration Paradigms

**Context-Aware Orchestration**[6]

- **Attention manipulation through recitation**: How agents maintain focus on objectives during long task sequences
- **Error recovery as a feature**: Keeping failed attempts in context to improve agent learning
- **Diversity injection**: Preventing agents from falling into repetitive patterns

**Dynamic Agent Selection**[11][12]

- **LLM Routing Agents**: Intelligently directing queries to the most appropriate specialist agent
- **Adaptive orchestration**: Systems that modify agent collaboration patterns based on task complexity

## Cloud vs Edge Deployment Considerations

**NVIDIA's Small Language Models Research**[13][14][15][16][17]
Your presentation should emphasize this paradigm shift:

- **Cost efficiency**: SLMs deliver 10-30x cost reduction while maintaining 40-70% task effectiveness[14][13]
- **Latency advantages**: Edge deployment enables real-time agent responses
- **Specialization benefits**: Task-specific fine-tuning often outperforms general-purpose large models for agent workflows

## Enterprise Implementation Challenges

**Governance and Compliance**[18][19]

- **Responsible AI validation**: Microsoft's approach to declarative agents includes mandatory RAI checks
- **Human-in-the-loop safeguards**: Critical for enterprise deployment
- **Role-based access control**: Managing who can create, modify, and deploy agents

**Scalability Patterns**[20][21][22]

- **Modular architecture**: Adding new agents without system-wide retraining
- **Distributed resilience**: How multi-agent systems handle individual agent failures
- **Performance optimization**: Balancing agent specialization with coordination overhead

## Advanced Multi-Agent Patterns

**Hierarchical Agent Architectures**[23][24]

- **Meta-planning agents**: Systems like MACI that use specialized planners to coordinate other agents
- **Supervisor agents**: Coordinating workflows and breaking complex tasks into manageable components
- **Reflection and refinement loops**: Self-improving multi-agent systems

**Agent Collaboration Strategies**[25][26][27]

- **Sequential workflows**: Task handoffs between specialized agents
- **Parallel processing**: Concurrent agent operation for time-sensitive tasks
- **Consensus mechanisms**: How multiple agents reach decisions on ambiguous tasks

## Industry-Specific Applications

**Real-World Success Cases**[5][28][29][4]

- **LinkedIn Hiring Assistant**: Demonstrating 70% admin work reduction and experiential memory
- **Financial services**: Multi-agent portfolio management with specialized research agents[30]
- **Healthcare systems**: Coordinated agents for patient monitoring, diagnosis, and treatment planning[26]

## Future-Proofing Considerations

**Interoperability Standards**[8][7][10]

- **Cross-platform agent communication**: How agents from different vendors can collaborate
- **Protocol evolution**: Preparing for emerging standards like A2A and MCP
- **Vendor lock-in mitigation**: Building systems that can adapt to new frameworks

**Evaluation and Monitoring**[12][31][32]

- **Multi-agent benchmarks**: Measuring collaborative performance vs. individual agent metrics
- **Tracing and observability**: Understanding complex agent interactions for debugging and optimization
- **Continuous improvement**: How multi-agent systems learn and adapt over time

## Key Recommendations for Your Presentation

1. **Add a section on memory and context management** - This is crucial for understanding how modern agents maintain state and improve over time

2. **Include discussion of communication protocols** - A2A and MCP represent the future of agent interoperability

3. **Emphasize the cost/performance trade-offs** - NVIDIA's research on small models fundamentally challenges assumptions about agent architecture

4. **Address enterprise governance** - Real-world deployment requires robust safety and compliance frameworks

5. **Showcase concrete success stories** - LinkedIn Hiring Assistant provides measurable ROI evidence for multi-agent approaches

6. **Discuss evaluation methodologies** - How to measure and improve multi-agent system performance

Your foundational coverage of orchestration levels and deployment strategies is excellent. Adding these elements will provide a comprehensive view of where multi-agent AI systems are today and where they're heading, giving your audience both practical insights and strategic guidance for their own implementations.

[1](https://toloka.ai/blog/llm-agents-explained-revolutionizing-ai-with-reasoning-and-action/)
[2](https://blog.langchain.com/memory-for-agents/)
[3](https://arxiv.org/abs/2508.15305)
[4](https://www.reworked.co/employee-experience/linkedin-joins-the-ai-agent-trend-with-launch-of-hiring-assistant/)
[5](https://joshbersin.com/2024/10/linkedin-enters-ai-agent-race-with-linkedin-hiring-assistant/)
[6](https://manus.im/blog/Context-Engineering-for-AI-Agents-Lessons-from-Building-Manus)
[7](https://fractal.ai/blog/orchestrating-heterogeneous-and-distributed-multi-agent-systems-using-agent-to-agent-a2a-protocol)
[8](https://devblogs.microsoft.com/semantic-kernel/guest-blog-building-multi-agent-solutions-with-semantic-kernel-and-a2a-protocol/)
[9](https://towardsai.net/p/artificial-intelligence/architecting-intelligent-multi-agent-ai-systems-a2a-vs-mcp)
[10](https://developers.googleblog.com/en/a2a-a-new-era-of-agent-interoperability/)
[11](https://empathyfirstmedia.com/12-best-n8n-ai-agents-to-build-ai-workflows-2025/)
[12](https://n8n.io/ai-agents/)
[13](https://blog.premai.io/small-models-big-wins-agentic-ai-in-enterprise-explained/)
[14](https://economictimes.com/magazines/panache/nvidia-research-says-small-language-models-are-the-future-of-ai-but-why-isnt-anyone-switching-from-llms-yet-the-57-billion-reason-may-surprise-you/articleshow/123502188.cms)
[15](https://rimusz.net/nvidias-small-language-models-pioneering-the-efficient-future-of-ai-agents/)
[16](https://www.theriseunion.com/en/blog/Small-LLMs-are-future-of-AgenticAI.html)
[17](https://galileo.ai/blog/small-language-models-nvidia)
[18](https://learn.microsoft.com/en-us/microsoft-365-copilot/extensibility/overview-declarative-agent)
[19](https://learn.microsoft.com/en-us/microsoft-365-copilot/extensibility/transparency-faq-declarative-agent)
[20](https://shelf.io/blog/enterprise-ai-solutions-why-multi-agent-systems-are-your-next-business-edge-2025/)
[21](https://promethium.ai/multi-agent-workflows-the-next-evolution-of-enterprise-ai/)
[22](https://www.edgeverve.com/ai-next/blogs/multi-agent-systems-boost-enterprise-ai-innovation/)
[23](https://openreview.net/forum?id=jK4dbpEEMo)
[24](https://arxiv.org/html/2402.03578v1)
[25](https://www.getdynamiq.ai/post/multi-agent-ai-systems-definition-benefits-limitations-how-to-build)
[26](https://www.ibm.com/think/topics/multi-agent-collaboration)
[27](https://www.sprinklr.com/blog/multi-agent-ai-systems/)
[28](https://www.linkedin.com/business/talent/blog/talent-acquisition/early-impact-of-linkedin-hiring-assistant-and-ai-agent)
[29](https://www.linkedin.com/business/talent/blog/talent-acquisition/introducing-hiring-assistant)
[30](https://cookbook.openai.com/examples/agents_sdk/multi-agent-portfolio-collaboration/multi_agent_portfolio_collaboration)
[31](https://n8n.io/ai/)
[32](https://adasci.org/building-agentic-ai-applications-using-openai-agents-sdk/)
[33](https://langfuse.com/blog/2025-03-19-ai-agent-comparison)
[34](https://www.k2view.com/what-are-llm-agents/)
[35](https://oyelabs.com/guide-to-build-a-multi-ai-agent-system/)
[36](https://oxylabs.io/blog/langgraph-vs-langchain)
[37](https://developer.nvidia.com/blog/an-easy-introduction-to-llm-reasoning-ai-agents-and-test-time-scaling/)
[38](https://springsapps.com/knowledge/everything-you-need-to-know-about-multi-ai-agents-in-2024-explanation-examples-and-challenges)
[39](https://research.aimultiple.com/agentic-frameworks/)
[40](https://blog.langchain.com/planning-for-agents/)
[41](https://blog.langchain.com/how-to-think-about-agent-frameworks/)
[42](https://www.superannotate.com/blog/llm-agents)
[43](https://dev.to/aniruddhaadak/manus-ai-the-revolutionary-multi-agent-system-reshaping-the-future-of-ai-assistants-2i35)
[44](https://www.hrdive.com/news/linkedin-ai-agent-hiring-assistant/731402/)
[45](https://smythos.com/developers/agent-comparisons/smythos-vs-manus-ai-report/)
[46](https://docs.n8n.io/advanced-ai/intro-tutorial/)
[47](https://www.baytechconsulting.com/blog/manus-ai-an-analytical-guide-to-the-autonomous-ai-agent-2025)
[48](https://www.linkedin.com/blog/engineering/generative-ai/the-tech-behind-the-first-agent-from-linkedin-hiring-assistant)
[49](https://www.siddharthbharath.com/openai-agents-sdk/)
[50](https://cookbook.openai.com/examples/deep_research_api/introduction_to_deep_research_api_agents)
[51](https://www.oreilly.com/radar/designing-collaborative-multi-agent-systems-with-the-a2a-protocol/)
[52](https://www.reddit.com/r/AI_Agents/comments/1jk14wz/open_source_deep_research_using_the_openai_agents/)
[53](https://learn.microsoft.com/en-us/microsoft-365-copilot/extensibility/declarative-agent-instructions)
[54](https://www.llmwatch.com/p/think-like-an-ai-agent-introduction)
[55](https://learn.microsoft.com/en-us/microsoft-365-copilot/extensibility/declarative-agent-tool-comparison)
[56](https://learn.microsoft.com/en-us/microsoft-365-copilot/extensibility/copilot-studio-agent-builder)
[57](http://llmagents-learning.org/slides/language_agents_YuSu_Berkeley.pdf)
[58](https://www.reddit.com/r/copilotstudio/comments/1md3sj9/declarative_agents_are_so_much_better/)
[59](https://www.youtube.com/watch?v=5Is4d50H9I8)
