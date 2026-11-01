#!/usr/bin/env node
// Automatic AI-interaction logger for Claude Code hooks.
//
// One interaction = one user prompt up to the agent finishing (Stop). Hook events in between
// (tool calls, skills, sub-agents, MCP tools) are collected in a per-session state file and
// written as ONE structured JSON line to docs/ai-log/ai-interactions.jsonl when the turn stops.
// docs/ai-log/ai-interactions-summary.md is regenerated from that file after every write.
//
// Fields a tool cannot know (human_decision, limitation, suggestion) stay null until the author
// fills them with: node .claude/hooks/annotate.js last --decision "..." --limitation "..."
const fs = require('fs');
const path = require('path');
const { renderSummary } = require('./render-summary');

const root = process.env.CLAUDE_PROJECT_DIR || process.cwd();
const logDir = path.join(root, 'docs', 'ai-log');
const logFile = path.join(logDir, 'ai-interactions.jsonl');

const clip = (value, max = 800) => {
  const text = typeof value === 'string' ? value : JSON.stringify(value ?? '');
  return text.length > max ? `${text.slice(0, max)}...[truncated]` : text;
};
const rel = (file) => (file ? path.relative(root, file).replace(/\\/g, '/') : file);
const slug = (text) => text.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-|-$/g, '').slice(0, 40);
const unique = (list) => [...new Set(list)];

function exists(...parts) {
  try { return fs.existsSync(path.join(root, ...parts)); } catch { return false; }
}

function modelFromTranscript(transcriptPath) {
  try {
    const size = fs.statSync(transcriptPath).size;
    const fd = fs.openSync(transcriptPath, 'r');
    const length = Math.min(size, 300000);
    const buffer = Buffer.alloc(length);
    fs.readSync(fd, buffer, 0, length, size - length);
    fs.closeSync(fd);
    const matches = [...buffer.toString('utf8').matchAll(/"model":"([^"]+)"/g)];
    return matches.length ? matches[matches.length - 1][1] : null;
  } catch { return null; }
}

function loadState(file) {
  try { return JSON.parse(fs.readFileSync(file, 'utf8')); } catch { return null; }
}

function freshState(input) {
  return {
    request: input.prompt ? clip(input.prompt, 1500) : null,
    started: new Date().toISOString(),
    mode: input.permission_mode === 'plan' ? 'plan' : 'agent',
    tools: [], skills: [], subagents: [], mcp: [], artifacts: [], commits: [], verification: [],
    explicitSkill: input.prompt && input.prompt.startsWith('/') ? input.prompt.slice(1).split(/\s/)[0] : null,
  };
}

function resultText(response) {
  if (typeof response === 'string') return response;
  if (response && typeof response === 'object') return [response.stdout, response.output, response.content].filter(Boolean).map(String).join('\n');
  return '';
}

function recordTool(state, input) {
  const name = input.tool_name || '';
  const args = input.tool_input || {};
  state.tools.push(name);

  if (name === 'Skill' && args.skill) {
    state.skills.push({
      name: args.skill,
      type: exists('.claude', 'skills', args.skill) ? 'custom' : 'built-in',
      purpose: clip(args.args || 'invoked by the agent', 160),
      invocation: state.explicitSkill === args.skill ? 'explicit' : 'automatic',
    });
  } else if (name === 'Agent') {
    const agentName = args.subagent_type || 'general-purpose';
    state.subagents.push({
      name: agentName,
      type: exists('.claude', 'agents', `${agentName}.md`) ? 'custom' : 'built-in',
      task: clip(args.description || args.prompt || '', 200),
      status: 'launched',
    });
  } else if (name.startsWith('mcp__')) {
    const [, server, ...tool] = name.split('__');
    state.mcp.push({ server, tool: tool.join('__'), purpose: clip(JSON.stringify(args), 160) });
  } else if (['Edit', 'Write', 'NotebookEdit'].includes(name) && args.file_path) {
    state.artifacts.push(rel(args.file_path));
  } else if (name === 'Bash' || name === 'PowerShell') {
    const command = String(args.command || '');
    const text = resultText(input.tool_response);
    const commit = text.match(/\[[^\]\s]+ ([0-9a-f]{7,})\] (.+)/);
    if (/git commit/.test(command) && commit) state.commits.push(`${commit[1]} ${commit[2]}`);
    if (/dotnet test|playwright test|npm test|stryker/.test(command)) {
      const line = text.match(/(Passed!|Failed!)[^\n]*|\d+ (passed|failed)[^\n]*|final mutation score is [\d.]+ ?%/);
      if (line) state.verification.push(`${clip(command, 60)} -> ${line[0].trim()}`);
    }
  }
}

