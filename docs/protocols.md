Several common agent-agent protocols are used in multi-agent systems beyond A2A, including both industry and research standards. The most prominent alternatives are **MCP (Model Context Protocol)**, **ACP (Agent Communication Protocol)**, **FIPA-ACL** (Agent Communication Language), **KQML** (Knowledge Query and Manipulation Language), and network-oriented protocols like **XMPP**, **WebRTC**, and **gRPC**.[1][2][3][4]

## Major Agent-Agent Protocols

### Model Context Protocol (MCP)

- Designed for broader interoperability and tool invocation across agents, often acting as an internal resource registry that enables agents to understand and call available tools and workflows.[5][6][1]
- Supports standardized context passing and data exchange, underlying complex agent environments.

### Agent Communication Protocol (ACP)

- An open standard developed by IBM to facilitate reliable local agent communication with dynamic agent discovery, registration, and structured messaging.[7][4][1][5]
- Well-suited for controlled enterprise environments—enables task delegation, local constraints, and robust message handling across agents with shared context.

### FIPA-ACL (Foundation for Intelligent Physical Agents - Agent Communication Language)

- A widely adopted standard specifying the semantics, syntax, and pragmatics for messages exchanged among agents, supporting performative speech acts (e.g., inform, request, negotiate).[8][3][9][10]
- Ensures strong interoperability for agents built by different vendors.

### KQML (Knowledge Query and Manipulation Language)

- Another well-known language and protocol for knowledge sharing and reasoning among agents.[3][9][10]
- Offers flexible support for different knowledge bases and reasoning systems.

## Core Agent-Agent Communication Protocols

### ANP (Agent Network Protocol)

**ANP** is indeed a legitimate agent-agent protocol designed for decentralized, peer-to-peer communication between autonomous agents. It features:[1][2][3]

- **Decentralized identity** using W3C DIDs (Decentralized Identifiers)
- **Three-layer architecture**: identity/encryption, meta-protocol negotiation, and application protocols
- **Agent discovery** through standardized `.well-known/agent-descriptions` endpoints
- **Direct agent-to-agent** communication without centralized servers[4][2][1]

### FIPA-ACL (Foundation for Intelligent Physical Agents - Agent Communication Language)

**FIPA-ACL** is a standardized protocol specifically for agent-to-agent communication:[5][6][7]

- **Speech act-based** messaging with performatives (inform, request, query-ref, etc.)
- **Structured message format** with sender, receiver, content, and ontology fields
- **JADE platform** implementation provides full FIPA-ACL support for multi-agent systems[6][5]
- **Enterprise-grade** standardization for reliable agent interoperability[7]

### KQML (Knowledge Query and Manipulation Language)

**KQML** is a foundational agent communication protocol:[8][9][7]

- **Three-layer architecture**: content, communication, and message layers
- **Performative-based** communication (ask-one, tell, subscribe, etc.)
- **Facilitator services** for agent discovery and brokering[8]
- **Flexible content languages** allowing different knowledge representations[7]

### OAA-ICL (Open Agent Architecture - Interagent Communication Language)

**OAA** provides agent-to-agent communication through its ICL:[10][11][8]

- **Facilitator-mediated** communication between distributed agents
- **Capability-based** agent discovery and task delegation
- **Speech-act foundation** similar to FIPA but with OAA-specific extensions[10]
- **Distributed agent services** coordination[11]

## Summary of Actual Agent-Agent Protocols

| Protocol | Focus | Communication Model | Key Features |
|----------|-------|-------------------|--------------|
| **ANP** | Decentralized P2P | Direct agent-to-agent | DID identity, meta-protocol negotiation |
| **FIPA-ACL** | Standardized messaging | Agent message passing | Speech acts, JADE implementation |
| **KQML** | Knowledge sharing | Facilitator-mediated | Three-layer architecture, flexible content |
| **OAA-ICL** | Distributed services | Facilitator-based | Capability matching, task delegation |

These are the actual agent-agent protocols designed specifically for autonomous agent communication, unlike network transport protocols (gRPC, WebRTC) which are general-purpose communication mechanisms.[1][5][6][7][8]
