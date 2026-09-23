# EnterpriseHR AI Assistant

EnterpriseHR AI Assistant is a production-style portfolio project
demonstrating secure Retrieval-Augmented Generation (RAG), structured
enterprise data access, controlled LLM tool calling, authorization-aware
retrieval, evaluation, observability, and conversational HR assistance.

The application lets an authenticated employee ask natural-language
questions about HR policies and receive grounded answers with document
citations. The assistant can also combine authorized HR policy content
with structured employee data, such as an employee's assigned office
schedule.

## Project Goals

This project was designed to explore the engineering patterns required
to move from a simple "chat with PDF" prototype toward an
enterprise-ready AI assistant:

-   Ground answers in authoritative internal documents.
-   Combine unstructured policy documents with structured SQL data.
-   Enforce authorization before information reaches the LLM.
-   Allow an LLM to use only explicitly approved application tools.
-   Preserve conversation context across follow-up questions.
-   Return citations for policy-derived answers.
-   Evaluate retrieval, RAG, conversation, agent routing, and security
    behavior.
-   Track token usage and estimated model cost.
-   Keep the architecture provider-aware but avoid unnecessary cloud
    dependencies.

## Architecture

``` text
EnterpriseHR.Console
        |
        | HTTP / JSON
        v
EnterpriseHR.Api
        |
        +-- Authentication / RBAC
        +-- Request Validation
        +-- Exception Handling
        |
        v
HrAssistantService
Controlled Agent Workflow
        |
        +-----------------------------+
        |                             |
        v                             v
EmployeeProfileTool             HrPolicySearchTool
        |                             |
        v                             v
SQL Server                     Hybrid Retrieval
Structured Employee Data       Semantic + FTS + RRF
                                      |
                                      v
                              Authorization Filtering
                                      |
                                      v
                               Context Expansion
                                      |
                                      v
                                  GPT-5-mini
                                      |
                                      v
                         Grounded Answer + Citations
                                      |
                        +-------------+-------------+
                        |                           |
                        v                           v
               Conversation Persistence      Token / Cost Usage
```

## Core Technology

-   C# / .NET 10
-   ASP.NET Core Minimal API
-   SQL Server 2022 Developer Edition
-   Entity Framework Core
-   OpenAI .NET SDK
-   `gpt-5-mini` for chat/tool orchestration
-   `text-embedding-3-small` for embeddings
-   PdfPig for PDF text extraction
-   SQL Server Full-Text Search
-   OpenTelemetry
-   Scalar API documentation
-   Console client for interactive chat

## Solution Structure

``` text
EnterpriseHR
├── EnterpriseHR.Api
├── EnterpriseHR.Console
├── EnterpriseHR.Core
├── EnterpriseHR.Infrastructure
└── EnterpriseHR.Tests
```

### EnterpriseHR.Api

Hosts HTTP endpoints, development authentication, authorization
policies, request validation, ProblemDetails exception handling, and
application configuration.

### EnterpriseHR.Core

Contains domain entities, contracts, models, evaluation models,
exceptions, feedback abstractions, audit abstractions, and AI/service
interfaces.

### EnterpriseHR.Infrastructure

Contains SQL/EF Core implementations, document ingestion, embeddings,
hybrid retrieval, RAG, conversation persistence, employee profile
access, AI tools, controlled agent orchestration, audit, telemetry, and
evaluation services.

### EnterpriseHR.Console

A deliberately simple interactive client that creates a conversation
session, sends questions to the API, displays grounded answers and PDF
sources, and shows token usage and estimated cost.

## Document Processing Pipeline

``` text
PDF
 |
 v
Text Extraction
 |
 v
Normalization
 |
 v
Chunking
 |
 +--> Metadata
 |
 v
Embeddings
 |
 v
SQL Persistence
```

Document metadata includes fields such as title, version, effective
date, issuing department, lifecycle status, policy key, and access
level.

The employee retrieval path considers only eligible active documents and
applies access filtering before content can be supplied to the model.

## Hybrid Retrieval

The project combines:

1.  Semantic vector similarity using document embeddings.
2.  SQL Server Full-Text Search.
3.  Reciprocal Rank Fusion (RRF) to combine rankings.
4.  Context expansion to provide surrounding policy context where
    appropriate.

This approach was selected to support both semantic questions and exact
policy terminology.

## RAG Pipeline

A traditional `RagService` remains in the solution for deterministic RAG
evaluation and comparison.

``` text
Question
  |
  v
Hybrid Search
  |
  v
Authorization-Filtered Results
  |
  v
Context Expansion
  |
  v
Grounded Prompt
  |
  v
LLM
  |
  v
Answer + Citations
```

The traditional RAG pipeline is intentionally separate from the
controlled agent workflow.

## Controlled Agent Workflow

The main `/chat/ask` experience uses `HrAssistantService`.

The model can request only approved application capabilities:

### `get_my_employee_profile`

Returns a deliberately narrow view of the currently authenticated
employee's structured profile, including employee number, department,
office schedule, location, and employment status.

The model cannot provide an arbitrary employee ID and does not receive
SQL credentials or arbitrary SQL access.

### `search_hr_policy`

Searches authorized HR policy content through the application's secured
hybrid retrieval pipeline.

The model controls the search intent, but it does not control document
IDs, access levels, SQL, or file paths.

