# Agentic LLM Working Principles

Agents combine two complementary concepts. The project guidance in [Copilot instructions](../.github/copilot-instructions.md) includes the Caveman and Ponytail prompts; this document explains their intent and adds a decision rule and response format.

## Caveman: communication efficiency
Source concept: https://github.com/JuliusBrussee/caveman

Apply to agent messages, plans, progress reports, reviews and repository guidance:
- Preserve full technical accuracy while removing filler, pleasantries, repetition and tool-call narration.
- Prefer short, precise statements and established technical terms; do not invent unclear abbreviations.
- Keep commands, identifiers, API names, errors, paths, protocol names and code exact.
- Quote only decisive log/error lines unless the full output is required.
- Preserve the user's language.
- Compression must never remove safety constraints, assumptions, acceptance criteria, failure reasons or test evidence.
- Code readability is not sacrificed merely to minimize tokens.

## Ponytail: implementation minimalism
Source concept: https://github.com/DietrichGebert/ponytail

Apply to architecture and code:
- The best code is code not needed. Solve the stated requirement with the smallest complete change.
- Prefer platform/framework capabilities and existing project abstractions over new dependencies or custom frameworks.
- Avoid speculative extensibility, premature generalization, duplicate models, wrappers without policy value and configuration without a real use case.
- Do not add files, layers, services, interfaces, tests or documentation that do not improve correctness, safety, maintainability or a stated acceptance criterion.
- Reuse before adding; delete obsolete code after replacement.
- Keep functions and modules locally understandable. Optimize for low cognitive load, not cleverness or minimum line count.
- Minimalism never overrides user library safety, security, database compatibility, metadata correctness or required tests.

## Combined decision rule
1. Understand the exact requirement and inspect existing code.
2. Identify the smallest safe end-to-end change that satisfies acceptance criteria.
3. Reuse existing capabilities; add abstractions only when concrete uses or a clear safety boundary justify them.
4. Implement and test the behavior.
5. Remove unnecessary code and dependencies.
6. Report outcome tersely: changed, tested, limitations, next blocker.

## Agent response format
For implementation updates, report what applies:
- Result
- Files changed
- Tests run/result
- Remaining limitations or blockers, if any

Do not emit long preambles, repeated plans, decorative prose or multiple alternative implementations unless a decision is genuinely blocked.
