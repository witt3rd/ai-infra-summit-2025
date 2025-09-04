# Microsoft 365 Copilot Agents Overview

Source: https://learn.microsoft.com/en-us/microsoft-365-copilot/extensibility/agents-overview

## Purpose

Microsoft 365 Copilot Agents extend Copilot's capabilities by:
- Creating specialized AI assistants for specific tasks
- Integrating additional knowledge and data sources
- Automating complex workflows
- Addressing specific organizational needs

## Agent Types

### 1. Declarative Agents
- **Infrastructure**: Use Copilot's existing AI infrastructure
- **Development**: Low-code configuration approach
- **Integration**: Native Microsoft 365 app integration
- **Constraints**: Limited to Copilot's orchestrator and models
- **Best For**: Simpler workflows, quick development, Microsoft ecosystem

### 2. Custom Engine Agents
- **Infrastructure**: Fully customizable AI assistants
- **Hosting**: Require additional hosting infrastructure
- **Flexibility**: Support complex workflows and logic
- **Models**: Can use custom models and orchestration
- **Best For**: Complex workflows, custom requirements, proactive interactions

## Core Agent Components

1. **Knowledge Base**: Domain-specific information and context
2. **Custom Actions**: Task execution capabilities
3. **Orchestrator**: Workflow management and coordination
4. **Foundation Models**: Underlying AI models (GPT, custom, etc.)
5. **User Experience Layer**: Interaction interface

## Key Capabilities

- **Knowledge Extension**: Incorporate enterprise-specific knowledge
- **Workflow Automation**: Multi-step process automation
- **Tailored Experiences**: User-specific interactions
- **Secure Integration**: Enterprise system connectivity with security

## Implementation Decision Framework

### Choose Declarative Agents When:
- Simpler workflows are sufficient
- Microsoft 365 app integration is primary
- Quick development is prioritized
- Using Copilot's existing capabilities

### Choose Custom Engine Agents When:
- Complex workflows are required
- Custom model requirements exist
- Proactive interactions are needed
- External app integration is critical
- Full control over orchestration is necessary

## Deployment Options

1. **Organizational Use**: Internal deployment for enterprise workflows
2. **Commercial Marketplace**: Publishing for broader distribution

## Key Considerations

- **Compliance**: Ensure adherence to organizational policies
- **Security**: Implement appropriate access controls and data protection
- **Cost**: Consider infrastructure and operational costs
- **Scalability**: Plan for growth and increased usage
- **Maintenance**: Factor in ongoing support and updates

## Architecture Patterns

### Declarative Agent Architecture
- Leverages Copilot's orchestrator
- Minimal infrastructure requirements
- Configuration-based approach
- Built-in Microsoft 365 integration

### Custom Engine Architecture
- Custom orchestration layer
- Flexible model selection
- External service integration
- Independent scaling capabilities

## Integration Points

- Microsoft Graph API
- SharePoint data sources
- Teams collaboration
- Power Platform connectors
- External APIs and services

## Development Lifecycle

1. **Planning**: Identify use cases and requirements
2. **Design**: Choose agent type and architecture
3. **Development**: Build using appropriate tools
4. **Testing**: Validate functionality and security
5. **Deployment**: Roll out to target users
6. **Monitoring**: Track usage and performance
7. **Optimization**: Refine based on feedback