#!/usr/bin/env node
// Appends one JSON line per AI interaction event to docs/ai-log/ai-interactions.jsonl.
// Wired to UserPromptSubmit, PostToolUse, SubagentStop and Stop in .claude/settings.json,
// so every entry has the same shape: { ts, event, session, agent, detail }.
const fs = require('fs');
const path = require('path');

const MAX = 600;
const clip = (value) => {
  const text = typeof value === 'string' ? value : JSON.stringify(value ?? '');
  return text.length > MAX ? `${text.slice(0, MAX)}...[truncated]` : text;
};

let raw = '';
process.stdin.on('data', (chunk) => (raw += chunk));
process.stdin.on('end', () => {
  let input = {};
  try { input = JSON.parse(raw || '{}'); } catch { /* log what we can */ }

  const event = input.hook_event_name || 'Unknown';
  const detail = {};
  if (event === 'UserPromptSubmit') detail.prompt = clip(input.prompt);
  if (event === 'PostToolUse') {
    detail.tool = input.tool_name;
    detail.input = clip(input.tool_input);
    detail.result = clip(input.tool_response);
  }
  if (event === 'SubagentStop' || event === 'Stop') detail.lastMessage = clip(input.last_assistant_message);

  const entry = {
    ts: new Date().toISOString(),
    event,
    session: input.session_id || null,
    agent: input.agent_type || 'main',
    detail,
  };

  const root = process.env.CLAUDE_PROJECT_DIR || process.cwd();
  const file = path.join(root, 'docs', 'ai-log', 'ai-interactions.jsonl');
  fs.mkdirSync(path.dirname(file), { recursive: true });
  fs.appendFileSync(file, `${JSON.stringify(entry)}\n`);
});
