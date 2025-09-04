# Designing Multi-Agent Systems

## Timing

    30 minutes presentation

## Goals

This presentation provides a comprehensive exploration of multi-agent AI systems, covering the full spectrum of architectures, implementation patterns, and real-world challenges.

Key objectives:

1. **Define the multi-agent landscape** - Cover the breadth of multi-agent architectures from local to distributed, declarative to programmatic
2. **Explore communication protocols** - Deep dive into A2A, MCP, and emerging standards for agent interoperability
3. **Architectural patterns** - Compare orchestration approaches (high vs low), memory management, and context engineering
4. **Decision framework** - When to choose multi-agent: hard requirements vs engineering benefits vs organizational needs
5. **Implementation challenges** - Address real pitfalls: communication overhead, state consistency, debugging complexity, agent sprawl
6. **Emerging trends** - Small language models, edge deployment, hierarchical architectures, and enterprise governance

The presentation balances theoretical understanding with practical implementation guidance, helping architects and engineers navigate the complex multi-agent ecosystem to build effective, maintainable systems.

I've woven the Academic Research Assistant scenario throughout the entire outline as a canonical example. Here's what I've added:

Key Scenario Integration Points:

1. Introduction: Introduces the scenario that will thread through the entire presentation
2. Single vs Multi-Agent: Shows evolution from 1 agent to 3 specialized agents (Clarification, Sequential Search, Web Search)
3. Declarative vs Custom: Contrasts YAML configuration with Python implementation
4. Local vs Distributed: Same agents running in shared memory vs microservices
5. Protocols: Shows how each protocol (A2A, MCP, ANP, ACP, FIPA-ACL) would handle agent communication
6. Orchestration: Demonstrates high vs low orchestration patterns
7. Memory/State: Different approaches (shared_memory dict, Redis, PostgreSQL)
8. Decision Points: Concrete examples of when/why to split into multiple agents
9. Challenges: Specific metrics (15 messages, 800ms latency, 9 test patterns)
10. Trends: SLMs replacing components, edge deployment for privacy
11. Frameworks: How to implement in LangChain, CrewAI, OpenAI SDK, Google ADK, Microsoft Copilot Studio
12. Evolution Path: MVP → Growth → Scale journey

  This canonical scenario now provides:

- Consistency: Same example throughout for easier understanding
- Concreteness: Real numbers, code snippets, architectural choices
- Comparison: Shows trade-offs at every decision point
- Practicality: Based on real OpenAI Deep Research implementation

  The audience will leave understanding exactly how to think about and implement multi-agent systems through this comprehensive example.

## Outline

### 1. Introduction (2 minutes)

- The multi-agent AI landscape: From hype to reality
- Quote: "we're actually moving from a period of exploration to a period of optimization" (RMntjZvNo8s.txt:22-24)
- Speaker introduction and credentials
- **Canonical Scenario Introduction**: Academic Research Assistant - "Understanding latest developments in small language models for edge AI"
- Preview: We'll use this scenario throughout to explore every aspect of multi-agent systems
- The $57 billion question: Are we doing this efficiently?

### 2. Foundations: What is an Agent? (3 minutes)

- Definition: Autonomous systems with reasoning, planning, acting capabilities
- Five core capabilities: Reasoning, Planning, Acting, Observing, Self-Correcting
- **Scenario Example**: Single agent attempting our research task - one model doing clarification, search, synthesis sequentially
- Key differentiator: Autonomy vs scripted automation
- Quote: "agents are different than sort of chat bots because they're able to go off and do multiple steps on their own. They don't require human interaction at every step" (RMntjZvNo8s.txt:114-119)
- Common misconceptions about agent capabilities
- Warning: "agents really, really up the token cost... there can be a lot of tokens generated, a lot of compute resources burned" (RMntjZvNo8s.txt:121-129)

### 3. Multi-Agent Systems Defined (3 minutes)

- Two or more agents collaborating (cooperative or adversarial)
- **Scenario Evolution**: Split into 3 agents - Clarification, Sequential Search, Web Search
  - Why split? Specialization, parallel execution, independent scaling
- Communication patterns: message passing, direct invocation, shared memory, environment modification
- **Scenario Communication**: Agents share research context via environment state
- Why multi-agent: The honest truth about engineering choices vs technical requirements
- Quote on efficiency: "You're just burning massive amounts of compute resources to make these models actually work" (RMntjZvNo8s.txt:57-61)