function stageOf(state) {
  const files = state.artifacts.join(' ');
  if (/openspec/.test(files)) return 'spec';
  if (/e2e\//.test(files)) return 'e2e-test';
  if (/tests?\//i.test(files) && !/src\//.test(files)) return 'test';
  if (/src\//.test(files)) return 'implementation';
  if (/ASSIGNMENT|pr-description|ai-log/.test(files)) return 'documentation';
  if (/AGENTS|\.claude/.test(files)) return 'setup';
  if (/review/i.test(state.request || '')) return 'review';
  return 'general';
}

function flush(input, state) {
  if (!state || !state.request) return;
  const ts = new Date().toISOString();
  const failed = state.verification.some((line) => /Failed!|\d+ failed/.test(line));
  const entry = {
    id: `${ts.replace(/[-:.]/g, '').slice(0, 15)}Z-agent-${slug(state.request) || 'interaction'}`,
    timestamp: state.started,
    completed_at: ts,
    stage: stageOf(state),
    agent: 'agent',
    agent_name: 'claude-code-main',
    agent_type: 'built-in',
    ai_tool: 'Claude Code',
    model: modelFromTranscript(input.transcript_path) || process.env.AI_LOG_MODEL || 'unknown',
    mode: state.mode,
    source: 'hook',
    session: input.session_id || null,
    related_ids: state.commits,
    parent_task: null,
    context: 'Assignment for AI in SDLC (approval limits by role)',
    request: state.request,
    skills_used: state.skills,
    subagents_used: state.subagents,
    mcp_tools_used: state.mcp,
    tools_used: unique(state.tools),
    suggestion: clip(input.last_assistant_message || '', 600) || null,
    human_decision: null,
    action_verification: state.verification.length ? state.verification : null,
    limitation: null,
    status: failed ? 'partial' : 'applied',
    outcome: failed ? 'partially completed' : 'successful',
    artifacts: unique(state.artifacts),
  };
  fs.mkdirSync(logDir, { recursive: true });
  fs.appendFileSync(logFile, `${JSON.stringify(entry)}\n`);
  renderSummary(root);
}

let raw = '';
process.stdin.on('data', (chunk) => (raw += chunk));
process.stdin.on('end', () => {
  let input = {};
  try { input = JSON.parse(raw || '{}'); } catch { return; }

  const event = input.hook_event_name;
  const stateFile = path.join(logDir, `.state-${input.session_id || 'unknown'}.json`);
  fs.mkdirSync(logDir, { recursive: true });
  let state = loadState(stateFile);

  if (event === 'UserPromptSubmit') {
    flush(input, state); // an earlier prompt that never reached Stop
    state = freshState(input);
  } else if (!state) {
    state = freshState({});
  }

  if (event === 'PostToolUse') recordTool(state, input);
  if (event === 'SubagentStop') {
    const pending = state.subagents.find((agent) => agent.status === 'launched');
    if (pending) {
      pending.status = 'completed';
      pending.result = clip(input.last_assistant_message || '', 300);
    }
  }

  if (event === 'Stop') {
    flush(input, state);
    try { fs.unlinkSync(stateFile); } catch { /* already gone */ }
    return;
  }
  fs.writeFileSync(stateFile, JSON.stringify(state));
});
