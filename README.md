# TechnologAI

TechnologAI was the last major iteration before what eventually became Agience.

This project was my attempt to build a modular agent architecture around three ideas that mattered to me: identity, tool use, and agent-to-agent communication. Long before standards like MCP became common, I was already experimenting with structured tool interfaces that agents could discover and invoke. In the same way, I was exploring A2A-style messaging, where agents could communicate through typed messages, maintain context, and coordinate behavior across roles and environments.

The codebase breaks the system into components for cognition, interaction, observation, authority, and execution. It includes experiments with reusable templates, embeddings and prompt workflows, structured messaging over MQTT, tool-like function interfaces for web and file operations, and a broader authority layer around identity and trust. There are also integrations touching services such as OpenAI, Jira, Reddit, Twitter, speech, and image generation.

More than anything, this repository captures my transition from isolated AI experiments into a more complete agent platform. A lot of the architectural ideas that later matured into Agience were already present here in rough form: persistent identity, portable capabilities, structured inter-agent communication, and a system design that treated tools as first-class interfaces.

This repository is incomplete and should be viewed as an archival prototype. It reflects an active architectural exploration rather than a finished platform, but it marks an important step in the lineage that led to Agience.

## Concepts explored

- persistent identity and authority models
- pre-MCP tool interfaces and capability invocation
- agent-to-agent communication patterns
- structured messaging and coordination over MQTT
- modular cognition, interaction, and execution layers
- prompt, embedding, and function-oriented workflows
- early foundations of the architecture that became Agience

## Status

Archival prototype preserved as the final major iteration before Agience.