### 4. Multi-Agent Architecture Spectrum (5 minutes)

#### Local vs Distributed Execution

- **Local**: Single process, shared memory, ~10-100ms latency (LangGraph, AutoGPT)
  - **Scenario**: All 3 agents in LocalResearchSystem class, direct function calls
- **Distributed**: Network protocols, async messaging, ~100ms-1s latency (A2A, MCP)
  - **Scenario**: ClarificationService, SearchService, WebSearchService as microservices
- State management and consistency trade-offs
- **Scenario State**: Local uses shared_memory dict vs Distributed uses Redis
- Quote on edge computing: "a slim is a language model that can fit into a common consumer electronic device and perform inference with latency sufficiently low" (RMntjZvNo8s.txt:186-191)

#### Orchestration Patterns

- **High orchestration**: Centralized control (LinkedIn DragonSwarm)
  - **Scenario**: Sequential Search Agent orchestrates multiple Web Search Agents
- Quote: "the large language model acts as an orchestrator, like a conductor of the orchestra" (RMntjZvNo8s.txt:491-493)
- **Low orchestration**: Peer-to-peer (A2A protocol)
  - **Scenario**: Agents discover and communicate directly via A2A
- **Declarative**: Configuration-driven (Copilot Studio, N8N)
  - **Scenario**: YAML configuration defines agent workflow
- **Programmatic**: Code-first (Manus, OpenAI SDK)
  - **Scenario**: Python classes with custom logic
- **Emerging Pattern - Agents as Tools**: OpenAI SDK approach
  - **Scenario**: Research agents become callable tools in hierarchical system

### 5. Communication Protocols and Interoperability (4 minutes)

#### Modern Protocols - Scenario Implementations

- **A2A (Agent-to-Agent)**: Google's standard with 50+ partners
  - **Scenario**: Clarification discovers Search via A2A, negotiates capabilities
- **MCP (Model Context Protocol)**: Anthropic's tool/resource interaction standard
  - **Scenario**: Web Search Agent exposes search tools via MCP
- **ANP (Agent Network Protocol)**: Decentralized P2P with DID identity
  - **Scenario**: Agents authenticate using DIDs, direct P2P communication
- **ACP (Agent Communication Protocol)**: IBM's enterprise agent communication
  - **Scenario**: Enterprise deployment with ACP message routing

#### Foundational Standards

- **FIPA-ACL**: Speech act-based messaging (inform, request, negotiate)
  - **Scenario**: "request" performative for search, "inform" for results
- **KQML**: Knowledge sharing and reasoning protocol
- **OAA-ICL**: Facilitator-mediated distributed services

#### Protocol Selection Considerations

- Direct vs facilitator-mediated communication
- **Scenario Trade-offs**: A2A for flexibility vs ACP for enterprise control
- Network transport (gRPC, WebRTC) vs agent-specific protocols

### 6. When to Choose Multi-Agent (4 minutes)

#### Hard Requirements (The Only True Needs)

- Physical parallelism with latency constraints
  - **Scenario**: Web Search Agent needs to hit multiple APIs simultaneously
- Regulatory/security boundaries
  - **Scenario**: Clarification handles PII, Search operates in DMZ
- Organizational boundaries
  - **Scenario**: Different teams own Clarification vs Search capabilities

#### Engineering Benefits (Why We Choose It Anyway)

- Team scalability and ownership
  - **Scenario**: Search team can update independently
- Deployment flexibility
  - **Scenario**: Scale Web Search to 10 replicas during peak
- Prompt maintainability
  - **Scenario**: 50-line prompts vs 500-line mega-prompt
- Testing isolation
  - **Scenario**: Mock Web Search to test Sequential Search
- Failure boundaries
  - **Scenario**: Clarification continues even if Search fails

### 7. Memory and Context Engineering (3 minutes)

- Short-term vs long-term memory
  - **Scenario**: Conversation history vs learned search patterns
- Experiential learning (LinkedIn example)
  - **Scenario**: System learns user prefers academic sources over blogs
- Context-aware orchestration
  - **Scenario**: Sequential Search maintains research plan in context
