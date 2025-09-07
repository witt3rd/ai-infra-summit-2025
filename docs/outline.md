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
- Speaker introduction and credentials
- **Two Running Examples**: 
  1. Academic Research Assistant - "Understanding latest developments in small language models for edge AI" (conceptual walkthrough)
  2. LinkedIn Hiring Assistant - Production-scale distributed multi-agent system (real-world implementation)
- Preview: We'll contrast theoretical patterns with LinkedIn's production solutions throughout
- The efficiency imperative: Are we building multi-agent systems intelligently?

### 2. Foundations: What is an Agent? (3 minutes)

- Definition: Autonomous systems with reasoning, planning, acting capabilities
- Five core capabilities: Reasoning, Planning, Acting, Observing, Self-Correcting
- **Scenario Example**: Single agent attempting our research task - one model doing clarification, search, synthesis sequentially
- Key differentiator: Autonomy vs scripted automation
- Quote: "agents are different than sort of chat bots because they're able to go off and do multiple steps on their own. They don't require human interaction at every step" (RMntjZvNo8s.txt:114-119)
- Common misconceptions about agent capabilities
- **The Cost Reality**: "agents really, really up the token cost... there can be a lot of tokens generated, a lot of compute resources burned" (RMntjZvNo8s.txt:121-129)

### 3. Multi-Agent Systems Defined (3 minutes)

- Two or more agents collaborating (cooperative or adversarial)
- **Scenario Evolution**: Split into 3 agents - Clarification, Sequential Search, Web Search
  - Why split? Specialization, parallel execution, independent scaling
- **LinkedIn Reality**: Supervisor pattern with specialized agents (Sourcing, Evaluation, Communication, Memory Management)
  - Built on LangGraph with 20+ teams building agents simultaneously
- Communication patterns: message passing, direct invocation, shared memory, environment modification
- **LinkedIn's Solution**: Leveraged existing messaging service for agent-to-agent communication
  - Message persistence, near-line recovery, proven scale (1.2B+ users)
- Why multi-agent: The honest truth about engineering choices vs technical requirements

### 4. Multi-Agent Architecture Spectrum (5 minutes)

#### Local vs Distributed Execution

- **Local**: Single process, shared memory, ~10-100ms latency (LangGraph, AutoGPT)
  - **Scenario**: All 3 agents in LocalResearchSystem class, direct function calls
- **Distributed**: Network protocols, async messaging, ~100ms-1s latency (A2A, MCP)
  - **Scenario**: ClarificationService, SearchService, WebSearchService as microservices
  - **LinkedIn AgentsOS**: 4-layer platform - Orchestration, Prompt Engineering, Skills/Tools, Memory
    - Python SDK with distributed orchestration, retry logic, traffic shifting
    - Cross-language support for diverse tech stack
- State management and consistency trade-offs
  - **Scenario**: Local uses shared_memory dict vs Distributed uses Redis
  - **LinkedIn**: 3-tier memory (Working, Long-Term, Collective) with messaging-based persistence

#### Orchestration Patterns

- **High orchestration**: Centralized control
  - **Scenario**: Sequential Search Agent orchestrates multiple Web Search Agents
  - **LinkedIn**: Supervisor multi-agent pattern with ambient agent design
    - Central supervisor coordinates specialized sub-agents
    - Asynchronous execution with progress notifications
- **Low orchestration**: Peer-to-peer (A2A protocol)
  - **Scenario**: Agents discover and communicate directly via A2A
- **Declarative**: Configuration-driven (Copilot Studio, N8N)
  - **Scenario**: YAML configuration defines agent workflow
- **Programmatic**: Code-first (Manus, OpenAI SDK)
  - **Scenario**: Python classes with custom logic
  - **LinkedIn**: Custom engine with LangGraph integration - binary serialization, streaming support
- **Emerging Pattern - Agents as Tools**: OpenAI SDK approach
  - **Scenario**: Research agents become callable tools in hierarchical system

### 5. Communication Protocols and Interoperability (4 minutes)

#### Modern Protocols - Scenario Implementations

- **A2A (Agent-to-Agent)**: Google's standard with 50+ partners
  - **Scenario**: Clarification discovers Search via A2A, negotiates capabilities
