# Deep Research Multi-Agent Scenario

## Base Scenario: Academic Research Assistant

**User Request**: "I need to understand the latest developments in small language models for edge AI, including performance benchmarks, deployment strategies, and real-world applications in robotics."

This scenario demonstrates how the same research task can be implemented across different multi-agent architectures and execution contexts.

---

## 1. Declarative vs Custom Engine Multi-Agent

### A. Declarative Multi-Agent (Configuration-Driven)

**Implementation**: Using Microsoft Copilot Studio or N8N

```yaml
# declarative_research_agents.yaml
agents:
  - name: Clarification_Agent
    type: standard
    model: gpt-4
    instructions: |
      Analyze research requests for ambiguities.
      Generate clarifying questions.
      No external tools required.
    handoff_to: Sequential_Search_Agent
    
  - name: Sequential_Search_Agent
    type: orchestrator
    model: gpt-4
    max_iterations: 5
    tools:
      - web_search
      - document_parser
    instructions: |
      Plan multi-step research strategy.
      Synthesize results from searches.
    uses_agent: WebSearch_Agent
    
  - name: WebSearch_Agent
    type: tool_executor
    model: gpt-3.5-turbo
    tools:
      - web_search_api
      - result_formatter
    output_formats: [json, text]

workflow:
  trigger: user_message
  steps:
    1: Clarification_Agent.analyze()
    2: Sequential_Search_Agent.research()
    3: format_and_return_results()
```

**Characteristics**:
- No code required, pure configuration
- Limited to predefined patterns
- Easy to modify workflow
- Constrained by platform capabilities

### B. Custom Engine Multi-Agent (Code-First)

**Implementation**: Using OpenAI Agents SDK or LangGraph

```python
# custom_research_agents.py
class ClarificationAgent:
    def __init__(self, model="gpt-4"):
        self.model = model
        self.memory = ConversationMemory()
    
    def analyze(self, request):
        # Custom logic for ambiguity detection
        ambiguities = self._detect_ambiguities(request)
        if ambiguities:
            questions = self._generate_questions(ambiguities)
            return {"status": "needs_clarification", "questions": questions}
        return {"status": "ready", "refined_query": self._refine(request)}
    
    def _detect_ambiguities(self, request):
        # Custom NLP analysis
        # Domain-specific rules
        # Historical context checking
        pass

class SequentialSearchAgent:
    def __init__(self, web_agent):
        self.web_agent = web_agent
        self.planner = ResearchPlanner()
        self.synthesizer = ResultSynthesizer()
    
    async def research(self, query):
        # Dynamic planning based on query complexity
        plan = await self.planner.create_plan(query)
        
        # Parallel execution of searches
        tasks = []
        for step in plan.steps:
            tasks.append(self.web_agent.search(step))
        
        results = await asyncio.gather(*tasks)
        
        # Custom synthesis with citations
        return self.synthesizer.combine(results, plan)

class WebSearchAgent:
    def __init__(self, config):
        self.search_apis = [GoogleAPI(), BingAPI(), ScholarAPI()]
        self.rate_limiter = RateLimiter(config.limits)
        self.cache = RedisCache()
    
    async def search(self, query):
        # Check cache first
        if cached := await self.cache.get(query):
            return cached
        
        # Intelligent API selection
        api = self._select_best_api(query)
        
        # Rate-limited search with retry logic
        result = await self.rate_limiter.execute(
            api.search, query, retries=3
        )
        
        # Cache and return
        await self.cache.set(query, result)
        return result
```

**Characteristics**:
- Full control over agent behavior
- Custom memory and state management
- Advanced features (caching, rate limiting)
- Complex debugging and maintenance

---

## 2. Local vs Distributed Execution

### A. Local Execution (Single Process)

**Architecture**: All agents run in the same process/container

```python
# local_execution.py
class LocalResearchSystem:
    def __init__(self):
        # All agents in same memory space
        self.clarifier = ClarificationAgent()
        self.searcher = SequentialSearchAgent(
            WebSearchAgent(local_config)
        )
        
        # Shared memory for fast communication
        self.shared_memory = {
            "conversation": [],
            "research_context": {},
            "results_cache": {}
        }
    
    async def process_request(self, request):
        # Direct function calls, no network overhead
        clarified = self.clarifier.analyze(request)
        
        # Immediate memory access
        self.shared_memory["conversation"].append(clarified)
        
        # Synchronous handoff
        if clarified["status"] == "ready":
            results = await self.searcher.research(
                clarified["refined_query"]
            )
            return self._format_results(results)
        
        return clarified["questions"]

# Deployment
if __name__ == "__main__":
    system = LocalResearchSystem()
    # Single process handles everything
    uvicorn.run(app, host="0.0.0.0", port=8000)
```