- Persistent storage strategies (NVIDIA approach)
  - **Scenario**: PostgreSQL for result indexing, Redis for session state
- **Citation Preservation**: Critical for research credibility
  - **Scenario**: Automatic source tracking through agent handoffs
- Hierarchical reasoning: "a high-level module responsible for slow abstract planning... and a low-level module handling rapid detailed computations" (RMntjZvNo8s.txt:259-264)
  - **Scenario**: GPT-4 plans research strategy, GPT-3.5 executes searches
- Quote: "With only 27 million parameters... HRM achieves exceptional performance on complex reasoning tasks using only 1,000 training samples" (RMntjZvNo8s.txt:280-286)

### 8. Implementation Challenges and Pitfalls (4 minutes)

- **Communication overhead**: Coordination costs exceeding benefits
  - **Scenario**: 3 agents × 5 rounds = 15 inter-agent messages for one query
- Quote: "when you're first trying to do something... you just throw everything at the wall... But the consequence of that methodology is that you're not optimized at all" (RMntjZvNo8s.txt:26-35)
- **State consistency**: Distributed consensus challenges
  - **Scenario**: Race condition when multiple searches update same context
- **Debugging complexity**: Distributed tracing, emergent behaviors
  - **Scenario**: Which agent caused the hallucination? Trace through 3 services
- **Agent sprawl**: Over-decomposition problems
  - **Scenario**: Do we need separate Citation Agent? Format Agent? Too many?
- **Performance degradation**: Cascading latencies
  - **Scenario**: 100ms + 500ms + 200ms = 800ms total latency
- **Testing complexity**: Interaction pattern explosion
  - **Scenario**: 3 agents = 9 possible interaction patterns to test
- **Emergent Behaviors**: Unexpected patterns from agent interactions
  - **Scenario**: Agents develop circular dependencies not present in individual testing

### 9. Emerging Trends (2 minutes)

- Small Language Models: 10-30x cost reduction
  - **Scenario**: Replace GPT-3.5 Web Search with 7B parameter SLM
- Quote from NVIDIA: "small language models or slims are sufficiently powerful, inherently more suitable, and necessarily more economical for many invocations in agentic systems" (RMntjZvNo8s.txt:132-137)
- Edge deployment for privacy/latency
  - **Scenario**: Clarification runs locally on user device for privacy
- Quote: "Tesla has had to do this for years because they're trying to do realtime full self-driving on the road with limited compute resources" (RMntjZvNo8s.txt:64-67)
- Hierarchical architectures
  - **Scenario**: GPT-4 orchestrates multiple Llama-3 search agents
- Hybrid future: "a hybridized system of these slims and large language models each handling tasks that are better suited for what they need to do" (RMntjZvNo8s.txt:519-522)
- Enterprise governance and RAI
  - **Scenario**: Microsoft RAI validation for each agent before deployment

### 10. Real-World Case Studies & Framework Comparison (2 minutes)

- **Our Scenario Across Frameworks** (with unique strengths):
  - **LangChain**: Graph-based workflows, deterministic replay, 110k+ stars
  - **CrewAI**: Role-based crews with automatic delegation, Oracle/Deloitte
  - **OpenAI SDK**: Agents-as-tools pattern, integrated tracing
  - **Pydantic AI**: Type-safe with dependency injection, structured outputs
  - **Smolagents**: Minimalist ~1000 LOC, code-centric execution
  - **AutoGen**: Conversational multi-agent, event-driven architecture
  - **Google ADK**: Gemini integration, built-in handoffs
  - **Microsoft Copilot Studio**: Declarative agents, enterprise RAI
- LinkedIn Hiring Assistant: 70% admin reduction
- OpenAI Deep Research: 3-agent modular architecture (our scenario inspiration)
- Tesla FSD example: "you could be driving down the road and maybe a tornado... that's beyond what the basic full self-driving is able to handle... pass that off to a model like Grock" (RMntjZvNo8s.txt:463-483)
- **Scenario Evolution Path**: MVP (Declarative+Local) → Growth (Custom+Local) → Scale (Custom+Distributed)
- **Framework Selection Matrix**: Development speed vs flexibility vs scalability
- Key lessons learned: "most tasks only require minimal smarts... it's overkill to be using these gigantic models" (RMntjZvNo8s.txt:356-360)