- **MCP (Model Context Protocol)**: Anthropic's tool/resource interaction standard
  - **Scenario**: Web Search Agent exposes search tools via MCP
- **LinkedIn's Messaging Infrastructure**: Production-proven distributed communication
  - Event-driven architecture with message persistence
  - Near-line flow recovery through queuing systems
  - Handles agent-to-agent and user-to-agent messaging at scale
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
  - **LinkedIn**: Distributed processing across regions for global scale
- Regulatory/security boundaries
  - **Scenario**: Clarification handles PII, Search operates in DMZ
  - **LinkedIn**: Centralized skill registry with access control and security management
- Organizational boundaries
  - **Scenario**: Different teams own Clarification vs Search capabilities
  - **LinkedIn**: 20+ teams building agents on shared platform infrastructure

#### Engineering Benefits (Why We Choose It Anyway)

- Team scalability and ownership
  - **Scenario**: Search team can update independently
  - **LinkedIn**: Independent agent development with centralized platform utilities
- Deployment flexibility
  - **Scenario**: Scale Web Search to 10 replicas during peak
  - **LinkedIn**: Traffic shifting, gradual rollouts, horizontal scaling of individual agents
- Prompt maintainability
  - **Scenario**: 50-line prompts vs 500-line mega-prompt
  - **LinkedIn**: 30% prompt size reduction through model training
- Testing isolation
  - **Scenario**: Mock Web Search to test Sequential Search
  - **LinkedIn**: Replay capabilities for debugging and optimization
- Failure boundaries
  - **Scenario**: Clarification continues even if Search fails
  - **LinkedIn**: Independent failure domains with circuit breaker patterns

### 7. Memory and Context Engineering (3 minutes)

- Short-term vs long-term memory
  - **Scenario**: Conversation history vs learned search patterns
  - **LinkedIn's 3-Tier Memory Architecture**:
    - Working Memory: LLM context windows for real-time processing
    - Long-Term Memory: Messaging infrastructure persistence with semantic search
    - Collective Memory: Shared knowledge across all agent interactions
- Experiential learning
  - **Scenario**: System learns user prefers academic sources over blogs
  - **LinkedIn**: Learning from successful recruiting patterns and outcomes across agents
- Context-aware orchestration
  - **Scenario**: Sequential Search maintains research plan in context
  - **LinkedIn**: Conversational Memory with LangChain abstractions
    - Semantic search for relevant history retrieval
    - Context summarization for long threads
    - Cross-device synchronization
- Persistent storage strategies
  - **Scenario**: PostgreSQL for result indexing, Redis for session state
  - **LinkedIn**: Messaging-based persistence with checkpointing for recovery
- Hierarchical reasoning in multi-agent systems
  - **Scenario**: GPT-4 plans research strategy, smaller models execute searches
  - **LinkedIn**: Model switching via simple parameters (OpenAI, internal EON models)

### 8. Implementation Challenges and Pitfalls (4 minutes)

- **Communication overhead**: Coordination costs exceeding benefits
  - **Scenario**: 3 agents × 5 rounds = 15 inter-agent messages for one query
  - **LinkedIn Solution**: Event-driven architecture with optimized message passing
- **The Optimization Challenge**: Quote: "when you're first trying to do something... you just throw everything at the wall... But the consequence of that methodology is that you're not optimized at all" (RMntjZvNo8s.txt:26-35)
  - Moving from exploration to optimization in multi-agent design
- **State consistency**: Distributed consensus challenges
  - **Scenario**: Race condition when multiple searches update same context
  - **LinkedIn Solution**: Message persistence with near-line recovery, checkpointing
- **Debugging complexity**: Distributed tracing, emergent behaviors
  - **Scenario**: Which agent caused the hallucination? Trace through 3 services
  - **LinkedIn Solution**: Comprehensive observability - analytics layer, replay capabilities, performance metrics
- **Agent sprawl**: Over-decomposition problems
  - **Scenario**: Do we need separate Citation Agent? Format Agent? Too many?
  - **LinkedIn Approach**: Centralized skill registry instead of proliferating agents