**Performance Profile**:
- Latency: ~10-100ms between agents
- Memory: Shared, efficient
- Scaling: Vertical only (bigger machine)
- Failure: All agents fail together

### B. Distributed Execution (Multiple Services)

**Architecture**: Agents run as separate services

```python
# distributed_execution.py

# Clarification Service (Container 1)
class ClarificationService:
    def __init__(self):
        self.agent = ClarificationAgent()
        self.message_bus = RabbitMQ()
        self.service_registry = Consul()
    
    async def start(self):
        # Register with service discovery
        await self.service_registry.register(
            "clarification-agent", 
            self.config.host, 
            self.config.port
        )
        
        # Subscribe to message queue
        await self.message_bus.subscribe(
            "research.requests",
            self.handle_request
        )
    
    async def handle_request(self, message):
        result = self.agent.analyze(message.data)
        
        # Async message passing
        if result["status"] == "ready":
            await self.message_bus.publish(
                "search.requests",
                result["refined_query"]
            )
        else:
            await self.message_bus.publish(
                "user.responses",
                result["questions"]
            )

# Search Service (Container 2)
class SearchService:
    def __init__(self):
        self.agent = SequentialSearchAgent()
        self.message_bus = RabbitMQ()
        self.state_store = Redis()
    
    async def handle_search(self, message):
        # Retrieve distributed state
        context = await self.state_store.get(
            f"context:{message.session_id}"
        )
        
        # Execute search with distributed web agents
        web_agent_urls = await self.discover_web_agents()
        results = await self.distributed_search(
            message.query, 
            web_agent_urls
        )
        
        # Store results in distributed cache
        await self.state_store.set(
            f"results:{message.session_id}",
            results
        )

# Web Search Service (Container 3-N, horizontally scaled)
class WebSearchService:
    def __init__(self, instance_id):
        self.instance_id = instance_id
        self.agent = WebSearchAgent()
        self.health_check = HealthCheck()
    
    async def search_endpoint(self, request):
        # Load balancer distributes requests
        return await self.agent.search(request.query)

# Kubernetes Deployment
"""
apiVersion: apps/v1
kind: Deployment
metadata:
  name: clarification-agent
spec:
  replicas: 2
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: search-orchestrator
spec:
  replicas: 3
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: web-search-agent
spec:
  replicas: 10  # Horizontal scaling
"""
```

**Performance Profile**:
- Latency: ~100ms-1s between agents
- Memory: Distributed, requires synchronization
- Scaling: Horizontal (add more instances)
- Failure: Partial failures possible, resilient

---

## Comparison Matrix

| Aspect | Declarative | Custom Engine | Local | Distributed |
|--------|------------|---------------|-------|-------------|
| **Development Speed** | Fast | Slow | Medium | Slow |
| **Flexibility** | Low | High | High | High |
| **Debugging** | Easy | Complex | Medium | Very Complex |
| **Scalability** | Platform-limited | Unlimited | Vertical | Horizontal |
| **Latency** | Medium | Variable | Low | High |
| **Cost** | Low initial | High initial | Low | High |
| **Maintenance** | Easy | Complex | Medium | Complex |
| **Fault Tolerance** | Basic | Customizable | None | High |

## Real-World Application

### Scenario Evolution:

1. **MVP (Declarative + Local)**: 
   - Quick prototype using Copilot Studio
   - Single-server deployment
   - Good for <100 requests/minute

2. **Growth (Custom + Local)**:
   - Add caching, custom logic
   - Optimize for specific domains
   - Handle 1000 requests/minute

3. **Scale (Custom + Distributed)**:
   - Microservice architecture
   - Geographic distribution
   - Handle 100,000+ requests/minute
   - 99.9% uptime SLA

## Key Takeaways

1. **Start Simple**: Declarative + Local for prototypes
2. **Optimize Gradually**: Move to custom when hitting limits
3. **Distribute Wisely**: Only when scale demands it
4. **Hybrid Approach**: Mix declarative and custom agents
5. **Monitor Transitions**: Track when to evolve architecture