The application also limits policy-search repetition and the total
number of tool-calling rounds.

## Security Model

Security is enforced by application code rather than by relying on
prompt instructions.

### Endpoint Authorization

Role-based policies control whether a user may call an operation.

### Document Authorization

Document access levels determine which documents may participate in
retrieval.

### Object-Level Access

Conversation sessions, messages, feedback, and employee profile access
are resolved against the authenticated application user rather than
trusting caller-provided user identity.

### Authorization Before LLM

Restricted content is filtered before context is supplied to the model.

A synthetic restricted HR policy containing the test code
`HR-LEAVE-7421` was used to verify that an Employee user could not
retrieve or expose the restricted value.

### Development Authentication

The current local-development environment uses a development
authentication handler and `X-Dev-User` for repeatable testing. It is
intentionally isolated from production architecture. A production
deployment would replace this with an approved identity provider such as
Microsoft Entra ID.

## Conversation and Feedback

The application persists chat sessions and messages so follow-up
questions can use prior conversation context.

Feedback is associated with an assistant message and the authenticated
user. Cross-user feedback attempts are rejected.

## Reliability and Hardening

Implemented hardening includes:

-   Request validation before AI execution.
-   Centralized exception handling with ProblemDetails.
-   Safe handling of 400, 403, 404, 503, 504, and unexpected 500
    conditions.
-   Client cancellation propagation.
-   Bounded overall AI request duration.
-   Provider-neutral AI service failure handling.
-   Development/production configuration separation.
-   Secret handling outside committed configuration.
-   Controlled agent round and policy-search limits.

## Observability and Cost

The project includes OpenTelemetry instrumentation and model usage
tracking.

Each completed agent request can report:

-   Input tokens
-   Output tokens
-   Total tokens
-   Estimated API cost

The console client displays these values directly after each answer.

## Evaluation

The project contains separate evaluation flows for retrieval, RAG,
conversation behavior, and the controlled agent.

### Retrieval Evaluation

A five-question retrieval evaluation achieved:

-   HitRate@3: 1.0
-   Top-1 success: 1.0
-   MRR: 1.0

These results describe the defined evaluation set and should not be
interpreted as universal model accuracy.

### Conversation Evaluation

A three-turn conversation evaluation passed all three defined follow-up
scenarios with citations.

### Agent Evaluation

The final controlled-agent evaluation passed 4/4 defined scenarios:

  -----------------------------------------------------------------------
  Scenario                Expected behavior       Result
  ----------------------- ----------------------- -----------------------
  Employee profile        Use structured employee Passed
  routing                 profile tool            

  Policy routing          Use secured HR policy   Passed
                          search with citation    

  Combined question       Use employee profile +  Passed
                          HR policy search        

  Restricted policy       Do not expose           Passed
  protection              restricted HR code      
  -----------------------------------------------------------------------

## Example

``` text
==================================================
        EnterpriseHR AI Assistant
==================================================

You: What is the maximum Fitness Centre usage?

Assistant:
Employees may use the Fitness Centre for a maximum
of 45 minutes per day.

Sources:
[1] BJIT Fitness Centre Guidelines, Page 2

--------------------------------------------------
Token Usage
Input Tokens:   3,409
Output Tokens:    550
Total Tokens:   3,959
Estimated Cost: $0.001952
--------------------------------------------------
```
<img width="2326" height="1157" alt="image" src="https://github.com/user-attachments/assets/a42e3d92-9d0e-45fa-b825-b0ba8f93c371" />

<img width="1386" height="652" alt="image" src="https://github.com/user-attachments/assets/b5f71d28-961a-4f98-a8cb-de23b3769d28" />




## Selected API Capabilities

Employee-facing capabilities include conversation sessions, `/chat/ask`,
message history, and feedback.

The solution also retains development/engineering endpoints for document
ingestion, chunking, embedding generation, document activation,
retrieval inspection, evaluation, usage reporting, authentication
testing, and database testing. These endpoints are useful during
engineering and evaluation but are not intended to represent the final
public production API surface.

## Local Development Notes

The current portfolio build uses SQL Server 2022 Developer Edition and
local application services. The model is accessed through the OpenAI
API.

Do not commit API keys, database credentials, or other secrets to source
control.

A production deployment would require production identity, managed
secret storage, deployment-specific database/search infrastructure,
network/security controls, and operational monitoring appropriate to the
target organization.

## Engineering Takeaways

This project demonstrates several patterns relevant to Forward Deployed
and enterprise AI engineering:

-   RAG quality depends on ingestion, retrieval, metadata, and
    evaluation---not only the model.
-   Structured enterprise data and document knowledge often need to be
    combined.
-   LLM tools should expose narrow business capabilities rather than raw
    databases.
-   Authorization must be enforced before retrieved content reaches the
    model.
-   Agent autonomy should be bounded by application-controlled tools and
    execution limits.
-   Evaluation should separately measure retrieval, answer quality,
    routing, citations, and security invariants.
-   Token and cost visibility helps make AI behavior operationally
    measurable.

## Status

**EnterpriseHR AI Assistant V1 --- Development Complete**

The current version is a portfolio implementation intended to
demonstrate enterprise AI architecture and engineering decisions.
Production deployment concerns such as enterprise identity integration
and infrastructure-specific operational controls are intentionally
documented rather than fully deployed.