- **Performance degradation**: Cascading latencies
  - **Scenario**: 100ms + 500ms + 200ms = 800ms total latency
  - **LinkedIn Solution**: Re-engineered orchestration for fast, fluid conversations
    - Intelligent caching strategies
    - Load balancing across regions
- **Fault Tolerance Requirements**:
  - **LinkedIn's Multi-Layer Resilience**:
    - Message-level: Automatic retry through queuing systems
    - Agent-level: Independent failure domains, graceful degradation
    - System-level: Circuit breaker patterns prevent cascading failures

### 9. Emerging Trends (2 minutes)

- **Small Language Models (SLMs) in Multi-Agent Systems**: 10-30x cost reduction
  - **Scenario**: Replace GPT-3.5 Web Search with 7B parameter SLM
  - **LinkedIn**: Custom EON models achieve 75x cost reduction vs GPT-4
  - Quote from NVIDIA: "small language models or slims are sufficiently powerful, inherently more suitable, and necessarily more economical for many invocations in agentic systems" (RMntjZvNo8s.txt:132-137)
- **Edge deployment for privacy/latency**
  - **Scenario**: Clarification runs locally on user device for privacy
  - Definition: "a slim is a language model that can fit into a common consumer electronic device and perform inference with latency sufficiently low" (RMntjZvNo8s.txt:186-191)
- **Hybrid Architectures**: Combining SLMs and LLMs
  - Hybrid future: "a hybridized system of these slims and large language models each handling tasks that are better suited for what they need to do" (RMntjZvNo8s.txt:519-522)
  - Hierarchical reasoning: "a high-level module responsible for slow abstract planning... and a low-level module handling rapid detailed computations" (RMntjZvNo8s.txt:259-264)
  - Orchestration pattern: "the large language model acts as an orchestrator, like a conductor of the orchestra" (RMntjZvNo8s.txt:491-493)
- **Platform Engineering for AI Agents**
  - **LinkedIn AgentsOS**: Foundation for future agent development
    - Standard utilities for tool calling, LLM inference, checkpointing
    - Version management and controlled rollouts
    - Skills as a service model
- **Enterprise governance and RAI**
  - **Scenario**: Microsoft RAI validation for each agent before deployment
  - **LinkedIn**: Governance built into platform with access control and audit trails

### 10. Real-World Case Studies & Framework Comparison (2 minutes)

- **LinkedIn Hiring Assistant - Production Multi-Agent at Scale**:
  - **Architecture**: Supervisor pattern on LangGraph with distributed execution
  - **Platform**: AgentsOS - 4-layer infrastructure supporting 20+ teams
  - **Memory**: 3-tier system (Working, Long-Term, Collective) with messaging persistence
  - **Skills Registry**: Centralized catalog with version management
  - **Results**: 
    - 70% admin task reduction
    - Users double the roles they can support
    - Charter customer program success
    - 75x cost reduction with custom EON models
  - **Key Innovation**: Leveraged existing LinkedIn infrastructure (messaging, scale, reliability)
- **Our Research Scenario Across Frameworks**:
  - **LangChain/LangGraph**: LinkedIn's choice - streaming, binary serialization, cross-language
  - **CrewAI**: Role-based crews with automatic delegation
  - **OpenAI SDK**: Agents-as-tools pattern, integrated tracing
  - **Microsoft Copilot Studio**: Declarative agents, enterprise RAI
- OpenAI Deep Research: 3-agent modular architecture (our scenario inspiration)
- **The Efficiency Imperative**:
  - Quote: "most tasks only require minimal smarts... it's overkill to be using these gigantic models" (RMntjZvNo8s.txt:356-360)
  - Quote: "We're just burning massive amounts of compute resources to make these models actually work" (RMntjZvNo8s.txt:57-61)
  - Quote: "we're actually moving from a period of exploration to a period of optimization" (RMntjZvNo8s.txt:22-24)
- **Evolution Path**: 
  - MVP: Local + Declarative (rapid prototyping)
  - Growth: Local + Custom (LinkedIn's initial phase)
  - Scale: Distributed + Custom Engine (LinkedIn's production system)
