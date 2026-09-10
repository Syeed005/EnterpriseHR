# EnterpriseHR AI Assistant — Local/On-Prem RAG for Enterprise Documents

## Overview

EnterpriseHR AI Assistant is a Retrieval-Augmented Generation (RAG) application designed to answer employee questions using authoritative enterprise documents such as HR policies, guidelines, procedures, and internal reference materials.

The project explores a common enterprise AI problem:

> How can an organization provide conversational access to internal documents while keeping its documents, application data, retrieval infrastructure, and business logic primarily within its own environment?

Instead of uploading an organization's complete document repository to a managed cloud AI/search platform, this architecture keeps document ingestion, metadata, chunking, retrieval, application logic, and operational data local/on-premises where practical.

An external LLM/embedding API can be used selectively for AI capabilities, while the surrounding architecture is designed to minimize unnecessary movement of enterprise data.

The project is intentionally designed as an engineering case study rather than only a chatbot demo. It examines document-processing reliability, retrieval quality, security boundaries, scalability, maintainability, and the trade-offs involved in building a constrained enterprise RAG architecture.

---

## The Business Problem

Organizations often maintain important operational knowledge across documents such as:

- HR policies
- Employee guidelines
- Standard operating procedures
- PDF manuals
- Word documents
- PowerPoint presentations
- Internal reference documents

Employees frequently need answers to questions such as:

- What is the current policy?
- Am I eligible for a particular benefit or service?
- What are the allowed operating hours?
- Which version of a policy is currently effective?
- What does the official document say about a specific situation?

Traditional document search requires users to locate the correct file, identify the relevant page or section, and interpret the information manually.

A general-purpose LLM provides a better conversational experience, but allowing it to answer directly from its pretrained knowledge introduces an important problem:

**the answer may not be grounded in the organization's authoritative documents.**

The goal of this project is therefore to combine the usability of an AI assistant with controlled retrieval from enterprise-owned information.

---

## Key Requirements

The solution was designed around several practical enterprise requirements:

1. Keep source documents and application data local where practical.
2. Support heterogeneous enterprise documents such as PDF, DOCX, and PPTX.
3. Extract meaningful content instead of treating every page as an arbitrary block of text.
4. Preserve document metadata such as title, version, effective date, and issuing organization.
5. Break documents into semantically useful chunks for retrieval.
6. Generate embeddings for semantic similarity search.
7. Retrieve only the most relevant context for a user's question.
8. Generate answers grounded in retrieved enterprise content.
9. Return citations/source information with responses.
10. Maintain an architecture that can later evolve toward larger-scale enterprise infrastructure.

---

## High-Level Architecture

```text
                    Enterprise Documents
                  PDF / DOCX / PPTX / etc.
                            |
                            v
                 +-----------------------+
                 | Document Ingestion    |
                 +-----------------------+
                            |
                            v
                 +-----------------------+
                 | Text/Table Extraction |
                 +-----------------------+
                            |
                            v
                 +-----------------------+
                 | Cleanup & Structure   |
                 | Detection             |
                 +-----------------------+
                            |
                            v
                 +-----------------------+
                 | Metadata Extraction   |
                 | Title / Version /     |
                 | Effective Date / etc. |
                 +-----------------------+
                            |
                            v
                 +-----------------------+
                 | Intelligent Chunking  |
                 +-----------------------+
                            |
                            v
                 +-----------------------+
                 | Embedding Generation  |
                 +-----------------------+
                            |
                            v
                 +-----------------------+
                 | Local Retrieval /     |
                 | Vector Data           |
                 +-----------------------+
                            |
             User Question |
                    |       v
                    |   Semantic Search
                    |       |
                    +-------+
                            |
                            v
                 +-----------------------+
                 | Retrieved Context     |
                 +-----------------------+
                            |
                            v
                 +-----------------------+
                 | LLM / RAG Generation  |
                 +-----------------------+
                            |
                            v
                 Grounded Answer + Citation
