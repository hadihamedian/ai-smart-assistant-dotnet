# AI Smart Assistant 🤖

[![CI Pipeline](https://github.com/hadihamedian/ai-smart-assistant-dotnet/actions/workflows/ci.yml/badge.svg)](https://github.com/hadihamedian/ai-smart-assistant-dotnet/actions)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-18-61DAFB?style=flat-square&logo=react)](https://reactjs.org/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](https://opensource.org/licenses/MIT)

A production-ready AI-powered Document Q&A application built on the **Retrieval-Augmented Generation (RAG)** pattern. Features a clean React frontend, an ASP.NET Core 10 API following **Clean Architecture** and **DDD**, and full Docker/CI support.

---

## 🎯 Project Overview

Upload any PDF document, ask questions about it in natural language, and get accurate, context-aware answers powered by OpenAI or a fully local Ollama model — all within a single Dockerized stack.

This project demonstrates enterprise-grade integration of LLMs into a .NET ecosystem: clean separation of concerns, testable application logic, and a pluggable AI provider model.

---

## 🏗️ Architecture

The backend is organized into four layers following **Clean Architecture**:

```
src/
├── SmartAssistant.Domain/          # Entities, Interfaces, Domain logic (no dependencies)
├── SmartAssistant.Application/     # Use cases, CQRS Commands/Queries via MediatR
├── SmartAssistant.Infrastructure/  # Qdrant, PdfPig, Semantic Kernel, EF Core
└── SmartAssistant.API/             # ASP.NET Core controllers, DI composition root
    └── SmartAssistant.UI/          # React 18 + TypeScript + Tailwind (Vite)
```

### System Diagram

```
+-----------------------+       HTTP/REST        +-----------------------+
|    React UI (Vite)    |  <==================>  |   ASP.NET Core API    |
+-----------------------+                        +----------+------------+
                                                            |
                              +-----------------------------+-----------------------------+
                              |                             |                             |
                              v                             v                             v
               +--------------+------+      +--------------+------+      +---------------+-----+
               |  IDocumentParser    |      |  MediatR Handlers   |      |  IChatSession       |
               |  (PdfPig impl)      |      |  Commands/Queries   |      |  Repository         |
               +---------------------+      +--------------+------+      +---------------------+
                                                           |
                              +----------------------------+----------------------------+
                              |                                                         |
                              v                                                         v
               +--------------+------+                              +-------------------+---+
               |  Qdrant Vector DB   |  <-- embeddings/search -->   |  Semantic Kernel      |
               |  (Dockerized)       |                              |  OpenAI / Ollama      |
               +---------------------+                              +-----------------------+
```

---

## 🧠 The RAG Pipeline

1. **Ingestion** — Uploaded PDFs are parsed via `IDocumentParser` (backed by PdfPig).
2. **Chunking** — Text is split into overlapping segments (2000 chars, 200 overlap).
3. **Embedding** — Chunks are vectorized using `text-embedding-ada-002` (OpenAI) or `nomic-embed-text` (Ollama) via Semantic Kernel.
4. **Storage** — Vectors and metadata are persisted in Qdrant.
5. **Retrieval** — User queries are embedded; top 3 chunks retrieved via Cosine Similarity.
6. **Generation** — Retrieved chunks are injected into a tailored LLM prompt to produce a grounded answer.

---

## 🛠️ Tech Stack

| Category | Technology |
|---|---|
| Backend Framework | ASP.NET Core 10.0, C# 12 |
| Frontend | React 18, TypeScript, Tailwind CSS, Vite |
| AI Orchestration | Microsoft Semantic Kernel |
| AI Providers | OpenAI API (cloud) · Ollama (local fallback) |
| Vector Database | Qdrant (Dockerized) |
| CQRS / Mediator | MediatR |
| Document Parsing | PdfPig (via `IDocumentParser`) |
| Testing | xUnit, Moq |
| Infrastructure | Docker, Docker Compose, GitHub Actions CI |

---

## 🚀 Getting Started

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) (for the React frontend)

---

### 1. Configuration & API Keys

API keys are **not** stored in source. Set your OpenAI key via .NET User Secrets:

```bash
dotnet user-secrets set "AI:OpenAI:ApiKey" "your-key-here" --project src/SmartAssistant.API
```

> **Prefer fully local?** Switch the provider from `OpenAI` to `Ollama` in `appsettings.json` — no API key needed, runs 100% offline.

---

### 2. Start Infrastructure (Docker)

Starts Qdrant and the .NET API in containers:

```bash
docker compose up --build -d
```

| Service | URL |
|---|---|
| API | `http://localhost:8080` |
| API (local run) | `http://localhost:5069` |
| Qdrant Dashboard | `http://localhost:6333/dashboard` |

---

### 3. Run the Frontend

```bash
cd src/SmartAssistant.UI/react-ts
npm install
npm run dev
```

Frontend available at: `http://localhost:5173`

---

## 📡 API Reference

### Upload a Document

Parses the PDF, generates embeddings, and stores vectors in Qdrant.

```bash
curl -X POST "http://localhost:8080/api/documents/upload" \
  -H "Content-Type: multipart/form-data" \
  -F "file=@/path/to/your/document.pdf"
```

**Response:**
```json
{
  "documentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "fileName": "document.pdf",
  "chunkCount": 12
}
```

---

### Ask a Question

Performs semantic retrieval and returns an LLM-generated answer grounded in document content.

```bash
curl -X POST "http://localhost:8080/api/chat/ask" \
  -H "Content-Type: application/json" \
  -d '{
    "documentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "question": "What is the main topic of this document?"
  }'
```

**Response:**
```json
{
  "answer": "The document covers ...",
  "sessionId": "a1b2c3d4-...",
  "sources": ["chunk_3", "chunk_7"]
}
```

---

## 🧪 Running Tests

Unit tests cover Domain logic and Application MediatR handlers with fully mocked external dependencies (Qdrant, Semantic Kernel, document parser).

```bash
dotnet test
```

Test projects follow the same Clean Architecture layer separation as the main source.

---

## 🗂️ Key Design Decisions

| Decision | Rationale |
|---|---|
| `IDocumentParser` interface | Decouples PdfPig from application logic; swap parsers without touching use cases |
| `IChatSessionRepository` separate from `IDocumentRepository` | Single Responsibility — session state and document storage evolve independently |
| MediatR CQRS | Thin controllers; all business logic lives in testable handler classes |
| Ollama fallback | Enables full local development and demos without API costs |
| User Secrets / env vars | No credentials in source; safe for open-source publishing |

---

## 📜 License

This project is licensed under the [MIT License](https://opensource.org/licenses/MIT).
No newline at end of file
